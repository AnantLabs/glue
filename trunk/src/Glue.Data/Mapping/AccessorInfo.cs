using System;
using System.Collections.Generic;
using System.Text;

namespace Glue.Data.Mapping
{
    public class AccessorInfo
    {
        public string FindCommandText;
        public string InsertCommandText;
        public string UpdateCommandText;
        public string DeleteCommandText;
        public string ReplaceCommandText;
        public Accessor Accessor;

        static Dictionary<Type, Dictionary<Type, AccessorInfo>> _cache = new Dictionary<Type, Dictionary<Type, AccessorInfo>>();

        public static AccessorInfo Obtain(BaseDataProvider provider, Type type)
        {
            Type providerType = provider.GetType();
            Dictionary<Type, AccessorInfo> bag;
            if (!_cache.TryGetValue(providerType, out bag))
            {
                bag = new Dictionary<Type, AccessorInfo>();
                _cache.Add(providerType, bag);
            }
            AccessorInfo info;
            if (!bag.TryGetValue(type, out info))
            {
                info = new AccessorInfo();
                info.Accessor = provider.CreateAccessor(type);
                bag.Add(type, info);
            }
            return info;
        }
    }
}
