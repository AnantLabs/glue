using System;
using System.IO;

namespace Glue.Data
{
    // if (exporter.WriteStart("mytable", new string[]{"id","name"}, new Type[]{Int32,String}))
    // {
    //   exporter.while (reader.Read()) 
    //   {
    //     exporter[0] = reader[0];
    //     exporter[1] = reader[1];
    //     WriteRow();
    //   }
    // }
    // WriteEnd();
    public interface IDataExporter 
    {
        void WriteStart(string name, string[] columns, Type[] types);
        void WriteRow();
        void WriteEnd();
        void SetValue(int index, object value);
        void SetValue(string name, object value);
    }
}
