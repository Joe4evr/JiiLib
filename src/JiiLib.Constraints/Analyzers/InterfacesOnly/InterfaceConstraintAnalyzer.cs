using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace JiiLib.Constraints.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class InterfaceConstraintAnalyzer : BaseConstraintAnalyzer<InterfacesOnlyAttribute>
    {
        private const string DiagnosticId = "JLC0001";
        private const string Title = "Type argument must be an interface type";
        private const string MessageFormat = "Type argument '{0}' must be an interface type";
        private const string Description = "Passing an invalid type argument.";
        private const string Category = "API Usage";

        private static readonly DiagnosticDescriptor _rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        private protected override bool CompliesWithConstraint(ITypeSymbol typeSymbol)
            => typeSymbol.TypeKind == TypeKind.Interface;

        private protected override DiagnosticDescriptor GetDiagnosticDescriptor() => _rule;
    }
}
