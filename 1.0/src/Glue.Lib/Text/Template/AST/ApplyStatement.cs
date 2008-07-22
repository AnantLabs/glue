using System;
using System.Collections;
using System.Text;
using Glue.Lib.Text.Template;

namespace Glue.Lib.Text.Template.AST
{
    public class ApplyStatement : Statement
    {
        public readonly ElementList Inner = new ElementList();
        public string Name;

        public ApplyStatement(Token t) : base(t) {}
        public ApplyStatement(Token t, string name) : base(t) { Name = name; }
    }
}