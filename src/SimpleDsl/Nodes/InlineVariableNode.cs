using System;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl.Nodes
{
    internal sealed class InlineVariableNode : IQueryNode
    {
        public IQueryNode ValueNode { get; }

        public Expression Value => ValueNode.Value;

        public InlineVariableNode(IQueryNode value)
        {
            ValueNode = value;
        }
    }
}
