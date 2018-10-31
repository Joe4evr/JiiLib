using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace JiiLib.SimpleDsl.Nodes
{
    internal sealed class BasicNode : IQueryNode
    {
        public Expression Value { get; }

        public BasicNode(Expression expression)
        {
            Value = expression;
        }
    }
}
