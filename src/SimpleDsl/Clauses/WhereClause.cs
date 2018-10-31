using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl
{
    internal sealed class WhereClause
    {
        private List<ParameterExpression> BlkVars { get; } = new List<ParameterExpression>();
        private List<IQueryNode> Nodes { get; } = new List<IQueryNode>();

        public void Add(IQueryNode node)
        {
            if (node.Value.Type != InfoCache.BoolType)
                throw new InvalidOperationException();

            if (node.Value.IsOrHasBlock(out var block))
                BlkVars.AddRange(block.Variables);

            Nodes.Add(node);
        }

        public BlockExpression BuildBlock()
        {
            var result = Expression.Variable(InfoCache.BoolType);
            BlkVars.Add(result);
            var exprs = new List<Expression>()
            {
                Expression.Assign(result, InfoCache.True),
            };

            foreach (var node in Nodes)
            {
                exprs.Add(Expression.Assign(result, Expression.AndAlso(result, node.Value)));
            }

            return Expression.Block(BlkVars, exprs);
        }
    }
}
