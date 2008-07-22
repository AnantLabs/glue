using System;
using System.Collections;
using System.Text;
using Glue.Lib.Text.Template;

namespace Glue.Lib.Text.Template.AST
{
    public class IfStatement : Statement
    {
        public readonly ElementList True = new ElementList();
        public readonly ElementList False = new ElementList();
        public Expression Test;

        public IfStatement(Token t) : base(t) {}
    }
}