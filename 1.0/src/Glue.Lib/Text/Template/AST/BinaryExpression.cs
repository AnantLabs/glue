using System;
using System.Collections;
using System.Text;
using Glue.Lib.Text.Template;

namespace Glue.Lib.Text.Template.AST
{
	public class BinaryExpression : Expression
	{
        public string Operator;
        public Expression Left;
        public Expression Right;

        public BinaryExpression(Token t) : base(t) {}
        public BinaryExpression(Token t, Expression left, string op, Expression right) : base(t) 
        {
            Left = left;
            Operator = op;
            Right = right;
        }
    }
}
