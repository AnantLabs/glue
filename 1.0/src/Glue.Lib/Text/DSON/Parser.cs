using Glue.Lib;
using Glue.Lib.Text.DSON;
using System.Collections;

using System;

namespace Glue.Lib.Text.DSON {



public class Parser {
	const int _EOF = 0;
	const int _string = 1;
	const int _ident = 2;
	const int _number = 3;
	const int _time = 4;
	const int _date = 5;
	const int maxT = 17;

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

int ConvertToInt(string s) 
{
    Console.WriteLine("Int: " + s);
    return Convert.ToInt32(s);
}

bool ConvertToBool(string s) 
{
    Console.WriteLine("Bool: " + s);
    return string.Compare(s, "true", true) == 0 || string.Compare(s, "yes", true) == 0;
}

static string[] date_time_formats = { 
                                "yyyy-MM-dd", 
                                "yyyy-MM-ddTHH:mm",
                                "yyyy-MM-ddTHH:mm:ss",
                                "yyyy-MM-ddTHH:mm:ss.ff",
                                "yyyy-MM-ddTHH:mm:ss.fff",
                                "HH:mm",
                                "HH:mm:ss",
                                "HH:mm:ss.ff",
                                "HH:mm:ss.fff"
                            };
/*
Regex = new Regex("
    ((\d\d\d\d)-(\d\d)-(\d\d)(T(\d\d):(\d\d)(:(\d\d(\.(\d+))))))|
    ((\d\d):(\d\d)(:(\d\d(\.(\d+)))))
    ";
*/

DateTime ConvertToDateTime(string s) 
{
    return DateTime.ParseExact(s, date_time_formats, null, System.Globalization.DateTimeStyles.None); 
}

string ConvertToString(string s) 
{
    // check if doc-string style
    if (s != null && s.Length >= 6 && s.StartsWith("\"\"\"") && s.EndsWith("\"\"\""))
    {
        s = s.Substring(3, s.Length - 6);
        s = StringHelper.Unindent(s);
        s = StringHelper.Eat(s, "\n");
        return s;
    }
    if (s != null && s.Length >= 2) 
    {
        if (s[0] == '"' && s[s.Length - 1] == '"')
            s = s.Substring(1, s.Length - 2);
        else if (s[0] == '\'' && s[s.Length - 1] == '\'')
            s = s.Substring(1, s.Length - 2);
        s = StringHelper.UnEscapeCStyle(s);
        return s;
    }
    return s;
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
		if (la.kind == 6) {
			Result = null; 
			Dict(out Result);
		} else if (la.kind == 9) {
			List(out Result);
		} else SynErr(18);
	}

	void Dict(out object value) {
		Item dict = new Item(); value = dict; 
		Expect(6);
		Member(dict);
		while (la.kind == 2 || la.kind == 7) {
			if (la.kind == 7) {
				Get();
			}
			Member(dict);
		}
		Expect(8);
	}

	void List(out object value) {
		Item list = new Item(); value = list; object item = null; 
		Expect(9);
		Value(out item);
		list.Add(item); 
		while (StartOf(1)) {
			if (la.kind == 7) {
				Get();
			}
			Value(out item);
			list.Add(item); 
		}
		Expect(10);
	}

	void Member(Item dict) {
		Expect(2);
		string key = t.val; object value = null; 
		Expect(11);
		Value(out value);
		dict.Set(key, value); 
	}

	void Value(out object value) {
		value = null; 
		switch (la.kind) {
		case 5: {
			Get();
			value = ConvertToDateTime(t.val); 
			break;
		}
		case 4: {
			Get();
			value = ConvertToDateTime(t.val); 
			break;
		}
		case 3: {
			Get();
			value = ConvertToInt(t.val); 
			break;
		}
		case 1: {
			Get();
			value = ConvertToString(t.val); 
			break;
		}
		case 12: case 13: case 14: case 15: {
			if (la.kind == 12) {
				Get();
			} else if (la.kind == 13) {
				Get();
			} else if (la.kind == 14) {
				Get();
			} else {
				Get();
			}
			value = ConvertToBool(t.val); 
			break;
		}
		case 16: {
			Get();
			value = null; 
			break;
		}
		case 2: {
			Get();
			value = t.val; 
			break;
		}
		case 9: {
			List(out value);
			break;
		}
		case 6: {
			Dict(out value);
			break;
		}
		default: SynErr(19); break;
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
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,T,T,T, T,T,T,T, x,T,x,x, T,T,T,T, T,x,x}

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
			case 4: s = "time expected"; break;
			case 5: s = "date expected"; break;
			case 6: s = "\"{\" expected"; break;
			case 7: s = "\",\" expected"; break;
			case 8: s = "\"}\" expected"; break;
			case 9: s = "\"[\" expected"; break;
			case 10: s = "\"]\" expected"; break;
			case 11: s = "\":\" expected"; break;
			case 12: s = "\"true\" expected"; break;
			case 13: s = "\"false\" expected"; break;
			case 14: s = "\"yes\" expected"; break;
			case 15: s = "\"no\" expected"; break;
			case 16: s = "\"null\" expected"; break;
			case 17: s = "??? expected"; break;
			case 18: s = "invalid Root"; break;
			case 19: s = "invalid Value"; break;

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