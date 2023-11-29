using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;

namespace TextAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AnalizatorProfesorowy : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NaszeIdDoDiagnostyki";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "NazwaNaszejKategorii";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } } //moze być więcej diagnostyk, które możemy dodać!

        public static string CheckWord = "profesor";

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxTreeAction(syntaxTreeContext =>
            {
                var root = syntaxTreeContext.Tree.GetRoot(syntaxTreeContext.CancellationToken);


                foreach (var classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    if (SprawdźCzyTokenZawieraCheckWord(classDeclaration.Identifier))
                    {
                        ReportDiagnostic(syntaxTreeContext, classDeclaration.Identifier);
                        //identyfikator zmiennej
                    }
                }


                //Sprawdzamy deklaracje zmiennych czy nie zawieraja naszego CheckWord
                foreach (var variableDeclaration in root.DescendantNodes().OfType<VariableDeclarationSyntax>())
                {
                    foreach (var variable in variableDeclaration.Variables)
                    {
                        if (SprawdźCzyTokenZawieraCheckWord(variable.Identifier))
                        {
                            ReportDiagnostic(syntaxTreeContext, variable.Identifier);
                            //identyfikator zmiennej
                        }
                    }
                }


                //Sprawdzamy deklaracje metod czy nie zawieraja naszego CheckWord
                foreach (var methodDeclaration in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    if (SprawdźCzyTokenZawieraCheckWord(methodDeclaration.Identifier))
                    {
                        ReportDiagnostic(syntaxTreeContext, methodDeclaration.Identifier);
                        //identyfikator metody
                    }

                    //Następnie parametry metody Method(int x, int y) - x,y
                    foreach (var parameter in methodDeclaration.ParameterList.Parameters)
                    {
                        if (SprawdźCzyTokenZawieraCheckWord(parameter.Identifier))
                        {
                            ReportDiagnostic(syntaxTreeContext, parameter.Identifier);
                            //identyfikator parametru metody
                        }
                    }
                }

                //Wszystkie pozostałe, czyli po prostu w tekscie.
                foreach (var identifier in root.DescendantNodes().OfType<IdentifierNameSyntax>())
                {
                    if (SprawdźCzyTokenZawieraCheckWord(identifier.Identifier))
                    {
                        ReportDiagnostic(syntaxTreeContext, identifier.Identifier);
                    }
                }
            });
        }

        //SyntaxToken - reprezentuje token w drzewie składni programu. Dostarcza on informacje o rodzaju tokenu, jego tekscie pozycji i innych szczegolach.
        //SyntaxTreeAnalysisContext - obiekt kontekstu dostarczany analizatorom podczas fazy analizy drzewa składniowego - udostepnia dostep do drzewa skladniowego itd. - np:

        //|-- UsingDirective (System)
        //|-- NamespaceDeclaration(MyNamespace)
        //|   |-- ClassDeclaration(MyClass)
        //|       |-- MethodDeclaration(MyMethod)
        //|           |-- ExpressionStatement
        //|               |-- InvocationExpression(Console.WriteLine)
        //|                   |-- Argument(StringLiteralExpression ("Hello, world!"))
        //
        //
        //Kod:
        /*
        using System;
        namespace MyNamespace
        {
            class MyClass
            {
                public void MyMethod()
                {
                    Console.WriteLine("Hello, world!");
                }
            }
        }
        */
        // IdentifierToken -  to nazwy, takie jak zmienne, metody, klasy itp.
        //
        //


        //metoda sprawdzajaca czy dany SyntaxToken nie zawiera w sobie naszego CheckWord
        private static bool SprawdźCzyTokenZawieraCheckWord(SyntaxToken identifier)
        {
            return identifier.ValueText.ToLower() == CheckWord;
        }


        //metoda która tworzy diagnostyke i przekazuje jej naszą zasade, która wczesniej opisalismy, Tekst jaki ma wyświetlić oraz lokacje
        private static void ReportDiagnostic(SyntaxTreeAnalysisContext syntaxTreeContext, SyntaxToken identifier)
        {
            var diagnostic = Diagnostic.Create(Rule, identifier.GetLocation(), $"Found '{CheckWord}' in the code.");
            syntaxTreeContext.ReportDiagnostic(diagnostic);
        }
    }
}
