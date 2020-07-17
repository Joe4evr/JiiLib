using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace JiiLib.Constraints.Tests
{
    public class NonAbstractAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void VerifyDiagnosticsOnInterfaceAndAbstractClass()
        {
            string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public class C<[NonAbstractOnly] T> { }

    public abstract class X { }
    public interface IX { }

    static class P
    {
        static void M()
        {
            var err1 = new C<X>();
            var err2 = new C<IX>();
        }
    }
}
";

            var expected = new[]
            {
                new DiagnosticResult
                {
                    Id = "JLC0002",
                    Message = "Type argument 'X' must be a non-abstract type.",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 15, column: 30)
                    }
                },
                new DiagnosticResult
                {
                    Id = "JLC0002",
                    Message = "Type argument 'IX' must be a non-abstract type.",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 16, column: 30)
                    }
                }
            };
            VerifyCSharpDiagnostic(source, expected);
        }

        [Fact]
        public void VerifyNoDiagnosticOnConcreteType()
        {
            string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public class C<[NonAbstractOnly] T> { }

    public class X { }
    public struct S { }

    static class P
    {
        static void M()
        {
            var valid1 = new C<X>();
            var valid2 = new C<S>();
        }
    }
}
";

            VerifyCSharpDiagnostic(source, Array.Empty<DiagnosticResult>());
        }

        [Fact]
        public void VerifyDiagnosticOnMethodCall()
        {
            var source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public class C
    {
        public C M<[NonAbstractOnly] T>() => this;
    }

    public abstract class X { }

    static class P
    {
        static void M()
        {
            var err = new C().M<X>();
        }
    }
}
";

            var expected = new[]
            {
                new DiagnosticResult
                {
                    Id = "JLC0002",
                    Message = "Type argument 'X' must be a non-abstract type.",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 17, column: 33)
                    }
                }
            };
            VerifyCSharpDiagnostic(source, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => (Activator.CreateInstance(
                assemblyName: "JiiLib.Constraints",
                typeName: "JiiLib.Constraints.Analyzers.NonAbstractConstraintAnalyzer")?.Unwrap() as DiagnosticAnalyzer)!;
    }
}
