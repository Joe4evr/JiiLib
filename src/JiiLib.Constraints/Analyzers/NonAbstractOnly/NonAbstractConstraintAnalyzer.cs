using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace JiiLib.Constraints.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class NonAbstractConstraintAnalyzer : BaseConstraintAnalyzer
    {
        private const string DiagnosticId = "JLC0002";
        private const string Title = "Type argument must be a non-abstract type.";
        private const string MessageFormat = "Type argument '{0}' must be a non-abstract type.";
        private const string Description = "Passing an invalid type argument.";
        private const string Category = "API Usage";

        private static readonly DiagnosticDescriptor _rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
        private static readonly Type _attributeType = typeof(NonAbstractOnlyAttribute);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        public NonAbstractConstraintAnalyzer()
            : base(_attributeType)
        {
        }

        private protected override bool CompliesWithConstraint(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is ITypeParameterSymbol otherTypeParameter)
            {
                // The type argument is itself a Type Parameter from its parent scope.
                // In this case, the 'struct' and 'new()' constraints are implicitly compliant.
                return (otherTypeParameter.HasConstructorConstraint
                    || otherTypeParameter.HasValueTypeConstraint);
            }

            return typeSymbol.IsAbstract is false;
        }

        private protected override DiagnosticDescriptor GetDiagnosticDescriptor() => _rule;
    }
}
