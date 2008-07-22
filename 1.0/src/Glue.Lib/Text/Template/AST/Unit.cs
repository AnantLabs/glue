using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using Glue.Lib.Text.Template;

namespace Glue.Lib.Text.Template.AST
{
    public class Unit : Statement
    {
        public string Namespace = "Generated";
        public string Name = "GeneratedUnit";
        public string Extends;
        public StringCollection Imports = new StringCollection();
        public readonly ElementList Inner = new ElementList();

        public Unit(Token t) : base(t) {}
    }
}