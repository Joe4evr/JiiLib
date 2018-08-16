using System;
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
        private static readonly Type _objType = typeof(object);
        private static readonly Type _strType = typeof(string);
        private static readonly Type _boolType = typeof(bool);
        private static readonly Type _intType = typeof(int);
        private static readonly Type _iconvType = typeof(IConvertible);
        private static readonly Type _linqType = typeof(Enumerable);
        private static readonly Type _ienumType = typeof(IEnumerable<>);
        private static readonly Type _targetType = typeof(T);
        private static readonly Type _ienumTargetType = typeof(IEnumerable<T>);
        private static readonly Type _iOrdEnumTargetType = typeof(IOrderedEnumerable<T>);
        private static readonly Type _funcTargetToIntType = typeof(Func<T, int>);

        private static readonly IReadOnlyDictionary<string, PropertyInfo> _targetProps = _targetType.GetProperties().ToImmutableDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        private static readonly Type[] _strTypeArr = new Type[] { _strType };
        private static readonly Type[] _strCompTypeArr = new Type[] { _strType, typeof(StringComparison) };
        private static readonly Type[] _intTypeArr = new Type[] { _intType };

        private static readonly MethodInfo _toString = _objType.GetMethod("ToString");
        private static readonly MethodInfo _strJoin = _strType.GetMethod("Join", new Type[] { _strType, typeof(object[]) });
        private static readonly MethodInfo _strContains = _strType.GetMethod("Contains", _strTypeArr);
        private static readonly MethodInfo _strConcat = _strType.GetMethod("Concat", new Type[] { _strType, _strType, _strType });
        private static readonly MethodInfo _strEquals = _strType.GetMethod("Equals", _strCompTypeArr);
        private static readonly MethodInfo _intEquals = _intType.GetMethod("Equals", _intTypeArr);
        private static readonly MethodInfo _intCompare = _intType.GetMethod("CompareTo", _intTypeArr);
        private static readonly MethodInfo _linqSum = _linqType.GetMethod("Sum", new Type[] { typeof(IEnumerable<int>) });

        private static readonly ParameterExpression _targetParamExpr = Expression.Parameter(_targetType, "target");
        private static readonly ParameterExpression _targetsParamExpr = Expression.Parameter(_ienumTargetType, "targets");
        private static readonly ConstantExpression _commaExpr = Expression.Constant(", ");
        private static readonly ConstantExpression _colonExpr = Expression.Constant(": ");
        private static readonly ConstantExpression _strCompExpr = Expression.Constant(StringComparison.OrdinalIgnoreCase);
        private static readonly ConstantExpression _intNegOneExpr = Expression.Constant(-1);
        private static readonly ConstantExpression _intZeroExpr = Expression.Constant(0);
        private static readonly ConstantExpression _intOneExpr = Expression.Constant(1);

        private static readonly string Where = "where";
        private static readonly string OrderBy = "orderby";
        private static readonly string Skip = "skip";
        private static readonly string Take = "take";
        private static readonly string Select = "select";

        private static readonly string Sum = "sum";

        private static readonly string Contains = "<-";
        private static readonly string NotContains = "!<-";
        private static readonly string LessThan = "<";
        private static readonly string LessThanOrEqual = "<=";
        private static readonly string GreaterThan = ">";
        private static readonly string GreaterThanOrEqual = ">=";
        private static readonly string IsEqual = "==";
        private static readonly string IsNotEqual = "!=";

        private static Operator ParseOperator(ReadOnlySpan<char> opSpan)
        {
            if (opSpan.SequenceEqual(Contains.AsSpan()))
                return Operator.Contains;
            if (opSpan.SequenceEqual(NotContains.AsSpan()))
                return Operator.NotContains;
            else if (opSpan.SequenceEqual(LessThan.AsSpan()))
                return Operator.LessThan;
            else if (opSpan.SequenceEqual(LessThanOrEqual.AsSpan()))
                return Operator.LessThanOrEqual;
            else if (opSpan.SequenceEqual(GreaterThan.AsSpan()))
                return Operator.GreaterThan;
            else if (opSpan.SequenceEqual(GreaterThanOrEqual.AsSpan()))
                return Operator.GreaterThanOrEqual;
            else if (opSpan.SequenceEqual(IsEqual.AsSpan()))
                return Operator.IsEqual;
            else if (opSpan.SequenceEqual(IsNotEqual.AsSpan()))
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
        private readonly Expression _cfgExpr;
        private readonly MethodInfo _stringFmt;

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
            _stringFmt = typeof(IInterpreterConfig<T>).GetMethod("FormatString", new Type[] { _strType, typeof(FormatModifiers) });
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
        /// <exception cref="InvalidOperationException">
        ///     Some part of the query was not valid.
        /// </exception>
        public QueryParseResult<T> ParseFull(string input)
        {
            Func<T, bool> predicate = null;
            Func<IEnumerable<T>, IOrderedEnumerable<T>> order = null;
            int skipAmount = 0;
            int takeAmount = 10;
            Func<T, string> selector = null;

            var span = input.AsSpan().Trim();
            var inlineVars = new Dictionary<string, Expression>(StringComparer.OrdinalIgnoreCase);

            var opWord = span.SliceUntil(' ', out var remainder);
            if (opWord.SequenceEqual(Where.AsSpan()))
            {
                var matchIdx = remainder.FindMatchingBrace();
                var whereClause = remainder.Slice(1, matchIdx - 1);
                remainder = remainder.Slice(matchIdx + 1).Trim();
                predicate = ParseFilters(whereClause);

                opWord = remainder.SliceUntil(' ', out remainder);
            }

            if (opWord.SequenceEqual(OrderBy.AsSpan()))
            {
                var matchIdx = remainder.FindMatchingBrace();
                var orderByClause = remainder.Slice(1, matchIdx - 1);
                remainder = remainder.Slice(matchIdx + 1).Trim();
                order = ParseOrderByClause(orderByClause, inlineVars);

                opWord = remainder.SliceUntil(' ', out remainder);
            }

            if (opWord.SequenceEqual(Skip.AsSpan()))
            {
                var skipStr = remainder.SliceUntil(' ', out remainder).Materialize();
                if (!Int32.TryParse(skipStr, out var s))
                    throw new InvalidOperationException();

                skipAmount = s;

                opWord = remainder.SliceUntil(' ', out remainder);
            }

            if (opWord.SequenceEqual(Take.AsSpan()))
            {
                var takeStr = remainder.SliceUntil(' ', out remainder).Materialize();
                if (!Int32.TryParse(takeStr, out var t))
                    throw new InvalidOperationException();

                takeAmount = t;

                opWord = remainder.SliceUntil(' ', out remainder);
            }

            if (opWord.SequenceEqual(Select.AsSpan()))
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
            var resExpr = Expression.Variable(_boolType, "result");
            var filterBlockExpr = Expression.Block(_boolType,
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
                ReadOnlySpan<char> propSpan,
                ReadOnlySpan<char> opSpan,
                ReadOnlySpan<char> valSpan)
            {
                var p = propSpan.Materialize();
                if (!_targetProps.TryGetValue(p, out var prop))
                    throw new InvalidOperationException($"No such property '{p}'");
                var pType = prop.PropertyType;

                var (isCollection, eType) = (pType.IsGenericType && pType.GetGenericTypeDefinition() == _ienumType)
                    ? (true, pType.GetGenericArguments()[0])
                    : (false, pType);

                var valExpr = GetValueExpression(valSpan.Materialize());
                var propExpr = Expression.Property(_targetParamExpr, prop);
                var intermVarExpr = Expression.Variable(pType, $"interm{blockExpr.Variables.Count}");
                var op = ParseOperator(opSpan);

                if (pType == _intType)
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
                                            Expression.Call(propExpr, _intCompare, valExpr)),
                                        Expression.Assign(
                                            resExpr,
                                            Expression.AndAlso(
                                                resExpr,
                                                GenerateExpression(true, intermVarExpr, _intEquals, _intNegOneExpr)))
                                    }));
                        case Operator.LessThanOrEqual:
                            return blockExpr.Update(
                                blockExpr.Variables.Concat(new[] { intermVarExpr }),
                                blockExpr.Expressions.Concat(
                                    new Expression[]
                                    {
                                        Expression.Assign(
                                            intermVarExpr,
                                            Expression.Call(propExpr, _intCompare, valExpr)),
                                        Expression.Assign(
                                            resExpr,
                                            Expression.AndAlso(
                                                resExpr,
                                                Expression.OrElse(
                                                    GenerateExpression(true, intermVarExpr, _intEquals, _intNegOneExpr),
                                                    GenerateExpression(true, intermVarExpr, _intEquals, _intZeroExpr))))
                                    }));
                        case Operator.GreaterThan:
                            return blockExpr.Update(
                                blockExpr.Variables.Concat(new[] { intermVarExpr }),
                                blockExpr.Expressions.Concat(
                                    new Expression[]
                                    {
                                        Expression.Assign(
                                            intermVarExpr,
                                            Expression.Call(propExpr, _intCompare, valExpr)),
                                        Expression.Assign(
                                            resExpr,
                                            Expression.AndAlso(
                                                resExpr,
                                                GenerateExpression(true, intermVarExpr, _intEquals, _intOneExpr)))
                                    }));
                        case Operator.GreaterThanOrEqual:
                            return blockExpr.Update(
                                blockExpr.Variables.Concat(new[] { intermVarExpr }),
                                blockExpr.Expressions.Concat(
                                    new Expression[]
                                    {
                                        Expression.Assign(
                                            intermVarExpr,
                                            Expression.Call(propExpr, _intCompare, valExpr)),
                                        Expression.Assign(
                                            resExpr,
                                            Expression.AndAlso(
                                                resExpr,
                                                Expression.OrElse(
                                                    GenerateExpression(true, intermVarExpr, _intEquals, _intOneExpr),
                                                    GenerateExpression(true, intermVarExpr, _intEquals, _intZeroExpr))))
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
                                            GenerateExpression(true, propExpr, _intEquals, valExpr)))
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
                                            GenerateExpression(false, propExpr, _intEquals, valExpr)))
                                    }));
                        default:
                            throw new InvalidOperationException($"Operation '{op}' not supported on integers.");
                    }
                }
                else if (pType == _strType)
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
                                                GenerateExpression(true, propExpr, _strContains, valExpr, _strCompExpr)))
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
                                                GenerateExpression(false, propExpr, _strContains, valExpr, _strCompExpr)))
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
                                            GenerateExpression(true, propExpr, _strEquals, valExpr, _strCompExpr)))
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
                                            GenerateExpression(false, propExpr, _strEquals, valExpr, _strCompExpr)))
                                    }));
                        default:
                            throw new InvalidOperationException($"Operation '{op}' not supported on strings.");
                    }
                }
                else if (isCollection)
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
                                                GenerateExpression(true, null, _config.GetContainsMethod(eType), propExpr, valExpr)))
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
                                                GenerateExpression(false, null, _config.GetContainsMethod(eType), propExpr, valExpr)))
                                    }));
                        default:
                            throw new InvalidOperationException($"Operation '{op}' not supported on a collections.");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Property type '{pType}' not supported.");
                }

                Expression GetValueExpression(string valueString)
                {
                    if (_targetProps.TryGetValue(valueString, out var compareProp))
                    {
                        if (compareProp.PropertyType == eType)
                        {
                            return Expression.Property(_targetParamExpr, compareProp);
                        }
                    }

                    if (_iconvType.IsAssignableFrom(eType))
                    {
                        return Expression.Constant(Convert.ChangeType(valueString, eType), eType);
                    }
                    else
                    {
                        return Expression.Constant(valueString, _strType);
                    }
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
                var invocation = ParseInvocationOrFunction(slice);

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
                    ? Expression.Call(_config.GetOrderByMethod(_targetType, d), _targetsParamExpr, selExpr)
                    : Expression.Call(_config.GetThenByMethod(_targetType, d), call, selExpr);
            }

            var lambda = Expression.Lambda<Func<IEnumerable<T>, IOrderedEnumerable<T>>>(call, _targetsParamExpr);

            return lambda.Compile();
        }

        private Func<T, string> ParseSelectClause(
            ReadOnlySpan<char> selectSpan,
            Dictionary<string, Expression> vars)
        {
            var exprs = new List<Expression>();

            for (var slice = selectSpan.SliceUntil(',', out var next); slice.Length > 0; slice = next.SliceUntil(',', out next))
            {
                var fmt = ParseFormatModifiers(ref slice);
                var (expr, type, name) = ParseInvocationOrVariable(slice, vars);

                exprs.Add(Expression.Convert(
                    Expression.Call(
                        _strConcat,
                        Expression.Constant(name),
                        _colonExpr,

                        Expression.Call(
                            _cfgExpr,
                            _stringFmt,
                            (type == _strType)
                                ? expr
                                : Expression.Call(expr, _toString),
                            Expression.Constant(fmt))),
                    _objType));
            }

            var fmtExpr = Expression.NewArrayInit(_objType, exprs);

            var lambda = Expression.Lambda<Func<T, string>>(
                Expression.Call(_strJoin, _commaExpr, fmtExpr),
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
        private static Expression ParseInvocationOrFunction(ReadOnlySpan<char> slice)
        {
            if (slice.StartsWith(Sum.AsSpan()))
            {
                return CreateSumExpression(slice.Slice(Sum.Length).TrimBraces());
            }

            var p = slice.Materialize();
            if (_targetProps.TryGetValue(p, out var property))
            {
                return Expression.Property(_targetParamExpr, property);
            }

            throw new InvalidOperationException($"No such property or known function '{p}'.");
        }
        private static (Expression, Type, string) ParseInvocationOrVariable(
            ReadOnlySpan<char> slice,
            IReadOnlyDictionary<string, Expression> vars)
        {
            var p = slice.Materialize();
            if (vars.TryGetValue(p, out var expr))
            {
                return (expr, _objType, p);
            }

            if (_targetProps.TryGetValue(p, out var property))
            {
                return (Expression.Property(_targetParamExpr, property), property.PropertyType, property.Name);
            }

            throw new InvalidOperationException($"No such property or declared variable '{p}'.");
        }
        private static Expression CreateSumExpression(ReadOnlySpan<char> itemSpan)
        {
            var props = new List<Expression>();
            for (var item = itemSpan.SliceUntil(' ', out var next); item.Length > 0; item = next.SliceUntil(',', out next))
            {
                var it = item.Materialize();
                if (_targetProps.TryGetValue(it, out var property))
                {
                    if (property.PropertyType != _intType)
                        throw new InvalidOperationException($"Property '{it}' must be a number to be used in 'sum()'.");

                    props.Add(Expression.Property(_targetParamExpr, property));
                }
                else if (Int32.TryParse(it, out var i))
                {
                    props.Add(Expression.Constant(i, _intType));
                }
                else
                {
                    throw new InvalidOperationException($"'{it}' must be a number or a numeric property to be used in 'sum()'.");
                }
            }

            return Expression.Call(_linqSum, Expression.NewArrayInit(_intType, props));
        }
    }

    [Flags]
    public enum FormatModifiers
    {
        None = 0,
        Bold = 1,
        Italic = 1 << 1
    }
}
