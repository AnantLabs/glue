using System;
using System.Collections;
using System.Text;
using Glue.Lib.Text.Template;

namespace Glue.Lib.Text.Template.AST
{
    public class ForStatement : Statement
    {
        public readonly ElementList Inner = new ElementList();
        public readonly ElementList Alt = new ElementList();
        public readonly ElementList Sep = new ElementList();
        public Expression Container;
        public string Iterator;
        public string Counter;

        public ForStatement(Token t) : base(t) {}
    }
}