using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using JiiLib.SimpleDsl.Nodes;

namespace JiiLib.SimpleDsl
{
    public partial class QueryInterpreter<T>
    {
        private struct ValueBag
        {
            [DebuggerStepThrough]
            public ValueBag(
                IReadOnlyDictionary<string, InlineVariableNode> vars,
                Expression<Func<T, bool>> predicate,
                ImmutableArray<OrderByExpression<T>> order,
                int skip,
                int take,
                Func<T, string> selector)
            {
                InlineVars = vars;
                Predicate = predicate;
                Order = order;
                Skip = skip;
                Take = take;
                Selector = selector;
            }

            public IReadOnlyDictionary<string, InlineVariableNode> InlineVars { get; }
            public Expression<Func<T, bool>> Predicate { get; }
            public ImmutableArray<OrderByExpression<T>> Order { get; }
            public int Skip { get; }
            public int Take { get; }
            public Func<T, string> Selector { get; }

            public QueryableQueryParseResult<T> ToQueryResult()
            {
                //var vars = InlineVars.ToImmutableDictionary(iv => iv.Key, iv => Expression.Lambda<Func<T, string>>(iv.Value.Member.Stringify(), iv.Value.Parent));

                return new QueryableQueryParseResult<T>(null, Predicate, Order, Skip, Take, Selector);
            }

            public EnumerableQueryParseResult<T> ToEnumerableResult()
            {
                //var vars = InlineVars.ToImmutableDictionary(iv => iv.Key, iv => Expression.Lambda<Func<T, string>>(iv.Value.Member.Stringify(), iv.Value.Parent));
                var predicate = Predicate?.Compile();
                var orderFuncs = Order.Select(e => e.Compile()).ToImmutableArray();

                return new EnumerableQueryParseResult<T>(null, predicate, orderFuncs, Skip, Take, Selector);
            }
        }
    }
}
