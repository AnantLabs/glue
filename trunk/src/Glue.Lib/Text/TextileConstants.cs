namespace Glue.Lib.Text
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Text.RegularExpressions;

    public class TextileConstants {

		protected static readonly int MODE_ENT_COMPAT = 0;
		protected static readonly int MODE_ENT_NOQUOTES = 2;
		protected static readonly int MODE_ENT_QUOTES = 3;


		protected static readonly string LESS_THAN = "&lt;";
		protected static readonly string GREATER_THAN = "&gt;";


		protected static readonly string EXP_ISHTML = "<.*>";

		protected static readonly string EXP_AMPERSAND = "&(?![#a-zA-Z0-9]+;)";
		protected static readonly string EXP_AMPERSAND_REPLACE = "x%x%";


		protected static readonly string EXP_DOUBLEQUOTE_MATCH = "(^|\\s)==(.*)==([^[:alnum:]]{0,2})(\\s|$)";
		protected static readonly string EXP_DOUBLEQUOTE_REPLACE = "$1<notextile>$2</notextile>$3$4";

		protected static readonly string EXP_IMAGE_QTAG_MATCH = "!([^\\s\\(=^!]+?)\\s?(\\(([^\\)]+?)\\))?!";
		protected static readonly string EXP_IMAGE_QTAG_REPLACE = "<img src=\"$1\" alt=\"$3\" />";

		protected static readonly string EXP_IMAGE_WITH_HREF_QTAG_MATCH = "(<img.+ \\/>):(\\S+)";
		protected static readonly string EXP_IMAGE_WITH_HREF_QTAG_REPLACE = "<a href=\"$2\">$1</a>";

		protected static readonly string EXP_HREF_QTAG_MATCH = "\"([^\"\\(]+)\\s?(\\(([^\\)]+)\\))?\":(\\S+)(\\/?)(\\.)?([^\\w\\s\\/;]|[1-9]*?)(\\s|$)";
      //protected static readonly string EXP_HREF_QTAG_MATCH = "\"([^\"\\(]+)\\s?(\\(([^\\)]+)\\))?\":(\\S+)(\\/?)(\\.)?([^\\w\\s\\/;]|[1-9]*?)(\\s|$)";
		protected static readonly string EXP_HREF_QTAG_REPLACE = "<a href=\\\"$4\\\" title=\\\"$3\\\">$1</a>";


		protected static readonly string[] EXP_PHRASE_MODIFIER_SOURCETAGS = {"\\*\\*", "\\*", "\\?\\?", "-", "\\+", "~", "@"};
		protected static readonly string[] EXP_PHRASE_MODIFIER_REPLACETAGS = {"b", "strong", "cite", "del", "ins", "sub", "code"};
		protected static readonly string EXP_PHRASE_MODIFIER = "";

		protected static readonly string EXP_ITALICS_MATCH = "(^|\\s)__(.*?)__([^\\w\\s]{0,2})(\\s|$)?";
		protected static readonly string EXP_ITALICS_REPLACE = "$1<i>$2</i>$4";

		protected static readonly string EXP_EMPHASIS_MATCH = "(^|\\s)_(.*?)_([^\\w\\s]{0,2})(\\s|$)?";
		protected static readonly string EXP_EMPHASIS_REPLACE = "$1<em>$2</em>$4";

		protected static readonly string EXP_SUPERSCRIPT_MATCH = "(^|\\s)\\^(.*?)\\^(\\s|$)?";
		protected static readonly string EXP_SUPERSCRIPT_REPLACE = "$1<sup>$2</sup>$3";

		protected static readonly string EXP_EOL_DBL_QUOTES = "\"$";


		protected static readonly string EXP_SINGLE_CLOSING = "\"([^\\\\']*)\\\\'([^\\\\']*)\"";
		protected static readonly string EXP_SINGLE_OPENING = "\\'";
		protected static readonly string EXP_DOUBLE_CLOSING = "([^\\']*)\\\"([^\\\"]*)";
		protected static readonly string EXP_DOUBLE_OPENING = "\"";
		protected static readonly string EXP_ELLIPSES = "\\b( )?\\.{3}";
		protected static readonly string EXP_3UPPER_ACCRONYM = "\\b([A-Z][A-Z0-9]{2,})\\b(\\(([^\\)]+)\\))";
		protected static readonly string EXP_3UPPERCASE_CAPS = "(^|[^\"][>\\s])([A-Z][A-Z0-9 ]{2,})([^<a-z0-9]|$)";
		protected static readonly string EXP_EM_DASH = "\\s?--\\s?";
		protected static readonly string EXP_EN_DASH = "\\s-\\s";
		protected static readonly string EXP_EN_DECIMAL_DASH = "(\\d+)-(\\d+)";
		protected static readonly string EXP_DIMENSION_SIGN = "(\\d+) ?x ?(\\d+)";
		protected static readonly string EXP_TRADEMARK = "\\b ?(\\((tm|TM)\\))";
		protected static readonly string EXP_REGISTERED = "\\b ?(\\([rR]\\))";
		protected static readonly string EXP_COPYRIGHT = "\\b ?(\\([cC]\\))";


		protected static readonly string REPLACE_SINGLE_CLOSING = "$1&#8217;$2";
		protected static readonly string REPLACE_SINGLE_OPENING = "&#8216;";
		protected static readonly string REPLACE_DOUBLE_CLOSING = "$1&#8221;$2";
		protected static readonly string REPLACE_DOUBLE_OPENING = "&#8220;";
		protected static readonly string REPLACE_ELLIPSES = "$1&#8230;";
		protected static readonly string REPLACE_3UPPER_ACCRONYM = "<acronym title=\"$3\">$1</acronym>";
		protected static readonly string REPLACE_3UPPERCASE_CAPS = "$1<span class=\"caps\">$2</span>$3";

		protected static readonly string REPLACE_EM_DASH = " &#8212; ";
		protected static readonly string REPLACE_EN_DASH = " &#8211; ";
		protected static readonly string REPLACE_EN_DECIMAL_DASH = "$1&#8211;$2";
		protected static readonly string REPLACE_DIMENSION_SIGN = "$1&#215;$2";
		protected static readonly string REPLACE_TRADEMARK = "&#8482;";
		protected static readonly string REPLACE_REGISTERED = "&#174;";
		protected static readonly string REPLACE_COPYRIGHT = "&#169;";


		protected static readonly string EXP_STARTPRESERVE = "<(code|pre|kbd|notextile)>";
		protected static readonly string EXP_ENDPRESERVE = "</(code|pre|kbd|notextile)>";

		protected static readonly string EXP_FORCESLINEBREAKS = "(\\S)(_*)([:punct:]*) *\\n([^#*\\s])";
		protected static readonly string REPLACE_FORCESLINEBREAK = "$1$2$3<br/>$4";


		protected static readonly string EXP_BULLETED_LIST = "^\\s?\\*\\s(.*)$";
		protected static readonly string EXP_NUMERIC_LIST = "^\\s?#\\s(.*)$";
		protected static readonly string EXP_BLOCKQUOTE = "^bq\\. (.*)";
		protected static readonly string EXP_HEADER_WITHCLASS = "^h(\\d)\\(([\\w]+)\\)\\.\\s(.*)";
		protected static readonly string EXP_HEADER = "^h(\\d)\\. (.*)";
		protected static readonly string EXP_PARA_WITHCLASS = "^p\\(([\\w]+)\\)\\.\\s(.*)";
		protected static readonly string EXP_PARA = "^p\\. (.*)";
		protected static readonly string EXP_REMAINING_PARA = "^([^\\t ]+.*)";


		protected static readonly string REPLACE_BULLETED_LIST = "\t<liu>$1</liu>";
		protected static readonly string REPLACE_NUMERIC_LIST = "\t<lio>$1</lio>";
		protected static readonly string REPLACE_BLOCKQUOTE = "\t<blockquote>$1</blockquote>";
		protected static readonly string REPLACE_HEADER_WITHCLASS = "\t<h$1 class=\"$2\">$3</h$1>";
		protected static readonly string REPLACE_HEADER = "\t<h$1>$2</h$1>";
		protected static readonly string REPLACE_PARA_WITHCLASS = "\t<p class=\"$1\">$2</p>";
		protected static readonly string REPLACE_PARA = "\t<p>$1</p>";
		protected static readonly string REPLACE_REMAINING_PARA = "\t<p>$1</p>";

		protected static readonly string EXP_LISTSTART = "\\t<li";
		protected static readonly string EXP_MATCHLIST = "^(\\t<li)(o|u)";
		protected static readonly string REPLACE_MATCHLIST = "\n<$2l>\n$1$2";
		protected static readonly string EXP_ENDMATCHLIST = "^(.*)$";
		protected static readonly string REPLACE_ENDMATCHLIST = "l>\n$1";

	}
}