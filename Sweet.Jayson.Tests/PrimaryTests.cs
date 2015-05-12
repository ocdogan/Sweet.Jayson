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
    public class PrimaryTests
    {
        [Test]
        public static void TestParseIso8601Date1()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);
            var date2 = JaysonCommon.ParseIso8601DateTime("1972-10-25", JaysonDateTimeZoneType.ConvertToUtc);
            Assert.True(date1.Date == date2.Date);
        }

        [Test]
        public static void TestParseIso8601Date2()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);
            var date2 = JaysonCommon.ParseIso8601DateTime("1972-10-25T12:45:32Z");
            Assert.True(date1 == date2);
        }

        [Test]
        public static void TestParseIso8601Date3()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Local);
            var tz = JaysonCommon.GetUtcOffset(date1);
            string str = String.Format("1972-10-25T12:45:32+{0:00}:{1:00}", tz.Hours, tz.Minutes);
            var date2 = JaysonCommon.ParseIso8601DateTime(str);
            Assert.True(date1 == date2);
        }

        [Test]
        public static void TestParseIso8601Date4()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Local);
            var tz = JaysonCommon.GetUtcOffset(date1);
            string str = String.Format("1972-10-25T12:45:32+{0:00}{1:00}", tz.Hours, tz.Minutes);
            var date2 = JaysonCommon.ParseIso8601DateTime(str);
            Assert.True(date1 == date2);
        }

        [Test]
        public static void TestParseIso8601Date5()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Local);
            var tz = JaysonCommon.GetUtcOffset(date1);
            string str = String.Format("19721025T124532+{0:00}{1:00}", tz.Hours, tz.Minutes);
            var date2 = JaysonCommon.ParseIso8601DateTime(str);
            Assert.True(date1 == date2);
        }

        [Test]
        public static void TestParseIso8601Date6()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Local);
            var tz = JaysonCommon.GetUtcOffset(date1);
            string str = String.Format("19721025T124532+{0:00}:{1:00}", tz.Hours, tz.Minutes);
            var date2 = JaysonCommon.ParseIso8601DateTime(str);
            Assert.True(date1 == date2);
        }

        [Test]
        public static void TestParseIso8601Date7()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);
            var date2 = JaysonCommon.ParseIso8601DateTime("19721025T124532Z");
            Assert.True(date1 == date2);
        }

        [Test]
        public static void TestParseIso8601Date8()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);
            var date2 = JaysonCommon.ParseIso8601DateTime("19721025T124532Z");
            Assert.True(date1 == date2);
        }

        [Test]
        public static void TestComplexObject()
        {
            var dto = TestClasses.GetTypedContainerDto();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            Assert.DoesNotThrow(() =>
            {
                string json = JaysonConverter.ToJsonString(dto, jaysonSerializationSettings);
                JaysonConverter.Parse(json, jaysonDeserializationSettings);
                JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);
            });
        }

        [Test]
        public static void TestInterfaceDeserializationUsingSType()
        {
            var dto = TestClasses.GetTypedContainerNoDto();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            object result = null;
            Assert.DoesNotThrow(() =>
            {
                string json = JaysonConverter.ToJsonString(dto, jaysonSerializationSettings);
                JaysonConverter.Parse(json, jaysonDeserializationSettings);
                result = JaysonConverter.ToObject<ITypedContainerNoDto>(json, jaysonDeserializationSettings);
            });

            Assert.IsNotNull(result);
            Assert.IsTrue(result is ITypedContainerNoDto);
        }

        private class TestBinder : System.Runtime.Serialization.SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                if (typeName.EndsWith("ITypedContainerNoDto"))
                {
                    return typeof(TypedContainerNoDto);
                }
                if (typeName.EndsWith("IJsonValueContainerNoDto"))
                {
                    return typeof(JsonValueContainerNoDto);
                }
                return null;
            }

            public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                assemblyName = null;
                typeName = null;
            }
        }

        [Test]
        public static void TestInterfaceDeserializationUsingBinder()
        {
            var dto = TestClasses.GetTypedContainerNoDto();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = true;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;
            jaysonDeserializationSettings.Binder = new TestBinder();

            object result = null;
            Assert.DoesNotThrow(() =>
            {
                string json = JaysonConverter.ToJsonString(dto, jaysonSerializationSettings);
                JaysonConverter.Parse(json, jaysonDeserializationSettings);
                result = JaysonConverter.ToObject<ITypedContainerNoDto>(json, jaysonDeserializationSettings);
            });

            Assert.IsNotNull(result);
            Assert.IsTrue(result is ITypedContainerNoDto);
        }

        [Test]
        public static void TestInterfaceDeserializationUsingObjectActivator()
        {
            var dto = TestClasses.GetTypedContainerNoDto();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = true;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;


            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;
            jaysonDeserializationSettings.ObjectActivator = delegate(Type objType,
                IDictionary<string, object> parsedObject,
                out bool useDefaultCtor)
            {
                useDefaultCtor = true;
                if (objType == typeof(ITypedContainerNoDto))
                {
                    useDefaultCtor = false;
                    return new TypedContainerNoDto();
                }
                if (objType == typeof(IJsonValueContainerNoDto))
                {
                    useDefaultCtor = false;
                    return new JsonValueContainerNoDto();
                }
                return null;
            };

            object result = null;
            Assert.DoesNotThrow(() =>
            {
                string json = JaysonConverter.ToJsonString(dto, jaysonSerializationSettings);
                JaysonConverter.Parse(json, jaysonDeserializationSettings);
                result = JaysonConverter.ToObject<ITypedContainerNoDto>(json, jaysonDeserializationSettings);
            });

            Assert.IsNotNull(result);
            Assert.IsTrue(result is ITypedContainerNoDto);
        }

        [Test]
        public static void TestSerializeDeserializeDateWithCustomDateTimeFormat()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;
            jaysonSerializationSettings.DateTimeFormat = "dd/MM/yyyy HH:mm:ss.fff%K";
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.CustomDate;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.DateTimeFormat = "dd/MM/yyyy HH:mm:ss.fff%K";

            string json = JaysonConverter.ToJsonString(date1, jaysonSerializationSettings);

            Assert.True(json.Contains("25\\/10\\/1972 12:45:32.000Z"));
            var date2 = JaysonConverter.ToObject<DateTime?>(json, jaysonDeserializationSettings);
            Assert.NotNull(date2);
            Assert.IsAssignableFrom<DateTime>(date2);
            Assert.True(date1 == (DateTime)date2);
        }

        [Test]
        public static void TestSerializeDateTimeUtc()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            string json = JaysonConverter.ToJsonString(date1, jaysonSerializationSettings);
            Assert.True(json.Contains("1972-10-25T12:45:32Z"));
            var date2 = JaysonConverter.ToObject<DateTime?>(json);
            Assert.NotNull(date2);
            Assert.IsAssignableFrom<DateTime>(date2);
            Assert.True(date1 == (DateTime)date2);
        }

        [Test]
        public static void TestSerializeDateTimeLocal()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Local);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            string json = JaysonConverter.ToJsonString(date1, jaysonSerializationSettings);
            Assert.True(json.Contains("1972-10-25T12:45:32+") || json.Contains("1972-10-25T12:45:32-"));
            var date2 = JaysonConverter.ToObject<DateTime?>(json);
            Assert.NotNull(date2);
            Assert.IsAssignableFrom<DateTime>(date2);
            Assert.True(date1 == (DateTime)date2);
        }

        [Test]
        public static void TestSerializeDateTimeUnspecified()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Unspecified);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            string json = JaysonConverter.ToJsonString(date1, jaysonSerializationSettings);
            Assert.True(json.Contains("1972-10-25T12:45:32+") || json.Contains("1972-10-25T12:45:32-"));
            var date2 = JaysonConverter.ToObject<DateTime?>(json);
            Assert.NotNull(date2);
            Assert.IsAssignableFrom<DateTime>(date2);
            Assert.True(date1 == (DateTime)date2);
        }

        [Test]
        public static void TestSerializeDateTimeUtcMicrosoft()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);
            var dto1 = new VerySimpleJsonValue
            {
                Value = date1
            };

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            Assert.True(json.Contains("/Date("));
            var dto2 = JaysonConverter.ToObject<VerySimpleJsonValue>(json);
            Assert.NotNull(dto2);
            Assert.NotNull(dto2.Value);
            Assert.IsAssignableFrom<DateTime>(dto2.Value);
            Assert.True(date1 == (DateTime)dto2.Value);
        }

        [Test]
        public static void TestSerializeDateTimeLocalMicrosoft()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Local);
            var dto1 = new VerySimpleJsonValue
            {
                Value = date1
            };

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            Assert.True(json.Contains("/Date(") && (json.Contains("+") || json.Contains("-")));
            var dto2 = JaysonConverter.ToObject<VerySimpleJsonValue>(json);
            Assert.NotNull(dto2);
            Assert.NotNull(dto2.Value);
            Assert.IsAssignableFrom<DateTime>(dto2.Value);
            Assert.True(date1 == (DateTime)dto2.Value);
        }

        [Test]
        public static void TestSerializeDateTimeUnspecifiedMicrosoft()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Unspecified);
            var dto1 = new VerySimpleJsonValue
            {
                Value = date1
            };

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            Assert.True(json.Contains("/Date(") && (json.Contains("+") || json.Contains("-")));
            var dto2 = JaysonConverter.ToObject<VerySimpleJsonValue>(json);
            Assert.NotNull(dto2);
            Assert.NotNull(dto2.Value);
            Assert.IsAssignableFrom<DateTime>(dto2.Value);
            Assert.True(date1 == (DateTime)dto2.Value);
        }

        [Test]
        public static void TestDeserializeDateTimeConvertToUtc()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Local);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            string json = JaysonConverter.ToJsonString(date1, jaysonSerializationSettings);
            Assert.True(json.Contains("1972-10-25T12:45:32"));

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.ConvertToUtc;

            var date2 = JaysonConverter.ToObject<DateTime>(json, jaysonDeserializationSettings);
            Assert.NotNull(date2);
            Assert.IsAssignableFrom<DateTime>(date2);
            Assert.True(date2.Kind == DateTimeKind.Utc);
        }

        [Test]
        public static void TestDeserializeDateTimeConvertToLocal()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            string json = JaysonConverter.ToJsonString(date1, jaysonSerializationSettings);
            Assert.True(json.Contains("1972-10-25T12:45:32"));

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.ConvertToLocal;

            var date2 = JaysonConverter.ToObject<DateTime>(json, jaysonDeserializationSettings);
            Assert.NotNull(date2);
            Assert.IsAssignableFrom<DateTime>(date2);
            Assert.True(date2.Kind == DateTimeKind.Local);
        }

        [Test]
        public static void TestDeserializeDateTimeKeepAsIs()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Local);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            string json = JaysonConverter.ToJsonString(date1, jaysonSerializationSettings);
            Assert.True(json.Contains("1972-10-25T12:45:32"));

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            var date2 = JaysonConverter.ToObject<DateTime>(json, jaysonDeserializationSettings);
            Assert.NotNull(date2);
            Assert.IsAssignableFrom<DateTime>(date2);
            Assert.True(date1 == date2);
            Assert.True(date2.Kind == DateTimeKind.Local);
        }

        [Test]
        public static void TestDecimal1()
        {
            var dcml1 = 12345.67890123456789m;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;

            string json = JaysonConverter.ToJsonString(dcml1, jaysonSerializationSettings);
            Assert.True(json.Contains(".Decimal"));
            var dcml2 = JaysonConverter.ToObject(json);
            Assert.NotNull(dcml2);
            Assert.IsAssignableFrom<decimal>(dcml2);
            Assert.True(dcml1 == (decimal)dcml2);
        }

        [Test]
        public static void TestDecimal2()
        {
            var dcml1 = 12345.67890123456789m;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = true;

            string json = JaysonConverter.ToJsonString(dcml1, jaysonSerializationSettings);
            Assert.True(json == "12345.67890123456789");
            var dcml2 = JaysonConverter.ToObject(json);
            Assert.NotNull(dcml2);
            Assert.IsAssignableFrom<decimal>(dcml2);
            Assert.True(dcml1 == (decimal)dcml2);
        }

        [Test]
        public static void TestDecimal3()
        {
            var dcml1 = 12345.67890123456789m;
            var dto1 = new VerySimpleJsonValue
            {
                Value = dcml1
            };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            Assert.True(json.Contains(".Decimal"));
            var dto2 = JaysonConverter.ToObject<VerySimpleJsonValue>(json);
            Assert.NotNull(dto2);
            Assert.NotNull(dto2.Value);
            Assert.IsAssignableFrom<Decimal>(dto2.Value);
            Assert.True(dcml1 == (Decimal)dto2.Value);
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

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            Assert.True(json.Contains("Hello") && json.Contains("World"));
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

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = true;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            Assert.True(json.Contains("Hello") && json.Contains("World"));
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

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            Assert.True(json.Contains("Hello") && json.Contains("World"));
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

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = true;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.Array;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            Assert.True(json.Contains("Hello") && json.Contains("World"));
            var list2 = JaysonConverter.ToObject(json, jaysonDeserializationSettings);
            Assert.NotNull(list2);
            Assert.IsAssignableFrom<object[]>(list2);
            Assert.True(list2.GetType().IsArray);
            Assert.True(list1.Count == ((IList)list2).Count);
        }

        [Test]
        public static void TestArray2()
        {
            var list1 = new ArrayList { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = true;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            Assert.True(json.Contains("1") && json.Contains("2"));
            var list2 = JaysonConverter.ToObject(json, jaysonDeserializationSettings);
            Assert.NotNull(list2);
            Assert.True(list2.GetType().IsArray);
            Assert.True(list1.Count == ((IList)list2).Count);
        }

        [Test]
        public static void TestArray3()
        {
            var list1 = new ArrayList { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, null };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = true;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.Array;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            Assert.True(json.Contains("1") && json.Contains("2"));
            var list2 = JaysonConverter.ToObject(json, jaysonDeserializationSettings);
            Assert.NotNull(list2);
            Assert.True(list2.GetType().IsArray);
            Assert.True(list1.Count == ((IList)list2).Count);
        }

        [Test]
        public static void TestReadOnlyCollection1()
        {
            var list1 = new ReadOnlyCollection<object>(
                new List<object> { null, true, false, 1, 1.4, 123456.6d, 1234.56789m, "Hello", "World", 
					new VerySimpleJsonValue { 
						Value = new VerySimpleJsonValue { 
							Value = true 
						} 
					} 
				});

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            Assert.True(json.Contains("Hello") && json.Contains("World"));
            var list2 = JaysonConverter.ToObject(json);
            Assert.NotNull(list2);
            Assert.IsAssignableFrom<ReadOnlyCollection<object>>(list2);
            Assert.True(list1.Count == ((IList<object>)list2).Count);
        }

#if !(NET4000 || NET3500 || NET3000 || NET2000)
        [Test]
        public static void TestReadOnlyDictionary1()
        {
            var dict1 = new ReadOnlyDictionary<object, object>(
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

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings();
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;

            string json = JaysonConverter.ToJsonString(dict1, jaysonSerializationSettings);
            Assert.True(json.Contains("Hello") && json.Contains("World"));
            var dict2 = JaysonConverter.ToObject(json);
            Assert.NotNull(dict2);
            Assert.IsAssignableFrom<ReadOnlyDictionary<object, object>>(dict2);
            Assert.True(dict1.Count == ((ReadOnlyDictionary<object, object>)dict2).Count);
        }
#endif

        [Test]
        public static void TestParse()
        {
            var dto1 = new SimpleObj
            {
                Value1 = "Hello",
                Value2 = "World"
            };

            string json = JaysonConverter.ToJsonString(dto1, new JaysonSerializationSettings());
            var obj = JaysonConverter.Parse(json);
            Assert.NotNull(obj);
            Assert.True(typeof(IDictionary<string, object>).IsAssignableFrom(obj.GetType()));
            Assert.True(((IDictionary<string, object>)obj).Count == 2);
        }

        [Test]
        public static void TestNoFormatting()
        {
            var dto1 = new SimpleObj
            {
                Value1 = "Hello",
                Value2 = "World"
            };

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
            {
                Formatting = false
            };

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            Assert.True(json.Contains("\"Value1\":\"Hello\"") &&
                json.Contains("\"Value2\":\"World\""));
        }

        [Test]
        public static void TestFormatting()
        {
            var dto1 = new SimpleObj
            {
                Value1 = "Hello",
                Value2 = "World"
            };

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
            {
                Formatting = true
            };

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            Assert.True(json.Contains("\"Value1\": \"Hello\"") &&
                json.Contains("\"Value2\": \"World\""));
        }

        [Test]
        public static void TestToJsonStringSimpleObjectPerformance()
        {
            var dto1 = new SimpleObj
            {
                Value1 = "Hello",
                Value2 = "World"
            };

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
            {
                Formatting = false,
                TypeNameInfo = JaysonTypeNameInfo.TypeName,
                TypeNames = JaysonTypeNameSerialization.Auto
            };

            Stopwatch sw = new Stopwatch();

            sw.Restart();
            for (int i = 0; i < 10000; i++)
            {
                JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            }
            sw.Stop();
#if (DEBUG)
            Debug.WriteLine(String.Format("TestPerformanceSimpleObject: {0} msec", sw.ElapsedMilliseconds));
#else
			Console.WriteLine(String.Format ("TestPerformanceSimpleObject: {0} msec", sw.ElapsedMilliseconds));
#endif
            Assert.LessOrEqual(sw.ElapsedMilliseconds, 200);
        }

        [Test]
        public static void TestToJsonStringComplexObjectPerformance()
        {
            var dto1 = TestClasses.GetTypedContainerDto();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            Stopwatch sw = new Stopwatch();

            sw.Restart();
            for (int i = 0; i < 10000; i++)
            {
                JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            }
            sw.Stop();
#if (DEBUG)
            Debug.WriteLine(String.Format("TestPerformanceComplexObject ToJsonString: {0} msec", sw.ElapsedMilliseconds));
#else
			Console.WriteLine(String.Format ("TestPerformanceComplexObject ToJsonString: {0} msec", sw.ElapsedMilliseconds));
#endif
            Assert.LessOrEqual(sw.ElapsedMilliseconds, 2000);
        }

        [Test]
        public static void TestParseComplexObjectPerformance()
        {
            var dto1 = TestClasses.GetTypedContainerDto();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json1 = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);

            Stopwatch sw = new Stopwatch();

            sw.Restart();
            for (int i = 0; i < 10000; i++)
            {
                JaysonConverter.Parse(json1, jaysonDeserializationSettings);
            }
            sw.Stop();
#if (DEBUG)
            Debug.WriteLine(String.Format("TestPerformanceComplexObject Parse: {0} msec", sw.ElapsedMilliseconds));
#else
			Console.WriteLine(String.Format ("TestPerformanceComplexObject Parse: {0} msec", sw.ElapsedMilliseconds));
#endif
            Assert.LessOrEqual(sw.ElapsedMilliseconds, 2000);
        }

        [Test]
        public static void TestToObjectComplexObjectPerformance()
        {
            var dto1 = TestClasses.GetTypedContainerDto();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            Assert.DoesNotThrow(() =>
            {
                string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
                JaysonConverter.Parse(json, jaysonDeserializationSettings);
                JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);
            });

            string json1 = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);

            Stopwatch sw = new Stopwatch();

            sw.Restart();
            for (int i = 0; i < 10000; i++)
            {
                JaysonConverter.ToObject<TypedContainerDto>(json1, jaysonDeserializationSettings);
            }
            sw.Stop();
