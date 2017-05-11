using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CSharpAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CSharpAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        //private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        //private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        //private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        //private const string Category = "Naming";

        private static readonly string Title = "Readonly fields must be assigned.";
        private static readonly string MessageFormat = "Readonly field(s) not assigned in constructor: {0}.";
        private static readonly LocalizableString Description = "Readonly fields which are not assigned on declaration must be assigned in every non-chained constructor.";
        private const string Category = "TODO";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            // context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ConstructorDeclaration);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
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
                var diagnostic = Diagnostic.Create(Rule, constructorNode.GetLocation(), string.Join(", ", unsetReadonlyFieldNames));
                context.ReportDiagnostic(diagnostic);
            }

        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            // Find just those named type symbols with names containing lowercase letters.
            if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
