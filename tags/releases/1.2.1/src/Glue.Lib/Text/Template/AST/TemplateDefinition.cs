using System;
using System.Collections;
using System.Text;
using Glue.Lib.Text.Template;

namespace Glue.Lib.Text.Template.AST
{
    public class TemplateDefinition : Statement
    {
        public readonly ElementList Header = new ElementList();
        public readonly ElementList Footer = new ElementList();
        public string Name;

        public TemplateDefinition(Token t) : this(t, null) {}
        public TemplateDefinition(Token t, string name) : base(t) 
        { 
            Name = name; 
        }
    }
}