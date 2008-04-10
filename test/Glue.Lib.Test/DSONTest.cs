using System;
using NUnit.Framework;
using Glue.Lib;

namespace Glue.Lib.Test
{
    [TestFixture]
    public class DSONTest
    {
        [Test]
        public static void Test()
        {
            string dson = @"
{
    test1: 2006-10-30T04:40
    test2: 04:40:50
    Member1: ""Va\tl\tue1"",

    Member2: [
                1
// Hello world
                """"""
                Hello my friend are you visible today?
                You know I never knew that it could be so strange
                """"""
/*              3,*/
                {
                    Member2Array4Test1: ""hello\""x\r\n ""
                    Member2Array4Test2: 3
                }
             ]
    jobs: [
        {
            name: Wok
            start: 2006-10-30T04:40
            enabled: yes
            retries: 10
            schedule: [
                { type: weekly, day: Mon, time: 01:20 }
                { type: weekly, day: Tue, time: 01:20 }
                { type: weekly, day: Wed, time: 01:20 }
            ]
        },
        {
            name: Wok,
            schedule: [
                { type: weekly, day: Mon, time: ""01:20"" }
                { type: weekly, day: Tue, time: ""01:20"" }
                { type: weekly, day: Wed, time: ""01:20"" }
            ]
        },
        {
            name:  Wok
            schedule: [ 
                { type: daily, time: ""01:20"" }
                { type: daily, time: ""04:20"" }
            ]
            description: """"""
                If you are confused check with the sun
                Carry a compass to help you along

                Your feet are going to be on the ground
                Your head is there to move you around
            """"""
            batch: """"""
                @echo off
                edf-backup --include x
                if errorlevel 0 goto end
                edf-cleanup
                curl
                :end
            """"""
        }
    ]
}
";
            Glue.Lib.Text.DSON.Scanner s = new Glue.Lib.Text.DSON.Scanner(dson);
            Glue.Lib.Text.DSON.Parser x = new Glue.Lib.Text.DSON.Parser(s);
            x.Parse();
            Console.WriteLine(x.Errors);
            Console.WriteLine(x.Result);
            Glue.Lib.Text.DSON.Item root = Glue.Lib.Text.DSON.Helper.Parse(dson);
            Console.WriteLine(root.Inspect());
            root.Set("Member3", new int[] {1,2,3,4,5});
            Console.WriteLine(root.Inspect());
            foreach (Glue.Lib.Text.DSON.Item item in root)
                Console.WriteLine(item.Inspect());
            object z = root.Get("Member3").Get(2);
        }
    }
}
