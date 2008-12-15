using System;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml;
using System.Web.Caching;
using System.Web;
using System.IO;

namespace Glue.Lib
{
    /// <summary>
    /// The Config class is the central registry for your application. You can mount 
    /// several (parts of) xml files onto keys in the Config class; it will take 
    /// care of creating and invoking the settings objects you define. The Config class
    /// can also watch for external changes in the underlying XML files and will
    /// automatically recreate your settings objects.
    /// 
    /// When you request the config object through the Config.Get
    /// method with a key, a configuration object of a previously registered
    /// type will be constructed by feeding it the xml contents. 
    /// Normally this object is created once and cached. You can 
    /// instruct the Config class to watch for changes in the underlying
    /// XML file by setting watch parameter to true.
    /// </summary>
    /// <example>
    ///   <code>
    ///   class MySettings
    ///   {
    ///       // Current settings singleton
    ///         
    ///       public static MySettings Current
    ///       {
    ///           return Config.Get("mysettings", typeof(MySettings));
    ///       }
    ///
    ///       public readonly int Number = 2;
    ///         
    ///       MySettings() {}
    ///         
    ///       MySettings(XmlNode node)
    ///       {
    ///           Number = Config.Read(node, "number", Number);
    ///       }
    ///     }
    ///     ...
    ///     Config.RegisterStore("%systemdrive%/logs/test.config", "mysettings", true);
    ///     ...
    ///     Console.WriteLine(MySettings.Current.Number);
    ///   </code>
    /// </example>
    public class Configuration : IConfigurationSectionHandler
    {
        static ArrayList _stores = new ArrayList();
        
        /// <summary>
        /// Static constructor, initializes the whole shebang with a static
        /// memory configuration store (for default registrations) and a file 
        /// configuration store mapping to the standard .NET config file
        /// (web.config or %exename%.config).
        /// </summary>
        static Configuration()
        {
            Register(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, true);
        }

        public static void Register(string path, bool watch)
        {
            _stores.Add(path);
            HttpRuntime.Cache.Remove("#Glue.Lib.Configuration#");
        }

        /// <summary>
        /// Returns the configuration object mounted at given key,
        /// returns null if no object is found.
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCode]
        public static object Get(string key)
        {
			return Get(key, null);
        }

        static string[] _rootdependency = {"#Glue.Lib.Configuration#"};

