using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace JiiLib.Constraints.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class NoInterfacesConstraintAnalyzer : BaseConstraintAnalyzer
    {
        private const string DiagnosticId = "JLC0004";
        private const string Title = "Type argument may not be an interface type";
        private const string MessageFormat = "Type argument '{0}' may not be an interface type";
        private const string Description = "Passing an invalid type argument.";
        private const string Category = "API Usage";

        private static readonly DiagnosticDescriptor _rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
        private static readonly Type _attributeType = typeof(NoInterfacesAttribute);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        public NoInterfacesConstraintAnalyzer()
            : base(_attributeType)
        {
        }

        private protected override bool CompliesWithConstraint(ITypeSymbol typeSymbol)
            => typeSymbol.TypeKind != TypeKind.Interface;

        private protected override DiagnosticDescriptor GetDiagnosticDescriptor() => _rule;
    }
}
