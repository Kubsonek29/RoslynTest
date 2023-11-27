using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Linq;
using System.Threading;

namespace TextAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TextAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "lethalcheck";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public static string CheckWord = "lethal";

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxTreeAction(syntaxTreeContext =>
            {
                var root = syntaxTreeContext.Tree.GetRoot(syntaxTreeContext.CancellationToken);

                // Process variable declarations
                foreach (var variableDeclaration in root.DescendantNodes().OfType<VariableDeclarationSyntax>())
                {
                    foreach (var variable in variableDeclaration.Variables)
                    {
                        if (ContainsCheckWord(variable.Identifier))
                        {
                            ReportDiagnostic(syntaxTreeContext, variable.Identifier);
                        }
                    }
                }

                // Process method declarations
                foreach (var methodDeclaration in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    if (ContainsCheckWord(methodDeclaration.Identifier))
                    {
                        ReportDiagnostic(syntaxTreeContext, methodDeclaration.Identifier);
                    }

                    // Process method parameters
                    foreach (var parameter in methodDeclaration.ParameterList.Parameters)
                    {
                        if (ContainsCheckWord(parameter.Identifier))
                        {
                            ReportDiagnostic(syntaxTreeContext, parameter.Identifier);
                        }
                    }
                }

                // Process other identifiers
                foreach (var identifier in root.DescendantNodes().OfType<IdentifierNameSyntax>())
                {
                    if (ContainsCheckWord(identifier.Identifier))
                    {
                        ReportDiagnostic(syntaxTreeContext, identifier.Identifier);
                    }
                }
            });
        }

        private static bool ContainsCheckWord(SyntaxToken identifier)
        {
            return identifier.ValueText.ToLower().Contains(CheckWord);
        }

        private static void ReportDiagnostic(SyntaxTreeAnalysisContext syntaxTreeContext, SyntaxToken identifier)
        {
            var diagnostic = Diagnostic.Create(Rule, identifier.GetLocation(), $"Found '{CheckWord}' in the code.");
            syntaxTreeContext.ReportDiagnostic(diagnostic);
        }


    }
}
