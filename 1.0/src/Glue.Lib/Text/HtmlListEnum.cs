using System;
using System.Collections;
using System.Reflection;
using System.Text;
using Glue.Lib;

namespace Glue.Lib.Text
{
    /// <summary>
    /// Helper class for enumerating lists and getting text|value combinations
    /// from it. Useful in constructing dropdown boxes, checkbox lists etc.
    /// </summary>
    public struct HtmlListItem
    {
        public object Text;
        public object Value;
    }

    /// <summary>
    /// Helper class for enumerating lists and getting text|value combinations
    /// from it. Useful in construction dropdown boxes, checkbox lists etc.
    /// </summary>
    public class HtmlListEnum: IEnumerable, IEnumerator
    {
        public HtmlListItem Item;

        IEnumerable list;
        IEnumerator inner;
        string textField;
        string valueField;
        MemberInfo textInfo;
        MemberInfo valueInfo;
        int index;

        /// <summary>
        /// Expects params with:
        /// list:
        ///   object containing items
        /// itemtext OR textfield:
        ///   field on item to get text from
        /// itemvalue OR valuefield:
        ///   field on item to get value from
        /// </summary>
        /// <param name="parms"></param>
        public HtmlListEnum(IDictionary parms)
        {
            parms["textfield"]  = NullConvert.Coalesce(parms["textfield"], parms["itemtext"]);
            parms["valuefield"] = NullConvert.Coalesce(parms["valuefield"], parms["itemvalue"]);
            Init(parms["list"], (string)parms["textfield"], (string)parms["valuefield"]);
        }
        public HtmlListEnum(object list, string textField, string valueField)
        {
            Init(list, textField, valueField);
        }
        
        private void Init(object list, string textField, string valueField)
        {
            if (list is Type && ((Type)list).IsEnum)
            {
                string[] names = Enum.GetNames((Type)list);
                Array values = Enum.GetValues((Type)list);
                OrderedDictionary dict = new OrderedDictionary();
                for (int i = 0; i < names.Length; i++)
                    dict.Add(values.GetValue(i), names[i]);
                list = dict;
            }
            this.list = list as IEnumerable;
            this.inner = this.list != null ? this.list.GetEnumerator() : null;
            this.textField = textField;
            this.valueField = valueField;
            this.index = -1;
        }

        public IEnumerator GetEnumerator()
        {
            return this;
        }
        public void Reset()
        {
            if (inner != null)
                inner.Reset();
            index = -1;
        }
        public object Current
        {
            get { return Item; }
        }
        public bool MoveNext()
        {
            if (inner == null)
                return false;
            if (!inner.MoveNext())
                return false;
            index++;
            if (inner.Current is DictionaryEntry)
            {
                DictionaryEntry ent = (DictionaryEntry)inner.Current;
                // CHECK: Item.Text = ent.Key;
                // Item.Value = ent.Value;
                Item.Value = ent.Key;
                Item.Text = ent.Value;
                return true;
            }
            System.Data.IDataRecord rec = (inner.Current as System.Data.IDataRecord);
            if (rec != null)
            {
                Item.Text = rec[textField];
                Item.Value = rec[valueField];
                return true;
            }
            if (textField == null)
            {
                Item.Text = inner.Current;
                Item.Value = index;
                return true;
            }
            if (textInfo == null)
            {
                MemberInfo[] mi = inner.Current.GetType().GetMember(textField, MemberTypes.Property | MemberTypes.Field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (mi == null || mi.Length == 0)
                    throw new MissingMemberException("Cannot find property or field '" + textField + "' on item: " + inner.Current.GetType());
                textInfo = mi[0];
                mi = inner.Current.GetType().GetMember(valueField, MemberTypes.Property | MemberTypes.Field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                valueInfo = mi[0];
                if (mi == null || mi.Length == 0)
                    throw new MissingMemberException("Cannot find property or field '" + valueField + "' on item: " + inner.Current.GetType());
            }
            if (textInfo is PropertyInfo)
                Item.Text = ((PropertyInfo)textInfo).GetValue(inner.Current, null);
            else
                Item.Text = ((FieldInfo)textInfo).GetValue(inner.Current);
            if (valueInfo is PropertyInfo)
                Item.Value = ((PropertyInfo)valueInfo).GetValue(inner.Current, null);
            else
                Item.Value = ((FieldInfo)valueInfo).GetValue(inner.Current);
            return true;
        }
    }
}
