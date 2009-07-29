using System;
using System.IO;
using System.Reflection;
using System.Xml;

using Glue.Data;
using Glue.Lib;
using Ionic.Zip;

namespace Glue.Web
{
    public class Admin
    {
        // Configuration values set in constructor:
        protected string RootDirectory;
        protected string AppAssemblyFile;
        protected string AppType;
        protected string WebConfigFile;
        protected string UpdateUrl;
        protected string OfflineHandler;

        protected string DataproviderElementName = "dataprovider"; // default value, can be overridden

        XmlDocument _webconfigXml;
        protected XmlDocument WebConfigXml
        {
            get
            {
                if (_webconfigXml == null)
                {
                    _webconfigXml = new XmlDocument();
                    _webconfigXml.Load(WebConfigFile);
                }
                return _webconfigXml;
            }
        }

        IDataProvider _dataprovider;
        protected IDataProvider DataProvider
        {
            get
            {
                if (_dataprovider == null) _dataprovider = (IDataProvider)Configuration.Get(DataproviderElementName);
                return _dataprovider;
            }
        }

        Assembly _appAssembly;
        protected Assembly AppAssembly
        {
            get
            {
                if (_appAssembly == null) _appAssembly = Assembly.LoadFile(AppAssemblyFile);
                return _appAssembly;
            }
        }

        protected Admin(string rootDirectory, string webConfigFile, string appAssemblyFile, string appType, string updateUrl, string offlineHandler)
        {
            RootDirectory = rootDirectory;
            WebConfigFile = webConfigFile;
            AppAssemblyFile = appAssemblyFile;
            AppType = appType;
            UpdateUrl = updateUrl;
            OfflineHandler = offlineHandler;

            if (WebConfigFile != null) Configuration.Register(WebConfigFile, false); // load web.config into Glue configuration
        }

        /// <summary>
        /// Read database schema version from database. this is now prpovider dependent... override?
        /// </summary>
        protected int CurrentSchemaVersion 
        { 
            get { return DataProvider.ExecuteScalarInt32("SELECT public.version()"); }
        }

        protected int CurrentConfigVersion
        {
            get 
            {
                XmlNode node = WebConfigXml.SelectSingleNode("/configuration/appSettings/add[@key='configversion']");
                if (node != null) return int.Parse(node.Attributes["value"].Value);
                return 0;
            }
        }

        protected bool IsOnline
        {
            get { return (null == WebConfigXml.SelectSingleNode("/configuration/system.web/httpHandlers/add[contains(@type,'"+OfflineHandler+"')]")); }
        }

        protected int AppVersion
        {
            get { return (int)AppAssembly.GetType(AppType).GetField("AppVersion").GetValue(null); }
        }

        protected int AppSchemaVersion
        {
            get { return (int)AppAssembly.GetType(AppType).GetField("AppSchemaVersion").GetValue(null); }
        }

        protected int AppConfigVersion
        {
            get { return (int)AppAssembly.GetType(AppType).GetField("AppConfigVersion").GetValue(null); }
        }

        protected void Status()
        {
            Console.WriteLine("Dll version: {0}", AppVersion);
            Console.WriteLine("Config version: {0}. Expected: {1}", CurrentConfigVersion, AppConfigVersion);
            Console.WriteLine("Schema version: {0}. Expected: {1}", CurrentSchemaVersion, AppSchemaVersion);
            Console.WriteLine("Status: {0}", IsOnline? "Online" : "Offline");
        }

         ///<summary>
         ///Take application offline by editing Web.config
         ///</summary>
        protected void Offline()
        {
            if (!IsOnline)
            {
                Console.WriteLine("Application is already offline.");
                return;
            }

            // Add any missing parent elements

            XmlNode r = WebConfigXml.DocumentElement;
            if (r.SelectSingleNode("system.web") == null) r.AppendChild(WebConfigXml.CreateElement("system.web"));
            if (r.SelectSingleNode("system.web/httpHandlers") == null)
                r.SelectSingleNode("system.web").AppendChild(WebConfigXml.CreateElement("httpHandlers"));
            //if (document.Root.Element("system.web") == null)
            //    document.Root.Add(new XElement("system.web"));
            //if (document.Root.Element("system.web").Element("httpHandlers") == null)
            //    document.Root.Element("system.web").Add(new XElement("httpHandlers"));
            XmlNode httpHandlers = r.SelectSingleNode("system.web/httpHandlers");
            XmlElement clear = WebConfigXml.CreateElement("clear");
                httpHandlers.AppendChild(clear);
            XmlElement add = WebConfigXml.CreateElement("add");
                XmlAttribute verb = WebConfigXml.CreateAttribute("verb");
                    verb.Value = "*";
                    add.Attributes.Append(verb);
                XmlAttribute path = WebConfigXml.CreateAttribute("path");
                    path.Value = "*";
                    add.Attributes.Append(path);
                XmlAttribute type = WebConfigXml.CreateAttribute("type");
                    type.Value = OfflineHandler;
                    add.Attributes.Append(type);
                httpHandlers.AppendChild(add);
            XmlComment comment = WebConfigXml.CreateComment("Taken offline by manager.exe");
                httpHandlers.AppendChild(comment);

            //document.Root.Element("system.web").Element("httpHandlers").Add(
            //    new XElement("clear"),
            //    new XElement("add", new XAttribute("verb", "*"), new XAttribute("path", "*"), new XAttribute("type", "demo01.OfflineHandler")),
            //    new XComment("Taken offline by manager.exe")
            //);
            
            WebConfigXml.Save(WebConfigFile);
            Console.WriteLine("Application is now offline.");
        }

