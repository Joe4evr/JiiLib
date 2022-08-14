using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace JiiLib.Constraints.Tests
{
    public class InterfacesOnlyUsageTests : DiagnosticVerifier<InterfacesOnlyAttribute>
    {
        [Fact]
        public async Task VerifyDiagnosticOfInvalidCombinationOnTypes()
        {
            const string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public class C<[InterfacesOnly] TError>
        where TError : struct
    {
    }

    public struct S<[InterfacesOnly] TError>
        where TError : unmanaged
    {
    }

    public class X { }
    public interface IX<[InterfacesOnly] TError>
        where TError : X
    {
    }

    public delegate void D<[InterfacesOnly] TError>()
        where TError : new();

    public class C2<T, [InterfacesOnly] TError>
        where T : new()
        where TError : T
    {
    }
}
";

            var expected = new[]
            {
                new DiagnosticResult
                {
                    Id = "JLC0001U",
                    Message = "The InterfacesOnly attribute on 'TError' in 'C' cannot be combined with the ValueType constraint",
                    Severity = DiagnosticSeverity.Error,
                    Locations= new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 6, column: 21)
                    }
                },
                new DiagnosticResult
                {
                    Id = "JLC0001U",
                    Message = "The InterfacesOnly attribute on 'TError' in 'S' cannot be combined with the ValueType constraint",
                    Severity = DiagnosticSeverity.Error,
                    Locations= new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 11, column: 22)
                    }
                },
                new DiagnosticResult
                {
                    Id = "JLC0001U",
                    Message = "The InterfacesOnly attribute on 'TError' in 'IX' cannot be combined with a Base Class constraint",
                    Severity = DiagnosticSeverity.Error,
                    Locations= new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 17, column: 26)
                    }
                },
                new DiagnosticResult
                {
                    Id = "JLC0001U",
                    Message = "The InterfacesOnly attribute on 'TError' in 'D' cannot be combined with the new() constraint",
                    Severity = DiagnosticSeverity.Error,
                    Locations= new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 22, column: 29)
                    }
                },
                new DiagnosticResult
                {
                    Id = "JLC0001U",
                    Message = "Type Parameter 'TError' in 'C2' is constrained to another type parameter with an incompatible constraint",
                    Severity = DiagnosticSeverity.Error,
                    Locations= new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 25, column: 25)
                    }
                }
            };
            await VerifyCSharpDiagnostic(source, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => (Activator.CreateInstance(
                assemblyName: "JiiLib.Constraints",
                typeName: "JiiLib.Constraints.Analyzers.InterfaceUsageAnalyzer")?.Unwrap() as DiagnosticAnalyzer)!;
    }
}
