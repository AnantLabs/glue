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
        static string rootdir = Path.GetFullPath(Path.Combine(appdir, "../"));
        static string updateurl = "http://localhost:86/";
        static string web_config = Path.Combine(webdir, "web.config");
        static string assembly_file = webdir + "\\bin\\demo01.dll";

        Program() : base(rootdir, webdir, assembly_file, "demo01.Global", updateurl, "demo01.OfflineHandler") { }

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
