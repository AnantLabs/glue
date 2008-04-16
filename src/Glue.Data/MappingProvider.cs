using System;
using System.Collections.Generic;
using System.Text;
using Glue.Lib;
using Glue.Data.Mapping;

namespace Glue.Data
{
    /// <summary>
    /// Singleton class for MappingXProvider
    /// </summary>
    public class MappingXProvider
    {
        private static Dictionary<string, IDataProvider> _providersByName = new Dictionary<string, IDataProvider>();
        private static Dictionary<Type, IDataProvider> _providersByType = new Dictionary<Type, IDataProvider>();


        /// <summary>
        /// Return a named MappingXProvider 
        /// </summary>
        /// <returns></returns>
        public static IDataProvider Get(string name)
        {
            IDataProvider provider = _providersByName[name];
            if (provider == null)
            {
                provider = (IDataProvider)Configuration.Get(name);
                _providersByName.Add(name, provider);
            }
            return provider;
        }

        /// <summary>
        /// Return the MappingXProvider corresponding to the given Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns>IDataProvider</returns>
        public static IDataProvider Get(Type type)
        {
            IDataProvider provider = _providersByType[type];
            if (provider == null)
            {
                // get mapping provider from type info
                Entity info = Entity.Obtain(type);
                // Log.Debug("Initializing MappingXProvider for " + typeof(T).FullName + " from configuration element '" + info.Table.DataProvider + "'.");
                provider = Get(info.Table.DataProvider);
                _providersByType.Add(type, provider);
            }
            return provider;
        }

        /// <summary>
        /// Retrieve the default MappingXProvider
        /// </summary>
        /// <remarks>
        /// Works ONLY if there is a dataprovider declared under the default element "dataprovider"!
        /// </remarks>
        public static IDataProvider Default
        {
            get { return Get("dataprovider"); }
        }
    }
}
