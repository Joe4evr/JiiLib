using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace JiiLib.Constraints.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class CombinatoricAnalyzer : DiagnosticAnalyzer
    {
        private const string DiagnosticId = "JLC0001X";
        private const string Title = "Invalid combination of constraint attributes.";
        private const string MessageFormat = "Attribute '{0}' on '{1}' in '{2}' cannot be combined with '{3}'.";
        private const string Description = "Invalid use of constraint attribute.";
        private const string Category = "API Usage";

        private static readonly DiagnosticDescriptor _rule = new(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        private const string InterfacesOnly = nameof(InterfacesOnlyAttribute);
        private const string NonAbstractOnly = nameof(NonAbstractOnlyAttribute);
        private const string SelfTypeOnly = nameof(SelfTypeAttribute);
        private const string NoInterfaces = nameof(NoInterfacesAttribute);


        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeTypeParameterList, SyntaxKind.TypeParameterList);
        }

        private void AnalyzeTypeParameterList(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is TypeParameterListSyntax typeParameterList))
                return;

            foreach (var typeParamNode in typeParameterList.Parameters)
            {
                var tracker = new CombinationTracker();
                var typeParamSymbol = context.SemanticModel.GetDeclaredSymbol(typeParamNode);
                var attributes = typeParamSymbol.GetAttributes();

                foreach (var attr1 in attributes)
                {
                    foreach (var attr2 in attributes)
                    {
                        if (ReferenceEquals(attr1, attr2))
                            continue;

                        if (IsCombinationInvalid(attr1.AttributeClass, attr2.AttributeClass))
                        {
                            var location = Location.Create(context.Node.SyntaxTree, attr1.ApplicationSyntaxReference.Span);
                            var parentId = typeParameterList.Parent switch
                            {
                                MethodDeclarationSyntax method => method.Identifier.ValueText,
                                BaseTypeDeclarationSyntax type => type.Identifier.ValueText,
                                DelegateDeclarationSyntax del => del.Identifier.ValueText,
                                _ => "(unknown)" // ¯\_(ツ)_/¯
                            };

                            tracker.AddCombination(attr1.AttributeClass.Name, attr2.AttributeClass.Name,
                                location, typeParamNode.Identifier.ValueText, parentId);
                        }
                    }
                }

                foreach (var (k, v) in tracker.GetConflicts())
                {
                    var (att1, att2) = k;
                    var (loc, tname, pname) = v;
                    var diagnostic = Diagnostic.Create(_rule, loc, att1, tname, pname, att2);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            static bool IsCombinationInvalid(INamedTypeSymbol first, INamedTypeSymbol second)
            {
                if (first.Name == InterfacesOnly)
                {
                    return second.Name switch
                    {
                        NonAbstractOnly => true,
                        NoInterfaces => true,
                        _ => false
                    };
                }

                if (first.Name == NonAbstractOnly)
                {
                    return second.Name switch
                    {
                        InterfacesOnly => true,
                        _ => false
                    };
                }

                //if (first.Name == SelfTypeOnly)
                //{
                //}

                if (first.Name == NoInterfaces)
                {
                    return second.Name switch
                    {
                        InterfacesOnly => true,
                        _ => false
                    };
                }

                return false;
            }
        }
    }
}
