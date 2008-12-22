using System;
using System.IO;
using System.Collections;
using Glue.Lib;
using Glue.Data;
using Glue.Data.Schema;

namespace Glue.Data.Schema
{
	/// <summary>
	/// Summary description for SchemaDiff.
	/// </summary>
	public class SchemaDiff
	{
		public SchemaDiff()
		{
		}

        public static void Compare(Database from, Database dest, TextWriter output)
        {
            DiffResult tables = Compare(from.Tables, dest.Tables);
            DiffResult views = Compare(from.Views, dest.Views);

            foreach (Table e in tables.Added)
                output.WriteLine("Table.Add: " + e.Name);
            foreach (Table e in tables.Removed)
                output.WriteLine("Table.Removed: " + e.Name);
            foreach (DiffItem e in tables.Changed)
                Compare((Table)e.From, (Table)e.Dest, output);

            foreach (View e in views.Added)
                output.WriteLine("View.Add: " + e.Name);
            foreach (View e in views.Removed)
                output.WriteLine("View.Removed: " + e.Name);
            foreach (DiffItem e in views.Changed)
                Compare((View)e.From, (View)e.Dest, output);
        }
        
        public static void Compare(Table from, Table dest, TextWriter output)
        {
            output.WriteLine("Table.Change: " + dest.Name);
            DiffResult columns = Compare(from.Columns, dest.Columns);
            DiffResult keys = Compare(from.Keys, dest.Keys);
            DiffResult indexes = Compare(from.Indexes, dest.Indexes);
            DiffResult constraints = Compare(from.Constraints, dest.Constraints);
            DiffResult triggers = Compare(from.Triggers, dest.Triggers);
            
            foreach (Column e in columns.Added)
                output.WriteLine("  Columns.Add " + e.Name + " " + e.DataType + " " + e.Size + " " + e.NativeType);
            foreach (DiffItem item in columns.Changed)
            {
                Column f = (Column)item.From;
                Column d = (Column)item.Dest;
                output.WriteLine(
                    "  Columns.Change " + d.Name + " " + d.DataType + " " + d.Size + " " + d.NativeType + " " + d.DefaultValue +
                    "  (was: " + f.DataType + " " + f.Size + " " + f.NativeType + " " + f.DefaultValue + ")"
                );
            }
            foreach (Column e in columns.Removed)
                output.WriteLine("  Columns.Remove " + e.Name);

            foreach (Key e in keys.Added)
                output.WriteLine("  Keys.Add " + e.Name + " ");
            foreach (DiffItem e in keys.Changed)
            {
                Key f = (Key)e.From;
                Key d = (Key)e.Dest;
                output.WriteLine(
                    "  Keys.Change " + d.Name + ": " + StringHelper.Join(",", d.MemberColumns) +
                    "  (was: " + StringHelper.Join(",", f.MemberColumns) + ")"
                    );
            }
            foreach (Key e in keys.Removed)
                output.WriteLine("  Keys.Remove " + e.Name + " ");

            foreach (Index e in indexes.Added)
                output.WriteLine("  Indexes.Add " + e.Name);
            foreach (DiffItem e in indexes.Changed)
            {
                Index f = (Index)e.From;
                Index d = (Index)e.Dest;
                output.WriteLine(
                    "  Indexes.Change " + d.Name + ": " + StringHelper.Join(",", d.MemberColumns) +
                    "  (was: " + StringHelper.Join(",", f.MemberColumns) + ")"
                    );
            }
            foreach (Index e in indexes.Removed)
                output.WriteLine("  Indexes.Remove " + e.Name);

            foreach (Constraint e in constraints.Added)
                output.WriteLine("  Constraints.Add " + e.Name);
            foreach (DiffItem e in constraints.Changed)
            {
                Constraint f = (Constraint)e.From;
                Constraint d = (Constraint)e.Dest;
                output.WriteLine("  Constraints.Change " + d.Name + " ");
            }
            foreach (Constraint e in constraints.Removed)
                output.WriteLine("  Constraints.Remove " + e.Name);
        }
        
        public static void Compare(View from, View dest, TextWriter output)
        {
            output.WriteLine("View.Change: ", from.Name, dest.Name);
            DiffResult columns = Compare(from.Columns, dest.Columns);
            
            foreach (Column e in columns.Added)
                output.WriteLine("  Columns.Add " + e.Name + " " + e.DataType + " " + e.Size + " " + e.NativeType);
            foreach (DiffItem item in columns.Changed)
            {
                Column f = (Column)item.From;
                Column d = (Column)item.Dest;
                output.WriteLine(
                    "  Columns.Change " + d.Name + " " + d.DataType + " " + d.Size + " " + d.NativeType + " " + d.DefaultValue +
                    "  (was: " + f.DataType + " " + f.Size + " " + f.NativeType + " " + f.DefaultValue + ")"
                );
            }
            foreach (Column e in columns.Removed)
                output.WriteLine("  Columns.Remove " + e.Name);
            string s1 = from.Text.Trim();
            string s2 = dest.Text.Trim();
            if (string.Compare(s1, s2) != 0)
                output.WriteLine("  View.Change: " + s2);
        }

        class DiffItem
        {
            public object From;
            public object Dest;
            public DiffItem(object from, object dest) { From = from; Dest = dest; }
        }

        class DiffResult
        {
            public ArrayList Added = new ArrayList();
            public ArrayList Removed = new ArrayList();
            public ArrayList Changed = new ArrayList();
            public ArrayList Equal = new ArrayList();
        }
            
        static DiffResult Compare(IList fromList, IList destList)
        {
            DiffResult result = new DiffResult();
            foreach (SchemaObject destItem in destList)
            {
                SchemaObject fromItem = SchemaObject.Find((SchemaObject[])fromList, destItem.Name);
                if (fromItem == null)
                    result.Added.Add(destItem);
                else if (fromItem.IsEq(destItem))
                    result.Equal.Add(destItem);
                else
                    result.Changed.Add(new DiffItem(fromItem, destItem));
            }
            foreach (SchemaObject fromItem in fromList)
            {
                SchemaObject destItem = SchemaObject.Find((SchemaObject[])destList, fromItem.Name);
                if (destItem == null)
                    result.Removed.Add(fromItem);
            }
            return result;
        }
	}
}
