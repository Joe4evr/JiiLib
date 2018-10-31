using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JiiLib.SimpleDsl.Nodes;

namespace JiiLib.SimpleDsl
{
    public partial class QueryInterpreter<T>
    {
        private Func<T, string> ParseSelectClause(
            ReadOnlySpan<char> selectSpan,
            ILinqCache linqCache)
        {
            var exprs = new List<Expression>();

            for (var slice = selectSpan.SliceUntilFirstUnnested(',', out var next); slice.Length > 0; slice = next.SliceUntilFirstUnnested(',', out next))
            {
                var fmt = DslHelpers.ParseFormatModifiers(ref slice);
                var (open, close) = _formats.CreateFormatExpressions(fmt);
                var (expr, name) = ParseFunctionVariableOrInvocation(slice, linqCache);
                Expression formatted;

                if (expr.Type == InfoCache.IEnumStringType)
                {
                    if (fmt != FormatModifiers.None)
                    {
                        //-> String.Concat(close, ", ", open);
                        var closeOpen = Expression.Call(InfoCache.StrConcat3, close, InfoCache.CommaExpr, open);

                        //-> String.Join(closeOpen, expr);
                        var temp = Expression.Call(InfoCache.StrJoin, closeOpen, expr);

                        //-> String.Concat(open, temp, close);
                        formatted = Expression.Call(InfoCache.StrConcat3, open, temp, close);
                    }
                    else
                        //-> String.Join(", ", expr);
                        formatted = Expression.Call(InfoCache.StrJoin, InfoCache.CommaExpr, expr);
                }
                else
                    formatted = (fmt != FormatModifiers.None)
                        //-> String.Concat(open, expr, close);
                        ? Expression.Call(InfoCache.StrConcat3, open, expr, close)
                        : expr;

                var prefix = (String.IsNullOrWhiteSpace(name))
                    ? InfoCache.EmptyStrExpr
                    : Expression.Constant(name + InfoCache.Colon);

                exprs.Add(
                    //-> String.Concat(prefix, formatted);
                    Expression.Call(InfoCache.StrConcat2, prefix, formatted.Stringify()));
            }

            var fmtExpr = Expression.NewArrayInit(InfoCache.StrType, exprs);
            //_model.InlineVars.Values.Select(n => n.Value)
            var fmtBlock = Expression.Block(new[]
            {
                //-> String.Join(", ", fmtExpr);
                Expression.Call(InfoCache.StrJoin, InfoCache.CommaExpr, fmtExpr)
            });

            return Expression.Lambda<Func<T, string>>(fmtBlock, _targetParamExpr).Compile();
        }

        private (Expression, string) ParseFunctionVariableOrInvocation(
            ReadOnlySpan<char> slice,
            ILinqCache linqCache)
        {
            if (ParseKnownFunction(slice, linqCache) is IQueryNode node)
                return (node.Value, "");

            var p = slice.Materialize();
            if (_model.InlineVars.TryGetValue(p, out var n))
                return (n.Value, p);

            if (Property(p) is PropertyInfo property)
                return (_targetParamExpr.PropertyAccess(property), property.Name);

            throw new InvalidOperationException($"No such function, property, or declared variable '{p}'.");
        }
    }
}
