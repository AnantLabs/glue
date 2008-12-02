using System;

namespace Glue.Data
{
    // while (importer.ReadStart(out name, out columns, out types)) 
    // {
    //   while (importer.ReadRow())
    //   {
    //      Write(importer[0]);
    //      Write(importer[1]);
    //      Write(importer[2]);
    //   }
    // }
    public interface IDataImporter
    {
        bool ReadStart();
        bool ReadRow();
        object GetValue(int index);
        object GetValue(string name);
        string Name { get; }
        string[] Columns { get; }
        Type[] Types { get; }
    }
}
