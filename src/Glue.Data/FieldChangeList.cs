using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Glue.Data
{
    /// <summary>
    /// List of field value changes
    /// </summary>
    /// <remarks>
    /// This class can be used, in combination with the <see cref="FieldChange"/>-class, to compute and store 
    /// changes to object field/property values. 
    /// 
    /// If the FieldChanges are to be stored in a database, the class expects a table of the following structure:
    /// <list type="bullet">
    /// <item>FieldName: string ((var)(n)char)</item>
    /// <item>OldValue: string ((var)(n)char)</item>
    /// <item>NewValue: string ((var)(n)char)</item>
    /// <item>ChangeUser: string ((var)(n)char)</item>
    /// <item>ChangeDate: datetime</item>
    /// </list>
    /// </remarks>
    public class FieldChangeList : IList<FieldChange> 
    {
        private List<FieldChange> _list = new List<FieldChange>();

        #region IList<FieldChange> Members

        public int IndexOf(FieldChange item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, FieldChange item)
        {
            if (item != null)
                _list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public FieldChange this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                _list[index] = value;
            }
        }

        #endregion

        #region ICollection<FieldChange> Members

        public void Add(FieldChange item)
        {
            if (item != null)
                _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(FieldChange item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(FieldChange[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(FieldChange item)
        {
            return _list.Remove(item);
        }

        #endregion

        #region IEnumerable<FieldChange> Members

        public IEnumerator<FieldChange> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Add IEnumerable FieldChangeList to FieldChangeList 
        /// </summary>
        public void Add(IEnumerable<FieldChange> list)
        {
            if (list != null)
                _list.AddRange(list);
        }

        /// <summary>
        /// Add FieldChange to FieldChangeList 
        /// </summary>
        /// <param name="list">FieldChangeList</param>
        /// <param name="change">FieldChange</param>
        /// <returns>FieldChangeList</returns>
        public static FieldChangeList operator +(FieldChangeList list, FieldChange change)
        {
            list.Add(change);
            return list;
        }

        /// <summary>
        /// Add FieldChangeList to FieldChangeList 
        /// </summary>
        /// <param name="list">FieldChangeList</param>
        /// <param name="changes">FieldChangeList</param>
        /// <returns>FieldChangeList</returns>
        public static FieldChangeList operator +(FieldChangeList list, IEnumerable<FieldChange> changes)
        {
            list.Add(changes);
            return list;
        }

        /// <summary>
        /// Insert the FieldChangeList into a database table.
        /// </summary>
        /// <param name="dataprovider">Dataprovider instance</param>
        /// <param name="table">Table name</param>
        /// <param name="standardColumnsNameValueList">Standard column name/value pairs</param>
        /// <remarks>
        /// The standard columns can be used to store a reference to id(s) to the changed record.
        /// </remarks>
        public virtual void Store(IDataProvider dataprovider, string table, params object[] standardColumnsNameValueList)
        {
            foreach (FieldChange change in _list)
                change.Store(dataprovider, table, standardColumnsNameValueList);
        }

        /// <summary>
        /// Creates new FieldChangeList instance.
        /// </summary>
        public FieldChangeList()
        {
        }

        /// <summary>
        /// Creates new FieldChangeList instance.
        /// </summary>
        public FieldChangeList(IEnumerable<FieldChange> changes)
        {
            _list = new List<FieldChange>(changes);
        }

        /// <summary>
        /// Return changes as Html table
        /// </summary>
        public string ToHtmlTable()
        {
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            s.Append(
                @"<table class=""grid"">
                    <tr>
                        <th>Date</th>
                        <th>User</th>
                        <th>Change</th>
                        <th>Old value</th>
                        <th>New value</th>
                    </tr>");
            foreach (FieldChange change in this)
            {
                s.Append("<tr>");
                s.Append("<td>" + change.ChangeDate + "</td>");
                s.Append("<td>" + change.ChangeUser + "</td>");
                s.Append("<td>" + change.FieldName + "</td>");
                s.Append("<td>" + change.OldValue + "</td>");
                s.Append("<td>" + change.NewValue + "</td>");
                s.Append("</tr>");
            }
            s.Append("</table>");
            
            return s.ToString();
        }
    }
}
