using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace CSharpAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSharpAnalyzerCodeFixProvider)), Shared]
    public class CSharpAnalyzerCodeFixProvider : CodeFixProvider
    {
        private readonly IEnumerable<FixableAnalysisRule> rules;

        public CSharpAnalyzerCodeFixProvider()
            : this(CSharpAnalyzerAnalyzer.AllRules
                    .WhereIs<AnalysisRule, FixableAnalysisRule>()
            )
        {

        }

        internal CSharpAnalyzerCodeFixProvider(IEnumerable<FixableAnalysisRule> rules)
        {
            this.rules = rules;
        }

        internal CSharpAnalyzerCodeFixProvider(params FixableAnalysisRule[] rules)
        {
            this.rules = rules;
        }

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return this.rules
                    .Select(rule => rule.DiagnosticId)
                    .ToImmutableArray();
            }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                this.rules.FirstOrDefault(rule => diagnostic.Id == rule.DiagnosticId)
                    ?.RegisterCodeFix(context, diagnostic);
            }
        }

        

        private async Task<Solution> MakeUppercaseAsync(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            // Compute new uppercase name.
            var identifierToken = typeDecl.Identifier;
            var newName = identifierToken.Text.ToUpperInvariant();

            // Get the symbol representing the type to be renamed.
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            // Return the new solution with the now-uppercase type name.
            return newSolution;
        }
    }
}