using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace JiiLib.Constraints.Tests
{
    public class SelfTypeAnalyzerTests : DiagnosticVerifier
    {
        //        [Fact]
        //        public void VerifyDiagnosticsOnInterfaceAndAbstractClass()
        //        {
        //            string source = @"using System;
        //using JiiLib.Constraints;

        //namespace N
        //{
        //    public class C<[NonAbstractOnly] T> { }

        //    public abstract class X { }
        //    public interface IX { }

        //    static class P
        //    {
        //        static void M()
        //        {
        //            var err1 = new C<X>();
        //            var err2 = new C<IX>();
        //        }
        //    }
        //}
        //";

        //            var expected = new[]
        //            {
        //                new DiagnosticResult
        //                {
        //                    Id = "JLC0002",
        //                    Message = "Type argument 'X' must be a non-abstract type.",
        //                    Severity = DiagnosticSeverity.Error,
        //                    Locations = new[]
        //                    {
        //                        new DiagnosticResultLocation("Test0.cs", line: 15, column: 29)
        //                    }
        //                },
        //                new DiagnosticResult
        //                {
        //                    Id = "JLC0002",
        //                    Message = "Type argument 'IX' must be a non-abstract type.",
        //                    Severity = DiagnosticSeverity.Error,
        //                    Locations = new[]
        //                    {
        //                        new DiagnosticResultLocation("Test0.cs", line: 16, column: 29)
        //                    }
        //                }
        //            };
        //            VerifyCSharpDiagnostic(source, expected);
        //        }

        [Fact]
        public void VerifyDiagnosticOnMismatchType()
        {
            string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public interface IX<[SelfType] TSelf> { }

    public class Error : IX<string> { }
}
";
            var expected = new[]
            {
                new DiagnosticResult
                {
                    Id = "JLC0003",
                    Message = "Type argument 'String' must be the implementing type.",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 8, column: 29)
                    }
                }
            };
            VerifyCSharpDiagnostic(source, expected);
        }

        [Fact]
        public void VerifyNoDiagnosticOnMethodDeclaration()
        {
            var source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public class C
    {
        public void ValidButIneffective<[SelfType] T>() { }
    }
}
";

            
            VerifyCSharpDiagnostic(source, Array.Empty<DiagnosticResult>());
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => (Activator.CreateInstance(
                assemblyName: "JiiLib.Constraints",
                typeName: "JiiLib.Constraints.Analyzers.SelfTypeConstraintAnalyzer")?.Unwrap() as DiagnosticAnalyzer)!;
    }
}
