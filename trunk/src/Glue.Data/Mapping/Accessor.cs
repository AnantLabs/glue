using System;
using System.Data;
using System.Collections;

namespace Glue.Data.Mapping
{
	/// <summary>
	/// Summary description for Accessor.
	/// </summary>
	public abstract class Accessor
	{
        /// <summary>
        /// Type forthis accessor
        /// </summary>
        public readonly Type Type;
        public readonly IDataProvider Provider;

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
        public virtual void AddParametersToCommandDynamic(object instance, IDbCommand command, IList names)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void AddParametersToCommandFixed(object instance, IDbCommand command)
        {
        }
    }
}
