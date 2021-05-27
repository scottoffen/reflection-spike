using System;
using System.Reflection;

namespace ReflectionSpike
{
    /* Considerations:
     * https://codeblog.jonskeet.uk/2008/08/09/making-reflection-fly-and-exploring-delegates/
     * http://geekswithblogs.net/Madman/archive/2008/06/27/faster-reflection-using-expression-trees.aspx
     * https://mattwarren.org/2016/12/14/Why-is-Reflection-slow/
     */
    public static class MagicMethodMaker
    {
        // Fetch the generic form
        private static readonly MethodInfo _genericHelper = typeof(MagicMethodMaker).GetMethod("MagicMethodHelper", BindingFlags.Static | BindingFlags.NonPublic);

        public static Func<T, object> MakeMagicMethod<T>(MethodInfo method) where T : class
        {
            // Supply the type arguments
            MethodInfo constructedHelper = _genericHelper.MakeGenericMethod(typeof(T), method.ReturnType);

            // Call it. The null argument is because it's a static method.
            object ret = constructedHelper.Invoke(null, new object[] { method });

            // Cast the result to the right kind of delegate and return it
            return (Func<T, object>)ret;
        }

        private static Func<TTarget, object> MagicMethodHelper<TTarget, TReturn>(MethodInfo method) where TTarget : class
        {
            // Convert the slow MethodInfo into a fast, strongly typed, open delegate
            Func<TTarget, TReturn> func = (Func<TTarget, TReturn>)Delegate.CreateDelegate(typeof(Func<TTarget, TReturn>), method);

            // Now create a more weakly typed delegate which will call the strongly typed one
            Func<TTarget, object> ret = (TTarget target) => func(target);
            return ret;
        }
    }
}