using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReflectionSpike
{
    public static class PropertyInfoCache<T>
    {
        private static List<PropertyInfo> _cache;

        static PropertyInfoCache()
        {
            _cache = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
        }

        public static void Init() { }

        public static Dictionary<string, object> ToDictionary(T obj)
        {
            var result = new Dictionary<string, object>();

            foreach (var property in _cache)
            {
                result[property.Name] = property.GetValue(obj);
            }

            return result;
        }
    }
}