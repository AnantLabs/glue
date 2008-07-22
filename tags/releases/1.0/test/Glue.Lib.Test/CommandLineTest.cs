using System;
using NUnit.Framework;
using Glue.Lib.Options;

namespace Glue.Lib.Test
{
    /// <summary>
    /// 
    /// </summary>
    class CommandLine
    {
        [Option(Short='p', Help="Just a testing Parameter")]
        public string[] Param = new string[] { "Default value" };

        [Option(Short='t', Name="turn", Help="Just a boolean testing parameter")]
        public bool TurnItOn = false;

        private bool verboseOn = false;

        [Option(Short='v', Help="Be verbose")]
        public bool Verbose
        {
            get
            {
                return verboseOn;
            }
            set 
            { 
                verboseOn = value; 
                Console.WriteLine("verbose was set to : " + verboseOn);
            }
        }

        [Option(Name="simple")]
        public void SimpleProcedure()
        {
            Console.WriteLine("Inside simpleProcedure()");
        }
    }

    
    /// <summary>
	/// Summary description for SmtpServerTest.
	/// </summary>
    [TestFixture]
    public class CommandLineTest
	{
		public CommandLineTest()
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
            string[] args = {"-V","-simple","-turn"};
            CommandLine options = new CommandLine();
            OptionConvert.Assign(options, args);
        }

	}
}
