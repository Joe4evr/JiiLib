using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl.Nodes
{
    internal sealed class NullCheckNode : IQueryNode
    {
        public Expression Value { get; }

        public NullCheckNode(Expression expression)
        {
            Value = Expression.IsFalse(IsNull(expression));
        }

        [DebuggerStepThrough]
        private static Expression IsNull(Expression member)
            => Expression.Call(InfoCache.ObjRefEquals, member, Expression.Constant(null, member.Type));
    }
}
