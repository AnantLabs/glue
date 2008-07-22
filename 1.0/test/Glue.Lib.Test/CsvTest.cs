using System;
using System.IO;
using System.Xml;
using Glue.Lib;
using Glue.Lib.IO;
using NUnit.Framework;

namespace Glue.Lib.Test
{
	/// <summary>
	/// Summary description for SgmlReaderTest.
	/// </summary>
	[TestFixture]
	public class WildCardTest
	{
		public WildCardTest()
		{
		}

        [Test]
        public void Test()
        {
            bool m = WildCard.Matches("index.html", "*.html");
            Console.WriteLine(m);
            m = WildCard.Matches("index.html", "*.h?ml");
            Console.WriteLine(m);
            m = WildCard.Matches("index.html", "index.html");
            Console.WriteLine(m);
            m = WildCard.Matches("index.html", "*index.html");
            Console.WriteLine(m);
            m = WildCard.Matches("index.html", "*index.html*");
            Console.WriteLine(m);
            m = WildCard.Matches("index.html", "*index.html?");
            Console.WriteLine(m);
            m = WildCard.Matches("index.html", "*");
            Console.WriteLine(m);
            m = WildCard.Matches("index.html", "");
            Console.WriteLine(m);
            m = WildCard.Matches("", "");
            Console.WriteLine(m);
            m = WildCard.Matches("", "x");
            Console.WriteLine(m);
            m = WildCard.Matches("", "?");
            Console.WriteLine(m);
            m = WildCard.Matches("", "*");
            Console.WriteLine(m);
            m = WildCard.Matches("*", "*");
            Console.WriteLine(m);
        }
    }
}