        /// <summary>
        /// Put application online.
        /// </summary>
        protected void Online() 
        {
            //XDocument document = XDocument.Load(web_config);
            if (IsOnline)
            {
                Console.WriteLine("Application is already online.");
                return;
            }

            WebConfigXml.SelectSingleNode("//httpHandlers").RemoveChild(WebConfigXml.SelectSingleNode("//httpHandlers/comment()[last()]"));
            WebConfigXml.SelectSingleNode("//httpHandlers").RemoveChild(WebConfigXml.SelectSingleNode("//httpHandlers/add[last()]"));
            WebConfigXml.SelectSingleNode("//httpHandlers").RemoveChild(WebConfigXml.SelectSingleNode("//httpHandlers/clear[last()]"));
            //document.Root.Element("system.web").Element("httpHandlers").LastNode.Remove(); // comment
            //document.Root.Element("system.web").Element("httpHandlers").LastNode.Remove(); // add offlineHandler 
            //document.Root.Element("system.web").Element("httpHandlers").LastNode.Remove(); // clear

            WebConfigXml.Save(WebConfigFile);
            Console.WriteLine("Application is now online.");
        }

        protected void ConfigUpdate()
        {
            //int appConfigVersion = AppConfigVersion;
            //int configVersion = CurrentConfigVersion;

            if (CurrentConfigVersion > AppConfigVersion)
            {
                Console.WriteLine("Config version is larger than expected. Please use 'update' to update the application.");
                return;
            }
            else if (CurrentConfigVersion == AppConfigVersion)
            {
                Console.WriteLine("Config is up to date.");
                return;
            }

            string fname = Path.GetFullPath(
                Path.Combine(RootDirectory, String.Format("scripts/configupdate_{0}_{1}.py", CurrentConfigVersion, CurrentConfigVersion + 1)));
            if (!File.Exists(fname))
            {
                Console.WriteLine("File does not exist: " + fname);
            }
            else
            {
                Console.WriteLine("Please execute the script '{0}' \nto update this installation.", fname);
                //ExecPython(fname, rootdir);
            }
        }

        protected void Update()
        {
            string updatedir = Path.GetFullPath(Path.Combine(RootDirectory, "updates"));
            string fname = Path.Combine(updatedir, String.Format("{0}-{1}.zip", AppType, AppVersion + 1));

            Console.WriteLine(fname);
            if (File.Exists(fname))
            {
                Console.WriteLine("Extracting files in " + fname);
                ZipFile z = new ZipFile(fname);
                //z.ExtractAll(rootdir, true);
                foreach (ZipEntry ze in z.Entries)
                {
                    Console.WriteLine(ze.FileName);
                    try
                    {
                        //ze.Extract(RootDirectory, true);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        string fullpath = Path.GetFullPath(Path.Combine(RootDirectory, ze.FileName));
                        Console.WriteLine("No authorization to overwrite {0}, moving to {1}", fullpath, fullpath + ".bak");
                        MoveForce(fullpath, fullpath + ".bak");
                        ze.Extract(RootDirectory, true);
                    }

                }

                Console.WriteLine("Done.");
            }
            else
            {
                Console.WriteLine("File does not exist: {0}", fname);
            }
        }

