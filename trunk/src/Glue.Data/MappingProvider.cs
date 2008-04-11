using System;
using System.Collections.Generic;
using System.Text;
using Glue.Lib;
using Glue.Data.Mapping;

namespace Glue.Data
{
    /// <summary>
    /// Singleton class for MappingProvider
    /// </summary>
    public class MappingProvider
    {
        private static Dictionary<string, IMappingProvider> _providersByName = new Dictionary<string, IMappingProvider>();
        private static Dictionary<Type, IMappingProvider> _providersByType = new Dictionary<Type, IMappingProvider>();


        /// <summary>
        /// Return a named MappingProvider 
        /// </summary>
        /// <returns></returns>
        public static IMappingProvider Get(string name)
        {
            IMappingProvider provider = _providersByName[name];
            if (provider == null)
            {
                // initialize mappingprovider
                provider = (IMappingProvider)Configuration.Get(name);
                _providersByName.Add(name, provider);
            }
            return provider;
        }

        /// <summary>
        /// Return the MappingProvider corresponding to the given Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns>IMappingProvider</returns>
        public static IMappingProvider Get(Type type)
        {
            IMappingProvider provider = _providersByType[type];
            if (provider == null)
            {
                // get mapping provider from type info
                Entity info = Entity.Obtain(type);
                // Log.Debug("Initializing MappingProvider for " + typeof(T).FullName + " from configuration element '" + info.Table.MappingProvider + "'.");
                provider = Get(info.Table.MappingProvider);
                _providersByType.Add(type, provider);
            }
            return provider;
        }

        /// <summary>
        /// Retrieve the default MappingProvider
        /// </summary>
        /// <remarks>
        /// Works ONLY if there is a dataprovider declared under the default element "dataprovider"!
        /// </remarks>
        public static IMappingProvider Default
        {
            get { return Get("dataprovider"); }
        }
    }
}
