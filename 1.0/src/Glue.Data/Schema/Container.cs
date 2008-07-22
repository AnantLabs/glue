using System;
using System.Data;
using System.Xml;
using System.Collections;

namespace Glue.Data.Schema
{
    /// <summary>
    /// Base class for Table and View.
    /// </summary>
    public class Container : SchemaObject
    {
        private Column[] columns;
        private Index[] indexes;
        private Trigger[] triggers;
        private Hashtable lookup;
        
        public Container(Database database, string name) : base(database, name)
        {
        }

        internal Container(Database database, XmlElement element) : base(database, element)
        {
            ArrayList list = new ArrayList();
            foreach (XmlElement e in element.SelectNodes("./columns/column"))
            {
                list.Add(new Column(this, e));
            }
            columns = (Column[])list.ToArray(typeof(Column));

            list = new ArrayList();
            foreach (XmlElement e in element.SelectNodes("./indexes/index"))
            {
                list.Add(new Index(this, e));
            }
            indexes = (Index[])list.ToArray(typeof(Index));

            list = new ArrayList();
            foreach (XmlElement e in element.SelectNodes("./triggers/trigger"))
            {
                list.Add(new Trigger(this, e));
            }
            triggers = (Trigger[])list.ToArray(typeof(Trigger));
        }

        public Column[] Columns
        {
            get 
            { 
                if (columns == null) 
                    columns = Database.Provider.GetColumns(this);
                return columns; 
            }
        }
        
        public Index[] Indexes
        {
            get 
            { 
                if (indexes == null)
                    indexes = Database.Provider.GetIndexes(this);
                return indexes; 
            }
        }
        
        public Trigger[] Triggers
        {
            get 
            { 
                if (triggers == null)
                    triggers = Database.Provider.GetTriggers(this);
                return triggers; 
            }
        }

        public override bool IsEq(object obj)
        {
            if (!base.IsEq(obj))
                return false;
            Container other = obj as Container;
            if (other == null)
                return false;
            return IsEqList(this.Columns, other.Columns);
        }

        public Column FindColumn(string name)
        {
            if (lookup == null)
                GenerateLookup();
            return (Column)lookup[name];
        }

        private void GenerateLookup()
        {
            lookup = new Hashtable(Columns.Length);
            foreach (Column c in Columns)
                lookup[c.Name] = c;
        }
    }
}
