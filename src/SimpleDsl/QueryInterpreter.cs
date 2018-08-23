using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl
{
    /// <summary>
    ///     Allows translating a user input string to query over some <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">
    ///     The element type of an <see cref="IEnumerable{T}"/>.
    /// </typeparam>
    public sealed partial class QueryInterpreter<T>
    {
        private static readonly Type _targetType = typeof(T);
        private static readonly Type[] _targetAndIntTypeArray = new[] { _targetType, InfoCache.IntType };

        private static readonly ParameterExpression _targetParamExpr = Expression.Parameter(_targetType, "target");
        private static readonly ParameterExpression _targetsParamExpr;

        private static readonly IReadOnlyDictionary<string, PropertyInfo> _targetProps = _targetType.GetProperties().ToImmutableDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        static QueryInterpreter()
        {
            var ienumTargetType = typeof(IEnumerable<T>);

            _targetsParamExpr = Expression.Parameter(ienumTargetType, "targets");
        }

        private static readonly ConcurrentDictionary<Type, INestedInterpreter> _nested = new ConcurrentDictionary<Type, INestedInterpreter>();
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
        ///     Parses the user input.
        /// </summary>
        /// <param name="query">
        ///     The input query.
        /// </param>
        /// <returns>
        ///     A <see cref="QueryParseResult{T}"/> that contains the desired queries.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="query"/> was <see langword="null"/> or empty.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Some part of the query was not valid.
        /// </exception>
        public QueryParseResult<T> ParseFull(string query)
        {
            if (String.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException(nameof(query));

            if (!InfoCache.Clauses.Any(s => query.Contains(s, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("At least one clause must be specified.");

            Func<T, bool> predicate = null;
            Func<IEnumerable<T>, IOrderedEnumerable<T>> order = null;
            int skipAmount = 0;
            int takeAmount = 10;
            Func<T, string> selector = null;

            var span = query.ToLowerInvariant().AsSpan().Trim();
            var inlineVars = new Dictionary<string, Expression>(StringComparer.OrdinalIgnoreCase);

            var opWord = span.SliceUntilFirstUnnested(' ', out var remainder);
            if (opWord.SequenceEqual(InfoCache.Where.AsSpan()))
            {
                var matchIdx = remainder.VerifyOpenChar('[', InfoCache.Where).FindMatchingBrace();
                var whereClause = remainder.Slice(1, matchIdx - 1);
                remainder = remainder.Slice(matchIdx + 1).Trim();
                predicate = ParseWhereClause(whereClause, inlineVars);

                opWord = remainder.SliceUntilFirstUnnested(' ', out remainder);
            }

            if (opWord.SequenceEqual(InfoCache.OrderBy.AsSpan()))
            {
                var matchIdx = remainder.VerifyOpenChar('[', InfoCache.OrderBy).FindMatchingBrace();
                var orderByClause = remainder.Slice(1, matchIdx - 1);
                remainder = remainder.Slice(matchIdx + 1).Trim();
                order = ParseOrderByClause(orderByClause, inlineVars);

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
                selector = ParseSelectClause(selectClause, inlineVars);

                if (!remainder.SequenceEqual(ReadOnlySpan<char>.Empty))
                    throw new InvalidOperationException("Select clause must be the last part of a query.");
            }

            return new QueryParseResult<T>(inlineVars.ToImmutableDictionary(), predicate, order, skipAmount, takeAmount, selector);
        }

        private Func<T, bool> ParseWhereClause(
            ReadOnlySpan<char> whereSpan,
            Dictionary<string, Expression> vars)
        {
            var resExpr = Expression.Variable(InfoCache.BoolType, "result");
            var filterBlockExpr = Expression.Block(InfoCache.BoolType,
                new[] { resExpr },
                Expression.Assign(resExpr, InfoCache.True));

            for (var slice = whereSpan.SliceUntilFirstUnnested(',', out var next); slice.Length > 0; slice = next.SliceUntilFirstUnnested(',', out next))
                filterBlockExpr = ParseOperation(slice, filterBlockExpr, resExpr, GetExprAndType, GetRhsExpression, InfoCache.AndAlso, vars);

            var lambda = Expression.Lambda<Func<T, bool>>(filterBlockExpr, _targetParamExpr);

            return lambda.Compile();

        }

        private Func<IEnumerable<T>, IOrderedEnumerable<T>> ParseOrderByClause(
            ReadOnlySpan<char> orderBySpan,
            Dictionary<string, Expression> vars)
        {
            var exprs = new List<(Expression, bool)>();

            for (var slice = orderBySpan.SliceUntilFirstUnnested(',', out var next); slice.Length > 0; slice = next.SliceUntilFirstUnnested(',', out next))
            {
                var identifier = ParseVarDecl(ref slice);
                bool isDesc = slice.ParseIsDescending();
                var invocation = ParseFunctionOrInvocation(slice);
                vars.AddInlineVar(identifier, invocation);

                exprs.Add((invocation, isDesc));
            }

            MethodCallExpression call = null;

            foreach (var (expr, d) in exprs)
            {
                var selExpr = Expression.Lambda<Func<T, int>>(expr, _targetParamExpr);

                call = (call == null)
                    //-> targets.OrderBy{Descending}(selExpr);
                    ? Expression.Call((d ? InfoCache.LinqOBDOpen : InfoCache.LinqOBOpen).MakeGenericMethod(_targetAndIntTypeArray), _targetsParamExpr, selExpr)
                    //-> call.ThenBy{Descending}(selExpr);
                    : Expression.Call((d ? InfoCache.LinqTBDOpen : InfoCache.LinqTBOpen).MakeGenericMethod(_targetAndIntTypeArray), call, selExpr);
            }

            var lambda = Expression.Lambda<Func<IEnumerable<T>, IOrderedEnumerable<T>>>(call, _targetsParamExpr);
            return lambda.Compile();
        }

        private Func<T, string> ParseSelectClause(
            ReadOnlySpan<char> selectSpan,
            IReadOnlyDictionary<string, Expression> vars)
        {
            var exprs = new List<Expression>();

            for (var slice = selectSpan.SliceUntilFirstUnnested(',', out var next); slice.Length > 0; slice = next.SliceUntilFirstUnnested(',', out next))
            {
                var fmt = ParseFormatModifiers(ref slice);
                var (open, close) = CreateFormatExpressions(fmt);
                var (expr, name) = ParseFunctionVariableOrInvocation(slice, vars);
                MethodCallExpression formatted;

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
                    //-> String.Concat(open, expr, close);
                    formatted = Expression.Call(InfoCache.StrConcat3, open, expr, close);

                var prefix = (String.IsNullOrWhiteSpace(name))
                    ? InfoCache.EmptyStrExpr
                    : Expression.Constant(name + InfoCache.Colon);

                exprs.Add(
                    //-> String.Concat(prefix, formatted);
                    Expression.Call(InfoCache.StrConcat2, prefix, formatted));
            }

            var fmtExpr = Expression.NewArrayInit(InfoCache.StrType, exprs);

            var lambda = Expression.Lambda<Func<T, string>>(
                //-> String.Join(", ", fmtExpr);
                Expression.Call(InfoCache.StrJoin, InfoCache.CommaExpr, fmtExpr),
                _targetParamExpr);

            return lambda.Compile();
        }

        private static BlockExpression ParseOperation(
            ReadOnlySpan<char> span,
            BlockExpression blockExpr,
            Expression resExpr,
            ExprType getLhs,
            Func<string, Type, Expression> getRhs,
            Func<Expression, Expression, Expression> tempAssign,
            Dictionary<string, Expression> vars)
        {
            var lhsSpan = span.SliceUntilFirstUnnested(' ', out var rhsSpan);
            var identifier = ParseVarDecl(ref lhsSpan);
            var negatedBool = (lhsSpan[0] == '!');
            lhsSpan = negatedBool ? lhsSpan.Slice(1).Trim() : lhsSpan;

            BlockExpression interimBlock;
            Expression result;
            bool truthy;
            if (lhsSpan.StartsWith(InfoCache.Or.AsSpan()))
            {
                var tmpRes = Expression.Variable(InfoCache.BoolType);
                var tmpBlk = Expression.Block(
                    new[] { tmpRes },
                    new Expression[] { Expression.Assign(tmpRes, InfoCache.False) });

                var items = lhsSpan.Slice(InfoCache.Or.Length).VerifyOpenChar('(', InfoCache.Or).TrimBraces();
                for (var item = items.SliceUntilFirstUnnested(',', out var next); item.Length > 0; item = next.SliceUntilFirstUnnested(',', out next))
                    tmpBlk = ParseOperation(item, tmpBlk, tmpRes, getLhs, getRhs, InfoCache.OrElse, vars);

                (interimBlock, result, truthy) = (tmpBlk, tmpRes, true);
            }
            else
            {
                var (invocation, pType) = getLhs(lhsSpan);
                var isCollection = pType.IsCollectionType(out var eType);

                if (pType == InfoCache.BoolType) //bool access
                    (interimBlock, result, truthy) = (InfoCache.EmptyBlock, invocation, !negatedBool);
                else if (invocation is MemberExpression memberExpr && rhsSpan[0] == '{') //nested op
                {
                    if (!isCollection)
                        throw new InvalidOperationException($"Property '{memberExpr.Member.Name}' must be a collection type to allow nested queries.");

                    var reader = _nested.GetOrAdd(eType, (k) => (INestedInterpreter)Activator.CreateInstance(typeof(NestedInterpreter<>).MakeGenericType(_targetType, k)));
                    var trimmed = rhsSpan.TrimBraces();

                    (interimBlock, result, truthy) = (InfoCache.EmptyBlock, reader.ParseNestedWhere(trimmed, memberExpr, vars), true);
                }
                else //simple op
                {
                    vars.AddInlineVar(identifier, invocation);
                    var opSpan = rhsSpan.SliceUntilFirstUnnested(' ', out var rem);

                    ((interimBlock, result), truthy) = CreateOperatorExpression(invocation.Type, invocation, ParseOperator(opSpan), getRhs(rem.TrimBraces().Materialize(), eType));
                }
            }

            var resAssign = (truthy) ? Expression.IsTrue(result) : Expression.IsFalse(result);
            return blockExpr.Update(
                    blockExpr.Variables.Concat(interimBlock.Variables),
                    blockExpr.Expressions.Concat(
                        interimBlock.Expressions.Append(
                            Expression.Assign(resExpr, tempAssign(resExpr, resAssign)))));
        }

        private static string ParseVarDecl(ref ReadOnlySpan<char> slice)
        {
            if (slice.IndexOf('|') >= 0)
            {
                var id = slice.SliceUntilFirstUnnested('|', out slice).Materialize();
                if (_targetProps.ContainsKey(id))
                    throw new InvalidOperationException($"Cannot use '{id}' as an inline variable identifier as it is already the name of a property.");

                return id;
            }
            return null;
        }
        private static FormatModifiers ParseFormatModifiers(ref ReadOnlySpan<char> slice)
        {
            var fmt = FormatModifiers.None;
            if (slice.IndexOf(':') >= 0)
            {
                var prefix = slice.SliceUntilFirstUnnested(':', out slice);

                for (int i = 0; i < prefix.Length; i++)
                {
                    var cur = prefix[i];
                    switch (cur)
                    {
                        case 'b':
                            fmt = fmt |= FormatModifiers.Bold;
                            break;
                        case 'i':
                            fmt = fmt |= FormatModifiers.Italic;
                            break;
                        case 'u':
                            fmt = fmt |= FormatModifiers.Underline;
                            break;
                        default:
                            throw new InvalidOperationException($"Unknown format modifier '{cur}'.");
                    }
                }
            }
            return fmt;
        }
        private static Expression ParseFunctionOrInvocation(ReadOnlySpan<char> slice)
        {
            if (ParseKnownFunction(slice) is Expression knownFunc)
                return knownFunc;

            var p = slice.Materialize();
            if (Property(p) is PropertyInfo property)
                return PropertyAccessExpression(property);

            throw new InvalidOperationException($"No such function or property '{p}'.");
        }
        private static (Expression, string) ParseFunctionVariableOrInvocation(
            ReadOnlySpan<char> slice,
            IReadOnlyDictionary<string, Expression> vars)
        {
            if (ParseKnownFunction(slice) is Expression knownFunc)
                return (knownFunc, "");

            var p = slice.Materialize();
            if (vars.TryGetValue(p, out var expr))
                return (expr, p);

            if (Property(p) is PropertyInfo property)
                return (PropertyAccessExpression(property), property.Name);

            throw new InvalidOperationException($"No such function, property, or declared variable '{p}'.");
        }

        private static Expression GetRhsExpression(string valueString, Type comparingType)
        {
            if (Property(valueString) is PropertyInfo compareProp
                && compareProp.PropertyType == comparingType)
                return PropertyAccessExpression(compareProp);

            if (comparingType.IsEnum)
            {
                return (Enum.TryParse(comparingType, valueString, true, out var e))
                    ? Expression.Constant(e, comparingType)
                    : throw new InvalidOperationException($"Enum type '{comparingType}' does not have a definition for '{valueString}'.");
            }

            return (InfoCache.IConvType.IsAssignableFrom(comparingType))
                ? Expression.Constant(Convert.ChangeType(valueString, comparingType), comparingType)
                : Expression.Constant(valueString, InfoCache.StrType);
        }
        private static Expression GenerateEqualityExpression(
            bool isTrue,
            Expression target,
            MethodInfo method,
            params Expression[] args)
        {
            var call = (method.IsStatic)
                ? Expression.Call(method, args)
                : Expression.Call(target, method, args);

            return (isTrue) ? Expression.IsTrue(call) : Expression.IsFalse(call);
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

        private static Expression ParseKnownFunction(ReadOnlySpan<char> slice)
        {
            if (slice.StartsWith(InfoCache.Sum.AsSpan()))
                return CreateSumExpression(slice.Slice(InfoCache.Sum.Length).VerifyOpenChar('(', InfoCache.Sum).TrimBraces());

            if (slice.StartsWith(InfoCache.Min.AsSpan()))
                return CreateMinExpression(slice.Slice(InfoCache.Min.Length).VerifyOpenChar('(', InfoCache.Min).TrimBraces());

            if (slice.StartsWith(InfoCache.Max.AsSpan()))
                return CreateMaxExpression(slice.Slice(InfoCache.Max.Length).VerifyOpenChar('(', InfoCache.Max).TrimBraces());

            if (slice.StartsWith(InfoCache.Average.AsSpan()))
                return CreateAverageExpression(slice.Slice(InfoCache.Average.Length).VerifyOpenChar('(', InfoCache.Average).TrimBraces());

            if (slice.StartsWith(InfoCache.Count.AsSpan()))
                return CreateCountExpression(slice.Slice(InfoCache.Count.Length).VerifyOpenChar('(', InfoCache.Count).TrimBraces());

            return null;
        }
        private static Expression CreateSumExpression(ReadOnlySpan<char> itemSpan)
            //ints.Sum();
            => Expression.Call(InfoCache.LinqSum, Expression.NewArrayInit(InfoCache.IntType, CreateIntList(itemSpan, InfoCache.Sum)));
        private static Expression CreateMinExpression(ReadOnlySpan<char> itemSpan)
            //ints.Min();
            => Expression.Call(InfoCache.LinqMin, Expression.NewArrayInit(InfoCache.IntType, CreateIntList(itemSpan, InfoCache.Min)));
        private static Expression CreateMaxExpression(ReadOnlySpan<char> itemSpan)
            //ints.Max();
            => Expression.Call(InfoCache.LinqMax, Expression.NewArrayInit(InfoCache.IntType, CreateIntList(itemSpan, InfoCache.Max)));
        private static Expression CreateAverageExpression(ReadOnlySpan<char> itemSpan)
            //ints.Average();
            => Expression.Call(InfoCache.LinqAverage, Expression.NewArrayInit(InfoCache.IntType, CreateIntList(itemSpan, InfoCache.Average)));
        private static Expression CreateCountExpression(ReadOnlySpan<char> itemSpan)
        {
            var p = itemSpan.Materialize();
            if (!(Property(p) is PropertyInfo property))
                throw new InvalidOperationException($"No such property '{p}'.");

            var isCollection = property.PropertyType.IsCollectionType(out var eType);
            if (!isCollection)
                throw new InvalidOperationException($"Property '{p}' must be a collection type to be used in 'count()'.");

            //property.Count();
            return Expression.Call(
                InfoCache.LinqCountOpen.MakeGenericMethod(eType),
                PropertyAccessExpression(property));
        }

        private static PropertyInfo Property(string propName)
            => (_targetProps.TryGetValue(propName, out var property))
                ? property : null;
        private static MemberExpression PropertyAccessExpression(PropertyInfo property)
            => Expression.Property(_targetParamExpr, property);
        private static List<Expression> CreateIntList(ReadOnlySpan<char> itemSpan, string functionName)
        {
            var props = new List<Expression>();
            for (var item = itemSpan.SliceUntilFirstUnnested(',', out var next); item.Length > 0; item = next.SliceUntilFirstUnnested(',', out next))
            {
                var p = item.Materialize();
                if (Property(p) is PropertyInfo property)
                {
                    if (property.PropertyType != InfoCache.IntType)
                        throw new InvalidOperationException($"Property '{p}' must be a number to be used in '{functionName}()'.");

                    props.Add(PropertyAccessExpression(property));
                }
                else if (Int32.TryParse(p, out var i))
                    props.Add(Expression.Constant(i, InfoCache.IntType));
                else
                    throw new InvalidOperationException($"'{p}' must be a number or a numeric property to be used in '{functionName}()'.");
            }

            return props;
        }
    }
}
