using System;
using NUnit.Framework;
using Glue.Lib;

namespace Glue.Data.Test
{
	/// <summary>
	/// Summary description for App.
	/// </summary>
	public class App
	{
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Glue.Lib.Log.Level = Glue.Lib.Level.Info;
            ObjectDumper.Write(Console.Out, new Contact(), 1);

            // DataProvider
            // Tester.Run<SqlDataProviderTest>();
            Tester.Run<MySqlDataProviderTest>();
            // Tester.Run<SQLiteDataProviderTest>();

            // DataMapping
            // Tester.Run<SqlDataMappingTest>();
            Tester.Run<MySqlDataMappingTest>();
            // Tester.Run<SQLiteDataMappingTest>();
        }
    }
}
