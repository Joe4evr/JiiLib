using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using JiiLib.SimpleDsl.Nodes;

namespace JiiLib.SimpleDsl
{
    internal sealed class QueryModel
    {
        internal IReadOnlyDictionary<string, InlineVariableNode> InlineVars => _inlineVars;
        internal WhereClause WhereClause { get; } = new WhereClause();
        //internal OrderByClause OrderByClause { get; } = new OrderByClause();

        internal int Skip { get; set; }
        internal int Take { get; set; }
        //private readonly LinkedList<IQueryNode> _selectNodes = new LinkedList<IQueryNode>();
        private readonly Dictionary<string, InlineVariableNode> _inlineVars
            = new Dictionary<string, InlineVariableNode>(StringComparer.OrdinalIgnoreCase);

        internal void AddInlineVar(string name, IQueryNode node)
        {
            if (String.IsNullOrWhiteSpace(name))
                return;

            if (!_inlineVars.TryAdd(name, new InlineVariableNode(node)))
                throw new InvalidOperationException($"Inline variable identifier '{name}' is already used.");
        }

        internal void AddWhereNode(IQueryNode node)
        {
            WhereClause.Add(node);
        }

        //internal void AddOrderByNode(IQueryNode node)
        //{
        //    OrderByClause.Add(node);
        //}
    }
}
