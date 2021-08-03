using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace JiiLib.Constraints.Tests
{
    public class NonAbstractUsageTests : DiagnosticVerifier
    {
        [Fact]
        public async Task VerifyDiagnosticOnIneffectiveCombinations()
        {
            const string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public class C<[NonAbstractOnly] T>
        where T : struct
    {
    }

    public struct S<[NonAbstractOnly] T>
        where T : new()
    {
    }
}
";

            var expected = new[]
            {
                new DiagnosticResult
                {
                    Id = "JLC0002U",
                    Message = "Use of the NonAbstract attribute on 'T' in 'C' is ineffective",
                    Severity = DiagnosticSeverity.Info,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 6, column: 21)
                    }
                },
                new DiagnosticResult
                {
                    Id = "JLC0002U",
                    Message = "Use of the NonAbstract attribute on 'T' in 'S' is ineffective",
                    Severity = DiagnosticSeverity.Info,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 11, column: 22)
                    }
                }
            };
            await VerifyCSharpDiagnostic(source, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => (Activator.CreateInstance(
                assemblyName: "JiiLib.Constraints",
                typeName: "JiiLib.Constraints.Analyzers.NonAbstractUsageAnalyzer")?.Unwrap() as DiagnosticAnalyzer)!;
    }
}
