using System;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl
{
    internal interface IQueryNode
    {
        Expression Value { get; }
    }
}
