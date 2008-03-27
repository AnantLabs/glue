using System;
using System.IO;
using System.Collections.Specialized;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Text.RegularExpressions;

namespace Glue.Lib.Compilation
{
	/// <summary>
	/// Summary description for AspTemplateCompiler.
	/// </summary>
	public class AspTemplateCompiler : TemplateCompiler
	{
        // ASP like patterns;
        protected static readonly Regex DirectiveRegex = new Regex(@"<%@\s*(?<directive>\w*)(\s*(?<attrname>\w+(?=\W))(\s*=\s*""(?<attrval>[^""]*)""))*\s*?%>", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
        
        protected override void Parse(string path)
        {
            string content = this.GetFileContents(path);
            
            // Walk subsequent <% .. %> fragments. A tad primitive, 
            // but highly effective nonetheless.
            int prev = 0;
            int curr = content.IndexOf("<%");
            while (curr >= 0)
            {
                int next = content.IndexOf("%>", curr + 2);
                if (next < 0)
                    throw new Exception("Parser error");

                // String literal
                if (prev < curr)
                {
                    CodeExpressionStatement stmt = new CodeExpressionStatement(
                        new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("writer"),
                        "Write",
                        new CodePrimitiveExpression(content.Substring(prev, curr - prev))
                        ));
                    stmt.LinePragma = new CodeLinePragma(path, GetLineCount(content, 0, prev) + 1);
                    _statements.Add(stmt);
                }
                
                if (content[curr + 2] == '@') 
                {
                    // Parse directive
                    Match m = DirectiveRegex.Match(content, curr, next - curr + 2);
                    if (!m.Success)
                        throw new Exception("Parser error");
                    string directive = m.Groups["directive"].Value;
                    StringDictionary attributes = new StringDictionary();
                    CaptureCollection names = m.Groups["attrname"].Captures;
                    CaptureCollection vals  = m.Groups["attrval"].Captures;
                    for (int i = 0; i < names.Count; i++)
                        attributes[names[i].Value] = vals[i].Value;
                    ParseDirective(directive.ToLower(), attributes);
                }
                else if (content[curr + 2] == '-' && content[curr + 3] == '-') // Comment
                {
                    // Ignore server side comment
                    next = content.IndexOf("--%>", curr + 4);
                    if (next < 0)
                        throw new Exception("Parser error");
                    next += 2;
                }
                else if (content[curr + 2] == '#')
                { 
                    // Member functions
                    _members.Add(
                        new CodeSnippetTypeMember(
                        content.Substring(curr + 3, next - curr - 3)
                        ));
                }
                else if (content[curr + 2] == '=') 
                {
                    // Expression <%= expr %>
                    CodeExpressionStatement stmt = new CodeExpressionStatement(
                        new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("writer"),
                        "Write",
                        new CodeSnippetExpression(content.Substring(curr + 3, next - curr - 3))
                        ));
                    stmt.LinePragma = new CodeLinePragma(path, GetLineCount(content, 0, curr) + 1);
                    _statements.Add(stmt);
                }
                else 
                {
                    // Statement <% statements; %>
                    CodeSnippetStatement stmt = new CodeSnippetStatement(content.Substring(curr + 2, next - curr - 2));
                    stmt.LinePragma = new CodeLinePragma(path, GetLineCount(content, 0, curr) + 1);
                    _statements.Add(stmt);
                }
                prev = next + 2;
                curr = content.IndexOf("<%", prev);
            }

            if (prev < content.Length)
            {
                // Append remaining string literal
                CodeExpressionStatement stmt = new CodeExpressionStatement(
                    new CodeMethodInvokeExpression(
                    new CodeVariableReferenceExpression("writer"),
                    "Write",
                    new CodePrimitiveExpression(content.Substring(prev))
                    ));
                stmt.LinePragma = new CodeLinePragma(path, GetLineCount(content, 0, prev) + 1);
                _statements.Add(stmt);
            }
        } 

        protected virtual void ParseDirective(string directive, StringDictionary attributes)
        {
            switch (directive)
            {
                case "include":
                {
                    string path = "" + attributes["file"];
                    path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                    path = Path.Combine(Path.GetDirectoryName(FileName), path);
                    Parse(MapPath(path));
                    break;
                }
                case "assembly":
                {
                    string path = ResolveAssemblyPath(attributes["name"]);
                    if (path != null && path.Length > 0)
                        _unit.ReferencedAssemblies.Add(path);
                    break;
                }
                case "import":
                    _namespace.Imports.Add(new CodeNamespaceImport(attributes["namespace"]));
                    break;
                default:
                    Log.Warn("Unknown directive: '{0}'", directive);
                    break;
            }
        }
	}
}
