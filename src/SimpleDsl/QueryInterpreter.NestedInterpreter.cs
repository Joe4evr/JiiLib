using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl
{
    public sealed partial class QueryInterpreter<T> : INestedInterpreter
    {
        private static readonly ConstantExpression _nullExpr;

        Expression INestedInterpreter.ParseNestedWhere(
            ReadOnlySpan<char> span,
            //MemberExpression memberExpr,
            ParameterExpression itemExpr,
            Expression resExpr,
            Dictionary<string, Expression> vars)
        {
            if (itemExpr.Type.IsCollectionType(out var eType))
            {
                var nestedVars = new Dictionary<string, Expression>(StringComparer.OrdinalIgnoreCase);
                var lambda = ParseWhereClause(span, nestedVars);

                if (nestedVars.Count > 0)
                {
                    //-> parentPropExpr.Where(lambda);
                    var filtered = Expression.Call(_linqWhere, itemExpr, lambda);
                    foreach (var (id, expr) in nestedVars)
                    {
                        //-> filtered.Select(expr);
                        var selected = Expression.Call(
                            _linqSelect,
                            filtered,
                            Expression.Lambda<Func<T, string>>(
                                expr.Stringify(),
                                _targetParamExpr));

                        vars.AddInlineVar(id, selected);
                    }
                }

                //-> parentPropExpr.Any(lambda);
                return Expression.Call(_linqAny, itemExpr, lambda);
            }
            else
            {
                //var itemExpr = Expression.Variable(memberExpr.Type, "nestedItem");
                var blkVars = new List<ParameterExpression>()
                {
                    itemExpr
                };
                var exprs = new List<Expression>()
                {
                    //Expression.Assign(itemExpr, memberExpr),
                    Expression.Assign(
                        resExpr, Expression.AndAlso(
                            resExpr, Expression.IsFalse(IsNull(itemExpr))))
                };

                for (var slice = span.SliceUntilFirstUnnested(',', out var next); slice.Length > 0; slice = next.SliceUntilFirstUnnested(',', out next))
                {
                    exprs.Add(
                        Expression.Assign(
                            resExpr, Expression.AndAlso(
                                resExpr, ParseWhereOperand(slice, resExpr, vars, blkVars))));
                }
                return Expression.Block(InfoCache.BoolType, blkVars, exprs);
            }
        }

        private static Expression IsNull(Expression member)
            => (member.Type.IsValueType)
                ? (Expression)InfoCache.False
                : Expression.Call(InfoCache.ObjRefEquals, member, _nullExpr);
    }
}
