using System;
using System.Collections;
using System.Text;
using Glue.Lib.Text.Template;

namespace Glue.Lib.Text.Template.AST
{
    public class NamedArgumentExpression : Expression
    {
        public string Name;
        public Expression Expression;

        public NamedArgumentExpression(Token t) : this(t, null, null) {}
        public NamedArgumentExpression(Token t, string name) : this(t, name, null) {}
        public NamedArgumentExpression(Token t, string name, Expression expr) : base(t) 
        {
            Name = name; 
            Expression = expr;
        }
    }
}
