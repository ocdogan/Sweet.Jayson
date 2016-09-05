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
#if !(NET3500 || NET3000 || NET2000)
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
    [TestFixture]
    public class PrimaryTests
    {
        [Test]
        public static void TestDoubleNaN1a1 ()
        {
            var dto1 = new NaNType {
                D1 = double.NaN,
                D2 = double.PositiveInfinity,
                D3 = double.NegativeInfinity,
                F1 = float.NaN,
                F2 = float.PositiveInfinity,
                F3 = float.NegativeInfinity
            };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.Error;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.Error;

            Assert.Catch (() => { JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings); });
        }

        [Test]
        public static void TestDoubleNaN1a2 ()
        {
            var dto1 = double.NaN;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.Error;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.Error;

            Assert.Catch (() => { JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings); });
        }

        [Test]
        public static void TestDoubleNaN1a3 ()
        {
            var dto1 = double.PositiveInfinity;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.Error;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.Error;

            Assert.Catch (() => { JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings); });
        }

        [Test]
        public static void TestDoubleNaN1a4 ()
        {
            var dto1 = double.NegativeInfinity;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.Error;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.Error;

            Assert.Catch (() => { JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings); });
        }

        [Test]
        public static void TestDoubleNaN1b1 ()
        {
            var dto1 = new NaNType {
                D1 = double.NaN,
                D2 = double.PositiveInfinity,
                D3 = double.NegativeInfinity,
                F1 = float.NaN,
                F2 = float.PositiveInfinity,
                F3 = float.NegativeInfinity
            };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.ToDefault;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.ToDefault;

            string json = JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings);
            Assert.True (json.Contains ("\"D1\":"));
            Assert.True (json.Contains ("\"D2\":"));
            Assert.True (json.Contains ("\"D3\":"));
            Assert.True (json.Contains ("\"F1\":"));
            Assert.True (json.Contains ("\"F2\":"));
            Assert.True (json.Contains ("\"F3\":"));

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone ();
            jaysonDeserializationSettings.UseDefaultValues = true;

            var dto2 = JaysonConverter.ToObject<NaNType> (json, jaysonDeserializationSettings);

            Assert.IsNotNull (dto2);
            Assert.AreEqual (dto2.D1, default(double));
            Assert.AreEqual (dto2.D2, default(double));
            Assert.AreEqual (dto2.D3, default(double));
            Assert.AreEqual (dto2.F1, default(float));
            Assert.AreEqual (dto2.F2, default(float));
            Assert.AreEqual (dto2.F3, default(float));
        }

        [Test]
        public static void TestDoubleNaN1b2 ()
        {
            var dto1 = double.NaN;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.ToDefault;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.ToDefault;

            string json = JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings);
            Assert.True (json == "0");

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone ();
            jaysonDeserializationSettings.UseDefaultValues = true;

            var dto2 = JaysonConverter.ToObject<double> (json, jaysonDeserializationSettings);
            Assert.AreEqual (dto2, default (double));
        }

        [Test]
        public static void TestDoubleNaN1b3 ()
        {
            var dto1 = double.PositiveInfinity;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.ToDefault;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.ToDefault;

            string json = JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings);
            Assert.True (json == "0");

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone ();
            jaysonDeserializationSettings.UseDefaultValues = true;

            var dto2 = JaysonConverter.ToObject<double> (json, jaysonDeserializationSettings);
            Assert.AreEqual (dto2, default (double));
        }

        [Test]
        public static void TestDoubleNaN1b4 ()
        {
            var dto1 = double.NegativeInfinity;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.ToDefault;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.ToDefault;

            string json = JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings);
            Assert.True (json == "0");

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone ();
            jaysonDeserializationSettings.UseDefaultValues = true;

            var dto2 = JaysonConverter.ToObject<double> (json, jaysonDeserializationSettings);
            Assert.AreEqual (dto2, default (double));
        }

        [Test]
        public static void TestDoubleNaN1c1()
        {
            var dto1 = new NaNType {
                D1 = double.NaN,
                D2 = double.PositiveInfinity,
                D3 = double.NegativeInfinity,
                F1 = float.NaN,
                F2 = float.PositiveInfinity,
                F3 = float.NegativeInfinity
            };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.ToNull;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.ToNull;

            string json = JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings);
            Assert.True (json.Contains ("\"D1\":null"));
            Assert.True (json.Contains ("\"D2\":null"));
            Assert.True (json.Contains ("\"D3\":null"));
            Assert.True (json.Contains ("\"F1\":null"));
            Assert.True (json.Contains ("\"F2\":null"));
            Assert.True (json.Contains ("\"F3\":null"));

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone ();
            jaysonDeserializationSettings.UseDefaultValues = true;

            var dto2 = JaysonConverter.ToObject<NaNType> (json, jaysonDeserializationSettings);

            Assert.IsNotNull (dto2);
            Assert.AreEqual (dto2.D1, default(double));
            Assert.AreEqual (dto2.D2, default(double));
            Assert.AreEqual (dto2.D3, default(double));
            Assert.AreEqual (dto2.F1, default(float));
            Assert.AreEqual (dto2.F2, default(float));
            Assert.AreEqual (dto2.F3, default(float));
        }

        [Test]
        public static void TestDoubleNaN1c2 ()
        {
            var dto1 = double.NaN;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.ToNull;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.ToNull;

            string json = JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings);
            Assert.True (json == "null");

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone ();
            jaysonDeserializationSettings.UseDefaultValues = true;

            var dto2 = JaysonConverter.ToObject<double> (json, jaysonDeserializationSettings);
            Assert.AreEqual (dto2, default (double));
        }

        [Test]
        public static void TestDoubleNaN1c3 ()
        {
            var dto1 = double.PositiveInfinity;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.ToNull;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.ToNull;

            string json = JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings);
            Assert.True (json == "null");

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone ();
            jaysonDeserializationSettings.UseDefaultValues = true;

            var dto2 = JaysonConverter.ToObject<double> (json, jaysonDeserializationSettings);
            Assert.AreEqual (dto2, default (double));
        }

        [Test]
        public static void TestDoubleNaN1c4 ()
        {
            var dto1 = double.NegativeInfinity;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.ToNull;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.ToNull;

            string json = JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings);
            Assert.True (json == "null");

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone ();
            jaysonDeserializationSettings.UseDefaultValues = true;

            var dto2 = JaysonConverter.ToObject<double> (json, jaysonDeserializationSettings);
            Assert.AreEqual (dto2, default (double));
        }

        [Test]
        public static void TestDoubleNaN1d1 ()
        {
            var dto1 = new NaNType {
                D1 = double.NaN,
                D2 = double.PositiveInfinity,
                D3 = double.NegativeInfinity,
                F1 = float.NaN,
                F2 = float.PositiveInfinity,
                F3 = float.NegativeInfinity
            };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.IgnoreNullValues = true;
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.ToNull;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.ToNull;

            string json = JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings);
            Assert.True (json == "{}");

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone ();
            jaysonDeserializationSettings.UseDefaultValues = true;

            var dto2 = JaysonConverter.ToObject<NaNType> (json, jaysonDeserializationSettings);

            Assert.IsNotNull (dto2);
            Assert.AreEqual (dto2.D1, default (double));
            Assert.AreEqual (dto2.D2, default (double));
            Assert.AreEqual (dto2.D3, default (double));
            Assert.AreEqual (dto2.F1, default (float));
            Assert.AreEqual (dto2.F2, default (float));
            Assert.AreEqual (dto2.F3, default (float));
        }

        [Test]
        public static void TestDoubleNaN1d2 ()
        {
            var dto1 = double.NaN;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.IgnoreNullValues = true;
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.ToNull;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.ToNull;

            string json = JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings);
            Assert.True (json == "null");

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone ();
            jaysonDeserializationSettings.UseDefaultValues = true;

            var dto2 = JaysonConverter.ToObject<double> (json, jaysonDeserializationSettings);
            Assert.AreEqual (dto2, default (double));
        }

        [Test]
        public static void TestDoubleNaN1d3 ()
        {
            var dto1 = double.PositiveInfinity;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.IgnoreNullValues = true;
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.ToNull;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.ToNull;

            string json = JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings);
            Assert.True (json == "null");

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone ();
            jaysonDeserializationSettings.UseDefaultValues = true;

            var dto2 = JaysonConverter.ToObject<double> (json, jaysonDeserializationSettings);
            Assert.AreEqual (dto2, default (double));
        }

        [Test]
        public static void TestDoubleNaN1d4 ()
        {
            var dto1 = double.NegativeInfinity;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.IgnoreNullValues = true;
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.ToNull;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.ToNull;

            string json = JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings);
            Assert.True (json == "null");

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone ();
            jaysonDeserializationSettings.UseDefaultValues = true;

            var dto2 = JaysonConverter.ToObject<double> (json, jaysonDeserializationSettings);
            Assert.AreEqual (dto2, default (double));
        }

        [Test]
        public static void TestDoubleNaN1e1()
        {
            var dto1 = new NaNType {
                D1 = double.NaN,
                D2 = double.PositiveInfinity,
                D3 = double.NegativeInfinity,
                F1 = float.NaN,
                F2 = float.PositiveInfinity,
                F3 = float.NegativeInfinity
            };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.ToString;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.ToString;

            string json = JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings);
            Assert.True (json.Contains ("\"D1\":\"NaN\""));
            Assert.True (json.Contains ("\"D2\":\"Infinity\""));
            Assert.True (json.Contains ("\"D3\":\"-Infinity\""));
            Assert.True (json.Contains ("\"F1\":\"NaN\""));
            Assert.True (json.Contains ("\"F2\":\"Infinity\""));
            Assert.True (json.Contains ("\"F3\":\"-Infinity\""));

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone ();
            jaysonDeserializationSettings.UseDefaultValues = true;

            var dto2 = JaysonConverter.ToObject<NaNType> (json, jaysonDeserializationSettings);

            Assert.IsNotNull (dto2);
            Assert.IsTrue (double.IsNaN (dto2.D1));
            Assert.IsTrue (double.IsPositiveInfinity (dto2.D2));
            Assert.IsTrue (double.IsNegativeInfinity (dto2.D3));
            Assert.IsTrue (float.IsNaN (dto2.F1));
            Assert.IsTrue (float.IsPositiveInfinity (dto2.F2));
            Assert.IsTrue (float.IsNegativeInfinity (dto2.F3));
        }

        [Test]
        public static void TestDoubleNaN1e2 ()
        {
            var dto1 = double.NaN;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.ToString;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.ToString;

            string json = JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings);
            Assert.True (json == "\"NaN\"");

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone ();
            jaysonDeserializationSettings.UseDefaultValues = true;

            var dto2 = JaysonConverter.ToObject<double> (json, jaysonDeserializationSettings);
            Assert.AreEqual (dto2, double.NaN);
        }

        [Test]
        public static void TestDoubleNaN1e3 ()
        {
            var dto1 = double.PositiveInfinity;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.ToString;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.ToString;

            string json = JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings);
            Assert.True (json == "\"Infinity\"");

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone ();
            jaysonDeserializationSettings.UseDefaultValues = true;

            var dto2 = JaysonConverter.ToObject<double> (json, jaysonDeserializationSettings);
            Assert.AreEqual (dto2, double.PositiveInfinity);
        }

        [Test]
        public static void TestDoubleNaN1e4 ()
        {
            var dto1 = double.NegativeInfinity;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone ();
            jaysonSerializationSettings.FloatNanStrategy = JaysonFloatSerStrategy.ToString;
            jaysonSerializationSettings.FloatInfinityStrategy = JaysonFloatSerStrategy.ToString;

            string json = JaysonConverter.ToJsonString (dto1, jaysonSerializationSettings);
            Assert.True (json == "\"-Infinity\"");

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone ();
            jaysonDeserializationSettings.UseDefaultValues = true;

            var dto2 = JaysonConverter.ToObject<double> (json, jaysonDeserializationSettings);
            Assert.AreEqual (dto2, double.NegativeInfinity);
        }


        [Test]
        public static void TestInheritedOverridenTypes1a()
        {
            var dto1 = TestClasses.GetTypedContainerInheritedDto() as TypedContainerDto;

            var defaultGuid = new Guid("{4B2AFF22-0CD6-472D-A830-258C57DE8AD3}");

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            /* jaysonSerializationSettings.UseKVModelForISerializable = true;
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreDefaultValues = true;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K"; */
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.CustomDate;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffzzz"; // 2014-07-03T03:06:51.129+03:00
            jaysonSerializationSettings.DateTimeUTCFormat = "yyyy-MM-ddTHH:mm:ss.fffZ"; // 2014-07-03T03:06:51.129Z
            jaysonSerializationSettings.IgnoreNullValues = true;
            jaysonSerializationSettings.IgnoreDefaultValues = true;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.IgnoreEmptyCollections = true;
            jaysonSerializationSettings.UseEnumNames = true;

            jaysonSerializationSettings.AddTypeOverride(new JaysonTypeOverride<TypedContainerDto>()
                .SetMemberAlias("Timestamp", "@timestamp"));

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            Assert.True(!json.Contains("\"Timestamp\":"));
            Assert.True(json.Contains("\"@timestamp\":"));

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.UseDefaultValues = true;
            jaysonDeserializationSettings.AddTypeOverride(new JaysonTypeOverride<TypedContainerDto>()
                .SetDefaultValue("Guid3", defaultGuid));

            TypedContainerDto dto2 = JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dto2);
            Assert.AreEqual(dto2.Guid3, defaultGuid);

            dto2.Guid3 = Guid.Empty;
            CompareTypedContainerDtos(dto1, dto2);
        }

        [Test]
        public static void TestIgnoreEmptyCollections1a()
        {
            var dto1 = TestClasses.GetTypedContainerDto() as TypedContainerDto;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.UseKVModelForISerializable = true;
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.Auto;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreDefaultValues = true;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.IgnoreEmptyCollections = true; // <--
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            Assert.True(!json.Contains("\"EmptyList1\":"));

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.UseDefaultValues = true;

            TypedContainerDto dto2 = JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dto2);
            CompareTypedContainerDtos(dto1, dto2);
        }

        [Test]
        public static void TestIgnoreEmptyCollections1b()
        {
            var dto1 = TestClasses.GetTypedContainerDto() as TypedContainerDto;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.UseKVModelForISerializable = true;
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreDefaultValues = true;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.IgnoreEmptyCollections = false; // <--
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            Assert.True(json.Contains("\"EmptyList1\":"));

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.UseDefaultValues = true;

            TypedContainerDto dto2 = JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dto2);
            CompareTypedContainerDtos(dto1, dto2);
        }

        [Test]
        public static void TestIgnoreAndUseDefaultValues1a()
        {
            var dto1 = TestClasses.GetTypedContainerDto() as TypedContainerDto;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.UseKVModelForISerializable = true;
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreDefaultValues = true;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            Assert.True(!json.Contains("\"Enum3\":"));
            Assert.True(!json.Contains("\"Null1\":"));
            Assert.True(!json.Contains("\"Null2\":"));

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.UseDefaultValues = true;

            TypedContainerDto dto2 = JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dto2);
            CompareTypedContainerDtos(dto1, dto2);
        }

        [Test]
        public static void TestIgnoreAndUseDefaultValues1b()
        {
            var dto1 = TestClasses.GetTypedContainerDto() as TypedContainerDto;

            var defaultGuid = new Guid("{4B2AFF22-0CD6-472D-A830-258C57DE8AD3}");

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.UseKVModelForISerializable = true;
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreDefaultValues = true;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";
            jaysonSerializationSettings.AddTypeOverride(new JaysonTypeOverride<TypedContainerDto>()
                .SetDefaultValue("Guid3", Guid.Empty)
                .SetDefaultValue("Null2", JaysonNull.Value));

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            Assert.True(!json.Contains("\"Enum3\":"));
            Assert.True(!json.Contains("\"Null1\":"));
            Assert.True(!json.Contains("\"Null2\":"));

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.UseDefaultValues = true;
            jaysonDeserializationSettings.AddTypeOverride(new JaysonTypeOverride<TypedContainerDto>()
                .SetDefaultValue("Guid3", defaultGuid));

            TypedContainerDto dto2 = JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dto2);
            Assert.AreEqual(dto2.Guid3, defaultGuid);

            dto2.Guid3 = Guid.Empty;
            CompareTypedContainerDtos(dto1, dto2);
        }

        [Test]
        public static void TestIgnoreAndUseDefaultValues1c()
        {
            var dto1 = TestClasses.GetTypedContainerDto() as TypedContainerDto;

            var defaultGuid = new Guid("{4B2AFF22-0CD6-472D-A830-258C57DE8AD3}");

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.UseKVModelForISerializable = true;
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreDefaultValues = true;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";
            jaysonSerializationSettings.AddTypeOverride(new JaysonTypeOverride<TypedContainerDto>()
                .SetDefaultValue("Guid3", Guid.Empty));

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            Assert.True(!json.Contains("\"EmptyList2\":"));

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.UseDefaultValues = true;
            jaysonDeserializationSettings.AddTypeOverride(new JaysonTypeOverride<TypedContainerDto>()
                .SetDefaultValue("Guid3", defaultGuid));

            TypedContainerDto dto2 = JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dto2);
            Assert.AreEqual(dto2.Guid3, defaultGuid);

            dto2.Guid3 = Guid.Empty;
            CompareTypedContainerDtos(dto1, dto2);
        }

        [Test]
        public static void TestISerializable1a()
        {
            var dto1 = TestClasses.GetTypedContainerDto() as TypedContainerDto;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.UseKVModelForISerializable = true;
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            Assert.True(json.Contains("\"$ctx\":"));

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            TypedContainerDto dto2 = JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dto2);
            CompareTypedContainerDtos(dto1, dto2);
        }

        [Test]
        public static void TestISerializable1b()
        {
            var dto1 = TestClasses.GetTypedContainerDto() as TypedContainerDto;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.UseKVModelForISerializable = false;
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = true;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            Assert.True(!json.Contains("\"$ctx\":"));

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = true;

            TypedContainerDto dto2 = JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dto2);
            CompareTypedContainerDtos(dto1, dto2);
        }

        [Test]
        public static void TestISerializable1c()
        {
            var dto1 = TestClasses.GetTypedContainerDto() as TypedContainerDto;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.UseKVModelForISerializable = false; // Fails on ISerializable objects if CaseSensitive is FALSE and UseKVModelForISerializable is FALSE
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false; // Fails on ISerializable objects if CaseSensitive is FALSE and UseKVModelForISerializable is FALSE
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            Assert.True(!json.Contains("\"$ctx\":"));

            try
            {
                JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
                jaysonDeserializationSettings.CaseSensitive = false;

                TypedContainerDto dto2 = JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);

                Assert.IsNotNull(dto2);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(System.Runtime.Serialization.SerializationException), e);
            }
        }

        [Test]
        public static void TestComment1a()
        {
            var jsonObj = JaysonConverter.ToDictionary("// abc\r\n{\"a\":\"1\\\"\"}");
            Assert.IsTrue(jsonObj is IDictionary<string, object>);
        }

        [Test]
        public static void TestComment1b()
        {
            var jsonObj = JaysonConverter.ToDictionary("/* abc\r\ndef*/  {\"a\":\"1\\\"\"}");
            Assert.IsTrue(jsonObj is IDictionary<string, object>);
        }

        [Test]
        public static void TestComment1c()
        {
            var jsonObj = JaysonConverter.ToDictionary("{/* abc\r\ndef*/\"a\"://xyz\r\n\"1\\\"\"}");
            Assert.IsTrue(jsonObj is IDictionary<string, object>);
        }

        [Test]
        public static void TestComment1d()
        {
            var jsonObj = JaysonConverter.ToObject("/**/");
            Assert.IsNull(jsonObj);
        }

        [Test]
        public static void TestComment1e()
        {
            var jsonObj = JaysonConverter.ToObject("/* xyz */");
            Assert.IsNull(jsonObj);
        }

        [Test]
        public static void TestComment1f()
        {
            var jsonObj = JaysonConverter.ToObject("//");
            Assert.IsNull(jsonObj);
        }

        [Test]
        public static void TestComment1g()
        {
            var jsonObj = JaysonConverter.ToObject("// xyz ");
            Assert.IsNull(jsonObj);
        }

        [Test]
        public static void TestComment1h()
        {
            var jsonObj = JaysonConverter.ToObject("// xyz \r\n1");
            Assert.AreEqual(jsonObj, 1);
        }

        [Test]
        public static void TestComment1i()
        {
            var jsonObj = JaysonConverter.ToObject("// xyz \r\n\"1\"");
            Assert.AreEqual(jsonObj, "1");
        }

        [Test]
        public static void TestComment1j()
        {
            var jsonObj = JaysonConverter.ToObject("[1, // xyz \r\n,\"2\"]");
            Assert.IsInstanceOf(typeof(IList), jsonObj);

            IList list = (IList)jsonObj;
            Assert.IsTrue(list.Count == 2);
            Assert.AreEqual(list[0], 1);
            Assert.AreEqual(list[1], "2");
        }

        [Test]
        public static void TestComment2a()
        {
            try
            {
                JaysonDeserializationSettings settings = JaysonDeserializationSettings.DefaultClone();
                settings.CommentHandling = JaysonCommentHandling.ThrowError;

                JaysonConverter.ToDictionary("// abc\r\n{\"a\":\"1\\\"\"}", settings);
            }
            catch (JaysonException e)
            {
                Assert.IsTrue(e.Message == JaysonError.InvalidJson);
            }
        }

        [Test]
        public static void TestComment2b()
        {
            try
            {
                JaysonDeserializationSettings settings = JaysonDeserializationSettings.DefaultClone();
                settings.CommentHandling = JaysonCommentHandling.ThrowError;

                JaysonConverter.ToDictionary("/* abc\r\ndef*/  {\"a\":\"1\\\"\"}", settings);
            }
            catch (JaysonException e)
            {
                Assert.IsTrue(e.Message == JaysonError.InvalidJson);
            }
        }

        [Test]
        public static void TestEnumNames1()
        {
            var dto1 = TestClasses.GetTypedContainerDto() as TypedContainerDto;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.UseGlobalTypeNames = true;
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = true;
            jaysonSerializationSettings.UseEnumNames = true;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            object jsonObj = JaysonConverter.Parse(json, jaysonDeserializationSettings);
            TypedContainerDto dto2 =
                JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(jsonObj);
            CompareTypedContainerDtos(dto1, dto2);
        }

        [Test]
        public static void TestEnumNames2()
        {
            var dto1 = TestClasses.GetTypedContainerDto() as TypedContainerDto;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.UseGlobalTypeNames = true;
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = true;
            jaysonSerializationSettings.UseEnumNames = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            object jsonObj = JaysonConverter.Parse(json, jaysonDeserializationSettings);
            TypedContainerDto dto2 = JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(jsonObj);
            CompareTypedContainerDtos(dto1, dto2);
        }

        [Test]
        public static void TestEscapeChars1()
        {
            var jsonObj = JaysonConverter.ToDictionary("{\"a\":\"1\\\"\"}");
            Assert.IsTrue(jsonObj is IDictionary<string, object>);
        }

        [Test]
        public static void TestMethodInfo1()
        {
            var m1 = typeof(TestClasses).GetMethod("GetA");

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.UseKVModelForJsonObjects = false;

            var jsonObj = JaysonConverter.ToJsonObject(m1, jaysonSerializationSettings);

            Assert.IsTrue(jsonObj is IDictionary<string, object>);
        }

        [Test]
        public static void TestMethodInfo2()
        {
            var m1 = typeof(TestClasses).GetMethod("GetA");

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.UseKVModelForJsonObjects = false;

            var json = JaysonConverter.ToJsonString(m1, jaysonSerializationSettings);

            Assert.IsTrue(json.Contains("\"QualifiedName\":\"Sweet.Jayson.Tests.TestClasses, Sweet.Jayson.Tests\""));
            Assert.IsTrue(json.Contains("\"MemberName\":\"GetA\""));
        }

        [Test]
        public static void TestMethodInfo3()
        {
            var m1 = typeof(TestClasses).GetMethod("MethodA", BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.UseKVModelForJsonObjects = false;

            var json = JaysonConverter.ToJsonString(m1, jaysonSerializationSettings);

            Assert.IsTrue(json.Contains("\"QualifiedName\":\"Sweet.Jayson.Tests.TestClasses, Sweet.Jayson.Tests\""));
            Assert.IsTrue(json.Contains("\"MemberName\":\"MethodA\""));

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();

            var m2 = JaysonConverter.ToObject<MethodInfo>(json, jaysonDeserializationSettings);

            Assert.AreEqual(m1, m2);
        }

        [Test]
        public static void TestMethodInfo4()
        {
            var c1 = typeof(CustomException).GetConstructor(new Type[] { typeof(string), typeof(Exception) });

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.UseKVModelForJsonObjects = true;

            var json = JaysonConverter.ToJsonString(c1, jaysonSerializationSettings);

            Assert.IsTrue(json.Contains("\"QualifiedName\":\"Sweet.Jayson.Tests.CustomException, Sweet.Jayson.Tests\""));

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();

            var c2 = JaysonConverter.ToObject<ConstructorInfo>(json, jaysonDeserializationSettings);

            Assert.AreEqual(c1, c2);
        }

        [Test]
        public static void TestObjectGraph1()
        {
            var o1 = TestClasses.GetObjectGraph1();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.UseKVModelForJsonObjects = true;

            var json = JaysonConverter.ToJsonString(o1, jaysonSerializationSettings);

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();

            var o2 = JaysonConverter.ToObject<ObjectGraph>(json, jaysonDeserializationSettings);

            Assert.NotNull(o2);

            Assert.AreEqual(o1.AddressUri, o2.AddressUri);
            Assert.AreEqual(o1.IntValue, o2.IntValue);
            Assert.AreEqual(o1.SomeType, o2.SomeType);

            Assert.IsNotNull(o2.ObjectData);
            Assert.IsAssignableFrom<object[]>(o2.ObjectData);

            object[] od1 = (object[])o1.ObjectData;
            object[] od2 = (object[])o2.ObjectData;

            Assert.AreEqual(od2.Length, 2);
            Assert.IsTrue(od2[0] is ConstructorInfo);
            Assert.IsTrue(od2[1] is PropertyInfo);
            Assert.AreEqual(od1[0], od2[0]);
            Assert.AreEqual(od1[1], od2[1]);

            Assert.IsNotNull(o2.Data);
            Assert.IsTrue(o1.Data.Exception is ArgumentNullException);
            Assert.IsTrue(o2.Data.Exception is ArgumentNullException);
            Assert.AreEqual(((ArgumentNullException)o1.Data.Exception).ParamName, ((ArgumentNullException)o2.Data.Exception).ParamName);
            Assert.AreEqual(o1.Data.Identifier, o2.Data.Identifier);
            Assert.AreEqual(o1.Data.Object, o2.Data.Object);
            Assert.AreEqual(o1.Data.Type, o2.Data.Type);
            Assert.AreEqual(o1.Data.TypeList, o2.Data.TypeList);
            Assert.AreEqual(o1.Data.TS, o2.Data.TS);

            Assert.IsNotNull(o1.MyCollection);
            Assert.IsNotNull(o2.MyCollection);
            Assert.AreEqual(o1.MyCollection.Count, o2.MyCollection.Count);

            for (int i = 0; i < o2.MyCollection.Count; i++)
            {
                Assert.AreEqual(o1.MyCollection[i].Name, o2.MyCollection[i].Name);
                Assert.AreEqual(o1.MyCollection[i].Value, o2.MyCollection[i].Value);
            }
        }

        [Test]
        public static void TestObjectGraph2()
        {
            var o1 = TestClasses.GetObjectGraph1();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.UseKVModelForJsonObjects = true;

            var jsonObj = JaysonConverter.ToJsonObject(o1, jaysonSerializationSettings);

            Assert.IsTrue(jsonObj is IDictionary<string, object>);
        }

        [Test]
        public static void TestToJsonObjectUseKVModelForJsonObjects1()
        {
            var a1 = TestClasses.GetA();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.UseKVModelForJsonObjects = false;

            var jsonObj = JaysonConverter.ToJsonObject(a1, jaysonSerializationSettings);

            Assert.IsTrue(jsonObj is IDictionary<string, object>);

            var a2 = (IDictionary<string, object>)jsonObj;

            foreach (var pi in a1.GetType().GetProperties())
            {
                if (pi.Name == "L2")
                {
                    var l21 = pi.GetValue(a1, new object[0]) as IList;
                    var l22 = a2["L2"] as IList;

                    Assert.IsNotNull(l21);
                    Assert.IsNotNull(l22);
                    Assert.AreEqual(l21.Count, l22.Count);

                    for (int i = 0; i < l21.Count; i++)
                    {
                        Assert.AreEqual(l21[i], l22[i]);
                    }
                }
                else
                {
                    var val = pi.GetValue(a1, new object[0]);
                    if (val != null && a2.ContainsKey(pi.Name))
                    {
                        Assert.AreEqual(val, a2[pi.Name]);
                    }
                }
            }

            foreach (var fi in a1.GetType().GetFields())
            {
                if (fi.Name == "D3")
                {
                    var d31 = fi.GetValue(a1) as IDictionary<object, object>;
                    var d32 = a2["D3"] as IDictionary<string, object>;

                    Assert.IsNotNull(d31);
                    Assert.IsNotNull(d32);
                    Assert.AreEqual(d31.Count, d32.Count);

                    foreach (var kvp in d31)
                    {
                        Assert.IsTrue(d32.ContainsKey(kvp.Key.ToString()));
                        Assert.AreEqual(kvp.Value, d32[kvp.Key.ToString()]);
                    }
                }
                else
                {
                    var val = fi.GetValue(a1);
                    if (val != null && a2.ContainsKey(fi.Name))
                    {
                        Assert.AreEqual(val, a2[fi.Name]);
                    }
                }
            }
        }

        [Test]
        public static void TestToJsonObjectUseKVModelForJsonObjects2()
        {
            var a1 = TestClasses.GetA();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.UseKVModelForJsonObjects = true;

            var jsonObj = JaysonConverter.ToJsonObject(a1, jaysonSerializationSettings);

            Assert.IsTrue(jsonObj is IDictionary<string, object>);

            var a2 = (IDictionary<string, object>)jsonObj;

            foreach (var pi in a1.GetType().GetProperties())
            {
                if (pi.Name == "L2")
                {
                    var l21 = pi.GetValue(a1, new object[0]) as IList;
                    var l22 = a2["L2"] as IList;

                    Assert.IsNotNull(l21);
                    Assert.IsNotNull(l22);
                    Assert.AreEqual(l21.Count, l22.Count);

                    for (int i = 0; i < l21.Count; i++)
                    {
                        Assert.AreEqual(l21[i], l22[i]);
                    }
                }
                else
                {
                    var val = pi.GetValue(a1, new object[0]);
                    if (val != null && a2.ContainsKey(pi.Name))
                    {
                        Assert.AreEqual(val, a2[pi.Name]);
                    }
                }
            }

            foreach (var fi in a1.GetType().GetFields())
            {
                if (fi.Name == "D3")
                {
                    var d31 = fi.GetValue(a1) as IDictionary<object, object>;
                    var d32 = a2["D3"] as IDictionary<string, object>;

                    Assert.IsNotNull(d31);
                    Assert.IsNotNull(d32);

                    Assert.AreEqual(d32.Count, 1);
                    Assert.IsTrue(d32.ContainsKey("$kv"));

                    var kv = d32["$kv"] as List<object>;
                    Assert.IsNotNull(kv);
                    Assert.AreEqual(d31.Count, kv.Count);

                    foreach (var kvp in d31)
                    {
                        Assert.IsNotNull(kv.FirstOrDefault(item => (item as IDictionary<string, object>)["$k"].Equals(kvp.Key) &&
                            (item as IDictionary<string, object>)["$v"].Equals(kvp.Value)));
                    }
                }
                else
                {
                    var val = fi.GetValue(a1);
                    if (val != null && a2.ContainsKey(fi.Name))
                    {
                        Assert.AreEqual(val, a2[fi.Name]);
                    }
                }
            }
        }

        [Test]
        public static void TestUseKVModelForJsonObjects1a()
        {
            var a1 = TestClasses.GetA();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.UseKVModelForJsonObjects = false;

            string json = JaysonConverter.ToJsonString(a1, jaysonSerializationSettings);

            Assert.IsTrue(!json.Contains("$kv"));
        }

        [Test]
        public static void TestUseKVModelForJsonObjects1b()
        {
            var a1 = TestClasses.GetA();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.UseKVModelForJsonObjects = true;

            string json = JaysonConverter.ToJsonString(a1, jaysonSerializationSettings);

            Assert.IsTrue(json.Contains("$kv"));
        }

        [Test]
        public static void TestUseKVModelForJsonObjects2()
        {
            var a1 = TestClasses.GetA();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.UseKVModelForJsonObjects = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();

            string json = JaysonConverter.ToJsonString(a1, jaysonSerializationSettings);
            var a2 = JaysonConverter.ToObject<A>(json, jaysonDeserializationSettings);

            Assert.IsTrue(!json.Contains("$kv"));

            Assert.IsNotNull(a2);
            Assert.AreEqual(a1.D1, a2.D1);
            Assert.AreEqual(a1.D2, a2.D2);

            Assert.AreEqual(a1.D3.Count, a2.D3.Count);
            foreach (var kvp in a1.D3)
            {
                Assert.IsTrue(a2.D3.ContainsKey(kvp.Key.ToString()));
                Assert.AreEqual(kvp.Value, a2.D3[kvp.Key.ToString()]);
            }

            Assert.AreEqual(a1.E1, a2.E1);
            Assert.AreEqual(a1.I1, a2.I1);
            Assert.AreEqual(a1.L1, a2.L1);

            Assert.AreEqual(a1.L2.Count, a2.L2.Count);
            for (int i = 0; i < a1.L2.Count; i++)
            {
                Assert.AreEqual(a1.L2[i], a2.L2[i]);
            }

            Assert.AreEqual(a1.O1, a2.O1);
            Assert.AreEqual(a1.O2, a2.O2);
        }

        [Test]
        public static void TestA1()
        {
            var a1 = TestClasses.GetA();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.UseKVModelForJsonObjects = true;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();

            string json = JaysonConverter.ToJsonString(a1, jaysonSerializationSettings);
            var a2 = JaysonConverter.ToObject<A>(json, jaysonDeserializationSettings);

            Assert.IsTrue(json.Contains("$kv"));

            Assert.IsNotNull(a2);
            Assert.AreEqual(a1.D1, a2.D1);
            Assert.AreEqual(a1.D2, a2.D2);

            Assert.AreEqual(a1.D3.Count, a2.D3.Count);
            foreach (var kvp in a1.D3)
            {
                Assert.IsTrue(a2.D3.ContainsKey(kvp.Key));
                Assert.AreEqual(kvp.Value, a2.D3[kvp.Key]);
            }

            Assert.AreEqual(a1.E1, a2.E1);
            Assert.AreEqual(a1.I1, a2.I1);
            Assert.AreEqual(a1.L1, a2.L1);

            Assert.AreEqual(a1.L2.Count, a2.L2.Count);
            for (int i = 0; i < a1.L2.Count; i++)
            {
                Assert.AreEqual(a1.L2[i], a2.L2[i]);
            }

            Assert.AreEqual(a1.O1, a2.O1);
            Assert.AreEqual(a1.O2, a2.O2);
        }

        [Test]
        public static void TestA2()
        {
            var a1 = TestClasses.GetA2();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.UseObjectReferencing = true;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();

            string json = JaysonConverter.ToJsonString(a1, jaysonSerializationSettings);
            var a2 = JaysonConverter.ToObject<A>(json, jaysonDeserializationSettings);

            Assert.IsTrue(json.Contains("$kv"));

            Assert.IsNotNull(a2);
            Assert.AreEqual(a1.D1, a2.D1);
            Assert.AreEqual(a1.D2, a2.D2);

            Assert.IsNotNull(a2.D3);
            Assert.AreEqual(a1.D3.Count, a2.D3.Count);

            Assert.IsTrue(a2.D3.ContainsKey("#self"));
            Assert.IsTrue(ReferenceEquals(a2.D3, a2.D3["#self"]));

            Assert.IsTrue(a2.D3.ContainsKey("#list"));
            Assert.IsTrue(a2.D3["#list"] is IList);
            Assert.AreEqual(((IList)a2.D3["#list"]).Count, 1);
            Assert.IsTrue(ReferenceEquals(a2.D3, ((IList)a2.D3["#list"])[0]));

            foreach (var kvp in a1.D3)
            {
                if (!(kvp.Key.Equals("#self") || kvp.Key.Equals("#list")))
                {
                    Assert.IsTrue(a2.D3.ContainsKey(kvp.Key));
                    Assert.AreEqual(kvp.Value, a2.D3[kvp.Key]);
                }
            }

            Assert.AreEqual(a1.E1, a2.E1);
            Assert.AreEqual(a1.I1, a2.I1);
            Assert.AreEqual(a1.L1, a2.L1);

            Assert.AreEqual(a1.L2.Count, a2.L2.Count);
            for (int i = 0; i < a1.L2.Count; i++)
            {
                Assert.AreEqual(a1.L2[i], a2.L2[i]);
            }

            Assert.AreEqual(a1.O1, a2.O1);

            Assert.IsNotNull(a2.O2);
            Assert.IsTrue(ReferenceEquals(a2.O2, a2.D3));
        }

        [Test]
        public static void TestA3()
        {
            var a1 = TestClasses.GetA3();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.UseObjectReferencing = true;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();

            string json = JaysonConverter.ToJsonString(a1, jaysonSerializationSettings);
            var a2 = JaysonConverter.ToObject<A>(json, jaysonDeserializationSettings);

            Assert.IsTrue(!json.Contains("$kv"));

            Assert.IsNotNull(a2);
            Assert.AreEqual(a1.D1, a2.D1);
            Assert.AreEqual(a1.D2, a2.D2);

            Assert.IsNull(a2.D3);
            Assert.IsNotNull(a2.D4);
            Assert.AreEqual(a1.D4.Count, a2.D4.Count);

            Assert.IsTrue(a2.D4.ContainsKey("#self"));
            Assert.IsTrue(ReferenceEquals(a2.D4, a2.D4["#self"]));

            Assert.IsTrue(a2.D4.ContainsKey("#list"));
            Assert.IsTrue(a2.D4["#list"] is IList);
            Assert.AreEqual(((IList)a2.D4["#list"]).Count, 1);
            Assert.IsTrue(ReferenceEquals(a2.D4, ((IList)a2.D4["#list"])[0]));

            foreach (var kvp in a1.D4)
            {
                if (!(kvp.Key.Equals("#self") || kvp.Key.Equals("#list")))
                {
                    Assert.IsTrue(a2.D4.ContainsKey(kvp.Key));
                    Assert.AreEqual(kvp.Value, a2.D4[kvp.Key]);
                }
            }

            Assert.AreEqual(a1.E1, a2.E1);
            Assert.AreEqual(a1.I1, a2.I1);
            Assert.AreEqual(a1.L1, a2.L1);

            Assert.AreEqual(a1.L2.Count, a2.L2.Count);
            for (int i = 0; i < a1.L2.Count; i++)
            {
                Assert.AreEqual(a1.L2[i], a2.L2[i]);
            }

            Assert.AreEqual(a1.O1, a2.O1);

            Assert.IsNotNull(a2.O2);
            Assert.IsTrue(ReferenceEquals(a2.O2, a2.D4));
        }

        [Test]
        public static void TestNullList1()
        {
            var l1 = new List<object>();
            l1.Add(null);
            l1.Add(33);
            l1.Add(12345m);
            l1.Add(23456m);
            l1.Add("abcdefg");
            l1.Add("wxyz");
            l1.Add(false);
            l1.Add(true);

            string json = JaysonConverter.ToJsonString(l1);
            var l2 = JaysonConverter.ToObject<List<object>>(json);

            Assert.IsNotNull(l2);
            Assert.AreEqual(l1.Count, l2.Count);

            for (int i = l1.Count - 1; i > -1; i--)
            {
                Assert.AreEqual(l1[i], l2[i]);
            }
        }

        [Test]
        public static void TestNullList2()
        {
            var l1 = new List<object>();
            l1.Add(true);
            l1.Add(33);
            l1.Add(12345m);
            l1.Add(23456m);
            l1.Add("abcdefg");
            l1.Add("wxyz");
            l1.Add(false);
            l1.Add(null);

            string json = JaysonConverter.ToJsonString(l1);
            var l2 = JaysonConverter.ToObject<List<object>>(json);

            Assert.IsNotNull(l2);
            Assert.AreEqual(l1.Count, l2.Count);

            for (int i = l1.Count - 1; i > -1; i--)
            {
                Assert.AreEqual(l1[i], l2[i]);
            }
        }

        [Test]
        public static void TestNullList3()
        {
            var l1 = new List<object>();
            l1.Add(false);
            l1.Add(33);
            l1.Add(12345m);
            l1.Add(23456m);
            l1.Add("abcdefg");
            l1.Add("wxyz");
            l1.Add(null);
            l1.Add(false);

            string json = JaysonConverter.ToJsonString(l1);
            var l2 = JaysonConverter.ToObject<List<object>>(json);

            Assert.IsNotNull(l2);
            Assert.AreEqual(l1.Count, l2.Count);

            for (int i = l1.Count - 1; i > -1; i--)
            {
                Assert.AreEqual(l1[i], l2[i]);
            }
        }

        [Test]
        public static void TestStack1()
        {
            var s1 = new Stack();
            s1.Push(2);
            s1.Push(33);
            s1.Push(12345m);
            s1.Push(23456m);
            s1.Push("abcdefg");
            s1.Push("wxyz");
            s1.Push(true);

            string json = JaysonConverter.ToJsonString(s1);
            var s2 = JaysonConverter.ToObject<Stack>(json);

            Assert.IsNotNull(s2);
            Assert.AreEqual(s1.Count, s2.Count);

            for (int i = s1.Count - 1; i > -1; i--)
            {
                Assert.AreEqual(s1.Pop(), s2.Pop());
            }
        }

        [Test]
        public static void TestStack2()
        {
            var s1 = new Stack<int>();
            s1.Push(2);
            s1.Push(33);
            s1.Push(12345);
            s1.Push(23456);

            string json = JaysonConverter.ToJsonString(s1);
            var s2 = JaysonConverter.ToObject<Stack<int>>(json);

            Assert.IsNotNull(s2);
            Assert.AreEqual(s1.Count, s2.Count);

            for (int i = s1.Count - 1; i > -1; i--)
            {
                Assert.AreEqual(s1.Pop(), s2.Pop());
            }
        }

#if !(NET3500 || NET3000 || NET2000)
        [Test]
        public static void TestStack3()
        {
            var s1 = new ConcurrentStack<int>();
            s1.Push(2);
            s1.Push(33);
            s1.Push(12345);
            s1.Push(23456);

            string json = JaysonConverter.ToJsonString(s1);
            var s2 = JaysonConverter.ToObject<ConcurrentStack<int>>(json);

            Assert.IsNotNull(s2);
            Assert.AreEqual(s1.Count, s2.Count);

            int i1, i2;
            for (int i = s1.Count - 1; i > -1; i--)
            {
                Assert.AreEqual(s1.TryPop(out i1), s2.TryPop(out i2));
                Assert.AreEqual(i1, i2);
            }
        }
#endif

        [Test]
        public static void TestQueue1()
        {
            var q1 = new Queue();
            q1.Enqueue(2);
            q1.Enqueue(33);
            q1.Enqueue(12345m);
            q1.Enqueue(23456m);
            q1.Enqueue("abcdefg");
            q1.Enqueue("wxyz");
            q1.Enqueue(true);

            string json = JaysonConverter.ToJsonString(q1);
            var q2 = JaysonConverter.ToObject<Queue>(json);

            Assert.IsNotNull(q2);
            Assert.AreEqual(q1.Count, q2.Count);

            for (int i = q1.Count - 1; i > -1; i--)
            {
                Assert.AreEqual(q1.Dequeue(), q2.Dequeue());
            }
        }

        [Test]
        public static void TestQueue2()
        {
            var q1 = new Queue<int>();
            q1.Enqueue(2);
            q1.Enqueue(33);
            q1.Enqueue(12345);
            q1.Enqueue(23456);

            string json = JaysonConverter.ToJsonString(q1);
            var q2 = JaysonConverter.ToObject<Queue<int>>(json);

            Assert.IsNotNull(q2);
            Assert.AreEqual(q1.Count, q2.Count);

            for (int i = q1.Count - 1; i > -1; i--)
            {
                Assert.AreEqual(q1.Dequeue(), q2.Dequeue());
            }
        }

#if !(NET3500 || NET3000 || NET2000)
        [Test]
        public static void TestQueue3()
        {
            var q1 = new ConcurrentQueue<int>();
            q1.Enqueue(2);
            q1.Enqueue(33);
            q1.Enqueue(12345);
            q1.Enqueue(23456);

            string json = JaysonConverter.ToJsonString(q1);
            var q2 = JaysonConverter.ToObject<ConcurrentQueue<int>>(json);

            Assert.IsNotNull(q2);
            Assert.AreEqual(q1.Count, q2.Count);

            int i1, i2;
            for (int i = q1.Count - 1; i > -1; i--)
            {
                Assert.AreEqual(q1.TryDequeue(out i1), q2.TryDequeue(out i2));
                Assert.AreEqual(i1, i2);
            }
        }
#endif

#if !(NET3500 || NET3000 || NET2000)
        [Test]
        public static void TestConcurrentBag()
        {
            var b1 = new ConcurrentBag<int>();
            b1.Add(2);
            b1.Add(33);
            b1.Add(12345);
            b1.Add(23456);

            string json = JaysonConverter.ToJsonString(b1);
            var b2 = JaysonConverter.ToObject<ConcurrentBag<int>>(json);

            Assert.IsNotNull(b2);
            Assert.AreEqual(b1.Count, b2.Count);

            int i1, i2;
            for (int i = b1.Count - 1; i > -1; i--)
            {
                Assert.AreEqual(b1.TryTake(out i1), b2.TryTake(out i2));
                Assert.AreEqual(i1, i2);
            }
        }
#endif

        [Test]
        public static void TestStruct1()
        {
            var s1 = new SampleStructDto1();
            s1.I1 = 2;
            s1.I2 = 33;
            s1.D1 = 12345m;
            s1.D2 = 23456m;
            s1.S1 = "abcdefg";
            s1.S2 = "wxyz";

            string json = JaysonConverter.ToJsonString(s1);
            var s2 = JaysonConverter.ToObject<SampleStructDto1>(json);

            Assert.IsNotNull(s2);
            Assert.AreEqual(s1.I1, s2.I1);
            Assert.AreEqual(s1.I2, s2.I2);
            Assert.AreEqual(s1.D1, s2.D1);
            Assert.AreEqual(s1.D2, s2.D2);
            Assert.AreEqual(s1.S1, s2.S1);
            Assert.AreEqual(s1.S2, s2.S2);
        }

        [Test]
        public static void TestStruct2()
        {
            var s1 = new SampleStructDto2(2, 33);
            s1.D1 = 12345m;
            s1.D2 = 23456m;
            s1.S1 = "abcdefg";
            s1.S2 = "wxyz";

            string json = JaysonConverter.ToJsonString(s1);
            var s2 = JaysonConverter.ToObject<SampleStructDto2>(json);

            Assert.IsNotNull(s2);
            Assert.AreEqual(s1.I1, s2.I1);
            Assert.AreEqual(s1.I2, s2.I2);
            Assert.AreEqual(s1.D1, s2.D1);
            Assert.AreEqual(s1.D2, s2.D2);
            Assert.AreEqual(s1.S1, s2.S1);
            Assert.AreEqual(s1.S2, s2.S2);
        }

#if !(NET3500 || NET3000 || NET2000)
        [Test]
        public static void TestTuple1()
        {
            var t1 = new Tuple<int, int, int>(2, 33, 44);
            string json = JaysonConverter.ToJsonString(t1);
            var t2 = JaysonConverter.ToObject<Tuple<int, int, int>>(json);

            Assert.IsNotNull(t2);
            Assert.AreEqual(t1.Item1, t2.Item1);
            Assert.AreEqual(t1.Item2, t2.Item2);
            Assert.AreEqual(t1.Item3, t2.Item3);
        }

        [Test]
        public static void TestTuple2a()
        {
            var t1 = new Tuple<int, int, int>(2, 33, 44);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();

            string json = JaysonConverter.ToJsonString(t1, jaysonSerializationSettings);
            var t2 = JaysonConverter.ToObject<Tuple<int, int, int>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(t2);
            Assert.AreEqual(t1.Item1, t2.Item1);
            Assert.AreEqual(t1.Item2, t2.Item2);
            Assert.AreEqual(t1.Item3, t2.Item3);
        }

        [Test]
        public static void TestTuple2b()
        {
            var t1 = new Tuple<int, int?, Tuple<int>>(2, null, Tuple.Create(44));

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();

            string json = JaysonConverter.ToJsonString(t1, jaysonSerializationSettings);
            var t2 = JaysonConverter.ToObject<Tuple<int, int?, Tuple<int>>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(t2);
            Assert.AreEqual(t1.Item1, t2.Item1);
            Assert.AreEqual(t1.Item2, t2.Item2);
            Assert.AreEqual(t1.Item3, t2.Item3);
        }

        [Test]
        public static void TestTuple2c()
        {
            var t1 = new Tuple<int, int?, Tuple<int>>(2, null, Tuple.Create(44));

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();

            string json = JaysonConverter.ToJsonString(t1, jaysonSerializationSettings);
            var t2 = (Tuple<int, int?, Tuple<int>>)JaysonConverter.ToObject(json, jaysonDeserializationSettings);

            Assert.IsNotNull(t2);
            Assert.AreEqual(t1.Item1, t2.Item1);
            Assert.AreEqual(t1.Item2, t2.Item2);
            Assert.AreEqual(t1.Item3, t2.Item3);
        }

        [Test]
        public static void TestTuple2d()
        {
            var t1 = new Tuple<int, int?, Tuple<int>>(2, 3, Tuple.Create(44));

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();

            string json = JaysonConverter.ToJsonString(t1, jaysonSerializationSettings);
            var t2 = (Tuple<int, int?, Tuple<int>>)JaysonConverter.ToObject(json, jaysonDeserializationSettings);

            Assert.IsNotNull(t2);
            Assert.AreEqual(t1.Item1, t2.Item1);
            Assert.AreEqual(t1.Item2, t2.Item2);
            Assert.AreEqual(t1.Item3, t2.Item3);
        }

        [Test]
        public static void TestTuple2e()
        {
            var t1 = new Tuple<int, int?, Tuple<int>>(2, null, Tuple.Create(44));

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();

            string json = JaysonConverter.ToJsonString(t1, jaysonSerializationSettings);
            var t2 = JaysonConverter.ToObject<Tuple<int, int?, Tuple<int>>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(t2);
            Assert.AreEqual(t1.Item1, t2.Item1);
            Assert.AreEqual(t1.Item2, t2.Item2);
            Assert.AreEqual(t1.Item3, t2.Item3);
        }

        [Test]
        public static void TestTuple3a()
        {
            var t1 = new Tuple<int, int?, Tuple<bool?, decimal>>(2, null, new Tuple<bool?, decimal>(null, 12345.67890987654m));

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CtorParamMatcher = (paramName, obj) =>
            {
                return obj.FirstOrDefault(kvp =>
                    paramName.Equals(kvp.Key.Replace("_", ""), StringComparison.OrdinalIgnoreCase)).Value;
            };

            string json = JaysonConverter.ToJsonString(t1, jaysonSerializationSettings);
            var t2 = JaysonConverter.ToObject<Tuple<int, int?, Tuple<bool?, decimal>>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(t2);
            Assert.AreEqual(t1.Item1, t2.Item1);
            Assert.AreEqual(t1.Item2, t2.Item2);
            Assert.IsNotNull(t2.Item3);
            Assert.AreEqual(t1.Item3.Item1, t2.Item3.Item1);
            Assert.AreEqual(t1.Item3.Item2, t2.Item3.Item2);
        }

        [Test]
        public static void TestTuple3b()
        {
            var t1 = new Tuple<int, int?, Tuple<bool?, decimal>>(2, null, new Tuple<bool?, decimal>(null, 12345.67890987654m));

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ObjectActivator = delegate(Type objType,
                IDictionary<string, object> parsedObject,
                out bool useDefaultCtor)
            {
                useDefaultCtor = true;

                if (objType == typeof(Tuple<int, int?, Tuple<bool?, decimal>>))
                {
                    useDefaultCtor = false;

                    int item1 = 0;
                    int? item2 = null;
                    Tuple<bool?, decimal> item3 = null;

                    object obj;
                    if (parsedObject.TryGetValue("Item1", out obj))
                    {
                        item1 = JaysonConverter.ConvertJsonObject<int>(obj, jaysonDeserializationSettings);
                    }
                    if (parsedObject.TryGetValue("Item2", out obj))
                    {
                        item2 = JaysonConverter.ConvertJsonObject<int?>(obj, jaysonDeserializationSettings);
                    }
                    if (parsedObject.TryGetValue("Item3", out obj))
                    {
                        item3 = JaysonConverter.ConvertJsonObject<Tuple<bool?, decimal>>(obj, jaysonDeserializationSettings);
                    }

                    return new Tuple<int, int?, Tuple<bool?, decimal>>(item1, item2, item3);
                }

                if (objType == typeof(Tuple<bool?, decimal>))
                {
                    useDefaultCtor = false;

                    bool? item1 = null;
                    decimal item2 = 0m;

                    object obj;
                    if (parsedObject.TryGetValue("Item1", out obj))
                    {
                        item1 = JaysonConverter.ConvertJsonObject<bool?>(obj, jaysonDeserializationSettings);
                    }
                    if (parsedObject.TryGetValue("Item2", out obj))
                    {
                        item2 = JaysonConverter.ConvertJsonObject<decimal>(obj, jaysonDeserializationSettings);
                    }
                    return new Tuple<bool?, decimal>(item1, item2);
                }

                return null;
            };

            string json = JaysonConverter.ToJsonString(t1, jaysonSerializationSettings);
            var t2 = JaysonConverter.ToObject<Tuple<int, int?, Tuple<bool?, decimal>>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(t2);
            Assert.AreEqual(t1.Item1, t2.Item1);
            Assert.AreEqual(t1.Item2, t2.Item2);
            Assert.IsNotNull(t2.Item3);
            Assert.AreEqual(t1.Item3.Item1, t2.Item3.Item1);
            Assert.AreEqual(t1.Item3.Item2, t2.Item3.Item2);
        }
#endif

        [Test]
        public static void TestTimeSpan1()
        {
            var ts1 = new TimeSpan(2, 33, 44);
            string json = JaysonConverter.ToJsonString(ts1);
            var ts2 = JaysonConverter.ToObject<TimeSpan>(json);

            Assert.AreEqual(ts1.Ticks, ts2.Ticks);
        }

        [Test]
        public static void TestTimeSpan2()
        {
            var ts1 = new TimeSpan(1, 2, 33, 44, 555);
            string json = JaysonConverter.ToJsonString(ts1);
            var ts2 = JaysonConverter.ToObject<TimeSpan>(json);

            Assert.AreEqual(ts1.Ticks, ts2.Ticks);
        }

        [Test]
        public static void TestGuid1()
        {
            var guid1 = new Guid("199B7309-8E94-4DB0-BDD9-DA311E8C47AC");
            string json = JaysonConverter.ToJsonString(guid1);
            var guid2 = JaysonConverter.ToObject<Guid>(json);

            Assert.AreEqual(guid1, guid2);
        }

        [Test]
        public static void TestGuid2()
        {
            var guid1 = new Guid("199B7309-8E94-4DB0-BDD9-DA311E8C47AC");

            var simpleObj1 = new VerySimpleJsonValue
            {
                Value = guid1
            };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;

            string json = JaysonConverter.ToJsonString(simpleObj1, jaysonSerializationSettings);
            var simpleObj2 = JaysonConverter.ToObject<VerySimpleJsonValue>(json);

            Assert.AreEqual((Guid)simpleObj1.Value, (Guid)simpleObj2.Value);
        }

        [Test]
        public static void TestGuid3()
        {
            var guid1 = new Guid("199B7309-8E94-4DB0-BDD9-DA311E8C47AC");

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.GuidAsByteArray = true;

            string json = JaysonConverter.ToJsonString(guid1, jaysonSerializationSettings);
            var guid2 = JaysonConverter.ToObject<Guid>(json);

            Assert.AreEqual(guid1, guid2);
        }

        [Test]
        public static void TestGuid4()
        {
            var guid1 = new Guid("199B7309-8E94-4DB0-BDD9-DA311E8C47AC");

            var simpleObj1 = new VerySimpleJsonValue
            {
                Value = guid1
            };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.GuidAsByteArray = true;
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;

            string json = JaysonConverter.ToJsonString(simpleObj1, jaysonSerializationSettings);
            var simpleObj2 = JaysonConverter.ToObject<VerySimpleJsonValue>(json);

            Assert.AreEqual((Guid)simpleObj1.Value, (Guid)simpleObj2.Value);
        }

        [Test]
        public static void TestLong1()
        {
            var long1 = 1998730980944080L;
            string json = JaysonConverter.ToJsonString(long1);
            var long2 = JaysonConverter.ToObject<long>(json);

            Assert.AreEqual(long1, long2);
        }

        [Test]
        public static void TestLong2()
        {
            long? long1 = 1998730980944080L;
            string json = JaysonConverter.ToJsonString(long1);
            var long2 = JaysonConverter.ToObject<long?>(json);

            Assert.AreEqual(long1, long2);
        }

        [Test]
        public static void TestLong3()
        {
            var simpleObj1 = new VerySimpleJsonValue
            {
                Value = 1998730980944080L
            };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;

            string json = JaysonConverter.ToJsonString(simpleObj1, jaysonSerializationSettings);
            var simpleObj2 = JaysonConverter.ToObject<VerySimpleJsonValue>(json);

            Assert.IsInstanceOf<long>(simpleObj2.Value);
            Assert.AreEqual((long)simpleObj1.Value, (long)simpleObj2.Value);
        }

        [Test]
        public static void TestLong4()
        {
            var simpleObj1 = new VerySimpleJsonValue
            {
                Value = (long?)1998730980944080L
            };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;

            string json = JaysonConverter.ToJsonString(simpleObj1, jaysonSerializationSettings);
            var simpleObj2 = JaysonConverter.ToObject<VerySimpleJsonValue>(json);

            Assert.IsInstanceOf<long?>(simpleObj2.Value);
            Assert.AreEqual((long?)simpleObj1.Value, (long?)simpleObj2.Value);
        }

        [Test]
        public static void TestInt1()
        {
            var int1 = 1998730980;

            string json = JaysonConverter.ToJsonString(int1);
            var int2 = JaysonConverter.ToObject<int?>(json);

            Assert.AreEqual(int1, int2);
        }

        [Test]
        public static void TestInt2()
        {
            int? int1 = 1998730980;

            string json = JaysonConverter.ToJsonString(int1);
            var int2 = JaysonConverter.ToObject<int?>(json);

            Assert.AreEqual(int1, int2);
        }

        [Test]
        public static void TestInt3()
        {
            var simpleObj1 = new VerySimpleJsonValue
            {
                Value = 1998730980
            };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;

            string json = JaysonConverter.ToJsonString(simpleObj1, jaysonSerializationSettings);
            var simpleObj2 = JaysonConverter.ToObject<VerySimpleJsonValue>(json);

            Assert.IsInstanceOf<int>(simpleObj2.Value);
            Assert.AreEqual((int)simpleObj1.Value, (int)simpleObj2.Value);
        }

        [Test]
        public static void TestInt4()
        {
            var simpleObj1 = new VerySimpleJsonValue
            {
                Value = (int?)1998730980
            };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;

            string json = JaysonConverter.ToJsonString(simpleObj1, jaysonSerializationSettings);
            var simpleObj2 = JaysonConverter.ToObject<VerySimpleJsonValue>(json);

            Assert.IsInstanceOf<int?>(simpleObj2.Value);
            Assert.AreEqual((int?)simpleObj1.Value, (int?)simpleObj2.Value);
        }

        [Test]
        public static void TestParseIso8601Date1()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);
            var date2 = JaysonCommon.ParseIso8601DateTime("1972-10-25", JaysonDateTimeZoneType.ConvertToUtc);

            Assert.AreEqual(date1.Date, date2.Date);
        }

        [Test]
        public static void TestParseIso8601Date2()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);
            var date2 = JaysonCommon.ParseIso8601DateTime("1972-10-25T12:45:32Z");

            Assert.AreEqual(date1, date2);
        }

        [Test]
        public static void TestParseIso8601Date3()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Local);
            var tz = JaysonCommon.GetUtcOffset(date1);

            string str = String.Format("1972-10-25T12:45:32+{0:00}:{1:00}", tz.Hours, tz.Minutes);
            var date2 = JaysonCommon.ParseIso8601DateTime(str);

            Assert.AreEqual(date1, date2);
        }

        [Test]
        public static void TestParseIso8601Date4()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Local);
            var tz = JaysonCommon.GetUtcOffset(date1);

            string str = String.Format("1972-10-25T12:45:32+{0:00}{1:00}", tz.Hours, tz.Minutes);
            var date2 = JaysonCommon.ParseIso8601DateTime(str);

            Assert.AreEqual(date1, date2);
        }

        [Test]
        public static void TestParseIso8601Date5()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Local);
            var tz = JaysonCommon.GetUtcOffset(date1);

            string str = String.Format("19721025T124532+{0:00}{1:00}", tz.Hours, tz.Minutes);
            var date2 = JaysonCommon.ParseIso8601DateTime(str);

            Assert.AreEqual(date1, date2);
        }

        [Test]
        public static void TestParseIso8601Date6()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Local);
            var tz = JaysonCommon.GetUtcOffset(date1);

            string str = String.Format("19721025T124532+{0:00}:{1:00}", tz.Hours, tz.Minutes);
            var date2 = JaysonCommon.ParseIso8601DateTime(str);

            Assert.AreEqual(date1, date2);
        }

        [Test]
        public static void TestParseIso8601Date7()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);
            var date2 = JaysonCommon.ParseIso8601DateTime("19721025T124532Z");

            Assert.AreEqual(date1, date2);
        }

        [Test]
        public static void TestParseIso8601Date8()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);
            var date2 = JaysonCommon.ParseIso8601DateTime("19721025T124532Z");

            Assert.AreEqual(date1, date2);
        }

        [Test]
        public static void TestToJsonObject()
        {
            var dto = TestClasses.GetTypedContainerDto();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;

            object jsonObj = JaysonConverter.ToJsonObject(dto, jaysonSerializationSettings);

            Assert.IsTrue(jsonObj is Dictionary<string, object>);
        }

        private static object RoundMinute(string memberName, object date)
        {
            var d = (DateTime)date;
            return d.Subtract(new TimeSpan(0, 0, d.Minute - 5 * (d.Minute / 5), d.Second, d.Millisecond));
        }

        private static object RoundDouble(string memberName, object value)
        {
            var dbl = (double)value;
            return Math.Round(dbl, 3);
        }

        private static void CompareTypedContainerDtos(TypedContainerDto dto1, TypedContainerDto dto2)
        {
            if (dto1 == null)
            {
                Assert.AreEqual(dto2, null);
            }
            else
            {
                Assert.IsNotNull(dto2);

                Assert.AreEqual(dto1.Address1, dto2.Address1);
                Assert.AreEqual(dto1.Address2, dto2.Address2);

                Assert.AreEqual(dto1.ByteArray, dto2.ByteArray);
                Assert.AreEqual(RoundMinute("Date1", dto1.Date1), RoundMinute("Date1", dto2.Date1));
                Assert.AreEqual(RoundMinute("Date2", dto1.Date2), RoundMinute("Date2", dto2.Date2));
                Assert.AreEqual(RoundMinute("Date3", dto1.Date3), RoundMinute("Date3", dto2.Date3));

                Assert.AreEqual(RoundDouble("Double1", dto1.Double1), RoundDouble("Double1", dto2.Double1));
                Assert.AreEqual(RoundDouble("Double2", dto1.Double2), RoundDouble("Double2", dto2.Double2));

                Assert.AreEqual(dto1.Enum1, dto2.Enum1);
                Assert.AreEqual(dto1.Enum2, dto2.Enum2);
                Assert.AreEqual(dto1.Enum3, dto2.Enum3);

                Assert.AreEqual(dto1.Guid1, dto2.Guid1);
                Assert.AreEqual(dto1.Guid2, dto2.Guid2);

                Assert.AreEqual(dto1.Destination, dto2.Destination);
#if !(NET3500 || NET3000 || NET2000)
                Assert.AreEqual(dto1.DynamicProperty, dto2.DynamicProperty);
#endif
                Assert.AreEqual(dto1.IntArray2D, dto2.IntArray2D);

                if (dto1.Object2DArray == null)
                {
                    Assert.AreEqual(dto2.Object2DArray, null);
                }
                else
                {
                    Assert.IsNotNull(dto2.Object2DArray);
                    Assert.AreEqual(dto1.Object2DArray.Length, dto2.Object2DArray.Length);

                    for (int i = 0; i < dto1.Object2DArray.Length; i++)
                    {
                        Assert.AreEqual(dto1.Object2DArray[i].Length, dto2.Object2DArray[i].Length);
                        for (int j = 0; j < dto1.Object2DArray[i].Length; j++)
                        {
                            Assert.AreEqual(dto1.Object2DArray[i][j].Length, dto2.Object2DArray[i][j].Length);
                            for (int k = 0; k < dto1.Object2DArray[i][j].Length; k++)
                            {
                                Assert.AreEqual(dto1.Object2DArray[i][j][k], dto2.Object2DArray[i][j][k]);
                            }
                        }
                    }
                }

                if (dto1.ObjectArrayList == null)
                {
                    Assert.AreEqual(dto2.ObjectArrayList, null);
                }
                else
                {
                    Assert.IsNotNull(dto2.ObjectArrayList);
                    Assert.AreEqual(dto1.ObjectArrayList.Count, dto2.ObjectArrayList.Count);

                    for (int i = 0; i < dto1.ObjectArrayList.Count; i++)
                    {
                        Assert.AreEqual(dto1.Object2DArray[i].Length, dto2.Object2DArray[i].Length);
                        for (int j = 0; j < dto1.Object2DArray[i].Length; j++)
                        {
                            Assert.AreEqual(dto1.Object2DArray[i][j], dto2.Object2DArray[i][j]);
                        }
                    }
                }

                if (dto1.P1 == null)
                {
                    Assert.AreEqual(dto2.P1, null);
                }
                else
                {
                    Assert.IsNotNull(dto2.P1);
                    Assert.AreEqual(dto1.P1.Count, dto2.P1.Count);

                    for (int i = 0; i < dto1.P1.Count; i++)
                    {
                        Assert.AreEqual(dto1.P1[i], dto2.P1[i]);
                    }
                }

                if (dto1.P2 == null)
                {
                    Assert.AreEqual(dto2.P2, null);
                }
                else
                {
                    Assert.IsNotNull(dto2.P2);
                    Assert.AreEqual(dto1.P2.Count, dto2.P2.Count);

                    for (int i = 0; i < dto1.P2.Count; i++)
                    {
                        Assert.AreEqual(dto1.P2[i], dto2.P2[i]);
                    }
                }

                if (dto1.Source == null)
                {
                    Assert.AreEqual(dto2.Source, null);
                }
                else
                {
                    Assert.IsNotNull(dto2.Source);
                    Assert.AreEqual(dto1.Source.Count, dto2.Source.Count);

                    for (int i = 0; i < dto1.Source.Count; i++)
                    {
                        if (dto1.Source[i] == null)
                        {
                            Assert.AreEqual(dto2.Source[i], null);
                        }
                        else
                        {
                            Assert.IsNotNull(dto2.Source[i]);

                            Assert.AreEqual(dto1.Source[i].ElementId, dto2.Source[i].ElementId);

                            if (dto1.Source[i].Action == null)
                            {
                                Assert.AreEqual(dto1.Source[i].Action, null);
                            }
                            else
                            {
                                Assert.IsNotNull(dto1.Source[i].Action);

                                Assert.AreEqual(dto1.Source[i].Action.Action, dto2.Source[i].Action.Action);
                                Assert.AreEqual(dto1.Source[i].Action.ElementId, dto2.Source[i].Action.ElementId);
                            }
                        }
                    }
                }
            }
        }

        private static void CompareTypedContainerNoDtos(ITypedContainerNoDto dto1, ITypedContainerNoDto dto2)
        {
            if (dto1 == null)
            {
                Assert.AreEqual(dto2, null);
            }
            else
            {
                CompareTypedContainerDtos(dto1 as TypedContainerDto, dto2 as TypedContainerDto);

                var dto11 = dto1 as TypedContainerNoDto;
                var dto21 = dto1 as TypedContainerNoDto;

                if (dto11.ValueContainer == null)
                {
                    Assert.AreEqual(dto21.ValueContainer, null);
                }
                else
                {
                    Assert.IsNotNull(dto21.ValueContainer);

                    Assert.AreEqual(dto11.ValueContainer.Destination, dto21.ValueContainer.Destination);
                    Assert.AreEqual(dto11.ValueContainer.Source, dto21.ValueContainer.Source);
                }
            }
        }

        [Test]
        public static void TestUseGlobalTypeNames()
        {
            var dto1 = TestClasses.GetTypedContainerDto() as TypedContainerDto;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.UseGlobalTypeNames = true;
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = true;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            object jsonObj = JaysonConverter.Parse(json, jaysonDeserializationSettings);
            TypedContainerDto dto2 = JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(jsonObj);
            CompareTypedContainerDtos(dto1, dto2);
        }

        [Test]
        public static void TestListT1()
        {
            List<int> list1 = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            List<int> list2 = JaysonConverter.ToObject<List<int>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.AreEqual(list1.Count, list2.Count);

            for (int i = 0; i < list1.Count; i++)
            {
                Assert.AreEqual(list1[i], list2[i]);
            }
        }

        [Test]
        public static void TestListT2()
        {
            List<int> list1 = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            List<int> list2 = JaysonConverter.ToObject<List<int>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.AreEqual(list1.Count, list2.Count);

            for (int i = 0; i < list1.Count; i++)
            {
                Assert.AreEqual(list1[i], list2[i]);
            }
        }

        [Test]
        public static void TestListT3()
        {
            List<int[]> list1 = new List<int[]> { 
				new int[] {1, 2, 3}, 
				new int[] {4, 5, 6, 7, 8} 
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            List<int[]> list2 = JaysonConverter.ToObject<List<int[]>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.AreEqual(list1.Count, list2.Count);

            for (int i = 0; i < list1.Count; i++)
            {
                Assert.AreEqual(list1[i], list2[i]);
            }
        }

        [Test]
        public static void TestListT4()
        {
            List<int[,]> list1 = new List<int[,]> { 
				new int[,] { 
					{ 1, 2 }, 
					{ 3, 4 } 
				}, 
				new int[,] { 
					{ 5, 6 }, 
					{ 7, 8 } 
				} 
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            List<int[,]> list2 = JaysonConverter.ToObject<List<int[,]>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.AreEqual(list1.Count, list2.Count);

            for (int i = 0; i < list1.Count; i++)
            {
                Assert.AreEqual(list1[i], list2[i]);
            }
        }

        [Test]
        public static void TestListT5()
        {
            List<Dictionary<string, int[,]>> list1 = new List<Dictionary<string, int[,]>> { 
				new Dictionary<string, int[,]> { 
					{ "A", new int[,] { { 1, 2 }, { 3, 4 } } }, 
					{ "B", new int[,] { { 5, 6 }, { 7, 8 } } } 
				}
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

#if (NET3500 || NET3000 || NET2000)
			object list2 = null;
#else
            dynamic list2 = null;
#endif

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            list2 = JaysonConverter.ToObject(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
#if !(NET3500 || NET3000 || NET2000)
            Assert.AreEqual(list1.Count, list2.Count);

            for (int i = 0; i < list1.Count; i++)
            {
                foreach (var kvp in list1[i])
                {
                    Assert.IsTrue(list2[i].ContainsKey(kvp.Key.ToLowerInvariant()));
                    Assert.AreEqual(kvp.Value, list2[i][kvp.Key.ToLowerInvariant()]);
                }
            }
#endif
        }

        [Test]
        public static void TestListT6()
        {
            List<Dictionary<string, List<int[,]>>> list1 = new List<Dictionary<string, List<int[,]>>> { 
				new Dictionary<string, List<int[,]>> { 
					{ "A", new List<int[,]> { new int[,] { { 1, 2 }, { 3, 4 } } } }, 
					{ "B", new List<int[,]> { new int[,] { { 5, 6 }, { 7, 8 } } } }
				}
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

#if (NET3500 || NET3000 || NET2000)
			object list2 = null; 
#else
            dynamic list2 = null;
#endif

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            list2 = JaysonConverter.ToObject(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
#if !(NET3500 || NET3000 || NET2000)
            Assert.AreEqual(list1.Count, list2.Count);

            for (int i = 0; i < list1.Count; i++)
            {
                foreach (var kvp in list1[i])
                {
                    Assert.IsTrue(list2[i].ContainsKey(kvp.Key.ToLowerInvariant()));
                    Assert.AreEqual(kvp.Value, list2[i][kvp.Key.ToLowerInvariant()]);
                }
            }
#endif
        }

        [Test]
        public static void TestDictionaryTK1()
        {
            Dictionary<string, int> dictionary1 = new Dictionary<string, int> { 
				{ "1", 2 }, 
				{ "3", 4 }, 
				{ "5", 6 }, 
				{ "7", 8 } 
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(dictionary1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            Dictionary<string, int> dictionary2 = JaysonConverter.ToObject<Dictionary<string, int>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dictionary2);
            Assert.AreEqual(dictionary1.Count, dictionary2.Count);

            foreach (var key in dictionary1.Keys)
            {
                Assert.AreEqual(dictionary1[key], dictionary2[key]);
            }
        }

        [Test]
        public static void TestDictionaryTK2()
        {
            Dictionary<string, int> dictionary1 = new Dictionary<string, int> { 
				{ "1", 2 }, 
				{ "3", 4 }, 
				{ "5", 6 }, 
				{ "7", 8 } 
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(dictionary1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            Dictionary<string, int> dictionary2 = JaysonConverter.ToObject<Dictionary<string, int>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dictionary2);
            Assert.AreEqual(dictionary1.Count, dictionary2.Count);

            foreach (var key in dictionary1.Keys)
            {
                Assert.AreEqual(dictionary1[key], dictionary2[key]);
            }
        }

        [Test]
        public static void TestDictionaryTK3()
        {
            Dictionary<string, int[]> dictionary1 = new Dictionary<string, int[]> { 
				{ "A", new int[] { 1, 2, 3 } }, 
				{ "B", new int[] { 4, 5, 6, 7, 8 } } 
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(dictionary1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            Dictionary<string, int[]> dictionary2 = JaysonConverter.ToObject<Dictionary<string, int[]>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dictionary2);
            Assert.AreEqual(dictionary1.Count, dictionary2.Count);

            foreach (var key in dictionary1.Keys)
            {
                Assert.AreEqual(dictionary1[key], dictionary2[key.ToLowerInvariant()]);
            }
        }

        [Test]
        public static void TestDictionaryTK4()
        {
            Dictionary<string, int[,]> dictionary1 = new Dictionary<string, int[,]> { 
				{ "A", new int[,] { { 1, 2 }, { 3, 4 } } }, 
				{ "B", new int[,] { { 5, 6 }, { 7, 8 } } } 
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(dictionary1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            Dictionary<string, int[,]> dictionary2 = JaysonConverter.ToObject<Dictionary<string, int[,]>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dictionary2);
            Assert.AreEqual(dictionary1.Count, dictionary2.Count);

            foreach (var key in dictionary1.Keys)
            {
                Assert.AreEqual(dictionary1[key], dictionary2[key.ToLowerInvariant()]);
            }
        }

        [Test]
        public static void TestDictionaryTK5()
        {
            Dictionary<string, List<int>> dictionary1 = new Dictionary<string, List<int>> { 
				{ "1", new List<int> { 2, 4, 6, 8 } }, 
				{ "3", new List<int> { 4, 6, 8 } }, 
				{ "5", new List<int> { 6, 8 } }, 
				{ "7", new List<int> { 8 } } 
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(dictionary1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            Dictionary<string, List<int>> dictionary2 = JaysonConverter.ToObject<Dictionary<string, List<int>>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dictionary2);
            Assert.AreEqual(dictionary1.Count, dictionary2.Count);

            List<int> list2;
            foreach (var kvp in dictionary1)
            {
                list2 = dictionary2[kvp.Key];
                Assert.AreEqual(kvp.Value.Count, list2.Count);
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    Assert.AreEqual(kvp.Value[i], list2[i]);
                }
            }
        }

        [Test]
        public static void TestDictionaryTK6()
        {
            Dictionary<string, List<int>> dictionary1 = new Dictionary<string, List<int>> { 
				{ "1", new List<int> { 2, 4, 6, 8 } }, 
				{ "3", new List<int> { 4, 6, 8 } }, 
				{ "5", new List<int> { 6, 8 } }, 
				{ "7", new List<int> { 8 } } 
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(dictionary1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            Dictionary<string, List<int>> dictionary2 = JaysonConverter.ToObject<Dictionary<string, List<int>>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dictionary2);
            Assert.AreEqual(dictionary1.Count, dictionary2.Count);

            List<int> list2;
            foreach (var kvp in dictionary1)
            {
                list2 = dictionary2[kvp.Key];
                Assert.AreEqual(kvp.Value.Count, list2.Count);
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    Assert.AreEqual(kvp.Value[i], list2[i]);
                }
            }
        }

        [Test]
        public static void TestDictionaryTK7()
        {
            Dictionary<string, List<int[]>> dictionary1 = new Dictionary<string, List<int[]>> { 
				{ "A", new List<int[]> { new int[] { 1, 2, 3 } } }, 
				{ "B", new List<int[]> { new int[] { 4, 5, 6, 7, 8 } } } 
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(dictionary1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            Dictionary<string, List<int[]>> dictionary2 = JaysonConverter.ToObject<Dictionary<string, List<int[]>>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dictionary2);
            Assert.AreEqual(dictionary1.Count, dictionary2.Count);

            List<int[]> list2;
            foreach (var kvp in dictionary1)
            {
                list2 = dictionary2[kvp.Key.ToLowerInvariant()];
                Assert.AreEqual(kvp.Value.Count, list2.Count);
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    Assert.AreEqual(kvp.Value[i], list2[i]);
                }
            }
        }

        [Test]
        public static void TestDictionaryTK8()
        {
            Dictionary<string, List<int[,]>> dictionary1 = new Dictionary<string, List<int[,]>> { 
				{ "A", new List<int[,]> { new int[,] { { 1, 2 }, { 3, 4 } } } }, 
				{ "B", new List<int[,]> { new int[,] { { 5, 6 }, { 7, 8 } } } } 
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(dictionary1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            Dictionary<string, List<int[,]>> dictionary2 = JaysonConverter.ToObject<Dictionary<string, List<int[,]>>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dictionary2);
            Assert.AreEqual(dictionary1.Count, dictionary2.Count);

            List<int[,]> list2;
            foreach (var kvp in dictionary1)
            {
                list2 = dictionary2[kvp.Key.ToLowerInvariant()];
                Assert.AreEqual(kvp.Value.Count, list2.Count);
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    Assert.AreEqual(kvp.Value[i], list2[i]);
                }
            }
        }

        [Test]
        public static void TestDictionaryTK9()
        {
            Dictionary<string, IList<int[,]>> dictionary1 = new Dictionary<string, IList<int[,]>> { 
				{ "A", new List<int[,]> { new int[,] { { 1, 2 }, { 3, 4 } } } }, 
				{ "B", new List<int[,]> { new int[,] { { 5, 6 }, { 7, 8 } } } } 
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(dictionary1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            Dictionary<string, IList<int[,]>> dictionary2 = JaysonConverter.ToObject<Dictionary<string, IList<int[,]>>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dictionary2);
            Assert.AreEqual(dictionary1.Count, dictionary2.Count);

            IList<int[,]> list2;
            foreach (var kvp in dictionary1)
            {
                list2 = dictionary2[kvp.Key.ToLowerInvariant()];
                Assert.AreEqual(kvp.Value.Count, list2.Count);
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    Assert.AreEqual(kvp.Value[i], list2[i]);
                }
            }
        }

        [Test]
        public static void TestDictionaryTK10()
        {
            Dictionary<char, IList<int[,]>> dictionary1 = new Dictionary<char, IList<int[,]>> { 
				{ 'A', new List<int[,]> { new int[,] { { 1, 2 }, { 3, 4 } } } }, 
				{ 'B', new List<int[,]> { new int[,] { { 5, 6 }, { 7, 8 } } } } 
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(dictionary1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            Dictionary<char, IList<int[,]>> dictionary2 = JaysonConverter.ToObject<Dictionary<char, IList<int[,]>>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dictionary2);
            Assert.AreEqual(dictionary1.Count, dictionary2.Count);

            IList<int[,]> list2;
            foreach (var kvp in dictionary1)
            {
                list2 = dictionary2[kvp.Key];
                Assert.AreEqual(kvp.Value.Count, list2.Count);
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    Assert.AreEqual(kvp.Value[i], list2[i]);
                }
            }
        }

        [Test]
        public static void TestDictionaryTK11()
        {
            Dictionary<IList, IList<int[,]>> dictionary1 = new Dictionary<IList, IList<int[,]>> { 
				{ new ArrayList { 'A' }, new List<int[,]> { new int[,] { { 1, 2 }, { 3, 4 } } } }, 
				{ new ArrayList { 'B' }, new List<int[,]> { new int[,] { { 5, 6 }, { 7, 8 } } } } 
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(dictionary1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            Dictionary<IList, IList<int[,]>> dictionary2 = JaysonConverter.ToObject<Dictionary<IList, IList<int[,]>>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dictionary2);
            Assert.AreEqual(dictionary1.Count, dictionary2.Count);

            IList<int[,]> list2;
            foreach (var kvp in dictionary1)
            {
                list2 = dictionary2.FirstOrDefault(kvp2 => kvp2.Key[0].ToString().ToLowerInvariant() ==
                    kvp.Key[0].ToString().ToLowerInvariant()).Value;

                Assert.AreEqual(kvp.Value.Count, list2.Count);
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    Assert.AreEqual(kvp.Value[i], list2[i]);
                }
            }
        }

        [Test]
        public static void TestMultiDimentionalArray1()
        {
            int[,] intArray2D = new int[,] { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 } };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(intArray2D, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            int[,] intArray2D2 = JaysonConverter.ToObject<int[,]>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(intArray2D2);
            Assert.AreEqual(intArray2D, intArray2D2);
        }

        [Test]
        public static void TestMultiDimentionalArray2()
        {
            int[,] intArray2D = new int[,] { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 } };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(intArray2D, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            int[,] intArray2D2 = JaysonConverter.ToObject<int[,]>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(intArray2D2);
            Assert.AreEqual(intArray2D, intArray2D2);
        }

        [Test]
        public static void TestMultiDimentionalArray3()
        {
            int[,] intArray2D = new int[,] { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 } };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.AllButNoPrimitive;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(intArray2D, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            int[,] intArray2D2 = JaysonConverter.ToObject<int[,]>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(intArray2D2);
            Assert.AreEqual(intArray2D, intArray2D2);
        }

        [Test]
        public static void TestMultiDimentionalEmptyArray1()
        {
            int[,] intArray2D = new int[,] { { }, { }, { }, { } };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(intArray2D, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            int[,] intArray2D2 = JaysonConverter.ToObject<int[,]>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(intArray2D2);
            Assert.AreEqual(intArray2D, intArray2D2);
        }

        [Test]
        public static void TestMultiDimentionalEmptyArray2()
        {
            int[,] intArray2D = new int[,] { { }, { }, { }, { } };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(intArray2D, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            int[,] intArray2D2 = JaysonConverter.ToObject<int[,]>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(intArray2D2);
            Assert.AreEqual(intArray2D, intArray2D2);
        }

        [Test]
        public static void TestMultiDimentionalEmptyArray3()
        {
            int[,] intArray2D = new int[,] { { }, { }, { }, { } };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.AllButNoPrimitive;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(intArray2D, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            int[,] intArray2D2 = JaysonConverter.ToObject<int[,]>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(intArray2D2);
            Assert.AreEqual(intArray2D, intArray2D2);
        }

        [Test]
        public static void TestListAndMultiDimentionalArray1()
        {
            List<int[,]> intArray2D = new List<int[,]> { new int[,] { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 } } };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(intArray2D, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            List<int[,]> intArray2D2 = JaysonConverter.ToObject<List<int[,]>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(intArray2D2);
            Assert.AreEqual(intArray2D.Count, intArray2D2.Count);

            for (int i = 0; i < intArray2D.Count; i++)
            {
                Assert.AreEqual(intArray2D[i], intArray2D2[i]);
            }
        }

        [Test]
        public static void TestListAndMultiDimentionalArray2()
        {
            List<int[,]> intArray2D = new List<int[,]> { new int[,] { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 } } };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(intArray2D, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            List<int[,]> intArray2D2 = JaysonConverter.ToObject<List<int[,]>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(intArray2D2);
            Assert.AreEqual(intArray2D.Count, intArray2D2.Count);

            for (int i = 0; i < intArray2D.Count; i++)
            {
                Assert.AreEqual(intArray2D[i], intArray2D2[i]);
            }
        }

        [Test]
        public static void TestListAndMultiDimentionalArray3()
        {
            List<int[,]> intArray2D = new List<int[,]> { new int[,] { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 } } };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.AllButNoPrimitive;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(intArray2D, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            List<int[,]> intArray2D2 = JaysonConverter.ToObject<List<int[,]>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(intArray2D2);
            Assert.AreEqual(intArray2D.Count, intArray2D2.Count);

            for (int i = 0; i < intArray2D.Count; i++)
            {
                Assert.AreEqual(intArray2D[i], intArray2D2[i]);
            }
        }

        [Test]
        public static void TestListAndMultiDimentionalArray4()
        {
            List<int[,]> intArray2D = new List<int[,]> { new int[,] { { }, { }, { }, { } } };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.AllButNoPrimitive;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(intArray2D, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            List<int[,]> intArray2D2 = JaysonConverter.ToObject<List<int[,]>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(intArray2D2);
            Assert.AreEqual(intArray2D.Count, intArray2D2.Count);

            for (int i = 0; i < intArray2D.Count; i++)
            {
                Assert.AreEqual(intArray2D[i], intArray2D2[i]);
            }
        }

        [Test]
        public static void TestListAndMultiDimentionalArray5()
        {
            List<int[,]> intArray2D = new List<int[,]> { new int[,] { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 } } };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.AllButNoPrimitive;
            jaysonSerializationSettings.Formatting = JaysonFormatting.None;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(intArray2D, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            List<int[,]> intArray2D2 = JaysonConverter.ToObject<List<int[,]>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(intArray2D2);
            Assert.AreEqual(intArray2D.Count, intArray2D2.Count);

            for (int i = 0; i < intArray2D.Count; i++)
            {
                Assert.AreEqual(intArray2D[i], intArray2D2[i]);
            }
        }

        [Test]
        public static void TestComplexObject()
        {
            var dto1 = TestClasses.GetTypedContainerDto() as TypedContainerDto;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            TypedContainerDto dto2 = JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dto2);
            CompareTypedContainerDtos(dto1, dto2);
        }

        [Test]
        public static void TestTypeOverride()
        {
            var dto1 = TestClasses.GetTypedContainerDto() as TypedContainerDto;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffff%K";
            jaysonSerializationSettings.AddTypeOverride(new JaysonTypeOverride<TextElementDto>()
                .IgnoreMember("ElementType")
                .SetMemberAlias("ElementId", "id"));

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;
            jaysonDeserializationSettings.
                AddTypeOverride(new JaysonTypeOverride<TextElementDto, TextElementDto2>()).
                AddTypeOverride(new JaysonTypeOverride<TextElementDto2>().
                    SetMemberAlias("ElementId", "id").
                    IgnoreMember("ElementType"));

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            TypedContainerDto dto2 = JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dto2);
            CompareTypedContainerDtos(dto1, dto2);
        }

        [Test]
        public static void TestInterfaceDeserializationUsingSType()
        {
            var dto1 = TestClasses.GetTypedContainerNoDto() as ITypedContainerNoDto;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            var dto2 = JaysonConverter.ToObject<ITypedContainerNoDto>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dto2);
            Assert.IsTrue(dto2 is ITypedContainerNoDto);

            CompareTypedContainerNoDtos(dto1, dto2);
        }

        [Test]
        public static void TestIgnoreMember1()
        {
            var dto = TestClasses.GetTypedContainerIgnoreMemberDto();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(dto, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            TypedContainerIgnoreMemberDto result = JaysonConverter.ToObject<TypedContainerIgnoreMemberDto>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.InheritedInt1, 0);
            Assert.AreEqual(result.InheritedInt2, 2);
            Assert.AreEqual(result.InheritedInt3, 0);
            Assert.AreEqual(result.InheritedInt4, 4);
            Assert.AreEqual(result.InheritedStr1, null);
            Assert.AreEqual(result.InheritedStr2, "Str2");
            Assert.AreEqual(result.InheritedStr3, null);
            Assert.AreEqual(result.InheritedStr4, "Str4");
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

#if !(NET3500 || NET3000 || NET2000)
            public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                assemblyName = null;
                typeName = null;
            }
#endif
        }

        [Test]
        public static void TestInterfaceDeserializationUsingBinder()
        {
            var dto1 = TestClasses.GetTypedContainerNoDto() as TypedContainerNoDto;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;

            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;
            jaysonDeserializationSettings.Binder = new TestBinder();

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            var dto2 = JaysonConverter.ToObject<ITypedContainerNoDto>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dto2);
            Assert.IsTrue(dto2 is ITypedContainerNoDto);

            CompareTypedContainerNoDtos(dto1, dto2);
        }

        [Test]
        public static void TestInterfaceDeserializationUsingObjectActivator()
        {
            var dto1 = TestClasses.GetTypedContainerNoDto();

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
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

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            ITypedContainerNoDto dto2 = JaysonConverter.ToObject<ITypedContainerNoDto>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dto2);

            CompareTypedContainerNoDtos((ITypedContainerNoDto)dto1, dto2);
        }

        [Test]
        public static void TestSerializeDeserializeDateWithCustomDateTimeFormat()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.DateTimeFormat = "dd/MM/yyyy HH:mm:ss.fff%K";
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.CustomDate;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.DateTimeFormat = "dd/MM/yyyy HH:mm:ss.fff%K";

            string json = JaysonConverter.ToJsonString(date1, jaysonSerializationSettings);
            var date2 = JaysonConverter.ToObject<DateTime?>(json, jaysonDeserializationSettings);

            Assert.IsTrue(json.Contains("25\\/10\\/1972 12:45:32.000Z"));
            Assert.IsNotNull(date2);
            Assert.IsTrue(date2 is DateTime);
            Assert.AreEqual(date1, (DateTime)date2);
        }

        [Test]
        public static void TestSerializeDateTimeUtc()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            string json = JaysonConverter.ToJsonString(date1, jaysonSerializationSettings);
            var date2 = JaysonConverter.ToObject<DateTime?>(json);

            Assert.IsTrue(json.Contains("1972-10-25T12:45:32Z"));
            Assert.IsNotNull(date2);
            Assert.IsTrue(date2 is DateTime);
            Assert.AreEqual(date1, (DateTime)date2);
        }

        [Test]
        public static void TestSerializeDateTimeLocal()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Local);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            string json = JaysonConverter.ToJsonString(date1, jaysonSerializationSettings);
            var date2 = JaysonConverter.ToObject<DateTime?>(json);

            Assert.IsTrue(json.Contains("1972-10-25T12:45:32+") || json.Contains("1972-10-25T12:45:32-"));
            Assert.IsNotNull(date2);
            Assert.IsTrue(date2 is DateTime);
            Assert.AreEqual(date1, (DateTime)date2);
        }

        [Test]
        public static void TestSerializeDateTimeUnspecified()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Unspecified);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            string json = JaysonConverter.ToJsonString(date1, jaysonSerializationSettings);
            var date2 = JaysonConverter.ToObject<DateTime?>(json);

            Assert.IsTrue(json.Contains("1972-10-25T12:45:32+") || json.Contains("1972-10-25T12:45:32-"));
            Assert.IsNotNull(date2);
            Assert.IsTrue(date2 is DateTime);
            Assert.AreEqual(date1, (DateTime)date2);
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
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            var dto2 = JaysonConverter.ToObject<VerySimpleJsonValue>(json);

            Assert.IsTrue(json.Contains("\\/Date("));
            Assert.IsNotNull(dto2);
            Assert.IsNotNull(dto2.Value);
            Assert.IsTrue(dto2.Value is DateTime);
            Assert.AreEqual(date1, (DateTime)dto2.Value);
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
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            var dto2 = JaysonConverter.ToObject<VerySimpleJsonValue>(json);

            Assert.IsTrue(json.Contains("\\/Date(") && (json.Contains("+") || json.Contains("-")));
            Assert.IsNotNull(dto2);
            Assert.IsNotNull(dto2.Value);
            Assert.IsTrue(dto2.Value is DateTime);
            Assert.AreEqual(date1, (DateTime)dto2.Value);
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
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            var dto2 = JaysonConverter.ToObject<VerySimpleJsonValue>(json);

            Assert.IsTrue(json.Contains("\\/Date(") && (json.Contains("+") || json.Contains("-")));
            Assert.IsNotNull(dto2);
            Assert.IsNotNull(dto2.Value);
            Assert.IsTrue(dto2.Value is DateTime);
            Assert.AreEqual(date1, (DateTime)dto2.Value);
        }

        [Test]
        public static void TestDeserializeDateTimeConvertToUtc()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Local);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.ConvertToUtc;

            string json = JaysonConverter.ToJsonString(date1, jaysonSerializationSettings);
            var date2 = JaysonConverter.ToObject<DateTime>(json, jaysonDeserializationSettings);

            Assert.IsTrue(json.Contains("1972-10-25T12:45:32"));
            Assert.IsNotNull(date2);
            Assert.AreEqual(date2.Kind, DateTimeKind.Utc);
        }

        [Test]
        public static void TestDeserializeDateTimeConvertToLocal()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Utc);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.ConvertToLocal;

            string json = JaysonConverter.ToJsonString(date1, jaysonSerializationSettings);
            var date2 = JaysonConverter.ToObject<DateTime>(json, jaysonDeserializationSettings);

            Assert.IsTrue(json.Contains("1972-10-25T12:45:32"));
            Assert.IsNotNull(date2);
            Assert.AreEqual(date2.Kind, DateTimeKind.Local);
        }

        [Test]
        public static void TestDeserializeDateTimeKeepAsIs()
        {
            var date1 = new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Local);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Iso8601;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            string json = JaysonConverter.ToJsonString(date1, jaysonSerializationSettings);
            var date2 = JaysonConverter.ToObject<DateTime>(json, jaysonDeserializationSettings);

            Assert.IsTrue(json.Contains("1972-10-25T12:45:32"));
            Assert.IsNotNull(date2);
            Assert.AreEqual(date1, date2);
            Assert.AreEqual(date2.Kind, DateTimeKind.Local);
        }

        [Test]
        public static void TestDecimal1()
        {
            var dcml1 = 12345.67890123456789m;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            string json = JaysonConverter.ToJsonString(dcml1, jaysonSerializationSettings);
            var dcml2 = JaysonConverter.ToObject(json);

            Assert.IsTrue(json.Contains(".Decimal"));
            Assert.IsNotNull(dcml2);
            Assert.IsTrue(dcml2 is decimal);
            Assert.AreEqual(dcml1, (decimal)dcml2);
        }

        [Test]
        public static void TestDecimal2()
        {
            var dcml1 = 12345.67890123456789m;

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            string json = JaysonConverter.ToJsonString(dcml1, jaysonSerializationSettings);
            var dcml2 = JaysonConverter.ToObject(json);

            Assert.IsTrue(json == "12345.67890123456789");
            Assert.IsNotNull(dcml2);
            Assert.IsTrue(dcml2 is decimal);
            Assert.IsTrue(dcml1 == (decimal)dcml2);
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
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);
            var dto2 = JaysonConverter.ToObject<VerySimpleJsonValue>(json);

            Assert.IsTrue(json.Contains(".Decimal"));
            Assert.IsNotNull(dto2);
            Assert.IsNotNull(dto2.Value);
            Assert.IsTrue(dto2.Value is decimal);
            Assert.AreEqual(dcml1, (Decimal)dto2.Value);
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
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject(json);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2 is List<object>);

            var list22 = (List<object>)list2;
            Assert.AreEqual(list1.Count, list22.Count);

            for (int i = 0; i < list1.Count - 1; i++)
            {
                Assert.AreEqual(list1[i], list22[i]);
            }

            Assert.IsTrue(list22[list22.Count - 1] is VerySimpleJsonValue);

            var vsjv1 = (VerySimpleJsonValue)list22[list22.Count - 1];
            Assert.IsTrue(vsjv1.Value is VerySimpleJsonValue);

            var vsjv2 = (VerySimpleJsonValue)vsjv1.Value;
            Assert.IsTrue(vsjv2.Value is bool && (bool)vsjv2.Value);
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
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject(json);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2 is List<object>);

            var list22 = (List<object>)list2;
            Assert.AreEqual(list1.Count, list22.Count);

            for (int i = 0; i < list1.Count - 1; i++)
            {
                Assert.AreEqual(list1[i], list22[i]);
            }

            Assert.IsTrue(list22[list22.Count - 1] is IDictionary<string, object>);

            var vsjv1 = (IDictionary<string, object>)list22[list22.Count - 1];
            Assert.IsTrue(vsjv1["Value"] is IDictionary<string, object>);

            var vsjv2 = (IDictionary<string, object>)vsjv1["Value"];
            Assert.IsTrue(vsjv2["Value"] is bool && (bool)vsjv2["Value"]);
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
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject(json);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2 is ArrayList);

            var list22 = (ArrayList)list2;
            Assert.AreEqual(list1.Count, list22.Count);

            for (int i = 0; i < list1.Count - 1; i++)
            {
                Assert.AreEqual(list1[i], list22[i]);
            }

            Assert.IsTrue(list22[list22.Count - 1] is VerySimpleJsonValue);

            var vsjv1 = (VerySimpleJsonValue)list22[list22.Count - 1];
            Assert.IsTrue(vsjv1.Value is VerySimpleJsonValue);

            var vsjv2 = (VerySimpleJsonValue)vsjv1.Value;
            Assert.IsTrue(vsjv2.Value is bool && (bool)vsjv2.Value);
        }

        [Test]
        public static void TestArrayList1()
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
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.Array;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2 is object[]);
            Assert.IsTrue(list2.GetType().IsArray);

            var list22 = (Array)list2;
            Assert.AreEqual(list1.Count, list22.Length);

            for (int i = 0; i < list1.Count - 1; i++)
            {
                Assert.AreEqual(list1[i], list22.GetValue(i));
            }

            Assert.IsTrue(list22.GetValue(list22.Length - 1) is IDictionary<string, object>);

            var vsjv1 = (IDictionary<string, object>)list22.GetValue(list22.Length - 1);
            Assert.IsTrue(vsjv1["Value"] is IDictionary<string, object>);

            var vsjv2 = (IDictionary<string, object>)vsjv1["Value"];
            Assert.IsTrue(vsjv2["Value"] is bool && (bool)vsjv2["Value"]);
        }

        [Test]
        public static void TestArrayList2()
        {
            var list1 = new ArrayList { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);

            var list22 = (IList)list2;
            Assert.AreEqual(list1.Count, list22.Count);

            for (int i = 0; i < list1.Count; i++)
            {
                Assert.AreEqual(list1[i], list22[i]);
            }
        }

        [Test]
        public static void TestArrayList3()
        {
            var list1 = new ArrayList { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, null };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.Array;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);

            var list22 = (IList)list2;
            Assert.AreEqual(list1.Count, list22.Count);

            for (int i = 0; i < list1.Count; i++)
            {
                Assert.AreEqual(list1[i], list22[i]);
            }
        }

        [Test]
        public static void TestArray1a()
        {
            var list1 = new int[] { };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);

            var list22 = (IList)list2;
            Assert.AreEqual(list1.Length, list22.Count);

            for (int i = 0; i < list1.Length; i++)
            {
                Assert.AreEqual(list1[i], list22[i]);
            }
        }

        [Test]
        public static void TestArray1b()
        {
            var list1 = new int[] { };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject<int[]>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);
            Assert.AreEqual(list1, list2);
        }

        [Test]
        public static void TestArray2a()
        {
            var list1 = new int[][] { };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);

            var list22 = (IList)list2;
            Assert.AreEqual(list1, list22);
        }

        [Test]
        public static void TestArray2b()
        {
            var list1 = new int[][] { };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject<int[][]>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);
            Assert.AreEqual(list1.Length, list2.Length);
            Assert.AreEqual(list2.Length, 0);
            Assert.AreEqual(list1, list2);
        }

        [Test]
        public static void TestArray3a()
        {
            var list1 = new int[][] { new int[] { } };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);
            Assert.AreEqual(list1.Length, ((IList)list2).Count);
            Assert.AreEqual(((IList)list2).Count, 1);
            Assert.IsTrue(((IList)list2)[0].GetType().IsArray);
            Assert.AreEqual(((IList)((IList)list2)[0]).Count, 0);
        }

        [Test]
        public static void TestArray3b()
        {
            var list1 = new int[][] { new int[] { } };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject<int[][]>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);
            Assert.AreEqual(list1.Length, list2.Length);
            Assert.AreEqual(list2.Length, 1);
            Assert.AreEqual(list2[0].Length, 0);
            Assert.AreEqual(list1, list2);
        }

        [Test]
        public static void TestArray4a()
        {
            var list1 = new int[,] { };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);
            Assert.AreEqual(list1.Length, ((IList)list2).Count);
            Assert.AreEqual(list1, list2);
        }

        [Test]
        public static void TestArray4b()
        {
            var list1 = new int[,] { };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject<int[,]>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);
            Assert.AreEqual(list1.Length, list2.Length);
            Assert.AreEqual(list2.Rank, 2);
            Assert.AreEqual(list2.GetLength(0), 0);
            Assert.AreEqual(list2.GetLength(1), 0);
            Assert.AreEqual(list1, list2);
        }

        [Test]
        public static void TestArray5a()
        {
            var list1 = new int[,,] { };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);
            Assert.AreEqual(list1.Length, ((IList)list2).Count);
            Assert.AreEqual(list1, list2);
        }

        [Test]
        public static void TestArray5b()
        {
            var list1 = new int[,,] { };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject<int[, ,]>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);
            Assert.AreEqual(list1.Length, list2.Length);
            Assert.AreEqual(list2.Rank, 3);
            Assert.AreEqual(list2.GetLength(0), 0);
            Assert.AreEqual(list2.GetLength(1), 0);
            Assert.AreEqual(list2.GetLength(2), 0);
            Assert.AreEqual(list1, list2);
        }

        [Test]
        public static void TestArray6a()
        {
            var list1 = new int[,,,] { };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);
            Assert.AreEqual(list1.Length, ((IList)list2).Count);
            Assert.AreEqual(list1, list2);
        }

        [Test]
        public static void TestArray6b()
        {
            var list1 = new int[,,,] { };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject<int[, , ,]>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);
            Assert.AreEqual(list1.Length, list2.Length);
            Assert.AreEqual(list2.Rank, 4);
            Assert.AreEqual(list2.GetLength(0), 0);
            Assert.AreEqual(list2.GetLength(1), 0);
            Assert.AreEqual(list2.GetLength(2), 0);
            Assert.AreEqual(list2.GetLength(3), 0);
            Assert.AreEqual(list1, list2);
        }

        [Test]
        public static void TestArray7a()
        {
            var list1 = new int[][][] { 
				new int[][] { 
					new int[] { }, 
					new int[] { } 
				}, 
				new int[][] { }, 
				new int[][] {
					new int[] { }
				} 
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);
            Assert.AreEqual(list1.Length, ((IList)list2).Count);

            Assert.AreEqual(((IList)list2).Count, 3);

            Assert.AreEqual(((IList)((IList)list2)[0]).Count, 2);
            Assert.AreEqual(((IList)((IList)list2)[1]).Count, 0);
            Assert.AreEqual(((IList)((IList)list2)[2]).Count, 1);
        }

        [Test]
        public static void TestArray7b()
        {
            var list1 = new int[][][] { 
				new int[][] { 
					new int[] { }, 
					new int[] { } 
				}, 
				new int[][] { }, 
				new int[][] {
					new int[] { }
				} 
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject<int[][][]>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);
            Assert.AreEqual(list1.Length, list2.Length);

            Assert.AreEqual(list2.Length, 3);

            Assert.AreEqual(list2[0].Length, 2);
            Assert.AreEqual(list2[1].Length, 0);
            Assert.AreEqual(list2[2].Length, 1);

            Assert.AreEqual(list2[0][0].Length, 0);
            Assert.AreEqual(list2[0][1].Length, 0);
            Assert.AreEqual(list2[2][0].Length, 0);

            Assert.AreEqual(list1, list2);
        }

        [Test]
        public static void TestArray8a()
        {
            var list1 = new int[,,] { { }, { }, { } };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);
            Assert.AreEqual(list1.Length, ((Array)list2).Length);

            Assert.AreEqual(((Array)list2).Rank, 3);

            Assert.AreEqual(((Array)list2).GetLength(0), 3);
            Assert.AreEqual(((Array)list2).GetLength(1), 0);
            Assert.AreEqual(((Array)list2).GetLength(2), 0);
        }

        [Test]
        public static void TestArray8b()
        {
            var list1 = new int[,,] { { }, { }, { } };

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject<int[, ,]>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);
            Assert.AreEqual(list1.Length, list2.Length);

            Assert.AreEqual(list2.Rank, 3);

            Assert.AreEqual(list2.GetLength(0), 3);
            Assert.AreEqual(list2.GetLength(1), 0);
            Assert.AreEqual(list2.GetLength(2), 0);

            Assert.AreEqual(list1, list2);
        }

        [Test]
        public static void TestArray9a()
        {
            var list1 = new int[][,] { 
				new int[,] { }, 
				new int[,] { { 1, 2 } }, 
				new int[,] { { 3, 4 }, { 5, 6 } }, 
				new int[,] { { 7, 8, 9 }, { 10, 11, 12 }, { 13, 14, 15 } }
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject<int[][,]>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);
            Assert.AreEqual(list1.Length, list2.Length);

            Assert.AreEqual(list2.Length, 4);
            Assert.AreEqual(list2[0].Rank, 2);
            Assert.AreEqual(list2[1].Rank, 2);
            Assert.AreEqual(list2[2].Rank, 2);
            Assert.AreEqual(list2[3].Rank, 2);

            Assert.AreEqual(list2[0].GetLength(0), 0);
            Assert.AreEqual(list2[1].GetLength(0), 1);
            Assert.AreEqual(list2[1].GetLength(1), 2);
            Assert.AreEqual(list2[2].GetLength(0), 2);
            Assert.AreEqual(list2[2].GetLength(1), 2);
            Assert.AreEqual(list2[3].GetLength(0), 3);
            Assert.AreEqual(list2[3].GetLength(1), 3);

            Assert.AreEqual(list2[1][0, 0], 1);
            Assert.AreEqual(list2[1][0, 1], 2);

            Assert.AreEqual(list2[2][0, 0], 3);
            Assert.AreEqual(list2[2][0, 1], 4);
            Assert.AreEqual(list2[2][1, 0], 5);
            Assert.AreEqual(list2[2][1, 1], 6);

            Assert.AreEqual(list2[3][0, 0], 7);
            Assert.AreEqual(list2[3][0, 1], 8);
            Assert.AreEqual(list2[3][0, 2], 9);
            Assert.AreEqual(list2[3][1, 0], 10);
            Assert.AreEqual(list2[3][1, 1], 11);
            Assert.AreEqual(list2[3][1, 2], 12);
            Assert.AreEqual(list2[3][2, 0], 13);
            Assert.AreEqual(list2[3][2, 1], 14);
            Assert.AreEqual(list2[3][2, 2], 15);
        }

        [Test]
        public static void TestArray10a()
        {
            var list1 = new int[,][] { 
				{ 
					new int[] { 1, 2 }, 
					new int[] { 3, 4 } 
				}, 
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject<int[,][]>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);
            Assert.AreEqual(list1.Length, list2.Length);

            Assert.AreEqual(list2[0, 0][0], 1);
            Assert.AreEqual(list2[0, 0][1], 2);
            Assert.AreEqual(list2[0, 1][0], 3);
            Assert.AreEqual(list2[0, 1][1], 4);
        }

        [Test]
        public static void TestArray10b()
        {
            var list1 = new int[][,] { 
				new int[,] {
					{ 1, 2 }, 
					{ 3, 4 },
					{ 5, 6 }
				}, 
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject<int[][,]>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);

            Assert.AreEqual(list1.Length, list2.Length);

            Assert.AreEqual(list2[0][0, 0], 1);
            Assert.AreEqual(list2[0][0, 1], 2);
            Assert.AreEqual(list2[0][1, 0], 3);
            Assert.AreEqual(list2[0][1, 1], 4);
            Assert.AreEqual(list2[0][2, 0], 5);
            Assert.AreEqual(list2[0][2, 1], 6);
        }

        [Test]
        public static void TestArray11()
        {
            var list1 = new int[,][] { 
				{ 
					new int[] { 1 }, 
					new int[] { 2, 3 }
				}, 
				{ 
					new int[] { 4, 5, 6 }, 
					new int[] { 7, 8, 9, 10 }
				}, 
				{ 
					new int[] { 11, 12, 13, 14, 15 } ,
					new int[] { 16, 17, 18, 19, 20, 21 } 
				}
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject<int[,][]>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);

            Assert.AreEqual(list2.Rank, 2);

            Assert.AreEqual(list2[0, 0].Length, 1);
            Assert.AreEqual(list2[0, 1].Length, 2);
            Assert.AreEqual(list2[1, 0].Length, 3);
            Assert.AreEqual(list2[1, 1].Length, 4);
            Assert.AreEqual(list2[2, 0].Length, 5);
            Assert.AreEqual(list2[2, 1].Length, 6);

            Assert.AreEqual(list2[0, 0][0], 1);
            Assert.AreEqual(list2[0, 1][0], 2);
            Assert.AreEqual(list2[0, 1][1], 3);

            Assert.AreEqual(list2[1, 0][0], 4);
            Assert.AreEqual(list2[1, 0][1], 5);
            Assert.AreEqual(list2[1, 0][2], 6);
            Assert.AreEqual(list2[1, 1][0], 7);
            Assert.AreEqual(list2[1, 1][1], 8);
            Assert.AreEqual(list2[1, 1][2], 9);
            Assert.AreEqual(list2[1, 1][3], 10);

            Assert.AreEqual(list2[2, 0][0], 11);
            Assert.AreEqual(list2[2, 0][1], 12);
            Assert.AreEqual(list2[2, 0][2], 13);
            Assert.AreEqual(list2[2, 0][3], 14);
            Assert.AreEqual(list2[2, 0][4], 15);

            Assert.AreEqual(list2[2, 1][0], 16);
            Assert.AreEqual(list2[2, 1][1], 17);
            Assert.AreEqual(list2[2, 1][2], 18);
            Assert.AreEqual(list2[2, 1][3], 19);
            Assert.AreEqual(list2[2, 1][4], 20);
            Assert.AreEqual(list2[2, 1][5], 21);
        }

        [Test]
        public static void TestArray12()
        {
            var list1 = new int[][][,] { 
				new int[][,] { 
					new int[,] { { 1, 2 }, { 3, 4 } }, 
					new int[,] { { 5, 6 }, { 7, 8 } } 
				}, 
				new int[][,] { }, 
				new int[][,] { 
					new int[,] { { 9, 10 } }
				}, 
				new int[][,] {
					new int[,] { { 11, 12 }, { 13, 14 } }
				} 
			};

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.ArrayType = ArrayDeserializationType.ArrayDefined;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject<int[][][,]>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2.GetType().IsArray);
            Assert.AreEqual(list1.Length, list2.Length);

            Assert.AreEqual(list1, list2);
        }

        [Test]
        public static void TestArray13()
        {
            var jsonObj = JaysonConverter.ToObject("[\"900000\",null,[null,\"2570847921467975139\"]]");
            Assert.IsNotNull(jsonObj);
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
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            string json = JaysonConverter.ToJsonString(list1, jaysonSerializationSettings);
            var list2 = JaysonConverter.ToObject(json);

            Assert.IsNotNull(list2);
            Assert.IsTrue(list2 is ReadOnlyCollection<object>);

            var list22 = (ReadOnlyCollection<object>)list2;
            Assert.AreEqual(list1.Count, list22.Count);

            for (int i = 0; i < list1.Count - 1; i++)
            {
                Assert.AreEqual(list1[i], list22[i]);
            }

            Assert.IsTrue(list22[list22.Count - 1] is VerySimpleJsonValue);

            var vsjv1 = (VerySimpleJsonValue)list22[list22.Count - 1];
            Assert.IsTrue(vsjv1.Value is VerySimpleJsonValue);

            var vsjv2 = (VerySimpleJsonValue)vsjv1.Value;
            Assert.IsTrue(vsjv2.Value is bool && (bool)vsjv2.Value);
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
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;

            string json = JaysonConverter.ToJsonString(dict1, jaysonSerializationSettings);
            var dict2 = JaysonConverter.ToObject(json);

            Assert.IsNotNull(dict2);
            Assert.IsTrue(dict2 is ReadOnlyDictionary<object, object>);

            var dict22 = (ReadOnlyDictionary<object, object>)dict2;
            Assert.AreEqual(dict1.Count, dict22.Count);

            foreach (var kvp in dict1)
            {
                Assert.IsTrue(dict22.ContainsKey(kvp.Key));

                if (!kvp.Key.Equals((int?)13579))
                {
                    Assert.AreEqual(kvp.Value, dict22[kvp.Key]);
                }
                else
                {
                    Assert.IsTrue(dict22[kvp.Key] is VerySimpleJsonValue);

                    var vsjv1 = (VerySimpleJsonValue)dict22[kvp.Key];
                    Assert.IsTrue(vsjv1.Value is VerySimpleJsonValue);

                    var vsjv2 = (VerySimpleJsonValue)vsjv1.Value;
                    Assert.IsTrue(vsjv2.Value is bool && (bool)vsjv2.Value);
                }
            }
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

            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is IDictionary<string, object>);

            var dict2 = (IDictionary<string, object>)obj;
            Assert.AreEqual(dict2.Count, 2);
            Assert.IsTrue(dict2.ContainsKey("Value1"));
            Assert.IsTrue(dict2["Value1"].Equals("Hello"));
            Assert.IsTrue(dict2.ContainsKey("Value2"));
            Assert.IsTrue(dict2["Value2"].Equals("World"));
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
                    Formatting = JaysonFormatting.None
                };

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);

            Assert.IsTrue(json.Contains("\"Value1\":\"Hello\"") &&
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
                    Formatting = JaysonFormatting.Tab
                };

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);

            Assert.IsTrue(json.Contains("\"Value1\": \"Hello\"") &&
                json.Contains("\"Value2\": \"World\""));
        }

        [Test]
        public static void TestIncludeTypeInfoAuto1a()
        {
            var dto1 = new SimpleObj
            {
                Value1 = "Hello",
                Value2 = "World"
            };

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
                {
                    Formatting = JaysonFormatting.None,
                    TypeNameInfo = JaysonTypeNameInfo.TypeName,
                    TypeNames = JaysonTypeNameSerialization.Auto
                };

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);

            Assert.AreEqual(json, @"{""$type"":""Sweet.Jayson.Tests.SimpleObj"",""Value2"":""World"",""Value1"":""Hello""}");
        }


        [Test]
        public static void TestIncludeTypeInfoAuto1b()
        {
            var dto1 = TestClasses.GetTypedContainerDto() as TypedContainerDto;

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
                {
                    Formatting = JaysonFormatting.None,
                    TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
                    TypeNames = JaysonTypeNameSerialization.Auto
                };

            string json = JaysonConverter.ToJsonString(dto1, jaysonSerializationSettings);

            Assert.IsTrue(json.Contains("System.Object[], mscorlib"));
            Assert.IsTrue(!json.Contains("System.Collections.Generic.List`1[[System.Object[], mscorlib]], mscorlib"));
            Assert.IsTrue(!json.Contains("System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.Object, mscorlib]], mscorlib"));

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.UseDefaultValues = true;

            TypedContainerDto dto2 = JaysonConverter.ToObject<TypedContainerDto>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dto2);
            CompareTypedContainerDtos(dto1, dto2);
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
                    Formatting = JaysonFormatting.None,
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
                    Formatting = JaysonFormatting.None,
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
            dt1.Columns.Add(new DataColumn("col5", typeof(byte[])));

            dt1.Rows.Add(new object[] { null, true, new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Utc),
				new SimpleObj {
					Value1 = "Hello",
					Value2 = "World 1"
				},
				Encoding.UTF8.GetBytes ("Hello World 1")
			});
            dt1.Rows.Add(new object[] { "row2", false, new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Local),
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

            string json = JaysonConverter.ToJsonString(dt1, jaysonSerializationSettings);

            Assert.IsTrue(json.Contains("My DataTable 1") && json.Contains("myTableNamespace"));
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
                    Formatting = JaysonFormatting.Tab,
                    TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
                    TypeNames = JaysonTypeNameSerialization.Auto
                };

            string json = JaysonConverter.ToJsonString(ds, jaysonSerializationSettings);

            Assert.IsTrue(json.Contains("My DataSet") && json.Contains("My DataTable 1") &&
                json.Contains("myTableNamespace"));
        }

        [Test]
        public static void TestDeserializeSimpleDataTable1a()
        {
            DataTable dt1 = new DataTable("My DataTable 1", "myTableNamespace");
            dt1.Columns.Add(new DataColumn("col1", typeof(string), null, MappingType.Element));
            dt1.Columns.Add(new DataColumn("col2", typeof(bool)));
            dt1.Columns.Add(new DataColumn("col3", typeof(DateTime)));
            dt1.Columns.Add(new DataColumn("col4", typeof(SimpleObj)));
            dt1.Columns.Add(new DataColumn("col5", typeof(byte[])));

            dt1.Columns[0].ExtendedProperties.Add(1, 2m);
            dt1.Columns[0].ExtendedProperties.Add(3, 4m);

            dt1.Rows.Add(new object[] { null, true, new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Utc),
				new SimpleObj {
					Value1 = "Hello",
					Value2 = "World 1"
				},
				null
			});
            dt1.Rows.Add(new object[] { "row2", false, new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Local),
				new SimpleObjDerivative {
					Value1 = "Hello",
					Value2 = "My",
					Value3 = "World 2"
				}});

            dt1.Rows[0][4] = Encoding.UTF8.GetBytes("Hello World 1");

            dt1.ExtendedProperties.Add(5, 6m);
            dt1.ExtendedProperties.Add(7, 8m);

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
                {
                    Formatting = JaysonFormatting.None,
                    TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
                    TypeNames = JaysonTypeNameSerialization.Auto
                };

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = true;

            string json = JaysonConverter.ToJsonString(dt1, jaysonSerializationSettings);
            DataTable dt2 = JaysonConverter.ToObject<DataTable>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dt2);

            Assert.AreEqual(dt1.Columns.Count, dt2.Columns.Count);
            Assert.AreEqual(dt1.Rows.Count, dt2.Rows.Count);

            Assert.AreEqual(dt1.Columns[0].ExtendedProperties.Count, dt2.Columns[0].ExtendedProperties.Count);
            Assert.AreEqual(dt2.Columns[0].ExtendedProperties[1], 2m);
            Assert.AreEqual(dt2.Columns[0].ExtendedProperties[3], 4m);
            Assert.AreEqual(dt1.ExtendedProperties.Count, dt2.ExtendedProperties.Count);
            Assert.AreEqual(dt2.ExtendedProperties[5], 6m);
            Assert.AreEqual(dt2.ExtendedProperties[7], 8m);

            Assert.AreEqual(dt2.Rows[0][0], DBNull.Value);
            Assert.AreEqual((bool)dt2.Rows[0][1], true);
            Assert.AreEqual(dt2.Rows[0][2], new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Utc));
            Assert.IsTrue(dt2.Rows[0][3] is SimpleObj);
            Assert.AreEqual(((SimpleObj)dt2.Rows[0][3]).Value1, "Hello");
            Assert.AreEqual(((SimpleObj)dt2.Rows[0][3]).Value2, "World 1");
            Assert.AreEqual(((byte[])dt2.Rows[0][4])[0], Encoding.UTF8.GetBytes("Hello World 1")[0]);
            Assert.AreEqual(dt2.Rows[1][0], "row2");
            Assert.AreEqual((bool)dt2.Rows[1][1], false);
            Assert.AreEqual(dt2.Rows[1][2], new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Local));
            Assert.IsTrue(dt2.Rows[1][3] is SimpleObjDerivative);
            Assert.AreEqual(((SimpleObjDerivative)dt2.Rows[1][3]).Value1, "Hello");
            Assert.AreEqual(((SimpleObjDerivative)dt2.Rows[1][3]).Value2, "My");
            Assert.AreEqual(((SimpleObjDerivative)dt2.Rows[1][3]).Value3, "World 2");
        }

        [Test]
        public static void TestDeserializeSimpleDataTable1b()
        {
            DataTable dt1 = new DataTable("My DataTable 1", "myTableNamespace");
            dt1.Columns.Add(new DataColumn("col1", typeof(string), null, MappingType.Element));
            dt1.Columns.Add(new DataColumn("col2", typeof(bool)));
            dt1.Columns.Add(new DataColumn("col3", typeof(DateTime)));
            dt1.Columns.Add(new DataColumn("col4", typeof(SimpleObj)));
            dt1.Columns.Add(new DataColumn("col5", typeof(byte[])));

            dt1.Columns[0].ExtendedProperties.Add(1, 2m);
            dt1.Columns[0].ExtendedProperties.Add(3, 4m);

            dt1.Rows.Add(new object[] { null, true, new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Utc),
				new SimpleObj {
					Value1 = "Hello",
					Value2 = "World 1"
				}
			});
            dt1.Rows.Add(new object[] { "row2", false, new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Local),
				new SimpleObjDerivative {
					Value1 = "Hello",
					Value2 = "My",
					Value3 = "World 2"
				}
			});

            dt1.Rows[0][4] = Encoding.UTF8.GetBytes("Hello World 1");

            dt1.ExtendedProperties.Add(5, 6m);
            dt1.ExtendedProperties.Add(7, 8m);

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
                {
                    Formatting = JaysonFormatting.None,
                    TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
                    TypeNames = JaysonTypeNameSerialization.All
                };

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = true;

            string json = JaysonConverter.ToJsonString(dt1, jaysonSerializationSettings);
            DataTable dt2 = JaysonConverter.ToObject<DataTable>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(dt2);

            Assert.AreEqual(dt1.Columns.Count, dt2.Columns.Count);
            Assert.AreEqual(dt1.Rows.Count, dt2.Rows.Count);

            Assert.AreEqual(dt1.Columns[0].ExtendedProperties.Count, dt2.Columns[0].ExtendedProperties.Count);
            Assert.AreEqual(dt2.Columns[0].ExtendedProperties[1], 2m);
            Assert.AreEqual(dt2.Columns[0].ExtendedProperties[3], 4m);
            Assert.AreEqual(dt1.ExtendedProperties.Count, dt2.ExtendedProperties.Count);
            Assert.AreEqual(dt2.ExtendedProperties[5], 6m);
            Assert.AreEqual(dt2.ExtendedProperties[7], 8m);

            Assert.AreEqual(dt2.Rows[0][0], DBNull.Value);
            Assert.AreEqual((bool)dt2.Rows[0][1], true);
            Assert.AreEqual(dt2.Rows[0][2], new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Utc));
            Assert.IsTrue(dt2.Rows[0][3] is SimpleObj);
            Assert.AreEqual(((SimpleObj)dt2.Rows[0][3]).Value1, "Hello");
            Assert.AreEqual(((SimpleObj)dt2.Rows[0][3]).Value2, "World 1");
            Assert.AreEqual(((byte[])dt2.Rows[0][4])[0], Encoding.UTF8.GetBytes("Hello World 1")[0]);
            Assert.AreEqual(dt2.Rows[1][0], "row2");
            Assert.AreEqual((bool)dt2.Rows[1][1], false);
            Assert.AreEqual(dt2.Rows[1][2], new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Local));
            Assert.IsTrue(dt2.Rows[1][3] is SimpleObjDerivative);
            Assert.AreEqual(((SimpleObjDerivative)dt2.Rows[1][3]).Value1, "Hello");
            Assert.AreEqual(((SimpleObjDerivative)dt2.Rows[1][3]).Value2, "My");
            Assert.AreEqual(((SimpleObjDerivative)dt2.Rows[1][3]).Value3, "World 2");
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

            dt1.Columns[0].ExtendedProperties.Add(1, 2m);
            dt1.Columns[0].ExtendedProperties.Add(3, 4m);

            dt1.Rows.Add(new object[] { null, true, new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Utc),
				new SimpleObj {
					Value1 = "Hello",
					Value2 = "World 1"
				}});
            dt1.Rows.Add(new object[] { "row2", false, new DateTime (1972, 10, 25, 12, 45, 32, DateTimeKind.Local),
				new SimpleObjDerivative {
					Value1 = "Hello",
					Value2 = "My",
					Value3 = "World 2"
				}});

            dt1.ExtendedProperties.Add(5, 6m);
            dt1.ExtendedProperties.Add(7, 8m);

            ds1.Tables.Add(dt1);

            JaysonSerializationSettings jaysonSerializationSettings = new JaysonSerializationSettings
                {
                    Formatting = JaysonFormatting.Tab,
                    TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
                    TypeNames = JaysonTypeNameSerialization.Auto
                };

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = true;

            string json = JaysonConverter.ToJsonString(ds1, jaysonSerializationSettings);
            DataSet ds2 = JaysonConverter.ToObject<DataSet>(json, jaysonDeserializationSettings);

            Assert.IsTrue(json.Contains("My DataSet") && json.Contains("My DataTable 1") &&
                json.Contains("myTableNamespace"));

            Assert.IsNotNull(ds2);

            Assert.AreEqual(ds1.Tables.Count, ds2.Tables.Count);

            Assert.AreEqual(ds1.Tables[0].Columns.Count, ds2.Tables[0].Columns.Count);
            Assert.AreEqual(ds1.Tables[0].Columns[0].ExtendedProperties.Count, ds2.Tables[0].Columns[0].ExtendedProperties.Count);
            Assert.AreEqual(ds2.Tables[0].Columns[0].ExtendedProperties[1], 2m);
            Assert.AreEqual(ds2.Tables[0].Columns[0].ExtendedProperties[3], 4m);
            Assert.AreEqual(ds1.Tables[0].ExtendedProperties.Count, ds2.Tables[0].ExtendedProperties.Count);
            Assert.AreEqual(ds2.Tables[0].ExtendedProperties[5], 6m);
            Assert.AreEqual(ds2.Tables[0].ExtendedProperties[7], 8m);

            Assert.AreEqual(ds1.Tables[0].Rows.Count, ds2.Tables[0].Rows.Count);
            Assert.AreEqual(ds2.Tables[0].Rows[0][0], DBNull.Value);
            Assert.AreEqual(ds2.Tables[0].Rows[0][1], true);
            Assert.AreEqual(ds2.Tables[0].Rows[0][2], new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Utc));
            Assert.IsTrue(ds2.Tables[0].Rows[0][3] is SimpleObj);
            Assert.AreEqual(((SimpleObj)ds2.Tables[0].Rows[0][3]).Value1, "Hello");
            Assert.AreEqual(((SimpleObj)ds2.Tables[0].Rows[0][3]).Value2, "World 1");
            Assert.AreEqual(ds2.Tables[0].Rows[1][0], "row2");
            Assert.AreEqual(ds2.Tables[0].Rows[1][1], false);
            Assert.AreEqual(ds2.Tables[0].Rows[1][2], new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Local));
            Assert.IsTrue(ds2.Tables[0].Rows[1][3] is SimpleObjDerivative);
            Assert.AreEqual(((SimpleObjDerivative)ds2.Tables[0].Rows[1][3]).Value1, "Hello");
            Assert.AreEqual(((SimpleObjDerivative)ds2.Tables[0].Rows[1][3]).Value2, "My");
            Assert.AreEqual(((SimpleObjDerivative)ds2.Tables[0].Rows[1][3]).Value3, "World 2");
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
				new SimpleObjDerivative {
					Value1 = "Hello",
					Value2 = "My",
					Value3 = "World 2"
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
                    Formatting = JaysonFormatting.Tab,
                    TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
                    TypeNames = JaysonTypeNameSerialization.All
                };

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = true;

            string json = JaysonConverter.ToJsonString(ds1, jaysonSerializationSettings);
            DataSet ds2 = JaysonConverter.ToObject<DataSet>(json, jaysonDeserializationSettings);

            Assert.IsTrue(json.Contains("My DataSet") &&
                json.Contains("My DataTable 1") && json.Contains("myTableNamespace1") &&
                json.Contains("My DataTable 2") && json.Contains("myTableNamespace2"));

            Assert.IsNotNull(ds2);
            Assert.AreEqual(ds1.Tables.Count, ds2.Tables.Count);
            Assert.IsNotNull(ds2.Tables["My DataTable 1", "myTableNamespace1"]);
            Assert.IsNotNull(ds2.Tables["My DataTable 2", "myTableNamespace2"]);
            Assert.IsNotNull(ds2.Relations["dr1"]);
            Assert.AreEqual(ds2.ExtendedProperties["x4"], true);

            Assert.AreEqual(ds1.Tables[0].Columns.Count, ds2.Tables[0].Columns.Count);
            Assert.AreEqual(ds1.Tables[0].Columns[0].ExtendedProperties.Count, ds2.Tables[0].Columns[0].ExtendedProperties.Count);
            Assert.AreEqual(ds2.Tables[0].Columns[0].ExtendedProperties["x1"], "X1");
            Assert.AreEqual(ds1.Tables[0].ExtendedProperties.Count, ds2.Tables[0].ExtendedProperties.Count);
            Assert.AreEqual(ds2.Tables[0].ExtendedProperties["x2"], 2);

            Assert.AreEqual(ds1.Tables[0].Rows.Count, ds2.Tables[0].Rows.Count);
            Assert.AreEqual(ds2.Tables[0].Rows[0][0], 0);
            Assert.AreEqual(ds2.Tables[0].Rows[0][1], DBNull.Value);
            Assert.AreEqual(ds2.Tables[0].Rows[0][2], true);
            Assert.AreEqual(ds2.Tables[0].Rows[0][3], new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Utc));
            Assert.IsTrue(ds2.Tables[0].Rows[0][4] is SimpleObj);
            Assert.AreEqual(((SimpleObj)ds2.Tables[0].Rows[0][4]).Value1, "Hello");
            Assert.AreEqual(((SimpleObj)ds2.Tables[0].Rows[0][4]).Value2, "World 1");
            Assert.AreEqual(ds2.Tables[0].Rows[1][0], 1);
            Assert.AreEqual(ds2.Tables[0].Rows[1][1], "row2");
            Assert.AreEqual(ds2.Tables[0].Rows[1][2], false);
            Assert.AreEqual(ds2.Tables[0].Rows[1][3], new DateTime(1972, 10, 25, 12, 45, 32, DateTimeKind.Local));
            Assert.IsTrue(ds2.Tables[0].Rows[1][4] is SimpleObjDerivative);
            Assert.AreEqual(((SimpleObjDerivative)ds2.Tables[0].Rows[1][4]).Value1, "Hello");
            Assert.AreEqual(((SimpleObjDerivative)ds2.Tables[0].Rows[1][4]).Value2, "My");
            Assert.AreEqual(((SimpleObjDerivative)ds2.Tables[0].Rows[1][4]).Value3, "World 2");

            Assert.AreEqual(ds1.Tables[1].Columns.Count, ds2.Tables[1].Columns.Count);
            Assert.AreEqual(ds1.Tables[1].ExtendedProperties.Count, ds2.Tables[1].ExtendedProperties.Count);
            Assert.AreEqual(ds2.Tables[1].ExtendedProperties["x3"], 3);

            Assert.AreEqual(ds1.Tables[1].Rows.Count, ds2.Tables[1].Rows.Count);
            Assert.AreEqual(ds2.Tables[1].Rows[0][0], 0L);
            Assert.AreEqual(ds2.Tables[1].Rows[0][1], 0L);
            Assert.AreEqual(ds2.Tables[1].Rows[0][2], "string1");
            Assert.AreEqual(ds2.Tables[1].Rows[1][0], 1L);
            Assert.AreEqual(ds2.Tables[1].Rows[1][1], 1L);
            Assert.AreEqual(ds2.Tables[1].Rows[1][2], "string2");
            Assert.AreEqual(ds2.Tables[1].Rows[2][0], 2L);
            Assert.AreEqual(ds2.Tables[1].Rows[2][1], 0L);
            Assert.AreEqual(ds2.Tables[1].Rows[2][2], "string3");
            Assert.AreEqual(ds2.Tables[1].Rows[3][0], 3L);
            Assert.AreEqual(ds2.Tables[1].Rows[3][1], 1L);
            Assert.AreEqual(ds2.Tables[1].Rows[3][2], "string4");
            Assert.AreEqual(ds2.Tables[1].Rows[4][0], 4L);
            Assert.AreEqual(ds2.Tables[1].Rows[4][1], 0L);
            Assert.AreEqual(ds2.Tables[1].Rows[4][2], "string5");
            Assert.AreEqual(ds2.Tables[1].Rows[5][0], 5L);
            Assert.AreEqual(ds2.Tables[1].Rows[5][1], 1L);
            Assert.AreEqual(ds2.Tables[1].Rows[5][2], "string6");
            Assert.AreEqual(ds2.Tables[1].Rows[6][0], 6L);
            Assert.AreEqual(ds2.Tables[1].Rows[6][1], 0L);
            Assert.AreEqual(ds2.Tables[1].Rows[6][2], "string7");
            Assert.AreEqual(ds2.Tables[1].Rows[7][0], 7L);
            Assert.AreEqual(ds2.Tables[1].Rows[7][1], 1L);
            Assert.AreEqual(ds2.Tables[1].Rows[7][2], "string8");
            Assert.AreEqual(ds2.Tables[1].Rows[8][0], 8L);
            Assert.AreEqual(ds2.Tables[1].Rows[8][1], 0L);
            Assert.AreEqual(ds2.Tables[1].Rows[8][2], "string9");
            Assert.AreEqual(ds2.Tables[1].Rows[9][0], 9L);
            Assert.AreEqual(ds2.Tables[1].Rows[9][1], 1L);
            Assert.AreEqual(ds2.Tables[1].Rows[9][2], "string10");
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
                    Formatting = JaysonFormatting.Tab,
                    TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
                    TypeNames = JaysonTypeNameSerialization.All
                };

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = true;

            string json = JaysonConverter.ToJsonString(ds1, jaysonSerializationSettings);
            DataSet ds2 = JaysonConverter.ToObject<DataSet>(json, jaysonDeserializationSettings);
            DataSet ds3 = JaysonConverter.ToObject<CustomDataSet1>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(ds2);
            Assert.IsNotNull(ds3);

            Assert.AreEqual(ds1.Tables.Count, ds2.Tables.Count);
            Assert.AreEqual(ds1.Tables.Count, ds3.Tables.Count);

            Assert.AreEqual(ds2.Tables[0].Columns[0].ExtendedProperties["x1"], "X1");
            Assert.AreEqual(ds3.Tables[0].Columns[0].ExtendedProperties["x1"], "X1");

            Assert.AreEqual(ds2.Tables[0].ExtendedProperties["x2"], 2);
            Assert.AreEqual(ds3.Tables[0].ExtendedProperties["x2"], 2);

            Assert.AreEqual(ds2.Tables[1].ExtendedProperties["x3"], 3);
            Assert.AreEqual(ds3.Tables[1].ExtendedProperties["x3"], 3);

            Assert.AreEqual(ds2.ExtendedProperties["x4"], true);
            Assert.AreEqual(ds3.ExtendedProperties["x4"], true);

            for (int i = 0; i < ds1.Tables.Count; i++)
            {
                Assert.AreEqual(ds1.Tables[i].TableName, ds2.Tables[i].TableName);
                Assert.AreEqual(ds1.Tables[i].Rows.Count, ds2.Tables[i].Rows.Count);
                Assert.AreEqual(ds1.Tables[i].Columns.Count, ds2.Tables[i].Columns.Count);

                Assert.AreEqual(ds1.Tables[i].TableName, ds3.Tables[i].TableName);
                Assert.AreEqual(ds1.Tables[i].Rows.Count, ds3.Tables[i].Rows.Count);
                Assert.AreEqual(ds1.Tables[i].Columns.Count, ds3.Tables[i].Columns.Count);

                for (int j = 0; j < ds1.Tables[i].Rows.Count; j++)
                {
                    for (int k = 0; k < ds1.Tables[i].Columns.Count; k++)
                    {
                        Assert.AreEqual(ds1.Tables[i].Rows[j][k], ds2.Tables[i].Rows[j][k]);
                        Assert.AreEqual(ds1.Tables[i].Rows[j][k], ds3.Tables[i].Rows[j][k]);
                    }
                }
            }
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
                    Formatting = JaysonFormatting.Tab,
                    TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
                    TypeNames = JaysonTypeNameSerialization.None
                };

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = true;

            string json = JaysonConverter.ToJsonString(ds1, jaysonSerializationSettings);
            DataSet ds2 = JaysonConverter.ToObject<DataSet>(json, jaysonDeserializationSettings);
            DataSet ds3 = JaysonConverter.ToObject<CustomDataSet1>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(ds2);
            Assert.IsNotNull(ds3);

            Assert.AreEqual(ds1.Tables.Count, ds2.Tables.Count);
            Assert.AreEqual(ds1.Tables.Count, ds3.Tables.Count);

            Assert.AreEqual(ds2.Tables[0].Columns[0].ExtendedProperties["x1"], "X1");
            Assert.AreEqual(ds3.Tables[0].Columns[0].ExtendedProperties["x1"], "X1");

            Assert.AreEqual(ds2.Tables[0].ExtendedProperties["x2"], 2);
            Assert.AreEqual(ds3.Tables[0].ExtendedProperties["x2"], 2);

            Assert.AreEqual(ds2.Tables[1].ExtendedProperties["x3"], 3);
            Assert.AreEqual(ds3.Tables[1].ExtendedProperties["x3"], 3);

            Assert.AreEqual(ds2.ExtendedProperties["x4"], true);
            Assert.AreEqual(ds3.ExtendedProperties["x4"], true);

            for (int i = 0; i < ds1.Tables.Count; i++)
            {
                Assert.AreEqual(ds1.Tables[i].TableName, ds2.Tables[i].TableName);
                Assert.AreEqual(ds1.Tables[i].Rows.Count, ds2.Tables[i].Rows.Count);
                Assert.AreEqual(ds1.Tables[i].Columns.Count, ds2.Tables[i].Columns.Count);

                Assert.AreEqual(ds1.Tables[i].TableName, ds3.Tables[i].TableName);
                Assert.AreEqual(ds1.Tables[i].Rows.Count, ds3.Tables[i].Rows.Count);
                Assert.AreEqual(ds1.Tables[i].Columns.Count, ds3.Tables[i].Columns.Count);

                for (int j = 0; j < ds1.Tables[i].Rows.Count; j++)
                {
                    for (int k = 0; k < ds1.Tables[i].Columns.Count; k++)
                    {
                        Assert.AreEqual(ds1.Tables[i].Rows[j][k], ds2.Tables[i].Rows[j][k]);
                        Assert.AreEqual(ds1.Tables[i].Rows[j][k], ds3.Tables[i].Rows[j][k]);
                    }
                }
            }
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
                    Formatting = JaysonFormatting.Tab,
                    TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly,
                    TypeNames = JaysonTypeNameSerialization.None
                };

            var jsonObj = JaysonConverter.ToJsonObject(ds1, jaysonSerializationSettings);

            Assert.IsNotNull(jsonObj);
        }

        [Test]
        public static void TestHashSet1a()
        {
            var hset1 = new HashSet<int>();
            hset1.Add(1);
            hset1.Add(2);
            hset1.Add(3);
            hset1.Add(4);
            hset1.Add(5);
            hset1.Add(6);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(hset1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            HashSet<int> hset2 = JaysonConverter.ToObject<HashSet<int>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(hset2);
            Assert.AreEqual(hset2.Count, 6);
            Assert.IsTrue(hset2.SetEquals(hset1));
        }

        [Test]
        public static void TestHashSet1b()
        {
            var hset1 = new HashSet<int>();
            hset1.Add(1);
            hset1.Add(2);
            hset1.Add(3);
            hset1.Add(4);
            hset1.Add(5);
            hset1.Add(6);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(hset1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            HashSet<int> hset2 = JaysonConverter.ToObject<HashSet<int>>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(hset2);
            Assert.AreEqual(hset2.Count, 6);
            Assert.IsTrue(hset2.SetEquals(hset1));
        }

        [Test]
        public static void TestHashTable1a()
        {
            var htable1 = new Hashtable();
            htable1.Add(1, 2m);
            htable1.Add(3, 4m);
            htable1.Add(5, 6m);
            htable1.Add(7, 8m);
            htable1.Add(9, 10m);
            htable1.Add(11, 12m);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.None;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(htable1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            Hashtable htable2 = JaysonConverter.ToObject<Hashtable>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(htable2);
            Assert.AreEqual(htable2.Count, 6);
            Assert.AreEqual(2, htable2[1]);
            Assert.AreEqual(4, htable2[3]);
            Assert.AreEqual(6, htable2[5]);
            Assert.AreEqual(8, htable2[7]);
            Assert.AreEqual(10, htable2[9]);
            Assert.AreEqual(12, htable2[11]);
        }

        [Test]
        public static void TestHashTable1b()
        {
            var htable1 = new Hashtable();
            htable1.Add(1, 2m);
            htable1.Add(3, 4m);
            htable1.Add(5, 6m);
            htable1.Add(7, 8m);
            htable1.Add(9, 10m);
            htable1.Add(11, 12m);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(htable1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            Hashtable htable2 = JaysonConverter.ToObject<Hashtable>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(htable2);
            Assert.AreEqual(htable2.Count, 6);
            Assert.AreEqual(2m, htable2[1]);
            Assert.AreEqual(4m, htable2[3]);
            Assert.AreEqual(6m, htable2[5]);
            Assert.AreEqual(8m, htable2[7]);
            Assert.AreEqual(10m, htable2[9]);
            Assert.AreEqual(12m, htable2[11]);
        }

        [Test]
        public static void TestHashTable1c()
        {
            var htable1 = new Hashtable();
            htable1.Add(1, 2m);
            htable1.Add(3, 4m);
            htable1.Add(5, 6m);
            htable1.Add(7, 8m);
            htable1.Add(9, 10m);
            htable1.Add(11, 12m);

            JaysonSerializationSettings jaysonSerializationSettings = JaysonSerializationSettings.DefaultClone();
            jaysonSerializationSettings.TypeNames = JaysonTypeNameSerialization.All;
            jaysonSerializationSettings.Formatting = JaysonFormatting.Tab;
            jaysonSerializationSettings.IgnoreNullValues = false;
            jaysonSerializationSettings.CaseSensitive = false;
            jaysonSerializationSettings.DateFormatType = JaysonDateFormatType.Microsoft;
            jaysonSerializationSettings.DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
            jaysonSerializationSettings.UseGlobalTypeNames = true;

            JaysonDeserializationSettings jaysonDeserializationSettings = JaysonDeserializationSettings.DefaultClone();
            jaysonDeserializationSettings.CaseSensitive = false;

            string json = JaysonConverter.ToJsonString(htable1, jaysonSerializationSettings);
            JaysonConverter.Parse(json, jaysonDeserializationSettings);
            Hashtable htable2 = JaysonConverter.ToObject<Hashtable>(json, jaysonDeserializationSettings);

            Assert.IsNotNull(htable2);
            Assert.AreEqual(htable2.Count, 6);
            Assert.AreEqual(2m, htable2[1]);
            Assert.AreEqual(4m, htable2[3]);
            Assert.AreEqual(6m, htable2[5]);
            Assert.AreEqual(8m, htable2[7]);
            Assert.AreEqual(10m, htable2[9]);
            Assert.AreEqual(12m, htable2[11]);
        }
    }
}

