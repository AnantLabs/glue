using System;
using System.IO;
using System.Collections;
using System.Xml;

namespace Glue.Lib
{
    /// <summary>  
    /// Log level
    /// </summary> 
    public enum Level
    {
        /// <summary>
        /// 0: Fatal
        /// </summary>
        Fatal = 0,

        /// <summary>
        /// 1: Error
        /// </summary>
        Error = 1,

        /// <summary>
        /// 2: Warn
        /// </summary>
        Warn = 2,

        /// <summary>
        /// 3: Info
        /// </summary>
        Info = 3,

        /// <summary>
        /// 4: debug
        /// </summary>
        Debug = 4
    }

    /// <summary>
    /// Logs information to different media. The Log initializes itself from the 
    /// mounted /logging configuration (see <see cref="Glue.Lib.Configuration"/>). If no configuration
    /// data is found, a default logger will be initialized.
    /// </summary>
    /// 
    /// <remarks>
    /// <p>
    /// Log messages are written to one or more <see cref="Glue.Lib.LogAppender" /> objects. Appenders 
    /// are responsible for writing the message, for example, to the console 
    /// (<see cref="Glue.Lib.ConsoleAppender"/>), a rolling log file (<see cref="Glue.Lib.FileAppender"/>),
    /// or the default output (<see cref="Glue.Lib.DefaultAppender"/>).
    /// </p>
    /// <p>
    /// The <see cref="Glue.Lib.Log" /> class expect configuration data to persist under config::/logging
    /// structured as follows:
    /// </p>
    /// <code>
    /// <![CDATA[
    ///   <logging level="Fatal|Error|Warn|Info|Debug">
    ///     <appenders>
    ///       <add type="DefaultAppender" />
    ///       <add type="ConsoleAppender" />
    ///       <add type="FileAppender" file="$exe$-$yyyy$-$mm$-$dd$.log" />
    ///     </appenders>
    ///   </logging>
    /// ]]>
    /// </code>
    /// </remarks>
    [System.Diagnostics.DebuggerNonUserCode]
    public class Log : IDisposable
    {
        // Static members

        public static string[] LevelText = {"FATAL", "ERROR", "WARN", "INFO", "DEBUG"};

        public static Log Instance
        {
            get { return Configuration.Get("logging", typeof(Log)) as Log; }
        }

        public static Level Level
        {
            get { return Instance._level; }
            set { Instance._level = value; }
        }

        public static void Fatal(object o)
        {
            Instance.Write(Level.Fatal, "" + o);
        }

        public static void Fatal(string msg)
        {
            Instance.Write(Level.Fatal, msg);
        }

        public static void Fatal(string msg, params object[] args)
        {
            Instance.Write(Level.Fatal, msg, args);
        }
    
        public static void Fatal(Exception e)
        {
            Instance.Write(Level.Fatal, e);
        }


        public static void Error(object o)
        {
            Instance.Write(Level.Error, "" + o);
        }

        public static void Error(string msg)
        {
            Instance.Write(Level.Error, msg);
        }
    
        public static void Error(string msg, params object[] args)
        {
            Instance.Write(Level.Error, msg, args);
        }
    
        public static void Error(Exception e)
        {
            Instance.Write(Level.Error, e);
        }


        public static void Warn(object o)
        {
            Instance.Write(Level.Warn, "" + o);
        }

        public static void Warn(string msg)
        {
            Instance.Write(Level.Warn, msg);
        }

        public static void Warn(string msg, params object[] args)
        {
            Instance.Write(Level.Warn, msg, args);
        }

        public static void Warn(Exception e)
        {
            Instance.Write(Level.Warn, e);
        }

        
        public static void Info(object o)
        {
            Instance.Write(Level.Info, "" + o);
        }

        public static void Info(string msg)
        {
            Instance.Write(Level.Info, msg);
        }

        public static void Info(string msg, params object[] args)
        {
            Instance.Write(Level.Info, msg, args);
        }

        public static void Info(Exception e)
        {
            Instance.Write(Level.Info, e);
        }

        public static void Debug(object o)
        {
            Instance.Write(Level.Debug, "" + o);
        }

        public static void Debug(string msg)
        {
            Instance.Write(Level.Debug, msg);
        }

        public static void Debug(string msg, params object[] args)
        {
            Instance.Write(Level.Debug, msg, args);
        }

        public static void Debug(Exception e)
        {
            Instance.Write(Level.Debug, e);
        }

        // Instance members

#if DEBUG
        private Level _level = Level.Debug;
#else
        private Level _level = Level.Warn;
#endif
        private ArrayList _appenders = new ArrayList();

        /// <summary>
        /// Protected default constructor.
        /// </summary>
        protected Log()
        {
            _appenders.Add(new DefaultAppender());
            _appenders.Add(new ConsoleAppender(null));
        }

