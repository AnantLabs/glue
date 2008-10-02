using System;
using System.Collections;
using System.Text;
using Glue.Lib.Text.Template;

namespace Glue.Lib.Text.Template.AST
{
	public abstract class Expression : Element
	{
		public Expression(Token t) : base(t) { }
	}
}
