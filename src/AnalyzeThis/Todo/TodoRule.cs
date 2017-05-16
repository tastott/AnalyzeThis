using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzeThis.Todo
{
    internal class TodoRule : AnalysisRule
    {
        private static readonly Regex TodoPattern = new Regex(@"^//\s*(TODO\b.+)", RegexOptions.IgnoreCase);

        public TodoRule() 
            : base(diagnosticId: "AT002", title: "TODO comment", messageFormat: "{0}", category: "TODO", severity: DiagnosticSeverity.Info, description: "TODO comment")
        {
        }

        public override void Register(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(this.HandleSyntaxTree);
        }

        private void HandleSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            SyntaxNode root = context.Tree.GetCompilationUnitRoot(context.CancellationToken);
            var singleLineComments = from node in root.DescendantTrivia() where node.IsKind(SyntaxKind.SingleLineCommentTrivia) select node;

            foreach (var comment in singleLineComments)
            {
                Match match;
                if (TodoPattern.TryGetMatch(comment.ToString(), out match))
                {
                    var diagnostic = Diagnostic.Create(this.Descriptor, comment.GetLocation(), match.Groups[1].Value);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
