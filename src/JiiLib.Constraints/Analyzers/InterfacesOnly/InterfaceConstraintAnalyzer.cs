﻿using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace JiiLib.Constraints.Analyzers
{
    /// <summary>
    /// 
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class InterfaceConstraintAnalyzer : BaseConstraintAnalyzer
    {
        private const string DiagnosticId = "JLC0001";
        private const string Title = "Type argument must be an interface type.";
        private const string MessageFormat = "Type argument '{0}' must be an interface type.";
        private const string Description = "Passing an invalid type argument.";
        private const string Category = "API Usage";

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
        private static readonly Type _attributeType = typeof(InterfacesOnlyAttribute);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        public InterfaceConstraintAnalyzer()
            : base(_attributeType)
        {
        }

        private protected override bool CompliesWithConstraint(ITypeSymbol typeSymbol)
            => typeSymbol.TypeKind == TypeKind.Interface;
        private protected override DiagnosticDescriptor GetDiagnosticDescriptor() => _rule;
    }
}
