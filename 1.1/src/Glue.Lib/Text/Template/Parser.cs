using Glue.Lib.Text.Template;
using Glue.Lib.Text.Template.AST;

using System;

namespace Glue.Lib.Text.Template {



public class Parser {
	const int _EOF = 0;
	const int _string = 1;
	const int _ident = 2;
	const int _number = 3;
	const int maxT = 45;

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

string identifier = "";
  Unit unit = null;

  public Unit Unit
  {
      get { return unit; }
  }

  void Err(string msg) 
  {
      errors.Error(la.line, la.col, msg);
  }

  void L(string fmt, params object[] args) 
  {
      Console.WriteLine(fmt, args);
  }

  int ConvertToNumber(string s) 
  {
      return Convert.ToInt32(s);
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

  bool IsNamedArgument() 
  {
      Token next = scanner.Peek();
      return la.kind == _ident && next.val == "=";
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
		unit = new Unit(t); 
		Block(unit.Inner);
	}

	void Block(ElementList list) {
		while (StartOf(1)) {
			Statement stm; 
			Statement(out stm);
			list.Add(stm); 
		}
	}

	void Statement(out Statement stm) {
		stm = null; 
		switch (la.kind) {
		case 4: {
			PutStatement(out stm);
			break;
		}
		case 6: {
			ForStatement(out stm);
			break;
		}
		case 12: {
			WhileStatement(out stm);
			break;
		}
		case 14: {
			IfStatement(out stm);
			break;
		}
		case 15: {
			AssignStatement(out stm);
			break;
		}
		case 17: {
			ReturnStatement(out stm);
			break;
		}
		case 20: {
			DefStatement(out stm);
			break;
		}
		case 18: {
			TemplateStatement(out stm);
			break;
		}
		case 2: {
			ApplyStatement(out stm);
			break;
		}
		case 5: {
			HaltStatement(out stm);
			break;
		}
		default: SynErr(46); break;
		}
	}

	void PutStatement(out Statement stm) {
		Expect(4);
		PutStatement s = new PutStatement(t); 
		Expr(out s.Expression);
		stm = s; 
	}

	void ForStatement(out Statement stm) {
		Expect(6);
		ForStatement s = new ForStatement(t); 
		Ident();
		s.Iterator = identifier; 
		Expect(7);
		Expr(out s.Container);
		if (la.kind == 8) {
			Get();
			Ident();
			s.Counter = identifier; 
		}
		Block(s.Inner);
		if (la.kind == 9) {
			Get();
			Block(s.Alt);
		}
		if (la.kind == 10) {
			Get();
			Block(s.Sep);
		}
		Expect(11);
		stm = s; 
	}

	void WhileStatement(out Statement stm) {
		Expect(12);
		WhileStatement s = new WhileStatement(t); 
		Expr(out s.Test);
		Block(s.True);
		if (la.kind == 13) {
			Get();
			Block(s.False);
		}
		Expect(11);
		stm = s; 
	}

	void IfStatement(out Statement stm) {
		Expect(14);
		IfStatement s = new IfStatement(t); 
		Expr(out s.Test);
		Block(s.True);
		if (la.kind == 13) {
			Get();
			Block(s.False);
		}
		Expect(11);
		stm = s; 
	}

	void AssignStatement(out Statement stm) {
		Expect(15);
		Ident();
		AssignStatement s = new AssignStatement(t, identifier); 
		Expect(16);
		Expr(out s.Expression);
		stm = s; 
	}

	void ReturnStatement(out Statement stm) {
		Expect(17);
		stm = new ReturnStatement(t); 
	}

	void DefStatement(out Statement stm) {
		Expect(20);
		Ident();
		MethodDefinition s = new MethodDefinition(t, identifier); 
		if (la.kind == 21) {
			Get();
			if (la.kind == 2) {
				ParameterDefinition p; 
				Par(out p);
				s.Parameters.Add(p); 
				while (la.kind == 22) {
					Get();
					Par(out p);
					s.Parameters.Add(p); 
				}
			}
			Expect(23);
		}
		Block(s.Inner);
		Expect(11);
		stm = s; 
	}

	void TemplateStatement(out Statement stm) {
		Expect(18);
		Ident();
		TemplateDefinition s = new TemplateDefinition(t, identifier); 
		Block(s.Header);
		if (la.kind == 19) {
			Get();
			Block(s.Footer);
		}
		Expect(11);
		stm = s; 
	}

	void ApplyStatement(out Statement stm) {
		Ident();
		ApplyStatement s = new ApplyStatement(t, identifier); 
		Block(s.Inner);
		Expect(11);
		stm = s; 
	}

	void HaltStatement(out Statement stm) {
		Expect(5);
		stm = new HaltStatement(t); 
	}

	void Expr(out Expression expr) {
		WokExpr(out expr);
		while (StartOf(2)) {
			if (la.kind == 24) {
				Get();
			} else if (la.kind == 25) {
				Get();
			} else if (la.kind == 26) {
				Get();
			} else if (la.kind == 27) {
				Get();
			} else {
				Get();
			}
			string op = t.val; Expression right; 
			WokExpr(out right);
			expr = new BinaryExpression(t, expr, op, right); 
		}
	}

	void Ident() {
		Expect(2);
		identifier = t.val; 
	}

	void Par(out ParameterDefinition par) {
		Ident();
		par = new ParameterDefinition(t, identifier); 
		if (la.kind == 16) {
			Get();
			Expr(out par.Default);
		}
	}

	void Arg(out Expression expr) {
		expr = null; 
		if (IsNamedArgument()) {
			Ident();
			NamedArgumentExpression e = new NamedArgumentExpression(t, identifier); 
			Expect(16);
			Expr(out e.Expression);
			expr = e; 
		} else if (StartOf(3)) {
			Expr(out expr);
		} else SynErr(47);
	}

	void WokExpr(out Expression expr) {
		SimpleExpr(out expr);
		while (StartOf(4)) {
			switch (la.kind) {
			case 29: {
				Get();
				break;
			}
			case 30: {
				Get();
				break;
			}
			case 31: {
				Get();
				break;
			}
			case 32: {
				Get();
				break;
			}
			case 33: {
				Get();
				break;
			}
			case 34: {
				Get();
				break;
			}
			}
			string op = t.val; Expression right; 
			SimpleExpr(out right);
			expr = new BinaryExpression(t, expr, op, right); 
		}
	}

	void SimpleExpr(out Expression expr) {
		Term(out expr);
		while (la.kind == 35 || la.kind == 36) {
			if (la.kind == 35) {
				Get();
			} else {
				Get();
			}
			string op = t.val; Expression right; 
			Term(out right);
			expr = new BinaryExpression(t, expr, op, right); 
		}
	}

	void Term(out Expression expr) {
		Factor(out expr);
		while (la.kind == 37 || la.kind == 38 || la.kind == 39) {
			if (la.kind == 37) {
				Get();
			} else if (la.kind == 38) {
				Get();
			} else {
				Get();
			}
			string op = t.val; Expression right; 
			Factor(out right);
			expr = new BinaryExpression(t, expr, op, right); 
		}
	}

	void Factor(out Expression expr) {
		expr = null; 
		if (la.kind == 2) {
			Qualident();
			ReferenceExpression refexpr = new ReferenceExpression(t, identifier);
			expr = refexpr; 
			if (la.kind == 21) {
				Get();
				Expression arg; 
				if (StartOf(3)) {
					Arg(out arg);
					refexpr.Arguments.Add(arg); 
					while (la.kind == 22) {
						Get();
						Arg(out arg);
						refexpr.Arguments.Add(arg); 
					}
				}
				Expect(23);
			}
		} else if (la.kind == 3) {
			Get();
			expr = new PrimitiveExpression(t, ConvertToNumber(t.val)); 
		} else if (la.kind == 1) {
			Get();
			expr = new PrimitiveExpression(t, ConvertToString(t.val)); 
		} else if (StartOf(5)) {
			if (la.kind == 40) {
				Get();
			} else if (la.kind == 41) {
				Get();
			} else if (la.kind == 42) {
				Get();
			} else {
				Get();
			}
			expr = new PrimitiveExpression(t, ConvertToBool(t.val)); 
		} else if (la.kind == 35 || la.kind == 36) {
			if (la.kind == 36) {
				Get();
			} else {
				Get();
			}
			string op = t.val; 
			Factor(out expr);
			expr = new BinaryExpression(t, new PrimitiveExpression(t, 0), op, expr); 
		} else SynErr(48);
	}

	void Qualident() {
		Expect(2);
		identifier = t.val; 
		while (la.kind == 44) {
			Get();
			Expect(2);
			identifier += "." + t.val; 
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
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,T,x, T,T,T,x, x,x,x,x, T,x,T,T, x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, T,T,T,T, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, x,x,x}

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
			case 4: s = "\"put\" expected"; break;
			case 5: s = "\"halt\" expected"; break;
			case 6: s = "\"for\" expected"; break;
			case 7: s = "\"in\" expected"; break;
			case 8: s = "\"counter\" expected"; break;
			case 9: s = "\"alt\" expected"; break;
			case 10: s = "\"sep\" expected"; break;
			case 11: s = "\"end\" expected"; break;
			case 12: s = "\"while\" expected"; break;
			case 13: s = "\"else\" expected"; break;
			case 14: s = "\"if\" expected"; break;
			case 15: s = "\"set\" expected"; break;
			case 16: s = "\"=\" expected"; break;
			case 17: s = "\"return\" expected"; break;
			case 18: s = "\"template\" expected"; break;
			case 19: s = "\"inner\" expected"; break;
			case 20: s = "\"def\" expected"; break;
			case 21: s = "\"(\" expected"; break;
			case 22: s = "\",\" expected"; break;
			case 23: s = "\")\" expected"; break;
			case 24: s = "\"&&\" expected"; break;
			case 25: s = "\"||\" expected"; break;
			case 26: s = "\"and\" expected"; break;
			case 27: s = "\"or\" expected"; break;
			case 28: s = "\"xor\" expected"; break;
			case 29: s = "\"==\" expected"; break;
			case 30: s = "\"!=\" expected"; break;
			case 31: s = "\"<=\" expected"; break;
			case 32: s = "\">=\" expected"; break;
			case 33: s = "\"<\" expected"; break;
			case 34: s = "\">\" expected"; break;
			case 35: s = "\"+\" expected"; break;
			case 36: s = "\"-\" expected"; break;
			case 37: s = "\"*\" expected"; break;
			case 38: s = "\"/\" expected"; break;
			case 39: s = "\"%\" expected"; break;
			case 40: s = "\"true\" expected"; break;
			case 41: s = "\"false\" expected"; break;
			case 42: s = "\"yes\" expected"; break;
			case 43: s = "\"no\" expected"; break;
			case 44: s = "\".\" expected"; break;
			case 45: s = "??? expected"; break;
			case 46: s = "invalid Statement"; break;
			case 47: s = "invalid Arg"; break;
			case 48: s = "invalid Factor"; break;

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