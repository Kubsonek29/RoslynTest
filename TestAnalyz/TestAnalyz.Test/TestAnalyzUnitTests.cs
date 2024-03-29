﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = TestAnalyz.Test.CSharpCodeFixVerifier<
    TestAnalyz.TestAnalyzAnalyzer,
    TestAnalyz.TestAnalyzCodeFixProvider>;

namespace TestAnalyz.Test
{
    [TestClass]
    public class TestAnalyzUnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task TestMethod1()
        {
            var test =@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    public class test
    {   
        public static void func()
        {
            for(;;)
               for(;;)
               {
                  int i;
               }
        }

        public static void fun2()
        {
            int x;
        }
    }
}";

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    public class test
    {   
        public static void func()
        {
            for(;;)
            {
               for(;;)
               {
                  int i;
               }
            }
        }

        public static void fun2()
        {
            int x;
        }
    }
}";

            var expected = VerifyCS.Diagnostic("BraceAnalyzer").WithSpan(16, 16, 18, 17);
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
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
        class {|#0:TypeName|}
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
        class TYPENAME
        {   
        }
    }";

            var expected = VerifyCS.Diagnostic("TestAnalyz").WithLocation(0).WithArguments("TypeName");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