        /// <summary>
        /// Protected constructor, to be called from Config class.
        /// </summary>
        protected Log(XmlNode node)
        {
            try   
            { 
                _level = (Level)Configuration.GetAttrEnum(node, "level", typeof(Level), _level); 
            } 
            catch { }
            foreach (XmlElement child in Configuration.GetAddRemoveList(node, "appenders", null))
            {
                try 
                {
                    AddAppender((LogAppender)Configuration.GetAttrInstance(child, "type", "Glue.Lib", null));
                }
                catch
                {
                }
            }
        }

        public void Dispose()
        {
            foreach (LogAppender appender in _appenders)
            {
                try { appender.Close(); }
                catch { }
            }
            try { _appenders.Clear(); }
            catch {}
        }

        public void ClearAppenders()
        {
            Dispose();
        }
        
        public void AddAppender(LogAppender appender)
        {
            _appenders.Add(appender);   
        }

        public void Write(Level level, Exception e)
        {
            Write(level, e.ToString(), null);
        }

        protected void Write(Level level, string msg)
        {
            Write(level, msg, null);
        }

        protected void Write(Level level, string msg, params object[] args)
        {
            try 
            {
                if (level <= _level)
                {
                    int threadid = System.Threading.Thread.CurrentThread.ManagedThreadId;
                    DateTime dt = DateTime.Now;
                    if (msg == null)
                        msg = "";
                    else if (args != null)
                        msg = string.Format(
                            System.Globalization.CultureInfo.InvariantCulture,
                            msg,
                            args
                            );
                    foreach (LogAppender appender in _appenders)
                        try   { appender.Write(level, dt, threadid, msg); }
                        catch { }
                }
            }
            catch 
            {
            }
        }
    }

    /// <summary>
    /// Abstract base class for <see cref="Glue.Lib.Log" /> Appenders. 
    /// </summary>
    /// <remarks>
    /// <p>
    /// Log messages are written to one or more <see cref="Glue.Lib.LogAppender" /> objects. Appenders 
    /// are responsible for writing the message, for example, to the console 
    /// (<see cref="Glue.Lib.ConsoleAppender"/>), a rolling log file (<see cref="Glue.Lib.FileAppender"/>),
    /// or the default output (<see cref="Glue.Lib.DefaultAppender"/>).
    /// </p>
    /// <p>
    /// The <see cref="Glue.Lib.Log" /> class expect configuration data to persist under config::/logging
    /// structured as follows:
    /// </p>
    /// <code>
    /// <![CDATA[
    ///   <logging level="Fatal|Error|Warn|Info|Debug">
    ///     <appenders>
    ///       <add type="DefaultAppender" />
    ///       <add type="ConsoleAppender" />
    ///       <add type="FileAppender" file="$exe$-$yyyy$-$mm$-$dd$.log" />
    ///     </appenders>
    ///   </logging>
    /// ]]>
    /// </code>
    /// </remarks>
    public abstract class LogAppender
    {
        public virtual void Close()
        {
        }
        public abstract void Write(Level level, DateTime dt, int threadid, string msg);
    }

    /// <summary>
    /// Writes to system debug output.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Log messages are written to one or more <see cref="Glue.Lib.LogAppender" /> objects. Appenders 
    /// are responsible for writing the message, for example, to the console 
    /// (<see cref="Glue.Lib.ConsoleAppender"/>), a rolling log file (<see cref="Glue.Lib.FileAppender"/>),
    /// or the default output (<see cref="Glue.Lib.DefaultAppender"/>).
    /// </p>
    /// <p>
    /// The <see cref="Glue.Lib.Log" /> class expect configuration data to persist under config::/logging
    /// structured as follows:
    /// </p>
    /// <code>
    /// <![CDATA[
    ///   <logging level="Fatal|Error|Warn|Info|Debug">
    ///     <appenders>
    ///       <add type="DefaultAppender" />
    ///       <add type="ConsoleAppender" />
    ///       <add type="FileAppender" file="$exe$-$yyyy$-$mm$-$dd$.log" />
    ///     </appenders>
    ///   </logging>
    /// ]]>
    /// </code>
    /// </remarks>
    [System.Diagnostics.DebuggerNonUserCode]
    public class DefaultAppender : LogAppender
    {
        public DefaultAppender()
        {
        }

        public DefaultAppender(XmlNode node)
        {
        }

