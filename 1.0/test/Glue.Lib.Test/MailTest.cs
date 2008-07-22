using System;
using System.IO;
using Glue.Lib.Mail;
using NUnit.Framework;

namespace Glue.Lib.Test
{
	/// <summary>
	/// Summary description for MailTest.
	/// </summary>
	[TestFixture]
	public class MailTest
	{
        static string server = "calypso";

		public MailTest()
		{
		}

        [Test]
        public void TestTextMail()
        {
            Console.WriteLine("Done.");
        }
        
        [Test]
        public void TestHtmlMail()
        {
            Console.WriteLine("Done.");
        }
        
        [Test]
        public void TestAttachments()
        {
            MailMessage msg = new MailMessage();
            msg.From = MailAddress.Parse("Gertjan [xs4ăll.nl] gjs@xs4all.nl");
            msg.To = MailAddressCollection.Parse("\"Gertjan [èdf]\" gertjan@edf.nl");
            msg.Bcc = MailAddressCollection.Parse("snok@xs4all.nl");
            msg.Subject = "Hatsaklatsa doet het ook met geïndiceerde dingen.";
            msg.TextBody = 
@"Hallo daar
Ziet er goed uit. ËÇÅÃ hoop ik.

----
Sent by Edf.Mail.Mime
";
            msg.HtmlBody = 
@"<html>
<head>
<link rel=""stylesheet"" href=""http://www.edf.nl/catalog/css/internal.css"">
</head>
<body>
<h2>Hallo daar! ËÇÅÃ</h2>
<p>dit mailtje gaat van gjs naar zonnet met
een bcc naar edf. אבג</p>
<hr>
Sent by Edf.Mail.Mime
</body>
</html>";
            msg.Attachments.Add(new MailAttachment("d:\\documents\\logo-outline.tif", true));
            msg.Attachments.Add(new MailAttachment("d:\\documents\\test.pdf"));
            msg.Priority = MailPriority.Low;

            SmtpMail.SmtpServer = server;
            SmtpMail.Send(msg);
        }
    
        [Test]
        public void TestSpecialChars()
        {
            MailMessage msg = new MailMessage();
            msg.From = MailAddress.Parse("Gertjan [xs4ăll.nl] snok@xs4all.nl");
            msg.To = MailAddressCollection.Parse("\"Gærtjăn [èdf]\" gertjan@edf.nl, \"Wouter\" wouter@edf.nl");
            msg.Bcc = MailAddressCollection.Parse("gjs@xs4all.nl");
            msg.Subject = "Hatsaklatsa doet het ook met geïndiceerde dingen.";
            msg.TextBody = 
@"Hallo daar
Ziet er goed uit. ËÇÅÃ hoop ik.
";
            msg.HtmlBody = 
@"<html>
<head>
<link rel=""stylesheet"" href=""http://www.edf.nl/catalog/css/internal.css"">
</head>
<body>
<h2>Hatsaklatsa! ËÇÅÃ nu ook met vreemde tekens.</h2>
<p>dit mailtje gaat van gjs naar zonnet met
een bcc naar edf. אבג</p>
<hr>
Sent by Edf.Mail.Mime
</body>
</html>";

            SmtpMail.SmtpServer = server;
            SmtpMail.Send(msg);
        }
    }
}
