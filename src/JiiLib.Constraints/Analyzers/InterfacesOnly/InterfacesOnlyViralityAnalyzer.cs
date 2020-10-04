﻿using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace JiiLib.Constraints.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class InterfacesOnlyViralityAnalyzer : BaseViralityAnalyzer
    {
        private const string DiagnosticId = "JLC0001V";
        private const string Title = "Type parameter must be annotated.";
        private const string MessageFormat = "Type parameter '{0}' in '{1}' must be annotated with '[InterfacesOnly]' to use as a type argument for '{2}'.";
        private const string Description = "Missing constraint attribute.";
        private const string Category = "API Usage";

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
        private static readonly Type _attributeType = typeof(InterfacesOnlyAttribute);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        public InterfacesOnlyViralityAnalyzer()
            : base(_attributeType)
        {
        }


        private protected override DiagnosticDescriptor GetDiagnosticDescriptor() => _rule;
    }
}
