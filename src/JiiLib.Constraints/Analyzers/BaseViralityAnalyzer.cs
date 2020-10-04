using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace JiiLib.Constraints.Analyzers
{
    internal abstract class BaseViralityAnalyzer : DiagnosticAnalyzer
    {
        private Type CheckedAttribute { get; }

        private protected BaseViralityAnalyzer(Type attribute)
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
            if (!(context.Node is TypeArgumentListSyntax typeArgumentList))
                return;

            if (!ShouldAnalyze(typeArgumentList))
                return;

            if (!(typeArgumentList.Parent is { } parent))
                return;

            var declarationSymbol = context.SemanticModel.GetSymbolInfo(parent).Symbol!;
            var originalSymbol = declarationSymbol.OriginalDefinition;

            //var parentName = parent switch
            //{
            //    MethodDeclarationSyntax method => method.Identifier.ValueText,
            //    BaseTypeDeclarationSyntax type => type.Identifier.ValueText,
            //    DelegateDeclarationSyntax del => del.Identifier.ValueText,
            //    _ => String.Empty
            //};

            _ = originalSymbol switch
            {
                INamedTypeSymbol os => CheckTypeParameters(os.TypeParameters, typeArgumentList.Arguments, os.Name, context),
                IMethodSymbol os => CheckTypeParameters(os.TypeParameters, typeArgumentList.Arguments, os.Name, context),
                _ => false
            };
        }

        private bool CheckTypeParameters(
            ImmutableArray<ITypeParameterSymbol> typeParams,
            SeparatedSyntaxList<TypeSyntax> typeArgs,
            string calleeId,
            SyntaxNodeAnalysisContext context)
        {
            if (typeParams.Length != typeArgs.Count)
                return false;

            foreach (var (typeParam, typeArg) in typeParams.ZipT(typeArgs))
            {
                var attrs = typeParam.GetAttributes();
                if (attrs.Any(a => a?.AttributeClass?.Name == CheckedAttribute.Name)
                    && context.SemanticModel.GetSymbolInfo(typeArg).Symbol is ITypeParameterSymbol typeSymbol
                    && typeSymbol.GetAttributes().None(a => a?.AttributeClass?.Name == CheckedAttribute.Name))
                {
                    var declaringSymbolId = typeSymbol switch
                    {
                        { DeclaringMethod: { } method } => method.Name,
                        { DeclaringType: { } type } => type.Name,
                        _ => String.Empty
                    };

                    var diagnostic = Diagnostic.Create(GetDiagnosticDescriptor(), typeArg.GetLocation(),
                        typeSymbol.Name, declaringSymbolId, calleeId);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            return true;
        }

        [DebuggerStepThrough]
        private protected virtual bool ShouldAnalyze(TypeArgumentListSyntax typeArgumentList) => true;
        private protected abstract DiagnosticDescriptor GetDiagnosticDescriptor();
    }
}
