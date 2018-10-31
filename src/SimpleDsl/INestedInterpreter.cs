using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl
{
    internal interface INestedInterpreter
    {
        LambdaExpression ParseNestedWhere(ReadOnlySpan<char> span, ILinqCache linqCache);
        //LambdaExpression ParseNestedOrderBy(ReadOnlySpan<char> span, ILinqCache linqCache);
        //LambdaExpression ParseNestedSelect(ReadOnlySpan<char> span, ILinqCache linqCache);
    }
}
