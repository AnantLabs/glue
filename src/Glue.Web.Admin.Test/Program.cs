using System;
using System.IO;
using System.Reflection;
using System.Xml;

using Glue.Data;

namespace Glue.Web.AdminTest
{
    class Program : Glue.Web.Admin
    {
        static string appdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        static string webdir = Path.GetFullPath(Path.Combine(appdir, "../web"));
        static string updatedir = Path.GetFullPath(Path.Combine(appdir, "../updates"));
        static string backupdir = Path.GetFullPath(Path.Combine(appdir, "../backups"));
        static string rootdir = Path.GetFullPath(Path.Combine(appdir, "../"));

        static string updateurl = "http://localhost:86/";

        static string web_config = Path.Combine(webdir, "web.config");
        static string assembly_file = webdir + "\\bin\\demo01.dll";

        ///// <summary>
        ///// Return true iff application is online
        ///// </summary>
        //private static bool isOnline(XDocument doc)
        //{
        //    XNode handlerNode;
        //    try
        //    {
        //        handlerNode = doc.Root.Element("system.web").Element("httpHandlers").LastNode.PreviousNode;
        //    }
        //    catch (NullReferenceException)
        //    {
        //        return true; // Geen offline node gevonden
        //    }

        //    // een na laatse node moet de offlineHandler instellen
        //    if (handlerNode.NodeType == XmlNodeType.Element
        //            && ((XElement)handlerNode).Name == "add"
        //            && ((XElement)handlerNode).Attribute("type") != null
        //            && ((XElement)handlerNode).Attribute("type").Value == "demo01.OfflineHandler")
        //        return false;
        //    else return true;
        //}

        /// <summary>
        /// Geeft configversion waarde uit document(web.config). Default waarde is 0.
        /// </summary>
        //private static int GetCurrentConfigVersion(XDocument document)
        //{
        //    var v = from c in document.Root.Elements("appSettings").Elements("add")
        //            where (string)c.Attribute("key") == "configversion" 
        //            select c.Attribute("value");
        //    if (v.Count() > 0)
        //        return (int)v.First();
        //    return 0;
        //}

        private static int GetCurrentSchemaVersion()
        {
            //XmlDocument doc = new XmlDocument(); doc.Load(web_config);
            //PostgreSQLDataProvider provider = new PostgreSQLDataProvider(doc.GetElementsByTagName("dataprovider")[0]);

            IDataProvider provider = (IDataProvider)Glue.Lib.Configuration.Get("dataprovider");
            return provider.ExecuteScalarInt32("SELECT public.version()");
        }

        static void PrintError(Exception e)
        {
            Console.WriteLine("An error has occurred: {0}", e.Message);
#if DEBUG
            Console.WriteLine(e.StackTrace);
#endif
        }

        static void MoveForce(string sourceFileName, string destFileName)
        {
            if (File.Exists(destFileName)) File.Delete(destFileName);
            File.Move(sourceFileName, destFileName);
        }

        Program() : base(rootdir, web_config, assembly_file, "demo01.Global", updateurl) { }

        static void Main(string[] args)
        {
            new Program().Run(args);
        }

        void Run(string[] args)
        {
            //Glue.Lib.Configuration.Register(web_config, false);
            if (args.Length == 0) Usage();
            else switch (args[0].ToLower())
                {
                    case "offline":
                        //Offline();
                        break;
                    case "online":
                        //Online();
                        break;
                    case "status":
                        Status();
                        break;
                    case "update":
                        //Update();
                        break;
                    case "configupdate":
                        ConfigUpdate();
                        break;
                    case "dbupdate":
                        //DbUpdate();
                        break;
                    case "backup":
                        //AppBackup();
                        break;
                    case "restore":
                        //AppRestore("");
                        break;
                    case "download":
                        Download();
                        break;
                    default:
                        Usage();
                        break;
                }
        }

        /// <summary>
        /// Print usage information
        /// </summary>
        static void Usage()
        {
            Console.WriteLine("Usage: manager.exe [online|offline|status]");
        }



