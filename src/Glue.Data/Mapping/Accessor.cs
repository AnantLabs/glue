using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;

namespace Glue.Data.Mapping
{
    /// <summary>
	/// Summary description for Accessor.
	/// </summary>
	public abstract class Accessor
	{
        /// <summary>
        /// Type for this accessor
        /// </summary>
        public Type Type;
        public IDataProvider Provider;
        public Entity Entity;
        public string FindCommandText;
        public string InsertCommandText;
        public string UpdateCommandText;
        public string DeleteCommandText;
        public string ReplaceCommandText;

        static Dictionary<Type, Dictionary<Type, Accessor>> _cache = new Dictionary<Type, Dictionary<Type, Accessor>>();

        /// <summary>
        /// Obtain an Accessor for given provider and object type.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Accessor Obtain(BaseDataProvider provider, Type type)
        {
            Type providerType = provider.GetType();
            Dictionary<Type, Accessor> bag;
            Accessor accessor;
            lock (_cache)
            {
                if (!_cache.TryGetValue(providerType, out bag))
                {
                    bag = new Dictionary<Type, Accessor>();
                    _cache.Add(providerType, bag);
                }
                if (!bag.TryGetValue(type, out accessor))
                {
                    accessor = provider.CreateAccessor(type);
                    accessor.Entity = Entity.Obtain(type);
                    bag.Add(type, accessor);
                }
            }
            return accessor;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected Accessor(IDataProvider provider, Type type)
        {
            this.Provider = provider;
            this.Type = type;
        }

        /// <summary>
        /// Create a lookup list of fieldnames => ordinals on the opened DataReader.
        /// </summary>
        public virtual IDictionary CreateColumnOrdinals(IDataReader reader)
        {
            IDictionary columnOrdinals = new System.Collections.Specialized.HybridDictionary(reader.FieldCount, true);
            for (int i = 0; i < reader.FieldCount; i++)
                columnOrdinals[reader.GetName(i)] = i;
            return columnOrdinals;
        }
        
        /// <summary>
        /// Instantiates a list of objects from the IDataReader; the columns in the 
        /// reader can be in any order.
        /// </summary>
        public virtual ArrayList ListFromReaderDynamic(IDataReader reader, Limit limit)
        {
            ArrayList list = new ArrayList();
            if (limit == null)
                limit = Limit.Unlimited;

            if (limit.Count == 0)
                return list;

            // skip to first of limit
            int n = limit.Index; 
            while (n > 0)
            {
                if (!reader.Read())
                    return list;
                n--;
            }

            // read first and set up lookup stuff
            if (!reader.Read())
                return list;
            IDictionary columnOrdinals = CreateColumnOrdinals(reader);
            list.Add(CreateFromReaderDynamic(reader, columnOrdinals));

            // read rest
            n = limit.Count - 1;
            while (n != 0)
            {
                if (!reader.Read())
                    return list;
                list.Add(CreateFromReaderDynamic(reader, columnOrdinals));
                n--;
            }
            return list;
        }

        /// <summary>
        /// Instantiates a list of objects from the IDataReader; the columns in the 
        /// reader must be in fixed order, corresponding to the class in-memory layout.
        /// </summary>
        public virtual ArrayList ListFromReaderFixed(IDataReader reader)
        {
            ArrayList list = new ArrayList();
            while (reader.Read())
                list.Add(CreateFromReaderFixed(reader, 0));
            return list;
        }

        /// <summary>
        /// Instantiate an object from given IDataReader. Columns must be in fixed order,
        /// corresponding to the class member layout.
        /// </summary>
        public virtual object CreateFromReaderDynamic(IDataReader reader, IDictionary columnOrdinals)
        {
            object instance = Activator.CreateInstance(Type);
            InitFromReaderDynamic(instance, reader, columnOrdinals);
            return instance;
        }

        /// <summary>
        /// Instantiate an object from given IDataReader. Columns must be in fixed order,
        /// corresponding to the class member layout.
        /// </summary>
        public virtual object CreateFromReaderFixed(IDataReader reader, int ordinalOffset)
        {
            object instance = Activator.CreateInstance(Type);
            InitFromReaderFixed(instance, reader, ordinalOffset);
            return instance;
        }
        
        /// <summary>
        /// Throws exception if value is a string and value is longer than maxlength/
        /// </summary>
        public object CheckLength(object value, int maxlength, string name)
        {
            if (value != null && value is string && ((string)value).Length > maxlength)
                throw new ArgumentOutOfRangeException(name, "String too long");
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract void InitFromReaderDynamic(object instance, IDataReader reader, IDictionary fieldOrdinalMap);

        /// <summary>
        /// 
        /// </summary>
        public abstract void InitFromReaderFixed(object instance, IDataReader reader, int ordinalOffset);

        /// <summary>
        /// 
        /// </summary>
        public virtual void AddAllParametersToCommand(object instance, IDbCommand command)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void AddKeyParametersToCommand(object instance, IDbCommand command)
        {
        }
    }
}
