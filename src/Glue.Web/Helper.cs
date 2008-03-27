using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using Glue.Lib;
using Glue.Lib.Text;

namespace Glue.Web
{
    /// <summary>
	/// Summary description for Helpers
	/// </summary>
	public class Helper
	{

        #region Shortcuts

        /// <summary>
        /// Creates a Bag
        /// </summary>
        public static IDictionary B(params object[] keyvals)
        {
            return Bag(keyvals);
        }

        /// <summary>
        /// Translate, hooks into I18 translation stuff
        /// </summary>
        public static string T(string s, params object[] args)
        {
            //return Localization.Current.T(s, args);
            return string.Format(s, args);
        }

        /// <summary>
        /// HtmlEncode
        /// </summary>
        public static string H(string s)
        {
            return HtmlEncode(s);
        }

        /// <summary>
        /// UrlEncode
        /// </summary>
        public static string U(string s)
        {
            return UrlEncode(s);
        }

        /// <summary>
        /// ExpandLink
        /// </summary>
        public static string L(string s)
        {
            return Link(s);
        }

        #endregion

        #region Collection and Dictionary helpers

        public static IDictionary Bag(params object[] keyvals)
        {
            Hashtable dict = new Hashtable();
            int i = 0;
            while (i < keyvals.Length - 1)
            {
                dict[keyvals[i]] = keyvals[i+1];
                i += 2;
            }
            return dict;
        }

        public static IDictionary OrderedBag(params object[] keyvals)
        {
            Glue.Lib.OrderedDictionary dict = new Glue.Lib.OrderedDictionary();
            int i = 0;
            while (i < keyvals.Length - 1)
            {
                dict.Add(keyvals[i], keyvals[i+1]);
                i += 2;
            }
            return dict;
        }

        public static IList List(params object[] items)
        {
            return items == null ? new object[0] : items;
        }

        #endregion
        
        #region URL, HTML and Path utility stuff

        /// <summary>
        /// Returns the n'th segment of a path part of a URL. 
        /// Assumes forward slashed (/) in the path.
        /// </summary>
        public static string GetPathSegment(string path, int index) 
        {
            if (path == null || path.Length == 0)
                return null;
            int i = 0;
            if (path[0] == '/')
                i++;
            int s = i;
            while (i < path.Length) 
            {
                if (path[i] == '/' || path[i] == '?' || path[i] == '#')
                    if (--index >= 0)
                        s = i + 1;
                    else
                        return path.Substring(s, i - s);
                i++;
            }
            if (index == 0 && i > s)
                return path.Substring(s, i - s);
            else
                return null;
        }

        /// <summary>
        /// Returns path from the n'th segment on. 
        /// </summary>
        public static string GetPathFromSegment(string path, int index) 
        {
            if (path == null || path.Length == 0)
                return null;
            int i = 0;
            if (path[0] == '/')
                i++;
            int s = i;
            while (i < path.Length) 
            {
                if (path[i] == '/' || path[i] == '?' || path[i] == '#')
                    if (--index >= 0)
                        s = i + 1;
                    else
                        return path.Substring(s);
                i++;
            }
            if (index == 0 && i > s)
                return path.Substring(s);
            else
                return null;
        }

        public static string HtmlAttributeEncode(string s)
        {
            return HttpUtility.HtmlAttributeEncode(s);
        }

        public static string HtmlEncode(string s)
        {
            return HttpUtility.HtmlEncode(s);
        }

        public static string HtmlDecode(string s)
        {
            return HttpUtility.HtmlDecode(s);
        }

        public static string UrlEncode(string s)
        {
            return HttpUtility.UrlEncode(s);
        }

        public static string UrlDecode(string s)
        {
            return HttpUtility.UrlDecode(s);
        }

        public static string UrlGetPath(string url)
        {
            int i = url.IndexOf('?');
            return i >= 0 ? url.Substring(0, i) : url;
        }

        public static NameValueCollection UrlGetQueryString(string url)
        {
            NameValueCollection qs = new NameValueCollection();
            int i = url.IndexOf('?');
            if (i >= 0)
                Glue.Lib.Servers.HttpProtocol.FillValuesFromString(qs, url.Substring(i + 1));
            return qs;
        }

        protected static string UrlCreate(string path, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                string url = path + "?";
                for (int i = 0; i < args.Length; i +=2)
                {
                    url += args[i] + "=" + HttpUtility.UrlEncode(Convert.ToString(args[i+1])) + "&";
                }
                return url.Substring(0, url.Length - 1);
            }
            return path;
        }

