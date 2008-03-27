using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Glue.Lib;
using Glue.Data.Schema;

namespace Glue.Data.Providers.Sql
{
    /// <summary>
    /// SqlSchemaProvider.
    /// </summary>
    public class SqlSchemaProvider : ISchemaProvider
    {
        string server = null;
        string username = null;
        string password = null;

        /// <summary>
        /// SqlSchemaProvider
        /// </summary>
        public SqlSchemaProvider(string server, string username, string password)
        {
            this.server = server;
            this.username = username;
            this.password = password;
        }

        /// <summary>
        /// Initialisation from config.
        /// </summary>
        protected SqlSchemaProvider(XmlNode node)
        {
            this.server   = Configuration.GetAttr(node, "server");
            this.username = Configuration.GetAttr(node, "username", null);
            this.password = Configuration.GetAttr(node, "password", null);
        }

        /*
        /// <summary>
        /// Initialize the provider. Returns true if successful, false on invalid 
        /// credentials, throws an exceptionon all other errors.
        /// </summary>
        public bool Initialize(string connectionString)
        {
            this.connectionString = connectionString;
            SqlConnection connection = new SqlConnection(this.connectionString);
            try
            {
                connection.Open();
            }
            catch (SqlException e)
            {
                if (e.Number == 18456) // Access denied
                    return false;
                throw;
            }
            connection.Close();
            return true;
        }
        */

        /// <summary>
        /// Scheme identifier of this provider
        /// </summary>
        public string Scheme 
        { 
            get { return "sql"; }
        }
        
        /// <summary>
        /// GetDatabase
        /// </summary>
        public Database GetDatabase(string name)
        {
            return new Database(this, name);
        }

        /// <summary>
        /// GetDatabase
        /// </summary>
        public Database[] GetDatabases()
        {
            ArrayList list = new ArrayList();
            using (SqlDataReader reader = ExecuteReader(null, "SELECT name,dbid FROM sysdatabases WHERE name <> 'msdb' AND name <> 'master' AND name <> 'tempdb' AND name <> 'model'"))
                while (reader.Read())
                    list.Add(new Database(this, (string)reader[0]));
            return (Database[])list.ToArray(typeof(Database));
        }

        /// <summary>
        /// GetTables
        /// </summary>
        public Table[] GetTables(Database database)
        {
            ArrayList list = new ArrayList();
            using (SqlDataReader reader = ExecuteReader(database, @"
SELECT 
    id,name 
FROM 
    sysobjects 
WHERE 
    xtype='U'
    AND name!='dtproperties'
ORDER BY
    name" 
                       ))
            {
                while (reader.Read())
                {
                    Table t = new Table(database, (string)reader["name"]);
                    t.Id = (int)reader["id"];
                    list.Add(t);
                }
            }
            return (Table[])list.ToArray(typeof(Table));
        }

        /// <summary>
        /// GetViews
        /// </summary>
        public View[] GetViews(Database database)
        {
            ArrayList list = new ArrayList();
            using (SqlDataReader reader = ExecuteReader(database, @"
SELECT 
    id,name 
FROM 
    sysobjects 
WHERE 
    xtype='V' 
    AND name!='syssegments' AND name!='sysconstraints' AND name!='sysalternates' AND uid!=3
ORDER BY
    name"
                       ))
            {
                while (reader.Read())
                {
                    View v = new View(database, (string)reader["name"]);
                    v.Id = (int)reader["id"];
                    list.Add(v);
                }
            }
            return (View[])list.ToArray(typeof(View));
        }

        /// <summary>
        /// GetProcedures
        /// </summary>
        public Procedure[] GetProcedures(Database database)
        {
            ArrayList list = new ArrayList();
            using (SqlDataReader reader = ExecuteReader(database, @"
SELECT 
    id,name 
FROM 
    sysobjects 
WHERE 
    (xtype='P' OR xtype='FN' OR xtype='TF' OR xtype='IF')
    AND LEFT(name,3) !='dt_'
ORDER BY
    name"
                       ))
            {
                while (reader.Read())
                {
                    Procedure p = new Procedure(database, (string)reader["name"]);
                    p.Id = (int)reader["id"];
                    list.Add(p);
                }
            }
            return (Procedure[])list.ToArray(typeof(Procedure));
        }

