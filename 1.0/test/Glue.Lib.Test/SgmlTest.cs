using System;
using System.IO;
using System.Xml;
using Glue.Lib.Xml.Sgml;
using NUnit.Framework;

namespace Glue.Lib.Test
{
	/// <summary>
	/// Summary description for SgmlReaderTest.
	/// </summary>
	[TestFixture]
	public class SgmlTest
	{
		public SgmlTest()
		{
		}

        [Test]
        public void TestHorribleHtml1()
        {
            string html = @"
                    <html>
                    <head>
                    <title>Horrible HTML</title>
                    <style><!-- { <X } test --></style>
                    </head>
                    <body leftmargin='4' topmargin=3>
                    <ul>
                    <li>listitem
                    <li>listitem</li>
                    </ul>
                    <table align=left>
                    <tr>Hello
                    <tr><td>Hi
                    test<br>
                    <p>nieuwe para
                    </body>
                    </html>
                    ";

            SgmlReader reader = new SgmlReader();
            reader.SetBaseUri("http://some-fantasy-url/");
            reader.DocType = "HTML";
            // reader.ToUpper = false;
            reader.WhitespaceHandling = WhitespaceHandling.None;
            reader.InputStream = new StringReader(html);
            XmlTextWriter writer = new XmlTextWriter(Console.Out);
            writer.Formatting = Formatting.Indented;
            while (!reader.EOF) 
            {
                writer.WriteNode(reader, true);
            }
            writer.Close();

        }

        [Test]
        public void TestHorribleHtml2()
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            using (Stream stm = wc.OpenRead("http://www.directwonen.nl/3/f01-frontpage.asp"))
            {
                SgmlReader reader = new SgmlReader();
                reader.DocType = "HTML";
                // reader.ToUpper = false;
                reader.WhitespaceHandling = WhitespaceHandling.None;
                reader.InputStream = new StreamReader(stm);

                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                
                Console.WriteLine("All images");
                foreach (XmlElement elm in doc.SelectNodes("//img"))
                {
                    Console.WriteLine("img => " + elm.GetAttribute("src"));
                }

                Console.WriteLine("All links");
                foreach (XmlElement elm in doc.SelectNodes("//a"))
                {
                    Console.WriteLine("a => " + elm.GetAttribute("href") + " " + elm.InnerText);
                }

                doc.Save(Console.Out);

                stm.Close();
            }
        }
    }
}
