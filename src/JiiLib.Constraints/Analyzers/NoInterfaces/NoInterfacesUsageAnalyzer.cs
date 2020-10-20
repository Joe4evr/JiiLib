//using System;
//using System.Collections.Immutable;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.Diagnostics;

//namespace JiiLib.Constraints.Analyzers
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    [DiagnosticAnalyzer(LanguageNames.CSharp)]
//    internal sealed class NoInterfacesUsageAnalyzer : BaseUsageAnalyzer<BasicDiagnosticChoice>
//    {
//        private const string DiagnosticId = "JLC0004U";
//        private const string Title = "Use of the NoInterfaces attribute cannot be combined with InterfacesOnly.";
//        private const string MessageFormat = "Use of the SelfType attribute on '{0}' in generic method '{1}' is ineffective.";
//        private const string Description = "Invalid use of constraint attribute.";
//        private const string Category = "API Usage";

//        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(
//            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
//        private static readonly Type _attributeType = typeof(NoInterfacesAttribute);

//        /// <inheritdoc/>
//        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

//        public NoInterfacesUsageAnalyzer()
//            : base(_attributeType)
//        {
//        }

//        private protected override bool ShouldAnalyze(TypeParameterListSyntax typeArgumentList)
//            => typeArgumentList.Parent.IsKind(SyntaxKind.MethodDeclaration);
//        private protected override BasicDiagnosticChoice GetDiagnosticChoice(ITypeParameterSymbol typeParameterSymbol) => BasicDiagnosticChoice.No;
//        private protected override DiagnosticDescriptor GetDiagnosticDescriptor(BasicDiagnosticChoice _) => _rule;
//    }
//}
