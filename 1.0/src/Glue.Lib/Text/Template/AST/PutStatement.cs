using System;
using System.Collections;
using System.Text;
using Glue.Lib.Text.Template;

namespace Glue.Lib.Text.Template.AST
{
    public class PutStatement : Statement
    {
        public Expression Expression;

        public PutStatement(Token t) : base(t) {}
    }
}