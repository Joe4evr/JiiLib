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
            //context.RegisterSyntaxNodeAction(AnalyzeTypeParameterList, SyntaxKind.TypeParameterList);
            context.RegisterSyntaxNodeAction(AnalyzeTypeArgumentList, SyntaxKind.TypeArgumentList);
        }

        //private void AnalyzeTypeParameterList(SyntaxNodeAnalysisContext context)
        //{
        //    if (context.Node is not TypeParameterListSyntax typeParameterList)
        //        return;

        //    if (typeParameterList.Parent is not MethodDeclarationSyntax methodDeclaration)
        //        return;

        //    if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration) is not { } declarationSymbol)
        //        return;

        //    if (!declarationSymbol.ExplicitInterfaceImplementations.IsEmpty)
        //        return; // Explicit implementations don't require the attribute

        //    var symbolId = declarationSymbol.OriginalDefinition.Name;
        //    if (declarationSymbol.IsImplementationOfAnyImplicitInterfaceMember<IMethodSymbol>(out var interfaceMethod))
        //    {
        //        _ = CheckTypeParameters(interfaceMethod.TypeParameters,
        //            methodDeclaration.TypeParameterList!.Parameters,
        //            symbolId, context);
        //    }
        //    if (declarationSymbol.IsOverride)
        //    {
        //        var baseMethod = declarationSymbol.OverriddenMethod!;
        //        _ = CheckTypeParameters(baseMethod.TypeParameters,
        //            methodDeclaration.TypeParameterList!.Parameters,
        //            symbolId, context);
        //    }
        //}

        private void AnalyzeTypeArgumentList(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not TypeArgumentListSyntax typeArgumentList)
                return;

            if (!ShouldAnalyze(typeArgumentList))
                return;

            if (!(typeArgumentList.Parent is { } parent))
                return;

            var declarationSymbol = context.SemanticModel.GetSymbolInfo(parent).Symbol!;
            var originalSymbol = declarationSymbol.OriginalDefinition;

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
                if (attrs.Any(a =>
                    a?.AttributeClass?.ContainingNamespace?.Name == "JiiLib.Constraints"
                    && a?.AttributeClass?.Name == CheckedAttribute.Name)
                    && context.SemanticModel.GetSymbolInfo(typeArg).Symbol is ITypeParameterSymbol typeArgSymbol)
                {
                    CheckArgToParam(typeArgSymbol, typeArg, calleeId, context);
                }
            }

            return true;
        }

        //private bool CheckTypeParameters(
        //    ImmutableArray<ITypeParameterSymbol> typeParams,
        //    SeparatedSyntaxList<TypeParameterSyntax> typeArgs,
        //    string calleeId,
        //    SyntaxNodeAnalysisContext context)
        //{
        //    if (typeParams.Length != typeArgs.Count)
        //        return false;

        //    foreach (var (typeParam, typeArg) in typeParams.ZipT(typeArgs))
        //    {
        //        if (HasAttribute(typeParam)
        //            && context.SemanticModel.GetDeclaredSymbol(typeArg) is ITypeParameterSymbol typeArgSymbol)
        //        {
        //            CheckArgToParam(typeParam, typeArgSymbol, typeArg, calleeId, context);
        //        }
        //    }

        //    return true;
        //}


        private void CheckArgToParam(
            ITypeParameterSymbol typeArgument,
            SyntaxNode typeArgNode, string calleeId,
            SyntaxNodeAnalysisContext context)
        {
            if ((typeArgument is ITypeParameterSymbol otherTypeParam) && HasAttribute(otherTypeParam))
            {
                return;
            }
            else if (!IsImplicitlyValid(typeArgument))
            {
                var declaringSymbolId = typeArgument switch
                {
                    { DeclaringMethod: { } method } => method.Name,
                    { DeclaringType: { } type } => type.Name,
                    _ => String.Empty
                };

                var diagnostic = Diagnostic.Create(GetDiagnosticDescriptor(), typeArgNode.GetLocation(),
                    typeArgument.Name, declaringSymbolId, calleeId);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private bool HasAttribute(ITypeParameterSymbol typeParamSymbol)
            => typeParamSymbol.GetAttributes().Any(attr => attr.AttributeClass?.Name == CheckedAttribute.Name);

        [DebuggerStepThrough]
        private protected virtual bool ShouldAnalyze(TypeArgumentListSyntax typeArgumentList) => true;
        private protected virtual bool IsImplicitlyValid(ITypeParameterSymbol typeParameterSymbol) => false;
        private protected abstract DiagnosticDescriptor GetDiagnosticDescriptor();
    }
}
