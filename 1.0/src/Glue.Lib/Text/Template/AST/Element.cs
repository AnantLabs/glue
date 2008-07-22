using System;
using System.Collections;
using System.Text;
using Glue.Lib.Text.Template;

namespace Glue.Lib.Text.Template.AST
{
    public abstract class Element
    {
        public readonly int Line;
        public readonly int Col;
        public readonly string File;
        public string UserData;

        public Element(Token t) : this(t.line, t.col, null) {}
        public Element(int line, int col) : this(line, col, null) {}
        public Element(int line, int col, string file)
        {
            Line = line;
            Col = col;
            File = file;
        }
   }
}