        public static string UrlCreate(string path, NameValueCollection queryString)
        {
            string qs = QueryStringCreate(queryString);
            if (qs.Length > 0)
                return string.Concat(path, "?", qs);
            else
                return path;
        }
        
        public static string UrlMerge(string url, params object[] modifiers)
        {
            return UrlMerge(UrlGetPath(url), UrlGetQueryString(url), modifiers);
        }

        public static string UrlMerge(string path, NameValueCollection originalQueryString, params object[] modifiers)
        {
            return UrlCreate(path, QueryStringMerge(originalQueryString, modifiers));
        }
        
        public static string UrlSign(string url)
        {
            if (url == null)
                url = string.Empty;
            url += url.IndexOf('?') < 0 ? "?" : "&";
            return url + "sgndx=" + url.GetHashCode();
        }

        public static bool UrlHasValidSignature(string url)
        {
            if (url == null)
                url = string.Empty;
            int i = url.IndexOf("sgndx=");
            if (i < 0)
                return false;
            string code = url.Substring(i + 6);
            url = url.Substring(0, i);
            return (url.GetHashCode().ToString() == code);
        }

        public static string QueryStringCreate(NameValueCollection queryString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string key in queryString)
            {
                foreach (string val in queryString.GetValues(key))
                {
                    sb.Append(key).Append('=').Append(System.Web.HttpUtility.UrlEncode(val)).Append('&');
                }
            }
            if (sb.Length > 0)
                sb.Length = sb.Length - 1;
            return sb.ToString();
        }

        public static NameValueCollection QueryStringMerge(NameValueCollection originalQueryString, params object[] modifiers)
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString.Add(originalQueryString);
            if (modifiers != null)
                for (int i = 0; i < modifiers.Length;)
                {
                    queryString.Remove((string)modifiers[i]);
                    if (i + 1 < modifiers.Length && modifiers[i+1] != null)
                    {
                        queryString.Add((string)modifiers[i], Convert.ToString(modifiers[i+1]));
                        i++;
                    }
                    i++;
                }
            return queryString;
        }

        /// <summary>
        /// Expands a URL starting with "~/" to include the web 
        /// application's root URL.
        /// </summary>
        public static string Link(string url)
        {
            if (url == null || url.Length == 0)
                return url;
            if (url[0] != '~')
                return url;
            if (url == "~")
                return App.Request.Root + "/";
            return App.Request.Root + url.Substring(1);
        }
        
        public static string SafeFileName(string text)
        {
            return 
                StringHelper.RemoveSpans(
                StringHelper.StripNonWordChars(
                StringHelper.StripDiacritics("" + text, '-'), '-'), '-').
                Trim('-').
                ToLower();
        }

        #endregion

        #region Diagnostics

        internal static string FormatHtmlError(Exception exception)
        {
            // Filter out undescriptive target invokation calls
            while (exception is System.Reflection.TargetInvocationException && exception.InnerException != null)
            {
                exception = exception.InnerException;
            }
            StringWriter s = new StringWriter();
            s.WriteLine(@"<html>
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
"
                );
            s.WriteLine("<h1>{0}</h1>", exception.Message);
            
            //s.WriteLine("<p><b>Source:</b> {0}</p>", exception.Source);
            //s.WriteLine("<p><b>Type:</b> {0}</p>", exception.GetType());

            string simplified = exception.ToString();
            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(simplified, "^[ ]+at[ ]+.* in .*:line [1-9][0-9]*$", System.Text.RegularExpressions.RegexOptions.Multiline);
            bool showStackTrace = true;
            string sourceFile = null;
            int sourceLine = 0;
            if (match.Success)
            {
                simplified = simplified.Substring(0, match.Index + match.Length);
                simplified = System.Text.RegularExpressions.Regex.Replace(simplified, "^[ ]+at[ ]+.* in .*:line 0\r\n", "", System.Text.RegularExpressions.RegexOptions.Multiline);
                simplified = System.Text.RegularExpressions.Regex.Replace(simplified, "^[ ]+at[ ]+System\\..*\r\n", "", System.Text.RegularExpressions.RegexOptions.Multiline);
                match = System.Text.RegularExpressions.Regex.Match(simplified, "^[ ]+at[ ]+.* in (.*):line ([1-9][0-9]*)", System.Text.RegularExpressions.RegexOptions.Multiline);
                if (match.Success)
                {
                    sourceFile = match.Groups[1].Value;
                    sourceLine = int.Parse(match.Groups[2].Value);
                    simplified = simplified.Substring(0, match.Groups[1].Index - 4) + "\r\n  " + simplified.Substring(match.Groups[1].Index - 4);
                }
            }
            s.WriteLine("<pre>{0}</pre>", simplified);
            
            if (exception is Glue.Lib.Compilation.CompilationException)
            {
                Glue.Lib.Compilation.CompilationException e = exception as Glue.Lib.Compilation.CompilationException;
                s.WriteLine("<h2>Compiler Results</h2>");
                s.WriteLine("<div><b>{0}</b><br/><br/></div>", e.SourceFile);
                s.WriteLine("<div><b>Errors</b>:</div>");
                s.WriteLine("<code><pre>{0}</pre></code>", e.ErrorMessage);
                if (e.Results != null)
                {
                    if (e.Results.Errors.Count > 0)
                    {
                        s.WriteLine("<div><b>Source</b>:</div>");
                        foreach (System.CodeDom.Compiler.CompilerError detail in e.Results.Errors)
                            s.WriteLine("<code><pre>{0}</pre></code>", ReadSampleLinesFromSource(detail.FileName, detail.Line));
                    }
                    s.WriteLine("<div><b>Output</b>:</div>");
                    s.WriteLine("<code><pre>");
                    foreach (string line in e.Results.Output)
                        s.WriteLine(line);
                    s.WriteLine("</pre></code>");
                }
                sourceFile = null;
                showStackTrace = false;
            } 

            if (sourceFile != null)
            {
                s.WriteLine("<h2>Source</h2>");
                s.WriteLine("<code><pre>{0}</pre></code>", ReadSampleLinesFromSource(sourceFile, sourceLine));
            }
            
            // Simplified stack trace, skip all internal System. calls.
            if (showStackTrace)
            {
                s.WriteLine("<h2>Stack Trace</h2>\r\n");
                string trace = exception.StackTrace;
                if (trace != null)
                    trace = System.Text.RegularExpressions.Regex.Replace(trace, "^[ ]+at[ ]+(?!System).*$", "<em>$0</em>", System.Text.RegularExpressions.RegexOptions.Multiline);
                s.WriteLine("<code><pre>{0}</pre></code>", trace);
                s.WriteLine(@"
    </body>
    </html>"
                    );
            }
            return s.ToString();
        }

        static string ReadSampleLinesFromSource(string fileName, int lineNumber)
        {
            try 
            {
                string sample = "";
                using (TextReader reader = File.OpenText(fileName))
                {
                    int num = 0;
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        num++;
                        if (num == lineNumber)
                        {
                            sample += string.Format("<i>{0,4}</i>: <em>{1}</em>\r\n", num, HttpUtility.HtmlEncode(line));
                        }
                        else if (num >= lineNumber - 2 && num <= lineNumber + 2)
                        {
                            sample += string.Format("<i>{0,4}</i>: {1}\r\n", num, HttpUtility.HtmlEncode(line));
                        }
                        if (num > lineNumber + 2)
                        {
                            break;
                        }
                        line = reader.ReadLine();
                    }
                }
                return sample;
            }
            catch
            {
            }
            return "";
        }

        public static string FormatHtmlHelp(Type controllerType)
        {
            StringWriter s = new StringWriter();
            s.WriteLine(@"<html>
<head>
<style>
body,p,td,th { font-family:verdana,arial,helvetica;font-size:70%; }
pre          { font-family: lucida console; font-weight: normal; margin-top: 0px; margin-left: 20px; background-color: #f3f0e8; font-size:100%; }
code         { font-weight: 400; font-family: ""courier new""; } 
em           { color: red; font-style: normal; }
h1           { font-weight: normal; font-size: 165%; }
h2           { font-family:arial; font-weight: bold; font-size: 124%; }
dt           { font-weight: bold; width: 200px; float: left; }
</style>
</head>
<body>
"
                );
            s.WriteLine("<h1>{0}</h1>", controllerType);
            s.WriteLine("<h2>Fields</h2>");
            s.WriteLine("<div>");
            s.WriteLine("<dl>");
            foreach (System.Reflection.MemberInfo info in controllerType.GetMembers(
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public))
            {
                if (info.MemberType == System.Reflection.MemberTypes.Property ||
                    info.MemberType == System.Reflection.MemberTypes.Field)
                {
                    s.Write("<dt>");
                    s.Write(info.Name);
                    s.Write("</dt><dd>");
                    s.Write("doc");
                    s.WriteLine("</dd>");
                }
            }
            s.WriteLine("</dl>");
            s.WriteLine("</div>");
            s.WriteLine("</body>");
            s.WriteLine("</html>");
            s.WriteLine("<h2>Actions</h2>");
            s.WriteLine("<div>");
            s.WriteLine("<dl>");
            foreach (System.Reflection.MemberInfo info in controllerType.GetMembers(
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public))
            {
                if (info.MemberType == System.Reflection.MemberTypes.Method)
                {
                    System.Reflection.MethodInfo m = info as System.Reflection.MethodInfo;
                    if (!m.IsSpecialName)
                    {
                        s.Write("<dt>");
                        s.Write(info.Name);
                        s.Write("</dt><dd>");
                        s.Write("doc");
                        s.WriteLine("</dd>");
                    }
                }
            }
            s.WriteLine("</dl>");
            s.WriteLine("</div>");
            s.WriteLine("</body>");
            s.WriteLine("</html>");
            return s.ToString();
        }

        #endregion

        #region HtmlBuilder and Controls

        protected static HtmlBuilder Html(string s)
        {
            return new HtmlBuilder(s);
        }

        /// <summary>
        /// Hyperlink
        /// id:       id
        /// href:     address
        /// text:     inner text
        /// confirm:  confirmation text
        /// nofollow: true/false
        /// target:   target window
        /// </summary>
        public static string Hyperlink(IDictionary parms)
        {
            string href = NullConvert.ToString(parms["href"]);
            if (parms["confirm"] != null)
                href = "javascript:if(confirm('" + Convert.ToString(parms["confirm"]).Replace("'","\\'") + "')) window.location='" + href + "';";
            else if (NullConvert.ToBoolean(parms["nofollow"], false))
                href = "javascript:window.location='" + href + "';";
            return Html("<a").
                Attr("id",      parms).
                Attr("href",    href).
                Attr("class",   parms).
                Attr("style",   parms).
                Append(">").
                Append(parms["text"]).
                Append("</a>");
        }

        public static string JavaScriptLink(string url)
        {
            if (App.Current.Debug)
                return "<script type=\"text/javascript\" src=\"" + url + "\"></script>";
            else
                return "<script type=\"text/javascript\" src=\"" + url + "?" + DateTime.Now.ToString("yyyyMMddHH") + "\"></script>";
        }

        public static string StyleSheetLink(string url)
        {
            if (App.Current.Debug)
                return "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + url + "?" + DateTime.Now.ToString("yyyyMMddHH") + "\" />";
            else
                return "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + url + "\" />";
        } 
        
        public static string StyleSheetLink(string url, string media)
        {
            if (App.Current.Debug)
                return "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + url + "?" + DateTime.Now.ToString("yyyyMMddHH") + "\" media=\"" + media + "\" />";
            else
                return "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + url + "\" media=\"" + media + "\" />";
        }

        /// <summary>
        /// Button
        /// id:     
        /// text:   
        /// </summary>
        public static string Button(IDictionary parms)
        {
            return Html("<input").
                Attr("type",    "submit").
                Attr("name",    parms["id"]).
                Attr("id",      parms).
                Attr("style",   parms).
                Attr("class",   parms, "button").
                AttrIfTrue("disabled", parms).
                Attr("value",   parms["text"]).
                Append(" />");
        }

        /// <summary>
        /// TextBox
        /// id:         
        /// value:      
        /// required:   
        /// </summary>
        public static string TextBox(IDictionary parms)
        {
            return Html("<input").
                Attr("type",     parms, "text").
                Attr("name",     parms["id"]).
                Attr("id",       parms).
                Attr("class",    parms, "textbox").
                Attr("style",    parms).
                Attr("value",    parms).
                Attr("required", parms).
                Attr("readonly", parms).
                AttrIfTrue("disabled", parms).
                Attr("onchange", parms).
                Append(" />");
        }

        /// <summary>
        /// PasswordBox
        /// id:         
        /// value:      
        /// required:   
        /// </summary>
        public static string PasswordBox(IDictionary parms)
        {
            return Html("<input").
                Attr("type",    parms, "password").
                Attr("name",    parms["id"]).
                Attr("id",      parms).
                Attr("class",   parms, "textbox").
                Attr("style",   parms).
                Attr("value",   parms).
                Attr("required",parms).
                Append(" />");
        }

        public static string HiddenBox(IDictionary parms)
        {
            return Html("<input").
                Attr("type",    "hidden").
                Attr("id",      parms).
                Attr("name",    parms["id"]).
                Attr("value",   parms).
                Append(" />");
        }
        
        public static string TextArea(IDictionary parms)
        {
            return Html("<textarea").
                Attr("name",    parms["id"]).
                Attr("id",      parms).
                Attr("class",   parms, "textarea").
                Attr("style",   parms).
                Attr("cols",    parms).
                Attr("rows",    parms).
                AttrIfTrue("disabled",parms).
                Attr("required",parms).
                Append(">").
                Append(parms["value"]).
                Append("</textarea>");
        }

        /// <summary>
        /// DateBox
        /// id:         
        /// value:       
        /// required:   
        /// pattern:   parse pattern
        /// format:    format pattern
        /// </summary>
        public static string DateBox(IDictionary parms)
        {
            parms["pattern"] = NullConvert.Coalesce(parms["pattern"], App.Current.DateFormat);
            parms["format"]  = NullConvert.Coalesce(parms["format"], App.Current.DateFormat);
            DateTime dt = DateTime.MinValue;
            try { dt = (DateTime)parms["value"]; } 
            catch {}
            parms["value"] = dt == DateTime.MinValue ? "" : dt.ToString((string)parms["format"]);
            return Html("<input").
                Attr("type",    "text").
                Attr("name",    parms["id"] + ".value").
                Attr("id",      parms["id"] + ".value").
                Attr("class",   parms, "datebox").
                Attr("style",   parms).
                Attr("value",   parms, "value").
                Attr("readonly", parms).
                AttrIfTrue("disabled", parms).
                Attr("pattern", parms).
                Attr("required",parms).
                Append(" />").
                If(parms["required"]).
                Append("<span class=\"required\">required (").Append(parms["pattern"]).Append(")</span>").
                End().
                IfNot(parms["required"]).
                Append("<span style='white-space:nowrap;'>(").Append(parms["pattern"]).Append(")</span>").
                End().
                Append("<input").
                Attr("type", "hidden").
                Attr("id", parms["id"] + ".pattern").
                Attr("name", parms["id"] + ".pattern").
                Attr("value", parms["pattern"]).
                Append(" />");
        }

        /// <summary>
        /// DateBox
        /// id:         
        /// value:       
        /// required:   
        /// pattern:   
        /// </summary>
        public static string DateTimeBox(IDictionary parms)
        {
            parms["pattern"] = NullConvert.Coalesce(parms["pattern"], App.Current.DateTimeFormat);
            parms["format"] = NullConvert.Coalesce(parms["format"], App.Current.DateTimeFormat);
            parms["class"] = NullConvert.Coalesce(parms["class"], "datetimebox");
            return DateBox(parms);
        }

        /// <summary>
        /// DropDownBox
        /// id:         
        /// value:      
        /// list:       
        /// textfield:   
        /// valuefield:  
        /// required    
        /// nulltext:
        /// nullvalue:
        /// defaultvalue:
        /// </summary>
        public static string DropDownBox(IDictionary parms)
        {
            HtmlBuilder html = Html("<select").
                Attr("class", parms, "dropdownbox").
                Attr("style", parms).
                Attr("name", parms["id"]).
                AttrIfTrue("disabled", parms).
                Attr("id", parms).
                Attr("onchange", parms).
                Attr("required", parms).
                Append(">");
            if (parms["nulltext"] != null)
            {
                html.Append("<option").
                    Attr("value", parms["nullvalue"]).
                    Attr("noselect", "noselect").
                    Append(">").
                    Append(parms["nulltext"]).
                    Append("</option>");
            }
            parms["value"] = NullConvert.Coalesce(parms["value"], parms["defaultvalue"]); 
            foreach (HtmlListItem item in new HtmlListEnum(parms))
            {
                html.Append("<option").
                    Attr("value", item.Value).
                    If(object.Equals(item.Value, parms["value"]) || String.Compare(Convert.ToString(item.Value), Convert.ToString(parms["value"]), true) == 0).
                    Attr("selected", "selected").
                    End().
                    Append(">").
                    Append(item.Text).
                    Append("</option>");
            }
            return html.Append("</select>").
                If(parms["required"]).
                Append("<span class=\"required\">verplicht</span>").
                End();
        }

        /// <summary>
        /// RadioBox
        /// id:         
        /// value:     
        /// text:         
        /// checked:   
        /// </summary>
        public static string RadioButton(IDictionary parms)
        {
            return Html("<input").
                Attr("type",    "radio").
                Attr("name",    parms["id"]).
                Attr("id",      parms).
                Attr("style",   parms).
                Attr("class",   parms, "radio").
                Attr("value",   parms, "value").
                If(NullConvert.ToBoolean(parms["checked"], false)).
                Attr("checked", "checked").
                End().
                Append(">").
                Append(parms["text"]).
                Append("</input>");
        }

        /// <summary>
        /// RadioButtonList
        /// id:         
        /// value:      
        /// list:       
        /// textfield:   
        /// valuefield:  
        /// required:    
        /// </summary>
        public static string RadioButtonList(IDictionary parms)
        {
            HtmlBuilder html = Html("<div ").
                Attr("id", parms).
                Attr("style", parms).
                Attr("class", parms, "radiobuttonlist").
                Attr("type", "radiobuttonlist").
                Attr("required", parms).
                Append(">");
            foreach (HtmlListItem item in new HtmlListEnum(parms["list"], (string)parms["itemtext"], (string)parms["itemvalue"]))
            {
                html.Append("<input").
                    Attr("type",    "radio").
                    Attr("id",      parms).
                    Attr("name",    parms["id"]).
                    Attr("value",   item.Value).
                    If(Convert.ToString(item.Value) == Convert.ToString(parms["value"])).
                    Attr("checked", "checked").
                    End().
                    Append(">").
                    Append(item.Text).
                    Append("</input>").
                    Append(parms["separator"]);
            }
            return html.Append("</div>").
                If(parms["required"]).
                Append("<span class=\"required\">verplicht</span>").
                End();
        }

        /// <summary>
        /// CheckBox
        /// id:         
        /// value  
        /// text         
        /// yes_value   (default: true)
        /// no_value    (default: false)
        /// </summary>
        public static string CheckBox(IDictionary parms)
        {
            parms["yes_value"] = NullConvert.Coalesce(parms["yes_value"], 1);
            parms["no_value"] = NullConvert.Coalesce(parms["no_value"], 0);

			string onclick = "if (this.checked) " +
							 "  document.getElementById('" + parms["id"] + "').value ='" + parms["yes_value"] + "';" +
							 " else " +
							 "  document.getElementById('" + parms["id"] + "').value ='" + parms["no_value"] + "';";
            bool _checked = IsEqualValue(parms["value"], parms["yes_value"]);
            object value = _checked ? parms["yes_value"] : parms["no_value"];
            return Html("<input").
                Attr("type",    "checkbox").
                Attr("name",    parms["id"] + "_cb").
                Attr("id",      parms["id"] + "_cb").
                Attr("style",   parms).
                AttrIfTrue("disabled",   parms).
                Attr("class",   parms, "checkbox").
                Attr("value",   parms["yes_value"]).
				Attr("onclick", onclick).
                If(_checked).
                Attr("checked", "checked").
				End().
                Append(">").
                Append(parms["text"]).
                Append("</input>").
                Append("<input").
                Attr("name", parms["id"]).
                Attr("id", parms["id"]).
                Attr("type", "hidden").
                Attr("onchange", parms).
                Attr("value", value).
                Append("/>");
        }

        /// <summary>
        /// CheckBoxList
        /// id:         
        /// value:      
        /// list:       
        /// textfield:   
        /// valuefield:  
        /// required:    
        /// </summary>
        public static string CheckBoxList(IDictionary parms)
        {
            HtmlBuilder html = Html("<div ").
                Attr("id", parms).
                Attr("style", parms).
                Attr("class", parms, "checkboxlist").
                Attr("type", "checkboxlist").
                Append(">");
            foreach (HtmlListItem item in new HtmlListEnum(parms["list"], (string)parms["itemtext"], (string)parms["itemvalue"]))
            {
                html.Append("<input").
                    Attr("type", "checkbox").
                    Attr("id", parms).
                    Attr("name", parms["id"]).
                    Attr("value", item.Value).
                    If(Convert.ToString(item.Value) == Convert.ToString(parms["value"])).
                    Attr("checked", "checked").
                    End().
                    Append(">").
                    Append(item.Text).
                    Append("</input>");
            }
            html.Append("</div>");
            return html;
        }

        public static bool IsEqualValue(object a, object b)
        {
            if (a is Boolean)
                a = Convert.ToInt32(a);
            if (b is Boolean)
                b = Convert.ToInt32(b);
            return a == b || object.Equals(a, b) || string.Compare(Convert.ToString(a), Convert.ToString(b), true) == 0;
        }

        #endregion
    }
}
