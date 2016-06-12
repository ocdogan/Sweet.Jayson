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

        # endregion Constants

        # region Static Members

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

        public static readonly char[] NewLine = Environment.NewLine.ToCharArray();
        public static readonly string[] Indentation = new string[61];
        public static readonly string[] IndentationTabbed = new string[61];

        public static readonly TimeZone CurrentTimeZone = TimeZone.CurrentTimeZone;

        // Min: 621355968000000000L, Max: 642830688000000000L
        public static readonly long UnixEpochMinValue = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;
        public static readonly long UnixEpochMaxValue = (new DateTime(2038, 1, 19, 0, 0, 0, DateTimeKind.Utc)).Ticks;

        public static readonly DateTime DateTimeUtcMinValue = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static readonly DateTime DateTimeUnixEpochMinValue = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static readonly DateTime DateTimeUnixEpochMaxValue = new DateTime(2038, 1, 19, 0, 0, 0, DateTimeKind.Utc);
        public static readonly DateTime DateTimeUnixEpochMinValueUnspecified = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        public static readonly long[] PowerOf10Long = new long[] { 
			1L,
			10L,
			100L,
			1000L,
			10000L,
			100000L,
			1000000L,
			10000000L,
			100000000L,
			1000000000L,
			10000000000L,
			100000000000L,
			1000000000000L,
			10000000000000L,
			100000000000000L,
			1000000000000000L,
			10000000000000000L,
			100000000000000000L,
			1000000000000000000L
		};

        public static readonly double[] PowerOf10Double = new double[] { 
			1d,
			10d,
			100d,
			1000d,
			10000d,
			100000d,
			1000000d,
			10000000d,
			100000000d,
			1000000000d,
			10000000000d,
			100000000000d,
			1000000000000d,
			10000000000000d,
			100000000000000d,
			1000000000000000d,
			10000000000000000d,
			100000000000000000d,
			1000000000000000000d,
			10000000000000000000d
		};

        public static readonly decimal[] PowerOf10Decimal = new decimal[] { 
			1m,
			10m,
			100m,
			1000m,
			10000m,
			100000m,
			1000000m,
			10000000m,
			100000000m,
			1000000000m,
			10000000000m,
			100000000000m,
			1000000000000m,
			10000000000000m,
			100000000000000m,
			1000000000000000m,
			10000000000000000m,
			100000000000000000m,
			1000000000000000000m,
			10000000000000000000m
		};

        # endregion Static Members

        static JaysonConstants()
        {
            string newLine = Environment.NewLine;

            Indentation[0] = newLine;
            IndentationTabbed[0] = newLine;

            for (int i = 1; i < Indentation.Length; i++)
            {
                Indentation[i] = newLine.PadRight(newLine.Length + (4 * i), ' ');
                IndentationTabbed[i] = newLine.PadRight(newLine.Length + i, '\t');
            }
        }
    }
}