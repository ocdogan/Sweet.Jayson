using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
#if !(NET3500 || NET3000 || NET2000)
using System.Threading.Tasks;
#endif
using NUnit.Framework;

using Sweet.Jayson;

namespace Sweet.Jayson.Tests
{
	[TestFixture]
	public static class PerformanceTests
	{
		[Test]
		public static void Test1()
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

			string s;

			sw.Restart();
			for (int i = 0; i < 10000; i++)
			{
				s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);
			}
			sw.Stop();
			Assert.True (sw.ElapsedMilliseconds < 2000);
			Console.WriteLine ("Sweet.JaysonConverter Convert to String {0} ms", sw.ElapsedMilliseconds);

			s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

			sw.Restart();
			for (int i = 0; i < 10000; i++)
			{
				JaysonConverter.Parse(s, jaysonDeserializationSettings);
			}
			sw.Stop();
			Assert.True (sw.ElapsedMilliseconds < 2000);
			Console.WriteLine ("Sweet.JaysonConverter Parse {0} ms", sw.ElapsedMilliseconds);

			s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

			sw.Restart();
			for (int i = 0; i < 10000; i++)
			{
				JaysonConverter.ToObject<TypedContainerDto>(s, jaysonDeserializationSettings);
			}
			sw.Stop();
			Assert.True (sw.ElapsedMilliseconds < 2000);
			Console.WriteLine ("Sweet.JaysonConverter Convert to Object {0} ms", sw.ElapsedMilliseconds);
		}

		[Test]
		public static void Test2()
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

			string s;

			sw.Restart();
			for (int i = 0; i < 10000; i++)
			{
				s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);
			}
			sw.Stop();
			Assert.True (sw.ElapsedMilliseconds < 2000);
            Console.WriteLine("Sweet.JaysonConverter Convert to String {0} msec", sw.ElapsedMilliseconds);

			s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

			sw.Restart();
			for (int i = 0; i < 10000; i++)
			{
				JaysonConverter.Parse(s, jaysonDeserializationSettings);
			}
			sw.Stop();
			Assert.True (sw.ElapsedMilliseconds < 2000);
            Console.WriteLine("Sweet.JaysonConverter Parse {0} msec", sw.ElapsedMilliseconds);

			s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

			sw.Restart();
			for (int i = 0; i < 10000; i++)
			{
				JaysonConverter.ToObject<TypedContainerDto>(s, jaysonDeserializationSettings);
			}
			sw.Stop();
			Assert.True (sw.ElapsedMilliseconds < 2000);
            Console.WriteLine("Sweet.JaysonConverter Convert to Object {0} msec", sw.ElapsedMilliseconds);
		}

		[Test]
		public static void Test3()
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
			jaysonDeserializationSettings.CaseSensitive = true;

			string s;

			sw.Restart();
			for (int i = 0; i < 10000; i++)
			{
				JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);
			}
			sw.Stop();
			Assert.True (sw.ElapsedMilliseconds < 2000);
            Console.WriteLine("Sweet.JaysonConverter Convert to String {0} msec", sw.ElapsedMilliseconds);

			s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

			sw.Restart();
			for (int i = 0; i < 10000; i++)
			{
				JaysonConverter.Parse(s, jaysonDeserializationSettings);
			}
			sw.Stop();
			Assert.True (sw.ElapsedMilliseconds < 2000);
            Console.WriteLine("Sweet.JaysonConverter Parse {0} msec", sw.ElapsedMilliseconds);

			s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

			sw.Restart();
			for (int i = 0; i < 10000; i++)
			{
				JaysonConverter.ToObject<TypedContainerDto>(s, jaysonDeserializationSettings);
			}
			sw.Stop();
			Assert.True (sw.ElapsedMilliseconds < 2000);
            Console.WriteLine("Sweet.JaysonConverter Convert to Object {0} msec", sw.ElapsedMilliseconds);
		}

		#if !(NET3500 || NET3000 || NET2000)
		[Test]
		public static void Test4()
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

			string s;

			sw.Restart();
			var pResult = Parallel.For (0, 10000, i => {
				JaysonConverter.ToJsonString (obj, jaysonSerializationSettings);
			});
			while (!pResult.IsCompleted)
				Thread.Yield ();
			sw.Stop();
			Assert.True (sw.ElapsedMilliseconds < 2000);
            Console.WriteLine("Sweet.JaysonConverter Convert to String {0} msec", sw.ElapsedMilliseconds);

			s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

			sw.Restart();
			pResult = Parallel.For (0, 10000, i => {
				JaysonConverter.Parse (s, jaysonDeserializationSettings);
			});
			while (!pResult.IsCompleted)
				Thread.Yield ();
			sw.Stop();
			Assert.True (sw.ElapsedMilliseconds < 2000);
            Console.WriteLine("Sweet.JaysonConverter Parse {0} msec", sw.ElapsedMilliseconds);

			s = JaysonConverter.ToJsonString(obj, jaysonSerializationSettings);

			sw.Restart();
			pResult = Parallel.For (0, 10000, i => {
				JaysonConverter.ToObject<TypedContainerDto> (s, jaysonDeserializationSettings);
			});
			while (!pResult.IsCompleted)
				Thread.Yield ();
			sw.Stop();
			Assert.True (sw.ElapsedMilliseconds < 2000);
            Console.WriteLine("Sweet.JaysonConverter Convert to Object {0} msec", sw.ElapsedMilliseconds);
		}
		#endif

        [Test]
        public static void TestPerformanceDataTable()
		{
			DataTable dt1 = new DataTable ("My DataTable 1", "myTableNamespace");
			dt1.Columns.Add (new DataColumn("col1", typeof(string), null, MappingType.Element));
			dt1.Columns.Add (new DataColumn("col2", typeof(bool)));
			dt1.Columns.Add (new DataColumn("col3", typeof(DateTime)));
			dt1.Columns.Add (new DataColumn("col4", typeof(SimpleObj)));

			dt1.Rows.Add (new object[] { null, true, new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Utc),
				new SimpleObj {
					Value1 = "Hello",
					Value2 = "World 1"
				}});
			dt1.Rows.Add (new object[] { "row2", false, new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Local),
				new SimpleObj {
					Value1 = "Hello",
					Value2 = "World 2"
				}});

			JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings {
				Formatting = true,
				TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
				TypeNames = JaysonTypeNameSerialization.Auto
			};

            JaysonDeserializationSettings jaysonDeserializationSettings =
                (JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone();
            jaysonDeserializationSettings.CaseSensitive = true;

            string json = null;
            Stopwatch sw = new Stopwatch();

            sw.Restart();
            for (int i = 0; i < 10000; i++)
            {
                json = JaysonConverter.ToJsonString(dt1, jaysonSerializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter DataTable serialization {0} msec", sw.ElapsedMilliseconds);
            Assert.True(sw.ElapsedMilliseconds < 10000);

            json = JaysonConverter.ToJsonString(dt1, jaysonSerializationSettings);
            sw.Restart();
            for (int i = 0; i < 10000; i++)
            {
                JaysonConverter.ToObject<DataTable>(json, jaysonDeserializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter DataTable deserialization {0} msec", sw.ElapsedMilliseconds);
            Assert.True(sw.ElapsedMilliseconds < 10000);
        }

        [Test]
        public static void TestPerformanceDataSet()
        {
            DataSet ds1 = new DataSet("My DataSet");

            DataTable dt1 = new DataTable("My DataTable 1", "myTableNamespace");
            dt1.Columns.Add(new DataColumn("col1", typeof(string), null, MappingType.Element));
            dt1.Columns.Add(new DataColumn("col2", typeof(bool)));
            dt1.Columns.Add(new DataColumn("col3", typeof(DateTime)));
            dt1.Columns.Add(new DataColumn("col4", typeof(SimpleObj)));

            dt1.Rows.Add(new object[] { null, true, new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Utc),
				new SimpleObj {
					Value1 = "Hello",
					Value2 = "World 1"
				}});
            dt1.Rows.Add(new object[] { "row2", false, new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Local),
				new SimpleObj {
					Value1 = "Hello",
					Value2 = "World 2"
				}});

            ds1.Tables.Add(dt1);

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
            {
                Formatting = true,
                TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
                TypeNames = JaysonTypeNameSerialization.Auto
            };

            JaysonDeserializationSettings jaysonDeserializationSettings =
                (JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone();
            jaysonDeserializationSettings.CaseSensitive = true;

            string json = null;
            Stopwatch sw = new Stopwatch();

            sw.Restart();
            for (int i = 0; i < 10000; i++)
            {
                json = JaysonConverter.ToJsonString(ds1, jaysonSerializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter DataSet serialization {0} msec", sw.ElapsedMilliseconds);
            Assert.True(sw.ElapsedMilliseconds < 10000);

            json = JaysonConverter.ToJsonString(ds1, jaysonSerializationSettings);

            sw.Restart();
            for (int i = 0; i < 10000; i++)
            {
                JaysonConverter.ToObject<DataSet>(json, jaysonDeserializationSettings);
            }
            sw.Stop();
            Console.WriteLine("Sweet.JaysonConverter DataSet deserialization {0} msec", sw.ElapsedMilliseconds);
            Assert.True(sw.ElapsedMilliseconds < 10000);
        }
    }
}

