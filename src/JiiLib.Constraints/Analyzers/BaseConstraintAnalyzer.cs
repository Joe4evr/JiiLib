using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace JiiLib.Constraints.Analyzers
{
    internal abstract class BaseConstraintAnalyzer : DiagnosticAnalyzer
    {
        private Type CheckedAttribute { get; }

        private protected BaseConstraintAnalyzer(Type attribute)
        {
            CheckedAttribute = attribute;
        }

        public sealed override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeTypeArgumentList, SyntaxKind.TypeArgumentList);
            
        }

        private void AnalyzeTypeArgumentList(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not TypeArgumentListSyntax typeArgumentList)
                return;

            if (!ShouldAnalyze(typeArgumentList))
                return;

            var declarationSymbol = context.SemanticModel.GetSymbolInfo(typeArgumentList.Parent!).Symbol!;
            var originalSymbol = declarationSymbol.OriginalDefinition;

            _ = (originalSymbol, declarationSymbol) switch
            {
                (INamedTypeSymbol os, INamedTypeSymbol _) => CheckTypeParameters(os.TypeParameters, typeArgumentList.Arguments, context),
                (IMethodSymbol os, IMethodSymbol _) => CheckTypeParameters(os.TypeParameters, typeArgumentList.Arguments, context),
                _ => false
            };
        }

        private bool CheckTypeParameters(
            ImmutableArray<ITypeParameterSymbol> typeParams,
            SeparatedSyntaxList<TypeSyntax> typeArgs,
            SyntaxNodeAnalysisContext context)
        {
            if (typeParams.Length != typeArgs.Count)
                return false;

            foreach (var (typeParam, typeArg) in typeParams.ZipT(typeArgs))
            {
                var attrs = typeParam.GetAttributes();
                if (attrs.Any(a => a?.AttributeClass?.Name == CheckedAttribute.Name)
                    && context.SemanticModel.GetSymbolInfo(typeArg).Symbol is ITypeSymbol typeSymbol
                    && !CompliesWithConstraint(typeArg, context.SemanticModel, typeSymbol))
                {
                    var diagnostic = Diagnostic.Create(GetDiagnosticDescriptor(), typeArg.GetLocation(), typeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            return true;
        }

        private protected virtual bool ShouldAnalyze(TypeArgumentListSyntax typeArgumentList) => true;
        private protected virtual bool CompliesWithConstraint(
            TypeSyntax typeSyntaxNode, SemanticModel semanticModel, ITypeSymbol typeSymbol)
            => CompliesWithConstraint(typeSymbol);
        private protected abstract bool CompliesWithConstraint(ITypeSymbol typeSymbol);
        private protected abstract DiagnosticDescriptor GetDiagnosticDescriptor();
    }
}
