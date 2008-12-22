using System;
using System.Collections;
using System.IO;
using System.Data;
using Glue.Data;

namespace Glue.Data.Schema
{
	/// <summary>
	/// Summary description for SimpleDataExporter.
	/// </summary>
	public class XmlDataImporter : IDataImporter
	{
        public XmlDataImporter()
        {
        }

        public bool ReadStart()
        {
            return false;
        }

        public bool ReadRow()
        {
            /*
            int depth = -1;
            while (reader.Read())
            {
                if (depth != -1 && reader.Depth < depth)
                {
                    return;
                }
                bool execute = false;
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                    {
                        if ((reader.Depth == depth || -1 == depth) && "row" == reader.Name)
                        {
                            depth = reader.Depth;
                            foreach (SqlParameter parm in command.Parameters)
                            {
                                parm.Value = DBNull.Value;
                            }
                            while (reader.MoveToNextAttribute())
                            {
                                command.Parameters["@" + reader.Name].Value = reader.Value;
                            }
                            if (reader.IsEmptyElement)
                            {
                                execute = true;
                            }
                        }
                        else if (reader.Depth == depth + 1)
                        {
                            SqlParameter parm = command.Parameters["@" + reader.Name];
                            reader.MoveToContent();
                            switch (parm.DbType)
                            {
                                case DbType.Binary:
                                    parm.Value = BinHexEncoding.Decode(reader.ReadString().ToCharArray());
                                    break;
                                case DbType.AnsiString:
                                case DbType.AnsiStringFixedLength:
                                case DbType.String:
                                case DbType.StringFixedLength:
                                    parm.Value = reader.ReadString();
                                    break;
                                case DbType.Date:
                                case DbType.DateTime:
                                case DbType.Time:
                                    parm.Value = XmlConvert.ToDateTime(reader.ReadString());
                                    break;
                                case DbType.Boolean:
                                    parm.Value = XmlConvert.ToBoolean(reader.ReadString());
                                    break;
                                case DbType.Byte:
                                    parm.Value = XmlConvert.ToByte(reader.ReadString());
                                    break;
                                case DbType.Guid:
                                    parm.Value = XmlConvert.ToGuid(reader.ReadString());
                                    break;
                                case DbType.Int16:
                                    parm.Value = XmlConvert.ToInt16(reader.ReadString());
                                    break;
                                case DbType.Int32:
                                    parm.Value = XmlConvert.ToInt32(reader.ReadString());
                                    break;
                                case DbType.Int64:
                                    parm.Value = XmlConvert.ToInt64(reader.ReadString());
                                    break;
                                case DbType.UInt16:
                                    parm.Value = XmlConvert.ToUInt16(reader.ReadString());
                                    break;
                                case DbType.UInt32:
                                    parm.Value = XmlConvert.ToUInt32(reader.ReadString());
                                    break;
                                case DbType.UInt64:
                                    parm.Value = XmlConvert.ToUInt64(reader.ReadString());
                                    break;
                                case DbType.SByte:
                                    parm.Value = XmlConvert.ToSByte(reader.ReadString());
                                    break;
                                case DbType.Single:
                                    parm.Value = XmlConvert.ToSingle(reader.ReadString());
                                    break;
                                case DbType.Double:
                                    parm.Value = XmlConvert.ToDouble(reader.ReadString());
                                    break;
                                case DbType.Currency:
                                case DbType.Decimal:
                                    parm.Value = XmlConvert.ToDecimal(reader.ReadString());
                                    break;
                                default:
                                    throw new DataException("Unsupported: " + parm.DbType);
                            }
                        }
                        break; 
                    }
                    case XmlNodeType.EndElement:
                        // At the end of the row, all values have been read. 
                        // Ready to execute command.
                        if (reader.Depth == depth)
                        {
                            execute = true;
                        }
                        break;
                }
                if (!skip && execute)
                {
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
            */
            return false;
        }

        public object GetValue(int index)
        {
            return null;
        }

        public object GetValue(string name)
        {
            return null;
        }

        public string Name 
        {
            get { return null; }
        }

        public string[] Columns
        {
            get { return null; }
        }

        public Type[] Types
        {
            get { return null; }
        }
    }
}
