# region License
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
#if !(NET3500 || NET3000 || NET2000)
using System.Threading.Tasks;
#endif
using NUnit.Framework;

using Sweet.Jayson;

namespace Sweet.Jayson.Tests
{
    public static class PerformanceTests
    {
        private const int LoopCount = 1000;

        public static void Test1()
        {
            Stopwatch sw = new Stopwatch();

            var obj = TestClasses.GetTypedContainerDto();

            JaysonSerializationSettings jaysonSerializationSettings =
                (JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.Auto;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            JaysonDeserializationSettings jaysonDeserializationSettings =
                (JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string s;

            sw.Reset();
            sw.Start();
            for (int i = 0; i < LoopCount; i++)
            {
                s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter Convert to String {0} ms", sw.ElapsedMilliseconds);

            s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

            sw.Reset();
            sw.Start();
            for (int i = 0; i < LoopCount; i++)
            {
                JaysonConverter.Parse(s, jaysonDeserializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter Parse {0} ms", sw.ElapsedMilliseconds);

            s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

            sw.Reset();
            sw.Start();
            for (int i = 0; i < LoopCount; i++)
            {
                JaysonConverter.ToObject<TypedContainerDto>(s, jaysonDeserializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter Convert to Object {0} ms", sw.ElapsedMilliseconds);
        }

        public static void Test2()
        {
            Stopwatch sw = new Stopwatch();

            var obj = TestClasses.GetTypedContainerDto();

            JaysonSerializationSettings jaysonSerializationSettings =
                (JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            JaysonDeserializationSettings jaysonDeserializationSettings =
                (JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string s;

            sw.Reset();
            sw.Start();
            for (int i = 0; i < LoopCount; i++)
            {
                s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter Convert to String {0} msec", sw.ElapsedMilliseconds);

            s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

            sw.Reset();
            sw.Start();
            for (int i = 0; i < LoopCount; i++)
            {
                JaysonConverter.Parse(s, jaysonDeserializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter Parse {0} msec", sw.ElapsedMilliseconds);

            s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

            sw.Reset();
            sw.Start();
            for (int i = 0; i < LoopCount; i++)
            {
                JaysonConverter.ToObject<TypedContainerDto>(s, jaysonDeserializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter Convert to Object {0} msec", sw.ElapsedMilliseconds);
        }

        public static void Test3()
        {
            Stopwatch sw = new Stopwatch();

            var obj = TestClasses.GetTypedContainerDto();

            JaysonSerializationSettings jaysonSerializationSettings =
                (JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.None;
            jaysonSerializationSettings.IgnoreNullValues = true;
            jaysonSerializationSettings.CaseSensitive = true;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            JaysonDeserializationSettings jaysonDeserializationSettings =
                (JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone();
            jaysonDeserializationSettings.CaseSensitive = true;

            string s;

            sw.Reset();
            sw.Start();
            for (int i = 0; i < LoopCount; i++)
            {
                JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter Convert to String {0} msec", sw.ElapsedMilliseconds);

            s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

            sw.Reset();
            sw.Start();
            for (int i = 0; i < LoopCount; i++)
            {
                JaysonConverter.Parse(s, jaysonDeserializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter Parse {0} msec", sw.ElapsedMilliseconds);

            s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

            sw.Reset();
            sw.Start();
            for (int i = 0; i < LoopCount; i++)
            {
                JaysonConverter.ToObject<TypedContainerDto>(s, jaysonDeserializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter Convert to Object {0} msec", sw.ElapsedMilliseconds);
        }

#if !(NET3500 || NET3000 || NET2000)
        public static void Test4()
        {
            Stopwatch sw = new Stopwatch();

            var obj = TestClasses.GetTypedContainerDto();

            JaysonSerializationSettings jaysonSerializationSettings =
                (JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.None;
            jaysonSerializationSettings.IgnoreNullValues = true;
            jaysonSerializationSettings.CaseSensitive = true;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            JaysonDeserializationSettings jaysonDeserializationSettings =
                (JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string s;

            sw.Reset();
            sw.Start();
            var pResult = Parallel.For(0, LoopCount, i =>
            {
                JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);
            });
            while (!pResult.IsCompleted)
                Thread.Yield();
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter Convert to String {0} msec", sw.ElapsedMilliseconds);

            s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

            sw.Reset();
            sw.Start();
            pResult = Parallel.For(0, LoopCount, i =>
            {
                JaysonConverter.Parse(s, jaysonDeserializationSettings);
            });
            while (!pResult.IsCompleted)
                Thread.Yield();
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter Parse {0} msec", sw.ElapsedMilliseconds);

            s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

            sw.Reset();
            sw.Start();
            pResult = Parallel.For(0, LoopCount, i =>
            {
                JaysonConverter.ToObject<TypedContainerDto>(s, jaysonDeserializationSettings);
            });
            while (!pResult.IsCompleted)
                Thread.Yield();
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter Convert to Object {0} msec", sw.ElapsedMilliseconds);
        }
#endif

        public static void TestPerformanceDataTable()
        {
            DataTable dt1 = new DataTable("My DataTable 1", "myTableNamespace");
            dt1.Columns.Add(new DataColumn("col1", typeof(string), null, MappingType.Element));
            dt1.Columns.Add(new DataColumn("col2", typeof(bool)));
            dt1.Columns.Add(new DataColumn("col3", typeof(DateTime)));
            dt1.Columns.Add(new DataColumn("col4", typeof(SimpleObj)));

            dt1.Rows.Add(new object[] { null, true, new DateTime (1983, 10, 25, 12, 45, 32, DateTimeKind.Utc),
				new SimpleObj {
					Value1 = "Hello",
					Value2 = "World 1"
				}});
            dt1.Rows.Add(new object[] { "row2", false, new DateTime (1983, 10, 25, 12, 45, 32, DateTimeKind.Local),
				new SimpleObj {
					Value1 = "Hello",
					Value2 = "World 2"
				}});

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
                {
                    Formatting = JaysonFormatting.Tab,
                    TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
                    TypeNames = JaysonTypeNameSerialization.Auto
                };

            JaysonDeserializationSettings jaysonDeserializationSettings =
                (JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone();
            jaysonDeserializationSettings.CaseSensitive = true;

            string json = null;
            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();
            for (int i = 0; i < LoopCount; i++)
            {
                json = JaysonConverter.ToJsonString(dt1, jaysonSerializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter DataTable serialization {0} msec", sw.ElapsedMilliseconds);

            json = JaysonConverter.ToJsonString(dt1, jaysonSerializationSettings);

            sw.Reset();
            sw.Start();
            for (int i = 0; i < LoopCount; i++)
            {
                JaysonConverter.ToObject<DataTable>(json, jaysonDeserializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter DataTable deserialization {0} msec", sw.ElapsedMilliseconds);
        }

        public static void TestPerformanceDataSet()
        {
            DataSet ds1 = new DataSet("My DataSet");

            DataTable dt1 = new DataTable("My DataTable 1", "myTableNamespace");
            dt1.Columns.Add(new DataColumn("col1", typeof(string), null, MappingType.Element));
            dt1.Columns.Add(new DataColumn("col2", typeof(bool)));
            dt1.Columns.Add(new DataColumn("col3", typeof(DateTime)));
            dt1.Columns.Add(new DataColumn("col4", typeof(SimpleObj)));

            dt1.Rows.Add(new object[] { null, true, new DateTime (1983, 10, 25, 12, 45, 32, DateTimeKind.Utc),
				new SimpleObj {
					Value1 = "Hello",
					Value2 = "World 1"
				}});
            dt1.Rows.Add(new object[] { "row2", false, new DateTime (1983, 10, 25, 12, 45, 32, DateTimeKind.Local),
				new SimpleObj {
					Value1 = "Hello",
					Value2 = "World 2"
				}});

            ds1.Tables.Add(dt1);

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
                {
                    Formatting = JaysonFormatting.Tab,
                    TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
                    TypeNames = JaysonTypeNameSerialization.Auto
                };

            JaysonDeserializationSettings jaysonDeserializationSettings =
                (JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone();
            jaysonDeserializationSettings.CaseSensitive = true;

            string json = null;
            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();
            for (int i = 0; i < LoopCount; i++)
            {
                json = JaysonConverter.ToJsonString(ds1, jaysonSerializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter DataSet serialization {0} msec", sw.ElapsedMilliseconds);

            json = JaysonConverter.ToJsonString(ds1, jaysonSerializationSettings);

            sw.Reset();
            sw.Start();
            for (int i = 0; i < LoopCount; i++)
            {
                JaysonConverter.ToObject<DataSet>(json, jaysonDeserializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter DataSet deserialization {0} msec", sw.ElapsedMilliseconds);
        }

        public static void TestPerformanceUseGlobalTypeNames()
        {
            var dto = TestClasses.GetTypedContainerDto();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.UseGlobalTypeNames = true;
            jaysonSerializationSettings.TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly;
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.Auto;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            Stopwatch sw = new Stopwatch();

            string json = null;

            sw.Reset();
            sw.Start();
            for (int i = 0; i < LoopCount; i++)
            {
                json = JaysonConverter.ToJsonString(dto, jaysonSerializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter UseGlobalTypeNames serialization {0} msec", sw.ElapsedMilliseconds);

            json = JaysonConverter.ToJsonString(dto, jaysonSerializationSettings);

            sw.Reset();
            sw.Start();
            for (int i = 0; i < LoopCount; i++)
            {
                JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter UseGlobalTypeNames deserialization {0} msec", sw.ElapsedMilliseconds);
        }

        public static void TestPerformanceDontUseGlobalTypeNames()
        {
            var dto = TestClasses.GetTypedContainerDto();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.UseGlobalTypeNames = false;
            jaysonSerializationSettings.TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly;
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.Auto;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            Stopwatch sw = new Stopwatch();

            string json = null;

            sw.Reset();
            sw.Start();
            for (int i = 0; i < LoopCount; i++)
            {
                json = JaysonConverter.ToJsonString(dto, jaysonSerializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter don't UseGlobalTypeNames serialization {0} msec", sw.ElapsedMilliseconds);

            json = JaysonConverter.ToJsonString(dto, jaysonSerializationSettings);

            sw.Reset();
            sw.Start();
            for (int i = 0; i < LoopCount; i++)
            {
                JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter don't UseGlobalTypeNames deserialization {0} msec", sw.ElapsedMilliseconds);
        }
    }
}

