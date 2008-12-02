using Glue.Lib;
using Glue.Lib.Text.JSON;
using System.Collections;

using System;

namespace Glue.Lib.Text.JSON {



public class Parser {
	const int _EOF = 0;
	const int _string = 1;
	const int _ident = 2;
	const int _number = 3;
	const int maxT = 15;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	Scanner scanner;
	Errors  errors;
    public Scanner Scanner { get { return scanner; } }
    public Errors  Errors  { get { return errors; } }

	Token t;    // last recognized token
	Token la;   // lookahead token
	int errDist = minErrDist;

public object Result = null;

void Log(string fmt, params object[] args) 
{
    Glue.Lib.Log.Debug(String.Format(fmt, args));
}

void Err(string msg) 
{
    errors.Error(la.line, la.col, msg);
}

bool ConvertToBool(string s) 
{
    return string.Compare(s, "true", true) == 0 || string.Compare(s, "yes", true) == 0;
}

string ConvertToString(string s) 
{
    if (s != null && s.Length >= 2) 
    {
        if (s[0] == '"' && s[s.Length - 1] == '"')
        {
            s = s.Substring(1, s.Length - 2);
        }
        else if (s[0] == '\'' && s[s.Length - 1] == '\'')
        {
            s = s.Substring(1, s.Length - 2);
        }
    }
    return s.Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\t", "\t").Replace("\\\"", "\"").Replace("\\'", "'");
}

/*--------------------------------------------------------------------------*/



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		this.errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.Error(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}
	
	bool WeakSeparator (int n, int syFol, int repFol) {
		bool[] s = new bool[maxT+1];
		if (la.kind == n) { Get(); return true; }
		else if (StartOf(repFol)) return false;
		else {
			for (int i=0; i <= maxT; i++) {
				s[i] = set[syFol, i] || set[repFol, i] || set[0, i];
			}
			SynErr(n);
			while (!s[la.kind]) Get();
			return StartOf(syFol);
		}
	}
	
	void Root() {
		if (la.kind == 4) {
			Result = null; 
			Object(out Result);
		} else if (la.kind == 8) {
			Array(out Result);
		} else SynErr(16);
	}

	void Object(out object value) {
		OrderedDictionary dict = new OrderedDictionary(); value = dict; 
		Expect(4);
		Member(dict);
		while (la.kind == 5) {
			Get();
			Member(dict);
		}
		Expect(6);
	}

	void Array(out object value) {
		ArrayList list = new ArrayList(); value = list; object item = null; 
		Expect(8);
		Value(out item);
		list.Add(item); 
		while (la.kind == 5) {
			Get();
			Value(out item);
			list.Add(item); 
		}
		Expect(9);
	}

	void Member(IDictionary dict) {
		Expect(2);
		string key = t.val; object value = null; 
		Expect(7);
		Value(out value);
		dict.Add(key, value); 
	}

	void Value(out object value) {
		value = null; 
		switch (la.kind) {
		case 3: {
			Get();
			value = Convert.ToInt32(t.val); 
			break;
		}
		case 1: {
			Get();
			value = t.val; 
			break;
		}
		case 10: case 11: case 12: case 13: {
			if (la.kind == 10) {
				Get();
			} else if (la.kind == 11) {
				Get();
			} else if (la.kind == 12) {
				Get();
			} else {
				Get();
			}
			value = (bool)(t.val == "true" || t.val == "yes"); 
			break;
		}
		case 14: {
			Get();
			value = null; 
			break;
		}
		case 8: {
			Array(out value);
			break;
		}
		case 4: {
			Object(out value);
			break;
		}
		default: SynErr(17); break;
		}
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Root();

    Expect(0);
	}
	
	bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x}

	};
} // end Parser


public class Errors : System.Collections.Specialized.StringCollection {
	// public int count = 0;                            // number of errors detected
	string errMsgFormat = "-- line {0} col {1}: {2}";   // 0=line, 1=column, 2=text
	
	public void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "string expected"; break;
			case 2: s = "ident expected"; break;
			case 3: s = "number expected"; break;
			case 4: s = "\"{\" expected"; break;
			case 5: s = "\",\" expected"; break;
			case 6: s = "\"}\" expected"; break;
			case 7: s = "\":\" expected"; break;
			case 8: s = "\"[\" expected"; break;
			case 9: s = "\"]\" expected"; break;
			case 10: s = "\"true\" expected"; break;
			case 11: s = "\"false\" expected"; break;
			case 12: s = "\"yes\" expected"; break;
			case 13: s = "\"no\" expected"; break;
			case 14: s = "\"null\" expected"; break;
			case 15: s = "??? expected"; break;
			case 16: s = "invalid Root"; break;
			case 17: s = "invalid Value"; break;

			default: s = "error " + n; break;
		}
        Add(string.Format(errMsgFormat, line, col, s));
	}

	public void SemErr (int line, int col, int n) {
        Add(string.Format(errMsgFormat, line, col, "error " + n));
	}

	public void Error (int line, int col, string s) {
        Add(string.Format(errMsgFormat, line, col, s));
	}

	public override string ToString()
	{
	    string result = "";
	    foreach (string s in this)
	        result += s + "\r\n";
	    return result;
	}
	
} // Errors

}