#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LoadingScreenKit.Scripts
{
    public static class TypeUtility
    {
        public static IEnumerable<Type> GetTypesAssignableFrom<T>() =>
            GetTypesAssignableFrom(typeof(T));

        public static IEnumerable<Type> GetTypesAssignableFrom(Type type) =>
            AppDomain.CurrentDomain.GetAssemblies()
                .AsParallel()
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        return ex.Types.Where(t => t != null);
                    }
                    catch
                    {
                        return Enumerable.Empty<Type>();
                    }
                })
                .Where(type.IsAssignableFrom)
                .Where(t => !t.IsAbstract)
                .Where(t => t != type);
    }

}
#endif
