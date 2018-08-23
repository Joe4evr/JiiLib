using System;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl
{
    public sealed partial class QueryInterpreter<T>
    {
        [Flags]
        private enum FormatModifiers
        {
            None = 0,
            Bold = 1,
            Italic = 1 << 1,
            Underline = 1 << 2
        }

        private (ConstantExpression open, ConstantExpression close) CreateFormatExpressions(FormatModifiers formats)
        {
            if (formats == FormatModifiers.None)
                return (InfoCache.EmptyStrExpr, InfoCache.EmptyStrExpr);

            var o = String.Empty;
            var c = String.Empty;

            if ((formats & FormatModifiers.Bold) == FormatModifiers.Bold)
            {
                o = _formats.BoldOpen + o;
                c += _formats.BoldClose;
            }

            if ((formats & FormatModifiers.Italic) == FormatModifiers.Italic)
            {
                o = _formats.ItalicOpen + o;
                c += _formats.ItalicClose;
            }

            if ((formats & FormatModifiers.Underline) == FormatModifiers.Underline)
            {
                o = _formats.UnderlineOpen + o;
                c += _formats.UnderlineClose;
            }

            return (Expression.Constant(o), Expression.Constant(c));
        }
    }
}
