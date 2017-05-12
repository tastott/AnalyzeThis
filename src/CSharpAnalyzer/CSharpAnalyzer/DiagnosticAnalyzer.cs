using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using CSharpAnalyzer.ReadonlyField;

namespace CSharpAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CSharpAnalyzerAnalyzer : DiagnosticAnalyzer
    {

        internal readonly static IEnumerable<AnalysisRule> AllRules = 
            new AnalysisRule[]
            {
                new ReadonlyFieldRule()
            };

        private readonly IEnumerable<AnalysisRule> rules;

        public CSharpAnalyzerAnalyzer()
            : this(AllRules)
        {

        }

        internal CSharpAnalyzerAnalyzer(IEnumerable<AnalysisRule> rules)
        {
            this.rules = rules;
        }

        internal CSharpAnalyzerAnalyzer(params AnalysisRule[] rules)
        {
            this.rules = rules;
        }

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        //private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        //private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        //private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        //private const string Category = "Naming";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return this.rules
                    .Select(rule => rule.Descriptor)
                    .ToImmutableArray();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            foreach (var rule in AllRules)
            {
                rule.Register(context);
            }
        }
    }
}
