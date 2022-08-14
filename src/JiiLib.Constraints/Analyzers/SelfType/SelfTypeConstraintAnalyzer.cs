using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace JiiLib.Constraints.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class SelfTypeConstraintAnalyzer : BaseConstraintAnalyzer<SelfTypeAttribute>
    {
        private const string DiagnosticId = "JLC0003";
        private const string Title = "Type argument must be the implementing type";
        private const string MessageFormat = "Type argument '{0}' must be the implementing type";
        private const string Description = "Passing an invalid type argument.";
        private const string Category = "API Usage";

        private static readonly DiagnosticDescriptor _rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        private protected override bool ShouldAnalyze(TypeArgumentListSyntax typeArgumentList) 
            => typeArgumentList.Ancestors().Any(n => n.IsKind(SyntaxKind.BaseList));
        private protected override bool CompliesWithConstraint(
            ITypeParameterSymbol typeParameterSymbol, SemanticModel semanticModel, ITypeSymbol typeSymbol)
            => typeSymbol.Equals(typeParameterSymbol, SymbolEqualityComparer.Default);

        private protected override bool CompliesWithConstraint(ITypeSymbol typeSymbol)
            => throw new NotImplementedException();


        private protected override DiagnosticDescriptor GetDiagnosticDescriptor() => _rule;
    }
}
