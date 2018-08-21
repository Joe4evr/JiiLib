using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl
{
    public sealed partial class QueryInterpreter<T>
    {
        private interface INestedInterpreter
        {
            Expression ParseNestedWhere(ReadOnlySpan<char> span, Expression parentPropExpr, ITextFormatter formatter, Dictionary<string, Expression> vars);
        }
        private sealed class NestedInterpreter<TInner> : INestedInterpreter
        {
            private static readonly Type _type = typeof(TInner);
            private static readonly IReadOnlyDictionary<string, PropertyInfo> _props = _type.GetProperties().ToImmutableDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
            private static readonly ParameterExpression _nestedParamExpr = Expression.Parameter(_type, "prop");
            //private static readonly ParameterExpression _nestedIEnumParamExpr = Expression.Parameter(typeof(IEnumerable<TInner>), "props");

            public Expression ParseNestedWhere(
                ReadOnlySpan<char> span,
                Expression parentPropExpr,
                ITextFormatter formatter,
                Dictionary<string, Expression> vars)
            {
                var resExpr = Expression.Variable(InfoCache.BoolType, "nestResult");
                var filterBlockExpr = Expression.Block(InfoCache.BoolType,
                    new[] { resExpr },
                    Expression.Assign(resExpr, Expression.Constant(true)));

                for (var slice = span.SliceUntilFirst(',', out var next); slice.Length > 0; slice = next.SliceUntilFirst(',', out next))
                {
                    var identifier = ParseVarDecl(ref slice);
                    var lhsSpan = slice.SliceUntilFirst(' ', out var rhs);

                    var p = lhsSpan.Materialize();
                    if (!(Property(p) is PropertyInfo property))
                        throw new InvalidOperationException($"No such property '{p}'.");

                    var invocation = PropertyAccessExpression(property);
                    AddInlineVar(vars, identifier, invocation);

                    var opSpan = rhs.SliceUntilFirst(' ', out var rem);
                    property.PropertyType.IsCollectionType(out var eType);

                    filterBlockExpr = AddOp(filterBlockExpr, resExpr, formatter, invocation, ParseOperator(opSpan), GetRhsExpression(rem.TrimBraces().Materialize(), eType));
                }

                var lambda = Expression.Lambda<Func<TInner, bool>>(filterBlockExpr, _nestedParamExpr);

                return Expression.Call(InfoCache.LinqAny.MakeGenericMethod(_type), parentPropExpr, lambda);
            }

            private static PropertyInfo Property(string propName)
                => (_props.TryGetValue(propName, out var property))
                    ? property : null;
            private static MemberExpression PropertyAccessExpression(PropertyInfo property)
                => Expression.Property(_nestedParamExpr, property);
            private static Expression GetRhsExpression(string valueString, Type comparingType)
            {
                if (Property(valueString) is PropertyInfo compareProp
                    && compareProp.PropertyType == comparingType)
                    return PropertyAccessExpression(compareProp);

                return (InfoCache.IConvType.IsAssignableFrom(comparingType))
                    ? Expression.Constant(Convert.ChangeType(valueString, comparingType), comparingType)
                    : Expression.Constant(valueString, InfoCache.StrType);
            }
        }
    }
}