        /// <summary>
        /// GetColumns
        /// </summary>
        public Column[] GetColumns(Container container)
        {
            ArrayList list = new ArrayList();
            int ver = GetMajorVersion();
            string sql;
            if (ver == 8)
            {
                // Only SQL Server 8(=2000) has column description (and sysproperties table)
                sql = @"
SELECT 
    c.colid,c.name,c.xtype,c.xusertype,c.xprec,c.xscale,c.length,t.name AS xusertypename,
    c.iscomputed,c.isnullable,c.isoutparam,(colstat & 1) AS isidentity, 
    '(' + cc.text + ')' as defaultval,ccc.text as expr,d.value as description
FROM 
    syscolumns c INNER JOIN systypes t ON c.xusertype=t.xusertype 
    LEFT OUTER JOIN sysproperties d ON d.id=c.id AND d.smallid=c.colid
    LEFT OUTER JOIN syscomments cc ON c.cdefault=cc.id
    LEFT OUTER JOIN syscomments ccc ON c.id=ccc.id AND c.colid=ccc.number 
WHERE 
    c.id=" + container.Id + @"
ORDER BY
    c.colorder";
            }
            else if (ver == 7 || ver == 9)
            {
                // This one is for SQL Server 7 and 9 (2005) // 
                sql = @"
SELECT 
    c.colid,c.name,c.xtype,c.xusertype,c.xprec,c.xscale,c.length,t.name AS xusertypename,
    c.iscomputed,c.isnullable,c.isoutparam,(colstat & 1) AS isidentity, 
    cc.text as defaultval,ccc.text as expr,NULL as description
FROM 
    syscolumns c INNER JOIN systypes t ON c.xusertype=t.xusertype 
    LEFT OUTER JOIN syscomments cc ON c.cdefault=cc.id
    LEFT OUTER JOIN syscomments ccc ON c.id=ccc.id AND c.colid=ccc.number 
WHERE 
    c.id=" + container.Id + @"
ORDER BY
    c.colorder";
            }
            else
                throw new NotImplementedException("Version " + ver + " is not supported.");

            using (SqlDataReader reader = ExecuteReader(container.Database, sql))
            {
                while (reader.Read())
                {
                    int size = (Int16)reader["length"];
                    switch ((byte)reader["xtype"])
                    {
                        case 34: // text
                        case 35: // image
                        case 99: // ntext
                            size = 0;
                            break;
                        case 231: //nvarchar
                        case 239: //nchar
                            size = size / 2;
                            break;
                        default:
                            break;
                    }
                    Column c = new Column(
                        container,
                        (string)reader["name"],
                        GetDataType((Byte)reader["xtype"]),
                        NullConvert.ToString(reader["xusertypename"]),
                        (Int32)reader["isnullable"] == 1,
                        (Byte)reader["xprec"],
                        (Byte)reader["xscale"],
                        size,
                        NullConvert.ToString(reader["defaultval"]),
                        NullConvert.ToString(reader["description"]),
                        (Int32)reader["isidentity"] == 1,
                        (Int32)reader["iscomputed"] == 1,
                        NullConvert.ToString(reader["expr"])
                        );
                    c.Id = (Int16)reader["colid"];
                    list.Add(c);
                }
            }
            return (Column[])list.ToArray(typeof(Column));
        }

        /// <summary>
        /// GetIndexes
        /// </summary>
        public Index[] GetIndexes(Container container)
        {
            // status bits
            //
            //        0 =       0h = nonclustered
            //        1 =       1h = ignore duplicate keys
            //        2 =       2h = unique
            //        4 =       4h = ignore duplicate rows
            //       16 =      10h = clustered
            //       32 =      20h = hypothetical
            //       64 =      40h = statistics
            //     2048 =     800h = primary key
            //     4096 =    1000h = unique key
            //  8388608 =  800000h = auto create
            // 16777216 = 1000000h = stats no recompute
            //
            ArrayList list = new ArrayList();
            using (SqlDataReader outer = ExecuteReader(container.Database, @"
SELECT 
    id,indid,name,status
FROM 
    sysindexes
WHERE 
    id={0} AND 
    indid < 255 AND 
	(status & 64) = 0 AND
	(status & 8388608) = 0 AND
	(status & 16777216)= 0
ORDER BY    
    indid",
                       container.Id
                       ))
            
            {
                while (outer.Read())
                {
                    ArrayList cols = new ArrayList();
                    using (SqlDataReader inner = ExecuteReader(container.Database, @"
SELECT 
    c.name,k.colid,k.id,k.indid,k.keyno
FROM 
    sysindexkeys k
    JOIN syscolumns c ON c.id=k.id AND c.colid=k.colid 
WHERE 
    k.id={0} AND k.indid={1}
ORDER BY
    k.keyno",
                               container.Id,
                               (Int16)outer["indid"]
                               ))
                    {
                        while (inner.Read())
                        {
                            cols.Add((string)inner[0]);
                        }
                    }
                    Index i = new Index(
                        container,
                        NullConvert.ToString(outer["name"]),
                        ((int)outer["status"] & 0x0010) != 0,  // IsClustered
                        ((int)outer["status"] & 0x0800) != 0,  // IsPrimaryKey
                        ((int)outer["status"] & 0x1002) != 0,  // IsUnique
                        (string[])cols.ToArray(typeof(string))
                        );
                    i.Id = (Int16)outer["indid"];
                    list.Add(i);
                }
            }
            return (Index[])list.ToArray(typeof(Index));
        }

        /// <summary>
        /// GetTriggers
        /// </summary>
        public Trigger[] GetTriggers(Container container)
        {
            // TODO:
            return new Trigger[0];
        }

