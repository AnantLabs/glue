using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.IO;

using IdSharp.Tagging.ID3v1;
using Glue.Lib;
using Glue.Data;
using Glue.Data.Schema;
using Glue.Data.Providers.SQLite;

namespace mp3sql
{
    class Program
    {
        // The database will be located in ~\Application Data\mp3sql\
        // or ~/.config/mp3sql/ or $XDG_DATA_DIR/mp3sql/
        static string dbfile = Path.GetFullPath(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "mp3sql/sqlite.db"));
        static DataContext context;

        static void Main(string[] args)
        {
            Log.Level = Level.Fatal;

            //args = new string[] { "add", "../../fl.mp3" };

            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            // Create directory for db file if it doesn't exist yet.
            string dbDirectory = Path.GetDirectoryName(dbfile);
            if (!Directory.Exists(dbDirectory))
                Directory.CreateDirectory(dbDirectory);

            // Connection string for SQLite
            string connectionstring = "Data Source=" + dbfile;
            if (!File.Exists(dbfile))
                connectionstring += ";New=True";

            context = new DataContext(new SQLiteMappingProvider(connectionstring));
            context.CreateTables();

            switch (args[0])
            {
                case "list":
                    List();
                    return;
                case "add":
                    Add(args);
                    return;
                case "schema":
                    Schema();
                    return;
                default:
                    PrintUsage();
                    return;
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage:\nmp3sql add [filenames]\nmp3sql list");
        }

        /// <summary>
        /// Add mp3 files to database
        /// </summary>
        /// <param name="args"></param>
        static void Add(string[] args)
        {
            ID3v1 tags = null;
            string path = null;

            using (context.Provider.CreateConnection())
            {
                // Loop over files to add
                for (int i = 1; i < args.Length; i++)
                {
                    path = Path.GetFullPath(args[i]);
                    try
                    {
                        tags = new ID3v1(path);
                        //tags.Album = "XXX";
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        Console.Error.WriteLine("Could not find file: " + path);
                        continue;
                    }

                    // Save track to database
                    Track track = context.TrackFindOrNew(path);
                    track.Assign(tags);
                    context.TrackSave(track);

                    // Save Album to database
                    Album album = context.AlbumFindOrNew(tags.Album);
                    context.AlbumSave(album);

                    // Put track in album
                    context.AddTrackToAlbum(track, album);
                    Console.WriteLine("Added file: " + path);
                }
            }
        }

        /// <summary>
        /// List all tracks in database
        /// </summary>
        static void List()
        {
            IList<Track> ts = context.Provider.List<Track>(null, null, null);
            foreach (Track t in ts) 
                Console.WriteLine(t.Title + " (" + t.Artist + ")");
        }

        /// <summary>
        /// Print database schema
        /// </summary>
        static void Schema()
        {
            ISchemaProvider schema = context.Provider.GetSchemaProvider();
            Database[] dbs = schema.GetDatabases();
            Table[] tables = schema.GetTables(dbs[0]);

            //IDataReader reader = context.Provider.ExecuteReader("pragma table_info(album)", null);
            // IDataReader reader = context.Provider.ExecuteReader("pragma table_info(@test)", "@test", "test"); // doesnt work :(

            //PrettyPrint.Print(Console.Out, reader);

            // write database schema XML
            System.Xml.XmlWriter w = System.Xml.XmlWriter.Create(Console.Out);
            dbs[0].Write(w);
            w.Flush();

            // Write create script for this schema in SQLite
            //schema.Script(schema.GetDatabase(null), Console.Out);
        }
    }
}
