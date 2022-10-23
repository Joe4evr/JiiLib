using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace JiiLib.Analyzers.Tests
{
    public sealed class DefensiveCopyAnalyzerTests : DiagnosticVerifier<NoDefensiveCopiesAttribute>
    {
        [Fact]
        public async Task VerifyDiag()
        {
            const string source = """
                using System;
                using JiiLib.Analyzers;

                namespace N
                {
                    public struct S1
                    {
                        private int _i;
                        public void M1() => _i++;
                    }

                    public struct S2
                    {
                        private S1 _s1;

                        [NoDefensiveCopies]
                        public void M2a() => _s1.M1();

                        [NoDefensiveCopies]
                        public readonly void M2b() => _s1.M1();
                    }
                }
                """;

            var expected = new DiagnosticResult[]
            {
                new()
                {
                    Id = "JLA0001",
                    Message = "Call to member 'M1' will execute on a defensive copy of '_s1'",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 20, column: 39)
                    }
                }
            };
            await VerifyCSharpDiagnostic(source, expected);
        }

        [Fact]
        public async Task VerifyFilter()
        {
            const string source = """
                using System;
                using JiiLib.Analyzers;

                namespace N
                {
                    public struct S1
                    {
                        private int _i;
                        public void M1() => _i++;
                    }

                    public struct S2
                    {
                        private S1 _s1a;
                        private S1 _s1b;

                        [NoDefensiveCopies(nameof(_s1b))]
                        public readonly void M2a()
                        {
                            _s1a.M1();
                            _s1b.M1();
                        }
                    }
                }
                """;

            var expected = new DiagnosticResult[]
            {
                new()
                {
                    Id = "JLA0001",
                    Message = "Call to member 'M1' will execute on a defensive copy of '_s1b'",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 21, column: 13)
                    }
                }
            };
            await VerifyCSharpDiagnostic(source, expected);
        }

#if NET7_0_OR_GREATER
        [Fact]
        public async Task Check()
        {
            const string source = """
                using System;
                using System.Runtime.CompilerServices;
                using JiiLib.Analyzers;

                namespace N
                {
                    internal class Program
                    {
                        private static readonly Vec4 ReadOnlyVec = new Vec4(1, 2, 3, 4);

                        [MethodImpl(MethodImplOptions.NoInlining)]
                        [NoDefensiveCopies(nameof(ReadOnlyVec))]
                        private static ref Vec4 Foo()
                        {
                            ref Vec4 xyzw = ref ReadOnlyVec.Self;

                            return ref xyzw;
                        }
                    }

                    public struct Vec4
                    {
                        public Vec4(int x, int y, int z, int w) { }

                        [UnscopedRef]
                        public ref Vec4 Self => ref this;
                    }
                }
                """;

            var expected = new DiagnosticResult[]
            {
                new()
                {
                    Id = "JLA0001",
                    Message = "Call to member 'Self' will execute on a defensive copy of 'ReadOnlyVec'",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line: 15, column: 33)
                    }
                }
            };
            await VerifyCSharpDiagnostic(source, expected);
        }
#endif

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => (Activator.CreateInstance(
                assemblyName: "JiiLib.Analyzers",
                typeName: "JiiLib.Analyzers.DefensiveCopyAnalyzer")?.Unwrap() as DiagnosticAnalyzer)!;
    }
}
