using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JiiLib.SimpleDsl
{
    using System.Linq.Expressions;
    using LEnumerable = Enumerable;
    internal static partial class InfoCache
    {
        private sealed class EnumerableCache : ILinqCache
        {
            public EnumerableCache()
            {
                var Func2 = typeof(Func<,>);
                var LinqType = typeof(LEnumerable);
                var TSourceType = Type.MakeGenericMethodParameter(0);
                var TResultType = Type.MakeGenericMethodParameter(1);
                var IEnumTSource = OpenType.MakeGenericType(TSourceType);

                var StrTypeArr = new Type[] { StrType };
                var IEnumIntTypeArr = new Type[] { typeof(IEnumerable<int>) };
                //var IEnumGenParamIntArr = new Type[] { IEnumTSource, IntType };
                var IEnumGenParamFuncToBoolArr = new Type[] { IEnumTSource, Func2.MakeGenericType(TSourceType, BoolType) };
                var IEnumFuncTtoTResultArr = new Type[] { IEnumTSource, Func2.MakeGenericType(new Type[] { TSourceType, TResultType }) };
                var IOrdEnumFuncTtoTResultArr = new Type[] { typeof(IOrderedEnumerable<>).MakeGenericType(TSourceType), Func2.MakeGenericType(new Type[] { TSourceType, TResultType }) };

                LinqSum = LinqType.GetMethod(nameof(LEnumerable.Sum), IEnumIntTypeArr);
                LinqMin = LinqType.GetMethod(nameof(LEnumerable.Min), IEnumIntTypeArr);
                LinqMax = LinqType.GetMethod(nameof(LEnumerable.Max), IEnumIntTypeArr);
                LinqAverage = LinqType.GetMethod(nameof(LEnumerable.Average), IEnumIntTypeArr);
                LinqCountOpen = LinqType.GetMethod(nameof(LEnumerable.Count), 1, new Type[] { IEnumTSource });
                LinqContainsOpen = LinqType.GetMethod(nameof(LEnumerable.Contains), 1, new Type[] { IEnumTSource, TSourceType });

                LinqAny = LinqType.GetMethod(nameof(LEnumerable.Any), 1, IEnumGenParamFuncToBoolArr);
                LinqWhere = LinqType.GetMethod(nameof(LEnumerable.Where), 1, IEnumGenParamFuncToBoolArr);
                LinqSelect = LinqType.GetMethod(nameof(LEnumerable.Select), 2, IEnumFuncTtoTResultArr);
                LinqOBOpen = LinqType.GetMethod(nameof(LEnumerable.OrderBy), 2, IEnumFuncTtoTResultArr);
                LinqOBDOpen = LinqType.GetMethod(nameof(LEnumerable.OrderByDescending), 2, IEnumFuncTtoTResultArr);
                LinqTBOpen = LinqType.GetMethod(nameof(LEnumerable.ThenBy), 2, IOrdEnumFuncTtoTResultArr);
                LinqTBDOpen = LinqType.GetMethod(nameof(LEnumerable.ThenByDescending), 2, IOrdEnumFuncTtoTResultArr);
                IEnumStrContains = LinqType.GetMethod(nameof(LEnumerable.Contains), 1, new Type[] { IEnumTSource, TSourceType, IEqcmpOpenType.MakeGenericType(TSourceType) }).MakeGenericMethod(StrTypeArr);
            }

            public Type OpenType { get; } = typeof(IEnumerable<>);

            public MethodInfo LinqSum { get; }
            public MethodInfo LinqMin { get; }
            public MethodInfo LinqMax { get; }
            public MethodInfo LinqAverage { get; }
            public MethodInfo LinqAny { get; }
            public MethodInfo LinqWhere { get; }
            public MethodInfo LinqSelect { get; }
            public MethodInfo LinqCountOpen { get; }
            public MethodInfo LinqOBOpen { get; }
            public MethodInfo LinqOBDOpen { get; }
            public MethodInfo LinqTBOpen { get; }
            public MethodInfo LinqTBDOpen { get; }
            public MethodInfo LinqContainsOpen { get; }
            public MethodInfo IEnumStrContains { get; }

            private static readonly ConcurrentDictionary<(Type, string), MethodInfo> _methodCache = new ConcurrentDictionary<(Type, string), MethodInfo>();

            public MethodCallExpression Any(Expression items, LambdaExpression filter)
            {
                var t = filter.Parameters[0].Type;
                var m = _methodCache.GetOrAdd((t, "Any"), LinqAny.MakeGenericMethod(t));

                return Expression.Call(m, items, filter);
            }

            public MethodCallExpression Where(Expression items, LambdaExpression filter)
            {
                var t = filter.Parameters[0].Type;
                var m = _methodCache.GetOrAdd((t, "Where"), LinqWhere.MakeGenericMethod(t));

                return Expression.Call(m, items, filter);
            }

            public MethodCallExpression Select(Expression items, LambdaExpression selector)
            {
                var t = selector.Parameters[0].Type;
                var m = _methodCache.GetOrAdd((t, "Select"), LinqSelect.MakeGenericMethod(t, InfoCache.StrType));

                return Expression.Call(m, items, selector);
            }
        }
    }
}
