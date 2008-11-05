using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Glue.Lib
{
    /// <summary>
    /// A Mapper can be used to map properties from 
    /// a NameValueCollection to one or more instance types.
    /// </summary>
    public class Mapper
    {
        /// <summary>
        /// Protected constructor to prevent creation.
        /// </summary>
        protected Mapper()
        {
        }

        /// <summary>
        /// Create an instance of given type. Construct it with data in bag.
        /// Throws CombinedException on error.
        /// </summary>
        public static object Create(Type type, IDictionary bag)
        {
            return Create(type, bag, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Create an instance of given type. Construct it with data in bag.
        /// Throws CombinedException on error.
        /// </summary>
        public static object Create(Type type, IDictionary bag, IFormatProvider culture)
        {
            CombinedException errors = new CombinedException();
            object instance = Create(type, bag, culture, errors);
            if (errors.Count > 0)
                throw errors;
            return instance;
        }

        /// <summary>
        /// Create an instance of given type. Construct it with data in bag.
        /// </summary>
        public static object Create(Type type, IDictionary bag, CombinedException errors)
        {
            return Create(type, bag, CultureInfo.InvariantCulture, errors);
        }

        /// <summary>
        /// Create an instance of given type. Construct it with data in bag.
        /// </summary>
        public static object Create(Type type, IDictionary bag, IFormatProvider culture, CombinedException errors)
        {
            if (type.IsAbstract || type.IsInterface ) 
                return null;

            ConstructorInfo constructor = (ConstructorInfo)SelectBestCandidate(type.GetConstructors(), bag);
            if (constructor == null)
                return null;
            
            object instance = constructor.Invoke(BuildArgs(constructor, bag, errors));
            if (instance == null)
                return null;

            return Assign(instance, bag, culture, errors);
        }

        

        /// <summary>
        /// Assign values in bag to given instance.
        /// Throws CombinedException on error.
        /// </summary>
        public static object Assign(object instance, IDictionary bag)
        {
            return Assign(instance, bag, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Assign values in bag to given instance.
        /// Throws CombinedException on error.
        /// </summary>
        public static object Assign(object instance, IDictionary bag, IFormatProvider culture)
        {
            CombinedException errors = new CombinedException();
            Assign(instance, bag, culture, errors);
            if (errors.Count > 0)
                throw errors;
            return instance;
        }

        /// <summary>
        /// Assign values in bag to given instance.
        /// </summary>
        public static object Assign(object instance, IDictionary bag, CombinedException errors)
        {
            return Assign(instance, bag, CultureInfo.InvariantCulture, errors);
        }

        /// <summary>
        /// Assign values in bag to given instance.
        /// </summary>
        public static object Assign(object instance, IDictionary bag, IFormatProvider culture, CombinedException errors)
        {
            return AssignAllowed(instance, bag, null, culture, errors);
        }

        public static object AssignAllowed(object instance, IDictionary bag, string allowed, IFormatProvider culture)
        {
            CombinedException errors = new CombinedException();
            AssignAllowed(instance, bag, allowed, culture, errors);
            if (errors.Count > 0)
                throw errors;
            return instance;
        }

        /// <summary>
        /// Assign values in bag to given instance. 'allowed' is a comma separated list of names of fields/properties that may be assigned to.
        /// </summary>
        public static object AssignAllowed(object instance, IDictionary bag, string allowed, IFormatProvider culture, CombinedException errors)
        {
            Type type = instance.GetType();
            MemberHelper[] members = MemberHelper.GetFieldsOrProperties(type, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
            if (allowed != null)
            {
                List<string> allowedList = new List<string>(allowed.Split(','));
                List<string> notAllowed = CheckAllowed(type, bag, allowedList, "");

                if (notAllowed.Count > 0)
                {
                    errors.Add("Acces not allowed to fields or properties: " + String.Join(",", notAllowed.ToArray()));
                    return instance;
                }
            }
            foreach (MemberHelper member in members)
                {
                    try
                    {
                        // TODO: Handle attribute to remap names
                        string name = member.Name;

                        object value = bag[name];
                        if (value != null)
                            Assign(instance, member, value, culture, errors);
                    }
                    catch (Exception e)
                    {
                        errors.Add(member.Name, e);
                    }
                }
            return instance;
        }

        static List<string> CheckAllowed(Type type, IDictionary bag, List<string> allowedList, string prefix)
        {
            List<string> notAllowed = new List<string>();
            MemberHelper[] members = MemberHelper.GetFieldsOrProperties(type, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
            Dictionary<string, MemberHelper> memberDict = new Dictionary<string,MemberHelper>();
            foreach (MemberHelper member in members)
            {   // add all members to a dictionary, to be able to look them up by name.
                string memberName = member.Name.ToLower();
                if (memberDict.ContainsKey(memberName)) 
                    throw new Exception("Duplicate member name in case-insensitive member list: " + type.Name + "." + member.Name);
                memberDict[member.Name.ToLower()] = member;
            }

            foreach (DictionaryEntry item in bag)
            {
                string memberName = ((string)item.Key).ToLower();
                if (memberDict.ContainsKey(memberName))
                {
                    if (item.Value is IDictionary && !allowedList.Contains(prefix + memberName))
                        notAllowed.AddRange(CheckAllowed(memberDict[memberName].Type, (IDictionary)item.Value, allowedList, prefix + memberName + "."));
                    else if (!allowedList.Contains(prefix + memberName))
                        notAllowed.Add(prefix + memberName);
                }
            }
            return notAllowed;
        }
    
        /// <summary>
        /// Private function to assign or initialise an individual member of 
        /// an object instance (owner) with given value.
        /// This method is used to set fields and properties on classes *and*
        /// fill parameters in calls to methods. In that case owner will always
        /// be null.
        /// </summary>
        static void Assign(object owner, MemberHelper info, object value, IFormatProvider culture, CombinedException errors)
        {
            if (value is IDictionary)
            {
                if (info.Type == typeof(DateTime))
                {
                    IDictionary bag = (IDictionary)value;
                    try
                    {
                        DateTime dt;
                        if (bag.Contains("day"))
                        {
                            int year = Convert.ToInt32(bag["year"]);
                            int month = Convert.ToInt32(bag["month"]);
                            int day = Convert.ToInt32(bag["day"]);
                            dt = new DateTime(year, month, day);
                        }
                        else
                        {
                            string val = (string)bag["value"];
                            string pat = (string)bag["pattern"];
                            if (val == null || val == "")
                                return;
                            if (pat == "")
                                pat = null;
                            try
                            {
                                dt = DateTime.ParseExact(val, pat, null);
                            }
                            catch
                            {
                                dt = DateTime.Parse(val, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces);
                            }
                        }
                        info.SetValue(owner, dt, null);
                        return;
                    }
                    catch (InvalidCastException e)
                    {
                        errors.Add(info.Name, e);
                        return;
                    }
                    catch (FormatException e)
                    {
                        errors.Add(info.Name, e);
                        return;
                    }
                    catch (Exception e)
                    {
                        errors.Add(info.Name, "Error initializing datetime value", e);
                        return;
                    }
                }
                if (info.Writable)
                {
                    // We'are dealing with an object.
                    // If it's writable try to create and set a new object.
                    object newinst = Create(info.Type, value as IDictionary, culture, errors);
                    if (newinst != null)
                    {
                        info.SetValue(owner, newinst, null);
                        return;
                    }
                }
                // Setting new object failed, or member is not writable. 
                // That means we will traverse the object and set its 
                // individual members. This will fail for value-type objects
                // so bail out if we are dealing with those.
                if (info.Type.IsValueType)
                {
                    errors.Add(info.Name, "Cannot assign to non-writable valuetype.");
                    return;
                }
                // Get current object and try to assign to its individual members.
                object current = info.GetValue(owner, null);
                if (current != null)
                {
                    Assign(current, value as IDictionary, culture, errors);
                }
                else
                {
                    errors.Add(info.Name, "Unknown error");
                }
                return;
            }
            if (value is IList)
            {
                if ((info.Type.IsValueType || info.Type == typeof(String)) &&
                    ((IList)value).Count > 0)
                    value = ((IList)value)[0];
                else
                    errors.Add(info.Name, "Lists and arrays not supported.");
            }
            if (info.Writable)
            {
                if (info.Type == typeof(string))
                {
                    info.SetValue(owner, Convert.ToString(value), null);
                }
                else if (info.Type == typeof(Boolean))
                {
                    if (!NullConvert.IsNullOrEmpty(value))
                    {
                        info.SetValue(owner, NullConvert.ToBoolean(value, false), null);
                    }
                }
                else if (info.Type == typeof(Int32))
                {
                    if (!NullConvert.IsNullOrEmpty(value))
                        info.SetValue(owner, Convert.ToInt32(value), null);
                }
                else if (info.Type == typeof(UInt32))
                {
                    if (!NullConvert.IsNullOrEmpty(value))
                        info.SetValue(owner, Convert.ToUInt32(value), null);
                }
                else if (info.Type == typeof(DateTime))
                {
                    if (!NullConvert.IsNullOrEmpty(value))
                        info.SetValue(owner, Convert.ToDateTime(value), null);
                }
                else if (info.Type == typeof(Guid))
                {
                    if (value is Guid)
                        info.SetValue(owner, (Guid)value, null);
                    else if (!NullConvert.IsNullOrEmpty(value))
                        info.SetValue(owner, new Guid(Convert.ToString(value)), null);
                }
                else if (info.Type == typeof(Int64))
                {
                    if (!NullConvert.IsNullOrEmpty(value))
                        info.SetValue(owner, Convert.ToInt64(value), null);
                }
                else if (info.Type == typeof(UInt64))
                {
                    if (!NullConvert.IsNullOrEmpty(value))
                        info.SetValue(owner, Convert.ToUInt64(value), null);
                }
                else if (info.Type == typeof(Byte))
                {
                    if (!NullConvert.IsNullOrEmpty(value))
                        info.SetValue(owner, Convert.ToByte(value), null);
                }
                else if (info.Type == typeof(SByte))
                {
                    if (!NullConvert.IsNullOrEmpty(value))
                        info.SetValue(owner, Convert.ToSByte(value), null);
                }
                else if (info.Type == typeof(Int16))
                {
                    if (!NullConvert.IsNullOrEmpty(value))
                        info.SetValue(owner, Convert.ToInt16(value), null);
                }
                else if (info.Type == typeof(UInt16))
                {
                    if (!NullConvert.IsNullOrEmpty(value))
                        info.SetValue(owner, Convert.ToUInt16(value), null);
                }
                else if (info.Type == typeof(Double))
                {
                    if (!NullConvert.IsNullOrEmpty(value))
                    {
                        if (value.GetType() == typeof(string))
                            info.SetValue(owner, Double.Parse((string)value, NumberStyles.Number, culture), null);
                        else
                            info.SetValue(owner, Convert.ToDouble(value), null);
                    }
                }
                else if (info.Type == typeof(Single))
                {
                    if (!NullConvert.IsNullOrEmpty(value))
                    {
                        if (value.GetType() == typeof(string))
                            info.SetValue(owner, Single.Parse((string)value, NumberStyles.Number, culture), null);
                        else
                            info.SetValue(owner, Convert.ToSingle(value), null);
                    }
                }
                else if (info.Type == typeof(Decimal))
                {
                    if (!NullConvert.IsNullOrEmpty(value))
                    {
                        if (value.GetType() == typeof(string))
                            info.SetValue(owner, Decimal.Parse((string)value, NumberStyles.Number, culture), null);
                        else
                            info.SetValue(owner, Convert.ToSingle(value), null);
                    }
                }

                else
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(info.Type);
                    if (converter.CanConvertFrom(value.GetType()))
                    {
                        try
                        {
                            info.SetValue(owner, converter.ConvertFrom(value), null);
                        }
                        catch (Exception e)
                        {
                            errors.Add(info.Name, "Error converting from " + value.GetType().Name + " to " + info.Type.Name, e);
                        }
                    }
                    else
                    {
                        try
                        {
                            MethodInfo initializer = GetImplicitOrExplicitOperator(info.Type, value.GetType());
                            if (initializer != null)
                            {
                                object newinst = initializer.Invoke(null, new object[] {value});
                                info.SetValue(owner, newinst, null);
                            }
                            else
                            {
                                object newinst = Activator.CreateInstance(info.Type, new object[] {value});
                                info.SetValue(owner, newinst, null);
                            }
                        }
                        catch (Exception e)
                        {
                            errors.Add(info.Name, "Error converting from " + value.GetType().Name + " to " + info.Type.Name, e);
                        }
                    }
                }
                return;
            }
        }

        /// <summary>
        /// Select and invoke given method, passing parameters in bag.
        /// Throws CombinedException on error.
        /// </summary>
        public static object Invoke(object instance, string action, IDictionary bag)
        {
            CombinedException errors = new CombinedException();
            object ret = Invoke(instance, action, bag, errors);
            if (errors.Count > 0)
                throw errors;
            return ret;
        }
        
        /// <summary>
        /// Select and invoke given method, passing parameters in bag.
        /// Throws CombinedException on error.
        /// </summary>
        public static object Invoke(object instance, MethodInfo method, IDictionary bag)
        {
            CombinedException errors = new CombinedException();
            object ret = Invoke(instance, method, bag, errors);
            if (errors.Count > 0)
                throw errors;
            return ret;
        }
        
        /// <summary>
        /// Select and invoke given method, passing parameters in bag.
        /// </summary>
        public static object Invoke(object instance, string action, IDictionary bag, CombinedException errors)
        {
            Type type = instance.GetType();
            MethodInfo method = Select(type, action, bag, errors);
            return Invoke(instance, method, bag, errors);
        }

        /// <summary>
        /// Invoke given method, passing parameters in bag.
        /// </summary>
        protected static object Invoke(object instance, MethodInfo method, IDictionary bag, CombinedException errors)
        {
            object[] args = BuildArgs(method, bag, errors);
            return method.Invoke(instance, args);
        }

        /// <summary>
        /// Select a method based on given action and closest matching the parameters
        /// specified in bag.
        /// </summary>
        protected static MethodInfo Select(Type type, string action, IDictionary bag, CombinedException errors)
        {
            ArrayList candidates = new ArrayList();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        
            foreach (MethodInfo m in methods)
                if (string.Compare(m.Name, action, true) == 0)
                    candidates.Add(m);
        
            if (candidates.Count == 0)
                errors.Add(action, "No method found.");

            return (MethodInfo)SelectBestCandidate((MethodBase[])candidates.ToArray(typeof(MethodBase)), bag);
        }

        
        /// <summary>
        /// Select methods closest matching the given parameters in bag.
        /// </summary>
        protected static MethodBase SelectBestCandidate(MethodBase[] candidates, IDictionary bag)
        {
            if (candidates.Length == 1)
            {
                // There's nothing much to do in this situation
                return (MethodBase)candidates[0];
            }

            int lastMaxPoints = int.MinValue;
            MethodBase bestCandidate = null;

            foreach (MethodBase candidate in candidates)
            {
                if (candidate == null)
                    continue;
                int points = 0;
                ParameterInfo[] parameters = candidate.GetParameters();

                if (parameters.Length == bag.Count)
                {
                    points = 10;
                }

                foreach (ParameterInfo param in parameters)
                {
                    object value = bag[param.Name];
                    if (value != null) points += 10;
                }

                if (lastMaxPoints < points)
                {
                    lastMaxPoints = points;
                    bestCandidate = candidate;
                }
            }

            return bestCandidate;
        }

        /// <summary>
        /// Fill an array of arguments based on parameter information of given method (or constructor)
        /// and the values in bag.
        /// </summary>
        protected static object[] BuildArgs(MethodBase method, IDictionary bag, CombinedException errors)
        {
            ParameterInfo[] parameters = method.GetParameters();
            object[] args = new object[parameters.Length];
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    MemberHelper param = new MemberHelper(parameters[i]);

                    // TODO: Handle attribute to remap names
                    string name = param.Name;
                    object value = bag[name];
                
                    if (value != null)
                    {
                        Assign(null, param, value, CultureInfo.CurrentCulture, errors);
                        args[i] = param.GetValue(null, null);
                    }
                }
            }
            catch (FormatException)
            {
                // TODO: throw new ArgumentException(
                //     String.Format("Could not convert {0} to bag type. " + 
                //     "Argument value is '{1}'", param.Name, bag.Params[param.Name]), ex );
                // Log.Debug("Could not convert " + param.Name + " to bag type. Argument value is: " + param.Name);
            }
            catch (Exception)
            {
                // TODO: throw new ArgumentException(
                //     String.Format("Error building method arguments. " + 
                //     "Last param analized was {0} with value '{1}'", param.Name, value), ex );
                // Log.Debug("Error building method arguments. Last param analized was " + param.Name + " with value '" + value + "'");
            }
            return args;
        }

        protected static MethodInfo GetImplicitOrExplicitOperator(Type result, Type from)
        {
            MethodInfo[] operators = result.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (MethodInfo op in operators)
                if (op.IsSpecialName && op.ReturnType.Equals(result) && (op.Name == "op_Implicit" || op.Name == "op_Explicit"))
                {
                    ParameterInfo[] parameters = op.GetParameters();
                    if (parameters.Length == 1 && (from.Equals(parameters[0].ParameterType) || from.IsSubclassOf(parameters[0].ParameterType)))
                        return op;
                }
            operators = from.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (MethodInfo op in operators)
                if (op.IsSpecialName && op.ReturnType.Equals(result) && (op.Name == "op_Implicit" || op.Name == "op_Explicit"))
                {
                    ParameterInfo[] parameters = op.GetParameters();
                    if (parameters.Length == 1 && (from.Equals(parameters[0].ParameterType) || from.IsSubclassOf(parameters[0].ParameterType)))
                        return op;
                }
            return null;
        }

        /// <summary>
        /// Private helper class.
        /// </summary>
        private class MemberHelper
        {
            PropertyInfo _property;
            FieldInfo _field;
            ParameterInfo _parameter;
            string _name;
            Type _type;
            bool _writable;
            object _parameterValue;

            public static MemberHelper[] GetFieldsOrProperties(Type type, BindingFlags flags)
            {
                FieldInfo[] fields = type.GetFields(flags);
                PropertyInfo[] properties = type.GetProperties(flags);
                MemberHelper[] members = new MemberHelper[fields.Length + properties.Length];
                int j = 0;
                for (int i = 0; i < fields.Length;)
                    members[j++] = new MemberHelper(fields[i++]);
                for (int i = 0; i < properties.Length;)
                    members[j++] = new MemberHelper(properties[i++]);
                return members;
            }
            public static MemberHelper[] GetParameters(MethodBase method)
            {
                ParameterInfo[] parameters = method.GetParameters();
                MemberHelper[] members = new MemberHelper[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                    members[i] = new MemberHelper(parameters[i]);
                return members;
            }
            public MemberHelper(PropertyInfo property)
            {
                _property = property;
                _writable = property.CanWrite;
                _type = property.PropertyType;
                _name = property.Name;
            }
            public MemberHelper(FieldInfo field)
            {
                _field = field;
                _writable = !field.IsInitOnly;
                _type = field.FieldType;
                _name = field.Name;
            }
            public MemberHelper(ParameterInfo parameter)
            {
                _parameter = parameter;
                _writable = true;
                _type = parameter.ParameterType;
                _name = parameter.Name;
            }
            public object GetValue(object instance, object[] args)
            {
                if (_property != null)
                    return _property.GetValue(instance, args);
                else if (_field != null)
                    return _field.GetValue(instance);
                else
                    return _parameterValue;
            }
            public void SetValue(object instance, object value, object[] args)
            {
                if (_property != null)
                    _property.SetValue(instance, value, args);
                else if (_field != null)
                    _field.SetValue(instance, value);
                else
                    _parameterValue = value;
            }
            public string Name
            {
                get { return _name; }
            }
            public Type Type
            {
                get { return _type; }
            }
            public bool Writable
            {
                get { return _writable; }
            }
        }
    }
}
