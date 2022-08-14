using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace JiiLib.Constraints.Analyzers
{
    internal abstract class BaseConstraintAnalyzer<TAttribute> : DiagnosticAnalyzer
        where TAttribute : Attribute
    {
        private static readonly Type _attrType = typeof(TAttribute);

        private protected BaseConstraintAnalyzer() { }

        public sealed override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeTypeParameterList, SyntaxKind.TypeParameterList);
            context.RegisterSyntaxNodeAction(AnalyzeTypeArgumentList, SyntaxKind.TypeArgumentList);
        }

        private void AnalyzeTypeParameterList(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not TypeParameterListSyntax typeParameterList)
                return;

            if (typeParameterList.Parent is not MethodDeclarationSyntax methodDeclaration)
                return;

            if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration) is not { } declarationSymbol)
                return;

            if (!declarationSymbol.ExplicitInterfaceImplementations.IsEmpty)
                return; // Explicit implementations don't require the attribute

            if (declarationSymbol.IsImplementationOfAnyImplicitInterfaceMember<IMethodSymbol>(out var interfaceMethod))
            {
                _ = CheckTypeParameters(interfaceMethod.TypeParameters, methodDeclaration.TypeParameterList!.Parameters, context);
            }
            if (declarationSymbol.IsOverride)
            {
                var baseMethod = declarationSymbol.OverriddenMethod!;
                _ = CheckTypeParameters(baseMethod.TypeParameters, methodDeclaration.TypeParameterList!.Parameters, context);
            }


            //_ = CheckTypeParameters(interfaceMethod.TypeParameters, methodDeclaration.TypeParameterList!.Parameters, context);
        }

        private void AnalyzeTypeArgumentList(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not TypeArgumentListSyntax typeArgumentList)
                return;

            if (!ShouldAnalyze(typeArgumentList))
                return;

            var declarationSymbol = context.SemanticModel.GetSymbolInfo(typeArgumentList.Parent!).Symbol!;
            var originalSymbol = declarationSymbol.OriginalDefinition;

            _ = originalSymbol switch
            {
                INamedTypeSymbol os => CheckTypeParameters(os.TypeParameters, typeArgumentList.Arguments, context),
                IMethodSymbol os => CheckTypeParameters(os.TypeParameters, typeArgumentList.Arguments, context),
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
                if (typeParam.GetAttributeData(_attrType) is { }
                    && context.SemanticModel.GetSymbolInfo(typeArg).Symbol is ITypeSymbol typeArgSymbol)
                {
                    CheckArgToParam(typeParam, typeArgSymbol, typeArg, context);
                }
            }

            return true;
        }

        private bool CheckTypeParameters(
            ImmutableArray<ITypeParameterSymbol> typeParams,
            SeparatedSyntaxList<TypeParameterSyntax> typeArgs,
            SyntaxNodeAnalysisContext context)
        {
            if (typeParams.Length != typeArgs.Count)
                return false;

            foreach (var (typeParam, typeArg) in typeParams.ZipT(typeArgs))
            {
                if (typeParam.GetAttributeData(_attrType) is { }
                    && context.SemanticModel.GetDeclaredSymbol(typeArg) is ITypeSymbol typeArgSymbol)
                {
                    CheckArgToParam(typeParam, typeArgSymbol, typeArg, context);
                }
            }

            return true;
        }


        private void CheckArgToParam(
            ITypeParameterSymbol typeParameter,
            ITypeSymbol typeArgument,
            SyntaxNode typeArgNode,
            SyntaxNodeAnalysisContext context)
        {
            if ((typeArgument is ITypeParameterSymbol otherTypeParam)
                && otherTypeParam.GetAttributeData(_attrType) is { })
            {
                return;
            }
            else if (!CompliesWithConstraint(typeParameter, context.SemanticModel, typeArgument))
            {
                var diagnostic = Diagnostic.Create(GetDiagnosticDescriptor(), typeArgNode.GetLocation(), typeArgument.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private protected virtual bool ShouldAnalyze(TypeArgumentListSyntax typeArgumentList) => true;
        private protected virtual bool CompliesWithConstraint(
            ITypeParameterSymbol typeParameterSymbol, SemanticModel semanticModel, ITypeSymbol typeArgSymbol)
        {
            return CompliesWithConstraint(typeArgSymbol);
        }

        private protected abstract bool CompliesWithConstraint(ITypeSymbol typeSymbol);
        private protected abstract DiagnosticDescriptor GetDiagnosticDescriptor();
    }
}