        public string FormatLine(Level level, DateTime dt, int threadid, string msg)
        {
            string line = string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0:yyyy-MM-dd HH:mm:ss,fff} [{1:X4}] {2,-8} ",
                dt,
                threadid,
                Log.LevelText[(int)level]
                );
            int indent = line.Length;
            if (msg != null && msg.IndexOf('\n') >= 0)
                msg = msg.Replace("\n", "\n" + new String(' ', line.Length));
            return line + msg;
        }

        public override void Write(Level level, DateTime dt, int threadid, string msg)
        {
            System.Diagnostics.Debug.WriteLine(FormatLine(level, dt, threadid, msg));
        }
    }
    
    /// <summary>
    /// Writes log output to stdout.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Log messages are written to one or more <see cref="Glue.Lib.LogAppender" /> objects. Appenders 
    /// are responsible for writing the message, for example, to the console 
    /// (<see cref="Glue.Lib.ConsoleAppender"/>), a rolling log file (<see cref="Glue.Lib.FileAppender"/>),
    /// or the default output (<see cref="Glue.Lib.DefaultAppender"/>).
    /// </p>
    /// <p>
    /// The <see cref="Glue.Lib.Log" /> class expect configuration data to persist under config::/logging
    /// structured as follows:
    /// </p>
    /// <code>
    /// <![CDATA[
    ///   <logging level="Fatal|Error|Warn|Info|Debug">
    ///     <appenders>
    ///       <add type="DefaultAppender" />
    ///       <add type="ConsoleAppender" />
    ///       <add type="FileAppender" file="$exe$-$yyyy$-$mm$-$dd$.log" />
    ///     </appenders>
    ///   </logging>
    /// ]]>
    /// </code>
    /// </remarks>
    [System.Diagnostics.DebuggerNonUserCode]
    public class ConsoleAppender : DefaultAppender
    {
        public ConsoleAppender(XmlNode node)
        {
        }

        public override void Write(Level level, DateTime dt, int threadid, string msg)
        {
            ConsoleColor color = Console.ForegroundColor;
            if (level == Level.Error || level == Level.Fatal)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (level == Level.Warn)
                Console.ForegroundColor = ConsoleColor.Magenta;
            else if (level == Level.Info)
                Console.ForegroundColor = ConsoleColor.White;
            else
                Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(FormatLine(level, dt, threadid, msg));
        }
    }

    /// <summary>
    /// Writes log output to rolling log file.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Log messages are written to one or more <see cref="Glue.Lib.LogAppender" /> objects. Appenders 
    /// are responsible for writing the message, for example, to the console 
    /// (<see cref="Glue.Lib.ConsoleAppender"/>), a rolling log file (<see cref="Glue.Lib.FileAppender"/>),
    /// or the default output (<see cref="Glue.Lib.DefaultAppender"/>).
    /// </p>
    /// <p>
    /// The <see cref="Glue.Lib.Log" /> class expect configuration data to persist under config::/logging
    /// structured as follows:
    /// </p>
    /// <code>
    /// <![CDATA[
    ///   <logging level="Fatal|Error|Warn|Info|Debug">
    ///     <appenders>
    ///       <add type="DefaultAppender" />
    ///       <add type="ConsoleAppender" />
    ///       <add type="FileAppender" file="$exe$-$yyyy$-$mm$-$dd$.log" />
    ///     </appenders>
    ///   </logging>
    /// ]]>
    /// </code>
    /// 
    /// <p>
    /// This way, a file appender can be declared. The following values can be used:
    /// <list type="bullet">
    /// <item>$yyyy$: full year </item>
    /// <item>$yy$: year (short format)</item>
    /// <item>$mm$: month</item>
    /// <item>$dd$: day</item>
    /// <item>$exe$: process name</item>
    /// </list>
    /// </p>
    /// </remarks>
    public class FileAppender : DefaultAppender
    {
        TextWriter writer;
        string spec;
        string path;
        DateTime check = DateTime.MinValue;

        public FileAppender(XmlNode node)
        {
            spec = Configuration.GetAttr(node, "file");
        }

        public void RollOver()
        {
            Close();
            DateTime now = DateTime.Now;
            
            string exe = AppDomain.CurrentDomain.FriendlyName;
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            if (dir.EndsWith("\\bin\\"))
                dir = dir.Substring(0, dir.Length - 4);
            else if (dir.EndsWith("\\bin"))
                dir = dir.Substring(0, dir.Length - 3);

            path = spec.Replace("$yyyy$", now.Year.ToString("0000"));
            path = path.Replace("$yy$", now.Year.ToString("00"));
            path = path.Replace("$mm$", now.Month.ToString("00"));
            path = path.Replace("$dd$", now.Day.ToString("00"));
            path = path.Replace("$exe$", Path.GetFileNameWithoutExtension(exe));
            path = Path.Combine(dir, path);

            check = now.Date.AddDays(1);
            writer = new StreamWriter(path, true, System.Text.Encoding.UTF8, 256);
        }

        public override void Close()
        {
            if (writer != null)
                try { writer.Close(); }
                catch {}
            writer = null;
        }

        public override void Write(Level level, DateTime dt, int threadid, string msg)
        {
            if (DateTime.Now > check)
                RollOver();
            writer.WriteLine(FormatLine(level, dt, threadid, msg));
            writer.Flush();
        }
    }
}
