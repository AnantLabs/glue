using System;
using System.Collections;
using Glue.Lib.Text.Template;

namespace Glue.Lib.Text.Template.AST
{
    public class MethodDefinition : Statement
    {
        public string Name;
        public readonly ElementList Parameters = new ElementList();
        public readonly ElementList Inner = new ElementList();

        public MethodDefinition(Token t) : base(t) {}
        public MethodDefinition(Token t, string name) : base(t) 
        {
            Name = name;
        }
    }
}