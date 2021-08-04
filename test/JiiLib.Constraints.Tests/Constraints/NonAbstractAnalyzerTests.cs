using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace JiiLib.Constraints.Tests
{
    public class NonAbstractAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public async Task VerifyDiagnosticsOnInterfaceAndAbstractClass()
        {
            const string source = @"using System;
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
                    Message = "Type argument 'X' must be a non-abstract type",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 15, column: 30)
                    }
                },
                new DiagnosticResult
                {
                    Id = "JLC0002",
                    Message = "Type argument 'IX' must be a non-abstract type",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 16, column: 30)
                    }
                }
            };
            await VerifyCSharpDiagnostic(source, expected);
        }

        [Fact]
        public async Task VerifyNoDiagnosticOnConcreteType()
        {
            const string source = @"using System;
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

            await VerifyCSharpDiagnostic(source, Array.Empty<DiagnosticResult>());
        }

        [Fact]
        public async Task VerifyDiagnosticOnMethodCall()
        {
            const string source = @"using System;
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
                    Message = "Type argument 'X' must be a non-abstract type",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 17, column: 33)
                    }
                }
            };
            await VerifyCSharpDiagnostic(source, expected);
        }

        [Fact]
        public async Task VerifyNoDiagnosticOnSameAttributeTypeParam()
        {
            const string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public class C
    {
        public NoAbstract<T> M<[NonAbstractOnly] T>()
            where T : IFormattable
        {
            return new NoAbstract<T>();
        }
    }

    public class NoAbstract<[NonAbstractOnly] T> { }
}
";

            await VerifyCSharpDiagnostic(source, Array.Empty<DiagnosticResult>());
        }

        [Fact]
        public async Task VerifyNoDiagnosticOnEIM()
        {
            const string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public interface I
    {
        void M<[NonAbstractOnly] T>();
    }

    public class C : I
    {
        void I.M<T>() { }
    }
}
";

            await VerifyCSharpDiagnostic(source, Array.Empty<DiagnosticResult>());
        }

        [Fact]
        public async Task VerifyDiagnosticOnIIM()
        {
            const string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public interface I
    {
        void M<[NonAbstractOnly] T>();
    }

    public class C : I
    {
        public void M<T>() { }
    }
}
";

            var expected = new[]
            {
                new DiagnosticResult()
                {
                    Id = "JLC0002",
                    Message = "Type argument 'T' must be a non-abstract type",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 13, column: 23)
                    }
                }
            };
            await VerifyCSharpDiagnostic(source, expected);
        }

        [Fact]
        public async Task VerifyDiagnosticOnOverride()
        {
            const string source = @"using System;
using JiiLib.Constraints;

namespace N
{
    public abstract class Base
    {
        public abstract void M<[NonAbstractOnly] T>();
    }

    public class C : Base
    {
        public override void M<T>() { }
    }
}
";

            var expected = new[]
            {
                new DiagnosticResult()
                {
                    Id = "JLC0002",
                    Message = "Type argument 'T' must be a non-abstract type",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 13, column: 32)
                    }
                }
            };
            await VerifyCSharpDiagnostic(source, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => (Activator.CreateInstance(
                assemblyName: "JiiLib.Constraints",
                typeName: "JiiLib.Constraints.Analyzers.NonAbstractConstraintAnalyzer")?.Unwrap() as DiagnosticAnalyzer)!;
    }
}
