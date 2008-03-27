using System;
using Glue.Lib;
using Glue.Data;
using Glue.Data.Schema;

namespace Glue.Web
{
#if DEBUG
	/// <summary>
	/// Summary description for SchemaController.
	/// </summary>
    public class SchemaController : Controller
    {
        public SchemaController(IRequest request, IResponse response) : base(request, response)
        {
        }
        
        public void Index()
        {
            Response.Write(@"<html>
<head>
<style>
body,p,td,th { font-family:verdana,arial,helvetica;font-size:70%; }
pre          { font-family: lucida console; font-weight: normal; margin-top: 0px; margin-left: 20px; background-color: #f3f0e8; font-size:100%; }
code         { font-weight: 400; font-family: ""courier new""; } 
em           { color: red; font-style: normal; }
h1           { font-weight: normal; font-size: 165%; }
h2           { font-family:arial; font-weight: bold; font-size: 124%; }
</style>
</head>
<body>
<h1>Schema</h1>
<ul>
<li><a href=""view"">Show current schema</a></li>
<li><a href=""diff"">Compare schemas</a></li>
</ul>
</body>
</html>
"
                );
        }

        public void View(string provider)
        {
            Database database = Management.OpenDatabaseFromConfiguration(provider);
            Response.ContentType = "text/xml";
            System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(Response.Output);
            writer.Formatting = System.Xml.Formatting.Indented;
            Management.SaveSchemaToXml(database, writer);
            writer.Close();
        }

        public void Diff(string provider)
        {
            Response.Write(@"<html>
<head>
<style>
body,p,td,th { font-family:verdana,arial,helvetica;font-size:70%; }
pre          { font-family: lucida console; font-weight: normal; margin-top: 0px; margin-left: 20px; background-color: #f3f0e8; font-size:100%; }
code         { font-weight: 400; font-family: ""courier new""; } 
em           { color: red; font-style: normal; }
h1           { font-weight: normal; font-size: 165%; }
h2           { font-family:arial; font-weight: bold; font-size: 124%; }
</style>
</head>
<body>
<h1>Schema - Compare</h1>
");
            if (!IsPostBack)
            {
                Response.Write(@"
<form method=""post"" enctype=""multipart/form-data"">
<label>Upload schema</label>
<input name=""schema"" type=""file"" />
<input type=""submit"" value=""OK""/>
</form>"
                    );
            }
            else
            {
                Database database1 = Management.OpenDatabaseFromConfiguration(provider);
                Database database2 = Management.LoadSchemaFromXml(new System.Xml.XmlTextReader(Request.Files[0].Content));
                Response.Write("<pre>");
                SchemaDiff.Compare(database1, database2, Response.Output);
                Response.Write("</pre>");
            }
            Response.Write(@"
</body>
</html>
");
        }
    }
#endif
}
