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
using System.Collections.Generic;
using System.Globalization;

namespace Sweet.Jayson
{
    public static class JaysonConstants
    {
        # region Constants

        public const int CacheCapacity = 10000;
        public const int CacheInitialCapacity = 100;
        public const int ListCapacity = 5;
        public const int DictionaryCapacity = 5;

        public const string True = "true";
        public const string False = "false";

        public const string EmptyJsonString = "\"\"";

        public const string TimeSpanDefaultFormat = "c";
        public const string DateIso8601Format = "yyyy-MM-ddTHH:mm%K";
        public const string DateDefaultFormat = "yyyy-MM-ddTHH:mm:ss.fffffff%K";
        public const string DateMicrosoftJsonFormat = "/Date({0})/";
        public const string DateMicrosoftJsonFormatEscaped = "\\/Date({0})\\/";

        public const string SingleCommentTag = "//";
        public const string MultiCommentTag = "/**/";

        internal const string MicrosoftDateFormatStart = "/Date(";
        internal const string MicrosoftDateFormatEnd = ")/";

        internal const string Null = "null";

        internal const string IntMinValue = "-2147483648";
        internal const string LongMinValue = "-9223372036854775808";

        internal const string JScriptDateZero = "new Date(0)";
        internal const string MicrosoftDateZero = "\"/Date(0)/\"";
        internal const string MicrosoftDateZeroEscaped = "\"\\/Date(0)\\/\"";

        public const string GuidEmpty = "00000000-0000-0000-0000-000000000000";
        public const string GuidEmptyStr = "\"" + JaysonConstants.GuidEmpty + "\"";

        # endregion Constants

        # region Static Members

        public const int FloatMinExponent = -38;
        public const int FloatMaxExponent = 38;
        public const int FloatSignificantDigits = 7;

        public const int DoubleNanExponent = -324;
        public const int DoubleMinExponent = -308;
        public const int DoubleMaxExponent = 308;
        public const int DoubleSignificantDigits = 15;
        
        public const int DecimalMinExponent = -28;
        public const int DecimalMaxExponent = 28;
        public const int DecimalSignificantDigits = 28;

        public const int IntMaxExponent = 10;
        public const int LongMaxExponent = 19;
        public const int ULongMaxExponent = 20;

        internal static readonly int MicrosoftDateFormatLen = "/Date()/".Length;
        internal static readonly int MicrosoftDateFormatStartLen = "/Date(".Length;
        internal static readonly int MicrosoftDateFormatEndLen = ")/".Length;

        public static readonly string[] DateDefaultFormats = new string[] {
			DateIso8601Format,
			DateDefaultFormat
		};

        public static readonly char CharA10 = (char)('A' - 10);
        public static readonly char Chara10 = (char)('a' - 10);

        public static readonly object[] EmptyObjArray = new object[0];

        public static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        public static readonly Type DefaultArrayType = typeof(object[]);
        public static readonly Type DefaultListType = typeof(List<object>);
        public static readonly Type DefaultDictionaryType = typeof(Dictionary<string, object>);

        public static readonly decimal LongMaxValueAsDecimal = Convert.ToDecimal(long.MaxValue);
        public static readonly decimal LongMinValueAsDecimal = Convert.ToDecimal(long.MinValue);

        public static readonly long IntMaxValueAsLong = (long)int.MaxValue;
        public static readonly long IntMinValueAsLong = (long)int.MinValue;

        public static readonly double FloatMaxValueAsDouble = (double)float.MaxValue;
        public static readonly double FloatMinValueAsDouble = (double)float.MinValue;

        public static readonly char[] NewLine = Environment.NewLine.ToCharArray();
        public static readonly string[] Indentation = new string[61];
        public static readonly string[] IndentationTabbed = new string[61];

        // Min: 621355968000000000L, Max: 642830688000000000L
        public static readonly long UnixEpochMinValue = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;
        public static readonly long UnixEpochMaxValue = (new DateTime(2038, 1, 19, 0, 0, 0, DateTimeKind.Utc)).Ticks;

