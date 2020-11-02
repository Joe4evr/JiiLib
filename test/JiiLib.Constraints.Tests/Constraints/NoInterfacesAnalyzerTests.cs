using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace JiiLib.Constraints.Tests
{
    public sealed class NoInterfacesAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public async Task VerifyNoDiagnosticOnClassStructAndDelegate()
        {
            string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public class Test<[NoInterfaces] T> { }

    public class C { }
    public struct S { }
    public delegate void D();

    static class P
    {
        static void M()
        {
            var valid1 = new Test<C>();
            var valid2 = new Test<S>();
            var valid3 = new Test<D>();
        }
    }
}
";

            await VerifyCSharpDiagnostic(source, Array.Empty<DiagnosticResult>());
        }

        [Fact]
        public async Task VerifyDiagnosticOnInterface()
        {
            string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public class C<[NoInterfaces] T> { }

    public interface IX { }

    static class P
    {
        static void M()
        {
            var error = new C<IX>();
        }
    }
}
";

            var expected = new[]
            {
                new DiagnosticResult
                {
                    Id = "JLC0004",
                    Message = "Type argument 'IX' may not be an interface type.",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 14, column: 31)
                    }
                }
            };
            await VerifyCSharpDiagnostic(source, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => (Activator.CreateInstance(
                assemblyName: "JiiLib.Constraints",
                typeName: "JiiLib.Constraints.Analyzers.NoInterfacesConstraintAnalyzer")?.Unwrap() as DiagnosticAnalyzer)!;
    }
}
