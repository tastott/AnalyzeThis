using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;

namespace AnalyzeThis.Test.Verifiers
{
    public abstract class AnalysisRuleVerifier : DiagnosticVerifier
    {
        private readonly AnalysisRule rule;

        internal AnalysisRuleVerifier(AnalysisRule rule)
        {
            this.rule = rule;
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AnalyzeThisAnalyzer(this.rule);
        }
    }
}