        public static readonly DateTime DateTimeUtcMinValue = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static readonly DateTime DateTimeUnixEpochMinValue = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static readonly DateTime DateTimeUnixEpochMaxValue = new DateTime(2038, 1, 19, 0, 0, 0, DateTimeKind.Utc);
        public static readonly DateTime DateTimeUnixEpochMinValueUnspecified = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        public static readonly int[] PowerOf10Int;
        public static readonly long[] PowerOf10Long;
        public static readonly ulong[] PowerOf10ULong;
        public static readonly float[] PowerOf10Float;
        public static readonly double[] PowerOf10Double;
        public static readonly decimal[] PowerOf10Decimal;

        public static readonly JaysonTuple<string, object>[] UnordinaryNumbers = new JaysonTuple<string, object> [] {
            new JaysonTuple<string, object>(decimal.MinValue.ToString(JaysonConstants.InvariantCulture), decimal.MinValue),
            new JaysonTuple<string, object>(decimal.MaxValue.ToString(JaysonConstants.InvariantCulture), decimal.MaxValue),
            new JaysonTuple<string, object>(double.MinValue.ToString(JaysonConstants.InvariantCulture), double.MinValue),
            new JaysonTuple<string, object>(double.MaxValue.ToString(JaysonConstants.InvariantCulture), double.MaxValue),
            new JaysonTuple<string, object>(float.MinValue.ToString(JaysonConstants.InvariantCulture), float.MinValue),
            new JaysonTuple<string, object>(float.MaxValue.ToString(JaysonConstants.InvariantCulture), float.MaxValue),
            new JaysonTuple<string, object>(double.NaN.ToString(JaysonConstants.InvariantCulture), double.NaN),
            new JaysonTuple<string, object>(double.NegativeInfinity.ToString(JaysonConstants.InvariantCulture), double.NegativeInfinity),
            new JaysonTuple<string, object>(double.PositiveInfinity.ToString(JaysonConstants.InvariantCulture), double.PositiveInfinity),
            new JaysonTuple<string, object>(double.Epsilon.ToString(JaysonConstants.InvariantCulture), double.Epsilon),
            new JaysonTuple<string, object>(float.NaN.ToString(JaysonConstants.InvariantCulture), float.NaN),
            new JaysonTuple<string, object>(float.NegativeInfinity.ToString(JaysonConstants.InvariantCulture), float.NegativeInfinity),
            new JaysonTuple<string, object>(float.PositiveInfinity.ToString(JaysonConstants.InvariantCulture), float.PositiveInfinity),
            new JaysonTuple<string, object>(float.Epsilon.ToString(JaysonConstants.InvariantCulture), float.Epsilon),
        };

        # endregion Static Members

        static JaysonConstants()
        {
            PowerOf10Float = new float[FloatMaxExponent];
            PowerOf10Float[0] = 1f;

            var pflt = 1f;
            for (var i = 1; i < FloatMaxExponent; i++) {
                pflt *= 10;
                PowerOf10Float[i] = pflt;
            }

            PowerOf10Double = new double[DoubleMaxExponent];
            PowerOf10Double[0] = 1d;

            var pdbl = 1d;
            for (var i = 1; i < DoubleMaxExponent; i++) {
                pdbl *= 10;
                PowerOf10Double[i] = pdbl;
            }

            PowerOf10Decimal = new decimal[DecimalMaxExponent];
            PowerOf10Decimal[0] = 1m;

            var pdcl = 1m;
            for (var i = 1; i < DecimalMaxExponent; i++) {
                pdcl *= 10;
                PowerOf10Decimal[i] = pdcl;
            }

            PowerOf10ULong = new ulong[ULongMaxExponent];
            PowerOf10ULong[0] = 1;

            var pul = (ulong)1;
            for (var i = 1; i < ULongMaxExponent; i++) {
                pul *= 10;
                PowerOf10ULong[i] = pul;
            }

            PowerOf10Long = new long[LongMaxExponent];
            PowerOf10Long[0] = 1;

            var pl = 1L;
            for (var i = 1; i < LongMaxExponent; i++) {
                pl *= 10;
                PowerOf10Long[i] = pl;
            }

            PowerOf10Int = new int[IntMaxExponent];
            PowerOf10Int[0] = 1;

            var pi = 1;
            for (var i = 1; i < IntMaxExponent; i++) {
                pi *= 10;
                PowerOf10Int[i] = pi;
            }
        }
    }
}