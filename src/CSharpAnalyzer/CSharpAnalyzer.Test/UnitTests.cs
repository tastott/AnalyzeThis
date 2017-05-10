using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using CSharpAnalyzer;

namespace CSharpAnalyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void EmptyFile()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void ReadOnlyPropertyNotSetInConstructor()
        {
            var test = @"
    namespace MyNamespace
    {
        class MyClass
        {
            private readonly int blah;
            private readonly int foo;

            public MyClass()
            {
                this.foo = 4;
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "CSharpAnalyzer",
                Message = $"Readonly field(s) not assigned in constructor: blah.",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    namespace MyNamespace
    {
        class MyClass
        {
            private readonly int blah;
            private readonly int foo;

            public MyClass()
            {
                this.foo = 4;
                this.blah = 0;
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpAnalyzerAnalyzer();
        }
    }
}