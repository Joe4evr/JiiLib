﻿using System;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl.Nodes
{
    internal sealed class BinaryOperationNode : IQueryNode
    {
        public IQueryNode Left { get; }
        public BinaryOperatorNode Operator { get; }
        public IQueryNode Right { get; }

        public Expression Value { get; }

        public BinaryOperationNode(IQueryNode left, BinaryOperatorNode op, IQueryNode right)
        {
            Left = left;
            Operator = op;
            Right = right;

            var (val, truthy) = DslHelpers.CreateOperatorExpression(left.Value, op.Operator, right.Value);
            Value = truthy ? Expression.IsTrue(val) : Expression.IsFalse(val);
        }
    }
}
