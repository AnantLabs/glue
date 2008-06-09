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

    /// <summary>
    /// Exports the contents of a table. (Typically to a file/stream like object you pass to the constructor).
    /// </summary>
    public interface IDataExporter 
    {
        /// <summary>
        /// Declares table schema information like table and column names, to be used in the Set...() and Write...() methods
        /// </summary>
        /// <param name="name">table name</param>
        /// <param name="columns">Column names, e.g. {"Id", "name", "birthdate"}</param>
        /// <param name="types">Types for the columns in 'columns', e.g. {Int32, String, DateTime}</param>
        void WriteStart(string name, string[] columns, Type[] types);
        
        /// <summary>
        /// Writes current row.
        /// </summary>
        void WriteRow();

        /// <summary>
        /// "Closes" the table (e.g. write closing tags). You should call this method after writing all the rows with WriteRow().
        /// </summary>
        void WriteEnd();

        /// <summary>
        /// Sets value of a column in the current row. After setting the values of all columns, call WriteRow().
        /// </summary>
        /// <param name="index">Column number (zero-based).</param>
        /// <param name="value">Value to set in the current row. The object should have the type you declared in WriteStart().</param>
        void SetValue(int index, object value);
        
        /// <summary>
        /// Sets value of a column in the current row. After setting the values of all columns, call WriteRow().
        /// </summary>
        /// <param name="name">Column name. Should be one of the values of the columns[] parameter you passed to WriteStart().</param>
        /// <param name="value">Value to set in the current row. The object should have the type you declared in WriteStart().</param>
        void SetValue(string name, object value);
    }
}
