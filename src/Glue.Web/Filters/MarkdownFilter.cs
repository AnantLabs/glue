using System;

namespace Glue.Web.Filters
{
	/// <summary>
	/// Summary description for MarkdownFilter.
	/// </summary>
	public class MarkdownFilter : Glue.Lib.Text.Markdown
	{
        public static string Apply(string text) 
        {
            MarkdownFilter filter = new MarkdownFilter();
            return filter.Process(text);
        }
	}
}