        /// <summary>
        /// GetKeys
        /// </summary>
        public Key[] GetKeys(Table table)
        {
            ArrayList keys = new ArrayList();

            // http://lucaslabs.net/blogs/lucast/archive/2004/02/04/451.aspx

            // Get primary key 
            using (SqlDataReader reader = 
                       ExecuteReader(table.Database, @"
SELECT
    opk.name as 'pk_name',
    col.name as 'pk_col',
    ot.name as 'table_name',
    ot.id as 'table_id'
FROM 
    sysobjects opk
    JOIN sysobjects ot ON opk.parent_obj=ot.id
    JOIN sysconstraints cpk ON cpk.constid=opk.id
    JOIN sysindexes i ON i.id = ot.id AND i.name=opk.name
    JOIN sysindexkeys k ON k.id = i.id AND  k.indid = i.indid
    JOIN syscolumns col ON col.id = k.id AND col.colid = k.colid
WHERE 
    ot.id={0} AND opk.xtype = 'PK' 
ORDER BY 
    opk.name, k.keyno", 
                       table.Id))
            {
                string pkname = null;
                ArrayList cols = new ArrayList();
                while (reader.Read())
                {
                    pkname = (string)reader[0];
                    cols.Add((string)reader[1]);
                }
                PrimaryKey pk = new PrimaryKey(
                    table,
                    pkname,
                    (string[])cols.ToArray(typeof(string))
                    );
                keys.Add(pk);
            }

            // Get foreign key(s)
            using (SqlDataReader outer = 
                       ExecuteReader(table.Database, @"
SELECT 
    o.id as 'fk_table_id',
    o.name as 'fk_table_name',
    o1.name as 'fk_constraint_name',
    fk.constid as 'fk_constraint_id',
    o2.id as 'pk_table_id',
    o2.name as 'pk_table_name'
FROM 
    sysobjects o 
    JOIN sysobjects o1 ON o1.parent_obj=o.id
    JOIN sysforeignkeys fk ON fk.constid=o1.id
    JOIN sysobjects o2 ON fk.rkeyid=o2.id
WHERE 
    o.id={0}
ORDER BY
    o1.name", 
                       table.Id))
            {
                while (outer.Read())
                {
                    string fkname = (string)outer[2];
                    int fkid = (int)outer[3];
                    string rtable = (string)outer[5];
                    ArrayList fcols = new ArrayList();
                    ArrayList rcols = new ArrayList();

                    // For each foreign key, get column pairs
                    using (SqlDataReader inner = 
                               ExecuteReader(table.Database, @"
SELECT 
    fk.constid as 'fk_constraint_id',
    c1.id as 'fk_table_id',c1.name as 'fk_col_name',
    c2.id as 'rk_table_id',c2.name as 'rk_col_name'
FROM 
    sysforeignkeys fk 
    LEFT JOIN syscolumns c1
    ON c1.id = fk.fkeyid
    AND c1.colid = fk.fkey
    LEFT JOIN syscolumns c2
    ON c2.id = fk.rkeyid
    AND c2.colid = fk.rkey
WHERE 
    fk.constid={0}
ORDER BY 
    fk.keyno", 
                               fkid))
                    {
                        while (inner.Read())
                        {
                            fcols.Add((string)inner[2]);
                            rcols.Add((string)inner[4]);
                        }
                    }
                    ForeignKey fk = new ForeignKey(
                        table,
                        fkname,
                        (string[])fcols.ToArray(typeof(string)),
                        rtable,
                        (string[])rcols.ToArray(typeof(string))
                        );
                    keys.Add(fk);
                }
            }

            // Get unique keys
            /*
            using (SqlDataReader outer = ExecuteReader(@"
SELECT
    opk.name as 'pk_name',
    col.name as 'pk_col',
    ot.name as 'table_name',
    ot.id as 'table_id'
FROM 
    sysobjects opk
    JOIN sysobjects ot ON opk.parent_obj=ot.id
    JOIN sysconstraints cpk ON cpk.constid=opk.id
    JOIN sysindexes i ON i.id = ot.id AND i.name=opk.name
    JOIN sysindexkeys k ON k.id = i.id AND  k.indid = i.indid
    JOIN syscolumns col ON col.id = k.id AND col.colid = k.colid
WHERE 
    ot.id={0} AND opk.xtype = 'UQ' 
ORDER BY 
    opk.name, k.keyno"
                ))
            {
                while (reader.Read())
                {
                    string pkname = null;
                    ArrayList cols = new ArrayList();
                    while (reader.Read())
                    {
                        pkname = (string)reader[0];
                        cols.Add((string)reader[1]);
                    }
                    PrimaryKey pk = new PrimaryKey(
                        table,
                        pkname,
                        (string[])cols.ToArray(typeof(string))
                        );
                    keys.Add(pk);
                }
            }
            */            

            return (Key[])keys.ToArray(typeof(Key));
        }

