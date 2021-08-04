using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace JiiLib.Constraints.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class NoInterfacesUsageAnalyzer : BaseUsageAnalyzer<BasicDiagnosticChoice>
    {
        private const string DiagnosticId = "JLC0004U";
        private const string Title = "Use of the NoInterfaces attribute is ineffective";
        private const string MessageFormat = "Use of the NoInterfaces attribute on '{0}' in generic method '{1}' is ineffective";
        private const string Description = "Invalid use of constraint attribute.";
        private const string Category = "API Usage";

        private static readonly DiagnosticDescriptor _rule = new(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description,
            customTags: WellKnownDiagnosticTags.Unnecessary);

        private static readonly Type _attributeType = typeof(NoInterfacesAttribute);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        public NoInterfacesUsageAnalyzer()
            : base(_attributeType)
        {
        }

        private protected override bool ShouldAnalyze(TypeParameterListSyntax typeArgumentList)
            => typeArgumentList.Parent.IsKind(SyntaxKind.MethodDeclaration);

        private protected override BasicDiagnosticChoice GetDiagnosticChoice(ITypeParameterSymbol typeParameterSymbol)
        {
            return typeParameterSymbol switch
            {
                { HasValueTypeConstraint: true } => BasicDiagnosticChoice.No,
                { HasConstructorConstraint: true } => BasicDiagnosticChoice.No,
                { ConstraintTypes: { IsEmpty: false } types } => GetChoiceFromConstraints(types, this),
                _ => BasicDiagnosticChoice.Valid
            };


            static BasicDiagnosticChoice GetChoiceFromConstraints(
                ImmutableArray<ITypeSymbol> types,
                NoInterfacesUsageAnalyzer @this)
            {
                foreach (var t in types)
                {
                    if (t.TypeKind == TypeKind.Class)
                        return BasicDiagnosticChoice.No;

                    if (t is ITypeParameterSymbol typeParam
                        && @this.GetDiagnosticChoice(typeParam) != BasicDiagnosticChoice.Valid)
                    {
                        return BasicDiagnosticChoice.No;
                    }
                }

                return BasicDiagnosticChoice.Valid;
            }
        }

        private protected override DiagnosticDescriptor GetDiagnosticDescriptor(BasicDiagnosticChoice choice) => _rule;
    }
}
