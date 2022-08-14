using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace JiiLib.Constraints.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class SelfTypeUsageAnalyzer : BaseUsageAnalyzer<SelfTypeAttribute, BasicDiagnosticChoice>
    {
        private const string DiagnosticId = "JLC0003U";
        private const string Title = "Use of the SelfType attribute is ineffective";
        private const string MessageFormat = "Use of the SelfType attribute on '{0}' in generic method '{1}' is ineffective";
        private const string Description = "Invalid use of constraint attribute.";
        private const string Category = "API Usage";

        private static readonly DiagnosticDescriptor _rule = new(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description,
            customTags: WellKnownDiagnosticTags.Unnecessary);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        private protected override bool ShouldAnalyze(TypeParameterListSyntax typeArgumentList)
            => typeArgumentList.Parent.IsKind(SyntaxKind.MethodDeclaration);
        private protected override BasicDiagnosticChoice GetDiagnosticChoice(ITypeParameterSymbol typeParameterSymbol) => BasicDiagnosticChoice.No;
        private protected override DiagnosticDescriptor GetDiagnosticDescriptor(BasicDiagnosticChoice _) => _rule;
    }
}
