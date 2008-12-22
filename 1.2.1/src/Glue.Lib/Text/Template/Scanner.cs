
using System;
using System.IO;
using System.Collections; 

namespace Glue.Lib.Text.Template {

public class Token {
	public int kind;    // token kind
	public int pos;     // token position in the source text (starting at 0)
	public int col;     // token column (starting at 0)
	public int line;    // token line (starting at 1)
	public string val;  // token value
	public Token next;  // ML 2005-03-11 Tokens are kept in linked list
}

public class Buffer {
    public const char EOF = (char)256;
    string buf;         // buffer
    int pos;            // current position in buffer

    public Buffer(string text) {
        buf = text;
        pos = 0;
    }
    
    public int Read() {
        if (pos < buf.Length)
            return buf[pos++];
        else
            return EOF;
    }

    public int Peek() {
        if (pos < buf.Length) 
            return buf[pos];
        else 
            return EOF;
    }

    public int Pos {
        get { 
            return pos; 
        }
        set { 
            if (value < 0) 
                value = 0; 
            else if (value > buf.Length) 
                value = buf.Length;
            pos = value;
        }
    }
}

public class Scanner {
	const char EOL = '\n';
	const int eofSym = 0; /* pdt */
	const int charSetSize = 256;
	const int maxT = 45;
	const int noSym = 45;
	short[] start = {
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0, 16,  1,  0,  0, 24, 11,  2,  8, 10, 22, 20,  9, 21, 25, 23,
	  5,  5,  5,  5,  5,  5,  5,  5,  5,  5,  0,  0, 27, 26, 28,  0,
	  0,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,
	  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  0,  0,  0,  0,  4,
	  0,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,
	  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  0, 13,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  -1};


	public Buffer buffer; // scanner buffer
	
	protected Token t;          // current token
	protected char ch;          // current input character
	protected int pos;          // column number of current character
	protected int line;         // line number of current character
	protected int lineStart;    // start position of current line
	protected int oldEols;      // EOLs that appeared in a comment;
	protected BitArray ignore;  // set of characters to be ignored by the scanner

	protected Token tokens;     // list of tokens already peeked (first token is a dummy)
	protected Token pt;         // current peek token
	
	protected char[] tval = new char[128]; // text of current token
	protected int tlen;         // length of current token
	
	public Scanner(string text) : this(new Buffer(text)) {
	}
	
	public Scanner(Buffer buf) {
	    buffer = buf;
	    Init();
	}
	
	protected void Init() {
		pos = -1; line = 1; lineStart = 0;
		oldEols = 0;
		NextCh();
		ignore = new BitArray(charSetSize+1);
		ignore[' '] = true;  // blanks are always white space
		ignore[9] = true; ignore[10] = true; ignore[13] = true; 
		pt = tokens = new Token();  // first token is a dummy
	}
	
	protected void NextCh() {
		if (oldEols > 0) { ch = EOL; oldEols--; } 
		else {
			ch = (char)buffer.Read(); pos++;
			// replace isolated '\r' by '\n' in order to make
			// eol handling uniform across Windows, Unix and Mac
			if (ch == '\r' && buffer.Peek() != '\n') ch = EOL;
			if (ch == EOL) { line++; lineStart = pos + 1; }
		}

	}

	protected void AddCh() {
		if (tlen >= tval.Length) {
			char[] newBuf = new char[2 * tval.Length];
			Array.Copy(tval, 0, newBuf, 0, tval.Length);
			tval = newBuf;
		}
		tval[tlen++] = ch;
		NextCh();
	}



	bool Comment0() {
		int level = 1, line0 = line, lineStart0 = lineStart;
		NextCh();
		if (ch == '/') {
			NextCh();
			for(;;) {
				if (ch == 13) {
					NextCh();
					if (ch == 10) {
						level--;
						if (level == 0) { oldEols = line - line0; NextCh(); return true; }
						NextCh();
					}
				} else if (ch == Buffer.EOF) return false;
				else NextCh();
			}
		} else {
			if (ch==EOL) {line--; lineStart = lineStart0;}
			pos = pos - 2; buffer.Pos = pos+1; NextCh();
		}
		return false;
	}

	bool Comment1() {
		int level = 1, line0 = line, lineStart0 = lineStart;
		NextCh();
		if (ch == '*') {
			NextCh();
			for(;;) {
				if (ch == '*') {
					NextCh();
					if (ch == '/') {
						level--;
						if (level == 0) { oldEols = line - line0; NextCh(); return true; }
						NextCh();
					}
				} else if (ch == '/') {
					NextCh();
					if (ch == '*') {
						level++; NextCh();
					}
				} else if (ch == Buffer.EOF) return false;
				else NextCh();
			}
		} else {
			if (ch==EOL) {line--; lineStart = lineStart0;}
			pos = pos - 2; buffer.Pos = pos+1; NextCh();
		}
		return false;
	}


