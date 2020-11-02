using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace JiiLib.Constraints.Tests
{
    public class NonAbstractViralityTests : DiagnosticVerifier
    {
        [Fact]
        public async Task VerifyDiagnosticOnAbsence()
        {
            var source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public class C<TError>
    {
        public static void M()
            => S.X<TError>();
    }

    public static class S
    {
        public static void X<[NonAbstractOnly] T>() { }
    }
}
";

            var expected = new[]
            {
                new DiagnosticResult
                {
                    Id = "JLC0002V",
                    Message = "Type parameter 'TError' in 'C' must be annotated with '[NonAbstractOnly]' to use as a type argument for 'X'.",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new []
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 9, column: 20)
                    }
                }
            };
            await VerifyCSharpDiagnostic(source, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => (Activator.CreateInstance(
                assemblyName: "JiiLib.Constraints",
                typeName: "JiiLib.Constraints.Analyzers.NonAbstractViralityAnalyzer")?.Unwrap() as DiagnosticAnalyzer)!;
    }
}
