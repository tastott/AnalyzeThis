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
using System.Threading;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Formatting;
using System.Text.RegularExpressions;

namespace AnalyzeThis.ReadonlyField
{
    internal class ReadonlyFieldRule : FixableAnalysisRule
    {
        private readonly string fixTitle = "Set with constructor parameter";

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

        public override void RegisterCodeFix(CodeFixContext context, Diagnostic diagnostic)
        {
            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: fixTitle,
                    createChangedDocument: c => AddConstructorParameterAndAssignAsync(context.Document, diagnostic.Location, c),
                    equivalenceKey: fixTitle
                ),
                diagnostic);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var constructorNode = context.Node as ConstructorDeclarationSyntax;


            IEnumerable<string> unsetReadonlyFieldNames = GetUnassignedReadonlyFields(constructorNode)
                .Select(declaration => declaration.GetIdentifierText());

            if (unsetReadonlyFieldNames.Any())
            {
                var diagnostic = Diagnostic.Create(this.Descriptor, constructorNode.GetLocation(), string.Join(", ", unsetReadonlyFieldNames));
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static IEnumerable<FieldDeclarationSyntax> GetUnassignedReadonlyFields(ConstructorDeclarationSyntax constructorNode)
        {
            // Ignore chained 'this' constructors
            if (constructorNode.Initializer?.Kind() == SyntaxKind.ThisConstructorInitializer)
            {
                return Enumerable.Empty<FieldDeclarationSyntax>();
            }

            var classNode = constructorNode.Parent as ClassDeclarationSyntax;

            var assignedFields = (constructorNode.Body?.DescendantNodes() ?? Enumerable.Empty<SyntaxNode>())
                .WhereIs<SyntaxNode, AssignmentExpressionSyntax>()
                .Select(x =>
                {
                    return x;
                })
                .Select(assignment => assignment.Left)
                .WhereIs<ExpressionSyntax, MemberAccessExpressionSyntax>()
                .Select(memberAccess => memberAccess.Name.Identifier.ValueText)
                .ToImmutableHashSet();

            var unsetReadonlyFields = classNode.Members
                .WhereIs<SyntaxNode, FieldDeclarationSyntax>() // Fields
                .Where(fieldNode => fieldNode.Modifiers.Any(SyntaxKind.ReadOnlyKeyword)) // Readonly
                .Where(fieldNode => fieldNode.Declaration.Variables.First().Initializer == null) // Not initialized inline
                .Where(fieldNode => !assignedFields.Contains(fieldNode.GetIdentifierText())); // Not assigned in constructor

            return unsetReadonlyFields;
        }

        private string GetLocalIdentifierName(string originalName)
        {
            return Regex.Replace(originalName, "^[A-Z]", match => match.Value.ToLower());
        }

        private async Task<Document> AddConstructorParameterAndAssignAsync(
            Document document,
            Location location,
            CancellationToken cancellationToken
        )
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var constructorNode = root.FindNode(location.SourceSpan) as ConstructorDeclarationSyntax;
            var existingParameters = constructorNode.ParameterList
                .Parameters
                .ToDictionary(parameter => parameter.Identifier.ValueText, StringComparer.OrdinalIgnoreCase);

            var unassignedFields = GetUnassignedReadonlyFields(constructorNode);
            var newParameters = unassignedFields
                .Where(field => !existingParameters.ContainsKey(field.GetIdentifierText()))
                .Select(field =>
                    SyntaxFactory.Parameter(
                        SyntaxFactory
                            .Identifier(this.GetLocalIdentifierName(field.GetIdentifierText()))
                        )
                        .WithType(field.Declaration.Type)
                    );

            var newStatements = unassignedFields.Select(field =>
            {

                ParameterSyntax existingParameter;
                string assignmentRight;

                // Find existing parameter with same name (ignoring case)
                if (existingParameters.TryGetValue(field.GetIdentifierText(), out existingParameter))
                {
                    // Use it if type is the same
                    if (existingParameter.Type.ProperEquals(field.Declaration.Type))
                    {
                        assignmentRight = existingParameter.Identifier.ValueText;
                    }
                    // Abort if type is different
                    else
                    {
                        return null;
                    }
                }
                // Otherwise use adjusted field name
                else
                {
                    assignmentRight = this.GetLocalIdentifierName(field.GetIdentifierText());
                }


                return SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.ThisExpression(),
                            SyntaxFactory.IdentifierName(field.GetIdentifierText())
                        ),
                        SyntaxFactory.IdentifierName(assignmentRight)
                    )
                );
            })
            .Where(statement => statement != null);

            var updatedMethod = constructorNode
                .AddParameterListParameters(newParameters.ToArray())
                .AddBodyStatements(newStatements.ToArray());

            var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken);

            var updatedSyntaxTree = root.ReplaceNode(constructorNode, updatedMethod);
            updatedSyntaxTree = Formatter.Format(updatedSyntaxTree, new AdhocWorkspace());


            return document.WithSyntaxRoot(updatedSyntaxTree);
        }
    }
}
