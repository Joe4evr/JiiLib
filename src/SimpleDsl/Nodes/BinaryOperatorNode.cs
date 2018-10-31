using System;

namespace JiiLib.SimpleDsl.Nodes
{
    internal sealed class BinaryOperatorNode
    {
        public Operator Operator { get; }

        public BinaryOperatorNode(Operator op)
        {
            Operator = op;
        }
    }
}
