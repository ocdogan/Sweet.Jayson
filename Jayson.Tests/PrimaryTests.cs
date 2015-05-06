using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
#if !(NET3500 || NET3000 || NET2000)
using System.Threading.Tasks;
#endif
using NUnit.Framework;

using Jayson;

namespace Jayson.Tests
{
	[TestFixture]
	public class PrimaryTest
	{
		[Test]
		public static void TestParseIso8601Date1()
		{
			var date1 = new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);
			var date2 = JaysonCommon.ParseIso8601DateTime ("1972-10-25", JaysonDateTimeZoneType.ConvertToUtc);
			Assert.True (date1.Date == date2.Date);
		}

		[Test]
		public static void TestParseIso8601Date2()
		{
			var date1 = new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);
			var date2 = JaysonCommon.ParseIso8601DateTime ("1972-10-25T12:45:32Z");
			Assert.True (date1 == date2);
		}

		[Test]
		public static void TestParseIso8601Date3()
		{
			var date1 = new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Local);
			var tz = JaysonCommon.GetUtcOffset (date1);
			string str = String.Format ("1972-10-25T12:45:32+{0:00}:{1:00}", tz.Hours, tz.Minutes);
			var date2 = JaysonCommon.ParseIso8601DateTime (str);
			Assert.True (date1 == date2);
		}

		[Test]
		public static void TestParseIso8601Date4()
		{
			var date1 = new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Local);
			var tz = JaysonCommon.GetUtcOffset (date1);
			string str = String.Format ("1972-10-25T12:45:32+{0:00}{1:00}", tz.Hours, tz.Minutes);
			var date2 = JaysonCommon.ParseIso8601DateTime (str);
			Assert.True (date1 == date2);
		}

		[Test]
		public static void TestParseIso8601Date5()
		{
			var date1 = new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Local);
			var tz = JaysonCommon.GetUtcOffset (date1);
			string str = String.Format ("19721025T124532+{0:00}{1:00}", tz.Hours, tz.Minutes);
			var date2 = JaysonCommon.ParseIso8601DateTime (str);
			Assert.True (date1 == date2);
		}

		[Test]
		public static void TestParseIso8601Date6()
		{
			var date1 = new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Local);
			var tz = JaysonCommon.GetUtcOffset (date1);
			string str = String.Format ("19721025T124532+{0:00}:{1:00}", tz.Hours, tz.Minutes);
			var date2 = JaysonCommon.ParseIso8601DateTime (str);
			Assert.True (date1 == date2);
		}

		[Test]
		public static void TestParseIso8601Date7()
		{
			var date1 = new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);
			var date2 = JaysonCommon.ParseIso8601DateTime ("19721025T124532Z");
			Assert.True (date1 == date2);
		}

		[Test]
		public static void TestParseIso8601Date8()
		{
			var date1 = new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);
			var date2 = JaysonCommon.ParseIso8601DateTime ("19721025T124532Z");
			Assert.True (date1 == date2);
		}

		[Test]
		public static void TestComplexObject()
		{
			var dto = TestClasses.GetTypedContainerDto ();

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;
			jaysonSerializationSettings.IgnoreNullValues = false;
			jaysonSerializationSettings.CaseSensitive = false;
			jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
			jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
			jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

			JaysonDeserializationSettings jaysonDeserializationSettings = 
				(JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone ();
			jaysonDeserializationSettings.CaseSensitive = false;

			Assert.DoesNotThrow (() => {
				string json = JaysonConverter.ToJsonString(dto, jaysonSerializationSettings);
				JaysonConverter.Parse(json, jaysonDeserializationSettings);
				JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);
			});
		}

		[Test]
		public static void TestSerializeDateTimeUtc()
		{
			var date = new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;
			jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
			jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

			string json = JaysonConverter.ToJsonString(date, jaysonSerializationSettings);
			Assert.True(json.Contains ("1972-10-25T12:45:32Z"));
			var date2 = JaysonConverter.ToObject<DateTime?>(json);
			Assert.NotNull(date2);
			Assert.IsAssignableFrom<DateTime>(date2);
			Assert.True(date == (DateTime)date2);
		}

		[Test]
		public static void TestSerializeDateTimeLocal()
		{
			var date = new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Local);

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;
			jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
			jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

			string json = JaysonConverter.ToJsonString(date, jaysonSerializationSettings);
			Assert.True(json.Contains ("1972-10-25T12:45:32+") || json.Contains ("1972-10-25T12:45:32-"));
			var date2 = JaysonConverter.ToObject<DateTime?>(json);
			Assert.NotNull(date2);
			Assert.IsAssignableFrom<DateTime>(date2);
			Assert.True(date == (DateTime)date2);
		}

		[Test]
		public static void TestSerializeDateTimeUnspecified()
		{
			var date = new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Unspecified);

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;
			jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
			jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

			string json = JaysonConverter.ToJsonString(date, jaysonSerializationSettings);
			Assert.True(json.Contains ("1972-10-25T12:45:32+") || json.Contains ("1972-10-25T12:45:32-"));
			var date2 = JaysonConverter.ToObject<DateTime?>(json);
			Assert.NotNull(date2);
			Assert.IsAssignableFrom<DateTime>(date2);
			Assert.True(date == (DateTime)date2);
		}

		[Test]
		public static void TestSerializeDateTimeUtcMicrosoft()
		{
			var date = new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);
			var dto1 = new VerySimpleJsonValue {
				Value = date
			};

			JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;
			jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
			jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

			string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
			Assert.True(json.Contains ("/Date("));
			var dto2 = JaysonConverter.ToObject<VerySimpleJsonValue>(json);
			Assert.NotNull(dto2);
			Assert.NotNull(dto2.Value);
			Assert.IsAssignableFrom<DateTime>(dto2.Value);
			Assert.True(date == (DateTime)dto2.Value);
		}

		[Test]
		public static void TestSerializeDateTimeLocalMicrosoft()
		{
			var date = new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Local);
			var dto1 = new VerySimpleJsonValue {
				Value = date
			};

			JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;
			jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
			jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

			string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
			Assert.True(json.Contains ("/Date(") && (json.Contains ("+") || json.Contains ("-")));
			var dto2 = JaysonConverter.ToObject<VerySimpleJsonValue>(json);
			Assert.NotNull(dto2);
			Assert.NotNull(dto2.Value);
			Assert.IsAssignableFrom<DateTime>(dto2.Value);
			Assert.True(date == (DateTime)dto2.Value);
		}

		[Test]
		public static void TestSerializeDateTimeUnspecifiedMicrosoft()
		{
			var date = new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Unspecified);
			var dto1 = new VerySimpleJsonValue {
				Value = date
			};

			JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;
			jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
			jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

			string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
			Assert.True(json.Contains ("/Date(") && (json.Contains ("+") || json.Contains ("-")));
			var dto2 = JaysonConverter.ToObject<VerySimpleJsonValue>(json);
			Assert.NotNull(dto2);
			Assert.NotNull(dto2.Value);
			Assert.IsAssignableFrom<DateTime>(dto2.Value);
			Assert.True(date == (DateTime)dto2.Value);
		}

		[Test]
		public static void TestDeserializeDateTimeConvertToUtc()
		{
			var date = new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Local);

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;
			jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
			jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

			string json = JaysonConverter.ToJsonString(date, jaysonSerializationSettings);
			Assert.True(json.Contains ("1972-10-25T12:45:32"));

			JaysonDeserializationSettings jaysonDeserializationSettings = 
				(JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone ();

			jaysonDeserializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.ConvertToUtc;

			var date2 = JaysonConverter.ToObject<DateTime>(json, jaysonDeserializationSettings);
			Assert.NotNull(date2);
			Assert.IsAssignableFrom<DateTime>(date2);
			Assert.True(date2.Kind == DateTimeKind.Utc);
		}

		[Test]
		public static void TestDeserializeDateTimeConvertToLocal()
		{
			var date = new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;
			jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
			jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

			string json = JaysonConverter.ToJsonString(date, jaysonSerializationSettings);
			Assert.True(json.Contains ("1972-10-25T12:45:32"));

			JaysonDeserializationSettings jaysonDeserializationSettings = 
				(JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone ();

			jaysonDeserializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.ConvertToLocal;

			var date2 = JaysonConverter.ToObject<DateTime>(json, jaysonDeserializationSettings);
			Assert.NotNull(date2);
			Assert.IsAssignableFrom<DateTime>(date2);
			Assert.True(date2.Kind == DateTimeKind.Local);
		}

		[Test]
		public static void TestDeserializeDateTimeKeepAsIs()
		{
			var date = new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Local);

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;
			jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
			jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

			string json = JaysonConverter.ToJsonString(date, jaysonSerializationSettings);
			Assert.True(json.Contains ("1972-10-25T12:45:32"));

			JaysonDeserializationSettings jaysonDeserializationSettings = 
				(JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone ();

			jaysonDeserializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

			var date2 = JaysonConverter.ToObject<DateTime>(json, jaysonDeserializationSettings);
			Assert.NotNull(date2);
			Assert.IsAssignableFrom<DateTime>(date2);
			Assert.True(date == date2);
			Assert.True(date2.Kind == DateTimeKind.Local);
		}

		[Test]
		public static void TestDecimal1()
		{
			var dcml = 12345.67890123456789m;

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;

			string json = JaysonConverter.ToJsonString(dcml, jaysonSerializationSettings);
			Assert.True(json.Contains (".Decimal"));
			var dcml2 = JaysonConverter.ToObject(json);
			Assert.NotNull(dcml2);
			Assert.IsAssignableFrom<decimal>(dcml2);
			Assert.True(dcml == (decimal)dcml2);
		}

		[Test]
		public static void TestDecimal2()
		{
			var dcml = 12345.67890123456789m;

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
			jaysonSerializationSettings.Formatting = true;

			string json = JaysonConverter.ToJsonString(dcml, jaysonSerializationSettings);
			Assert.True(json == "12345.67890123456789");
			var dcml2 = JaysonConverter.ToObject(json);
			Assert.NotNull(dcml2);
			Assert.IsAssignableFrom<decimal>(dcml2);
			Assert.True(dcml == (decimal)dcml2);
		}

		[Test]
		public static void TestDecimal3()
		{
			var dcml = 12345.67890123456789m;
			var dto1 = new VerySimpleJsonValue {
				Value = dcml
			};

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;

			string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
			Assert.True(json.Contains (".Decimal"));
			var dto2 = JaysonConverter.ToObject<VerySimpleJsonValue>(json);
			Assert.NotNull(dto2);
			Assert.NotNull(dto2.Value);
			Assert.IsAssignableFrom<Decimal>(dto2.Value);
			Assert.True(dcml == (Decimal)dto2.Value);
		}

		[Test]
		public static void TestList1()
		{
			var list1 = new List<object> { null, true, false, 1, 1.4, 123456.6f, 1234.56789m, "Hello", "World", 
				new VerySimpleJsonValue { 
					Value = new VerySimpleJsonValue { 
						Value = true 
					} 
				} 
			};

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;

			string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
			Assert.True(json.Contains ("Hello") && json.Contains ("World"));
			var list2 = JaysonConverter.ToObject(json);
			Assert.NotNull(list2);
			Assert.IsAssignableFrom<List<object>>(list2);
			Assert.True(list1.Count == ((List<object>)list2).Count);
		}

		[Test]
		public static void TestList2()
		{
			var list1 = new List<object> { null, true, false, 1, 1.4, 123456.6d, 1234.56789m, "Hello", "World", 
				new VerySimpleJsonValue { 
					Value = new VerySimpleJsonValue { 
						Value = true 
					} 
				} 
			};

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
			jaysonSerializationSettings.Formatting = true;

			string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
			Assert.True(json.Contains ("Hello") && json.Contains ("World"));
			var list2 = JaysonConverter.ToObject(json);
			Assert.NotNull(list2);
			Assert.IsAssignableFrom<List<object>>(list2);
			Assert.True(list1.Count == ((IList<object>)list2).Count);
		}

		[Test]
		public static void TestList3()
		{
			var list1 = new ArrayList { null, true, false, 1, 1.4, 123456.6d, 1234.56789m, "Hello", "World", 
				new VerySimpleJsonValue { 
					Value = new VerySimpleJsonValue { 
						Value = true 
					} 
				} 
			};

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;

			string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
			Assert.True(json.Contains ("Hello") && json.Contains ("World"));
			var list2 = JaysonConverter.ToObject(json);
			Assert.NotNull(list2);
			Assert.IsAssignableFrom<ArrayList>(list2);
			Assert.True(list1.Count == ((IList)list2).Count);
		}

		[Test]
		public static void TestArray1()
		{
			var list1 = new ArrayList { null, true, false, 1, 1.4, 123456.6d, 1234.56789m, "Hello", "World", 
				new VerySimpleJsonValue { 
					Value = new VerySimpleJsonValue { 
						Value = true 
					} 
				} 
			};

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
			jaysonSerializationSettings.Formatting = true;

			JaysonDeserializationSettings jaysonDeserializationSettings = 
				(JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone ();

			jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.Array;

			string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
			Assert.True(json.Contains ("Hello") && json.Contains ("World"));
			var list2 = JaysonConverter.ToObject(json, jaysonDeserializationSettings);
			Assert.NotNull(list2);
			Assert.IsAssignableFrom<object[]>(list2);
			Assert.True(list2.GetType ().IsArray);
			Assert.True(list1.Count == ((IList)list2).Count);
		}

		[Test]
		public static void TestArray2()
		{
			var list1 = new ArrayList { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
			jaysonSerializationSettings.Formatting = true;

			JaysonDeserializationSettings jaysonDeserializationSettings = 
				(JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone ();

			jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

			string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
			Assert.True(json.Contains ("1") && json.Contains ("2"));
			var list2 = JaysonConverter.ToObject(json, jaysonDeserializationSettings);
			Assert.NotNull(list2);
			Assert.True(list2.GetType ().IsArray);
			Assert.True(list1.Count == ((IList)list2).Count);
		}

		[Test]
		public static void TestArray3()
		{
			var list1 = new ArrayList { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, null };

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
			jaysonSerializationSettings.Formatting = true;

			JaysonDeserializationSettings jaysonDeserializationSettings = 
				(JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone ();

			jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.Array;

			string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
			Assert.True(json.Contains ("1") && json.Contains ("2"));
			var list2 = JaysonConverter.ToObject(json, jaysonDeserializationSettings);
			Assert.NotNull(list2);
			Assert.True(list2.GetType ().IsArray);
			Assert.True(list1.Count == ((IList)list2).Count);
		}

		[Test]
		public static void TestReadOnlyCollection1()
		{
			var list1 = new ReadOnlyCollection<object> (
				new List<object> { null, true, false, 1, 1.4, 123456.6d, 1234.56789m, "Hello", "World", 
					new VerySimpleJsonValue { 
						Value = new VerySimpleJsonValue { 
							Value = true 
						} 
					} 
				});

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;

			string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
			Assert.True(json.Contains ("Hello") && json.Contains ("World"));
			var list2 = JaysonConverter.ToObject(json);
			Assert.NotNull(list2);
			Assert.IsAssignableFrom<ReadOnlyCollection<object>>(list2);
			Assert.True(list1.Count == ((IList<object>)list2).Count);
		}

		#if !(NET4000 || NET3500 || NET3000 || NET2000)
		[Test]
		public static void TestReadOnlyDictionary1()
		{
			var dict1 = new ReadOnlyDictionary<object, object> (
				new Dictionary<object, object> { 
					{ "null", null }, 
					{ true, false }, 
					{ 1, 1.4 }, 
					{ 123456.6d, 1234.56789m },
					{ "Hello", "World" }, 
					{ (int?)13579, new VerySimpleJsonValue { 
							Value = new VerySimpleJsonValue { 
								Value = true 
							} 
						} 
					}
				});

			JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings ();

			jaysonSerializationSettings.IgnoreNullValues = false;
			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;

			string json = JaysonConverter.ToJsonString(dict1, jaysonSerializationSettings);
			Assert.True(json.Contains ("Hello") && json.Contains ("World"));
			var dict2 = JaysonConverter.ToObject(json);
			Assert.NotNull(dict2);
			Assert.IsAssignableFrom<ReadOnlyDictionary<object, object>>(dict2);
			Assert.True(dict1.Count == ((ReadOnlyDictionary<object, object>)dict2).Count);
		}
		#endif

		[Test]
		public static void TestParse()
		{
			var dto = new SimpleObj {
				Value1 = "Hello",
				Value2 = "World"
			};

			string json = JaysonConverter.ToJsonString(dto, new JaysonSerializationSettings());
			var obj = JaysonConverter.Parse (json);
			Assert.NotNull(obj);
			Assert.True(typeof(IDictionary<string, object>).IsAssignableFrom(obj.GetType ()));
			Assert.True(((IDictionary<string, object>)obj).Count == 2);
		}

		[Test]
		public static void TestNoFormatting()
		{
			var dto = new SimpleObj {
				Value1 = "Hello",
				Value2 = "World"
			};

			JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings {
				Formatting = false
			};

			string json = JaysonConverter.ToJsonString(dto, jaysonSerializationSettings);
			Assert.True (json.Contains ("\"Value1\":\"Hello\"") && 
				json.Contains ("\"Value2\":\"World\""));
		}

		[Test]
		public static void TestFormatting()
		{
			var dto = new SimpleObj {
				Value1 = "Hello",
				Value2 = "World"
			};

			JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings {
				Formatting = true
			};

			string json = JaysonConverter.ToJsonString(dto, jaysonSerializationSettings);
			Assert.True (json.Contains ("\"Value1\": \"Hello\"") && 
				json.Contains ("\"Value2\": \"World\""));
		}

		[Test]
		public static void TestToJsonStringSimpleObjectPerformance()
		{
			var dto = new SimpleObj {
				Value1 = "Hello",
				Value2 = "World"
			};

			JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings {
				Formatting = false,
				TypeNameInfo = JaysonTypeNameInfo.TypeName,
				TypeNames = JaysonTypeNameSerialization.Auto
			};

			Stopwatch sw = new Stopwatch ();

			sw.Restart ();
			for (int i = 0; i < 10000; i++) {
				JaysonConverter.ToJsonString (dto, jaysonSerializationSettings);
			}
			sw.Stop ();
			#if (DEBUG)
			Debug.WriteLine(String.Format ("TestPerformanceSimpleObject: {0} msec", sw.ElapsedMilliseconds));
			#else
			Console.WriteLine(String.Format ("TestPerformanceSimpleObject: {0} msec", sw.ElapsedMilliseconds));
			#endif
			Assert.LessOrEqual (sw.ElapsedMilliseconds, 200);
		}

		[Test]
		public static void TestToJsonStringComplexObjectPerformance()
		{
			var dto = TestClasses.GetTypedContainerDto ();

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;
			jaysonSerializationSettings.IgnoreNullValues = false;
			jaysonSerializationSettings.CaseSensitive = false;
			jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
			jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
			jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

			JaysonDeserializationSettings jaysonDeserializationSettings = 
				(JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone ();
			jaysonDeserializationSettings.CaseSensitive = false;

			Stopwatch sw = new Stopwatch ();

			sw.Restart ();
			for (int i = 0; i < 10000; i++) {
				JaysonConverter.ToJsonString(dto, jaysonSerializationSettings);
			}
			sw.Stop ();
			#if (DEBUG)
			Debug.WriteLine(String.Format ("TestPerformanceComplexObject ToJsonString: {0} msec", sw.ElapsedMilliseconds));
			#else
			Console.WriteLine(String.Format ("TestPerformanceComplexObject ToJsonString: {0} msec", sw.ElapsedMilliseconds));
			#endif
			Assert.LessOrEqual (sw.ElapsedMilliseconds, 1000);
		}

		[Test]
		public static void TestParseComplexObjectPerformance()
		{
			var dto = TestClasses.GetTypedContainerDto ();

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;
			jaysonSerializationSettings.IgnoreNullValues = false;
			jaysonSerializationSettings.CaseSensitive = false;
			jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
			jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
			jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

			JaysonDeserializationSettings jaysonDeserializationSettings = 
				(JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone ();
			jaysonDeserializationSettings.CaseSensitive = false;

			string json1 = JaysonConverter.ToJsonString(dto, jaysonSerializationSettings);

			Stopwatch sw = new Stopwatch ();

			sw.Restart ();
			for (int i = 0; i < 10000; i++) {
				JaysonConverter.Parse(json1, jaysonDeserializationSettings);
			}
			sw.Stop ();
			#if (DEBUG)
			Debug.WriteLine(String.Format ("TestPerformanceComplexObject Parse: {0} msec", sw.ElapsedMilliseconds));
			#else
			Console.WriteLine(String.Format ("TestPerformanceComplexObject Parse: {0} msec", sw.ElapsedMilliseconds));
			#endif
			Assert.LessOrEqual (sw.ElapsedMilliseconds, 1000);
		}

		[Test]
		public static void TestToObjectComplexObjectPerformance()
		{
			var dto = TestClasses.GetTypedContainerDto ();

			JaysonSerializationSettings jaysonSerializationSettings = 
				(JaysonSerializationSettings)JaysonSerializationSettings.Default.Clone ();

			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;
			jaysonSerializationSettings.IgnoreNullValues = false;
			jaysonSerializationSettings.CaseSensitive = false;
			jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
			jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
			jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

			JaysonDeserializationSettings jaysonDeserializationSettings = 
				(JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone ();
			jaysonDeserializationSettings.CaseSensitive = false;

			Assert.DoesNotThrow (() => {
				string json = JaysonConverter.ToJsonString(dto, jaysonSerializationSettings);
				JaysonConverter.Parse(json, jaysonDeserializationSettings);
				JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);
			});

			string json1 = JaysonConverter.ToJsonString(dto, jaysonSerializationSettings);

			Stopwatch sw = new Stopwatch ();

			sw.Restart ();
			for (int i = 0; i < 10000; i++) {
				JaysonConverter.ToObject<TypedContainerDto>(json1, jaysonDeserializationSettings);
			}
			sw.Stop ();
			#if (DEBUG)
			Debug.WriteLine(String.Format ("TestPerformanceComplexObject ToObject: {0} msec", sw.ElapsedMilliseconds));
			#else
			Console.WriteLine(String.Format ("TestPerformanceComplexObject ToObject: {0} msec", sw.ElapsedMilliseconds));
			#endif
			Assert.LessOrEqual (sw.ElapsedMilliseconds, 1000);
		}

		#if !(NET3500 || NET3000 || NET2000)
		[Test]
		public static void TestMultiThreaded()
		{
			var dto = TestClasses.GetTypedContainerDto ();

			JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.Default;
			jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
			jaysonSerializationSettings.Formatting = true;
			jaysonSerializationSettings.IgnoreNullValues = false;
			jaysonSerializationSettings.CaseSensitive = false;
			jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
			jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
			jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

			JaysonDeserializationSettings jaysonDeserializationSettings = 
				(JaysonDeserializationSettings)JaysonDeserializationSettings.Default.Clone ();
			jaysonDeserializationSettings.CaseSensitive = false;

			Stopwatch sw = new Stopwatch ();

			sw.Restart ();
			var pResult = Parallel.For (0, 10000, i => {
				JaysonConverter.ToJsonString (dto, jaysonSerializationSettings);
			});
			while (!pResult.IsCompleted)
				Thread.Yield ();
			sw.Stop ();
			#if (DEBUG)
			Debug.WriteLine(String.Format ("TestPerformanceComplexObject ToJsonString: {0} msec", sw.ElapsedMilliseconds));
			#else
			Console.WriteLine(String.Format ("TestPerformanceComplexObject ToJsonString: {0} msec", sw.ElapsedMilliseconds));
			#endif
			Assert.LessOrEqual (sw.ElapsedMilliseconds, 1000);
		}
		#endif

		[Test]
		public static void TestIncludeTypeInfoAuto()
		{
			var dto = new SimpleObj {
				Value1 = "Hello",
				Value2 = "World"
			};

			JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings {
				Formatting = false,
				TypeNameInfo = JaysonTypeNameInfo.TypeName,
				TypeNames = JaysonTypeNameSerialization.Auto
			};

			string json = JaysonConverter.ToJsonString(dto, jaysonSerializationSettings);
			Assert.AreEqual (json, @"{""$type"":""Jayson.Tests.SimpleObj"",""Value2"":""World"",""Value1"":""Hello""}");
		}

		[Test]
		public static void TestIncludeTypeInfo()
		{
			var dto = new SimpleObj {
				Value1 = "Hello",
				Value2 = "World"
			};

			JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings {
				Formatting = false,
				TypeNameInfo = JaysonTypeNameInfo.TypeName,
				TypeNames = JaysonTypeNameSerialization.All
			};

			string json = JaysonConverter.ToJsonString(dto, jaysonSerializationSettings);
			Assert.AreEqual (json, @"{""$type"":""Jayson.Tests.SimpleObj"",""Value2"":""World"",""Value1"":""Hello""}");
		}

		[Test]
		public static void TestIncludeTypeInfoWithAssembly()
		{
			var dto = new SimpleObj {
				Value1 = "Hello",
				Value2 = "World"
			};

			JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings {
				Formatting = false,
				TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
				TypeNames = JaysonTypeNameSerialization.All
			};

			string json = JaysonConverter.ToJsonString(dto, jaysonSerializationSettings);
			Assert.AreEqual (json, @"{""$type"":""Jayson.Tests.SimpleObj, Jayson.Tests"",""Value2"":""World"",""Value1"":""Hello""}");
		}
	}
}

