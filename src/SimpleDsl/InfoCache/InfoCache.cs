using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl
{
    internal static partial class InfoCache
    {
        internal static readonly Type StrType = typeof(string);
        internal static readonly Type BoolType = typeof(bool);
        internal static readonly Type IntType = typeof(int);
        internal static readonly Type NullableOpenType = typeof(Nullable<>);
        internal static readonly Type IConvType = typeof(IConvertible);
        internal static readonly Type IEnumObjType = typeof(IEnumerable<object>);
        internal static readonly Type IEnumStringType = typeof(IEnumerable<string>);
        internal static readonly Type IEqcmpOpenType = typeof(IEqualityComparer<>);

        //internal static HashSet<Type> NumericTypes = new HashSet<Type>
        //{
        //    typeof(byte),
        //    typeof(sbyte),
        //    typeof(short),
        //    typeof(ushort),
        //    typeof(int),
        //    typeof(uint),
        //    typeof(long),
        //    typeof(ulong)
        //};

        private static EnumerableCache? _eCache;
        private static QueryableCache? _qCache;
        internal static ILinqCache Enumerable => _eCache ??= new EnumerableCache();
        internal static ILinqCache Queryable  => _qCache ??= new QueryableCache();

        static InfoCache()
        {
            var ObjType = typeof(object);
            ObjToString = ObjType.GetMethod(nameof(Object.ToString), Array.Empty<Type>())!;
            ObjRefEquals = ObjType.GetMethod(nameof(Object.ReferenceEquals), new Type[] { ObjType, ObjType })!;

            StrEquals = StrType.GetMethod(nameof(String.Equals), new Type[] { StrType, StrType, typeof(StringComparison) })!;

            var IntTypeArr = new Type[] { IntType };
            IntEquals = IntType.GetMethod(nameof(Int32.Equals), IntTypeArr)!;
            IntCompare = IntType.GetMethod(nameof(Int32.CompareTo), IntTypeArr)!;
        }

        internal static readonly MethodInfo ObjToString;
        internal static readonly MethodInfo ObjRefEquals;

        internal static readonly MethodInfo StrConcat2 = StrType.GetMethod(nameof(String.Concat), new Type[] { StrType, StrType })!;
        internal static readonly MethodInfo StrConcat3 = StrType.GetMethod(nameof(String.Concat), new Type[] { StrType, StrType, StrType })!;
        internal static readonly MethodInfo StrJoin = StrType.GetMethod(nameof(String.Join), new Type[] { StrType, IEnumStringType })!;
        internal static readonly MethodInfo StrEquals;

        internal static readonly MethodInfo IntEquals;
        internal static readonly MethodInfo IntCompare;

        internal static readonly string Colon = ": ";
        internal static readonly ConstantExpression EmptyStrExpr = Expression.Constant(String.Empty);
        internal static readonly ConstantExpression CommaExpr = Expression.Constant(", ");
        internal static readonly ConstantExpression IntNegOneExpr = Expression.Constant(-1);
        internal static readonly ConstantExpression IntZeroExpr = Expression.Constant(0);
        internal static readonly ConstantExpression IntOneExpr = Expression.Constant(1);
        internal static readonly ConstantExpression StrCompsExpr = Expression.Constant(StringComparison.OrdinalIgnoreCase);
        internal static readonly ConstantExpression StrComprExpr = Expression.Constant(StringComparer.OrdinalIgnoreCase);
        internal static readonly ConstantExpression True = Expression.Constant(true);
        internal static readonly ConstantExpression False = Expression.Constant(false);

        internal static readonly BlockExpression EmptyBlock = Expression.Block(Expression.Default(BoolType));

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
        internal static readonly string Or = "or";

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
