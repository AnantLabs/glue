using System;
using System.Collections;
using System.Text;
using Glue.Lib.Text.Template;

namespace Glue.Lib.Text.Template.AST
{
	public class ReferenceExpression : Expression
	{
        public string Name;
        public ElementList Arguments = new ElementList();
        public ArrayList Resolved = null;

        public ReferenceExpression(Token t) : base(t) { }
        public ReferenceExpression(Token t, string name) : base(t) 
        { 
            Name = name; 
        }
    }
}
