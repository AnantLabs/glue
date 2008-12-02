using System;
using System.Collections;
using System.Text.RegularExpressions;
using Glue.Lib;

namespace Glue.Web
{
    // Maps URL's to controllers and actions.
    public class Route
    {
        IDictionary _parms;
        Regex _pattern;
        string[] _names;

        public Route(string pattern, IDictionary parms)
        {
            _pattern = new Regex(
                pattern, 
                RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase)
                ;
            _names = _pattern.GetGroupNames();
            _parms = parms;
        }
        
        public bool IsMatch(IRequest req)
        {
            Match m = _pattern.Match(req.Path);
            if (!m.Success)
                return false;
            foreach (string name in _names)
                req.Params[name] = m.Groups[name].ToString();
            if (_parms != null)
                foreach (DictionaryEntry e in _parms)
                    req.Params[(string)e.Key] = (string)e.Value;
            return true;
        }
    }

    // Maps URL's to controllers and actions.
    public class Routing : ArrayList
    {
        public void Add(string pattern, IDictionary parms)
        {
            base.Add(new Route(pattern, parms));
        }
        public new Route this[int i]
        {
            get { return (Route)base[i]; }
        }
    }
}
