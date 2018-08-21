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
        private static readonly Type _ienumTargetType = typeof(IEnumerable<T>);
        private static readonly Type _iOrdEnumTargetType = typeof(IOrderedEnumerable<T>);
        private static readonly Type _funcTargetToIntType = typeof(Func<T, int>);
        private static readonly Type[] _targetAndIntTypeArray = new[] { _targetType, InfoCache.IntType };

        private static readonly IReadOnlyDictionary<string, PropertyInfo> _targetProps = _targetType.GetProperties().ToImmutableDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        private static readonly MethodInfo _cfgStringFmt = typeof(ITextFormatter).GetMethod(nameof(ITextFormatter.FormatString), new Type[] { InfoCache.StrType, typeof(FormatModifiers) });

        private static readonly ParameterExpression _targetParamExpr = Expression.Parameter(_targetType, "target");
        private static readonly ParameterExpression _targetsParamExpr = Expression.Parameter(_ienumTargetType, "targets");

        private static Operator ParseOperator(ReadOnlySpan<char> opSpan)
        {
            if (opSpan.SequenceEqual(InfoCache.Contains.AsSpan()))
                return Operator.Contains;
            if (opSpan.SequenceEqual(InfoCache.NotContains.AsSpan()))
                return Operator.NotContains;
            else if (opSpan.SequenceEqual(InfoCache.LessThan.AsSpan()))
                return Operator.LessThan;
            else if (opSpan.SequenceEqual(InfoCache.LessThanOrEqual.AsSpan()))
                return Operator.LessThanOrEqual;
            else if (opSpan.SequenceEqual(InfoCache.GreaterThan.AsSpan()))
                return Operator.GreaterThan;
            else if (opSpan.SequenceEqual(InfoCache.GreaterThanOrEqual.AsSpan()))
                return Operator.GreaterThanOrEqual;
            else if (opSpan.SequenceEqual(InfoCache.IsEqual.AsSpan()))
                return Operator.IsEqual;
            else if (opSpan.SequenceEqual(InfoCache.IsNotEqual.AsSpan()))
                return Operator.NotEqual;
            else
                throw new InvalidOperationException("Unrecognized operator");
        }

        private enum Operator
        {
            Contains,
            NotContains,
            LessThan,
            LessThanOrEqual,
            GreaterThan,
            GreaterThanOrEqual,
            IsEqual,
            NotEqual
        }

        private readonly ConcurrentDictionary<Type, INestedInterpreter> _nested = new ConcurrentDictionary<Type, INestedInterpreter>();
        private readonly ITextFormatter _formatter;
        private readonly ConstantExpression _cfgExpr;

        /// <summary>
        ///     Creates a new interpreter.
        /// </summary>
        /// <param name="formatter">
        ///     A config that contains necessary lookup logic.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="formatter"/> was <see langword="null"/>.
        /// </exception>
        public QueryInterpreter(ITextFormatter formatter)
        {
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _cfgExpr = Expression.Constant(_formatter);
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

            var opWord = span.SliceUntilFirst(' ', out var remainder);
            if (opWord.SequenceEqual(InfoCache.Where.AsSpan()))
            {
                var matchIdx = remainder.VerifyOpenChar('[', InfoCache.Where).FindMatchingBrace();
                var whereClause = remainder.Slice(1, matchIdx - 1);
                remainder = remainder.Slice(matchIdx + 1).Trim();
                predicate = ParseFilters(whereClause, inlineVars);

                opWord = remainder.SliceUntilFirst(' ', out remainder);
            }

            if (opWord.SequenceEqual(InfoCache.OrderBy.AsSpan()))
            {
                var matchIdx = remainder.VerifyOpenChar('[', InfoCache.OrderBy).FindMatchingBrace();
                var orderByClause = remainder.Slice(1, matchIdx - 1);
                remainder = remainder.Slice(matchIdx + 1).Trim();
                order = ParseOrderByClause(orderByClause, inlineVars);

                opWord = remainder.SliceUntilFirst(' ', out remainder);
            }

            if (opWord.SequenceEqual(InfoCache.Skip.AsSpan()))
            {
                var skipStr = remainder.SliceUntilFirst(' ', out remainder).Materialize();
                if (!Int32.TryParse(skipStr, out var s))
                    throw new InvalidOperationException();

                skipAmount = s;

                opWord = remainder.SliceUntilFirst(' ', out remainder);
            }

            if (opWord.SequenceEqual(InfoCache.Take.AsSpan()))
            {
                var takeStr = remainder.SliceUntilFirst(' ', out remainder).Materialize();
                if (!Int32.TryParse(takeStr, out var t))
                    throw new InvalidOperationException();

                takeAmount = t;

                opWord = remainder.SliceUntilFirst(' ', out remainder);
            }

            if (opWord.SequenceEqual(InfoCache.Select.AsSpan()))
            {
                var matchIdx = remainder.VerifyOpenChar('[', InfoCache.Select).FindMatchingBrace();
                var selectClause = remainder.Slice(1, matchIdx - 1);
                remainder = remainder.Slice(matchIdx + 1).Trim();
                selector = ParseSelectClause(selectClause, inlineVars);
            }

            return new QueryParseResult<T>(inlineVars.ToImmutableDictionary(), predicate, order, skipAmount, takeAmount, selector);
        }

        private Func<T, bool> ParseFilters(
            ReadOnlySpan<char> filterSpan,
            Dictionary<string, Expression> vars)
        {
            var resExpr = Expression.Variable(InfoCache.BoolType, "result");
            var filterBlockExpr = Expression.Block(InfoCache.BoolType,
                new[] { resExpr },
                Expression.Assign(resExpr, Expression.Constant(true)));

            for (var slice = filterSpan.SliceUntilFirst(',', out var next); slice.Length > 0; slice = next.SliceUntilFirst(',', out next))
            {
                var identifier = ParseVarDecl(ref slice);
                var lhsSpan = slice.SliceUntilFirst(' ', out var rhs);

                if (rhs[0] == '{') //nested op
                {
                    var (p, pType) = GetExprAndType(lhsSpan);
                    var isCollection = pType.IsCollectionType(out var eType);

                    if (!isCollection)
                        throw new InvalidOperationException($"Property '{((MemberExpression)p).Member.Name}' must be a collection type to allow nested queries.");

                    var reader = _nested.GetOrAdd(eType, (k) => (INestedInterpreter)Activator.CreateInstance(typeof(NestedInterpreter<>).MakeGenericType(_targetType, k)));
                    var trimmed = rhs.TrimBraces();
                    var expression = reader.ParseNestedWhere(trimmed, p, _formatter, vars);
                    filterBlockExpr = filterBlockExpr.Update(
                        filterBlockExpr.Variables,
                        filterBlockExpr.Expressions.Concat(
                            new Expression[]
                            {
                                Expression.Assign(
                                    resExpr,
                                    Expression.AndAlso(
                                        resExpr,
                                        Expression.IsTrue(expression)))
                            }));
                }
                else //simple op
                {
                    var p = lhsSpan.Materialize();
                    if (!(Property(p) is PropertyInfo property))
                        throw new InvalidOperationException($"No such property '{p}'.");

                    var invocation = PropertyAccessExpression(property);
                    AddInlineVar(vars, identifier, invocation);
                    var opSpan = rhs.SliceUntilFirst(' ', out var rem);

                    property.PropertyType.IsCollectionType(out var eType);

                    filterBlockExpr = AddOp(filterBlockExpr, resExpr, _formatter, invocation, ParseOperator(opSpan), GetRhsExpression(rem.Materialize(), eType));
                }
            }

            var lambda = Expression.Lambda<Func<T, bool>>(filterBlockExpr, _targetParamExpr);

            return lambda.Compile();
            
        }

        private Func<IEnumerable<T>, IOrderedEnumerable<T>> ParseOrderByClause(
            ReadOnlySpan<char> orderBySpan,
            Dictionary<string, Expression> vars)
        {
            var exprs = new List<(Expression, bool)>();

            for (var slice = orderBySpan.SliceUntilFirst(',', out var next); slice.Length > 0; slice = next.SliceUntilFirst(',', out next))
            {
                var identifier = ParseVarDecl(ref slice);
                bool isDesc = ParseIsDescending(ref slice);
                var invocation = ParseFunctionOrInvocation(slice);

                if (identifier != null)
                {
                    if (!vars.TryAdd(identifier, invocation))
                        throw new InvalidOperationException($"Inline variable identifier '{identifier}' is already used.");
                }

                exprs.Add((invocation, isDesc));
            }

            MethodCallExpression call = null;

            foreach (var (expr, d) in exprs)
            {
                var selExpr = Expression.Lambda<Func<T, int>>(expr, _targetParamExpr);

                call = (call == null)
                    //targets.OrderBy{Descending}(selExpr);
                    ? Expression.Call((d ? InfoCache.LinqOBDOpen : InfoCache.LinqOBOpen).MakeGenericMethod(_targetAndIntTypeArray), _targetsParamExpr, selExpr)
                    //call.ThenBy{Descending}(selExpr);
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

            for (var slice = selectSpan.SliceUntilFirst(',', out var next); slice.Length > 0; slice = next.SliceUntilFirst(',', out next))
            {
                var fmt = ParseFormatModifiers(ref slice);
                var (expr, name) = ParseFunctionVariableOrInvocation(slice, vars);

                exprs.Add(Expression.Convert(
                    //String.Concat(name{: }, );
                    Expression.Call(
                        InfoCache.StrConcat,
                        Expression.Constant((name == null) ? name : InfoCache.Colon + name),

                        //_cfgExpr.FormatString(expr{.ToString()}, fmt);
                        Expression.Call(
                            _cfgExpr,
                            _cfgStringFmt,
                            (expr.Type == InfoCache.StrType)
                                ? expr
                                //expr.ToString();
                                : Expression.Call(expr, InfoCache.ObjToString),
                            Expression.Constant(fmt))),
                    InfoCache.ObjType));
            }

            var fmtExpr = Expression.NewArrayInit(InfoCache.ObjType, exprs);

            var lambda = Expression.Lambda<Func<T, string>>(
                //String.Join(", ", fmtExpr);
                Expression.Call(InfoCache.StrJoin, InfoCache.CommaExpr, fmtExpr),
                _targetParamExpr);

            return lambda.Compile();
        }

        private static string ParseVarDecl(ref ReadOnlySpan<char> slice)
        {
            if (slice.IndexOf('|') >= 0)
            {
                var id = slice.SliceUntilFirst('|', out slice).Materialize();
                if (_targetProps.ContainsKey(id))
                    throw new InvalidOperationException($"Cannot use '{id}' as an inline variable identifier as it is already the name of a property.");

                return id;
            }
            return null;
        }
        private static void AddInlineVar(Dictionary<string, Expression> vars, string identifier, Expression invocation)
        {
            if (identifier != null)
            {
                if (!vars.TryAdd(identifier, invocation))
                    throw new InvalidOperationException($"Inline variable identifier '{identifier}' is already used.");
            }
        }
        private static bool ParseIsDescending(ref ReadOnlySpan<char> slice)
        {
            if (slice.IndexOf(':') >= 0)
            {
                var prefix = slice.SliceUntilFirst(':', out slice);
                for (int i = 0; i < prefix.Length; i++)
                {
                    var cur = prefix[i];
                    if (cur == 'd')
                        return true;
                }
            }
            return false;
        }
        private static FormatModifiers ParseFormatModifiers(ref ReadOnlySpan<char> slice)
        {
            var fmt = FormatModifiers.None;
            if (slice.IndexOf(':') >= 0)
            {
                var prefix = slice.SliceUntilFirst(':', out slice);

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
        private static BlockExpression AddOp(
            BlockExpression blockExpr,
            Expression resExpr,
            ITextFormatter config,
            Expression lhsExpr,
            Operator op,
            Expression rhsExpr)
        {
            //var (lhsExpr, pType) = GetExprAndType(lhsSpan);
            var pType = lhsExpr.Type;
            var isCollection = pType.IsCollectionType(out var eType);
            var intermVarExpr = Expression.Variable(pType, $"interm{blockExpr.Variables.Count}");
            //var rhsExpr = GetRhsExpression(rhsSpan.Materialize(), eType);

            if (pType == InfoCache.IntType)
            {
                //lhsExpr.CompareTo(rhsExpr);
                var comp = Expression.Call(lhsExpr, InfoCache.IntCompare, rhsExpr);
                switch (op)
                {
                    case Operator.LessThan:
                        return blockExpr.Update(
                            blockExpr.Variables.Concat(new[] { intermVarExpr }),
                            blockExpr.Expressions.Concat(
                                new Expression[]
                                {
                                        Expression.Assign(intermVarExpr, comp),
                                        Expression.Assign(
                                            resExpr,
                                            Expression.AndAlso(
                                                resExpr,
                                                GenerateEqualityExpression(true, intermVarExpr, InfoCache.IntEquals, InfoCache.IntNegOneExpr)))
                                }));
                    case Operator.LessThanOrEqual:
                        return blockExpr.Update(
                            blockExpr.Variables.Concat(new[] { intermVarExpr }),
                            blockExpr.Expressions.Concat(
                                new Expression[]
                                {
                                        Expression.Assign(intermVarExpr, comp),
                                        Expression.Assign(
                                            resExpr,
                                            Expression.AndAlso(
                                                resExpr,
                                                GenerateEqualityExpression(false, intermVarExpr, InfoCache.IntEquals, InfoCache.IntOneExpr)))
                                }));
                    case Operator.GreaterThan:
                        return blockExpr.Update(
                            blockExpr.Variables.Concat(new[] { intermVarExpr }),
                            blockExpr.Expressions.Concat(
                                new Expression[]
                                {
                                        Expression.Assign(intermVarExpr, comp),
                                        Expression.Assign(
                                            resExpr,
                                            Expression.AndAlso(
                                                resExpr,
                                                GenerateEqualityExpression(true, intermVarExpr, InfoCache.IntEquals, InfoCache.IntOneExpr)))
                                }));
                    case Operator.GreaterThanOrEqual:
                        return blockExpr.Update(
                            blockExpr.Variables.Concat(new[] { intermVarExpr }),
                            blockExpr.Expressions.Concat(
                                new Expression[]
                                {
                                        Expression.Assign(intermVarExpr, comp),
                                        Expression.Assign(
                                            resExpr,
                                            Expression.AndAlso(
                                                resExpr,
                                                GenerateEqualityExpression(false, intermVarExpr, InfoCache.IntEquals, InfoCache.IntNegOneExpr)))
                                }));
                    case Operator.IsEqual:
                        return blockExpr.Update(
                            blockExpr.Variables,
                            blockExpr.Expressions.Concat(
                                new Expression[]
                                {
                                        Expression.Assign(
                                            resExpr,
                                            Expression.AndAlso(
                                                resExpr,
                                                GenerateEqualityExpression(true, lhsExpr, InfoCache.IntEquals, rhsExpr)))
                                }));
                    case Operator.NotEqual:
                        return blockExpr.Update(
                            blockExpr.Variables,
                            blockExpr.Expressions.Concat(
                                new Expression[]
                                {
                                        Expression.Assign(
                                            resExpr,
                                            Expression.AndAlso(
                                                resExpr,
                                                GenerateEqualityExpression(false, lhsExpr, InfoCache.IntEquals, rhsExpr)))
                                }));
                    default:
                        throw new InvalidOperationException($"Operation '{op}' not supported on integers.");
                }
            }
            else if (pType == InfoCache.StrType)
            {
                switch (op)
                {
                    case Operator.Contains:
                        return blockExpr.Update(
                            blockExpr.Variables,
                            blockExpr.Expressions.Concat(
                                new Expression[]
                                {
                                        Expression.Assign(
                                            resExpr,
                                            Expression.AndAlso(
                                                resExpr,
                                                GenerateEqualityExpression(true, lhsExpr, InfoCache.StrContains, rhsExpr, InfoCache.StrCompsExpr)))
                                }));
                    case Operator.NotContains:
                        return blockExpr.Update(
                            blockExpr.Variables,
                            blockExpr.Expressions.Concat(
                                new Expression[]
                                {
                                        Expression.Assign(
                                            resExpr,
                                            Expression.AndAlso(
                                                resExpr,
                                                GenerateEqualityExpression(false, lhsExpr, InfoCache.StrContains, rhsExpr, InfoCache.StrCompsExpr)))
                                }));
                    case Operator.IsEqual:
                        return blockExpr.Update(
                            blockExpr.Variables,
                            blockExpr.Expressions.Concat(
                                new Expression[]
                                {
                                    Expression.Assign(
                                        resExpr,
                                        Expression.AndAlso(
                                            resExpr,
                                            GenerateEqualityExpression(true, lhsExpr, InfoCache.StrEquals, rhsExpr, InfoCache.StrCompsExpr)))
                                }));
                    case Operator.NotEqual:
                        return blockExpr.Update(
                            blockExpr.Variables,
                            blockExpr.Expressions.Concat(
                                new Expression[]
                                {
                                    Expression.Assign(
                                        resExpr,
                                        Expression.AndAlso(
                                            resExpr,
                                            GenerateEqualityExpression(false, lhsExpr, InfoCache.StrEquals, rhsExpr, InfoCache.StrCompsExpr)))
                                }));
                    default:
                        throw new InvalidOperationException($"Operation '{op}' not supported on strings.");
                }
            }
            else if (isCollection)
            {
                var (method, args) = (eType == InfoCache.StrType)
                    ? (InfoCache.IEnumStrContains, new[] { lhsExpr, rhsExpr, InfoCache.StrComprExpr })
                    : (eType == InfoCache.IntType)
                        ? (InfoCache.IEnumIntContains, new[] { lhsExpr, rhsExpr })
                        : throw new InvalidOperationException("Use a nested query for properties that are not a string- or integer collection.");

                switch (op)
                {
                    case Operator.Contains:
                        return blockExpr.Update(
                            blockExpr.Variables,
                            blockExpr.Expressions.Concat(
                                new Expression[]
                                {
                                        Expression.Assign(
                                            resExpr,
                                            Expression.AndAlso(
                                                resExpr,
                                                GenerateEqualityExpression(true, null, method, args: args)))
                                }));
                    case Operator.NotContains:
                        return blockExpr.Update(
                            blockExpr.Variables,
                            blockExpr.Expressions.Concat(
                                new Expression[]
                                {
                                        Expression.Assign(
                                            resExpr,
                                            Expression.AndAlso(
                                                resExpr,
                                                GenerateEqualityExpression(false, null, method, args: args)))
                                }));
                    default:
                        throw new InvalidOperationException($"Operation '{op}' not supported on collections.");
                }
            }
            else
                throw new InvalidOperationException($"Property type '{pType}' not supported.");
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
            for (var item = itemSpan.SliceUntilFirst(',', out var next); item.Length > 0; item = next.SliceUntilFirst(',', out next))
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
