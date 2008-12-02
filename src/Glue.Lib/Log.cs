using System;
using System.Configuration;
using System.IO;
using System.Collections;
using System.Net.Sockets;
using System.Text;
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
    /// (<see cref="Glue.Lib.ConsoleAppender"/>), 
    /// a rolling log file (<see cref="Glue.Lib.FileAppender"/>),
    /// a syslog-server (<see cref="Glue.Lib.SysLogAppender"/>),
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
    ///       <add type="SysLogAppender" server="localhost" port="514" method="UDP" facility="deamon" category="TEST_CAT"/>
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
            appender.InitLevel(_level);
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
                    try   { appender.Write(level, msg); }
                    catch { }
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
    ///       <add type="SysLogAppender" server="localhost" port="514" method="UDP" facility="deamon" category="TEST_CAT"/>
    ///     </appenders>
    ///   </logging>
    /// ]]>
    /// </code>
    /// <p>
    /// A level-attribute is optional. This sets the log level for one specific appender. The default value is the global log level.
    /// </p>
    /// </remarks>
    public abstract class LogAppender
    {
        /// <summary>
        /// Log level
        /// </summary>
        public Level Level;

        protected bool _usingDefaultLevel = true;

        /// <summary>
        /// Initialize log level for this appender
        /// </summary>
        /// <param name="defaultLevel">Default log level</param>
        public virtual void InitLevel(Level defaultLevel)
        {
            if (_usingDefaultLevel)
                Level = defaultLevel;
        }

        public virtual void Close()
        {
        }
        public abstract void Write(Level level, string msg);
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
    ///       <add type="SysLogAppender" server="localhost" port="514" method="UDP" facility="deamon" category="TEST_CAT"/>
    ///     </appenders>
    ///   </logging>
    /// ]]>
    /// </code>
    /// <p>
    /// A level-attribute is optional. This sets the log level for one specific appender. The default value is the global log level.
    /// </p>
    /// </remarks>
    [System.Diagnostics.DebuggerNonUserCode]
    public class DefaultAppender : LogAppender
    {
        public DefaultAppender()
        {
        }

        public DefaultAppender(XmlNode node)
        {
            // Level
            if (Configuration.GetAttr(node, "level", "") != "")
            {
                Level = (Level)Configuration.GetAttrEnum(node, "level", typeof(Level));
                _usingDefaultLevel = false;
            }
        }

        public string FormatLine(Level level, string msg)
        {
            string line = string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0:yyyy-MM-dd HH:mm:ss,fff} [{1:X4}] {2,-8} ",
                DateTime.Now,
                System.Threading.Thread.CurrentThread.ManagedThreadId,
                Log.LevelText[(int)level]
                );
            int indent = line.Length;
            if (msg != null && msg.IndexOf('\n') >= 0)
                msg = msg.Replace("\n", "\n" + new String(' ', line.Length));
            return line + msg;
        }

        public override void Write(Level level, string msg)
        {
            if (level > Level)
                return;

            System.Diagnostics.Debug.WriteLine(FormatLine(level, msg));
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
    ///       <add type="SysLogAppender" server="localhost" port="514" method="UDP" facility="deamon" category="TEST_CAT"/>
    ///     </appenders>
    ///   </logging>
    /// ]]>
    /// </code>
    /// <p>
    /// A level-attribute is optional. This sets the log level for one specific appender. The default value is the global log level.
    /// </p>
    /// </remarks>
    [System.Diagnostics.DebuggerNonUserCode]
    public class ConsoleAppender : DefaultAppender
    {
        public ConsoleAppender(XmlNode node)
        {
            // Level
            if (Configuration.GetAttr(node, "level", "") != "")
            {
                Level = (Level)Configuration.GetAttrEnum(node, "level", typeof(Level));
                _usingDefaultLevel = false;
            }
        }

        public override void Write(Level level, string msg)
        {
            if (level > Level)
                return;

            ConsoleColor color = Console.ForegroundColor;
            if (level == Level.Error || level == Level.Fatal)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (level == Level.Warn)
                Console.ForegroundColor = ConsoleColor.Magenta;
            else if (level == Level.Info)
                Console.ForegroundColor = ConsoleColor.White;
            else
                Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(FormatLine(level, msg));
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
    /// <p>
    /// A level-attribute is optional. This sets the log level for one specific appender. The default value is the global log level.
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

            // Level
            if (Configuration.GetAttr(node, "level", "") != "")
            {
                Level = (Level)Configuration.GetAttrEnum(node, "level", typeof(Level));
                _usingDefaultLevel = false;
            }
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

        public override void Write(Level level, string msg)
        {
            if (level > Level)
                return;

            if (DateTime.Now > check)
                RollOver();
            writer.WriteLine(FormatLine(level, msg));
            writer.Flush();
        }
    }

    /// <summary>
    /// Writes log output to a syslog server.
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
    ///       <add type="SysLogAppender" server="localhost" port="514" method="UDP" facility="deamon" category="TEST_CAT"/>
    ///     </appenders>
    ///   </logging>
    /// ]]>
    /// </code>
    /// 
    /// <p>
    /// The category-attribute is mandatory. This should contain the name of the application or service. 
    /// </p>
    /// <p>
    /// The method-attribute is optional. Can be either "TCP" or "UDP". Default is UDP.
    /// </p>
    /// <p>
    /// A level-attribute is optional. This sets the log level for one specific appender. The default value is the global log level.
    /// </p>
    /// <p>
    /// The port-attribute is optional. Default value is 514 for UDP, 1468 for TCP.
    /// </p>
    /// <p>
    /// The facility-attribute is optional. Default value is 'deamon'. Other possible values:
    /// <list type="bullet">
    /// <item>Kernel </item>
    /// <item>User </item>
    /// <item>Mail </item>
    /// <item>Daemon </item>
    /// <item>Auth </item>
    /// <item>Syslog </item>
    /// <item>Lpr </item>
    /// <item>News </item>
    /// <item>UUCP </item>
    /// <item>Cron </item>
    /// <item>Local0 </item>
    /// <item>Local1 </item>
    /// <item>Local2 </item>
    /// <item>Local3 </item>
    /// <item>Local4 </item>
    /// <item>Local5 </item>
    /// <item>Local6 </item>
    /// <item>Local7 </item>
    /// </list>
    /// </p>
    /// </remarks>
    public class SysLogAppender : DefaultAppender
    {
        private enum PriorityType
        {
            Emergency = 0,      // Emergency: system is unusable
            Alert = 1,          // Alert: action must be taken immediately
            Critical = 2,       // Critical: critical conditions
            Error = 3,          // Error: error conditions
            Warning = 4,        // Warning: warning conditions
            Notice = 5,         // Notice: normal but significant condition
            Info = 6,           // Informational: informational messages
            Debug = 7           // Debug: debug-level messages
        }

        private enum FacilityType
        {
            Kernel = 0,
            User = 1,
            Mail = 2,
            Daemon = 3,
            Auth = 4,
            Syslog = 5,
            Lpr = 6,
            News = 7,
            UUCP = 8,
            Cron = 9,
            Local0 = 10,
            Local1 = 11,
            Local2 = 12,
            Local3 = 13,
            Local4 = 14,
            Local5 = 15,
            Local6 = 16,
            Local7 = 17,
        }
		
        /// <summary>
        /// Syslog server name 
        /// </summary>
        private string _server;

        /// <summary>
        /// Syslog server port
        /// </summary>
        private int _port;

        /// <summary>
        /// Facility
        /// </summary>
        private FacilityType _facility;
        
        /// <summary>
        /// ISyslogClient
        /// </summary>
        private ISyslogClient _client;

        /// <summary>
        /// Local machine name
        /// </summary>
        private string _machine;

        /// <summary>
        /// UDP or TCP
        /// </summary>
        private string _method;

        /// <summary>
        /// Category
        /// </summary>
        private string _category;

        public SysLogAppender(XmlNode node)
        {
            // syslog server
            _server = Configuration.GetAttr(node, "server");
            
            // default: UDP
            _method = Configuration.GetAttr(node, "method", "UDP").ToUpper(); 

            if (_method == "TCP")
            {
                // TCP
                _port = Configuration.GetAttrUInt(node, "port", 1468);
                _client = new TcpSyslogClient(_server, _port);
            }
            else
            {
                // UDP
                _port = Configuration.GetAttrUInt(node, "port", 514);
                _client = new UdpSyslogClient(_server, _port);
            }
            
            // facility
            _facility = (FacilityType) Configuration.GetAttrEnum(node, "facility", typeof(FacilityType), FacilityType.Daemon);

            // category
            // remove spaces
            _category = Configuration.GetAttr(node, "category");
            _category = _category.Trim().Replace(' ', '_');

            // local machine
            _machine = System.Net.Dns.GetHostName();

            // Level
            if (Configuration.GetAttr(node, "level", "") != "")
            {
                Level = (Level)Configuration.GetAttrEnum(node, "level", typeof(Level));
                _usingDefaultLevel = false;
            }
        }

        public override void Close()
        {
            if (_client != null)
            {
                try 
                { 
                    _client.Close(); 
                }
                catch { };
            }
            _client = null;
        }

        // example:
        // message = "<34>Oct 11 22:14:15 10.0.0.75 su: 'su root' failed for lonvick on /dev/pts/8";
        public override void Write(Level level, string msg)
        {
            PriorityType priority;

            // log this event with this appender?
            if (level > Level)
                return;

            switch (level)
            {
                case Level.Debug:
                    priority = PriorityType.Debug;
                    break;

                case Level.Info:
                    priority = PriorityType.Info;
                    break;

                case Level.Warn:
                    priority = PriorityType.Warning;
                    break;

                case Level.Error:
                    priority = PriorityType.Error;
                    break;

                case Level.Fatal:
                    priority = PriorityType.Alert;
                    break;
                
                default:
                    priority = PriorityType.Debug;
                    break;
            }

            // format date: first letter should be uppercase
            string date = DateTime.Now.ToString("MMM dd HH:mm:ss");
            date = string.Concat(date[0].ToString().ToUpper(), date.Substring(1));
            
            string message = string.Format("<{0}>{1} {2} {3}: {4}",
                ((int)_facility) * 8 + (int)priority,
                date,
                _machine,
                _category,
                msg
                );
            
            _client.Send(System.Text.Encoding.ASCII.GetBytes(message));
			
        }
    }

    internal interface ISyslogClient
    {
        void Close();
        void Send(byte[] buff);
    }

    internal class UdpSyslogClient : ISyslogClient  
    {
        private UdpClient _client;

        public UdpSyslogClient(string hostname, int port)
        {
            _client = new UdpClient(hostname, port);
        }

        public void Close()
        {
            _client.Close();
            _client = null;
        }

        public void Send(byte[] buff)
        {
            _client.Send(buff, buff.Length);
        }
    }

    internal class TcpSyslogClient : ISyslogClient
    {
        private TcpClient _client;

        public TcpSyslogClient(string hostname, int port)
        {
            _client = new TcpClient(hostname, port);
        }

        public void Close()
        {
            _client.Close();
            _client = null;
        }

        public void Send(byte[] buff)
        {
            _client.Client.Send(buff);
        }
    }
}
