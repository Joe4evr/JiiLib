using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JiiLib.SimpleDsl.Nodes;

namespace JiiLib.SimpleDsl
{
    public partial class QueryInterpreter<T>
    {
        private Expression<Func<T, bool>> ParseWhereClause(ReadOnlySpan<char> whereSpan, ILinqCache linqCache)
        {
            if (!_targetParamExpr.Type.IsValueType)
                _model.AddWhereNode(new NullCheckNode(_targetParamExpr));

            var commaSplit = whereSpan.CreateSplitter(',');
            while (commaSplit.TryMoveNext(out var slice))
            {
                var node = ParseNode(slice, InfoCache.Enumerable);
                _model.AddWhereNode(node);
            }

            var block = _model.WhereClause.BuildBlock();

            return Expression.Lambda<Func<T, bool>>(block, _targetParamExpr);
        }

        private IQueryNode ParseNode(ReadOnlySpan<char> span, ILinqCache linqCache)
        {
            var identifier = ParseVarDecl(ref span);
            var negatedBool = DslHelpers.IsNegatedBool(ref span);

            if (span.StartsWith(InfoCache.Or.AsSpan()))
            {
                IQueryNode node = BuildOrNode(span.Slice(InfoCache.Or.Length).VerifyOpenChar('(', InfoCache.Or).TrimBraces(), linqCache);
                return (negatedBool) ? new BoolNegationNode(node) : node;
            }
            else
            {
                var splitter = span.CreateSplitter(' ');
                splitter.TryMoveNext(out var lhs);
                var leftNode = ParseLhs(lhs.Materialize(), linqCache);

                _model.AddInlineVar(identifier, leftNode);

                if (leftNode.Value.Type == InfoCache.BoolType)
                    return (negatedBool) ? new BoolNegationNode(leftNode) : leftNode;
                else if (!splitter.TryMoveNext(out var op))
                    throw new InvalidOperationException("Non-boolean operands require an operator and comparand.");
                else if (leftNode is PropertyAccessNode property && op[0] == '{')
                {
                    var propertyType = property.Property.PropertyType;
                    if (propertyType.IsPrimitive)
                        throw new InvalidOperationException($"Property '{property.Property.Name}' must be a non-primitive type to allow nested queries.");

                    if (!propertyType.IsValueType)
                        _model.WhereClause.Add(new NullCheckNode(property.Value));

                    return BuildNestedQuery(property, op, linqCache);
                }
                else
                {
                    if (splitter.TryMoveNext(out var rhs))
                        return BuildBinaryOpNode(leftNode, op, rhs, linqCache);
                    else
                        throw new InvalidOperationException();
                }
            }
        }

        private OrFunctionNode BuildOrNode(ReadOnlySpan<char> span, ILinqCache linqCache)
        {
            var nodes = new List<IQueryNode>();

            var commaSplit = span.CreateSplitter(',');
            while (commaSplit.TryMoveNext(out var slice))
            {
                nodes.Add(ParseNode(slice, linqCache));
            }

            return new OrFunctionNode(nodes);
        }
        private static BinaryOperationNode BuildBinaryOpNode(IQueryNode leftNode, ReadOnlySpan<char> opSpan, ReadOnlySpan<char> rhsSpan, ILinqCache linqCache)
        {
            var opNode = new BinaryOperatorNode(DslHelpers.ParseOperator(opSpan));
            var rightNode = ParseRhs(rhsSpan.TrimBraces().Materialize(), leftNode.Value.Type);
            return new BinaryOperationNode(leftNode, opNode, rightNode);
        }

        private static IQueryNode ParseLhs(ReadOnlySpan<char> span, ILinqCache linqCache)
        {
            if (ParseKnownFunction(span, linqCache) is IQueryNode node)
                return node;
            var p = span.Materialize();
            if (_targetProps.TryGetValue(p, out var property))
                return new PropertyAccessNode(_targetParamExpr, property);

            throw new InvalidOperationException($"No such function or property '{p}'.");
        }
        private static IQueryNode ParseRhs(string valueString, Type comparingType)
        {
            comparingType.IsEnumerableType(out var eType);

            if (_targetProps.TryGet(valueString) is PropertyInfo compareProp
                && compareProp.PropertyType == eType)
                return new PropertyAccessNode(_targetParamExpr, compareProp);

            var t = eType;
            if ((eType.IsClass || eType.IsNullableStruct(out eType)) && valueString == "null")
                return new ConstantNode(null, t);

            if (eType.IsEnum)
            {
                return (Enum.TryParse(eType, valueString, true, out var e))
                    ? new ConstantNode(e, eType)
                    : throw new InvalidOperationException($"Enum type '{eType}' does not have a definition for '{valueString}'.");
            }

            return (eType != InfoCache.StrType && InfoCache.IConvType.IsAssignableFrom(eType))
                ? new ConstantNode(Convert.ChangeType(valueString, eType), eType)
                : new ConstantNode(valueString, InfoCache.StrType);
        }
    }
}
