using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

using Glue.Lib;

namespace Glue.Data
{
    /// <summary>
    /// Class to compute and store Field value changes.
    /// </summary>
    /// <remarks>
	/// <para>
    /// This class can be used, in combination with the <see cref="FieldChangeList"/>-class, to compute and store 
    /// changes to object field/property values. 
    /// </para>
	/// <para>
    /// If the FieldChanges are to be stored in a database, the class expects a table of the following structure:
    /// </para>
    /// <list type="table">
	/// <listheader><term>Field</term><description>Type</description></listheader>  
    /// <item><term>FieldName</term><description>string ((var)(n)char)</description></item>
    /// <item><term>OldValue</term><description>string ((var)(n)char)</description></item>
    /// <item><term>NewValue</term><description>string ((var)(n)char)</description></item>
    /// <item><term>ChangeUser</term><description>string ((var)(n)char)</description></item>
    /// <item><term>ChangeDate</term><description>DateTime ((small)datetime)</description></item>
    /// </list>
    /// <para>
    /// How does it work? Easy: call <c>FieldChange.ComputeChanges()</c> with two objects and a list of field- and/or propertynames
    /// and a list of <see cref="FieldChange"/>s (a <see cref="FieldChangeList"/>) is returned.
    /// </para>
	/// <para>
    /// Here's an example of a custom <c>Update</c>-method on a class <c>User</c>that returns the changes to the object:
	/// </para>
    /// <code>
    ///        public FieldChangeList Update(string updateUser)
    ///        {
    ///            FieldChangeList changes = FieldChange.ComputeChanges(
    ///                new string[] {                                  // compare these fields/ properties
    ///                    "UserName", 
    ///                    "FirstName", 
    ///                    "MiddleName",
    ///                    "LastName",
    ///                    "Email",
    ///                    "NormalHoursPerWeek",
    ///                    "CostRate",
    ///                    "IsAdmin"
    ///                },
    ///                Global.DataProvider.Find&lt;User&gt;(Id),        // retrieve old data
    ///                this,                                           // changed instance
    ///                updateUser);
    ///
    ///            // Store the updates to this instance
    ///            Global.DataProvider.Update(this);
    /// 
    ///            // return list of changes
    ///            return changes;
    ///        }
    /// </code>
	/// <para>
	/// To save a <see cref="FieldChangeList"/> to the database, call <c>Store()</c> on the <see cref="FieldChangeList"/>. <see cref="FieldChange"/> has a method List() to retrieve the changes.
	/// </para>
    /// <code>
    ///     // store changes
    ///     Changes.Store(Global.DataProvider, "Changes_User", "User_Id", User.Id);
    ///     
    ///     // retrieve changes
    ///     Changes = FieldChange.List(Global.DataProvider, "Changes_User", Filter.Create("User_Id=@0", User.Id));
    /// </code>
	/// <para>
    /// If an object has more than one source of changes (i.e. child objects in linked tables), the following pattern can be used:
    /// </para>
    /// <code>
    ///     public FieldChangeList Changes;
    ///     // main item
    ///     Changes = Project.Insert(CurrentUser.UserName);
    ///     
    ///     // other changes, linked tables etc.
    ///     // use the '+' or '+=' operators to combine the list of changes
    ///     Changes += Project.UpdateUserLinks(UserId, CurrentUser.UserName);
    ///     Changes += ProcessSubProjects(id);
    /// 
    ///     // store changes to table "Changes_Project"
	///     // add a reference to a key called "Project_Id" so we later on know how to retrieve changes for this project.
    ///     Changes.Store(Global.DataProvider, "Changes_Project", "Project_Id", Project.Id);
    /// </code>
    /// <para>
    /// Last but not least: to display the changes on a web page, <see cref="FieldChangeList"/> has a <c>ToHtmlTable()</c>-method, very useful in ASPX-pages:
	/// </para>
    /// <code>
    ///     &lt;h4&gt;Changes&lt;/h4&gt;
    ///     &lt;%=Changes.ToHtmlTable("grid") %&gt;
    /// </code>
	/// <para>
	/// This generates the following table:
	/// </para>
	/// <code>
	/// <![CDATA[
	/// <h4>Changes</h4>
	/// <table class="grid">
	///		<tr>
	///			<th>Date</th>
	///			<th>User</th>
	///			<th>Change</th>
	///			<th>Old value</th>
	///			<th>New value</th>
	///		</tr>
	///		<tr>
	///			<td>29-9-2008 23:38:38</td>
	///			<td>DOMAIN\Anonymous</td>
	///			<td>NormalHoursPerWeek</td>
	///			<td>0</td>
	///			<td>40</td>
	///		</tr>
	///		<tr>
	///			<td>29-9-2008 23:38:46</td>
	///			<td>DOMAIN\Admin</td>
	///			<td>NormalHoursPerWeek</td>
	///			<td>40</td>
	///			<td>48</td>
	///		</tr>
	///	</table>
	/// ]]>
	/// </code>
    /// </remarks>
    /// <seealso cref="FieldChangeList"/>
    public class FieldChange
    {
        /// <summary>
        /// Create new (empty) FieldChange
        /// </summary>
        public FieldChange()
        {

        }
        
