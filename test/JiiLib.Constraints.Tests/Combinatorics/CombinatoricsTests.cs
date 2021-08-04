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
            const string source = @"using System;
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
                    Message = "Attribute 'InterfacesOnlyAttribute' on 'T' in 'Test' cannot be combined with 'NoInterfacesAttribute'",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 6, column: 24)
                    }
                }
            };
            await VerifyCSharpDiagnostic(source, expected);
        }

        [Fact]
        public async Task VerifyInvalidCombinationThroughEIM()
        {
            const string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public interface I
    {
        void M<[NoInterfaces] V>();
    }

    public class C : I
    {
        void I.M<T>() => M2<T>();
        
        private void M2<[InterfacesOnly] U>() { }
    }
}
";

            var expected = new[]
            {
                new DiagnosticResult()
                {
                    Id = "JLC0001X",
                    Message = "Attribute 'NoInterfacesAttribute' on 'U' in 'M' cannot be combined with 'InterfacesOnlyAttribute'",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 8, column: 17)
                    }
                }
            };
            await VerifyCSharpDiagnostic(source, expected);
        }

        [Fact]
        public async Task VerifyInvalidCombinationThroughBaseList()
        {
            const string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public interface I<[NoInterfaces] T> { }

    public class C<[InterfacesOnly] U> : I<U> { }
}
";

            var expected = new[]
            {
                new DiagnosticResult()
                {
                    Id = "JLC0001X",
                    Message = "Attribute 'InterfacesOnlyAttribute' on 'T' in 'C' cannot be combined with 'NoInterfacesAttribute'",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 8, column: 21)
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
