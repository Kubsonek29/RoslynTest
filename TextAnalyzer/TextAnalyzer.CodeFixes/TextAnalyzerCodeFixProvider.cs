using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TextAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TextAnalyzerCodeFixProvider)), Shared]
    public class TextAnalyzerCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(TextAnalyzerAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.CodeFixTitle,
                    createChangedDocument: c => textchanger(context.Document, diagnostic, root),
                    equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                diagnostic);
        }

        Task<Document> textchanger(Document document, Diagnostic diagnostic, SyntaxNode root)
        {
            var location = diagnostic.Location;
            var token = root.FindToken(location.SourceSpan.Start);

            // Check if the token contains the word "salam"
            if (token.ValueText.ToLower().Contains(TextAnalyzerAnalyzer.CheckWord))
            {
                // Replace the token with a new one containing "We like visual studio"
                var newIdentifier = SyntaxFactory.IdentifierName("WeLikeVisualStudio")
                                       .WithLeadingTrivia(token.LeadingTrivia)
                                       .WithTrailingTrivia(token.TrailingTrivia);

                var newRoot = root.ReplaceNode(token.Parent, newIdentifier);
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }

            // If the word "salam" is not found, return the original document
            return Task.FromResult(document);
        }
    }
}
