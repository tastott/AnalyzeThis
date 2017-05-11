using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace CSharpAnalyzer.ReadonlyField
{
    internal class ReadonlyFieldRule : AnalysisRule
    {
        public ReadonlyFieldRule() 
            : base(
                  diagnosticId: "CSharpAnalyzer", 
                  title: "Readonly fields must be assigned.", 
                  messageFormat: "Readonly field(s) not assigned in constructor: {0}.", 
                  category: "TODO", 
                  severity: DiagnosticSeverity.Error,
                  description: "Readonly fields which are not assigned on declaration must be assigned in every non-chained constructor."
            )
        {
        }

        public override void Register(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ConstructorDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var constructorNode = context.Node as ConstructorDeclarationSyntax;

            // Ignore chained 'this' constructors
            if (constructorNode.Initializer?.Kind() == SyntaxKind.ThisConstructorInitializer)
            {
                return;
            }

            var classNode = constructorNode.Parent as ClassDeclarationSyntax;

            var assignedFields = (constructorNode.Body?.DescendantNodes() ?? Enumerable.Empty<SyntaxNode>())
                .WhereIs<SyntaxNode, AssignmentExpressionSyntax>()
                .Select(assignment => assignment.Left)
                .WhereIs<ExpressionSyntax, MemberAccessExpressionSyntax>()
                .Select(memberAccess => memberAccess.Name.Identifier.ValueText)
                .ToImmutableHashSet();

            var unsetReadonlyFieldNames = classNode.Members
                .WhereIs<SyntaxNode, FieldDeclarationSyntax>() // Fields
                .Where(fieldNode => fieldNode.Modifiers.Any(SyntaxKind.ReadOnlyKeyword)) // Readonly
                .Where(fieldNode => fieldNode.Declaration.Variables.First().Initializer == null) // Not initialized inline
                .Where(fieldNode => !assignedFields.Contains(fieldNode.Declaration.Variables.First().Identifier.ValueText)) // Not assigned in constructor
                .Select(fieldNode => fieldNode.Declaration.Variables.First().Identifier.ValueText);

            if (unsetReadonlyFieldNames.Any())
            {
                var diagnostic = Diagnostic.Create(this.Descriptor, constructorNode.GetLocation(), string.Join(", ", unsetReadonlyFieldNames));
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
