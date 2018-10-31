using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl.Nodes
{
    internal sealed class OrFunctionNode : IQueryNode
    {
        public IReadOnlyCollection<IQueryNode> Arguments { get; }
        public Expression Value { get; }

        public OrFunctionNode(IEnumerable<IQueryNode> arguments)
        {
            if (arguments.Any(node => node.Value.Type != InfoCache.BoolType))
                throw new InvalidOperationException();

            Arguments = ImmutableArray.CreateRange(arguments);
            Value = BuildValue();
        }

        private Expression BuildValue()
        {
            var tmpRes = Expression.Variable(InfoCache.BoolType);
            var tmpVars = new List<ParameterExpression>() { tmpRes };
            var tmpExprs = new List<Expression>()
            {
                Expression.Assign(tmpRes, InfoCache.False)
            };

            foreach (var node in Arguments)
            {
                tmpExprs.Add(
                    Expression.Assign(
                        tmpRes,
                        Expression.OrElse(tmpRes, node.Value)));
            }

            return Expression.Block(tmpVars, tmpExprs);
        }
    }
}
