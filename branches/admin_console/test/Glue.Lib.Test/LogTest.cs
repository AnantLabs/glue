using System;
using NUnit.Framework;

namespace Glue.Lib.Test
{
	/// <summary>
	/// Summary description for ConfigTest.
	/// </summary>
    [TestFixture]
	public class LogTest
	{
		public LogTest()
		{
		}
        
        [Test]
        public void Test()
        {
            Log.Debug("Debug message");
            Log.Info("Info message");
            Log.Warn("Warning message");
            Log.Error("Error message");
            Log.Fatal("Fatal message");
        }
    }
}