        /// <summary>
        /// Take application offline by editing Web.config
        /// </summary>
        //void Offline()
        //{
        //    XDocument document = XDocument.Load(web_config);
        //    if (!IsOnline)
        //    {
        //        Console.WriteLine("Application is already offline.");
        //        return;
        //    }

        //    // Add any missing parent elements
        //    if (document.Root.Element("system.web") == null)
        //        document.Root.Add(new XElement("system.web"));
        //    if (document.Root.Element("system.web").Element("httpHandlers") == null)
        //        document.Root.Element("system.web").Add(new XElement("httpHandlers"));

        //    document.Root.Element("system.web").Element("httpHandlers").Add(
        //        new XElement("clear"),
        //        new XElement("add", new XAttribute("verb", "*"), new XAttribute("path", "*"), new XAttribute("type", "demo01.OfflineHandler")),
        //        new XComment("Taken offline by manager.exe")
        //    );

        //    // backup original web.config and replace it.
        //    //File.Copy(web_config, web_config_saved);
        //    document.Save(web_config);

        //    Console.WriteLine("Application is now offline.");
        //}

        /// <summary>
        /// Put application online.
        /// </summary>
        //void Online()
        //{
        //    XDocument document = XDocument.Load(web_config);
        //    if (IsOnline)
        //    {
        //        Console.WriteLine("Application is already online.");
        //        return;
        //    }

        //    document.Root.Element("system.web").Element("httpHandlers").LastNode.Remove(); // comment
        //    document.Root.Element("system.web").Element("httpHandlers").LastNode.Remove(); // add offlineHandler 
        //    document.Root.Element("system.web").Element("httpHandlers").LastNode.Remove(); // clear

        //    document.Save(web_config);
        //    Console.WriteLine("Application is now online.");
        //}

        //void Update()
        //{
        //    int version = AppVersion;
        //    string fname = Path.Combine(updatedir, String.Format("Grondwerk.Demo01-{0}.zip", version + 1));

        //    Console.WriteLine(fname);
        //    if (File.Exists(fname))
        //    {
        //        Console.WriteLine("Extracting files in " + fname);
        //        ZipFile z = new ZipFile(fname);
        //        //z.ExtractAll(rootdir, true);
        //        foreach (ZipEntry ze in z.Entries)
        //        {
        //            Console.WriteLine(ze.FileName);
        //            try
        //            {
        //                ze.Extract(rootdir, true);
        //            }
        //            catch (UnauthorizedAccessException)
        //            {
        //                string fullpath = Path.GetFullPath(Path.Combine(rootdir, ze.FileName));
        //                Console.WriteLine("No authorization to overwrite {0}, moving to {1}", fullpath, fullpath + ".bak");
        //                MoveForce(fullpath, fullpath + ".bak");
        //                ze.Extract(rootdir, true);
        //            }

        //        }

        //        Console.WriteLine("Done.");
        //    }
        //    else
        //    {
        //        Console.WriteLine("File does not exist: {0}", fname);
        //    }
        //}

