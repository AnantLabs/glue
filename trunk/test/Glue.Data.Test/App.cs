using System;
using NUnit.Framework;

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
            Glue.Lib.Log.Level = Glue.Lib.Level.Debug;
            // Data mapping
            DataMappingTest.Test();
        }
    }
}
