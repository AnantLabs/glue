namespace Glue.Lib.Text
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Text.RegularExpressions;

    // TODO: Sort out Textile and Textile2 (see below)

    ///<summary>
    /// Textile
    /// 
    /// Glue Project: C# implementation of Textism's Textile Humane Web Text Generator
    ///
    /// Textile is Copyright (c) 2003, Dean Allen, www.textism.com, All rights reserved
    /// The  origional Textile can be found at http://www.textism.com/tools/textile
    /// 
    /// Block modifier syntax:
    /// 
    /// Header: hn.
    /// Paragraphs beginning with 'hn. ' (where n is 1-6) are wrapped in header tags.
    /// Example: <h1>Text</h1>
    /// 
    /// Header with CSS class: hn(class).
    /// Paragraphs beginning with 'hn(class). ' receive a CSS class attribute.
    /// Example: <h1 class="class">Text</h1>
    /// 
    /// Paragraph: p. (applied by default)
    /// Paragraphs beginning with 'p. ' are wrapped in paragraph tags.
    /// Example: <p>Text</p>
    /// 
    /// Paragraph with CSS class: p(class).
    /// Paragraphs beginning with 'p(class). ' receive a CSS class attribute.
    /// Example: <p class="class">Text</p>
    /// 
    /// Blockquote: bq.
    /// Paragraphs beginning with 'bq. ' are wrapped in block quote tags.
    /// Example: <blockquote>Text</blockquote>
    /// 
    /// Blockquote with citation: bq(citeurl).
    /// Paragraphs beginning with 'bq(citeurl). ' recieve a citation attribute.
    /// Example: <blockquote cite="citeurl">Text</blockquote>
    /// 
    /// Numeric list: #
    /// Consecutive paragraphs beginning with # are wrapped in ordered list tags.
    /// Example: <ol><li>ordered list</li></ol>
    /// 
    /// Bulleted list: *
    /// Consecutive paragraphs beginning with * are wrapped in unordered list tags.
    /// Example: <ul><li>unordered list</li></ul>
    /// 
    /// 
    /// Phrase modifier syntax:
    /// 
    /// _emphasis_             <em>emphasis</em>
    /// __italic__             <i>italic</i>
    /// *strong*               <strong>strong</strong>
    /// **bold**               <b>bold</b>
    /// ??citation??           <cite>citation</cite>
    /// -deleted text-         <del>deleted</del>
    /// +inserted text+        <ins>inserted</ins>
    /// ^superscript^          <sup>superscript</sup>
    /// ~subscript~            <sub>subscript</sub>
    /// @code@                 <code>computer code</code>
    /// 
    /// ==notextile==          leave text alone (do not format)
    /// 
    /// "linktext":url         <a href="url">linktext</a>
    /// "linktext(title)":url  <a href="url" title="title">linktext</a>
    /// 
    /// !imageurl!             <img src="imageurl" />
    /// !imageurl(alt text)!   <img src="imageurl" alt="alt text" />
    /// !imageurl!:linkurl     <a href="linkurl"><img src="imageurl" /></a>
    /// 
    /// ABC(Always Be Closing) <acronym title="Always Be Closing">ABC</acronym>
    /// 
    ///</summary>
    public class Textile : TextileConstants
    {
        public static readonly Textile Default = new Textile();

        /// <summary>
        /// Public Constructor
        /// </summary>
        public Textile() 
        {
        }

        /// <summary>
        /// Process a textile formatted string
        /// </summary>
        /// <param name="content">Textile formatted content</param>
        /// <returns>Content converted to HTML</returns>
        public string Process(string content) 
        {
            /*
             * Turn any incoming ampersands into a dummy character for now.
             * This uses a negative lookahead for alphanumerics followed by a semicolon,
             * implying an incoming html entity, to be skipped
             */
            //text = preg_replace("&(?![#a-zA-Z0-9]+;)","x%x%",text);
            content = Regex.Replace(content, EXP_AMPERSAND, EXP_AMPERSAND_REPLACE);

            // unentify angle brackets and ampersands
            content = content.Replace(GREATER_THAN, ">");
            content = content.Replace(LESS_THAN, "<");
            content = content.Replace("&amp;", "&");

            // zap carriage returns
            content = content.Replace("\r\n", "\n");

            // zap tabs
            content = content.Replace("\t", "");

            // preserve double line breaks
            content = content.Replace("\n\n", "\n \n");

            // # might be a problem with lists
            content = content.Replace("l><br/>", "l>\n");


            // trim each line.. no it is not faster to String.split() here 
            // since we are just trimming fat
            StringBuilder splitBuffer = new StringBuilder();
            foreach (string token in content.Split('\n'))   
            {
                splitBuffer.Append(token.Trim());
                splitBuffer.Append("\n");
            }
            content = splitBuffer.ToString();

            //### Find and replace quick tags

            /*
             * double equal signs mean <notextile>
             */
            content = Regex.Replace(content, EXP_DOUBLEQUOTE_MATCH, EXP_DOUBLEQUOTE_REPLACE);

            /*
             * image qtag
             */
            content = Regex.Replace(content, EXP_IMAGE_QTAG_MATCH, EXP_IMAGE_QTAG_REPLACE);

            /*
             * image with hyperlink
             */
            content = Regex.Replace(content, EXP_IMAGE_WITH_HREF_QTAG_MATCH, EXP_IMAGE_WITH_HREF_QTAG_REPLACE);

            /*
             *  hyperlink qtag
             */
            content = Regex.Replace(content, EXP_HREF_QTAG_MATCH, EXP_HREF_QTAG_REPLACE);

            /*
             * loop through the array, replacing qtags with html
             */

            for (int x = 0; x < EXP_PHRASE_MODIFIER_SOURCETAGS.Length; x++) 
            {
                string prefix = "(^|\\s|>)" + EXP_PHRASE_MODIFIER_SOURCETAGS[x]
                    + "(.+?)([^\\w\\s]*?)"
                    + EXP_PHRASE_MODIFIER_SOURCETAGS[x] + "([^\\w\\s]{0,2})(\\s|$)?";
                //            string prefix = "(^|\\s|>)" + EXP_PHRASE_MODIFIER_SOURCETAGS[x]
                //                    + "\\b(.+?)\\b([^\\w\\s]*?)"
                //                    + EXP_PHRASE_MODIFIER_SOURCETAGS[x] + "([^\\w\\s]{0,2})(\\s|$)?";

                string suffix = "$1<" + EXP_PHRASE_MODIFIER_REPLACETAGS[x] + ">$2$3</" + EXP_PHRASE_MODIFIER_REPLACETAGS[x] + ">$4";
                content = Regex.Replace(content, prefix, suffix);
            }

            /*
             * From the Origional Docs:
             * "some weird bs with underscores and \b word boundaries,
             * so we'll do those on their own"
             */
            content = Regex.Replace(content, EXP_ITALICS_MATCH, EXP_ITALICS_REPLACE);
            content = Regex.Replace(content, EXP_EMPHASIS_MATCH, EXP_EMPHASIS_REPLACE);
            content = Regex.Replace(content, EXP_SUPERSCRIPT_MATCH, EXP_SUPERSCRIPT_REPLACE);

            /*
             * small problem with double quotes at the end of a string
             */
            content = Regex.Replace(content, EXP_EOL_DBL_QUOTES, " ");


            string[] glyphMatches = {EXP_SINGLE_CLOSING,
                                        EXP_SINGLE_OPENING,
                                        EXP_DOUBLE_CLOSING,
                                        EXP_DOUBLE_OPENING,
                                        EXP_ELLIPSES,
                                        EXP_3UPPER_ACCRONYM,
                                        EXP_3UPPERCASE_CAPS,
                                        EXP_EM_DASH,
                                        EXP_EN_DASH,
                                        EXP_EN_DECIMAL_DASH,
                                        EXP_DIMENSION_SIGN,
                                        EXP_TRADEMARK,
                                        EXP_REGISTERED,
                                        EXP_COPYRIGHT};


            string[] glyphReplacement = {REPLACE_SINGLE_CLOSING,
                                            REPLACE_SINGLE_OPENING,
                                            REPLACE_DOUBLE_CLOSING,
                                            REPLACE_DOUBLE_OPENING,
                                            REPLACE_ELLIPSES,
                                            REPLACE_3UPPER_ACCRONYM,
                                            REPLACE_3UPPERCASE_CAPS,
                                            REPLACE_EM_DASH,
                                            REPLACE_EN_DASH,
                                            REPLACE_EN_DECIMAL_DASH,
                                            REPLACE_DIMENSION_SIGN,
                                            REPLACE_TRADEMARK,
                                            REPLACE_REGISTERED,
                                            REPLACE_COPYRIGHT};

            Regex ishtmlre = new Regex(EXP_ISHTML, RegexOptions.Compiled);
            bool ishtml = ishtmlre.IsMatch(content);
            bool inpreservation = false;

            if (!ishtml) 
            {
                content = ArrayReplaceAll(content, glyphMatches, glyphReplacement);
            } 
            else 
            {
                string[] segments = SplitContent(ishtmlre, content);

                StringBuilder segmentBuffer = new StringBuilder();
                for (int x = 0; x < segments.Length; x++) 
                {
                    //  # matches are off if we're between <code>, <pre> etc.
                    if (Regex.IsMatch(segments[x].ToLower(), EXP_STARTPRESERVE)) 
                    {
                        inpreservation = true;
                    } 
                    else if (Regex.IsMatch(segments[x].ToLower(), EXP_ENDPRESERVE)) 
                    {
                        inpreservation = false;
                    }

                    if (!ishtmlre.IsMatch(segments[x]) && !inpreservation) 
                    {
                        segments[x] = ArrayReplaceAll(segments[x], glyphMatches, glyphReplacement);
                    }

                    //# convert htmlspecial if between <code>
                    if (inpreservation) 
                    {
                        segments[x] = HtmlSpecialChars(segments[x], MODE_ENT_NOQUOTES);
                        segments[x] = segments[x].Replace("&lt;pre&gt;", "<pre>");
                        segments[x] = segments[x].Replace("&lt;code&gt;", "<code>");
                        segments[x] = segments[x].Replace("&lt;notextile&gt;", "<notextile>");
                    }

                    segmentBuffer.Append(segments[x]);

                }

                content = segmentBuffer.ToString();

            }


            //### Block level formatting

            //# deal with forced breaks; this is going to be a problem between
            //#  <pre> tags, but we'll clean them later


            string[] blockMatches = {EXP_BULLETED_LIST,
                                        EXP_NUMERIC_LIST,
                                        EXP_BLOCKQUOTE,
                                        EXP_HEADER_WITHCLASS,
                                        EXP_HEADER,
                                        EXP_PARA_WITHCLASS,
                                        EXP_PARA,
                                        EXP_REMAINING_PARA};

            string[] blockReplace = {REPLACE_BULLETED_LIST,
                                        REPLACE_NUMERIC_LIST,
                                        REPLACE_BLOCKQUOTE,
                                        REPLACE_HEADER_WITHCLASS,
                                        REPLACE_HEADER,
                                        REPLACE_PARA_WITHCLASS,
                                        REPLACE_PARA,
                                        REPLACE_REMAINING_PARA};


            StringBuilder blockBuffer = new StringBuilder();
            string list = "";
            content += "\n";

            bool inpre = false;
            //# split the text into an array by newlines
            string[] tokens = content.Split('\n');
            int tokenCount = tokens.Length;

            for (int x = 0; x < tokenCount; x++)  
            {
                string line = tokens[x];

                //#make sure the line isn't blank
                if (line.Length > 0) 
                {
                    //# matches are off if we're between <pre> or <code> tags
                    if (line.ToLower().IndexOf("<pre>") > -1) 
                    {
                        inpre = true;
                    }

                    //# deal with block replacements first, then see if we're in a list
                    if (!inpre) 
                    {
                        line = ArrayReplaceAll(line, blockMatches, blockReplace);
                    }

                    //# kill any br tags that slipped in earlier
                    if (inpre) 
                    {
                        line = line.Replace("<br/>", "\n");
                        line = line.Replace("<br/>", "\n");
                    }
                    //# matches back on after </pre>
                    if (line.ToLower().IndexOf("</pre>") > -1) 
                    {
                        inpre = false;
                    }

                    //# at the beginning of a list, $line switches to a value
                    bool islist = Regex.IsMatch(line, EXP_LISTSTART);
                    bool islistline = Regex.IsMatch(line, EXP_LISTSTART + list);
                    if (list.Length == 0 && islist) 
                    {
                        line = Regex.Replace(line, EXP_MATCHLIST, REPLACE_MATCHLIST);
                        list = line.Substring(2, 3);

                        //# at the end of a list, $line switches to empty
                    } 
                    else if (list.Length > 0 && !islistline) 
                    {
                        line = Regex.Replace(line, EXP_ENDMATCHLIST, "</" + list + REPLACE_ENDMATCHLIST);
                        list = "";
                    }
                }
                // push each line to a new array once it's processed
                blockBuffer.Append(line);
                blockBuffer.Append("\n");
            }

            if (list.Length > 0) 
            {
                blockBuffer.Append("</" + list + "l>\n");
                list = "";
            }

            content = blockBuffer.ToString();

            // Trim trailing EOL
            if (content.EndsWith("\n")) 
            {
                content = content.Substring(0, content.Length - 1);
            }
            //        // Trim starting EOL
            //        if (content.startsWith("\n") || content.startsWith("\t")) {
            //            content = content.substring(1, content.length());
            //        }



            content = Regex.Replace(content, EXP_FORCESLINEBREAKS, REPLACE_FORCESLINEBREAK);




            // Clean Up <notextile>
            content = Regex.Replace(content, "<\\/?notextile>", "");

            // Clean up liu and lio
            content = Regex.Replace(content, "<(\\/?)li(u|o)>", "<$1li>");

            // Turn the temp char back to an ampersand entity
            content = content.Replace("x%x%", "&#38;");

            //# Newline linebreaks, just for markup tidiness
            content = content.Replace("<br/>", "<br/>\n");


            return content;
        }

        /**
         * An implementation of the PHP htmlspecialchars()
         *
         * @param origContent Source string
         * @param mode        Mode to select replacement string for quotes
         * @return String with replace occurrences
         */
        private string HtmlSpecialChars(string content, int mode) 
        {
            content = content.Replace("&", "&amp;");

            if (mode != MODE_ENT_NOQUOTES) 
            {
                content = content.Replace("\"", "&quot;");
            }
            if (mode == MODE_ENT_QUOTES) 
            {
                content = content.Replace("'", "&#039;");
            }
            content = content.Replace("<", LESS_THAN);
            content = content.Replace(">", GREATER_THAN);
            return content;
        }

        private string[] SplitContent(string matchexp, string content) 
        {
            Regex pattern = new Regex(matchexp, RegexOptions.Compiled);
            return SplitContent(pattern, content);
        }

        /**
         * Splits a string into a string array based on a matching regex
         *
         * @param matchexp Expression to match
         * @param content  Content to split
         * @return String array of split content
         */
        private string[] SplitContent(Regex pattern, string content) 
        {
            int startAt = 0;
            ArrayList tempList = new ArrayList();

            Match match = pattern.Match(content);

            while (match.Success) 
            {
                tempList.Add(content.Substring(startAt, match.Index));
                tempList.Add(match.ToString());
                startAt = match.Index;
                match = match.NextMatch();
            }

            tempList.Add(content.Substring(startAt));

            string[] result = new string[tempList.Count];
            tempList.CopyTo(result);
            return result;
        }

        /**
         * Replace an array of match patterns in a string
         *
         * @param content  Source string
         * @param matches  Match patterns
         * @param replaces Replacement patterns
         * @return String with replaced occurrences
         */
        private string ArrayReplaceAll(string content, string[] matches, string[] replaces) 
        {
            string result = content;

            for (int x = 0; x < matches.Length; x++) 
            {
                result = Regex.Replace(result, matches[x], replaces[x]);
            }

            return result;
        }
    }

    public class Textile2
    {
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

        /// <summary>
        /// Public Constructor
        /// </summary>
        public Textile2() 
        {
        }

        /// <summary>
        /// Process a textile formatted string
        /// </summary>
        /// <param name="content">Textile formatted content</param>
        /// <returns>Content converted to HTML</returns>
        public string Process(string content) 
        {
            // Turn any incoming ampersands into a dummy character for now.
            // This uses a negative lookahead for alphanumerics followed by a semicolon,
            // implying an incoming html entity, to be skipped

            //text = preg_replace("&(?![#a-zA-Z0-9]+;)","x%x%",text);
            content = Regex.Replace(content, EXP_AMPERSAND, EXP_AMPERSAND_REPLACE);

            // unentify angle brackets and ampersands
            content = content.Replace(GREATER_THAN, ">");
            content = content.Replace(LESS_THAN, "<");
            content = content.Replace("&amp;", "&");

            // zap carriage returns
            content = content.Replace("\r\n", "\n");

            // zap tabs
            content = content.Replace("\t", "");

            // trim each line.. no it is not faster to string.split() here since we are just trimming fat
            StringBuilder splitBuffer = new StringBuilder();
            foreach (string token in content.Split('\n'))
            {
                splitBuffer.Append(token.Trim());
                splitBuffer.Append("\n");
            }
            content = splitBuffer.ToString();

            //### Find and replace quick tags

            // double equal signs mean <notextile>
            content = Regex.Replace(content, EXP_DOUBLEQUOTE_MATCH, EXP_DOUBLEQUOTE_REPLACE);

            // image qtag
            content = Regex.Replace(content, EXP_IMAGE_QTAG_MATCH, EXP_IMAGE_QTAG_REPLACE);

            // image with hyperlink
            content = Regex.Replace(content, EXP_IMAGE_WITH_HREF_QTAG_MATCH, EXP_IMAGE_WITH_HREF_QTAG_REPLACE);

            // hyperlink qtag
            content = Regex.Replace(content, EXP_HREF_QTAG_MATCH, EXP_HREF_QTAG_REPLACE);

            // loop through the array, replacing qtags with html
            for (int x = 0; x < EXP_PHRASE_MODIFIER_SOURCETAGS.Length; x++) 
            {
                string prefix = "(^|\\s|>)" + EXP_PHRASE_MODIFIER_SOURCETAGS[x]
                    + "(.+?)([^\\w\\s]*?)"
                    + EXP_PHRASE_MODIFIER_SOURCETAGS[x] + "([^\\w\\s]{0,2})(\\s|$)?";
                //            string prefix = "(^|\\s|>)" + EXP_PHRASE_MODIFIER_SOURCETAGS[x]
                //                    + "\\b(.+?)\\b([^\\w\\s]*?)"
                //                    + EXP_PHRASE_MODIFIER_SOURCETAGS[x] + "([^\\w\\s]{0,2})(\\s|$)?";

                string suffix = "$1<" + EXP_PHRASE_MODIFIER_REPLACETAGS[x] + ">$2$3</" + EXP_PHRASE_MODIFIER_REPLACETAGS[x] + ">$4";
                content = Regex.Replace(content, prefix, suffix);
            }

            // From the Origional Docs:
            // "some weird bs with underscores and \b word boundaries,
            // so we'll do those on their own"
            content = Regex.Replace(content, EXP_ITALICS_MATCH, EXP_ITALICS_REPLACE);
            content = Regex.Replace(content, EXP_EMPHASIS_MATCH, EXP_EMPHASIS_REPLACE);
            content = Regex.Replace(content, EXP_SUPERSCRIPT_MATCH, EXP_SUPERSCRIPT_REPLACE);

            // small problem with double quotes at the end of a string
            content = Regex.Replace(content, EXP_EOL_DBL_QUOTES, " ");


            string[] glyphMatches = {EXP_SINGLE_CLOSING,
                                        EXP_SINGLE_OPENING,
                                        EXP_DOUBLE_CLOSING,
                                        EXP_DOUBLE_OPENING,
                                        EXP_ELLIPSES,
                                        EXP_3UPPER_ACCRONYM,
                                        EXP_3UPPERCASE_CAPS,
                                        EXP_EM_DASH,
                                        EXP_EN_DASH,
                                        EXP_EN_DECIMAL_DASH,
                                        EXP_DIMENSION_SIGN,
                                        EXP_TRADEMARK,
                                        EXP_REGISTERED,
                                        EXP_COPYRIGHT};


            string[] glyphReplacement = {REPLACE_SINGLE_CLOSING,
                                            REPLACE_SINGLE_OPENING,
                                            REPLACE_DOUBLE_CLOSING,
                                            REPLACE_DOUBLE_OPENING,
                                            REPLACE_ELLIPSES,
                                            REPLACE_3UPPER_ACCRONYM,
                                            REPLACE_3UPPERCASE_CAPS,
                                            REPLACE_EM_DASH,
                                            REPLACE_EN_DASH,
                                            REPLACE_EN_DECIMAL_DASH,
                                            REPLACE_DIMENSION_SIGN,
                                            REPLACE_TRADEMARK,
                                            REPLACE_REGISTERED,
                                            REPLACE_COPYRIGHT};

            Regex ishtml = new Regex(EXP_ISHTML, RegexOptions.Compiled);
            bool inpreservation = false;

            if (!ishtml.IsMatch(content)) 
            {
                content = ArrayReplaceAll(content, glyphMatches, glyphReplacement);
            } 
            else 
            {
                string[] segments = SplitContent(ishtml, content);

                StringBuilder segmentBuffer = new StringBuilder();
                for (int x = 0; x < segments.Length; x++) 
                {
                    //  # matches are off if we're between <code>, <pre> etc.
                    if (Regex.IsMatch(segments[x].ToLower(), EXP_STARTPRESERVE))
                    {
                        inpreservation = true;
                    } 
                    else if (Regex.IsMatch(segments[x].ToLower(), EXP_ENDPRESERVE))
                    {
                        inpreservation = false;
                    }

                    if (!ishtml.IsMatch(segments[x]) && !inpreservation) 
                    {
                        segments[x] = ArrayReplaceAll(segments[x], glyphMatches, glyphReplacement);
                    }

                    //# convert htmlspecial if between <code>
                    if (inpreservation) 
                    {
                        segments[x] = HtmlSpecialChars(segments[x], MODE_ENT_NOQUOTES);
                        segments[x] = segments[x].Replace("&lt;pre&gt;", "<pre>");
                        segments[x] = segments[x].Replace("&lt;code&gt;", "<code>");
                        segments[x] = segments[x].Replace("&lt;notextile&gt;", "<notextile>");
                    }

                    segmentBuffer.Append(segments[x]);

                }

                content = segmentBuffer.ToString();

            }


            //### Block level formatting

            //# deal with forced breaks; this is going to be a problem between
            //#  <pre> tags, but we'll clean them later

            content = Regex.Replace(content, EXP_FORCESLINEBREAKS, REPLACE_FORCESLINEBREAK);

            //# might be a problem with lists
            content = content.Replace("l><br/>", "l>\n");


            string[] blockMatches = {EXP_BULLETED_LIST,
                                        EXP_NUMERIC_LIST,
                                        EXP_BLOCKQUOTE,
                                        EXP_HEADER_WITHCLASS,
                                        EXP_HEADER,
                                        EXP_PARA_WITHCLASS,
                                        EXP_PARA,
                                        EXP_REMAINING_PARA};

            string[] blockReplace = {REPLACE_BULLETED_LIST,
                                        REPLACE_NUMERIC_LIST,
                                        REPLACE_BLOCKQUOTE,
                                        REPLACE_HEADER_WITHCLASS,
                                        REPLACE_HEADER,
                                        REPLACE_PARA_WITHCLASS,
                                        REPLACE_PARA,
                                        REPLACE_REMAINING_PARA};


            StringBuilder blockBuffer = new StringBuilder();
            string list = "";
            content += "\n";

            bool inpre = false;
            //# split the text into an array by newlines
            string[] lines = content.Split('\n');
            for (int i = 0; i < lines.Length; i++) 
            {
                string line = lines[i];
                //#make sure the line isn't blank
                if (line.Length > 0) 
                {

                    //# matches are off if we're between <pre> or <code> tags
                    if (line.ToLower().IndexOf("<pre>") > -1) 
                    {
                        inpre = true;
                    }

                    //# deal with block replacements first, then see if we're in a list
                    if (!inpre) 
                    {
                        line = ArrayReplaceAll(line, blockMatches, blockReplace);
                    }

                    //# kill any br tags that slipped in earlier
                    if (inpre) 
                    {
                        line = line.Replace("<br/>", "\n");
                        line = line.Replace("<br/>", "\n");
                    }
                    //# matches back on after </pre>
                    if (line.ToLower().IndexOf("</pre>") > -1) 
                    {
                        inpre = false;
                    }

                    //# at the beginning of a list, $line switches to a value
                    bool islist = Regex.IsMatch(line, EXP_LISTSTART);
                    bool islistline = Regex.IsMatch(line, EXP_LISTSTART + list);
                    if (list.Length == 0 && islist) 
                    {
                        line = Regex.Replace(line, EXP_MATCHLIST, REPLACE_MATCHLIST);
                        list = line.Substring(2, 3);

                        //# at the end of a list, $line switches to empty
                    } 
                    else if (list.Length > 0 && !islistline) 
                    {
                        line = Regex.Replace(line, EXP_ENDMATCHLIST, "</" + list + REPLACE_ENDMATCHLIST);
                        list = "";
                    }
                }
                // push each line to a new array once it's processed
                blockBuffer.Append(line);
                blockBuffer.Append("\n");
            }

            content = blockBuffer.ToString();

            // Trim trailing EOL
            if (content.EndsWith("\n")) 
            {
                content = content.Substring(0, content.Length - 1);
            }

            // Clean Up <notextile>
            content = Regex.Replace(content, "<\\/?notextile>", "");

            // Clean up liu and lio
            content = Regex.Replace(content, "<(\\/?)li(u|o)>", "<$1li>");

            // Turn the temp char back to an ampersand entity
            content = content.Replace("x%x%", "&#38;");

            //# Newline linebreaks, just for markup tidiness
            content = content.Replace("<br/>", "<br/>\n");


            return content;
        }

        /// <summary>
        /// An implementation of the PHP htmlspecialchars()
        /// </summary>
        /// <param name="content">Source string</param>
        /// <param name="mode">Mode to select replacement string for quotes</param>
        /// <returns>string with replace occurrences</returns>
        private string HtmlSpecialChars(string content, int mode) 
        {
            content = content.Replace("&", "&amp;");

            if (mode != MODE_ENT_NOQUOTES) 
            {
                content = content.Replace("\"", "&quot;");
            }
            if (mode == MODE_ENT_QUOTES) 
            {
                content = content.Replace("'", "&#039;");
            }
            content = content.Replace("<", LESS_THAN);
            content = content.Replace(">", GREATER_THAN);
            return content;
        }


        /// <summary>
        /// Splits a string into a string array based on a matching regex
        /// </summary>
        /// <param name="matchexp">Expression to match</param>
        /// <param name="content">Content to split</param>
        /// <returns>string array of split content</returns>
        private string[] SplitContent(string matchexp, string content) 
        {
            Regex pattern = new Regex(matchexp, RegexOptions.Compiled);
            return SplitContent(pattern, content);
        }

        /// <summary>
        /// Splits a string into a string array based on a matching regex
        /// </summary>
        /// <param name="pattern">Expression to match</param>
        /// <param name="content">Content to split</param>
        /// <returns>string array of split content</returns>
        private string[] SplitContent(Regex pattern, string content)
        {
            int startAt = 0;
            ArrayList tempList = new ArrayList();

            Match match = pattern.Match(content);

            while (match.Success) 
            {
                tempList.Add(content.Substring(startAt, match.Index));
                tempList.Add(match.ToString());
                startAt = match.Index;
                match = match.NextMatch();
            }
            tempList.Add(content.Substring(startAt));

            string[] result = new string[tempList.Count];
            tempList.CopyTo(result);
            return result;
        }

        /// <summary>
        /// Replace an array of match patterns in a string
        /// </summary>
        /// <param name="content">Source string</param>
        /// <param name="matches">Match patterns</param>
        /// <param name="replaces">Replacement patterns</param>
        /// <returns>string with replaced occurrences</returns>
        private string ArrayReplaceAll(string content, string[] matches, string[] replaces) 
        {
            string result = content;

            for (int x = 0; x < matches.Length; x++) 
            {
                result = Regex.Replace(result, matches[x], replaces[x]);
            }

            return result;
        }
    }
}