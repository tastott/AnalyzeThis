using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpAnalyzer
{
    internal static class SyntaxNodeExtensions
    {
        public static string GetIdentifierText(this FieldDeclarationSyntax fieldDeclaration)
        {
            return fieldDeclaration.Declaration.Variables.First().Identifier.ValueText;
        }
    }
}