        /// <summary>
        /// Returns the configuration object mounted at given key,
        /// If no information is found, registeres type _default and 
        /// returns an instance of this type. 
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCode]
        public static object Get(string key, Type type)
        {
            object obj = HttpRuntime.Cache["#Glue.Lib.Configuration#Cached#" + key + "#" + type];
            if (obj == null)
            {
                // Find node, possibly changing type to a ancestor type.
                Type t = null;
                XmlNode n = GetElement(key);
                if (n == null)
				{
                    t = type;
                }
                else
                {
                    t = GetAttrType(n, "type", type);
                    if (type != null)
                        if (t == null || t != type && !t.IsSubclassOf(type))
                            t = type;
                }
                if (t == null)
                {
                    throw new System.Configuration.ConfigurationErrorsException("Error getting '" + key + "'");
                }
                
                try
                {
                    // We have a type, now create an instance.
                    obj = Activator.CreateInstance(
                        t, 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                        null,
                        n == null ? null : new object[] {n},
                        null,
                        null);
                    
                    // If the actual type is different from the type asked (that is, a subclass) 
                    // register this one in the cache too.
                    if (t != type)
                    {
                        HttpRuntime.Cache.Insert(
                            "#Glue.Lib.Configuration#Cached#" + key + "#" + t, 
                            obj, 
                            new CacheDependency(null, _rootdependency)
                            );
                    }

                    // Add to type cache.
                    HttpRuntime.Cache.Insert(
                        "#Glue.Lib.Configuration#Cached#" + key + "#" + type, 
                        obj, 
                        new CacheDependency(null, _rootdependency)
                        );
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }
            return obj;
        }

        /// <summary>
        /// Returns the root element of the merged configuration stores.
        /// </summary>
        public static XmlElement GetRoot()
        {
            XmlElement root = (XmlElement)HttpRuntime.Cache["#Glue.Lib.Configuration#"];
            if (root != null)
                return root;
            StringCollection dependencies = new StringCollection();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<configuration/>");
            root = doc.DocumentElement;

            // Load and merge all registered stores and all files referred 
            // to from within the config files.
            foreach (string store in _stores)
            {
                Merge(root, null, store, ref dependencies);
            }
            string[] deps = new string[dependencies.Count];
            dependencies.CopyTo(deps, 0);
            HttpRuntime.Cache.Insert(
                "#Glue.Lib.Configuration#", 
                root, 
                new CacheDependency(deps)
                );
            return root;
        }

        /// <summary>
        /// Internal method for looking up the named element.
        /// </summary>
        public static XmlElement GetElement(string key)
        {
            return GetRoot().SelectSingleNode(key) as XmlElement; 
        }

        /// <summary>
        /// Internal method for loading the complete merged configuration data. Will also
        /// load external file referred to in the loaded store itself (specified with the
        /// "src" attribute).
        /// </summary>
        protected static void Merge(XmlElement root, string baseDirectory, string store, ref StringCollection dependencies)
        {
            string fpath = GetFilePath(store);
#if DEBUG
            Console.WriteLine("Configuration: trying: " + fpath);
#endif
            fpath = Environment.ExpandEnvironmentVariables(fpath);
            if (baseDirectory == null)
                fpath = Glue.Lib.IO.Search.Where(fpath, AppDomain.CurrentDomain.BaseDirectory, ".", "..");
            else
                fpath = Path.Combine(baseDirectory, fpath);
            if (fpath == null)
            {
#if DEBUG
                Console.WriteLine("Configuration: not found: " + fpath);
#endif
                return;
            }
            fpath = Path.GetFullPath(fpath); //.ToLower();
#if DEBUG
            Console.WriteLine("Configuration: loading: " + fpath);
#endif
            string xpath = GetXmlPath(store);

            if (File.Exists(fpath) && !dependencies.Contains(fpath))
            {
                dependencies.Add(fpath);
                XmlElement other = Load(fpath, xpath);
                if (other.HasAttribute("src"))
                {
                    Merge(root, Path.GetDirectoryName(fpath), other.GetAttribute("src"), ref dependencies);
                }
                Merge(root, other, true);
            }
        }

        /// <summary>
        /// CacheItemRemoved
        /// </summary>
        protected static void CacheItemRemoved(string key, object obj, CacheItemRemovedReason reason)
        {
            IDisposable disposable = obj as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

        /// <summary>
        /// Returns value of given attribute. Throws an exception if attribute not found,
        /// or empty ("").
        /// </summary>
        public static string GetAttr(XmlNode node, string name)
        {
            string v = GetAttr(node, name, null);
            if (v == null)
                throw new ConfigurationErrorsException("Attribute required: " + name, node);
            return v;
        }

        /// <summary>
        /// Returns value of given attribute. Returns _default if attribute not found.
        /// Throws an error if empty ("")
        /// </summary>
        public static string GetAttr(XmlNode node, string name, string _default)
        {
            return GetAttr(node, name, _default, false);
        }

        /// <summary>
        /// Returns value of given attribute. Returns _default if attribute not found.
        /// </summary>
        public static string GetAttr(XmlNode node, string name, string _default, bool allowEmpty)
        {
            if (node == null)
                return _default;
            XmlAttribute attr = node.Attributes[name];
            if (attr == null)
                return _default;
            if (attr.Value == null || attr.Value.Length == 0)
                if (!allowEmpty)
                    throw new ConfigurationErrorsException("Attribute may not be empty: " + name, node);
                else
                    return string.Empty;
            return attr.Value;
        }

        /// <summary>
        /// Returns value of given attribute. Throws an exception if attribute not found.
        /// Throws an error if attribute is empty or not a valid number.
        /// </summary>
        public static int GetAttrUInt(XmlNode node, string name)
        {
            string v = GetAttr(node, name);
            try     
            { 
                return (int) UInt32.Parse(v); 
            } 
            catch   
            {
                throw new ConfigurationErrorsException("Invalid number in: " + name, node); 
            }
        }

        /// <summary>
        /// Returns value of given attribute. Returns _default if attribute not found.
        /// Throws an error if attribute is empty or not a valid number.
        /// </summary>
        public static int GetAttrUInt(XmlNode node, string name, int _default)
        {
            string v = GetAttr(node, name, null);
            if (v == null)
                return _default;
            try
            {
                return (int) UInt32.Parse(v); 
            }
            catch   
            { 
                throw new ConfigurationErrorsException("Invalid number in: " + name, node); 
            }
        }

        /// <summary>
        /// Returns value of given attribute. Throws an exception if attribute not 
        /// found. Throws an exception if is empty or not a valid flag ('true' or 'false').
        /// </summary>
        public static bool GetAttrBool(XmlNode node, string name)
        {
            string v = GetAttr(node, name);
            if (string.Compare(v, "on", true) == 0)
                return true;
            if (string.Compare(v, "yes", true) == 0)
                return true;
            if (string.Compare(v, "true", true) == 0)
                return true;
            if (string.Compare(v, "off", true) == 0)
                return false;
            if (string.Compare(v, "no", true) == 0)
                return false;
            if (string.Compare(v, "false", true) == 0)
                return false;
            throw new ConfigurationErrorsException("Invalid boolean value in: " + name, node); 
        }

        /// <summary>
        /// Returns value of given attribute. Returns _default if not found.
        /// Throws an exception if is empty or not a valid flag ('true' or 'false').
        /// </summary>
        public static bool GetAttrBool(XmlNode node, string name, bool _default)
        {
            string v = GetAttr(node, name, null);
            if (v == null)
                return _default;
            if (string.Compare(v, "on", true) == 0)
                return true;
            if (string.Compare(v, "yes", true) == 0)
                return true;
            if (string.Compare(v, "true", true) == 0)
                return true;
            if (string.Compare(v, "off", true) == 0)
                return false;
            if (string.Compare(v, "no", true) == 0)
                return false;
            if (string.Compare(v, "false", true) == 0)
                return false;
            throw new ConfigurationErrorsException("Invalid boolean value in: " + name, node); 
        }

        /// <summary>
        /// GetAttrEnum
        /// </summary>
        public static object GetAttrEnum(XmlNode node, string name, Type enumType)
        {
            string v = GetAttr(node, name);
            return Enum.Parse(enumType, v, true);
        }

        /// <summary>
        /// GetAttrEnum
        /// </summary>
        public static object GetAttrEnum(XmlNode node, string name, Type enumType, object _default)
        {
            string v = GetAttr(node, name, null);
            if (v == null)
                return _default;
            return Enum.Parse(enumType, v, true);
        }

        /// <summary>
        /// Searches for a given type (ignore case). Returns null if not found.
        /// </summary>
        public static Type FindType(string name)
        {
            if (name == null)
                return null;

            // Official way of finding the type
            Type type = Type.GetType(name, false, true);
            if (type != null)
                return type;
        
            // Walk all assemblies for type
            int i =name.IndexOf(',');
            if (i >= 0)
                name = name.Substring(0, i);

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(name, false, true);
                if (type != null)
                    return type;
            }
            // Nothing found
            return null;
        }

        /// <summary>
        /// Searches for a type in currently loaded assemblies. You 
        /// don't need to specify a fully qualified name. 
        /// Optionally give a subclass from which the type should
        /// inherit.
        /// </summary>
        public static Type SearchType(string name, Type subclass, bool ignoreCase)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsClass && 
                        string.Compare(type.Name, name, ignoreCase) == 0 &&
                        (subclass == null || type.IsSubclassOf(subclass))
                        )
                    {
                        return type;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns value of given attribute. Throws an exception if 
        /// attribute is not found or the type cannot be resolved.
        /// </summary>
        public static Type GetAttrType(XmlNode node, string name)
        {
            return GetAttrType(node, name, (string)null);
        }

        /// <summary>
        /// Returns value of given attribute. Throws an exception if 
        /// attribute is not found or the type cannot be resolved.
        /// </summary>
        public static Type GetAttrType(XmlNode node, string name, string defaultNamespace)
        {
            string v = GetAttr(node, name);
            Type t = FindType(v);
            if (t != null)
                return t;
            if (defaultNamespace != null)
                t = FindType(defaultNamespace + "." + v);
            if (t != null)
                return t;
            throw new ConfigurationErrorsException("Cannot resolve type: " + v + " in: " + name, node);
        }

        /// <summary>
        /// Returns value of given attribute. Returns _default if no attribute specified.
        /// Throws an exception if the type cannot be found.
        /// </summary>
        public static Type GetAttrType(XmlNode node, string name, Type _default)
        {
            return GetAttrType(node, name, null, _default);
        }

        /// <summary>
        /// Returns value of given attribute. Returns _default if no attribute specified.
        /// Throws an exception if the type cannot be found.
        /// </summary>
        public static Type GetAttrType(XmlNode node, string name, string defaultNamespace, Type _default)
        {
            string v = GetAttr(node, name, null);
            if (v == null)
                return _default;
            Type t = FindType(v);
            if (t != null)
                return t;
            if (defaultNamespace != null)
                t = FindType(defaultNamespace + "." + v);
            if (t != null)
                return t;
            throw new ConfigurationErrorsException("Cannot resolve type: " + v + " in " + name, node);
        }

        public static object GetAttrInstance(XmlNode node, string name, string defaultNamespace, Type _default)
        {
            Type type = GetAttrType(node, name, defaultNamespace, null);
            if (type == null)
                return null;
            try 
            {
                return Activator.CreateInstance(type, new object[] {node});
            }
            catch 
            {
                return Activator.CreateInstance(type, true);
            }
        }

        /// <summary>
        /// Returns full path.
        /// </summary>
        public static string GetAttrPath(XmlNode node, string name, string baseDirectory, string _default)
        {
            return Path.GetFullPath(Path.Combine(baseDirectory, GetAttr(node, name, _default))).Replace('/','\\');
        }

        /// <summary>
        /// Returns a child's element inner value, or _default.
        /// </summary>
        public static string GetChildValue(XmlNode node, string child, string _default)
        {
            node = node.SelectSingleNode(child);
            if (node == null)
                return _default;
            if (node is XmlText || node is XmlCDataSection)
                return node.Value;
            node = node.FirstChild;
            while (node != null)
            {
                if (node is XmlText || node is XmlCDataSection)
                    return node.Value;
                else
                    node = node.NextSibling;
            }
            return null;
        }

        /// <summary>
        /// GetAddRemoveList.
        /// </summary>
        public static XmlElement[] GetAddRemoveList(XmlNode node, string listName, string keyAttribute)
        {
            ArrayList list = new ArrayList();
            XmlElement element = FirstChildElement(((XmlElement)node)[listName]);
            while (element != null)
            {
                switch (element.Name)
                {
                    case "add":
                        list.Add(element);
                        break;
                    case "remove":
                        if (keyAttribute != null)
                        {
                            string key = GetAttr(element, keyAttribute);
                            for (int i = list.Count; i >= 0; i--)
                                if (key == GetAttr((XmlElement)list[i], keyAttribute))
                                    list.RemoveAt(i);
                        }
                        break;
                    case "clear":
                        list.Clear();
                        break;
                    default:
                        throw new ConfigurationErrorsException("Unexpected element in list. Should be 'add','remove' or 'clear'.");
                }
                element = NextElement(element);
            }
            return (XmlElement[])list.ToArray(typeof(XmlElement));
        }

        /// <summary>
        /// IConfigurationSectionHandler.Create
        /// </summary>
        object IConfigurationSectionHandler.Create(object parent, object configContext, XmlNode section)
        {
            return section;
        }

        /// <summary>
        /// GetFilePath
        /// </summary>
        protected static string GetFilePath(string src)
        {
            int i = src.LastIndexOf("//");
            return i < 0 ? SanitizePath(src) : SanitizePath(src.Substring(0, i));
        }

        /// <summary>
        /// GetXmlPath
        /// </summary>
        protected static string GetXmlPath(string src)
        {
            int i = src.LastIndexOf("//");
            return i < 0 ? null : src.Substring(i + 1);
        }

        /// <summary>
        /// SanitizePath
        /// </summary>
        protected static string SanitizePath(string path)
        {
            if (Path.DirectorySeparatorChar == '\\')
                return path.Replace('/', '\\');
            else
                return path.Replace('\\', '/');
        }

        /// <summary>
        /// ResolvePath
        /// </summary>
        protected static string ResolvePath(string path)
        {
            return Glue.Lib.IO.Search.Where(
                Environment.ExpandEnvironmentVariables(path), 
                AppDomain.CurrentDomain.BaseDirectory,
                ".", 
                ".."
                );
        }

        /// <summary>
        /// Load configuration file. Can be a standard .NET config file
        /// or a standalone XML file. Returns the root element of the 
        /// configuration. (In a stand-alone file this will correspond to
        /// the document's root element, in a .NET config file to the 
        /// configSection as specified in the file.
        /// </summary>
        public static XmlElement Load(string fpath, string xpath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fpath);

            XmlElement root;
            if (xpath == null)
            {
                root = doc.SelectSingleNode("/configuration/configSections/section[starts-with(@type,'Glue.Lib.Configuration')]") as XmlElement;
                if (root != null)
                    root = doc.SelectSingleNode("/configuration/" + root.GetAttribute("name")) as XmlElement;
            }
            else
            {
                root = doc.SelectSingleNode(xpath) as XmlElement;
            }
            
            if (root == null)
                root = doc.DocumentElement as XmlElement;

            return root;
        }
        
        /// <summary>
        /// Merges XmlNode dest with XmlNode from.
        /// </summary>
        public static void Merge(XmlNode dest, XmlNode from, bool overwrite)
        {
            // merge attributes
            XmlElement destelm = dest as XmlElement;
            foreach (XmlAttribute attr in from.Attributes)
                if (overwrite || !destelm.HasAttribute(attr.LocalName))
                    destelm.SetAttribute(attr.LocalName, attr.Value);
            
            // walk all child elements and merge
            XmlDocument doc = dest.OwnerDocument;
            XmlElement first = FirstChildElement(dest);
            XmlElement f = FirstChildElement(from);
            while (f != null)
            {
                XmlElement d = dest[f.Name];
                if (d == null || 
                    f.HasAttribute("name") && f.GetAttribute("name") != d.GetAttribute("name") ||
                    dest.Attributes.Count == 0 && dest.Name.EndsWith("s") ||
                    f.Name == "add" ||
                    f.Name == "clear" ||
                    f.Name == "remove")
                {
                    if (overwrite)
                        dest.AppendChild(doc.ImportNode(f, true));
                    else
                        dest.InsertBefore(doc.ImportNode(f, true), first);
                }
                else
                {
                    Merge(d, f, overwrite);
                } 
                f = NextElement(f);
            }
        }

        public static XmlElement NextElement(XmlNode node)
        {
            node = node.NextSibling;
            while (node != null && !(node is XmlElement))
                node = node.NextSibling;
            return node as XmlElement;
        }

        public static XmlElement FirstChildElement(XmlNode node)
        {
            if (node == null)
                return null;
            XmlNode first = node.FirstChild;
            while (first != null && !(first is XmlElement))
                first = first.NextSibling;
            return first as XmlElement;
        }

        public static XmlElement LastChildElement(XmlNode node)
        {
            XmlNode last = node.LastChild;
            while (last != null && !(last is XmlElement))
                last = last.PreviousSibling;
            return last as XmlElement;
        }

    }
}