        /// <summary>
        /// GetParameters
        /// </summary>
        public Parameter[] GetParameters(Procedure procedure)
        {
            ArrayList list = new ArrayList();
            int ver = GetMajorVersion();
            string sql;
            if (ver == 8)
            {
                // Only SQL Server 8(=2000) has column description (and sysproperties table)
                sql = @"
SELECT 
    c.colid,c.name,c.xtype,c.xusertype,c.xprec,c.xscale,c.length,c.iscomputed,
    c.isnullable,c.isoutparam,t.name AS xusertypename,cc.text as defaultval,d.value as description
FROM 
    syscolumns c 
    INNER JOIN systypes t ON c.xusertype=t.xusertype 
    LEFT OUTER JOIN syscomments cc ON c.cdefault=cc.id
    LEFT OUTER JOIN sysproperties d ON d.id=c.id AND d.smallid=c.colid
WHERE 
    c.id=" + procedure.Id + @"
ORDER BY
    colorder";
            }
            else
            {
                // SQL Server 7 and 9(=2005)
                sql = @"
SELECT 
    c.colid,c.name,c.xtype,c.xusertype,c.xprec,c.xscale,c.length,c.iscomputed,
    c.isnullable,c.isoutparam,t.name AS xusertypename,cc.text as defaultval,NULL as description
FROM 
    syscolumns c 
    INNER JOIN systypes t ON c.xusertype=t.xusertype 
    LEFT OUTER JOIN syscomments cc ON c.cdefault=cc.id
WHERE 
    c.id=" + procedure.Id + @"
ORDER BY
    colorder";
            }
            using (SqlDataReader reader = ExecuteReader(procedure.Database, sql))
            {
                while (reader.Read())
                {
                    int size = (Int16)reader["length"];
                    // if text or image or ntext, set size=0
                    if ((Byte)reader["xtype"] == 34 || (Byte)reader["xtype"] == 35 || (Byte)reader["xtype"] == 99)
                        size = 0;
                    Parameter c = new Parameter(
                        procedure.Database,
                        (string)reader["name"],
                        GetDataType((Byte)reader["xtype"]), 
                        (string)reader["xusertypename"],
                        (Int32)reader["isnullable"] == 1,
                        (Byte)reader["xprec"],
                        (Byte)reader["xscale"],
                        size,
                        NullConvert.ToString(reader["defaultval"]),
                        NullConvert.ToString(reader["description"]),
                        (Int32)reader["isoutparam"] == 1,
                        false  // TODO: RESULT
                        );
                    c.Id = (Int16)reader["colid"];
                    list.Add(c);
                }
            }
            return (Parameter[])list.ToArray(typeof(Parameter));
        }

        /// <summary>
        /// GetViewText
        /// </summary>
        public string GetViewText(View view)
        {
            return NullConvert.ToString(ExecuteScalar(view.Database, @"
SELECT 
    text
FROM 
    syscomments
WHERE 
    id={0}",
                view.Id
                ));
        }
        
        /// <summary>
        /// GetProcedureText
        /// </summary>
        public string GetProcedureText(Procedure procedure)
        {
            return NullConvert.ToString(ExecuteScalar(procedure.Database, @"
SELECT 
    text
FROM 
    syscomments
WHERE 
    id={0}",
                procedure.Id
                ));
        }

