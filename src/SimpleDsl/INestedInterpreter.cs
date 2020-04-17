using System;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl
{
    internal interface INestedInterpreter
    {
        LambdaExpression ParseNestedWhere(ReadOnlySpan<char> span, QueryModel model, ILinqCache linqCache);
        //LambdaExpression ParseNestedOrderBy(ReadOnlySpan<char> span, QueryModel model, ILinqCache linqCache);
        //LambdaExpression ParseNestedSelect(ReadOnlySpan<char> span, QueryModel model, ILinqCache linqCache);
    }
}
