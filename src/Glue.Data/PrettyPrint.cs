using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;

namespace Glue.Data
{
    public class PrettyPrint
    {
        public static void Print(TextWriter output, IDataReader reader)
        {
            int[] widths = new int[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
            {
                widths[i] = Math.Max(15, reader.GetName(i).Length);
                output.Write(reader.GetName(i).PadRight(widths[i]));
                output.Write(" | ");
            }
            output.WriteLine();
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    output.Write(reader[i].ToString().PadRight(widths[i]));
                    output.Write(" | ");
                }
                output.WriteLine();
            }
        }
    }
}
