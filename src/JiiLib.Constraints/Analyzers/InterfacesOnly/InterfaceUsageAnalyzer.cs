using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace JiiLib.Constraints.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class InterfaceUsageAnalyzer : BaseUsageAnalyzer<InterfaceConstraintDiagnosticChoice>
    {
        private const string DiagnosticId = "JLC0001U";
        private const string Title = "The InterfacesOnly attribute cannot be combined with an incompatible constraint.";
        private const string MessageFormatVT = "The InterfacesOnly attribute on '{1}' in '{0}' cannot be combined with the ValueType constraint.";
        private const string MessageFormatBC = "The InterfacesOnly attribute on '{1}' in '{0}' cannot be combined with a Base Class constraint.";
        private const string MessageFormatCtor = "The InterfacesOnly attribute on '{1}' in '{0}' cannot be combined with the new() constraint.";
        private const string Description = "Invalid use of constraint attribute.";
        private const string Category = "API Usage";

        private static readonly DiagnosticDescriptor _ruleVT = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormatVT, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
        private static readonly DiagnosticDescriptor _ruleBC = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormatBC, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
        private static readonly DiagnosticDescriptor _ruleCtor = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormatCtor, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
        private static readonly Type _attributeType = typeof(InterfacesOnlyAttribute);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_ruleVT, _ruleBC, _ruleCtor);

        public InterfaceUsageAnalyzer()
            : base(_attributeType)
        {
        }

        private protected override InterfaceConstraintDiagnosticChoice GetDiagnosticChoice(ITypeParameterSymbol typeParameterSymbol)
            => typeParameterSymbol switch
            {
                { HasValueTypeConstraint: true } => InterfaceConstraintDiagnosticChoice.Struct,
                { ConstraintTypes: var types } when types.Any(t => t.TypeKind == TypeKind.Class)
                    => InterfaceConstraintDiagnosticChoice.BaseClass,
                { HasConstructorConstraint: true } => InterfaceConstraintDiagnosticChoice.New,
                _ => InterfaceConstraintDiagnosticChoice.Valid
            };
        private protected override DiagnosticDescriptor GetDiagnosticDescriptor(InterfaceConstraintDiagnosticChoice choice)
            => choice switch
            {
                InterfaceConstraintDiagnosticChoice.Struct => _ruleVT,
                InterfaceConstraintDiagnosticChoice.BaseClass => _ruleBC,
                InterfaceConstraintDiagnosticChoice.New => _ruleCtor,
                _ => throw new InvalidOperationException()
            };
    }
}
