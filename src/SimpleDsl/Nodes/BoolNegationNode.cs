using System;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl.Nodes
{
    internal sealed class BoolNegationNode : IQueryNode
    {
        public IQueryNode Original { get; }
        public Expression Value { get; }

        public BoolNegationNode(IQueryNode node)
        {
            if (node.Value.Type != InfoCache.BoolType)
                throw new InvalidOperationException();

            Original = node;
            Value = Expression.IsFalse(node.Value);
        }
    }
}
