using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.IO;

using IdSharp.Tagging.ID3v1;
using Glue.Lib;
using Glue.Data;
using Glue.Data.Providers.SQLite;

namespace mp3sql
{
    class Program
    {
        // Database will be located in ~\Application Data\mp3sql\sqlite.db
        // or ~/.config/mp3sql/sqlite.db or $XDG_DATA_DIR/mp3sql/sqlite.db
        static string dbfile = Path.GetFullPath(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "mp3sql/sqlite.db"));


        static void Main(string[] args)
        {
            Log.Level = Level.Debug;
            ID3v1 tags = null;

            Log.Debug(dbfile);
            try
            {
                tags = new ID3v1("../../13 - Susanna Hoffs - The Look of Love.mp3");
            }
            catch (System.IO.FileNotFoundException)
            {
                Console.Error.WriteLine("Could not find file");
                return;
            }
            Console.WriteLine(tags.Artist);
            Console.WriteLine(tags.Title);

            Console.WriteLine("Database in: " + dbfile);
            
            //IDbConnection conn = (IDbConnection)new SQLiteConnection("Data Source=" + dbfile + ";New=True");
            //conn.Open();
            //IDbCommand cmd = conn.CreateCommand();
            //cmd.CommandText = "SELECT count(*) FROM sqlite_master";
            //object o = cmd.ExecuteScalar();
            //Int32 result = Convert.ToInt32(o);
            //PrintCommand(conn, "select * from track;");
            //conn.Close();
            //Console.WriteLine("Number of tables: " + result);

            
            // TODO: check op file niet bestaand / in use / verkeerd formaat / acces denied etc.
            IMappingProvider provider = new SQLiteMappingProvider("Data Source=" + dbfile + ";New=True");
            using (IDbConnection conn = provider.CreateConnection())
            {
                int tables = provider.ExecuteScalarInt32("SELECT count(*) FROM sqlite_master;");
                Console.WriteLine("Number of tables: " + tables);
                //if (tables == 0)
                //{
                //    CreateTables(provider);
                //}

                //tables = provider.ExecuteScalarInt32("SELECT count(*) FROM sqlite_master;");
                //Console.WriteLine("Number of tables: " + tables);
                PrintCommand(conn, "SELECT * FROM track");
                Array l = provider.List(typeof(Track), null, null, null);
                foreach (Track t in l)
                {
                    Console.WriteLine("Track: {0}\n  {1}\n  {2}\n  {3}\n  {4}", t.Id, t.Title, t.Path, t.Year, t.Quality);
                }
            }

            Console.WriteLine("Generic path: " + provider.Find<Track>(0).Path);

            foreach (Track t in ((SQLiteMappingProvider)provider).List<Track>(null, null, null)) 
            {
                Console.WriteLine("Track: {0}\n  {1}\n  {2}\n  {3}\n  {4}", t.Id, t.Title, t.Path, t.Year, t.Quality);
            }
        }

//        static void CreateTables(IDataProvider provider)
//        {
//            provider.ExecuteNonQuery(@"
//                CREATE TABLE tracks (
//			        id     INTEGER PRIMARY KEY,
//			        path   VARCHAR(255) UNIQUE NOT NULL,
//			        title  VARCHAR(30),
//			        artist VARCHAR(30),
//			        year   INTEGER,
//			        length INTEGER
//		        );
//            ");
//        }

        /// <summary>
        /// Print results of sql command
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="cmdStr"></param>
        static void PrintCommand(IDbConnection conn, string cmdStr)
        {
            Console.WriteLine(conn.State);
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            IDbCommand cmd = conn.CreateCommand();
            cmd.CommandText = cmdStr;
            IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Console.Write(reader[i] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
