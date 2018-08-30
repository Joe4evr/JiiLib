using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl
{
    internal interface INestedInterpreter
    {
        Expression ParseNestedWhere(
            ReadOnlySpan<char> span,
            //MemberExpression parentPropExpr,
            ParameterExpression memberExpr,
            Expression resExpr,
            Dictionary<string, Expression> vars);
        //Expression ParseNestedOrderBy(ReadOnlySpan<char> span, Expression parentPropExpr, Dictionary<string, Expression> vars);
        //Expression ParseNestedSelect(ReadOnlySpan<char> span, Expression parentPropExpr, IReadOnlyDictionary<string, Expression> vars);
    }
}
