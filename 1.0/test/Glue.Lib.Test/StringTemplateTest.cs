using System;
using System.Reflection;
using System.IO;
using Glue.Lib;
using Glue.Lib.Text;

namespace Glue.Lib.Test
{
	/// <summary>
	/// Summary description for StringTemplateTest.
	/// </summary>
	public class StringTemplateTest
	{
        public static void Test()
        {
            TestSimple();
            TestComplex();
        }
         
        public static void TestSimple()
        {
            StringTemplate t = StringTemplate.Create(
                "#if true# hello #end"
                );
            t.Render(Console.Out);
    
            t = StringTemplate.Create(@"
#def perc(i,j)
    #if i > 0 
        XX $perc(i-1, j)
    #else
        HA ${i*j}
    #end
#end

Hello $age $level $perc(5,8) $test(0)
#for i in range(1,6)
    $info('x'+i)
#sep
    ******************* $i
#end
#for i in range(1,6)
    <option value=""xx$i"">$test(i)<option>
#end
"
                ,
                Log.Instance
                );
            t["age"] = 20;
            t["level"] = 320;
            t.Render(Console.Out);
        }

        public static void TestComplex()
        {
            string test = LoadRes(Assembly.GetExecutingAssembly(), "Glue.Lib.Test.StringTemplateTest.txt");
            Console.WriteLine(test);
        }

        public static string LoadRes(Assembly assembly, string name)
        {
            using (StreamReader reader = new StreamReader(assembly.GetManifestResourceStream(name)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
