using System;
using NUnit.Framework;

namespace Glue.Lib.Test
{
	/// <summary>
	/// Summary description for App.
	/// </summary>
	public class App
	{
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Mapper
            // MapperTest.Test();
            // return;

            // new Pop3Test().Run();
            // MySqlTest.Test();

            DSONTest.Test();
            JSONTest.Test();

            // StringTemplate
            // StringTemplateTest.Test();
            // Glue.Lib.Text.StringTemplate t = Glue.Lib.Text.StringTemplate.CreateFromFile(@"D:\Drafts\Sitelauncher_Design\sitelauncher2.html");
            // Console.WriteLine(t.Render());


            CsvTest csvTest = new CsvTest();
            // csvTest.Test();

            // Data mapping
            // DataMappingTest.Test();

            // WildCard
            WildCardTest wildCardTest = new WildCardTest();
            wildCardTest.Test();

            // Textile
            TextileTest textileTest = new TextileTest();
            textileTest.Test();

            // Mime
            MimeTest mimeTest = new MimeTest();
            mimeTest.Run();

            // HttpServer
            HttpServerTest httpServerTest = new HttpServerTest();
            httpServerTest.Setup();
            httpServerTest.Run();
            httpServerTest.Done();

            // SmtpServer
            SmtpServerTest smtpServerTest = new SmtpServerTest();
            smtpServerTest.Setup();
            // smtpServerTest.Run();
            smtpServerTest.Done();

            // CommandLine
            // TODO:
            // CommandLineTest commandLineTest = new CommandLineTest();
            // commandLineTest.Run();

            // Sgml
            // TODO:
            // SgmlTest sgmlTest = new SgmlTest();
            // sgmlTest.TestHorribleHtml1();
            // sgmlTest.TestHorribleHtml2();

            // Mail
            MailTest mailTest = new MailTest();
            mailTest.TestSpecialChars();
        }
    }
}
