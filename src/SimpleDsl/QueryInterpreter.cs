using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JiiLib.SimpleDsl.Nodes;


namespace JiiLib.SimpleDsl
{
    /// <summary>
    ///     Allows translating a user input string to query over some <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">
    ///     The element type of an <see cref="IEnumerable{T}"/>.
    /// </typeparam>
    public partial class QueryInterpreter<T>
    {
        private static readonly ParameterExpression _targetParamExpr;
        private static readonly ParameterExpression _targetsParamExpr;
        private static readonly IReadOnlyDictionary<string, PropertyInfo> _targetProps;

        static QueryInterpreter()
        {
            var targetType = typeof(T);
            var ienumTargetType = typeof(IEnumerable<T>);

            _targetParamExpr = Expression.Parameter(targetType);
            _targetsParamExpr = Expression.Parameter(ienumTargetType);
            _targetProps = targetType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Where(p => p.CanRead)
                .ToImmutableDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        }

        private readonly ConcurrentDictionary<Type, INestedInterpreter> _nested = new ConcurrentDictionary<Type, INestedInterpreter>();
        private readonly QueryModel _model = new QueryModel();
        private readonly ITextFormats _formats;
        private readonly ConstantExpression _cfgExpr;

        /// <summary>
        ///     Creates a new interpreter.
        /// </summary>
        /// <param name="formats">
        ///     
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="formats"/> was <see langword="null"/>.
        /// </exception>
        public QueryInterpreter(ITextFormats formats)
        {
            _formats = formats ?? throw new ArgumentNullException(nameof(formats));
            _cfgExpr = Expression.Constant(_formats);
        }

        /// <summary>
        ///     Parses the user input for querying against an in-memory collection.
        /// </summary>
        /// <param name="query">
        ///     The input query.
        /// </param>
        /// <returns>
        ///     A <see cref="EnumerableQueryParseResult{T}"/> that contains the desired queries.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="query"/> was <see langword="null"/> or empty.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Some part of the query was not valid.
        /// </exception>
        public EnumerableQueryParseResult<T> ParseForEnumerable(string query)
        {
            VerifyPreconditions(query);

            var values = ParseFull(query, InfoCache.Enumerable);

            return values.ToEnumerableResult();
        }

        /// <summary>
        ///     Parses the user input for querying against an <see cref="IQueryable{T}"/> collection.
        /// </summary>
        /// <param name="query">
        ///     The input query.
        /// </param>
        /// <returns>
        ///     A <see cref="QueryableQueryParseResult{T}"/> that contains the desired queries.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="query"/> was <see langword="null"/> or empty.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Some part of the query was not valid.
        /// </exception>
        public QueryableQueryParseResult<T> ParseForQueryable(string query)
        {
            VerifyPreconditions(query);

            var values = ParseFull(query, InfoCache.Queryable);

            return values.ToQueryResult();
        }

        private static void VerifyPreconditions(string query)
        {
            if (String.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException(nameof(query));

            if (!InfoCache.Clauses.Any(s => query.Contains(s, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("At least one clause must be specified.");
        }

        private ValueBag ParseFull(string query, ILinqCache linqCache)
        {
            Expression<Func<T, bool>> predicate = null;
            var orders = ImmutableArray.CreateBuilder<OrderByExpression<T>>();
            int skipAmount = 0;
            int takeAmount = 10;
            Func<T, string> selector = null;

            var span = query.ToLowerInvariant().AsSpan().Trim();

            var opWord = span.SliceUntilFirstUnnested(' ', out var remainder);
            if (opWord.SequenceEqual(InfoCache.Where.AsSpan()))
            {
                var matchIdx = remainder.VerifyOpenChar('[', InfoCache.Where).FindMatchingBrace();
                var whereClause = remainder.Slice(1, matchIdx - 1);
                remainder = remainder.Slice(matchIdx + 1).Trim();

                predicate = ParseWhereClause(whereClause, linqCache);

                opWord = remainder.SliceUntilFirstUnnested(' ', out remainder);
            }

            if (opWord.SequenceEqual(InfoCache.OrderBy.AsSpan()))
            {
                var matchIdx = remainder.VerifyOpenChar('[', InfoCache.OrderBy).FindMatchingBrace();
                var clauses = remainder.Slice(1, matchIdx - 1);
                remainder = remainder.Slice(matchIdx + 1).Trim();
                orders.AddRange(ParseOrderByClauses(clauses, linqCache));

                opWord = remainder.SliceUntilFirstUnnested(' ', out remainder);
            }

            if (opWord.SequenceEqual(InfoCache.Skip.AsSpan()))
            {
                var skipStr = remainder.SliceUntilFirstUnnested(' ', out remainder).Materialize();
                if (!Int32.TryParse(skipStr, out var s))
                    throw new InvalidOperationException();

                skipAmount = s;

                opWord = remainder.SliceUntilFirstUnnested(' ', out remainder);
            }

            if (opWord.SequenceEqual(InfoCache.Take.AsSpan()))
            {
                var takeStr = remainder.SliceUntilFirstUnnested(' ', out remainder).Materialize();
                if (!Int32.TryParse(takeStr, out var t))
                    throw new InvalidOperationException();

                takeAmount = t;

                opWord = remainder.SliceUntilFirstUnnested(' ', out remainder);
            }

            if (opWord.SequenceEqual(InfoCache.Select.AsSpan()))
            {
                var matchIdx = remainder.VerifyOpenChar('[', InfoCache.Select).FindMatchingBrace();
                var selectClause = remainder.Slice(1, matchIdx - 1);
                remainder = remainder.Slice(matchIdx + 1).Trim();
                selector = ParseSelectClause(selectClause, linqCache);

                if (!remainder.SequenceEqual(ReadOnlySpan<char>.Empty))
                    throw new InvalidOperationException("Select clause must be the last part of a query.");
            }

            return new ValueBag(_model.InlineVars.ToImmutableDictionary(), predicate, orders.ToImmutable(), skipAmount, takeAmount, selector);
        }

        private static PropertyInfo Property(string propName)
            => (_targetProps.TryGetValue(propName, out var property))
                ? property : null;
        private static string ParseVarDecl(ref ReadOnlySpan<char> slice)
        {
            if (slice.ContainsUnnested('|', out var idspan))
            {
                slice = slice.Slice(idspan.Length + 1);
                var id = idspan.Materialize();
                if (_targetProps.ContainsKey(id))
                    throw new InvalidOperationException($"Cannot use '{id}' as an inline variable identifier as it is already the name of a property.");

                return id;
            }
            return null;
        }
    }
}
