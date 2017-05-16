using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzeThis
{
    internal static class SyntaxNodeExtensions
    {
        public static string GetIdentifierText(this FieldDeclarationSyntax fieldDeclaration)
        {
            return fieldDeclaration.Declaration.Variables.First().Identifier.ValueText;
        }

        public static bool ProperEquals(this TypeSyntax typeA, TypeSyntax typeB)
        {
            // TODO: There must be a better way of doing type equality
            return typeA.ToString().Equals(typeB.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
