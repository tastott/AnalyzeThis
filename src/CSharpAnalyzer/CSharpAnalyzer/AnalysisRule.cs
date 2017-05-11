using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpAnalyzer
{
    internal abstract class AnalysisRule
    {
        public readonly string DiagnosticId;
        public readonly DiagnosticDescriptor Descriptor;

        protected AnalysisRule(
            string diagnosticId, 
            string title, 
            string messageFormat, 
            string category,
            DiagnosticSeverity severity,
            string description
        )
        {
            this.DiagnosticId = diagnosticId;
            this.Descriptor = new DiagnosticDescriptor(diagnosticId, title, messageFormat, category, severity, isEnabledByDefault: true, description: description);
        }

        public abstract void Register(AnalysisContext context);
    }
}
