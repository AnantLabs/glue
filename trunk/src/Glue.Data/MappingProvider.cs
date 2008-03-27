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
            IMappingProvider prov;

            if (!_providersByName.ContainsKey(name))
            {
                // initialize mappingprovider
                prov = (IMappingProvider)Configuration.Get(name);
                _providersByName.Add(name, prov);
            }
            else
            {
                prov = _providersByName[name];
            }

            return prov;
        }

        /// <summary>
        /// Return the MappingProvider corresponding to the given Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns>IMappingProvider</returns>
        public static IMappingProvider Get(Type type)
        {
            IMappingProvider prov;

            if (!_providersByType.ContainsKey(type))
            {
                // get mapping provider from type info
                Entity ent = Entity.Obtain(type);
                TableAttribute table = ent.Table;

                // Log.Debug("Initializing MappingProvider for " + typeof(T).FullName + " from configuration element '" + table.MappingProvider + "'.");

                prov = Get(table.MappingProvider);
                _providersByType.Add(type, prov);
            }
            else
            {
                prov = _providersByType[type];
            }
            return prov;
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
