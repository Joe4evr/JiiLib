using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl
{
    using LQueryable = Queryable;
    internal static partial class InfoCache
    {
        private sealed class QueryableCache : ILinqCache
        {
            public QueryableCache()
            {
                var Func2 = typeof(Func<,>);
                var LinqType = typeof(LQueryable);
                var ExprOpenType = typeof(Expression<>);
                var TSourceType = Type.MakeGenericMethodParameter(0);
                var TResultType = Type.MakeGenericMethodParameter(1);
                var IQueryTSource = OpenType.MakeGenericType(TSourceType);

                var IQueryIntTypeArr = new Type[] { typeof(IQueryable<int>) };
                //var IQueryGenParamIntArr = new Type[] { IQueryTSource, IntType };
                var IQueryGenParamFuncToBoolArr = new Type[] { IQueryTSource, ExprOpenType.MakeGenericType(Func2.MakeGenericType(TSourceType, BoolType)) };
                var IQueryFuncTtoTResultArr = new Type[] { IQueryTSource, ExprOpenType.MakeGenericType(Func2.MakeGenericType(new Type[] { TSourceType, TResultType })) };
                var IOrdQueryFuncTtoTResultArr = new Type[] { typeof(IOrderedQueryable<>).MakeGenericType(TSourceType), ExprOpenType.MakeGenericType(Func2.MakeGenericType(new Type[] { TSourceType, TResultType })) };

                LinqSum = LinqType.GetMethod(nameof(LQueryable.Sum), IQueryIntTypeArr);
                LinqMin = LinqType.GetMethod(nameof(LQueryable.Min), IQueryIntTypeArr);
                LinqMax = LinqType.GetMethod(nameof(LQueryable.Max), IQueryIntTypeArr);
                LinqAverage = LinqType.GetMethod(nameof(LQueryable.Average), IQueryIntTypeArr);
                LinqCountOpen = LinqType.GetMethod(nameof(LQueryable.Count), 1, new Type[] { IQueryTSource });
                LinqContainsOpen = LinqType.GetMethod(nameof(LQueryable.Contains), 1, new Type[] { IQueryTSource, TSourceType });

                LinqAny = LinqType.GetMethod(nameof(LQueryable.Any), 1, IQueryGenParamFuncToBoolArr);
                LinqWhere = LinqType.GetMethod(nameof(LQueryable.Where), 1, IQueryGenParamFuncToBoolArr);
                LinqSelect = LinqType.GetMethod(nameof(LQueryable.Select), 2, IQueryFuncTtoTResultArr);
                LinqOBOpen = LinqType.GetMethod(nameof(LQueryable.OrderBy), 2, IQueryFuncTtoTResultArr);
                LinqOBDOpen = LinqType.GetMethod(nameof(LQueryable.OrderByDescending), 2, IQueryFuncTtoTResultArr);
                LinqTBOpen = LinqType.GetMethod(nameof(LQueryable.ThenBy), 2, IOrdQueryFuncTtoTResultArr);
                LinqTBDOpen = LinqType.GetMethod(nameof(LQueryable.ThenByDescending), 2, IOrdQueryFuncTtoTResultArr);
            }

            public Type OpenType { get; } = typeof(IQueryable<>);

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
            public MethodInfo IEnumStrContains => throw new NotSupportedException();

            private static readonly ConcurrentDictionary<(Type, string), MethodInfo> _methodCache = new ConcurrentDictionary<(Type, string), MethodInfo>();

            public MethodCallExpression Any(Expression items, LambdaExpression filter)
            {
                var t = filter.Parameters[0].Type;
                var m = _methodCache.GetOrAdd((t, "Any"), LinqAny.MakeGenericMethod(t));

                return Expression.Call(m, items, Expression.Quote(filter));
            }

            public MethodCallExpression Where(Expression items, LambdaExpression filter)
            {
                var t = filter.Parameters[0].Type;
                var m = _methodCache.GetOrAdd((t, "Where"), LinqWhere.MakeGenericMethod(t));

                return Expression.Call(m, items, Expression.Quote(filter));
            }

            public MethodCallExpression Select(Expression items, LambdaExpression selector)
            {
                var t = selector.Parameters[0].Type;
                var m = _methodCache.GetOrAdd((t, "Select"), LinqSelect.MakeGenericMethod(t));

                return Expression.Call(m, items, Expression.Quote(selector));
            }
        }
    }
}
