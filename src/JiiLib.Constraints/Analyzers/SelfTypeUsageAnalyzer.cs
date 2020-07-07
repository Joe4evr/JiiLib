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
    internal sealed class SelfTypeUsageAnalyzer : DiagnosticAnalyzer
    {
        private const string DiagnosticId = "JLC0004";
        private const string Title = "Use of the SelfType attribute is ineffective.";
        private const string MessageFormat = "Use of the SelfType attribute in generic method '{0}' is ineffective.";
        private const string Description = "Invalid use of constraint attribute.";
        private const string Category = "API Usage";

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description,
            customTags: WellKnownDiagnosticTags.Unnecessary);
        private static readonly Type _attributeType = typeof(SelfTypeAttribute);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeTypeParameter, SyntaxKind.TypeParameter);
        }

        private static void AnalyzeTypeParameter(SyntaxNodeAnalysisContext context)
        {
            if (!context.Node.Parent.Parent.IsKind(SyntaxKind.MethodDeclaration))
                return;

            var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node);
            if (!(symbol is ITypeParameterSymbol typeParameterSymbol))
                return;

            var attribute = typeParameterSymbol.GetAttributes().FirstOrDefault(a => a?.AttributeClass?.Name == _attributeType.Name);
            if (attribute is null)
                return;

            var attrSpan = attribute.ApplicationSyntaxReference.Span;
            var attrLocation = Location.Create(context.Node.SyntaxTree, attrSpan);

            var methodNode = context.Node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            var diagnostic = Diagnostic.Create(_rule, attrLocation, methodNode.Identifier.ValueText);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
