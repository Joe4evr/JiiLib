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
            const string source = @"using System;
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
                    Message = "Type parameter 'TError' in 'C' must be annotated with '[NonAbstractOnly]' to use as a type argument for 'X'",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new []
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 9, column: 20)
                    }
                }
            };
            await VerifyCSharpDiagnostic(source, expected);
        }

        [Fact]
        public async Task VerifyStructConstrainedTypeParamIsImplicitlyValid()
        {
            const string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public class C<[NonAbstractOnly] T>
    {
        public int I => 28;
    }

    static class P
    {
        static int M<U>()
            where U : struct
        {
            return new C<U>().I;
        }
    }
}
";

            await VerifyCSharpDiagnostic(source, Array.Empty<DiagnosticResult>());
        }

        [Fact]
        public async Task VerifyConstructorConstrainedTypeParamIsImplicitlyValid()
        {
            const string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public class C<[NonAbstractOnly] T>
    {
        public int I => 28;
    }

    static class P
    {
        static int M<U>()
            where U : new()
        {
            return new C<U>().I;
        }
    }
}
";

            await VerifyCSharpDiagnostic(source, Array.Empty<DiagnosticResult>());
        }

        [Fact]
        public async Task VerifyOtherConstraintsAreNotImplicitlyValid()
        {
            const string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public class C<[NonAbstractOnly] T>
    {
        public int I => 28;
    }

    static class P
    {
        static int M1<TError>()
            where TError : class
        {
            return new C<TError>().I;
        }

        static int M2<TError>()
            where TError : IFormattable
        {
            return new C<TError>().I;
        }

        static int M3<TError>()
            where TError : Attribute
        {
            return new C<TError>().I;
        }

        static int M4<TError, U>()
            where TError : U
        {
            return new C<TError>().I;
        }
    }
}
";

            var expected = new[]
            {
                new DiagnosticResult()
                {
                    Id = "JLC0002V",
                    Message = "Type parameter 'TError' in 'M1' must be annotated with '[NonAbstractOnly]' to use as a type argument for 'C'",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new []
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 16, column: 26)
                    }
                },
                new DiagnosticResult()
                {
                    Id = "JLC0002V",
                    Message = "Type parameter 'TError' in 'M2' must be annotated with '[NonAbstractOnly]' to use as a type argument for 'C'",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new []
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 22, column: 26)
                    }
                },
                new DiagnosticResult()
                {
                    Id = "JLC0002V",
                    Message = "Type parameter 'TError' in 'M3' must be annotated with '[NonAbstractOnly]' to use as a type argument for 'C'",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new []
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 28, column: 26)
                    }
                },
                new DiagnosticResult()
                {
                    Id = "JLC0002V",
                    Message = "Type parameter 'TError' in 'M4' must be annotated with '[NonAbstractOnly]' to use as a type argument for 'C'",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new []
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 34, column: 26)
                    }
                },
            };
            await VerifyCSharpDiagnostic(source, expected);
        }

        //[Fact]
        //public async Task VerifyViralityThroughEIM()
        //{
        //    const string source = @"";

        //    var expected = new[]
        //    {
        //        new DiagnosticResult()
        //        {

        //        }
        //    };
        //    await VerifyCSharpDiagnostic(source, expected);
        //}

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => (Activator.CreateInstance(
                assemblyName: "JiiLib.Constraints",
                typeName: "JiiLib.Constraints.Analyzers.NonAbstractViralityAnalyzer")?.Unwrap() as DiagnosticAnalyzer)!;
    }
}