	protected void CheckLiteral() {
		switch (t.val) {
			case "put": t.kind = 4; break;
			case "halt": t.kind = 5; break;
			case "for": t.kind = 6; break;
			case "in": t.kind = 7; break;
			case "counter": t.kind = 8; break;
			case "alt": t.kind = 9; break;
			case "sep": t.kind = 10; break;
			case "end": t.kind = 11; break;
			case "while": t.kind = 12; break;
			case "else": t.kind = 13; break;
			case "if": t.kind = 14; break;
			case "set": t.kind = 15; break;
			case "return": t.kind = 17; break;
			case "template": t.kind = 18; break;
			case "inner": t.kind = 19; break;
			case "def": t.kind = 20; break;
			case "and": t.kind = 26; break;
			case "or": t.kind = 27; break;
			case "xor": t.kind = 28; break;
			case "true": t.kind = 40; break;
			case "false": t.kind = 41; break;
			case "yes": t.kind = 42; break;
			case "no": t.kind = 43; break;
			default: break;
		}
	}

	protected Token NextToken() {
		while (ignore[ch]) NextCh();
		if (ch == '/' && Comment0() ||ch == '/' && Comment1()) return NextToken();
		t = new Token();
		t.pos = pos; t.col = pos - lineStart + 1; t.line = line; 
		int state = start[ch];
		tlen = 0; AddCh();
		
		switch (state) {
			case -1: { t.kind = eofSym; break; } // NextCh already done
			case 0: { t.kind = noSym; break; }   // NextCh already done
			case 1:
				if ((ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 255)) {AddCh(); goto case 1;}
				else if (ch == '"') {AddCh(); goto case 3;}
				else if (ch == 92) {AddCh(); goto case 6;}
				else {t.kind = noSym; break;}
			case 2:
				if ((ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 255)) {AddCh(); goto case 2;}
				else if (ch == 39) {AddCh(); goto case 3;}
				else if (ch == 92) {AddCh(); goto case 7;}
				else {t.kind = noSym; break;}
			case 3:
				{t.kind = 1; break;}
			case 4:
				if ((ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z')) {AddCh(); goto case 4;}
				else {t.kind = 2; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 5:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 5;}
				else {t.kind = 3; break;}
			case 6:
				if ((ch == '"' || ch == 39 || ch == 92 || ch == 'n' || ch == 'r' || ch == 't')) {AddCh(); goto case 1;}
				else {t.kind = noSym; break;}
			case 7:
				if ((ch == '"' || ch == 39 || ch == 92 || ch == 'n' || ch == 'r' || ch == 't')) {AddCh(); goto case 2;}
				else {t.kind = noSym; break;}
			case 8:
				{t.kind = 21; break;}
			case 9:
				{t.kind = 22; break;}
			case 10:
				{t.kind = 23; break;}
			case 11:
				if (ch == '&') {AddCh(); goto case 12;}
				else {t.kind = noSym; break;}
			case 12:
				{t.kind = 24; break;}
			case 13:
				if (ch == '|') {AddCh(); goto case 14;}
				else {t.kind = noSym; break;}
			case 14:
				{t.kind = 25; break;}
			case 15:
				{t.kind = 29; break;}
			case 16:
				if (ch == '=') {AddCh(); goto case 17;}
				else {t.kind = noSym; break;}
			case 17:
				{t.kind = 30; break;}
			case 18:
				{t.kind = 31; break;}
			case 19:
				{t.kind = 32; break;}
			case 20:
				{t.kind = 35; break;}
			case 21:
				{t.kind = 36; break;}
			case 22:
				{t.kind = 37; break;}
			case 23:
				{t.kind = 38; break;}
			case 24:
				{t.kind = 39; break;}
			case 25:
				{t.kind = 44; break;}
			case 26:
				if (ch == '=') {AddCh(); goto case 15;}
				else {t.kind = 16; break;}
			case 27:
				if (ch == '=') {AddCh(); goto case 18;}
				else {t.kind = 33; break;}
			case 28:
				if (ch == '=') {AddCh(); goto case 19;}
				else {t.kind = 34; break;}

		}
		t.val = new String(tval, 0, tlen);
		return t;
	}
	
	// get the next token (possibly a token already seen during peeking)
	public Token Scan () {
		if (tokens.next == null) {
			return NextToken();
		} else {
			pt = tokens = tokens.next;
			return tokens;
		}
	}

	// peek for the next token, ignore pragmas
	public Token Peek () {
		if (pt.next == null) {
			do {
				pt = pt.next = NextToken();
			} while (pt.kind > maxT); // skip pragmas
		} else {
			do {
				pt = pt.next;
			} while (pt.kind > maxT);
		}
		return pt;
	}
	
	// make sure that peeking starts at the current scan position
	public void ResetPeek () { pt = tokens; }

} // end Scanner

}