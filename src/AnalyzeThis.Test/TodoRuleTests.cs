using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnalyzeThis.Test.Verifiers;
using AnalyzeThis.Todo;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace AnalyzeThis.Test
{
    [TestClass]
    public class TodoRuleTests : AnalysisRuleVerifier
    {
        public TodoRuleTests()
            : base(new TodoRule())
        {

        }

        //No diagnostics expected to show up
        [TestMethod]
        public void EmptyFile()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void DetectsTodoCommentOnSeparateLine()
        {
            var test = @"
namespace MyNamespace
{
    class MyClass
    {
        public MyClass(int foo)
        {
            // TODO: Blah, blah, etc.
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = TodoRule.Id,
                Message = $"TODO: Blah, blah, etc.",
                Severity = DiagnosticSeverity.Info,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 8, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
    }
}
