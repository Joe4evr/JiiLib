using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace JiiLib.Constraints.Tests
{
    public class InterfacesOnlyAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void VerifyDiagnosticsOnClassStructAndDelegate()
        {
            string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public class Test<[InterfacesOnly] T> { }

    public class C { }
    public struct S { }
    public delegate void D();

    static class P
    {
        static void M()
        {
            var err1 = new Test<C>();
            var err2 = new Test<S>();
            var err3 = new Test<D>();
        }
    }
}
";

            var expected = new[]
            {
                new DiagnosticResult
                {
                    Id = "JLC0001",
                    Message = "Type argument 'C' must be an interface type.",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 16, column: 33)
                    }
                },
                new DiagnosticResult
                {
                    Id = "JLC0001",
                    Message = "Type argument 'S' must be an interface type.",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 17, column: 33)
                    }
                },
                new DiagnosticResult
                {
                    Id = "JLC0001",
                    Message = "Type argument 'D' must be an interface type.",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 18, column: 33)
                    }
                }
            };
            VerifyCSharpDiagnostic(source, expected);
        }

        [Fact]
        public void VerifyNoDiagnosticOnInterface()
        {
            string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public class C<[InterfacesOnly] T> { }

    public interface IX { }

    static class P
    {
        static void M()
        {
            var valid = new C<IX>();
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
        public C M<[InterfacesOnly] T>() => this;
    }

    public class X { }

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
                    Id = "JLC0001",
                    Message = "Type argument 'X' must be an interface type.",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 17, column: 33)
                    }
                }
            };
            VerifyCSharpDiagnostic(source, expected);
        }

        [Fact]
        public void VerifyDiagnosticOnImplicitUse()
        {
            var source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public class C
    {
        public C M<[InterfacesOnly] T>(T param) => this;
    }

    public class X { }

    static class P
    {
        static void M()
        {
            var err = new C().M(new X());
        }
    }
}
";

            VerifyCSharpDiagnostic(source);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => (Activator.CreateInstance(
                assemblyName: "JiiLib.Constraints",
                typeName: "JiiLib.Constraints.Analyzers.InterfaceConstraintAnalyzer")?.Unwrap() as DiagnosticAnalyzer)!;
    }
}
