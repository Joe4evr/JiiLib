using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace JiiLib.Constraints.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class NonAbstractUsageAnalyzer : BaseUsageAnalyzer<BasicDiagnosticChoice>
    {
        private const string DiagnosticId = "JLC0002U";
        private const string Title = "Use of the NonAbstractOnly attribute is ineffective";
        private const string MessageFormat = "Use of the NonAbstract attribute on '{0}' in '{1}' is ineffective";
        private const string Description = "Invalid use of constraint attribute.";
        private const string Category = "API Usage";

        private static readonly DiagnosticDescriptor _rule = new(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description,
            customTags: WellKnownDiagnosticTags.Unnecessary);
        private static readonly Type _attributeType = typeof(NonAbstractOnlyAttribute);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        public NonAbstractUsageAnalyzer()
            : base(_attributeType)
        {
        }

        private protected override BasicDiagnosticChoice GetDiagnosticChoice(ITypeParameterSymbol typeParameterSymbol)
            => typeParameterSymbol switch
            {
                { HasValueTypeConstraint: true } => BasicDiagnosticChoice.No,
                { HasConstructorConstraint: true } => BasicDiagnosticChoice.No,
                _ => BasicDiagnosticChoice.Valid
            };
        private protected override DiagnosticDescriptor GetDiagnosticDescriptor(BasicDiagnosticChoice _) => _rule;
    }
}
