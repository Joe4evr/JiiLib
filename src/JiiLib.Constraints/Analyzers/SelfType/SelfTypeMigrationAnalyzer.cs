//using System;
//using System.Collections.Immutable;
//using System.Linq;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.Diagnostics;

//// So this analyzer is likely dead until Roslyn exposes the project's target framework:
//// https://github.com/dotnet/roslyn/issues/4464 / https://github.com/dotnet/roslyn/issues/3798

//namespace JiiLib.Constraints.Analyzers
//{
//    [DiagnosticAnalyzer(LanguageNames.CSharp)]
//    internal sealed class SelfTypeMigrationAnalyzer : BaseUsageAnalyzer<BasicDiagnosticChoice>
//    {
//        private const string DiagnosticId = "JLC0003M";
//        private const string Title = "Migrate to the BCL-provided version of [SelfType]";
//        private const string MessageFormat = "Generic parameter '{0}' should migrate to the BCL-provided version of [SelfType]";
//        private const string Description = "";
//        private const string Category = "Migration";

//        private static readonly DiagnosticDescriptor _rule = new(
//            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description,
//            customTags: WellKnownDiagnosticTags.Unnecessary);
//        private static readonly Type _attributeType = typeof(SelfTypeAttribute);

//        /// <inheritdoc/>
//        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

//        public SelfTypeMigrationAnalyzer()
//            : base(_attributeType)
//        {
//        }


//        //private protected override bool ShouldAnalyze(TypeParameterListSyntax typeArgumentList) => tfm >= net7;
//        private protected override BasicDiagnosticChoice GetDiagnosticChoice(ITypeParameterSymbol typeParameterSymbol)
//        {
//            // don't report this diag if the S.R.CS version exists *in addition* to the package version
//            if (typeParameterSymbol.GetAttributes().Any(a =>
//                a?.AttributeClass?.ContainingNamespace?.Name == "System.Runtime.CompilerServices" // or "System.Diagnostics.CodeAnalysis"
//                && a?.AttributeClass?.Name == "SelfTypeAttribute"))
//                return BasicDiagnosticChoice.Valid;

//            return BasicDiagnosticChoice.No;
//        }

//        private protected override DiagnosticDescriptor GetDiagnosticDescriptor(BasicDiagnosticChoice _) => _rule;
//    }
//}