#if (DEBUG)
            Debug.WriteLine(String.Format("TestPerformanceComplexObject ToObject: {0} msec", sw.ElapsedMilliseconds));
#else
			Console.WriteLine(String.Format ("TestPerformanceComplexObject ToObject: {0} msec", sw.ElapsedMilliseconds));
#endif
            Assert.LessOrEqual(sw.ElapsedMilliseconds, 2000);
        }

#if !(NET3500 || NET3000 || NET2000)
        [Test]
        public static void TestMultiThreaded()
        {
            var dto1 = TestClasses.GetTypedContainerDto();

			JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = true;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            Stopwatch sw = new Stopwatch();

            sw.Restart();
            var pResult = Parallel.For(0, 10000, i =>
            {
                JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            });
            while (!pResult.IsCompleted)
                Thread.Yield();
            sw.Stop();
#if (DEBUG)
            Debug.WriteLine(String.Format("TestPerformanceComplexObject ToJsonString: {0} msec", sw.ElapsedMilliseconds));
#else
			Console.WriteLine(String.Format ("TestPerformanceComplexObject ToJsonString: {0} msec", sw.ElapsedMilliseconds));
#endif
            Assert.LessOrEqual(sw.ElapsedMilliseconds, 2000);
        }
#endif

        [Test]
        public static void TestIncludeTypeInfoAuto()
        {
            var dto1 = new SimpleObj
            {
                Value1 = "Hello",
                Value2 = "World"
            };

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
            {
                Formatting = false,
                TypeNameInfo = JaysonTypeNameInfo.TypeName,
                TypeNames = JaysonTypeNameSerialization.Auto
            };

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            Assert.AreEqual(json, @"{""$type"":""Sweet.Jayson.Tests.SimpleObj"",""Value2"":""World"",""Value1"":""Hello""}");
        }

        [Test]
        public static void TestIncludeTypeInfo()
        {
            var dto1 = new SimpleObj
            {
                Value1 = "Hello",
                Value2 = "World"
            };

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
            {
                Formatting = false,
                TypeNameInfo = JaysonTypeNameInfo.TypeName,
                TypeNames = JaysonTypeNameSerialization.All
            };

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            Assert.AreEqual(json, @"{""$type"":""Sweet.Jayson.Tests.SimpleObj"",""Value2"":""World"",""Value1"":""Hello""}");
        }

        [Test]
        public static void TestIncludeTypeInfoWithAssembly()
        {
            var dto1 = new SimpleObj
            {
                Value1 = "Hello",
                Value2 = "World"
            };

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
            {
                Formatting = false,
                TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
                TypeNames = JaysonTypeNameSerialization.All
            };

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            Assert.AreEqual(json, @"{""$type"":""Sweet.Jayson.Tests.SimpleObj, Sweet.Jayson.Tests"",""Value2"":""World"",""Value1"":""Hello""}");
        }

        [Test]
        public static void TestSerializeSimpleDataTable()
        {
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

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
            {
                Formatting = true,
                TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
                TypeNames = JaysonTypeNameSerialization.Auto
            };

            string json = JaysonConverter.ToJsonString(dt1, jaysonSerializationSettings);
            Assert.True(json.Contains("My DataTable 1") && json.Contains("myTableNamespace"));
        }

        [Test]
        public static void TestSerializeSimpleDataSet()
        {
            DataSet ds = new DataSet("My DataSet");

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

            ds.Tables.Add(dt1);

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
            {
                Formatting = true,
                TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
                TypeNames = JaysonTypeNameSerialization.Auto
            };

            string json = JaysonConverter.ToJsonString(ds, jaysonSerializationSettings);
            Assert.True(json.Contains("My DataSet") && json.Contains("My DataTable 1") &&
                json.Contains("myTableNamespace"));
        }

        [Test]
        public static void TestDeserializeSimpleDataTable()
        {
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

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
            {
                Formatting = false,
                TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
                TypeNames = JaysonTypeNameSerialization.Auto
            };

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = true;

            string json = JaysonConverter.ToJsonString(dt1, jaysonSerializationSettings);
            DataTable dt2 = JaysonConverter.ToObject<DataTable>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dt2);
            Assert.True(dt1.Columns.Count == dt2.Columns.Count);
        }

        [Test]
        public static void TestDeserializeSimpleDataSet()
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

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = true;

            string json = JaysonConverter.ToJsonString(ds1, jaysonSerializationSettings);
            Assert.True(json.Contains("My DataSet") && json.Contains("My DataTable 1") &&
                json.Contains("myTableNamespace"));

            DataSet ds2 = JaysonConverter.ToObject<DataSet>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(ds2);
            Assert.True(ds1.Tables.Count == ds2.Tables.Count);
        }

        [Test]
        public static void TestDeserializeComplexDataSet()
        {
            DataSet ds1 = new DataSet("My DataSet");

            DataTable dt1 = new DataTable("My DataTable 1", "myTableNamespace1");
            dt1.Columns.Add(new DataColumn("id", typeof(long)));
            dt1.Columns.Add(new DataColumn("col1", typeof(string), null, MappingType.Element));
            dt1.Columns.Add(new DataColumn("col2", typeof(bool)));
            dt1.Columns.Add(new DataColumn("col3", typeof(DateTime)));
            dt1.Columns.Add(new DataColumn("col4", typeof(SimpleObj)));

            dt1.Columns[0].Unique = true;
            dt1.Columns[0].ExtendedProperties.Add("x1", "X1");

            dt1.Rows.Add(new object[] { 0, null, true, new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Utc),
				new SimpleObj {
					Value1 = "Hello",
					Value2 = "World 1"
				}});
            dt1.Rows.Add(new object[] { 1, "row2", false, new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Local),
				new SimpleObj {
					Value1 = "Hello",
					Value2 = "World 2"
				}});

            dt1.ExtendedProperties.Add("x2", 2);

            DataTable dt2 = new DataTable("My DataTable 2", "myTableNamespace2");
            dt2.Columns.Add(new DataColumn("id", typeof(long)));
            dt2.Columns.Add(new DataColumn("parentid", typeof(long)));
            dt2.Columns.Add(new DataColumn("col1", typeof(string)));

            dt2.Columns[0].Unique = true;
            dt2.ExtendedProperties.Add("x3", 3);

            dt2.Rows.Add(new object[] { 0, 0, "string1" });
            dt2.Rows.Add(new object[] { 1, 1, "string2" });
            dt2.Rows.Add(new object[] { 2, 0, "string3" });
            dt2.Rows.Add(new object[] { 3, 1, "string4" });
            dt2.Rows.Add(new object[] { 4, 0, "string5" });
            dt2.Rows.Add(new object[] { 5, 1, "string6" });
            dt2.Rows.Add(new object[] { 6, 0, "string7" });
            dt2.Rows.Add(new object[] { 7, 1, "string8" });
            dt2.Rows.Add(new object[] { 8, 0, "string9" });
            dt2.Rows.Add(new object[] { 9, 1, "string10" });

            ds1.Tables.Add(dt1);
            ds1.Tables.Add(dt2);
            ds1.ExtendedProperties.Add("x4", true);

            ds1.Relations.Add(new DataRelation("dr1", dt1.Columns[0], dt2.Columns[1], true));

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
            {
                Formatting = true,
                TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
                TypeNames = JaysonTypeNameSerialization.All
            };

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = true;

            string json = JaysonConverter.ToJsonString(ds1, jaysonSerializationSettings);
            Assert.True(json.Contains("My DataSet") &&
                json.Contains("My DataTable 1") && json.Contains("myTableNamespace1") &&
                json.Contains("My DataTable 2") && json.Contains("myTableNamespace2"));

            DataSet ds2 = JaysonConverter.ToObject<DataSet>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(ds2);
            Assert.True(ds1.Tables.Count == ds2.Tables.Count);
            Assert.IsNotNull(ds2.Tables["My DataTable 1", "myTableNamespace1"]);
            Assert.IsNotNull(ds2.Tables["My DataTable 2", "myTableNamespace2"]);
            Assert.IsNotNull(ds2.Relations["dr1"]);
        }

        [Test]
        public static void TestSerializeDeserializeCustomDataSetTypeAll()
        {
            CustomDataSet1 ds1 = new CustomDataSet1();

            ds1.Table1.Rows.Add(new object[] { 0, null, "y1" });
            ds1.Table1.Rows.Add(new object[] { 1, "row2", "y2" });

            ds1.Table2.Rows.Add(new object[] { 0, 0, 0, "y1" });
            ds1.Table2.Rows.Add(new object[] { 0, 1, 1, "y2" });
            ds1.Table2.Rows.Add(new object[] { 0, 2, 0, "y3" });
            ds1.Table2.Rows.Add(new object[] { 0, 3, 1, "y4" });
            ds1.Table2.Rows.Add(new object[] { 0, 4, 0, "y5" });
            ds1.Table2.Rows.Add(new object[] { 1, 0, 0, "y6" });
            ds1.Table2.Rows.Add(new object[] { 1, 1, 1, "y7" });
            ds1.Table2.Rows.Add(new object[] { 1, 2, 0, "y8" });
            ds1.Table2.Rows.Add(new object[] { 1, 3, 1, "y9" });
            ds1.Table2.Rows.Add(new object[] { 1, 4, 0, "y10" });

            ds1.Table1.Columns[0].ExtendedProperties.Add("x1", "X1");
            ds1.Table1.ExtendedProperties.Add("x2", 2);
            ds1.Table2.ExtendedProperties.Add("x3", 3);
            ds1.ExtendedProperties.Add("x4", true);

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
            {
                Formatting = true,
                TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
                TypeNames = JaysonTypeNameSerialization.All
            };

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = true;

            string json = JaysonConverter.ToJsonString(ds1, jaysonSerializationSettings);
            DataSet ds2 = JaysonConverter.ToObject<DataSet>(json, jaysonDeserializationSettings);
            DataSet ds3 = JaysonConverter.ToObject<CustomDataSet1>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(ds2);
            Assert.True(ds1.Tables.Count == ds2.Tables.Count);
            Assert.True(ds1.Tables.Count == ds3.Tables.Count);
        }

        [Test]
        public static void TestSerializeDeserializeCustomDataSetTypeNone()
        {
            CustomDataSet1 ds1 = new CustomDataSet1();

            ds1.Table1.Rows.Add(new object[] { 0, null, "y1" });
            ds1.Table1.Rows.Add(new object[] { 1, "row2", "y2" });

            ds1.Table2.Rows.Add(new object[] { 0, 0, 0, "y1" });
            ds1.Table2.Rows.Add(new object[] { 0, 1, 1, "y2" });
            ds1.Table2.Rows.Add(new object[] { 0, 2, 0, "y3" });
            ds1.Table2.Rows.Add(new object[] { 0, 3, 1, "y4" });
            ds1.Table2.Rows.Add(new object[] { 0, 4, 0, "y5" });
            ds1.Table2.Rows.Add(new object[] { 1, 0, 0, "y6" });
            ds1.Table2.Rows.Add(new object[] { 1, 1, 1, "y7" });
            ds1.Table2.Rows.Add(new object[] { 1, 2, 0, "y8" });
            ds1.Table2.Rows.Add(new object[] { 1, 3, 1, "y9" });
            ds1.Table2.Rows.Add(new object[] { 1, 4, 0, "y10" });

            ds1.Table1.Columns[0].ExtendedProperties.Add("x1", "X1");
            ds1.Table1.ExtendedProperties.Add("x2", 2);
            ds1.Table2.ExtendedProperties.Add("x3", 3);
            ds1.ExtendedProperties.Add("x4", true);

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
            {
                Formatting = true,
                TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
                TypeNames = JaysonTypeNameSerialization.None
            };

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = true;

            string json = JaysonConverter.ToJsonString(ds1, jaysonSerializationSettings);
            DataSet ds2 = JaysonConverter.ToObject<DataSet>(json, jaysonDeserializationSettings);
            DataSet ds3 = JaysonConverter.ToObject<CustomDataSet1>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(ds2);
            Assert.True(ds1.Tables.Count == ds2.Tables.Count);
            Assert.True(ds1.Tables.Count == ds3.Tables.Count);
        }

		[Test]
		public static void TestToJsonObjectCustomDataSet()
		{
			CustomDataSet1 ds1 = new CustomDataSet1();

			ds1.Table1.Rows.Add(new object[] { 0, null, "y1" });
			ds1.Table1.Rows.Add(new object[] { 1, "row2", "y2" });

			ds1.Table2.Rows.Add(new object[] { 0, 0, 0, "y1" });
			ds1.Table2.Rows.Add(new object[] { 0, 1, 1, "y2" });
			ds1.Table2.Rows.Add(new object[] { 0, 2, 0, "y3" });
			ds1.Table2.Rows.Add(new object[] { 0, 3, 1, "y4" });
			ds1.Table2.Rows.Add(new object[] { 0, 4, 0, "y5" });
			ds1.Table2.Rows.Add(new object[] { 1, 0, 0, "y6" });
			ds1.Table2.Rows.Add(new object[] { 1, 1, 1, "y7" });
			ds1.Table2.Rows.Add(new object[] { 1, 2, 0, "y8" });
			ds1.Table2.Rows.Add(new object[] { 1, 3, 1, "y9" });
			ds1.Table2.Rows.Add(new object[] { 1, 4, 0, "y10" });

			ds1.Table1.Columns[0].ExtendedProperties.Add("x1", "X1");
			ds1.Table1.ExtendedProperties.Add("x2", 2);
			ds1.Table2.ExtendedProperties.Add("x3", 3);
			ds1.ExtendedProperties.Add("x4", true);

			JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
			{
				Formatting = true,
				TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
				TypeNames = JaysonTypeNameSerialization.None
			};

			var jsonObj = JaysonConverter.ToJsonObject(ds1, jaysonSerializationSettings);

			Assert.IsNotNull(jsonObj);
		}
    }
}

