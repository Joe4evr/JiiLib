using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace JiiLib.Constraints.Analyzers
{
    internal abstract class BaseUsageAnalyzer<TChoice> : DiagnosticAnalyzer
        where TChoice : struct, Enum
    {
        private Type CheckedAttribute { get; }

        private protected BaseUsageAnalyzer(Type checkedAttribute)
        {
            CheckedAttribute = checkedAttribute;
        }

        public sealed override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeTypeParameterList, SyntaxKind.TypeParameterList);
        }

        private void AnalyzeTypeParameterList(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not TypeParameterListSyntax typeParameterList)
                return;

            if (!ShouldAnalyze(typeParameterList))
                return;

            foreach (var typeParamNode in typeParameterList.Parameters)
            {
                if (context.SemanticModel.GetDeclaredSymbol(typeParamNode) is not { } typeParamSymbol)
                    continue;

                var attribute = typeParamSymbol.GetAttributes().FirstOrDefault(a =>
                    a?.AttributeClass?.ContainingNamespace?.Name == "JiiLib.Constraints"
                    && a?.AttributeClass?.Name == CheckedAttribute.Name);
                if (attribute != null)
                {
                    var choice = GetDiagnosticChoice(typeParamSymbol);
                    
                    // Do not hoist! The JIT will inline this
                    // full expression to the same asm as 'a == b'
                    if (EqualityComparer<TChoice>.Default.Equals(choice, default))
                        continue;

                    var location = Location.Create(context.Node.SyntaxTree, attribute.ApplicationSyntaxReference!.Span);
                    var parentId = typeParameterList.Parent switch
                    {
                        MethodDeclarationSyntax method => method.Identifier.ValueText,
                        BaseTypeDeclarationSyntax type => type.Identifier.ValueText,
                        DelegateDeclarationSyntax del => del.Identifier.ValueText,
                        _ => "(unknown)" // ¯\_(ツ)_/¯
                    };

                    var diagnostic = Diagnostic.Create(GetDiagnosticDescriptor(choice), location, typeParamNode.Identifier.ValueText, parentId);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }


        private protected virtual bool ShouldAnalyze(TypeParameterListSyntax typeArgumentList) => true;
        private protected abstract TChoice GetDiagnosticChoice(ITypeParameterSymbol typeParameterSymbol);
        private protected abstract DiagnosticDescriptor GetDiagnosticDescriptor(TChoice choice);
    }
}
