//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;

//namespace JiiLib.SimpleDsl
//{
//    public partial class QueryInterpreter<T>
//    {
//        private static readonly Type _carrierType = typeof(Carrier);
//        private static readonly PropertyInfo _carrierVars = _carrierType.GetProperty(nameof(Carrier.Vars));

//        private static void M(Expression lhs, Expression rhs, Dictionary<string, Func<T, object>> vars)
//        {
//            var key = "";
//            if (lhs is MemberExpression memberExpr && memberExpr.Member is PropertyInfo property)
//            {
//                vars.TryAdd(key, (o) => property.GetMethod.Invoke(o, Array.Empty<object>()));
//            }
//        }

//        private static IEnumerable<Carrier> Filter(IEnumerable<T> items, Func<T, bool> predicate, string propertyName)
//        {
//            var carried = items.Select(i => new Carrier(i));
//            var filtered = carried.Where(c =>
//            {
//                var r = TryGetValue(c.Object, predicate, propertyName, out var valFunc);
//                if (r)
//                {
//                    if (!c.Vars.TryAdd(propertyName, valFunc))
//                        throw new InvalidOperationException("");
//                }

//                return r;
//            });
//            return filtered;
//            //var selection = filtered.Select(c =>
//            //{
//            //    return String.Join(", ", c.Vars.NoCapSelect(c.Object, (n, v) => $"{n}: {v}"));
//            //});
//        }

//        private static bool TryGetValue(T obj, Func<T, bool> predicate, string propertyName, out Func<T, string> valueFunc)
//        {
//            if (predicate(obj))
//            {
//                var prop = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.GetProperty);
//                if (prop != null)
//                {
//                    valueFunc = (o) => prop.GetMethod.Invoke(o, Array.Empty<object>())?.ToString();
//                    return true;
//                }
//            }

//            valueFunc = null;
//            return false;
//        }

//        private sealed class Carrier
//        {
//            public T Object { get; }
//            public Dictionary<string, Func<T, string>> Vars { get; } = new Dictionary<string, Func<T, string>>(StringComparer.OrdinalIgnoreCase);

//            public Carrier(T obj)
//            {
//                Object = obj;
//            }
//        }
//    }
//}
