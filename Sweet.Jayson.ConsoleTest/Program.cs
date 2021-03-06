﻿# region License
//	The MIT License (MIT)
//
//	Copyright (c) 2015, Cagatay Dogan
//
//	Permission is hereby granted, free of charge, to any person obtaining a copy
//	of this software and associated documentation files (the "Software"), to deal
//	in the Software without restriction, including without limitation the rights
//	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//	copies of the Software, and to permit persons to whom the Software is
//	furnished to do so, subject to the following conditions:
//
//		The above copyright notice and this permission notice shall be included in
//		all copies or substantial portions of the Software.
//
//		THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//		IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//		FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//		AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//		LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//		OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//		THE SOFTWARE.
# endregion License

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
#if !(NET3500 || NET3000 || NET2000)
using System.Threading.Tasks;
#endif

using Sweet.Jayson;
using Sweet.Jayson.Tests;

namespace Sweet.Jayson.ConsoleTest
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            do
            {
                int testType = ReadTestType();
                switch (testType)
                {
                    case 1:
                    case 2:
                        {
                            Test(testType);
                            break;
                        }
                    default:
                        return;
                }

                Console.WriteLine("Press Escape to exit, any other to continue...");
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }

        private static int ReadTestType()
        {
            ConsoleKey key;
            do
            {
                Console.Clear();
                Console.WriteLine("Press,");
                Console.WriteLine("  1) for Unit Tests");
                Console.WriteLine("  2) for Performance Tests");
                Console.WriteLine("  3) for Exit");
                Console.WriteLine();

                key = Console.ReadKey(true).Key;
            } while (!(key == ConsoleKey.D1 || key == ConsoleKey.D2 ||
                key == ConsoleKey.D3));

            Console.Clear();
            return (int)(key - ConsoleKey.D1) + 1;
        }

        private static void TestResult(string methodName, Exception e)
        {
            ConsoleColor clr = Console.ForegroundColor;
            try
            {
                if (JaysonCommon.IsOnMono())
                {
                    clr = ConsoleColor.Black;
                }

                if (e == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("{0} passed.", methodName);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine("{0} failed.", methodName);
                    while (e is TargetInvocationException)
                    {
                        if (e.InnerException == null)
                            break;
                        e = e.InnerException;
                    }

                    Console.WriteLine(e.Message);
                }
            }
            finally
            {
                Console.ForegroundColor = clr;
            }
            Console.WriteLine();
        }

        private static void Test(int testType)
        {
            List<MethodInfo> methods;

            var testClass = typeof(PrimaryTests);
            if (testType != 1)
            {
                testClass = typeof(PerformanceTests);
            }

            var allMethods = testClass.GetMethods();

            var testMethods = allMethods.Where(m => !m.Name.Equals("Setup", StringComparison.OrdinalIgnoreCase));
            var setupMethod = allMethods.FirstOrDefault(m => m.Name.Equals("Setup", StringComparison.OrdinalIgnoreCase));

            methods = new List<MethodInfo>(testMethods);
            methods.Sort(new Comparison<MethodInfo>((m1, m2) => String.Compare(m1.Name, m2.Name)));

            if (setupMethod != null)
            {
                setupMethod.Invoke(null, new object[0]);
            }

            var failedTests = new List<string>();

            foreach (var method in methods)
            {
                if (method.Name.StartsWith("Test"))
                {
                    Exception err = null;
                    try
                    {
                        Console.WriteLine("Testing {0} ...", method.Name);
                        method.Invoke(null, new object[0]);
                    }
                    catch (Exception e)
                    {
                        failedTests.Add(method.Name);

                        while (e is TargetInvocationException)
                        {
                            if (e.InnerException == null)
                                break;
                            e = e.InnerException;
                        }
                        err = e;
                    }
                    TestResult(method.Name, err);
                }
            }

            Console.WriteLine();
            if (failedTests.Count == 0)
            {
                TestResult("All tests", null);
            }
            else
            {
                TestResult(String.Join(", ", failedTests.ToArray()), new Exception("!!!"));
            }
        }
    }
}
