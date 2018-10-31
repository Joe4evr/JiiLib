using System;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl.Nodes
{
    internal sealed class ConstantNode : IQueryNode
    {
        public Expression Value { get; }

        public ConstantNode(object value, Type type)
        {
            Value = Expression.Constant(value, type);
        }
    }
}
