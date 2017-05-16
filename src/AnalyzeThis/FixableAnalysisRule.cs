using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzeThis
{
    internal abstract class FixableAnalysisRule : AnalysisRule
    {
        public FixableAnalysisRule(
            string diagnosticId, 
            string title, 
            string messageFormat, 
            string category, 
            DiagnosticSeverity severity, 
            string description
        ) 
            : base(diagnosticId, title, messageFormat, category, severity, description)
        {
        }

        public abstract void RegisterCodeFix(CodeFixContext context, Diagnostic diagnostic);
    }
}
