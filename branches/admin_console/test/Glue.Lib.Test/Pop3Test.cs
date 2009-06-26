using System;
using System.Collections;
using Glue.Lib;
using Glue.Lib.Net.Pop3;
using NUnit.Framework;

namespace Glue.Lib.Test
{
	/// <summary>
	/// Summary description for SmtpServerTest.
	/// </summary>
    [TestFixture]
    public class Pop3Test
	{
		public Pop3Test()
		{
		}

        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void Done()
        {
        }

        [Test]
        public void Run()
        {
            Pop3Client pop = new Pop3Client("localhost", "postmaster", "secret");
            pop.Connect();
            foreach (int msg in pop.List())
            {
                Glue.Lib.Mime.MimePart part = pop.RetrieveMessage(msg);
                using (System.IO.Stream output = System.IO.File.Create("c:\\temp\\1-" + msg + ".eml"))
                    part.Write(output);
            }
            Hashtable map = pop.ListUniqueIds();
            foreach (int msg in map.Keys)
            {
                Console.WriteLine("" + msg + " => " + map[msg]);
                string data = pop.Retrieve(msg);
                using (System.IO.TextWriter output = System.IO.File.CreateText("c:\\temp\\2-" + msg + ".eml"))
                    output.Write(data);
            }
            // pop.Delete(1);
            pop.Close();
        }

	}
}
