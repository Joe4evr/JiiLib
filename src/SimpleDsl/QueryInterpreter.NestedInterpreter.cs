using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl
{
    public sealed partial class QueryInterpreter<T>
    {
        private delegate (Expression, Type) ExprType(ReadOnlySpan<char> span);

        private interface INestedInterpreter
        {
            Expression ParseNestedWhere(ReadOnlySpan<char> span, MemberExpression parentPropExpr, Dictionary<string, Expression> vars);
            //Expression ParseNestedOrderBy(ReadOnlySpan<char> span, Expression parentPropExpr, Dictionary<string, Expression> vars);
            //Expression ParseNestedSelect(ReadOnlySpan<char> span, Expression parentPropExpr, IReadOnlyDictionary<string, Expression> vars);
        }

        private sealed class NestedInterpreter<TInner> : INestedInterpreter
        {
            private static readonly MethodInfo _linqWhere;
            private static readonly MethodInfo _linqSelect;
            private static readonly MethodInfo _linqAny;
            private static readonly ParameterExpression _nestedParamExpr;
            private static readonly IReadOnlyDictionary<string, PropertyInfo> _props;

            static NestedInterpreter()
            {
                var type = typeof(TInner);
                var typeArr = new Type[] { type };
                var typeStrArr = new Type[] { type, InfoCache.StrType };

                _linqWhere = InfoCache.LinqWhere.MakeGenericMethod(typeArr);
                _linqSelect = InfoCache.LinqSelect.MakeGenericMethod(typeStrArr);
                _linqAny = InfoCache.LinqAny.MakeGenericMethod(typeArr);
                _linqSelect = InfoCache.LinqSelect.MakeGenericMethod(typeStrArr);
                _linqAny = InfoCache.LinqAny.MakeGenericMethod(typeArr);
                _props = type.GetProperties().ToImmutableDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
                _nestedParamExpr = Expression.Parameter(type, "prop");
            }

            Expression INestedInterpreter.ParseNestedWhere(
                ReadOnlySpan<char> span,
                MemberExpression parentPropExpr,
                Dictionary<string, Expression> vars)
            {
                var resExpr = Expression.Variable(InfoCache.BoolType, "nestResult");
                var filterBlockExpr = Expression.Block(InfoCache.BoolType,
                    new[] { resExpr },
                    Expression.Assign(resExpr, Expression.Constant(true)));

                var nestedVars = new Dictionary<string, Expression>(StringComparer.OrdinalIgnoreCase);

                for (var slice = span.SliceUntilFirstUnnested(',', out var next); slice.Length > 0; slice = next.SliceUntilFirstUnnested(',', out next))
                    filterBlockExpr = ParseOperation(slice, filterBlockExpr, resExpr, GetExprAndType, GetRhsExpression, InfoCache.AndAlso, nestedVars);

                var lambda = Expression.Lambda<Func<TInner, bool>>(filterBlockExpr, _nestedParamExpr);
                if (nestedVars.Count > 0)
                {
                    //-> parentPropExpr.Where(lambda);
                    var filtered = Expression.Call(_linqWhere, parentPropExpr, lambda);
                    foreach (var (id, expr) in nestedVars)
                    {
                        //-> filtered.Select(expr);
                        var selected = Expression.Call(
                            _linqSelect,
                            filtered,
                            Expression.Lambda<Func<TInner, string>>(
                                expr.Stringify(),
                                _nestedParamExpr));

                        vars.AddInlineVar(id, selected);
                    }
                }

                //-> parentPropExpr.Any(lambda);
                return Expression.Call(_linqAny, parentPropExpr, lambda);
            }

            //Expression INestedInterpreter.ParseNestedOrderBy(
            //    ReadOnlySpan<char> span,
            //    Expression parentPropExpr,
            //    Dictionary<string, Expression> vars)
            //{

            //}

            //Expression INestedInterpreter.ParseNestedSelect(
            //    ReadOnlySpan<char> span,
            //    Expression parentPropExpr,
            //    IReadOnlyDictionary<string, Expression> vars)
            //{

            //}

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

            private static (Expression, Type) GetExprAndType(ReadOnlySpan<char> span)
            {
                if (ParseKnownFunction(span) is Expression expr)
                    return (expr, expr.Type);
                else
                {
                    var p = span.Materialize();
                    if (!(Property(p) is PropertyInfo property))
                        throw new InvalidOperationException($"No such function or property '{p}'.");

                    return (PropertyAccessExpression(property), property.PropertyType);
                }
            }
        }
    }
}
