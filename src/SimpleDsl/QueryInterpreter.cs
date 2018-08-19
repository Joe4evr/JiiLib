﻿using System;
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
    public sealed class QueryInterpreter<T>
    {
        private static readonly Type _targetType = typeof(T);
        private static readonly Type _ienumTargetType = typeof(IEnumerable<T>);
        private static readonly Type _iOrdEnumTargetType = typeof(IOrderedEnumerable<T>);
        private static readonly Type _funcTargetToIntType = typeof(Func<T, int>);
        private static readonly Type[] _targetAndIntTypeArray = new[] { _targetType, InfoCache.IntType };

        private static readonly IReadOnlyDictionary<string, PropertyInfo> _targetProps = _targetType.GetProperties().ToImmutableDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        private static readonly MethodInfo _cfgStringFmt = typeof(IInterpreterConfig<T>).GetMethod("FormatString", new Type[] { InfoCache.StrType, typeof(FormatModifiers) });

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

        private readonly IInterpreterConfig<T> _config;
        private readonly ConstantExpression _cfgExpr;

        /// <summary>
        ///     Creates a new interpreter.
        /// </summary>
        /// <param name="config">
        ///     A config that contains necessary lookup logic.
        /// </param>
        public QueryInterpreter(IInterpreterConfig<T> config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _cfgExpr = Expression.Constant(_config);
        }

        /// <summary>
        ///     Parses the user input.
        /// </summary>
        /// <param name="input">
        ///     The input query.
        /// </param>
        /// <returns>
        ///     A <see cref="QueryParseResult{T}"/> that contains the desired queries.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="input"/> was <see langword="null"/> or empty.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Some part of the query was not valid.
        /// </exception>
        public QueryParseResult<T> ParseFull(string input)
        {
            if (String.IsNullOrWhiteSpace(input))
                throw new ArgumentNullException(nameof(input));

            if (!InfoCache.Clauses.Any(s => input.Contains(s)))
                throw new InvalidOperationException("At least one clause should be specified.");

            Func<T, bool> predicate = null;
            Func<IEnumerable<T>, IOrderedEnumerable<T>> order = null;
            int skipAmount = 0;
            int takeAmount = 10;
            Func<T, string> selector = null;

            var span = input.AsSpan().Trim();
            var inlineVars = new Dictionary<string, Expression>(StringComparer.OrdinalIgnoreCase);

            var opWord = span.SliceUntil(' ', out var remainder);
            if (opWord.SequenceEqual(InfoCache.Where.AsSpan()))
            {
                var matchIdx = remainder.FindMatchingBrace();
                var whereClause = remainder.Slice(1, matchIdx - 1);
                remainder = remainder.Slice(matchIdx + 1).Trim();
                predicate = ParseFilters(whereClause);

                opWord = remainder.SliceUntil(' ', out remainder);
            }

            if (opWord.SequenceEqual(InfoCache.OrderBy.AsSpan()))
            {
                var matchIdx = remainder.FindMatchingBrace();
                var orderByClause = remainder.Slice(1, matchIdx - 1);
                remainder = remainder.Slice(matchIdx + 1).Trim();
                order = ParseOrderByClause(orderByClause, inlineVars);

                opWord = remainder.SliceUntil(' ', out remainder);
            }

            if (opWord.SequenceEqual(InfoCache.Skip.AsSpan()))
            {
                var skipStr = remainder.SliceUntil(' ', out remainder).Materialize();
                if (!Int32.TryParse(skipStr, out var s))
                    throw new InvalidOperationException();

                skipAmount = s;

                opWord = remainder.SliceUntil(' ', out remainder);
            }

            if (opWord.SequenceEqual(InfoCache.Take.AsSpan()))
            {
                var takeStr = remainder.SliceUntil(' ', out remainder).Materialize();
                if (!Int32.TryParse(takeStr, out var t))
                    throw new InvalidOperationException();

                takeAmount = t;

                opWord = remainder.SliceUntil(' ', out remainder);
            }

            if (opWord.SequenceEqual(InfoCache.Select.AsSpan()))
            {
                var matchIdx = remainder.FindMatchingBrace();
                var selectClause = remainder.Slice(1, matchIdx - 1);
                remainder = remainder.Slice(matchIdx + 1).Trim();
                selector = ParseSelectClause(selectClause, inlineVars);
            }

            return new QueryParseResult<T>(predicate, order, skipAmount, takeAmount, selector);
        }

        private Func<T, bool> ParseFilters(ReadOnlySpan<char> filterSpan)
        {
            var resExpr = Expression.Variable(InfoCache.BoolType, "result");
            var filterBlockExpr = Expression.Block(InfoCache.BoolType,
                new[] { resExpr },
                Expression.Assign(resExpr, Expression.Constant(true)));

            for (var slice = filterSpan.SliceUntil(',', out var next); slice.Length > 0; slice = next.SliceUntil(',', out next))
            {
                var prop = slice.SliceUntil(' ', out var a);
                var op = a.SliceUntil(' ', out var val);
                filterBlockExpr = AddClause(filterBlockExpr, prop, op, val.TrimBraces());
            }

            var lambda = Expression.Lambda<Func<T, bool>>(
                filterBlockExpr,
                _targetParamExpr);

            return lambda.Compile();

            BlockExpression AddClause(
                BlockExpression blockExpr,
                ReadOnlySpan<char> lhsSpan,
                ReadOnlySpan<char> opSpan,
                ReadOnlySpan<char> rhsSpan)
            {
                var (lhsExpr, pType) = GetExprAndType(lhsSpan);
                var (isCollection, eType) = IsCollectionType(pType);
                var rhsExpr = GetValueExpression(rhsSpan.Materialize());
                var intermVarExpr = Expression.Variable(pType, $"interm{blockExpr.Variables.Count}");
                var op = ParseOperator(opSpan);

                if (pType == InfoCache.IntType)
                {
                    switch (op)
                    {
                        case Operator.LessThan:
                            return blockExpr.Update(
                                blockExpr.Variables.Concat(new[] { intermVarExpr }),
                                blockExpr.Expressions.Concat(
                                    new Expression[]
                                    {
                                        Expression.Assign(
                                            intermVarExpr,
                                            Expression.Call(lhsExpr, InfoCache.IntCompare, rhsExpr)),
                                        Expression.Assign(
                                            resExpr,
                                            Expression.AndAlso(
                                                resExpr,
                                                GenerateExpression(true, intermVarExpr, InfoCache.IntEquals, InfoCache.IntNegOneExpr)))
                                    }));
                        case Operator.LessThanOrEqual:
                            return blockExpr.Update(
                                blockExpr.Variables.Concat(new[] { intermVarExpr }),
                                blockExpr.Expressions.Concat(
                                    new Expression[]
                                    {
                                        Expression.Assign(
                                            intermVarExpr,
                                            Expression.Call(lhsExpr, InfoCache.IntCompare, rhsExpr)),
                                        Expression.Assign(
                                            resExpr,
                                            Expression.AndAlso(
                                                resExpr,
                                                Expression.OrElse(
                                                    GenerateExpression(true, intermVarExpr, InfoCache.IntEquals, InfoCache.IntNegOneExpr),
                                                    GenerateExpression(true, intermVarExpr, InfoCache.IntEquals, InfoCache.IntZeroExpr))))
                                    }));
                        case Operator.GreaterThan:
                            return blockExpr.Update(
                                blockExpr.Variables.Concat(new[] { intermVarExpr }),
                                blockExpr.Expressions.Concat(
                                    new Expression[]
                                    {
                                        Expression.Assign(
                                            intermVarExpr,
                                            Expression.Call(lhsExpr, InfoCache.IntCompare, rhsExpr)),
                                        Expression.Assign(
                                            resExpr,
                                            Expression.AndAlso(
                                                resExpr,
                                                GenerateExpression(true, intermVarExpr, InfoCache.IntEquals, InfoCache.IntOneExpr)))
                                    }));
                        case Operator.GreaterThanOrEqual:
                            return blockExpr.Update(
                                blockExpr.Variables.Concat(new[] { intermVarExpr }),
                                blockExpr.Expressions.Concat(
                                    new Expression[]
                                    {
                                        Expression.Assign(
                                            intermVarExpr,
                                            Expression.Call(lhsExpr, InfoCache.IntCompare, rhsExpr)),
                                        Expression.Assign(
                                            resExpr,
                                            Expression.AndAlso(
                                                resExpr,
                                                Expression.OrElse(
                                                    GenerateExpression(true, intermVarExpr, InfoCache.IntEquals, InfoCache.IntOneExpr),
                                                    GenerateExpression(true, intermVarExpr, InfoCache.IntEquals, InfoCache.IntZeroExpr))))
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
                                            GenerateExpression(true, lhsExpr, InfoCache.IntEquals, rhsExpr)))
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
                                            GenerateExpression(false, lhsExpr, InfoCache.IntEquals, rhsExpr)))
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
                                                GenerateExpression(true, lhsExpr, InfoCache.StrContains, rhsExpr, InfoCache.StrCompExpr)))
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
                                                GenerateExpression(false, lhsExpr, InfoCache.StrContains, rhsExpr, InfoCache.StrCompExpr)))
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
                                            GenerateExpression(true, lhsExpr, InfoCache.StrEquals, rhsExpr, InfoCache.StrCompExpr)))
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
                                            GenerateExpression(false, lhsExpr, InfoCache.StrEquals, rhsExpr, InfoCache.StrCompExpr)))
                                    }));
                        default:
                            throw new InvalidOperationException($"Operation '{op}' not supported on strings.");
                    }
                }
                else if (isCollection)
                {
                    var method = (eType == InfoCache.StrType)
                        ? InfoCache.IEnumStrContains
                        : (eType == InfoCache.IntType)
                            ? InfoCache.IEnumIntContains
                            : _config.GetContainsMethod(eType);

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
                                                GenerateExpression(true, null, method, args: new[] { lhsExpr, rhsExpr })))
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
                                                GenerateExpression(false, null, method, args: new[] { lhsExpr, rhsExpr })))
                                    }));
                        default:
                            throw new InvalidOperationException($"Operation '{op}' not supported on a collections.");
                    }
                }
                else
                    throw new InvalidOperationException($"Property type '{pType}' not supported.");

                (Expression, Type) GetExprAndType(ReadOnlySpan<char> span)
                {
                    if (ParseKnownFunction(span) is Expression expr)
                        return (expr, expr.Type);
                    else
                    {
                        var p = span.Materialize();
                        if (!(Property(p) is PropertyInfo property))
                            throw new InvalidOperationException($"No such function or property '{p}'");

                        return (PropertyAccessExpression(property), property.PropertyType);
                    }
                }
                Expression GetValueExpression(string valueString)
                {
                    if (Property(valueString) is PropertyInfo compareProp
                        && compareProp.PropertyType == eType)
                        return PropertyAccessExpression(compareProp);

                    return (InfoCache.IConvType.IsAssignableFrom(eType))
                        ? Expression.Constant(Convert.ChangeType(valueString, eType), eType)
                        : Expression.Constant(valueString, InfoCache.StrType);
                }
                Expression GenerateExpression(
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
            }
        }

        private Func<IEnumerable<T>, IOrderedEnumerable<T>> ParseOrderByClause(
            ReadOnlySpan<char> orderBySpan,
            Dictionary<string, Expression> vars)
        {
            var exprs = new List<(Expression, bool)>();

            for (var slice = orderBySpan.SliceUntil(',', out var next); slice.Length > 0; slice = next.SliceUntil(',', out next))
            {
                var identifier = ParseVarDecl(ref slice);
                bool isDesc = ParseIsDescending(ref slice);
                var invocation = ParseFunctionOrInvocation(slice);

                if (identifier != null)
                {
                    vars.Add(identifier, invocation);
                }

                exprs.Add((invocation, isDesc));
            }

            MethodCallExpression call = null;

            foreach (var (expr, d) in exprs)
            {
                var selExpr = Expression.Lambda<Func<T, int>>(expr, _targetParamExpr);

                call = (call == null)
                    ? Expression.Call((d ? InfoCache.LinqOBDOpen : InfoCache.LinqOBOpen).MakeGenericMethod(_targetAndIntTypeArray), _targetsParamExpr, selExpr)
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

            for (var slice = selectSpan.SliceUntil(',', out var next); slice.Length > 0; slice = next.SliceUntil(',', out next))
            {
                var fmt = ParseFormatModifiers(ref slice);
                var (expr, type, name) = ParseFunctionVariableOrInvocation(slice, vars);

                exprs.Add(Expression.Convert(
                    Expression.Call(
                        InfoCache.StrConcat,
                        Expression.Constant(name),
                        InfoCache.ColonExpr,

                        Expression.Call(
                            _cfgExpr,
                            _cfgStringFmt,
                            (type == InfoCache.StrType)
                                ? expr
                                : Expression.Call(expr, InfoCache.ObjToString),
                            Expression.Constant(fmt))),
                    InfoCache.ObjType));
            }

            var fmtExpr = Expression.NewArrayInit(InfoCache.ObjType, exprs);

            var lambda = Expression.Lambda<Func<T, string>>(
                Expression.Call(InfoCache.StrJoin, InfoCache.CommaExpr, fmtExpr),
                _targetParamExpr);

            return lambda.Compile();
        }

        private static string ParseVarDecl(ref ReadOnlySpan<char> slice)
        {
            if (slice.IndexOf('|') >= 0)
            {
                var id = slice.SliceUntil('|', out slice).Materialize();
                if (_targetProps.ContainsKey(id))
                    throw new InvalidOperationException($"Cannot use '{id}' as an inline variable identifier as it is already the name of a property.");

                return id;
            }
            return null;
        }
        private static bool ParseIsDescending(ref ReadOnlySpan<char> slice)
        {
            if (slice.IndexOf(':') >= 0)
            {
                var prefix = slice.SliceUntil(':', out slice);
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
                var prefix = slice.SliceUntil(':', out slice);

                for (int i = 0; i < prefix.Length; i++)
                {
                    var cur = prefix[i];
                    if (cur == 'b')
                        fmt = fmt |= FormatModifiers.Bold;
                    if (cur == 'i')
                        fmt = fmt |= FormatModifiers.Italic;
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

            throw new InvalidOperationException($"No such property or known function '{p}'.");
        }
        private static (Expression, Type, string) ParseFunctionVariableOrInvocation(
            ReadOnlySpan<char> slice,
            IReadOnlyDictionary<string, Expression> vars)
        {
            if (ParseKnownFunction(slice) is Expression knownFunc)
                return (knownFunc, InfoCache.IntType, "");

            var p = slice.Materialize();
            if (vars.TryGetValue(p, out var expr))
            {
                return (expr, InfoCache.ObjType, p);
            }

            if (Property(p) is PropertyInfo property)
                return (PropertyAccessExpression(property), property.PropertyType, property.Name);

            throw new InvalidOperationException($"No such property or declared variable '{p}'.");
        }


        private static Expression ParseKnownFunction(ReadOnlySpan<char> slice)
        {
            if (slice.StartsWith(InfoCache.Sum.AsSpan()))
                return CreateSumExpression(slice.Slice(InfoCache.Sum.Length).TrimBraces());

            if (slice.StartsWith(InfoCache.Min.AsSpan()))
                return CreateMinExpression(slice.Slice(InfoCache.Min.Length).TrimBraces());

            if (slice.StartsWith(InfoCache.Max.AsSpan()))
                return CreateMaxExpression(slice.Slice(InfoCache.Max.Length).TrimBraces());

            if (slice.StartsWith(InfoCache.Average.AsSpan()))
                return CreateAverageExpression(slice.Slice(InfoCache.Average.Length).TrimBraces());

            if (slice.StartsWith(InfoCache.Count.AsSpan()))
                return CreateCountExpression(slice.Slice(InfoCache.Count.Length).TrimBraces());

            return null;
        }
        private static Expression CreateSumExpression(ReadOnlySpan<char> itemSpan)
            => Expression.Call(InfoCache.LinqSum, Expression.NewArrayInit(InfoCache.IntType, CreateIntList(itemSpan, InfoCache.Sum)));
        private static Expression CreateMinExpression(ReadOnlySpan<char> itemSpan)
            => Expression.Call(InfoCache.LinqMin, Expression.NewArrayInit(InfoCache.IntType, CreateIntList(itemSpan, InfoCache.Min)));
        private static Expression CreateMaxExpression(ReadOnlySpan<char> itemSpan)
            => Expression.Call(InfoCache.LinqMax, Expression.NewArrayInit(InfoCache.IntType, CreateIntList(itemSpan, InfoCache.Max)));
        private static Expression CreateAverageExpression(ReadOnlySpan<char> itemSpan)
            => Expression.Call(InfoCache.LinqAverage, Expression.NewArrayInit(InfoCache.IntType, CreateIntList(itemSpan, InfoCache.Average)));
        private static Expression CreateCountExpression(ReadOnlySpan<char> itemSpan)
        {
            var p = itemSpan.Materialize();
            if (!(Property(p) is PropertyInfo property))
                throw new InvalidOperationException($"No such property or known function '{p}'.");

            var (isCollection, eType) = IsCollectionType(property.PropertyType);
            if (!isCollection)
                throw new InvalidOperationException($"Property '{p}' must be a collection type to be used in 'count()'.");

            return Expression.Call(
                InfoCache.LinqCountOpen.MakeGenericMethod(eType),
                PropertyAccessExpression(property));
        }

        private static PropertyInfo Property(string propName)
            => (_targetProps.TryGetValue(propName, out var property))
                ? property : null;
        private static (bool, Type) IsCollectionType(Type type)
            => (type.IsGenericType && type.GetGenericTypeDefinition() == InfoCache.IEnumOpenType)
                    ? (true, type.GetGenericArguments()[0])
                    : (false, type);
        private static MemberExpression PropertyAccessExpression(PropertyInfo property)
            => Expression.Property(_targetParamExpr, property);
        private static List<Expression> CreateIntList(ReadOnlySpan<char> itemSpan, string functionName)
        {
            var props = new List<Expression>();
            for (var item = itemSpan.SliceUntil(',', out var next); item.Length > 0; item = next.SliceUntil(',', out next))
            {
                var p = item.Trim().Materialize();
                if (Property(p) is PropertyInfo property)
                {
                    if (property.PropertyType != InfoCache.IntType)
                        throw new InvalidOperationException($"Property '{p}' must be a number to be used in '{functionName}()'.");

                    props.Add(PropertyAccessExpression(property));
                }
                else if (Int32.TryParse(p, out var i))
                {
                    props.Add(Expression.Constant(i, InfoCache.IntType));
                }
                else
                {
                    throw new InvalidOperationException($"'{p}' must be a number or a numeric property to be used in '{functionName}()'.");
                }
            }

            return props;
        }
    }
}
