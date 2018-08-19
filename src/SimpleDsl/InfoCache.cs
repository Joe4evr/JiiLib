using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl
{
    internal static class InfoCache
    {
        internal static readonly Type ObjType = typeof(object);
        internal static readonly Type StrType = typeof(string);
        internal static readonly Type BoolType = typeof(bool);
        internal static readonly Type IntType = typeof(int);
        internal static readonly Type IConvType = typeof(IConvertible);
        internal static readonly Type IEnumOpenType = typeof(IEnumerable<>);
        //internal static readonly Type IEqcmpOpenType = typeof(IEqualityComparer<>);

        private static readonly Type LinqType = typeof(Enumerable);
        private static readonly Type IEnumIntType = typeof(IEnumerable<int>);
        private static readonly Type GenParam0 = Type.MakeGenericMethodParameter(0);
        private static readonly Type IEnumGen = IEnumOpenType.MakeGenericType(GenParam0);

        private static readonly Type[] StrTypeArr = new Type[] { StrType };
        private static readonly Type[] IntTypeArr = new Type[] { IntType };
        private static readonly Type[] IEnumGenParamArr = new Type[] { IEnumGen };
        private static readonly Type[] IEnumGenParamIntArr = new Type[] { IEnumGen, IntType };

        internal static readonly MethodInfo ObjToString = ObjType.GetMethod(nameof(Object.ToString), Array.Empty<Type>());

        internal static readonly MethodInfo StrConcat = StrType.GetMethod(nameof(String.Concat), new Type[] { StrType, StrType, StrType });
        internal static readonly MethodInfo StrContains = StrType.GetMethod(nameof(String.Contains), StrTypeArr);
        internal static readonly MethodInfo StrEquals = StrType.GetMethod(nameof(String.Equals), new Type[] { StrType, typeof(StringComparison) });
        internal static readonly MethodInfo StrJoin = StrType.GetMethod(nameof(String.Join), new Type[] { StrType, typeof(object[]) });

        internal static readonly MethodInfo IntEquals = IntType.GetMethod(nameof(Int32.Equals), IntTypeArr);
        internal static readonly MethodInfo IntCompare = IntType.GetMethod(nameof(Int32.CompareTo), IntTypeArr);

        internal static readonly MethodInfo LinqSum = LinqType.GetMethod(nameof(Enumerable.Sum), new Type[] { IEnumIntType });
        internal static readonly MethodInfo LinqMin = LinqType.GetMethod(nameof(Enumerable.Min), new Type[] { IEnumIntType });
        internal static readonly MethodInfo LinqMax = LinqType.GetMethod(nameof(Enumerable.Max), new Type[] { IEnumIntType });
        internal static readonly MethodInfo LinqAverage = LinqType.GetMethod(nameof(Enumerable.Average), new Type[] { IEnumIntType });
        //internal static readonly MethodInfo LinqContainsEqOpen = LinqType.GetTypeInfo().DeclaredMethods.Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Count() == 3).GetGenericMethodDefinition();
        internal static readonly MethodInfo LinqCountOpen = LinqType.GetMethod(nameof(Enumerable.Count), 1, IEnumGenParamArr);
        internal static readonly MethodInfo LinqOBOpen  = LinqType.GetMethod(nameof(Enumerable.OrderBy), 2, IEnumGenParamIntArr);
        internal static readonly MethodInfo LinqOBDOpen = LinqType.GetMethod(nameof(Enumerable.OrderByDescending), 2, IEnumGenParamIntArr);
        internal static readonly MethodInfo LinqTBOpen  = LinqType.GetMethod(nameof(Enumerable.ThenBy), 2, IEnumGenParamIntArr);
        internal static readonly MethodInfo LinqTBDOpen = LinqType.GetMethod(nameof(Enumerable.ThenByDescending), 2, IEnumGenParamIntArr);

        private static readonly MethodInfo LinqContainsOpen = LinqType.GetMethod(nameof(Enumerable.Contains), 1, new Type[] { IEnumGen, GenParam0 });
        internal static readonly MethodInfo IEnumStrContains = LinqContainsOpen.MakeGenericMethod(StrTypeArr);
        internal static readonly MethodInfo IEnumIntContains = LinqContainsOpen.MakeGenericMethod(IntTypeArr);

        internal static readonly ConstantExpression CommaExpr = Expression.Constant(", ");
        internal static readonly ConstantExpression ColonExpr = Expression.Constant(": ");
        internal static readonly ConstantExpression StrCompExpr = Expression.Constant(StringComparison.OrdinalIgnoreCase);
        internal static readonly ConstantExpression IntNegOneExpr = Expression.Constant(-1);
        internal static readonly ConstantExpression IntZeroExpr = Expression.Constant(0);
        internal static readonly ConstantExpression IntOneExpr = Expression.Constant(1);

        internal static readonly string Where = "where";
        internal static readonly string OrderBy = "orderby";
        internal static readonly string Skip = "skip";
        internal static readonly string Take = "take";
        internal static readonly string Select = "select";

        internal static readonly string[] Clauses = new[] { Where, OrderBy, Skip, Take, Select };

        internal static readonly string Sum = "sum";
        internal static readonly string Min = "min";
        internal static readonly string Max = "max";
        internal static readonly string Average = "average";
        internal static readonly string Count = "count";

        internal static readonly string Contains = "<-";
        internal static readonly string NotContains = "!<-";
        internal static readonly string LessThan = "<";
        internal static readonly string LessThanOrEqual = "<=";
        internal static readonly string GreaterThan = ">";
        internal static readonly string GreaterThanOrEqual = ">=";
        internal static readonly string IsEqual = "==";
        internal static readonly string IsNotEqual = "!=";
    }
}
