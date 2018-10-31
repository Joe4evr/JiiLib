using System;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl.Nodes
{
    internal sealed class PropertyAccessNode : IQueryNode
    {
        public ParameterExpression Parent { get; }
        public PropertyInfo Property { get; }
        public Expression Value { get; }

        public PropertyAccessNode(ParameterExpression parentObject, PropertyInfo property)
        {
            Parent = parentObject;
            Property = property;
            Value = Expression.Property(parentObject, property);
        }
    }
}
