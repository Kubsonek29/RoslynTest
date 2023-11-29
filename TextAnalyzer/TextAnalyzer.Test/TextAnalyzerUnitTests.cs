using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = TextAnalyzer.Test.CSharpCodeFixVerifier<
    TextAnalyzer.AnalizatorProfesorowy,
    TextAnalyzer.AnalizatorProfesorowyCodeFixProvider>;

namespace TextAnalyzer.Test
{
    [TestClass]
    public class TextAnalyzerUnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task TestMethod1()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task TestMethod2()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class {|#0:profesor|}
    {

    }
}";

            var fixtest = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class ProfesorJacekMatulewski
    {

    }
}";

            var expected = VerifyCS.Diagnostic(AnalizatorProfesorowy.DiagnosticId).WithSpan(11, 11, 11, 19).WithArguments("Found 'profesor' in the code");
            // 11, 15 - 11 od lewej i 11 linijka kodu od góry  - start   i koniec syntaxu - 11, 19 czyli ostatnia litera - profeso[r] (liczymy od 1 nie od 0)
            //argumenty - opis diagnostyki
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
