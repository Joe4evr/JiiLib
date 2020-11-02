using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace JiiLib.Constraints.Tests
{
    public class CombinatoricsTests : DiagnosticVerifier
    {
        [Fact]
        public async Task VerifyDiagnosticOnIOandNI()
        {
            string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public class Test<[InterfacesOnly, NoInterfaces] T> { }
}
";

            var expected = new[]
            {
                new DiagnosticResult
                {
                    Id = "JLC0001X",
                    Message = "Attribute 'InterfacesOnlyAttribute' on 'T' in 'Test' cannot be combined with 'NoInterfacesAttribute'.",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 6, column: 24)
                    }
                }
            };
            await VerifyCSharpDiagnostic(source, expected);
        }



        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => (Activator.CreateInstance(
                assemblyName: "JiiLib.Constraints",
                typeName: "JiiLib.Constraints.Analyzers.CombinatoricAnalyzer")?.Unwrap() as DiagnosticAnalyzer)!;
    }
}
