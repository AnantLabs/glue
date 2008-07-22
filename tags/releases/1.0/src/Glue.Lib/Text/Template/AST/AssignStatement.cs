using System;
using System.Collections;
using System.Text;
using Glue.Lib.Text.Template;

namespace Glue.Lib.Text.Template.AST
{
    public class AssignStatement : Statement
    {
        public string Name;
        public Expression Expression;

        public AssignStatement(Token t) : this(t, null, null) {}
        public AssignStatement(Token t, string name) : this(t, name, null) {}
        public AssignStatement(Token t, string name, Expression expr) : base(t) 
        {
            Name = name; 
            Expression = expr;
        }
    }
}
