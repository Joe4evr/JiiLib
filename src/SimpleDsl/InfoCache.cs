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
        internal static readonly Type LinqType = typeof(Enumerable);
        internal static readonly Type IEnumType = typeof(IEnumerable<>);
        internal static readonly Type[] StrTypeArr = new Type[] { StrType };
        internal static readonly Type[] StrCompTypeArr = new Type[] { StrType, typeof(StringComparison) };
        internal static readonly Type[] IntTypeArr = new Type[] { IntType };

        internal static readonly MethodInfo ObjToString = ObjType.GetMethod("ToString");
        internal static readonly MethodInfo StrJoin = StrType.GetMethod("Join", new Type[] { StrType, typeof(object[]) });
        internal static readonly MethodInfo StrContains = StrType.GetMethod("Contains", StrTypeArr);
        internal static readonly MethodInfo IEnumStrContains = typeof(InfoCache).GetMethod("ContainsString", new Type[] { typeof(IEnumerable<string>), StrType });
        internal static readonly MethodInfo StrConcat = StrType.GetMethod("Concat", new Type[] { StrType, StrType, StrType });
        internal static readonly MethodInfo StrEquals = StrType.GetMethod("Equals", StrCompTypeArr);
        internal static readonly MethodInfo IntEquals = IntType.GetMethod("Equals", IntTypeArr);
        internal static readonly MethodInfo IntCompare = IntType.GetMethod("CompareTo", IntTypeArr);
        internal static readonly MethodInfo IEnumIntContains = typeof(InfoCache).GetMethod("ContainsInt", new Type[] { typeof(IEnumerable<int>), IntType });
        internal static readonly MethodInfo LinqSum = LinqType.GetMethod("Sum", new Type[] { typeof(IEnumerable<int>) });

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

        internal static readonly string Contains = "<-";
        internal static readonly string NotContains = "!<-";
        internal static readonly string LessThan = "<";
        internal static readonly string LessThanOrEqual = "<=";
        internal static readonly string GreaterThan = ">";
        internal static readonly string GreaterThanOrEqual = ">=";
        internal static readonly string IsEqual = "==";
        internal static readonly string IsNotEqual = "!=";

        public static bool ContainsString(IEnumerable<string> strs, string str)
            => strs.Any(s => s.Equals(str, StringComparison.OrdinalIgnoreCase));
        public static bool ContainsInt(IEnumerable<int> ints, int i)
            => ints.Contains(i);
    }
}
