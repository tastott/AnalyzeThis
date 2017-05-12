using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpAnalyzer.Todo
{
    internal class TodoRule : AnalysisRule
    {
        public TodoRule() 
            : base(diagnosticId: "Todo", title: "TODO comment", messageFormat: "{0}", category: "TODO", severity: DiagnosticSeverity.Info, description: "TODO comment")
        {
        }

        public override void Register(AnalysisContext context)
        {
        }
    }
}
