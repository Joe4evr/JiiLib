using System;
using System.Collections.Concurrent;
using System.Text;

namespace JiiLib.SimpleDsl
{
    public static class QueryLookups
    {
        internal static ConcurrentDictionary<Type, IOperatorLookup> MethodLookups { get; } = new ConcurrentDictionary<Type, IOperatorLookup>();

        static QueryLookups()
        {
            RegisterLookup(StringOperatorLookup.Instance);
        }

        public static void RegisterLookup<T>(OperatorLookup<T> lookup)
        {
            var type = typeof(T);
            MethodLookups.GetOrAdd(type, lookup);

            //if (type.IsValueType)
            //    MethodLookups.GetOrAdd(type, (k) => (IOperatorLookup)Activator.CreateInstance(typeof(NullableOperatorLookup<>).MakeGenericType(type)));
        }

        internal static IOperatorLookup GetLookup(Type type)
        {
            if (MethodLookups.TryGetValue(type, out var lookup))
                return lookup;

            if (type.IsEnum)
                return MethodLookups.GetOrAdd(type, (k) => (IOperatorLookup)Activator.CreateInstance(typeof(EnumOperatorLookup<>).MakeGenericType(k)));

            if (typeof(IComparable<>).MakeGenericType(type).IsAssignableFrom(type))
                return MethodLookups.GetOrAdd(type, (k) => (IOperatorLookup)Activator.CreateInstance(typeof(ComparableOperatorLookup<>).MakeGenericType(k)));

            if (type.IsCollectionType(out var eType))
                return MethodLookups.GetOrAdd(type, (k) => (IOperatorLookup)Activator.CreateInstance(typeof(EnumerableOperatorLookup<>).MakeGenericType(eType)));

            throw new InvalidOperationException($"No operator lookup found for property of type '{type}'.");
        }
    }
}
