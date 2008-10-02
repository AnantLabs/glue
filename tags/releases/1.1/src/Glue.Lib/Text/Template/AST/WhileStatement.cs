using System;
using System.Collections;
using System.Text;
using Glue.Lib.Text.Template;

namespace Glue.Lib.Text.Template.AST
{
    public class WhileStatement : IfStatement
    {
        public WhileStatement(Token t) : base(t) {}
    }
}