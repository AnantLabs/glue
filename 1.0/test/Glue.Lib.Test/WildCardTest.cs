using System;
using System.IO;
using System.Xml;
using Glue.Lib;
using Glue.Lib.IO;
using Glue.Lib.Text;
using NUnit.Framework;

namespace Glue.Lib.Test
{
	/// <summary>
	/// Summary description for CsvTest.
	/// </summary>
	[TestFixture]
	public class CsvTest
	{
		public CsvTest()
		{
		}

        [Test]
        public void Test()
        {
            TestSimple();
            TestWithHeader("d:\\temp\\csv\\test1.csv");
            TestWithoutHeader("d:\\temp\\csv\\test3.csv");
        }

        public void TestSimple()
        {
            string text = @"
# Comment
Id,UserName,DisplayName,Role
# Comment
1,Test1,Joep d'Ori,Guest
2,Test2,""Een,Twee,Dri"",Guest
3,,""Een """"text"","""",Admin
4,Test4,,
";
            using (CsvReader r = new CsvReader(new StringReader(text), true))
            {
                if (r.Read())
                {
                    Log.Info(r.Names[0] + "\t" + r.Names[1] + "\t" + r.Names[2] + "\t" + r.Names[3]);
                    do 
                    {
                        Log.Info(r["Id"] + "\t" + r["USERNAME"] + "\t" + r["displayname"] + "\t" + r["Role"]);
                    } while (r.Read());
                }
            }
        }

        public void TestWithHeader(string path)
        {
            using (CsvReader r = new CsvReader(File.OpenText(path), true))
            {
                int cols = 0;
                if (r.Read())
                {
                    cols = r.Values.Length;
                    Log.Info("Number of columns: " + r.Values.Length);
                    foreach (string s in r.Values)
                        Log.Info(s);
                }
                for (int i = 0; i < 10; i++)
                {
                    r.Read();
                    Log.Info(r[r.Names[9]] + "-" + r[r.Names[10]]);
                }
                while (r.Read())
                {
                }
                Log.Info("Read " + r.LineNumber + " lines.");
            }
        }
        
        public void TestWithoutHeader(string path)
        {
            using (CsvReader r = new CsvReader(File.OpenText(path), false))
            {
                int cols = 0;
                if (r.Read())
                {
                    cols = r.Values.Length;
                    Log.Info("Number of columns: " + r.Values.Length);
                    foreach (string s in r.Values)
                        Log.Info(s);
                }
                for (int i = 0; i < 10; i++)
                {
                    r.Read();
                    Log.Info(r[0] + "-" + r[9] + "-" + r[10]);
                }
                while (r.Read())
                {
                }
                Log.Info("Read " + r.LineNumber + " lines.");
            }
        }
    }
}