        /// <summary>
        /// Import
        /// </summary>
        public void Import(Table table, IDataImporter reader, ImportMode mode)
        {
            Console.WriteLine("Table: " + table.Name);

            // Construct the SQL command
            StringBuilder sql = new StringBuilder();
            
            // Check on identity insert
            bool identity = false;
            foreach (Column c in table.Columns)
                if (c.Identity)
                    identity = true;
            if (identity)
                sql.AppendFormat("SET IDENTITY_INSERT [{0}] ON\r\n", table.Name);

            // for Mode=Incremental and Mode=Update:
            // if not exists
            //   insert
            if (mode == ImportMode.Incremental || mode == ImportMode.Update)
            {
                sql.AppendFormat("IF NOT EXISTS (SELECT [{0}] FROM [{1}] WHERE (", table.Columns[0].Name, table.Name);
                foreach (Column c in table.PrimaryKey.MemberColumns)
                    sql.AppendFormat("[{0}]=@{0} AND ", c.Name);
                sql.Length = sql.Length - 5;
                sql.Append("))\r\n    ");

                sql.AppendFormat("INSERT [{0}] (", table.Name);
                foreach (Column c in table.Columns)
                    if (!c.Computed)
                        sql.AppendFormat("[{0}], ", c.Name);
                sql.Length = sql.Length - 2;
                sql.Append(") VALUES (");
                foreach (Column c in table.Columns)
                    if (!c.Computed)
                        sql.AppendFormat("@{0}, ", c.Name);
                sql.Length = sql.Length - 2;
                sql.Append(")\r\n");
            }

            // check if we can update any columns at all
            int updatecols = 0;
            foreach (Column c in table.Columns)
                if (Array.IndexOf(table.PrimaryKey.MemberColumns, c) < 0)
                    if (!c.Computed)
                        updatecols++;

            //bool skip = false;
            if (updatecols == 0)
            {
                // there are no updateable columns, so Mode=Refresh is useless
                //if (mode == ImportMode.Freshen)
                //    skip = true;
            }
            else
            {
                // if we're in Mode=Update we need the ELSE after the IF EXISTS clause
                if (mode == ImportMode.Update)
                    sql.Append("ELSE\r\n");

                // create the UPDATE WHERE statement
                if (mode == ImportMode.Update || mode == ImportMode.Freshen)
                {
                    sql.AppendFormat("UPDATE [{0}] SET ", table.Name);
                    foreach (Column c in table.Columns)
                        if (Array.IndexOf(table.PrimaryKey.MemberColumns, c) < 0)
                            if (!c.Computed)
                                sql.AppendFormat("[{0}]=@{0}, ", c.Name);
                    sql.Length = sql.Length - 2;
                    sql.Append(" WHERE (");
                    foreach (Column c in table.PrimaryKey.MemberColumns)
                        sql.AppendFormat("[{0}]=@{0} AND ", c.Name);
                    sql.Length = sql.Length - 5;
                    sql.Append(")\r\n");
                }
            }

            SqlCommand command = CreateCommand(table.Database, sql.ToString());
            foreach (Column c in table.Columns)
                command.Parameters.Add("@" + c.Name, GetNativeType(c.DataType, c.Size), c.Size);

            // Perform the actual import
            command.Connection.Open();
            try 
            {
                while (reader.ReadRow())
                {
                    for (int i = 0; i < reader.Columns.Length; i++)
                    {
                        object value = reader.GetValue(i);
                        command.Parameters["@" + reader.Columns[i]].Value = value == null ? DBNull.Value : value;
                    }
                    try
                    {
                        int n = command.ExecuteNonQuery();
                        if (n > 0)
                            Console.WriteLine("  Importing: " + command.Parameters[0].Value.ToString());
                    }
                    catch (SqlException e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
            finally
            {
                command.Connection.Close();
            }
        }

        /// <summary>
        /// Export
        /// </summary>
        public void Export(Container container, IDataExporter writer)
        {
            // Set column names and types
            string[] names = new string[container.Columns.Length];
            Type[] types = new Type[container.Columns.Length];
            for (int i = 0; i < container.Columns.Length; i++)
            {
                names[i] = container.Columns[i].Name;
                types[i] = container.Columns[i].SystemType;
            }
            writer.WriteStart(container.Name, names, types);

            // Export rows
            using (SqlDataReader reader = ExecuteReader(container.Database, "SELECT * FROM [" + container.Name + "]"))
            {
                while (reader.Read())
                {
                    for (int i = 0; i < names.Length; i++)
                    {
                        object value = reader[names[i]];
                        writer.SetValue(i, value);
                    }
                    writer.WriteRow();
                }
            }
            writer.WriteEnd();
        }

        /*
        public void ExportScript(Container container, TextWriter output)
        {
            // Check on identity insert
            bool identity = false;
            foreach (Column c in container.Columns)
                if (c.Identity)
                    identity = true;
            if (identity)
                output.WriteLine("SET IDENTITY_INSERT [{0}] ON", container.Name);

            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("INSERT [{0}] (", container.Name);
            foreach (Column c in container.Columns)
                if (!c.Computed)
                    sql.AppendFormat("[{0}], ", c.Name);
            sql.Length = sql.Length - 2;
            sql.Append(") ");
                
            StringBuilder vals = new StringBuilder();
            using (SqlDataReader reader = ExecuteReader(container.Database, "SELECT * FROM [" + container.Name + "]"))
            {
                while (reader.Read())
                {
                    vals.Length = 0;
                    vals.Append("VALUES(");
                    int ordinal = 0;
                    foreach (Column c in container.Columns)
                    {
                        if (!reader.IsDBNull(ordinal))
                        {
                            switch (c.DataType)
                            {
                                case DbType.AnsiString:
                                case DbType.AnsiStringFixedLength:
                                case DbType.String:
                                case DbType.StringFixedLength:
                                    vals.Append('\'');
                                    vals.Append(reader[ordinal].ToString().Replace("'", "''"));
                                    vals.Append('\'');
                                    break;
                                default:
                                    vals.Append(reader[ordinal]);
                                    break;
                            }
                        }
                        else
                        {
                            vals.Append("NULL");
                        }
                        vals.Append(", ");
                        ordinal++;
                    }
                    vals.Length = vals.Length - 2;
                    vals.Append(")");
                    output.Write(sql.ToString());
                    output.Write(vals.ToString());
                }
            }
        }
        */

        /// <summary>
        /// Script
        /// </summary>
        public void Script(Database database, TextWriter writer)
        {
            if (database.Name != null && database.Name != "")
            {
                writer.WriteLine("/* DATABASE */");
                writer.WriteLine("CREATE DATABASE " + database.Name);
                writer.WriteLine("USE " + database.Name);
                writer.WriteLine("GO");
                writer.WriteLine();
            }
            writer.WriteLine("/* TABLES */");
            writer.WriteLine();

            StringBuilder sql = new StringBuilder();
            foreach (Table table in database.Tables)
            {
                sql.AppendFormat("CREATE TABLE [{0}] (\r\n", table.Name);
                foreach (Column c in table.Columns)
                {
                    if (!c.Computed)
                    {
                        if (c.HasSize)
                            sql.AppendFormat("  [{0}] {1}({2})", c.Name, GetNativeType(c.DataType, c.Size), c.Size);
                        else
                            sql.AppendFormat("  [{0}] {1}", c.Name, GetNativeType(c.DataType, c.Size));
                        if (c.DefaultValue != null && c.DefaultValue.Length > 0)
                            sql.AppendFormat(" DEFAULT {0}", Filter.ToSql(c.DefaultValue));
                        if (c.Identity)
                            sql.Append(" IDENTITY (1, 1)");
                        if (c.Nullable)
                            sql.Append(" NULL");
                        else
                            sql.Append(" NOT NULL");
                        sql.Append(", \r\n");
                    }
                    else
                    {
                        sql.AppendFormat("[{0}] AS {1}, \r\n", c.Name, c.Expression);
                    }
                }
                foreach (Key k in table.Keys)
                {
                    if (k is PrimaryKey)
                    {
                        PrimaryKey pk = k as PrimaryKey;
                        sql.AppendFormat("  CONSTRAINT [{0}] PRIMARY KEY CLUSTERED (", k.Name);
                        // TODO: IsClustered
                        foreach (Column c in pk.MemberColumns)
                            sql.AppendFormat("[{0}], ", c.Name);
                        sql.Length = sql.Length - 2;
                        sql.Append("), \r\n");
                    }
                }
                sql.Length = sql.Length - 4;
                sql.Append("\r\n)\r\n");

                // TODO: TRIGGERS

                foreach (Index index in table.Indexes)
                {
                    if (!index.IsPrimaryKey)
                    {
                        if (index.IsUnique)
                            sql.Append("CREATE UNIQUE INDEX ");
                        else
                            sql.Append("CREATE INDEX ");
                        sql.AppendFormat("[{0}] ON [{1}](", index.Name, table.Name);
                        foreach (Column c in index.MemberColumns)
                            sql.AppendFormat("[{0}], ", c.Name);
                        sql.Length = sql.Length - 2;
                        sql.Append(")\r\n");
                    }
                }
                writer.WriteLine(sql.ToString());
                writer.WriteLine("GO");
                writer.WriteLine();
                sql.Length = 0;
            }

            // Foreign keys
            writer.WriteLine();
            writer.WriteLine();
            foreach (Table table in database.Tables)
            {
                sql.AppendFormat("ALTER TABLE [{0}] ADD\r\n", table.Name);
                bool yep = false;
                foreach (Key k in table.Keys)
                {
                    if (k is ForeignKey)
                    {
                        yep = true;
                        ForeignKey fk = k as ForeignKey;
                        sql.AppendFormat("  CONSTRAINT [{0}] FOREIGN KEY (", k.Name);
                        foreach (Column c in fk.MemberColumns)
                            sql.AppendFormat("[{0}], ", c.Name);
                        sql.Length = sql.Length - 2;
                        sql.AppendFormat(") REFERENCES [{0}] (", fk.ReferencedTable.Name);
                        foreach (Column c in fk.ReferencedColumns)
                            sql.AppendFormat("[{0}], ", c.Name);
                        sql.Length = sql.Length - 2;
                        sql.Append("), \r\n");
                    }
                    else
                    {
                        // TODO: UNIQUE AND CHECK
                    }
                }
                if (yep)
                {
                    sql.Length = sql.Length - 4;
                    sql.Append("\r\n\r\n");
                    writer.WriteLine(sql.ToString());
                    writer.WriteLine("GO");
                    writer.WriteLine();
                }
                sql.Length = 0;
            }

            // Views
            writer.WriteLine();
            writer.WriteLine("/* VIEWS */");
            writer.WriteLine();
            
            StringCollection order = new StringCollection();
            foreach (View view in database.Views)
                order.Add(view.Name);
                //GetViewCreationOrder(database, view, order);

            foreach (string name in order)
            {
                View view = GetViewByName(database, name);
                writer.WriteLine("IF EXISTS (SELECT * FROM dbo.sysobjects WHERE name='{0}' AND xtype='V')", view.Name);
                writer.WriteLine("  DROP VIEW [" + view.Name + "]");
                writer.WriteLine("GO");
                writer.WriteLine();
                writer.WriteLine(view.Text);
                writer.WriteLine();
                writer.WriteLine("GO");
                writer.WriteLine();
                sql.Length = 0;
                foreach (Index index in view.Indexes)
                {
                    if (index.IsUnique)
                        sql.Append("CREATE UNIQUE ");
                    else
                        sql.Append("CREATE ");
                    if (index.IsClustered)
                        sql.Append("CLUSTERED ");
                    sql.Append("INDEX ");
                    sql.AppendFormat("[{0}] ON [{1}](", index.Name, view.Name);
                    foreach (Column c in index.MemberColumns)
                        sql.AppendFormat("[{0}], ", c.Name);
                    sql.Length = sql.Length - 2;
                    sql.Append(")\r\n");
                }
                if (sql.Length > 0)
                {
                    writer.Write(sql.ToString());
                    writer.WriteLine("GO");
                }
                writer.WriteLine();
                sql.Length = 0;
            }

            writer.WriteLine();
            writer.WriteLine("/* PROCS */");
            writer.WriteLine();

            // Procedures
            foreach (Procedure proc in database.Procedures)
            {
                writer.WriteLine(proc.Text);
                writer.WriteLine();
            }
        }

        private View GetViewByName(Database database, string name)
        {
            foreach (View view in database.Views)
                if (string.Compare(name, view.Name, true) == 0)
                    return view;
            return null;
        }

        private void GetViewCreationOrder(Database database, View view, StringCollection order)
        {
            foreach (string d in ExecuteStrings(database, "SELECT DISTINCT name FROM sysdepends d,sysobjects o WHERE d.id={0} AND o.id=d.depid AND xtype='V' ORDER BY Name", view.Id))
            {
                if (!order.Contains(d))
                {
                    GetViewCreationOrder(database, GetViewByName(database, d), order);
                }
            }
            if (!order.Contains(view.Name))
                order.Add(view.Name);
        }
        
        #region SQL command helpers

        private int GetMajorVersion()
        {
            return ((int)ExecuteScalar(null, "SELECT @@microsoftversion")) >> 24;
        }

        private SqlConnection CreateConnection(Database database)
        {
            string s = "server=" + server;
            if (database != null)
                s += ";database=" + database.Name;
            else
                s += ";database=master";
            if (username != null && username != "")
                s += ";user id=" + username + ";password=" + password;
            else
                s += ";integrated security=true";
            return new SqlConnection(s);
        }

        private SqlCommand CreateCommand(Database database, string commandText)
        {
            return new SqlCommand(commandText, CreateConnection(database));
        }

        private SqlDataReader ExecuteReader(Database database, string commandText)
        {
            SqlCommand command = new SqlCommand(commandText, CreateConnection(database));
            command.Connection.Open();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        private SqlDataReader ExecuteReader(Database database, string commandText, params object[] args)
        {
            return ExecuteReader(database, string.Format(commandText, args));
        }

        private object ExecuteScalar(Database database, string commandText)
        {
            SqlCommand command = new SqlCommand(commandText, CreateConnection(database));
            using (command.Connection)
            {
                command.Connection.Open();
                return command.ExecuteScalar();
            }
        }

        private object ExecuteScalar(Database database, string commandText, params object[] args)
        {
            return ExecuteScalar(database, string.Format(commandText, args));
        }

        private int ExecuteNonQuery(Database database, string commandText)
        {
            SqlCommand command = new SqlCommand(commandText, CreateConnection(database));
            using (command.Connection)
            {
                command.Connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        private int ExecuteNonQuery(Database database, string commandText, params object[] args)
        {
            return ExecuteNonQuery(database, string.Format(commandText, args));
        }

        private string[] ExecuteStrings(Database database, string commandText)
        {
            ArrayList list = new ArrayList();
            using (SqlDataReader reader = ExecuteReader(database, commandText))
            {
                while (reader.Read())
                {
                    list.Add(reader[0]);
                }
                return (string[])list.ToArray(typeof(string));
            }
        }

        private string[] ExecuteStrings(Database database, string commandText, params object[] args)
        {
            return ExecuteStrings(database, string.Format(commandText, args));
        }

        #endregion

        private DbType GetDataType(int xtype)
        {
            switch (xtype)
            {
                case 34:     return DbType.Binary;      // image
                case 35:     return DbType.AnsiString;  // text
                case 36:     return DbType.Guid;        // uniqueidentifier
                case 48:     return DbType.SByte;       // tinyint
                case 52:     return DbType.Int16;       // smallint
                case 56:     return DbType.Int32;       // int
                case 58:     return DbType.DateTime;    // smalldatetime
                case 59:     return DbType.Single;      // real
                case 60:     return DbType.Currency;    // money
                case 61:     return DbType.DateTime;    // datetime
                case 62:     return DbType.Double;      // float
                case 98:     return DbType.Object;      // sql_variant
                case 99:     return DbType.String;      // ntext
                case 104:    return DbType.Boolean;     // bit
                case 106:    return DbType.Decimal;     // decimal
                case 108:    return DbType.Decimal;     // numeric
                case 122:    return DbType.Currency;    // smallmoney
                case 127:    return DbType.Int64;       // bigint
                case 165:    return DbType.Binary;      // varbinary
                case 167:    return DbType.AnsiString;  // varchar
                case 173:    return DbType.Binary;      // binary
                case 175:    return DbType.AnsiStringFixedLength; // char
                case 189:    return DbType.Time;        // timestamp
                case 231:    return DbType.String;      // nvarchar
                case 239:    return DbType.StringFixedLength; // nchar
            }
            throw new ArgumentException("Unknown type " + xtype);
        }

        private SqlDbType GetNativeType(DbType dbType, int size)
        {
            switch (dbType)
            {
                case DbType.AnsiString:                 return size == 0 ? SqlDbType.Text : SqlDbType.VarChar;
                case DbType.AnsiStringFixedLength:      return SqlDbType.Char;
                case DbType.Binary:                     return size == 0 ? SqlDbType.Image : SqlDbType.Binary;
                case DbType.Boolean:                    return SqlDbType.Bit;
                case DbType.Byte:                       return SqlDbType.TinyInt;
                case DbType.Currency:                   return SqlDbType.Money;
                case DbType.Date:                       return SqlDbType.DateTime;
                case DbType.DateTime:                   return SqlDbType.DateTime;
                case DbType.Decimal:                    return SqlDbType.Decimal;
                case DbType.Double:                     return SqlDbType.Float;
                case DbType.Guid:                       return SqlDbType.UniqueIdentifier;
                case DbType.Int16:                      return SqlDbType.SmallInt;
                case DbType.Int32:                      return SqlDbType.Int;
                case DbType.Int64:                      return SqlDbType.BigInt;
                case DbType.Object:                     return SqlDbType.Variant;
                case DbType.SByte:                      return SqlDbType.TinyInt;
                case DbType.Single:                     return SqlDbType.Real;
                case DbType.String:                     return size == 0 ? SqlDbType.NText : SqlDbType.NVarChar;
                case DbType.StringFixedLength:          return SqlDbType.NChar;
                case DbType.Time:                       return SqlDbType.DateTime;
                case DbType.UInt16:                     return SqlDbType.SmallInt;
                case DbType.UInt32:                     return SqlDbType.Int;
                case DbType.UInt64:                     return SqlDbType.BigInt;
                case DbType.VarNumeric:                 return SqlDbType.Binary;
            }
            throw new ArgumentException("Unknown type " + dbType.ToString());
        }

        /*
        private Type GetSystemType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:                 return typeof(String);
                case DbType.AnsiStringFixedLength:      return typeof(String);
                case DbType.Binary:                     return typeof(byte[]);
                case DbType.Boolean:                    return typeof(bool);
                case DbType.Byte:                       return typeof(byte);
                case DbType.Currency:                   return typeof(Decimal);
                case DbType.Date:                       return typeof(DateTime);
                case DbType.DateTime:                   return typeof(DateTime);
                case DbType.Decimal:                    return typeof(Decimal);
                case DbType.Double:                     return typeof(double);
                case DbType.Guid:                       return typeof(Guid);
                case DbType.Int16:                      return typeof(Int16);
                case DbType.Int32:                      return typeof(Int32);
                case DbType.Int64:                      return typeof(Int64);
                case DbType.Object:                     return typeof(object);
                case DbType.SByte:                      return typeof(SByte);
                case DbType.Single:                     return typeof(Single);
                case DbType.String:                     return typeof(String);
                case DbType.StringFixedLength:          return typeof(String);
                case DbType.Time:                       return typeof(DateTime);
                case DbType.UInt16:                     return typeof(UInt16);
                case DbType.UInt32:                     return typeof(UInt32);
                case DbType.UInt64:                     return typeof(UInt64);
                case DbType.VarNumeric:                 return typeof(byte[]);
            }
            throw new ArgumentException("Unknown type " + dbType.ToString());
        }
        */

        /*
        internal long GetDbType(string dbType)
        {
            switch (dbType)
            {
                case "bit": // 1
                    return (long) SqlDbType.Bit;
                case "tinyint": // 1
                    return (long) SqlDbType.TinyInt;
                case "smallint": // 2
                    return (long) SqlDbType.SmallInt;
                case "int": // 4
                    return (long) SqlDbType.Int;
                case "bigint": // 8
                    return (long) SqlDbType.BigInt;
                case "real": // 4
                    return (long) SqlDbType.Real;
                case "float": // 8
                    return (long) SqlDbType.Float;
                case "smalldatetime": // 4
                    return (long) SqlDbType.SmallDateTime;
                case "datetime": // 8
                    return (long) SqlDbType.DateTime;
                case "decimal":
                case "numeric":
                    return (long) SqlDbType.Decimal;
                case "nchar": // unicode (max 4000 chars)
                    return (long) SqlDbType.NChar;
                case "nvarchar":  // unicode (max 4000 chars)
                    return (long) SqlDbType.NVarChar;
                case "ntext": // unicode (max 2^32 chars)
                    return (long) SqlDbType.NText;
                case "char": // non-unicode (max 8000 chars)
                    return (long) SqlDbType.Char;
                case "varchar": // non-unicode (max 8000 chars)
                    return (long) SqlDbType.VarChar;
                case "text": // non-unicode (max 2^32 chars)
                    return (long) SqlDbType.Text;
                case "binary": // fixed size blob < 8000 bytes
                    return (long) SqlDbType.Binary;
                case "varbinary": // variable size blob < 8000 bytes 
                    return (long) SqlDbType.VarBinary;
                case "image": // 2^31 blob data
                    return (long) SqlDbType.Image;
                case "uniqueidentifier": // GUID
                    return (long) SqlDbType.UniqueIdentifier;
                    // weird type sql server seems to use when a table has been modified.. we just
                    // return NO_DBTYPE and ignore the returned value (the column has another,
                    // proper entry so we'll be called again for the same column).
                case "sysname": 
                    // fail for unsupported types (includes timestamp because its a read-only type)
                case "timestamp": // 8
                    // return (long) SqlDbType.Timestamp;
                default:
                    throw new ApplicationException("Unsupported type " + dbType);
            }
        }
        */
    }
}