        /// <summary>
        /// Create new FieldChange with default ChangeDate (now)
        /// </summary>
        public FieldChange(string fieldName, string oldValue, string newValue, string changeUser)
        {
            this.FieldName = fieldName;
            this.OldValue = oldValue;
            this.NewValue = newValue;
            this.ChangeUser = changeUser;
            this.ChangeDate = DateTime.Now;
        }

        /// <summary>
        /// Create new FieldChange
        /// </summary>
        public FieldChange(string fieldName, string oldValue, string newValue, string changeUser, DateTime changeDate)
        {
            this.FieldName = fieldName;
            this.OldValue = oldValue;
            this.NewValue = newValue;
            this.ChangeUser = changeUser;
            this.ChangeDate = changeDate;
        }

        /// <summary>
        /// Field name
        /// </summary>
        public string FieldName;

        /// <summary>
        /// Old field value
        /// </summary>
        public string OldValue;

        /// <summary>
        /// New field value
        /// </summary>
        public string NewValue;

        /// <summary>
        /// User responsible for change
        /// </summary>
        public string ChangeUser;

        /// <summary>
        /// Date the field value was changed
        /// </summary>
        public DateTime ChangeDate;

        /// <summary>
        /// Compute changes between an old and a new instance of an object (i.e. a row).
        /// </summary>
        /// <param name="fieldList">List of field names to compare.</param>
        /// <param name="o">"Old" object instance</param>
        /// <param name="n">"New" object instance</param>
        /// <param name="changeUser"></param>
        /// <param name="fieldNamePrefix">Prefix for field change name</param>
        /// <returns>List of changes</returns>
        public static FieldChangeList ComputeChanges(string[] fieldList, object o, object n, string changeUser, string fieldNamePrefix)
        {
            FieldChangeList changes = new FieldChangeList();
            foreach (string field in fieldList)
            {
                FieldChange change = new FieldChange();
                object obj;

                // get old, new value
                if (o != null)
                {
                    obj = o.GetType().InvokeMember(field, BindingFlags.GetField | BindingFlags.GetProperty, null, o, null);
                    if (obj != null)
                        change.OldValue = obj.ToString().TrimEnd();
                }
                if (n != null)
                {
                    obj = n.GetType().InvokeMember(field, BindingFlags.GetField | BindingFlags.GetProperty, null, n, null);
                    if (obj != null)
                        change.NewValue = obj.ToString().TrimEnd();
                }

                if (change.OldValue != change.NewValue)
                {
                    change.FieldName = fieldNamePrefix + field;
                    change.ChangeDate = DateTime.Now;
                    change.ChangeUser = changeUser;

                    Log.Debug("Changed {0} : '{1}' to '{2}'.", change.FieldName, change.OldValue, change.NewValue);
                    changes.Add(change);
                }
            }
            return changes;
        }

        /// <summary>
        /// Compute changes between an old and a new instance of an object (i.e. a row).
        /// </summary>
        /// <param name="fieldList">List of field names to compare.</param>
        /// <param name="o">"Old" object instance</param>
        /// <param name="n">"New" object instance</param>
        /// <param name="changeUser"></param>
        /// <returns>List of changes</returns>
        public static FieldChangeList ComputeChanges(string[] fieldList, object o, object n, string changeUser)
        {
            return ComputeChanges(fieldList, o, n, changeUser, "");
        }

        /// <summary>
        /// Insert the FieldChange into a database table.
        /// </summary>
        /// <param name="dataprovider">Dataprovider instance</param>
        /// <param name="table">Table name</param>
        /// <param name="standardColumnsNameValueList">Standard column name/value pairs</param>
        /// <remarks>
        /// The standard columns can be used to store a reference to id(s) to the changed record.
        /// </remarks>
        public virtual void Store(IDataProvider dataprovider, string table, params object[] standardColumnsNameValueList)
        {
            object[] columnNameValueList;
            if (standardColumnsNameValueList != null)
            {
                columnNameValueList = new object[standardColumnsNameValueList.Length + 10];
                standardColumnsNameValueList.CopyTo(columnNameValueList, 10);
            }
            else
            {
                columnNameValueList = new object[10];
            }

            columnNameValueList[0] = "FieldName";
            columnNameValueList[1] = FieldName;
            columnNameValueList[2] = "OldValue";
            columnNameValueList[3] = OldValue;
            columnNameValueList[4] = "NewValue";
            columnNameValueList[5] = NewValue;
            columnNameValueList[6] = "ChangeUser";
            columnNameValueList[7] = ChangeUser;
            columnNameValueList[8] = "ChangeDate";
            columnNameValueList[9] = ChangeDate;

            IDbCommand cmd = dataprovider.CreateInsertCommand(table, columnNameValueList);
            dataprovider.ExecuteNonQuery(cmd);
        }

