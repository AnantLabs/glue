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
    /// This class can be used, in combination with the <see cref="FieldChangeList"/>-class, to compute and store 
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
}
