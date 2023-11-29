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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AnalizatorProfesorowyCodeFixProvider)), Shared]
    public class AnalizatorProfesorowyCodeFixProvider : CodeFixProvider //dostawca poprawek dla błędów wykrytych przez analizator
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds //definiuje identyfikator diagnostyki, który jest obsługiwany przez tego dostawce poprawek.
        {
            get { return ImmutableArray.Create(AnalizatorProfesorowy.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md //metoda zwraca dostawcę rozwiązań dla wszystkich zgłoszonych diagnostyk
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <summary>
        /// Jest to metoda wywoływana przez Roslyn w celu zarejestrowania poprawek w odpowiedzi na wykryte diagnostyki,
        /// pobiera korzeń drzewa składniowego dokumentu i następnie znajduje w nim węzeł odpowiadajacy lokalizacji diagnostyki.
        /// 
        /// Zostanie ta funkcja użyta gdy użytkownik skorzysta z użycia poprawki kodu.
        /// </summary>
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false); // asynchroniczne zbieranie informacji o korzeniu drzewa kodu z CancellationToken
            //CancellationToken umożliwia na anulowania operacji synchronicznyc np. Gdy użytkownik stwierdzi żeby wyłączyć program w trakcie działania.

            var diagnostic = context.Diagnostics.First(); //pobieramy diagnostyke naszego 
            var diagnosticSpan = diagnostic.Location.SourceSpan; //pobieramy lokalizacje naszej diagnostyki - span - która kolumna i który wiersz itd.

            var node = root.FindNode(diagnosticSpan); //znajdujemy węzeł na bazie lokalizacji.

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.CodeFixTitle,
                    createChangedDocument: c => TextChanger(context.Document, node, c),
                    equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                diagnostic); //funkcja rejestrujaca poprawke w systemie i tworzy nowy obiekt CodeAction który reprezentuje poprawkę Kodu wraz z jej parametrami i funkcją do poprawy kodu.
        }

        private async Task<Document> TextChanger(Document document, SyntaxNode node, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken); //bierzemy korzen dokumentu czyli najwyzyszy punkt.

            var token = node.GetFirstToken(); //bierze pierwszy token z danego węzła, ktory wczesniej podajemy

            if (node is ClassDeclarationSyntax classDeclaration)
            {
                //Sprwadzamy czy identyfikator klasy czyli public class [Identifier]
                if (classDeclaration.Identifier.ValueText.ToLower() == AnalizatorProfesorowy.CheckWord.ToLower())
                {
                    //tworzymy nowy syntaxtoken ale z identyfikatorem już naszym nowym
                    var newIdentifier = SyntaxFactory.Identifier("ProfesorJacekMatulewski");

                    //tworzymy nowa deklaracje klasy bazujac na tej która mamy i zmieniamy jej identyfikator
                    var newClassDeclaration = classDeclaration.WithIdentifier(newIdentifier);

                    // Podmieniamy starą klase na nową
                    var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);

                    //zwracamy caly dokument z zmodyfikowana klasa
                    return document.WithSyntaxRoot(newRoot);
                }
            }
            else if (token.ValueText.ToLower() == AnalizatorProfesorowy.CheckWord.ToLower())
            {
                var newNode = SyntaxFactory.IdentifierName("ProfesorJacekMatulewski"); //tworzymy nowy węzeł który z identyfikatorem tekstowym "..."

                var newRoot = root.ReplaceToken(token, newNode.Identifier); //podmieniamy tokeny

                return document.WithSyntaxRoot(newRoot); //zwracamy cały dokument z zmienionym tokenem
            }

            return document; //jezeli nie znajdzie zwracamy dokument bez zmian
        }
    }
}