        protected void DbUpdate()
        {
            Console.WriteLine("Schema version: {0}", CurrentSchemaVersion);

            if (CurrentSchemaVersion == AppSchemaVersion)
            {
                Console.WriteLine("Schema is up to date.");
                return;
            }
            else if (CurrentSchemaVersion > AppSchemaVersion)
            {
                Console.WriteLine("Schema version is larger than expected! Please upgrade the application.");
                return;
            }

            string fname = Path.GetFullPath(
                Path.Combine(RootDirectory, String.Format("scripts/dbupdate_{0}_{1}.sql", CurrentSchemaVersion, CurrentSchemaVersion + 1)));
            if (!File.Exists(fname))
            {
                Console.WriteLine("File does not exist: " + fname);
                return;
            }

            string script = new System.IO.StreamReader(fname).ReadToEnd();
            // TODO ask for credentials
            try
            {
                DataProvider.ExecuteNonQuery(script);
                Console.WriteLine("Update succeeded.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("\nSchema update failed. Please manually execute the script '{0}' to update the schema.", fname);
            }

        }

        protected bool AppBackup()
        {
            string backupdir = Path.GetFullPath(Path.Combine(RootDirectory, "backups"));
            string webdir = Path.GetFullPath(Path.Combine(RootDirectory, "web"));
            // TODO verifieer of tool niet draait vanuit svn copy?
            ZipFile z = new ZipFile();
            string zipfilepath = Path.Combine(backupdir, "web_" + DateTime.Now.ToString("yyyy'-'MM'-'dd_HHmmss") + ".zip");

            Console.Out.WriteLine("Backing up to file: \n{0}", zipfilepath);
            z.AddDirectory(webdir);
            //z.AddDirectory(appdir, "zz2");

            string dir = Path.GetDirectoryName(zipfilepath);
            if (!Directory.Exists(dir))
            {
                Console.WriteLine("Creating the backup directory {0} ...", dir);
                Directory.CreateDirectory(dir);
            }

            z.Save(zipfilepath);
            return true;
        }

        protected void AppRestore(string filename)
        {
            string backupdir = Path.GetFullPath(Path.Combine(RootDirectory, "backups"));
            string webdir = Path.GetFullPath(Path.Combine(RootDirectory, "web"));
            // todo detecteer laatste backup
            if (!Directory.Exists(backupdir))
            {
                Console.WriteLine("No backups found in {0}.", backupdir);
                return;
            }

            string[] files = Directory.GetFiles(backupdir, "*.zip");
            if (files.Length == 0)
            {
                Console.WriteLine("No backups found in {0}.", backupdir);
                return;
            }

            string fname = files[0];
            Console.WriteLine(fname);
            if (File.Exists(fname))
            {
                if (Directory.Exists(webdir))
                {
                    Console.WriteLine("Deleting {0}", webdir);
                    Directory.Delete(webdir, true);
                }

                Console.WriteLine("Extracting files in " + fname);
                ZipFile z = new ZipFile(fname);
                //z.ExtractAll(rootdir, true);
                foreach (ZipEntry ze in z.Entries)
                {
                    Console.WriteLine(ze.FileName);
                    try
                    {
                        ze.Extract(RootDirectory, true);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        string fullpath = Path.GetFullPath(Path.Combine(RootDirectory, ze.FileName));
                        Console.WriteLine("No authorization to overwrite {0}, moving to {1}", fullpath, fullpath + ".bak");
                        MoveForce(fullpath, fullpath + ".bak");
                        ze.Extract(RootDirectory, true);
                    }

                }

                Console.WriteLine("Done.");
            }
        }

        protected void Download()
        {
            string updatedir = Path.GetFullPath(Path.Combine(RootDirectory, "updates"));
            Console.WriteLine("Current version is {0}.", AppVersion);

            string fname = "Grondwerk.Demo01-" + (AppVersion + 1) + ".zip";
            string url = UpdateUrl + fname;
            string path = Path.Combine(updatedir, fname);
            Console.WriteLine("Checking for updates...");

            if (File.Exists(path))
            {
                Console.WriteLine("File is already downloaded.");
                return;
            }

            if (!Directory.Exists(updatedir))
            {
                Console.WriteLine("Creating directory: {0}", updatedir);
                Directory.CreateDirectory(updatedir);
            }

            try
            {
                new System.Net.WebClient().DownloadFile(url, path);
                Console.WriteLine("Downloaded \"{0}\".", fname);
            }
            catch (System.Net.WebException e)
            {
                if (e.Status == System.Net.WebExceptionStatus.ProtocolError)
                {
                    Console.WriteLine("No updates are available.");
                }
                else PrintError(e);
            }
        }

        #region static methods
        /// <summary>
        /// Print usage information
        /// </summary>
        protected static void Usage()
        {
            Console.WriteLine("Usage: admin.exe [online|offline|status]");
        }
        protected static void MoveForce(string sourceFileName, string destFileName)
        {
            if (File.Exists(destFileName)) File.Delete(destFileName);
            File.Move(sourceFileName, destFileName);
        }

        protected static void PrintError(Exception e)
        {
            Console.WriteLine("An error has occurred: {0}", e.Message);
            Console.WriteLine(e.StackTrace);
        }
        #endregion
    }
}
