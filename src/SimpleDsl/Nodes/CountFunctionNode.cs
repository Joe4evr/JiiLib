using System;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl.Nodes
{
    internal sealed class CountFunctionNode : IQueryNode
    {
        public IQueryNode Argument { get; }
        public Expression Value { get; }

        public CountFunctionNode(MethodInfo openMethod, PropertyAccessNode argument)
        {
            if (!argument.Value.Type.IsEnumerableType(out var eType))
                throw new InvalidOperationException($"Property '{argument.Property.Name}' must be a collection type to be used in 'count()'.");

            Argument = argument;
            Value = Expression.Call(openMethod.MakeGenericMethod(eType), Argument.Value);
        }
    }
}
