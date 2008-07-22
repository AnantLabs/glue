using System;
using System.Collections;
using System.Text;
using Glue.Lib.Text.Template;

namespace Glue.Lib.Text.Template.AST
{
	public class PrimitiveExpression : Expression
	{
        public object Value;

        public PrimitiveExpression(Token t) : base(t) { }
        public PrimitiveExpression(Token t, object val) : base(t) 
        { 
            Value = val;
        }
    }
}
