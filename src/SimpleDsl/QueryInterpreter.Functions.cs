using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using JiiLib.SimpleDsl.Nodes;

namespace JiiLib.SimpleDsl
{
    public partial class QueryInterpreter<T>
    {
        private static IQueryNode ParseKnownFunction(ReadOnlySpan<char> span, ILinqCache linqCache)
        {
            if (span.StartsWith(InfoCache.Count.AsSpan()))
            {
                var arg = span.Slice(InfoCache.Count.Length).VerifyOpenChar('(', InfoCache.Count).TrimBraces().Materialize();
                if (_targetProps.TryGetValue(arg, out var property))
                    return new CountFunctionNode(linqCache.LinqCountOpen, new PropertyAccessNode(_targetParamExpr, property));
                else
                    throw new InvalidOperationException();
            }

            var args = ReadOnlySpan<char>.Empty;
            (MethodInfo method, string name) = default((MethodInfo, string));
            if (span.StartsWith(InfoCache.Sum.AsSpan()))
            {
                args = span.Slice(InfoCache.Sum.Length).VerifyOpenChar('(', InfoCache.Sum).TrimBraces();
                method = linqCache.LinqSum;
                name = InfoCache.Sum;
            }

            if (span.StartsWith(InfoCache.Min.AsSpan()))
            {
                args = span.Slice(InfoCache.Min.Length).VerifyOpenChar('(', InfoCache.Min).TrimBraces();
                method = linqCache.LinqMin;
                name = InfoCache.Min;
            }

            if (span.StartsWith(InfoCache.Max.AsSpan()))
            {
                args = span.Slice(InfoCache.Max.Length).VerifyOpenChar('(', InfoCache.Max).TrimBraces();
                method = linqCache.LinqMax;
                name = InfoCache.Max;
            }

            if (span.StartsWith(InfoCache.Average.AsSpan()))
            {
                args = span.Slice(InfoCache.Average.Length).VerifyOpenChar('(', InfoCache.Average).TrimBraces();
                method = linqCache.LinqAverage;
                name = InfoCache.Average;
            }

            if (method != null)
            {
                var nodes = new List<IQueryNode>();
                var splitter = args.CreateSplitter(',');
                while (splitter.TryMoveNext(out var item))
                {
                    var p = item.Materialize();
                    if (_targetProps.TryGet(p) is PropertyInfo property)
                    {
                        if (property.PropertyType != InfoCache.IntType)
                            throw new InvalidOperationException($"Property '{p}' must be a numeric type to be used in '{name}()'.");

                        nodes.Add(new PropertyAccessNode(_targetParamExpr, property));
                    }
                    else if (Int32.TryParse(p, out var i))
                        nodes.Add(new ConstantNode(i, InfoCache.IntType));
                    else
                        throw new InvalidOperationException($"'{p}' must be a number or a numeric property to be used in '{name}()'.");
                }
                return new NumericFunctionNode(method, nodes);
            }

            return null;
        }
    }
}
