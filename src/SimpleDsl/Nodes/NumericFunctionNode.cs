using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl.Nodes
{
    internal sealed class NumericFunctionNode : IQueryNode
    {
        public IReadOnlyCollection<IQueryNode> Arguments { get; }
        public Expression Value { get; }

        public NumericFunctionNode(MethodInfo method, IEnumerable<IQueryNode> arguments)
        {
            if (arguments.Any(node => node.Value.Type != InfoCache.IntType))
                throw new InvalidOperationException();

            Arguments = ImmutableArray.CreateRange(arguments);
            Value = Expression.Call(method, Expression.NewArrayInit(InfoCache.IntType, arguments.Select(node => node.Value)));
        }
    }
}
