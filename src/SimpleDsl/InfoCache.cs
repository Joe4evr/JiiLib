﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl
{
    internal static class InfoCache
    {
        internal static readonly Type StrType = typeof(string);
        internal static readonly Type BoolType = typeof(bool);
        internal static readonly Type IntType = typeof(int);
        internal static readonly Type IConvType = typeof(IConvertible);
        internal static readonly Type IEnumOpenType = typeof(IEnumerable<>);
        internal static readonly Type IEnumStringType = typeof(IEnumerable<string>);
        internal static readonly Type IEqcmpOpenType = typeof(IEqualityComparer<>);

        static InfoCache()
        {
            var TSourceType = Type.MakeGenericMethodParameter(0);
            var TResultType = Type.MakeGenericMethodParameter(1);
            var IEnumTSource = IEnumOpenType.MakeGenericType(TSourceType);
            var FuncTToTR = typeof(Func<,>);
            var FuncTSourceToBool = FuncTToTR.MakeGenericType(TSourceType, BoolType);

            var StrTypeArr = new Type[] { StrType };
            var IntTypeArr = new Type[] { IntType };
            var IEnumIntTypeArr = new Type[] { typeof(IEnumerable<int>) };
            var IEnumGenParamArr = new Type[] { IEnumTSource };
            var IEnumGenParamIntArr = new Type[] { IEnumTSource, IntType };
            var IEnumGenParamFuncToBoolArr = new Type[] { IEnumTSource, FuncTSourceToBool };
            var StrStrCompArr = new Type[] { StrType, typeof(StringComparison) };


            StrContains = StrType.GetMethod(nameof(String.Contains), StrStrCompArr);
            StrEquals = StrType.GetMethod(nameof(String.Equals), StrStrCompArr);

            IntEquals = IntType.GetMethod(nameof(Int32.Equals), IntTypeArr);
            IntCompare = IntType.GetMethod(nameof(Int32.CompareTo), IntTypeArr);

            var LinqType = typeof(Enumerable);
            LinqSum = LinqType.GetMethod(nameof(Enumerable.Sum), IEnumIntTypeArr);
            LinqMin = LinqType.GetMethod(nameof(Enumerable.Min), IEnumIntTypeArr);
            LinqMax = LinqType.GetMethod(nameof(Enumerable.Max), IEnumIntTypeArr);
            LinqAverage = LinqType.GetMethod(nameof(Enumerable.Average), IEnumIntTypeArr);
            LinqAny = LinqType.GetMethod(nameof(Enumerable.Any), 1, IEnumGenParamFuncToBoolArr);
            LinqWhere = LinqType.GetMethod(nameof(Enumerable.Where), 1, IEnumGenParamFuncToBoolArr);
            LinqSelect = LinqType.GetMethod(nameof(Enumerable.Select), 2, new Type[] { IEnumTSource, FuncTToTR.MakeGenericType(new Type[] { TSourceType, TResultType }) });
            LinqCountOpen = LinqType.GetMethod(nameof(Enumerable.Count), 1, IEnumGenParamArr);
            LinqOBOpen = LinqType.GetMethod(nameof(Enumerable.OrderBy), 2, IEnumGenParamIntArr);
            LinqOBDOpen = LinqType.GetMethod(nameof(Enumerable.OrderByDescending), 2, IEnumGenParamIntArr);
            LinqTBOpen = LinqType.GetMethod(nameof(Enumerable.ThenBy), 2, IEnumGenParamIntArr);
            LinqTBDOpen = LinqType.GetMethod(nameof(Enumerable.ThenByDescending), 2, IEnumGenParamIntArr);
            LinqContainsOpen = LinqType.GetMethod(nameof(Enumerable.Contains), 1, new Type[] { IEnumTSource, TSourceType });
            IEnumStrContains = LinqType.GetMethod(nameof(Enumerable.Contains), 1, new Type[] { IEnumTSource, TSourceType, IEqcmpOpenType.MakeGenericType(TSourceType) }).MakeGenericMethod(StrTypeArr);
            //IEnumIntContains = LinqContainsOpen.MakeGenericMethod(IntTypeArr);
        }

        internal static readonly MethodInfo ObjToString = typeof(object).GetMethod(nameof(Object.ToString), Array.Empty<Type>());

        internal static readonly MethodInfo StrConcat2 = StrType.GetMethod(nameof(String.Concat), new Type[] { StrType, StrType });
        internal static readonly MethodInfo StrConcat3 = StrType.GetMethod(nameof(String.Concat), new Type[] { StrType, StrType, StrType });
        internal static readonly MethodInfo StrJoin = StrType.GetMethod(nameof(String.Join), new Type[] { StrType, IEnumStringType });
        internal static readonly MethodInfo StrContains;
        internal static readonly MethodInfo StrEquals;

        internal static readonly MethodInfo IntEquals;
        internal static readonly MethodInfo IntCompare;

        internal static readonly MethodInfo LinqSum;
        internal static readonly MethodInfo LinqMin;
        internal static readonly MethodInfo LinqMax;
        internal static readonly MethodInfo LinqAverage;
        internal static readonly MethodInfo LinqAny;
        internal static readonly MethodInfo LinqWhere;
        internal static readonly MethodInfo LinqSelect;
        internal static readonly MethodInfo LinqCountOpen;
        internal static readonly MethodInfo LinqOBOpen;
        internal static readonly MethodInfo LinqOBDOpen;
        internal static readonly MethodInfo LinqTBOpen;
        internal static readonly MethodInfo LinqTBDOpen;

        internal static readonly MethodInfo LinqContainsOpen;
        internal static readonly MethodInfo IEnumStrContains;
        //internal static readonly MethodInfo IEnumIntContains;

        internal static readonly Func<Expression, Expression, Expression> AndAlso = Expression.AndAlso;
        internal static readonly Func<Expression, Expression, Expression> OrElse = Expression.OrElse;

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
