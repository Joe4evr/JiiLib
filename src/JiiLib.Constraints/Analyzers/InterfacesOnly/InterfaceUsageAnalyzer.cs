﻿using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace JiiLib.Constraints.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class InterfaceUsageAnalyzer : BaseUsageAnalyzer<InterfacesOnlyAttribute, InterfaceConstraintDiagnosticChoice>
    {
        private const string DiagnosticId = "JLC0001U";
        private const string Title = "The InterfacesOnly attribute cannot be combined with an incompatible constraint";
        private const string MessageFormatVT = "The InterfacesOnly attribute on '{0}' in '{1}' cannot be combined with the ValueType constraint";
        private const string MessageFormatBC = "The InterfacesOnly attribute on '{0}' in '{1}' cannot be combined with a Base Class constraint";
        private const string MessageFormatCtor = "The InterfacesOnly attribute on '{0}' in '{1}' cannot be combined with the new() constraint";
        private const string MessageFormatTParam = "Type Parameter '{0}' in '{1}' is constrained to another type parameter with an incompatible constraint";
        private const string Description = "Invalid use of constraint attribute.";
        private const string Category = "API Usage";

        private static readonly DiagnosticDescriptor _ruleVT = new(
            DiagnosticId, Title, MessageFormatVT, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
        private static readonly DiagnosticDescriptor _ruleCtor = new(
            DiagnosticId, Title, MessageFormatCtor, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
        private static readonly DiagnosticDescriptor _ruleBC = new(
            DiagnosticId, Title, MessageFormatBC, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
        private static readonly DiagnosticDescriptor _ruleTParam = new(
            DiagnosticId, Title, MessageFormatTParam, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_ruleVT, _ruleCtor, _ruleBC, _ruleTParam);

        private protected override InterfaceConstraintDiagnosticChoice GetDiagnosticChoice(ITypeParameterSymbol typeParameterSymbol)
        {
            return typeParameterSymbol switch
            {
                { HasValueTypeConstraint: true } => InterfaceConstraintDiagnosticChoice.Struct,
                { HasConstructorConstraint: true } => InterfaceConstraintDiagnosticChoice.New,
                { ConstraintTypes: { IsEmpty: false } types } => GetChoiceFromConstraints(types, this),
                _ => InterfaceConstraintDiagnosticChoice.Valid
            };

            static InterfaceConstraintDiagnosticChoice GetChoiceFromConstraints(
                ImmutableArray<ITypeSymbol> types,
                InterfaceUsageAnalyzer @this)
            {
                foreach (var t in types)
                {
                    if (t.TypeKind == TypeKind.Class)
                        return InterfaceConstraintDiagnosticChoice.BaseClass;

                    if (t is ITypeParameterSymbol typeParam
                        && @this.GetDiagnosticChoice(typeParam) != InterfaceConstraintDiagnosticChoice.Valid)
                    {
                        return InterfaceConstraintDiagnosticChoice.TypeParam;
                    }
                }

                return InterfaceConstraintDiagnosticChoice.Valid;
            }
        }

        private protected override DiagnosticDescriptor GetDiagnosticDescriptor(InterfaceConstraintDiagnosticChoice choice)
        {
            return choice switch
            {
                InterfaceConstraintDiagnosticChoice.Struct => _ruleVT,
                InterfaceConstraintDiagnosticChoice.New => _ruleCtor,
                InterfaceConstraintDiagnosticChoice.BaseClass => _ruleBC,
                InterfaceConstraintDiagnosticChoice.TypeParam => _ruleTParam,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
