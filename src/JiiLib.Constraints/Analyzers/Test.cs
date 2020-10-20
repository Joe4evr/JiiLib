//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.Diagnostics;

//namespace JiiLib.Constraints.Analyzers
//{
//    [DiagnosticAnalyzer(LanguageNames.CSharp)]
//    internal sealed class Test : DiagnosticSuppressor
//    {
//        private const string DiagnosticId = "";
//        private const string SurpressedId = "";
//        private const string MessageFormat = "";

//        private static readonly SuppressionDescriptor _rule = new SuppressionDescriptor(
//            DiagnosticId, SurpressedId, MessageFormat);

//        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(_rule);

//        public override void ReportSuppressions(SuppressionAnalysisContext context)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
