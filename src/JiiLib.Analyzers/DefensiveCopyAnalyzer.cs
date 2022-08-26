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
    private const string AliassedMessageFormat = "Call to member '{0}' will execute on a defensive copy of '{1}' via the alias '{2}'";
    private const string Description = "Prevent defensive struct copies.";
    private const string Category = "Correctness";

    private static readonly DiagnosticDescriptor _rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
    private static readonly DiagnosticDescriptor _aliassedRule = new(DiagnosticId, Title, AliassedMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
    private static readonly Type _attributeType = typeof(NoDefensiveCopiesAttribute);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule, _aliassedRule);

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

        var aliasTracker = new VariableAliasTracker(attrData.ConstructorArguments
            .SelectMany(a => a.Values.Select(c => c.Value))
            .OfType<string>());

        var interests = methodDeclaration.DescendantNodes()
            .Where(n => n.IsKind(SyntaxKind.LocalDeclarationStatement)
                || n.IsKind(SyntaxKind.SimpleAssignmentExpression)
                || n.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            .ToArray();

        foreach (var interest in interests)
        {
            // Aliassing: new ref-local declarations
            // ref var a = ref b;
            if (interest is LocalDeclarationStatementSyntax { Declaration: { Type: RefTypeSyntax, Variables: var vars } })
            {
                foreach (var item in vars)
                {
                    if (item.Initializer is { Value: RefExpressionSyntax { Expression: IdentifierNameSyntax { Identifier: var refValue } } })
                    {
                        aliasTracker.SetAlias(item.Identifier.ValueText, refValue.ValueText);
                    }
                }
            }
            // Aliassing: ref-local reassignment
            // a = ref c;
            else if (interest is AssignmentExpressionSyntax { Left: IdentifierNameSyntax id, Right: RefExpressionSyntax { Expression: IdentifierNameSyntax { Identifier: var refValue } } })
            {
                aliasTracker.SetAlias(id.Identifier.ValueText, refValue.ValueText);
            }
            // Member access: anything like 'a.b', 'a.b()', 'a().b', 'a().b()'
            else if (interest is MemberAccessExpressionSyntax access)
            {
                var lhsSymbol = context.SemanticModel.GetSymbolInfo(access.Expression, context.CancellationToken).Symbol;
                if (!IsPotentiallyProblematicLhs(lhsSymbol, thisMethodIsReadonly))
                {
                    // No diagnostic needed
                    return;
                }

                var foundName = default(string);
                if (aliasTracker.FilterCount == 0 || aliasTracker.Contains(lhsSymbol.Name, out foundName))
                {
                    var memberSymbol = context.SemanticModel.GetSymbolInfo(access, context.CancellationToken).Symbol;
                    if (IsProblematicMember(memberSymbol, isWrite: access.IsWrittenTo(memberSymbol)))
                    {
                        if (foundName is null || lhsSymbol.Name == foundName)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(_rule, access.GetLocation(), memberSymbol.Name, lhsSymbol.Name));
                        }
                        else
                        {
                            context.ReportDiagnostic(Diagnostic.Create(_aliassedRule, access.GetLocation(), memberSymbol.Name, lhsSymbol.Name, foundName));
                        }
                    }
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

    private static bool IsPotentiallyProblematicLhs([NotNullWhen(returnValue: true)] ISymbol? parent, bool callerIsReadonly)
    {
        return parent switch
        {
            IFieldSymbol { Type.IsValueType: true, IsStatic: false, IsReadOnly: false } when callerIsReadonly => true,
            IFieldSymbol { Type.IsValueType: true, IsStatic: false, IsReadOnly: true } => true,

            IPropertySymbol { Type.IsValueType: true, IsStatic: false, GetMethod.IsReadOnly: false } when callerIsReadonly => true,
            IPropertySymbol { Type.IsValueType: true, IsStatic: false, GetMethod.IsReadOnly: true } => true,
            IPropertySymbol { Type.IsValueType: true, IsStatic: false, RefKind: RefKind.RefReadOnly } => true,

            IMethodSymbol { ReturnType.IsValueType: true, IsStatic: false, IsReadOnly: false } when callerIsReadonly => true,
            IMethodSymbol { ReturnType.IsValueType: true, IsStatic: false, RefKind: RefKind.RefReadOnly } => true,

            ILocalSymbol     { Type.IsValueType: true, RefKind: RefKind.RefReadOnly } => true,
            IParameterSymbol { Type.IsValueType: true, RefKind: RefKind.RefReadOnly } => true,
            _ => false
        };
    }

    // If the lhs of the member access was potentially problematic,
    // this method checks if the member access *actually* causes a shadow copy.
    private static bool IsProblematicMember([NotNullWhen(returnValue: true)] ISymbol? member, bool isWrite)
    {
        return member switch
        {
            // A non-readonly field being written to can result in a shadow copy.
            // (A field read, even non-readonly, does not shadow copy.)
            IFieldSymbol { IsReadOnly: false, IsStatic: false } when isWrite => true,

            // Looks weird, but it is possible and legal to declare a property with
            // a 'readonly set' method, and a non-readonly setter can result in a shadow copy if used.
            IPropertySymbol { SetMethod.IsReadOnly: false, IsStatic: false } when isWrite => true,
            // A non-readonly ref-returning property being written to can result in a shadow copy.
            IPropertySymbol { GetMethod.IsReadOnly: false, IsStatic: false, ReturnsByRef: true } when isWrite => true,
            // Any non-readonly property getter can result in a shadow copy.
            IPropertySymbol { GetMethod.IsReadOnly: false, IsStatic: false } => true,

            // A non-readonly method's ref-return being written to can result in a shadow copy.
            // (Reading from the ref-return should be okay, since it's like a field access.)
            IMethodSymbol { IsReadOnly: false, IsStatic: false, ReturnsByRef: true } when isWrite => true,
            // Any non-readonly method invoke can result in a shadow copy.
            IMethodSymbol { IsReadOnly: false, IsStatic: false } => true,

            //IEventSymbol { },

            // Everything else won't result in a shadow copy.
            _ => false
        };
    }
}
