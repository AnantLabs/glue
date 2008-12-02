
using System;
using System.IO;
using System.Collections; 

namespace Glue.Lib.Text.DSON {

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
	const int maxT = 17;
	const int noSym = 17;
	short[] start = {
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0, 39,  0,  0,  0,  0,  2,  0,  0,  0,  0, 47,  0,  0,  0,
	 38, 38, 38, 38, 38, 38, 38, 38, 38, 38, 51,  0,  0,  0,  0,  0,
	  0,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,
	  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7, 49,  0, 50,  0,  7,
	  0,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,
	  7,  7,  7,  7,  7,  7,  7,  7,  7,  7,  7, 46,  0, 48,  0,  0,
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
			case "true": t.kind = 12; break;
			case "false": t.kind = 13; break;
			case "yes": t.kind = 14; break;
			case "no": t.kind = 15; break;
			case "null": t.kind = 16; break;
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
				else if (ch == '"') {AddCh(); goto case 6;}
				else if (ch == 92) {AddCh(); goto case 40;}
				else {t.kind = noSym; break;}
			case 2:
				if ((ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 255)) {AddCh(); goto case 2;}
				else if (ch == 39) {AddCh(); goto case 6;}
				else if (ch == 92) {AddCh(); goto case 41;}
				else {t.kind = noSym; break;}
			case 3:
				if (!(ch == '"') && ch != Buffer.EOF) {AddCh(); goto case 3;}
				else if (ch == '"') {AddCh(); goto case 4;}
				else {t.kind = noSym; break;}
			case 4:
				if (ch == '"') {AddCh(); goto case 5;}
				else {t.kind = noSym; break;}
			case 5:
				if (ch == '"') {AddCh(); goto case 6;}
				else {t.kind = noSym; break;}
			case 6:
				{t.kind = 1; break;}
			case 7:
				if ((ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z')) {AddCh(); goto case 7;}
				else {t.kind = 2; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 8:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 8;}
				else {t.kind = 3; break;}
			case 9:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 10;}
				else {t.kind = noSym; break;}
			case 10:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 11;}
				else {t.kind = noSym; break;}
			case 11:
				if (ch == ':') {AddCh(); goto case 12;}
				else {t.kind = 4; break;}
			case 12:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 13;}
				else {t.kind = noSym; break;}
			case 13:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 14;}
				else {t.kind = noSym; break;}
			case 14:
				if (ch == '.') {AddCh(); goto case 15;}
				else {t.kind = 4; break;}
			case 15:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 16;}
				else {t.kind = noSym; break;}
			case 16:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 17;}
				else {t.kind = noSym; break;}
			case 17:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 18;}
				else {t.kind = noSym; break;}
			case 18:
				{t.kind = 4; break;}
			case 19:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 20;}
				else {t.kind = noSym; break;}
			case 20:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 21;}
				else {t.kind = noSym; break;}
			case 21:
				if (ch == '-') {AddCh(); goto case 22;}
				else {t.kind = noSym; break;}
			case 22:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 23;}
				else {t.kind = noSym; break;}
			case 23:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 24;}
				else {t.kind = noSym; break;}
			case 24:
				if (ch == 'T') {AddCh(); goto case 25;}
				else {t.kind = 5; break;}
			case 25:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 26;}
				else {t.kind = noSym; break;}
			case 26:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 27;}
				else {t.kind = noSym; break;}
			case 27:
				if (ch == ':') {AddCh(); goto case 28;}
				else {t.kind = noSym; break;}
			case 28:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 29;}
				else {t.kind = noSym; break;}
			case 29:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 30;}
				else {t.kind = noSym; break;}
			case 30:
				if (ch == ':') {AddCh(); goto case 31;}
				else {t.kind = 5; break;}
			case 31:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 32;}
				else {t.kind = noSym; break;}
			case 32:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 33;}
				else {t.kind = noSym; break;}
			case 33:
				if (ch == '.') {AddCh(); goto case 34;}
				else {t.kind = 5; break;}
			case 34:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 35;}
				else {t.kind = noSym; break;}
			case 35:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 36;}
				else {t.kind = noSym; break;}
			case 36:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 37;}
				else {t.kind = noSym; break;}
			case 37:
				{t.kind = 5; break;}
			case 38:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 42;}
				else {t.kind = 3; break;}
			case 39:
				if ((ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 255)) {AddCh(); goto case 1;}
				else if (ch == '"') {AddCh(); goto case 43;}
				else if (ch == 92) {AddCh(); goto case 40;}
				else {t.kind = noSym; break;}
			case 40:
				if ((ch == '"' || ch == 39 || ch == 92 || ch == 'n' || ch == 'r' || ch == 't')) {AddCh(); goto case 1;}
				else {t.kind = noSym; break;}
			case 41:
				if ((ch == '"' || ch == 39 || ch == 92 || ch == 'n' || ch == 'r' || ch == 't')) {AddCh(); goto case 2;}
				else {t.kind = noSym; break;}
			case 42:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 44;}
				else if (ch == ':') {AddCh(); goto case 9;}
				else {t.kind = 3; break;}
			case 43:
				if (ch == '"') {AddCh(); goto case 3;}
				else {t.kind = 1; break;}
			case 44:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 45;}
				else {t.kind = 3; break;}
			case 45:
				if ((ch >= '0' && ch <= '9')) {AddCh(); goto case 8;}
				else if (ch == '-') {AddCh(); goto case 19;}
				else {t.kind = 3; break;}
			case 46:
				{t.kind = 6; break;}
			case 47:
				{t.kind = 7; break;}
			case 48:
				{t.kind = 8; break;}
			case 49:
				{t.kind = 9; break;}
			case 50:
				{t.kind = 10; break;}
			case 51:
				{t.kind = 11; break;}

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