        /// <summary>
        /// Retrieve FieldChangeList
        /// </summary>
        /// <param name="dataprovider">IDataProvider instance</param>
        /// <param name="table">Table name</param>
        /// <param name="constraint">Filter</param>
        public static FieldChangeList List(IDataProvider dataprovider, string table, Filter constraint)
        {
            IDbCommand cmd = dataprovider.CreateSelectCommand(
                table, "FieldName,OldValue,NewValue,ChangeUser,ChangeDate",
                constraint, "ChangeDate", Limit.Unlimited, null);
            return new FieldChangeList(dataprovider.List<FieldChange>(cmd));
        }

    }

    /// <summary>
    /// List of field value changes
    /// </summary>
    /// <remarks>
	/// <para>
    /// This class can be used, in combination with the <see cref="FieldChange"/>-class, to compute and store 
    /// changes to object field/property values. 
    /// </para>
	/// <para>
    /// If the <see cref="FieldChange"/>s are to be stored in a database, the class expects a table of the following structure:
    /// </para>
	/// <list type="table">
	/// <listheader><term>Field</term><description>Type</description></listheader>  
    /// <item><term>FieldName</term><description>string ((var)(n)char)</description></item>
    /// <item><term>OldValue</term><description>string ((var)(n)char)</description></item>
    /// <item><term>NewValue</term><description>string ((var)(n)char)</description></item>
    /// <item><term>ChangeUser</term><description>string ((var)(n)char)</description></item>
    /// <item><term>ChangeDate</term><description>DateTime ((small)datetime)</description></item>
    /// </list>
    /// <para>
    /// See <see cref="FieldChange"/> for some examples.
	/// </para>
    /// </remarks>
    /// <seealso cref="FieldChange"/>
    public class FieldChangeList : IList<FieldChange>
    {
        private List<FieldChange> _list = new List<FieldChange>();

        #region IList<FieldChange> Members

        /// <summary>
        /// Determines the index of a specific item in the FieldChangeList. 
        /// </summary>
        public int IndexOf(FieldChange item)
        {
            return _list.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item to the FieldChangeList at the specified index. 
        /// </summary>
        public void Insert(int index, FieldChange item)
        {
            if (item != null)
                _list.Insert(index, item);
        }

        /// <summary>
        /// Removes the FieldChangeList item at the specified index. 
        /// </summary>
        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }
        /// <summary>
        /// Removes the first occurrence of a specific object from the FieldChangeList. 
        /// </summary>
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

        /// <summary>
        /// Adds an item to the FieldChangeList. 
        /// </summary>
        public void Add(FieldChange item)
        {
            if (item != null)
                _list.Add(item);
        }

        /// <summary>
        /// Removes all items from the FieldChangeList. 
        /// </summary>
        public void Clear()
        {
            _list.Clear();
        }

        /// <summary>
        /// Determines whether the FieldChangeList contains a specific value. 
        /// </summary>
        public bool Contains(FieldChange item)
        {
            return _list.Contains(item);
        }

        /// <summary>
        /// Copies the FieldChangeList or a portion of it to a one-dimensional array. 
        /// </summary>
        public void CopyTo(FieldChange[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements actually contained in the FieldChangeList.
        /// </summary>
        public int Count
        {
            get { return _list.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the FieldChangeList is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the FieldChangeList. 
        /// </summary>
        public bool Remove(FieldChange item)
        {
            return _list.Remove(item);
        }

        #endregion

        #region IEnumerable<FieldChange> Members

        /// <summary>
        /// Returns an enumerator that iterates through the FieldChangeList. 
        /// </summary>
        public IEnumerator<FieldChange> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through the FieldChangeList. 
        /// </summary>
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
        /// <param name="tableClass">Name of css-class</param>
        public string ToHtmlTable(string tableClass)
        {
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            s.Append("<table");

            if (tableClass != null)
                s.Append(" class=\"" + tableClass + "\"");

            s.Append("><tr><th>Date</th><th>User</th><th>Change</th><th>Old value</th><th>New value</th></tr>");
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
        /// <summary>
        /// Return changes as Html table
        /// </summary>
        public string ToHtmlTable()
        {
            return ToHtmlTable(null);
        }
    }
}
