using System;
using System.Collections;
using System.Text;
using Glue.Lib.Text.Template;

namespace Glue.Lib.Text.Template.AST
{
    public abstract class Statement : Element
    {
        public Statement(Token t) : base(t) {}
    }
}