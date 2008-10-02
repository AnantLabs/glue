using System;
using System.IO;
using System.Xml;
using Glue.Lib.Text;
using NUnit.Framework;

namespace Glue.Lib.Test
{
	/// <summary>
	/// Summary description for SgmlReaderTest.
	/// </summary>
	[TestFixture]
	public class TextileTest
	{
		public TextileTest()
		{
		}

        [Test]
        public void Test()
        {
            string s = @"
h1. Header1

h2. Header2

Hello this is a paragraph. Should be
a paragraph. Must be a paragraph.
*Strong* **Bold** _Emphasized_ __Italic__

|*test*|*Test*|
|rrrr  | rrrr |


!!!Header3

# Numbered List Item
# Numbered List Item
# Numbered List Item
# Numbered List Item
    # Numbered List Item
    # Numbered List Item
# Numbered List Item
";
            Textile t = new Textile();
            s = t.Process(s);
            Console.WriteLine(s);
        }

    }
}