        void ConfigUpdate()
        {
            int appConfigVersion = AppConfigVersion;
            int configVersion = CurrentConfigVersion;

            if (configVersion > appConfigVersion)
            {
                Console.WriteLine("Config version is larger than expected. Please use 'update' to update the application.");
                return;
            }
            else if (configVersion == appConfigVersion)
            {
                Console.WriteLine("Config is up to date.");
                return;
            }

            string fname = Path.GetFullPath(
                Path.Combine(rootdir, String.Format("scripts/configupdate_{0}_{1}.py", configVersion, configVersion + 1)));
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

        //void DbUpdate()
        //{
        //    // Get database schema version, using dataprovider in web.config
        //    XmlDocument doc = new XmlDocument(); doc.Load(web_config);
        //    PostgreSQLDataProvider provider = new PostgreSQLDataProvider(doc.GetElementsByTagName("dataprovider")[0]);
        //    int schemaVersion = GetCurrentSchemaVersion();
        //    Console.WriteLine("Schema version: {0}", schemaVersion);

        //    int expectedVersion = AppSchemaVersion;

        //    if (schemaVersion == expectedVersion)
        //    {
        //        Console.WriteLine("Schema is up to date.");
        //        return;
        //    }
        //    else if (schemaVersion > expectedVersion)
        //    {
        //        Console.WriteLine("Schema version is larger than expected! Please upgrade the application.");
        //        return;
        //    }

        //    string fname = Path.GetFullPath(
        //        Path.Combine(rootdir, String.Format("scripts/dbupdate_{0}_{1}.sql", schemaVersion, schemaVersion + 1)));
        //    if (!File.Exists(fname))
        //    {
        //        Console.WriteLine("File does not exist: " + fname);
        //        return;
        //    }

        //    string script = new System.IO.StreamReader(fname).ReadToEnd();
        //    // TODO ask for credentials
        //    try
        //    {
        //        provider.ExecuteNonQuery(script);
        //        Console.WriteLine("Update succeeded.");
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //        Console.WriteLine("\nSchema update failed. Please manually execute the script '{0}' to update the schema.", fname);
        //    }

        //}

        //static bool AppBackup()
        //{
        //    // TODO verifieer of tool niet draait vanuit svn copy?
        //    ZipFile z = new ZipFile();
        //    string zipfilepath = Path.Combine(backupdir, "web_" + DateTime.Now.ToString("yyyy'-'MM'-'dd_HHmmss") + ".zip");

        //    Console.Out.WriteLine("Backing up to file: \n{0}", zipfilepath);
        //    z.AddDirectory(webdir);
        //    //z.AddDirectory(appdir, "zz2");

        //    string dir = Path.GetDirectoryName(zipfilepath);
        //    if (!Directory.Exists(dir))
        //    {
        //        Console.WriteLine("Creating the backup directory {0} ...", dir);
        //        Directory.CreateDirectory(dir);
        //    }

        //    z.Save(zipfilepath);
        //    return true;
        //}

        //static void AppRestore(string filename)
        //{
        //    // todo detecteer laatste backup
        //    if (!Directory.Exists(backupdir))
        //    {
        //        Console.WriteLine("No backups found in {0}.", backupdir);
        //        return;
        //    }

        //    string[] files = Directory.GetFiles(backupdir, "*.zip");
        //    if (files.Length == 0)
        //    {
        //        Console.WriteLine("No backups found in {0}.", backupdir);
        //        return;
        //    }

        //    string fname = files[0];
        //    Console.WriteLine(fname);
        //    if (File.Exists(fname))
        //    {
        //        if (Directory.Exists(webdir))
        //        {
        //            Console.WriteLine("Deleting {0}", webdir);
        //            Directory.Delete(webdir, true);
        //        }

        //        Console.WriteLine("Extracting files in " + fname);
        //        ZipFile z = new ZipFile(fname);
        //        //z.ExtractAll(rootdir, true);
        //        foreach (ZipEntry ze in z.Entries)
        //        {
        //            Console.WriteLine(ze.FileName);
        //            try
        //            {
        //                ze.Extract(rootdir, true);
        //            }
        //            catch (UnauthorizedAccessException)
        //            {
        //                string fullpath = Path.GetFullPath(Path.Combine(rootdir, ze.FileName));
        //                Console.WriteLine("No authorization to overwrite {0}, moving to {1}", fullpath, fullpath + ".bak");
        //                MoveForce(fullpath, fullpath + ".bak");
        //                ze.Extract(rootdir, true);
        //            }

        //        }

        //        Console.WriteLine("Done.");
        //    }
        //}

        void Download()
        {
            int version = AppVersion;
            Console.WriteLine("Current version is {0}.", version);

            string fname = "Grondwerk.Demo01-" + (version + 1) + ".zip";
            string url = updateurl + fname;
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
    }
}
