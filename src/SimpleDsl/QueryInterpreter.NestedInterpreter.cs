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
        LambdaExpression INestedInterpreter.ParseNestedWhere(ReadOnlySpan<char> span, ILinqCache linqCache)
            => ParseWhereClause(span, linqCache);

        private IQueryNode BuildNestedQuery(PropertyAccessNode property, ReadOnlySpan<char> querySpan, ILinqCache linqCache)
        {
            var isCollection = property.Property.PropertyType.IsEnumerableType(out var eType);
            var nestedReader = (INestedInterpreter)Activator.CreateInstance(
                typeof(QueryInterpreter<>).MakeGenericType(eType),
                _formats);

            return (isCollection)
                ? BuildCollectionNestedQuery(property, querySpan.TrimBraces(), nestedReader, linqCache)
                : new ScalarLambdaNode(nestedReader.ParseNestedWhere(querySpan.TrimBraces(), linqCache), property);
        }

        private static IQueryNode BuildCollectionNestedQuery(
            PropertyAccessNode property,
            ReadOnlySpan<char> query,
            INestedInterpreter reader,
            ILinqCache linqCache)
        {
            var lambda = reader.ParseNestedWhere(query, linqCache);

            return new BasicNode(linqCache.Any(property.Value, lambda));
        }
    }
}
