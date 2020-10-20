using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace JiiLib.Constraints.Analyzers
{
    /// <summary>
    /// 
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class NoInterfacesViralityAnalyzer : BaseViralityAnalyzer
    {
        private const string DiagnosticId = "JLC0004V";
        private const string Title = "Type parameter must be annotated.";
        private const string MessageFormat = "Type parameter '{0}' in '{1}' must be annotated with '[NoInterfaces]' to use as a type argument for '{2}'.";
        private const string Description = "Missing constraint attribute.";
        private const string Category = "API Usage";

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
        private static readonly Type _attributeType = typeof(NoInterfacesAttribute);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        public NoInterfacesViralityAnalyzer()
            : base(_attributeType)
        {
        }

        private protected override DiagnosticDescriptor GetDiagnosticDescriptor() => _rule;
    }
}
