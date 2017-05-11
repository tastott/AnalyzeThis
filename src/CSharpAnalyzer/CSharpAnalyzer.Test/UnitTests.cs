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

        [TestMethod]
        public void DetectsUnassignedReadOnlyProperty()
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
        }

        [TestMethod]
        public void IgnoresChainedThisConstructor()
        {
            var test = @"
    namespace MyNamespace
    {
        class MyClass
        {
            private readonly int foo;

            public MyClass(int foo)
            {
                this.foo = foo;
            }

            public MyClass()
                : this(4)
            {
            }
        }
    }";

            VerifyNoCSharpDiagnostics(test);
        }

        [TestMethod]
        public void DoesntIgnoreChainedBaseConstructor()
        {
            var test = @"
    namespace MyNamespace
    {
        class BaseClass {}

        class MyClass : BaseClass
        {
            private readonly int foo;

            public MyClass(int foo)
                : base()
            {
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = "CSharpAnalyzer",
                Message = $"Readonly field(s) not assigned in constructor: foo.",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 10, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void IgnoresFieldAssignedOnDeclaration()
        {
            var test = @"
    namespace MyNamespace
    {
        class MyClass
        {
            private readonly int foo = 1;

            public MyClass()
            {
            }
        }
    }";

            VerifyNoCSharpDiagnostics(test);
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