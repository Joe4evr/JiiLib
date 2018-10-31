using System;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl
{
    internal interface ILinqCache
    {
        Type OpenType { get; }
        MethodInfo LinqSum { get; }
        MethodInfo LinqMin { get; }
        MethodInfo LinqMax { get; }
        MethodInfo LinqAverage { get; }
        MethodInfo LinqAny { get; }
        MethodInfo LinqWhere { get; }
        MethodInfo LinqSelect { get; }
        MethodInfo LinqCountOpen { get; }
        MethodInfo LinqOBOpen { get; }
        MethodInfo LinqOBDOpen { get; }
        MethodInfo LinqTBOpen { get; }
        MethodInfo LinqTBDOpen { get; }
        MethodInfo LinqContainsOpen { get; }
        MethodInfo IEnumStrContains { get; }

        MethodCallExpression Any(Expression items, LambdaExpression filter);
        MethodCallExpression Where(Expression items, LambdaExpression filter);
        MethodCallExpression Select(Expression items, LambdaExpression selector);
    }
}
