using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Admin
{
    class Program : Glue.Web.Admin
    {
        static string appdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        static string webdir = Path.GetFullPath(Path.Combine(appdir, "../web"));
        static string rootdir = Path.GetFullPath(Path.Combine(appdir, "../"));
        static string updateurl = "http://localhost:86/";
        static string web_config = Path.Combine(webdir, "web.config");
        static string assembly_file = webdir + "\\bin\\web.dll";

        Program() : base(rootdir, web_config, assembly_file, "web.Global", updateurl, "Glue.Web.Admin.Demo", "web.OfflineHandler") { }

        static void Main(string[] args)
        {
            new Program().Run(args);
        }

        void Run(string[] args)
        {
            if (args.Length == 0) Usage();
            else switch (args[0].ToLower())
                {
                    case "offline":
                        Offline();
                        break;
                    case "online":
                        Online();
                        break;
                    case "status":
                        Status();
                        break;
                    case "update":
                        Update();
                        break;
                    case "configupdate":
                        ConfigUpdate();
                        break;
                    case "dbupdate":
                        DbUpdate();
                        break;
                    case "backup":
                        AppBackup();
                        break;
                    case "restore":
                        AppRestore("");
                        break;
                    case "download":
                        Download();
                        break;
                    default:
                        Usage();
                        break;
                }
        }
    }
}
