using System;
using System.Collections.Generic;
using System.Reflection;

namespace ReflectionSpike
{
    public static class Parakeet<T> where T : class
    {
        private static Dictionary<string, Func<T, object>> _cache = new Dictionary<string, Func<T, object>>();

        static Parakeet()
        {
            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var property in properties)
            {
                _cache.Add(property.Name, Parakeet.GenerateDelegate<T>(property.GetGetMethod()));
            }
        }

        public static void Init() { }

        public static Dictionary<string, object> ToDictionary(T obj)
        {
            var result = new Dictionary<string, object>();

            foreach (var kvp in _cache)
            {
                result.Add(kvp.Key, kvp.Value(obj));
            }

            return result;
        }

    }

    internal static class Parakeet
    {
        public static Func<T, object> GenerateDelegate<T>(MethodInfo method) where T : class
        {
            // First fetch the generic form
            MethodInfo genericHelper = typeof(Parakeet).GetMethod("GenerateDelegateHelper", BindingFlags.Static | BindingFlags.NonPublic);

            // Now supply the type arguments
            MethodInfo constructedHelper = genericHelper.MakeGenericMethod(typeof(T), method.ReturnType);

            // Now call it. The null argument is because it's a static method.
            object ret = constructedHelper.Invoke(null, new object[] { method });

            // Cast the result to the right kind of delegate and return it
            return (Func<T, object>)ret;
        }

        private static Func<TTarget, object> GenerateDelegateHelper<TTarget, TReturn>(MethodInfo method) where TTarget : class
        {
            // Convert the slow MethodInfo into a fast, strongly typed, open delegate
            Func<TTarget, TReturn> func = (Func<TTarget, TReturn>)Delegate.CreateDelegate(typeof(Func<TTarget, TReturn>), method);

            // Now create a more weakly typed delegate which will call the strongly typed one
            Func<TTarget, object> ret = (TTarget target) => func(target);
            return ret;
        }

    }
}