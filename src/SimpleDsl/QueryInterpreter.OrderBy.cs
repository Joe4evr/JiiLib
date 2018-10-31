using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JiiLib.SimpleDsl.Nodes;

namespace JiiLib.SimpleDsl
{
    public partial class QueryInterpreter<T>
    {
        private IEnumerable<OrderByExpression<T>> ParseOrderByClauses(ReadOnlySpan<char> orderBySpan, ILinqCache linqCache)
        {
            var exprs = new List<OrderByExpression<T>>();

            for (var slice = orderBySpan.SliceUntilFirstUnnested(',', out var next); slice.Length > 0; slice = next.SliceUntilFirstUnnested(',', out next))
            {
                var identifier = ParseVarDecl(ref slice);
                bool isDesc = slice.ParseIsDescending();
                var invocation = ParseFunctionOrInvocation(slice, linqCache);
                _model.AddInlineVar(identifier, new BasicNode(invocation));

                var lambda = Expression.Lambda<Func<T, int>>(invocation, _targetParamExpr);
                exprs.Add(new OrderByExpression<T>(lambda, isDesc));
            }

            return exprs;
        }

        private static Expression ParseFunctionOrInvocation(ReadOnlySpan<char> slice, ILinqCache linqCache)
        {
            if (ParseKnownFunction(slice, linqCache) is Expression knownFunc)
                return knownFunc;

            var p = slice.Materialize();
            if (Property(p) is PropertyInfo property)
                return _targetParamExpr.PropertyAccess(property);

            throw new InvalidOperationException($"No such function or property '{p}'.");
        }
    }
}
