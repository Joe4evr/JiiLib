using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace JiiLib.Constraints.Tests
{
    public class SelfTypeUsageTests : DiagnosticVerifier
    {
        [Fact]
        public async Task VerifyDiagnosticOnMethodDeclaration()
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

            var expected = new[]
            {
                new DiagnosticResult
                {
                    Id = "JLC0003U",
                    Message = "Use of the SelfType attribute on 'T' in generic method 'ValidButIneffective' is ineffective.",
                    Severity = DiagnosticSeverity.Info,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 8, column: 42)
                    }
                }
            };
            await VerifyCSharpDiagnostic(source, expected);
        }

        [Fact]
        public async Task VerifyNoDiagnosticOnTypeDeclarations()
        {
            var source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public interface IX<[SelfType] TSelf> { }

    public class C : IX<C> { }
    public struct S : IX<S> { }
}
";

            await VerifyCSharpDiagnostic(source, Array.Empty<DiagnosticResult>());
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => (Activator.CreateInstance(
                assemblyName: "JiiLib.Constraints",
                typeName: "JiiLib.Constraints.Analyzers.SelfTypeUsageAnalyzer")?.Unwrap() as DiagnosticAnalyzer)!;


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

        //        [Fact]
        //        public void VerifyNoDiagnosticOnConcreteType()
        //        {
        //            string source = @"using System;
        //using JiiLib.Constraints;

        //namespace N
        //{
        //    public class C<[NonAbstractOnly] T> { }

        //    public class X { }
        //    public struct S { }

        //    static class P
        //    {
        //        static void M()
        //        {
        //            var valid1 = new C<X>();
        //            var valid2 = new C<S>();
        //        }
        //    }
        //}
        //";

        //            VerifyCSharpDiagnostic(source, Array.Empty<DiagnosticResult>());
        //        }
    }
}
