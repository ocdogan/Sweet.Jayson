using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

using Jayson;
using Jayson.Tests;

namespace Jayson.ConsoleTest
{
	class MainClass
	{
		public static void Main (string[] args)
		{	
			if (Console.IsOutputRedirected)
			{
				PerformanceTests();
				return;
			}

            do
            {
                int testType = ReadTestType();
				switch (testType) {
				case 1:
	                {
	                    UnitTests();
						break;
	                }
				case 2:
                	{
                    	PerformanceTests();
						break;
					} 
				default:
					return;
				}

				Console.WriteLine ("Press Escape to exit, any other to continue...");
			} while (Console.ReadKey (true).Key != ConsoleKey.Escape);
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

        private static void PerformanceTests()
        {
            PerformanceTest1();
            Console.WriteLine();
            PerformanceTest2();
            Console.WriteLine();
            PerformanceTest3();
            Console.WriteLine();
        }

        private static void UnitTests()
        {
            var methods = typeof(PrimaryTest).GetMethods().OrderBy(m => m.Name);
            foreach (var method in methods)
            {
                if (method.Name.StartsWith("Test"))
                {
                    try
                    {
                        Console.WriteLine("Testing {0} ...", method.Name);
                        method.Invoke(null, new object[0]);
                        Console.WriteLine("Test {0} passed.", method.Name);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Test {0} failed.", method.Name);
                        while (e is TargetInvocationException)
                        {
                            if (e.InnerException == null)
                                break;
                            e = e.InnerException;
                        }

                        Console.WriteLine(e.Message);
                    }
                    Console.WriteLine();
                }
            }
        }

		static void PerformanceTest1()
		{
			Stopwatch sw = new Stopwatch();

			var obj = TestClasses.GetTypedContainerDto();

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();
			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.Auto;
			jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
			jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

			JaysonDeserializationSettings jaysonDeserializationSettings = 
				(JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone ();
			jaysonDeserializationSettings.CaseSensitive = false;

			object c;
			string s;

			sw.Restart();
			for (int i = 0; i < 10000; i++)
			{
				s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);
			}
			sw.Stop();
			Console.WriteLine("JaysonConverter Convert to String {0} ms", sw.ElapsedMilliseconds);

			s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

			sw.Restart();
			for (int i = 0; i < 10000; i++)
			{
				c = JaysonConverter.Parse(s, jaysonDeserializationSettings);
			}
			sw.Stop();
			Console.WriteLine("JaysonConverter Parse {0} ms", sw.ElapsedMilliseconds);

			s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

			sw.Restart();
			for (int i = 0; i < 10000; i++)
			{
				c = JaysonConverter.ToObject<TypedContainerDto>(s, jaysonDeserializationSettings);
			}
			sw.Stop();
			Console.WriteLine("JaysonConverter Convert to Object {0} ms", sw.ElapsedMilliseconds);
		}

		static void PerformanceTest2()
		{
			Stopwatch sw = new Stopwatch();

			var obj = TestClasses.GetTypedContainerDto();

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();
			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;
			jaysonSerializationSettings.IgnoreNullValues = false;
			jaysonSerializationSettings.CaseSensitive = false;
			jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
			jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

			JaysonDeserializationSettings jaysonDeserializationSettings = 
				(JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone ();
			jaysonDeserializationSettings.CaseSensitive = false;

			object c;
			string s;

			sw.Restart();
			for (int i = 0; i < 10000; i++)
			{
				s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);
			}
			sw.Stop();
			Console.WriteLine("JaysonConverter Convert to String {0} ms", sw.ElapsedMilliseconds);

			s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

			sw.Restart();
			for (int i = 0; i < 10000; i++)
			{
				c = JaysonConverter.Parse(s, jaysonDeserializationSettings);
			}
			sw.Stop();
			Console.WriteLine("JaysonConverter Parse {0} ms", sw.ElapsedMilliseconds);

			s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

			sw.Restart();
			for (int i = 0; i < 10000; i++)
			{
				c = JaysonConverter.ToObject<TypedContainerDto>(s, jaysonDeserializationSettings);
			}
			sw.Stop();
			Console.WriteLine("JaysonConverter Convert to Object {0} ms", sw.ElapsedMilliseconds);
		}

		static void PerformanceTest3()
		{
			Stopwatch sw = new Stopwatch();

			var obj = TestClasses.GetTypedContainerDto();

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();
			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
			jaysonSerializationSettings.Formatting = false;
			jaysonSerializationSettings.IgnoreNullValues = true;
			jaysonSerializationSettings.CaseSensitive = true;
			jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
			jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

			JaysonDeserializationSettings jaysonDeserializationSettings = 
				(JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone ();
			jaysonDeserializationSettings.CaseSensitive = false;

			object c;
			string s;

			sw.Restart();
			for (int i = 0; i < 10000; i++)
			{
				s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);
			}
			sw.Stop();
			Console.WriteLine("JaysonConverter Convert to String {0} ms", sw.ElapsedMilliseconds);

			s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

			sw.Restart();
			for (int i = 0; i < 10000; i++)
			{
				c = JaysonConverter.Parse(s, jaysonDeserializationSettings);
			}
			sw.Stop();
			Console.WriteLine("JaysonConverter Parse {0} ms", sw.ElapsedMilliseconds);

			s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

			sw.Restart();
			for (int i = 0; i < 10000; i++)
			{
				c = JaysonConverter.ToObject<TypedContainerDto>(s, jaysonDeserializationSettings);
			}
			sw.Stop();
			Console.WriteLine("JaysonConverter Convert to Object {0} ms", sw.ElapsedMilliseconds);
		}
	}
}
