using System;
using NUnit.Framework;
using Glue.Lib;

namespace Glue.Lib.Test
{
	[TestFixture]
	public class JSONTest
	{
        [Test]
		public static void Test()
		{
            string json = @"
{
    Member1: ""Va\tl\tue1"",
    Member2: [
                1,
    // Hello world
                ""wok"",
/*                3,
*/{
                    Member2Array4Test1: ""hello\""x\r\n "",
                    Member2Array4Test2: 3
                }
             ]
}
";
            
            Glue.Lib.Text.JSON.Scanner s = new Glue.Lib.Text.JSON.Scanner(json);
            Glue.Lib.Text.JSON.Parser x = new Glue.Lib.Text.JSON.Parser(s);
            x.Parse();
            Console.WriteLine(x.Errors);
            Console.WriteLine(x.Result);
            Glue.Lib.Text.JSON.Helper.Parse(json);
        }
	}
}
