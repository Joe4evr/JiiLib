using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace JiiLib.Constraints.Tests
{
    public sealed class NoInterfacesUsageTests : DiagnosticVerifier<NoInterfacesAttribute>
    {
        [Fact]
        public async Task VerifyDiagnosticOfInvalidCombinationOnTypes()
        {
            const string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public class C
    {
        public void M1<[NoInterfaces] TError>() where TError : struct { }
        public void M2<[NoInterfaces] TError>() where TError : unmanaged { }
        public void M3<[NoInterfaces] TError>() where TError : X { }
        public void M4<[NoInterfaces] TError>() where TError : new() { }
        public void M5<T, [NoInterfaces] TError>()
            where T : new()
            where TError : T { }
    }

    public class X { }
}
";

            var expected = new[]
            {
                new DiagnosticResult
                {
                    Id = "JLC0004U",
                    Message = "Use of the NoInterfaces attribute on 'TError' in generic method 'M1' is ineffective",
                    Severity = DiagnosticSeverity.Info,
                    Locations= new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 8, column: 25)
                    }
                },
                new DiagnosticResult
                {
                    Id = "JLC0004U",
                    Message = "Use of the NoInterfaces attribute on 'TError' in generic method 'M2' is ineffective",
                    Severity = DiagnosticSeverity.Info,
                    Locations= new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 9, column: 25)
                    }
                },
                new DiagnosticResult
                {
                    Id = "JLC0004U",
                    Message = "Use of the NoInterfaces attribute on 'TError' in generic method 'M3' is ineffective",
                    Severity = DiagnosticSeverity.Info,
                    Locations= new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 10, column: 25)
                    }
                },
                new DiagnosticResult
                {
                    Id = "JLC0004U",
                    Message = "Use of the NoInterfaces attribute on 'TError' in generic method 'M4' is ineffective",
                    Severity = DiagnosticSeverity.Info,
                    Locations= new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 11, column: 25)
                    }
                },
                new DiagnosticResult
                {
                    Id = "JLC0004U",
                    Message = "Use of the NoInterfaces attribute on 'TError' in generic method 'M5' is ineffective",
                    Severity = DiagnosticSeverity.Info,
                    Locations= new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 12, column: 28)
                    }
                }
            };
            await VerifyCSharpDiagnostic(source, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => (Activator.CreateInstance(
                assemblyName: "JiiLib.Constraints",
                typeName: "JiiLib.Constraints.Analyzers.NoInterfacesUsageAnalyzer")?.Unwrap() as DiagnosticAnalyzer)!;
    }
}
