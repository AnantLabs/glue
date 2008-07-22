using System;
using System.Collections;
using System.Text;
using Glue.Lib.Text.Template;

namespace Glue.Lib.Text.Template.AST
{
    public class ParameterDefinition : Element
    {
        public string Name;
        public Expression Default;

        public ParameterDefinition(Token t) : this(t, null, null) {}
        public ParameterDefinition(Token t, string name) : this(t, name, null) {}
        public ParameterDefinition(Token t, string name, Expression def) : base(t) 
        {
            Name = name; 
            Default = def;
        }
    }
}
