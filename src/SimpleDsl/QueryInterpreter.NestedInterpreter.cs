using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JiiLib.SimpleDsl.Nodes;

namespace JiiLib.SimpleDsl
{
    public partial class QueryInterpreter<T> : INestedInterpreter
    {
        LambdaExpression INestedInterpreter.ParseNestedWhere(
            ReadOnlySpan<char> span, QueryModel model, ILinqCache linqCache)
            => ParseWhereClause(span, model, linqCache);

        private IQueryNode BuildNestedQuery(
            PropertyAccessNode property, ReadOnlySpan<char> querySpan, QueryModel model, ILinqCache linqCache)
        {
            var isCollection = property.Property.PropertyType.IsEnumerableType(out var eType);
            var nestedReader = (INestedInterpreter)Activator.CreateInstance(
                typeof(QueryInterpreter<>).MakeGenericType(eType),
                _formats)!;

            return (isCollection)
                ? BuildCollectionNestedQuery(property, querySpan.TrimBraces(), model, nestedReader, linqCache)
                : new ScalarLambdaNode(nestedReader.ParseNestedWhere(querySpan.TrimBraces(), model, linqCache), property);
        }

        private static IQueryNode BuildCollectionNestedQuery(
            PropertyAccessNode property, ReadOnlySpan<char> query, QueryModel model,
            INestedInterpreter reader, ILinqCache linqCache)
        {
            var lambda = reader.ParseNestedWhere(query, model, linqCache);

            return new BasicNode(linqCache.Any(property.Value, lambda));
        }
    }
}
