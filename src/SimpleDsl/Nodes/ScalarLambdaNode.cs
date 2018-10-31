using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl.Nodes
{
    internal sealed class ScalarLambdaNode : IQueryNode
    {
        private static readonly MethodInfo _checkScalarMethod;

        static ScalarLambdaNode()
        {
            var TSourceType = Type.MakeGenericMethodParameter(0);
            var funcTToBool = typeof(Func<,>).MakeGenericType(TSourceType, InfoCache.BoolType);
            var checkParamTypesArray = new[] { funcTToBool, TSourceType };

            _checkScalarMethod = typeof(ScalarLambdaNode).GetMethod(nameof(Check), 1, checkParamTypesArray);
        }

        public PropertyAccessNode Property { get; }
        public Expression Value { get; }

        public ScalarLambdaNode(LambdaExpression lambda, PropertyAccessNode property)
        {
            Property = property;

            var method = _checkScalarMethod.MakeGenericMethod(new[] { Property.Value.Type });
            Value = Expression.Call(method, lambda, Property.Value);
        }

        [DebuggerStepThrough]
        public static bool Check<T>(Func<T, bool> predicate, T arg)
            => predicate(arg);
    }
}
