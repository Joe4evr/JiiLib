using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace JiiLib.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class DefensiveCopyAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "JLA0001";
    private const string Title = "Prevent defensive struct copies";
    private const string MessageFormat = "Call to member '{0}' will execute on a defensive copy of '{1}'";
    private const string Description = "Prevent defensive struct copies.";
    private const string Category = "Correctness";

    private static readonly DiagnosticDescriptor _rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
    private static readonly Type _attributeType = typeof(NoDefensiveCopiesAttribute);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

    public sealed override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax methodDeclaration)
            return;

        if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration) is not { } methodSymbol)
            return;

        if (HasAttribute(methodSymbol) is not { } attrData)
            return;

        var thisMethodIsReadonly = methodSymbol.IsReadOnly;

        var filter = new HashSet<string>(attrData.ConstructorArguments
            .SelectMany(a => a.Values.Select(c => c.Value))
            .OfType<string>(), StringComparer.Ordinal);

        var accesses = methodDeclaration.DescendantNodes()
            .OfType<MemberAccessExpressionSyntax>()
            .ToArray();

        foreach (var access in accesses)
        {
            var parentSymbol = context.SemanticModel.GetSymbolInfo(access.Expression).Symbol;
            if (!IsPotentiallyProblematicParent(parentSymbol, thisMethodIsReadonly))
            {
                // No diagnostic needed
                return;
            }

            if (filter.Count == 0 || filter.Contains(parentSymbol.Name))
            {
                var memberSymbol = context.SemanticModel.GetSymbolInfo(access).Symbol;
                if (IsProblematicMember(memberSymbol))
                {
                    context.ReportDiagnostic(Diagnostic.Create(_rule, access.GetLocation(), memberSymbol.Name, parentSymbol.Name));
                }
            }
        }
    }

    private static AttributeData? HasAttribute(IMethodSymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().FirstOrDefault(attr =>
            attr?.AttributeClass?.ContainingNamespace?.ToDisplayString() == _attributeType.Namespace
            && attr.AttributeClass?.Name == _attributeType.Name);
    }

    private static bool IsPotentiallyProblematicParent([NotNullWhen(returnValue: true)] ISymbol? parent, bool callerIsReadonly)
    {
        return parent switch
        {
            IFieldSymbol { Type.IsValueType: true, IsReadOnly: false } when callerIsReadonly => true,
            IFieldSymbol { Type.IsValueType: true, IsReadOnly: true } => true,

            IPropertySymbol { Type.IsValueType: true, GetMethod.IsReadOnly: false } when callerIsReadonly => true,
            IPropertySymbol { Type.IsValueType: true, GetMethod.IsReadOnly: true } => true,
            IPropertySymbol { Type.IsValueType: true, RefKind: RefKind.RefReadOnly } => true,

            IMethodSymbol { ReturnType.IsValueType: true, IsReadOnly: false } when callerIsReadonly => true,
            IMethodSymbol { ReturnType.IsValueType: true, RefKind: RefKind.RefReadOnly } => true,

            ILocalSymbol     { Type.IsValueType: true, RefKind: RefKind.RefReadOnly } => true,
            IParameterSymbol { Type.IsValueType: true, RefKind: RefKind.RefReadOnly } => true,
            _ => false
        };
    }

    private static bool IsProblematicMember([NotNullWhen(returnValue: true)] ISymbol? member)
    {
        return member switch
        {
            IFieldSymbol              { IsReadOnly: false } => true,
            IPropertySymbol { GetMethod.IsReadOnly: false } => true,
            IMethodSymbol             { IsReadOnly: false } => true,
            _ => false
        };
    }
}
