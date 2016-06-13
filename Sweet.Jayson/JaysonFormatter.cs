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
using System.Globalization;
using System.Text;

namespace Sweet.Jayson
{
    # region JsonFormatter

    internal sealed class JaysonFormatter
    {
        # region Static Readonly Members

        private const int Char255 = 255;
        private const int IntStringLen = 11;
        private const int LongStringLen = 20;

        private const string DefaultDateTimeFormat = "yyyy-MM-ddTHH:mm:ss%K";

        // always use dot separator for doubles
        private static readonly CultureInfo FormatingCulture = CultureInfo.InvariantCulture;

        private static readonly char[] EscapedChars = new char[] { '\\', '/', '\b', '\f', '\n', '\r', '\t' };
        private static readonly int EscapedCharsEndPosition = EscapedChars.Length - 1;

        private static readonly int ABase = 'A' - 10;
        private static readonly int ZeroBase = (int)'0';
        private static readonly long IntMinBarrier = (long)int.MinValue;
        private static readonly long IntMaxBarrier = (long)int.MaxValue;

        public readonly string NumberFormat;
        public readonly string DateTimeFormat;
        public readonly string DateTimeOffsetFormat;
        public readonly string TimeSpanFormat;
        public readonly bool ConvertDecimalToDouble;
        public readonly bool GuidAsByteArray;
        public readonly bool UseEnumNames;
        public readonly bool EscapeChars;
        public readonly bool EscapeUnicodeChars;

        public readonly JaysonDateFormatType DateFormatType;
        public readonly JaysonDateTimeZoneType DateTimeZoneType;

        # endregion Static Readonly Members

        public JaysonFormatter()
            : this(null, null, JaysonDateFormatType.Iso8601, null, JaysonDateTimeZoneType.KeepAsIs,
                null, false, true, false, false, false)
        { }

        public JaysonFormatter(string numberFormat, string timeSpanFormat, JaysonDateFormatType dateFormatType,
            string dateTimeFormat, JaysonDateTimeZoneType dateTimeZoneType, string dateTimeOffsetFormat,
            bool useEnumNames, bool escapeChars, bool escapeUnicodeChars, bool convertDecimalToDouble,
            bool guidAsByteArray)
        {
            UseEnumNames = useEnumNames;
            EscapeChars = escapeChars;
            EscapeUnicodeChars = escapeUnicodeChars;
            ConvertDecimalToDouble = convertDecimalToDouble;
            DateFormatType = dateFormatType;
            DateTimeZoneType = dateTimeZoneType;
            DateTimeOffsetFormat = dateTimeOffsetFormat;
            GuidAsByteArray = guidAsByteArray;

            NumberFormat = ((numberFormat != null && numberFormat.Length > 0) ? numberFormat : "G");
            TimeSpanFormat = ((timeSpanFormat != null && timeSpanFormat.Length > 0) ? timeSpanFormat :
                JaysonConstants.TimeSpanDefaultFormat);
            DateTimeFormat = ((dateTimeFormat != null && dateTimeFormat.Length > 0) ? dateTimeFormat :
                (dateFormatType == JaysonDateFormatType.CustomUnixEpoch ? 
                    (escapeChars ? JaysonConstants.DateMicrosoftJsonFormatEscaped : JaysonConstants.DateMicrosoftJsonFormat) :
                    JaysonConstants.DateIso8601Format));
        }

        # region Instance Methods

        private void WriteDatePart(char[] chArr, int part, int start, int length)
        {
            int index = start + length - 1;

            if (part < 10)
            {
                chArr[index--] = (char)('0' + part);
            }
            else
            {
                int mod;
                while (part != 0)
                {
                    mod = part % 10;
                    part = part / 10;
                    chArr[index--] = (char)('0' + mod);
                }
            }

            while (index >= start)
            {
                chArr[index--] = '0';
            }
        }

        public void Format(DateTime dt, StringBuilder builder)
        {
            DateTimeKind kind = dt.Kind;

            switch (DateTimeZoneType)
            {
                case JaysonDateTimeZoneType.KeepAsIs:
                    break;
                case JaysonDateTimeZoneType.ConvertToUtc:
                    if (kind != DateTimeKind.Utc)
                    {
                        dt = JaysonCommon.ToUniversalTime(dt);
                        kind = dt.Kind;
                    }
                    break;
                case JaysonDateTimeZoneType.ConvertToLocal:
                    if (kind == DateTimeKind.Utc)
                    {
                        dt = JaysonCommon.ToLocalTime(dt);
                        kind = dt.Kind;
                    }
                    break;
                default:
                    break;
            }

            switch (DateFormatType)
            {
                case JaysonDateFormatType.Iso8601:
                    {
                        char[] chArr;
                        if (kind == DateTimeKind.Utc)
                        {
                            chArr = new char[22];
                        }
                        else
                        {
                            chArr = new char[26];
                        }

                        int index = 0;
                        chArr[index++] = '"';

                        WriteDatePart(chArr, dt.Year, index, 4);
                        index += 4;

                        chArr[index++] = '-';
                        WriteDatePart(chArr, dt.Month, index, 2);
                        index += 2;

                        chArr[index++] = '-';
                        WriteDatePart(chArr, dt.Day, index, 2);
                        index += 2;

                        chArr[index++] = 'T';
                        WriteDatePart(chArr, dt.Hour, index, 2);
                        index += 2;

                        chArr[index++] = ':';
                        WriteDatePart(chArr, dt.Minute, index, 2);
                        index += 2;

                        chArr[index++] = ':';
                        WriteDatePart(chArr, dt.Second, index, 2);
                        index += 2;

                        if (kind == DateTimeKind.Utc)
                        {
                            chArr[index++] = 'Z';
                        }
                        else
                        {
                            TimeSpan tz = TimeZone.CurrentTimeZone.GetUtcOffset(dt);
                            if (tz.Ticks != 0L)
                            {
                                chArr[index++] = tz.Ticks > 0 ? '+' : '-';

                                WriteDatePart(chArr, tz.Hours, index, 2);
                                index += 2;

                                WriteDatePart(chArr, tz.Minutes, index, 2);
                                index += 2;
                            }
                        }

                        chArr[index++] = '"';

                        builder.Append(chArr, 0, index);
                    }
                    break;
                case JaysonDateFormatType.JScript:
                case JaysonDateFormatType.Microsoft:
                    {
                        long epoc = JaysonCommon.ToUnixTimeMsec(dt);

                        if (epoc <= 0L)
                        {
                            if (DateFormatType == JaysonDateFormatType.Microsoft)
                            {
                                builder.Append(EscapeChars ? JaysonConstants.MicrosoftDateZeroEscaped : JaysonConstants.MicrosoftDateZero);
                            }
                            else
                            {
                                builder.Append(JaysonConstants.JScriptDateZero);
                            }
                        }
                        else
                        {
                            int index = 24;
                            char[] chArr = new char[24];

                            if (kind != DateTimeKind.Utc)
                            {
                                TimeSpan tz = JaysonConstants.CurrentTimeZone.GetUtcOffset(dt);
                                if (tz.Ticks != 0L)
                                {
                                    index -= 2;
                                    WriteDatePart(chArr, tz.Minutes, index, 2);

                                    index -= 2;
                                    WriteDatePart(chArr, tz.Hours, index, 2);

                                    chArr[--index] = tz.Ticks > 0 ? '+' : '-';
                                }
                            }

                            int mod;
                            while (epoc > 0L)
                            {
                                mod = (int)(epoc % 10);
                                epoc = epoc / 10;

                                chArr[--index] = (char)('0' + mod);
                            }

                            if (DateFormatType == JaysonDateFormatType.Microsoft)
                            {
                                if (EscapeChars)
                                {
                                    builder.Append("\"\\/Date(");
                                    builder.Append(chArr, index, 24 - index);
                                    builder.Append(")\\/\"");
                                } 
                                else
                                {
                                    builder.Append("\"/Date(");
                                    builder.Append(chArr, index, 24 - index);
                                    builder.Append(")/\"");
                                }
                            }
                            else
                            {
                                builder.Append("new Date(");
                                builder.Append(chArr, index, 24 - index);
                                builder.Append(')');
                            }
                        }
                    }
                    break;
                case JaysonDateFormatType.UnixEpoch:
                    {
                        long epoc = JaysonCommon.ToUnixTimeMsec(dt);

                        int index = 20;
                        char[] chArr = new char[20];

                        int mod;
                        while (epoc > 0L)
                        {
                            mod = (int)(epoc % 10);
                            epoc = epoc / 10;

                            chArr[--index] = (char)('0' + mod);
                        }

                        builder.Append(chArr, index, 20 - index);
                    }
                    break;
                case JaysonDateFormatType.CustomDate:
                    {
                        FormatString(dt.ToString(DateTimeFormat, FormatingCulture), builder, EscapeChars,
                            EscapeUnicodeChars);
                    }
                    break;
                case JaysonDateFormatType.CustomUnixEpoch:
                    {
                        long epoc = JaysonCommon.ToUnixTimeMsec(dt);

                        FormatString(epoc.ToString(DateTimeFormat, FormatingCulture), builder, EscapeChars,
                            EscapeUnicodeChars);
                    }
                    break;
                default:
                    break;
            }
        }

        public void Format(DateTimeOffset dto, StringBuilder builder)
        {
            DateTime dt = dto.DateTime;
            DateTimeKind kind = dt.Kind;

            switch (DateTimeZoneType)
            {
                case JaysonDateTimeZoneType.KeepAsIs:
                    break;
                case JaysonDateTimeZoneType.ConvertToUtc:
                    if (kind != DateTimeKind.Utc)
                    {
                        dto = dto.ToUniversalTime();
                        dt = dto.DateTime;
                        kind = dt.Kind;
                    }
                    break;
                case JaysonDateTimeZoneType.ConvertToLocal:
                    if (kind == DateTimeKind.Utc)
                    {
                        dto = dto.ToLocalTime();
                        dt = dto.DateTime;
                        kind = dt.Kind;
                    }
                    break;
            }

            switch (DateFormatType)
            {
                case JaysonDateFormatType.Iso8601:
                    {
                        char[] chArr = (kind == DateTimeKind.Utc) ? new char[19] : new char[23];

                        int index = 0;
                        chArr[index++] = '"';

                        WriteDatePart(chArr, dt.Year, index, 4);
                        index += 4;

                        chArr[index++] = '-';
                        WriteDatePart(chArr, dt.Month, index, 2);
                        index += 2;

                        chArr[index++] = '-';
                        WriteDatePart(chArr, dt.Day, index, 2);
                        index += 2;

                        chArr[index++] = 'T';
                        WriteDatePart(chArr, dt.Hour, index, 2);
                        index += 2;

                        chArr[index++] = ':';
                        WriteDatePart(chArr, dt.Minute, index, 2);
                        index += 2;

                        if (kind == DateTimeKind.Utc)
                        {
                            chArr[index++] = 'Z';
                        }
                        else
                        {
                            TimeSpan tz = dto.Offset;
                            if (tz.Ticks != 0L)
                            {
                                chArr[index++] = tz.Ticks > 0 ? '+' : '-';

                                WriteDatePart(chArr, tz.Hours, index, 2);
                                index += 2;

                                WriteDatePart(chArr, tz.Minutes, index, 2);
                                index += 2;
                            }
                        }

                        chArr[index++] = '"';

                        builder.Append(chArr, 0, index);
                    }
                    break;
                case JaysonDateFormatType.JScript:
                case JaysonDateFormatType.Microsoft:
                    {
                        long epoc = JaysonCommon.ToUnixTimeMsec(dt);

                        if (epoc <= 0L)
                        {
                            if (DateFormatType == JaysonDateFormatType.Microsoft)
                            {
                                builder.Append(EscapeChars ? JaysonConstants.MicrosoftDateZeroEscaped : JaysonConstants.MicrosoftDateZero);
                            }
                            else
                            {
                                builder.Append(JaysonConstants.JScriptDateZero);
                            }
                        }
                        else
                        {
                            int index = 24;
                            char[] chArr = new char[24];

                            if (kind != DateTimeKind.Utc)
                            {
                                TimeSpan tz = dto.Offset;
                                if (tz.Ticks != 0L)
                                {
                                    index -= 2;
                                    WriteDatePart(chArr, tz.Minutes, index, 2);

                                    index -= 2;
                                    WriteDatePart(chArr, tz.Hours, index, 2);

                                    chArr[--index] = tz.Ticks > 0 ? '+' : '-';
                                }
                            }

                            int mod;
                            while (epoc > 0L)
                            {
                                mod = (int)(epoc % 10);
                                epoc = epoc / 10;

                                chArr[--index] = (char)('0' + mod);
                            }

                            if (DateFormatType == JaysonDateFormatType.Microsoft)
                            {
                                if (EscapeChars)
                                {
                                    builder.Append("\"\\/Date(");
                                    builder.Append(chArr, index, 24 - index);
                                    builder.Append(")\\/\"");
                                } 
                                else
                                {
                                    builder.Append("\"/Date(");
                                    builder.Append(chArr, index, 24 - index);
                                    builder.Append(")/\"");
                                }
                            }
                            else
                            {
                                builder.Append("new Date(");
                                builder.Append(chArr, index, 24 - index);
                                builder.Append(')');
                            }
                        }
                    }
                    break;
                case JaysonDateFormatType.UnixEpoch:
                    {
                        if (kind != DateTimeKind.Utc)
                        {
                            dt = dto.UtcDateTime;
                        }

                        long epoc = JaysonCommon.ToUnixTimeMsec(dt);

                        int index = 20;
                        char[] chArr = new char[20];

                        int mod;
                        while (epoc > 0L)
                        {
                            mod = (int)(epoc % 10);
                            epoc = epoc / 10;

                            chArr[--index] = (char)('0' + mod);
                        }

                        builder.Append(chArr, index, 20 - index);
                    }
                    break;
                case JaysonDateFormatType.CustomDate:
                    {
                        FormatString(dto.ToString(DateTimeOffsetFormat, FormatingCulture), builder, EscapeChars,
                            EscapeUnicodeChars);
                    }
                    break;
                case JaysonDateFormatType.CustomUnixEpoch:
                    {
                        long epoc = JaysonCommon.ToUnixTimeMsec(dt);

                        FormatString(epoc.ToString(DateTimeFormat, FormatingCulture), builder, EscapeChars,
                            EscapeUnicodeChars);
                    }
                    break;
                default:
                    break;
            }
        }

        public void Format(object obj, Type objType, StringBuilder builder)
        {
            if (obj == null)
            {
                builder.Append(JaysonConstants.Null);
                return;
            }

            JaysonTypeCode jtc = JaysonTypeInfo.GetJTypeCode(objType);

            // Do not change the check order
            switch (jtc)
            {
                case JaysonTypeCode.String:
                    {
                        FormatString((string)obj, builder, EscapeChars, EscapeUnicodeChars);
                        break;
                    }
                case JaysonTypeCode.Int:
                    {
                        Format((int)obj, builder);
                        break;
                    }
                case JaysonTypeCode.Bool:
                    {
                        builder.Append((bool)obj ? JaysonConstants.True : JaysonConstants.False);
                        break;
                    }
                case JaysonTypeCode.Long:
                    {
                        Format((long)obj, builder);
                        break;
                    }
                case JaysonTypeCode.DateTime:
                    {
                        Format((DateTime)obj, builder);
                        break;
                    }
                case JaysonTypeCode.Double:
                    {
                        // if user supplied own format use it
                        builder.Append(((double)obj).ToString(NumberFormat, FormatingCulture));
                        break;
                    }
                case JaysonTypeCode.Short:
                    {
                        builder.Append(((short)obj).ToString(FormatingCulture));
                        break;
                    }
                case JaysonTypeCode.IntNullable:
                    {
                        Format(((int?)obj).Value, builder);
                        break;
                    }
                case JaysonTypeCode.BoolNullable:
                    {
                        builder.Append(((bool?)obj).Value ? JaysonConstants.True : JaysonConstants.False);
                        break;
                    }
                case JaysonTypeCode.LongNullable:
                    {
                        Format(((long?)obj).Value, builder);
                        break;
                    }
                case JaysonTypeCode.Byte:
                    {
                        builder.Append(((byte)obj).ToString(FormatingCulture));
                        break;
                    }
                case JaysonTypeCode.Float:
                    {
                        // if user supplied own format use it
                        builder.Append(((float)obj).ToString(NumberFormat, FormatingCulture));
                        break;
                    }
                case JaysonTypeCode.Decimal:
                    {
                        // if user supplied own format use it
                        if (ConvertDecimalToDouble)
                        {
                            builder.Append(Convert.ToDouble((decimal)obj).ToString(NumberFormat, FormatingCulture));
                        }
                        else
                        {
                            builder.Append(((decimal)obj).ToString(NumberFormat, FormatingCulture));
                        }
                        break;
                    }
                case JaysonTypeCode.DateTimeNullable:
                    {
                        Format(((DateTime?)obj).Value, builder);
                        break;
                    }
                case JaysonTypeCode.DoubleNullable:
                    {
                        // if user supplied own format use it
                        builder.Append(((double?)obj).Value.ToString(NumberFormat, FormatingCulture));
                        break;
                    }
                case JaysonTypeCode.ShortNullable:
                    {
                        builder.Append(((short?)obj).Value.ToString(FormatingCulture));
                        break;
                    }
                case JaysonTypeCode.ByteNullable:
                    {
                        builder.Append(((byte?)obj).Value.ToString(FormatingCulture));
                        break;
                    }
                case JaysonTypeCode.FloatNullable:
                    {
                        // if user supplied own format use it
                        builder.Append(((float?)obj).Value.ToString(NumberFormat, FormatingCulture));
                        break;
                    }
                case JaysonTypeCode.DecimalNullable:
                    {
                        // if user supplied own format use it
                        if (ConvertDecimalToDouble)
                        {
                            builder.Append(Convert.ToDouble(((decimal?)obj).Value).ToString(NumberFormat, FormatingCulture));
                        }
                        else
                        {
                            builder.Append(((decimal?)obj).Value.ToString(NumberFormat, FormatingCulture));
                        }
                        break;
                    }
                case JaysonTypeCode.Guid:
                    {
                        Guid guid = (Guid)obj;

                        builder.Append('"');
                        if (GuidAsByteArray)
                        {
                            builder.Append('!');
                            builder.Append(Convert.ToBase64String(guid.ToByteArray()));
                        }
                        else if (guid == Guid.Empty)
                        {
                            builder.Append(JaysonConstants.GuidEmpty);
                        }
                        else
                        {
                            builder.Append(JaysonCommon.AsciiToUpper(guid.ToString("D")));
                        }
                        builder.Append('"');
                        break;
                    }
                case JaysonTypeCode.Char:
                case JaysonTypeCode.CharNullable:
                    {
                        if (jtc == JaysonTypeCode.CharNullable)
                        {
                            obj = ((char?)obj).Value;
                        }

                        char ch = (char)obj;
                        builder.Append('"');

                        if (!(EscapeChars || EscapeUnicodeChars))
                        {
                            builder.Append(ch);
                        }
                        else
                        {
                            string chStr = ToJsonChar(ch, EscapeUnicodeChars);
                            if (chStr == null)
                            {
                                builder.Append(ch);
                            }
                            else
                            {
                                if (chStr.Length == 4)
                                {
                                    builder.Append('\\');
                                    builder.Append('u');
                                }
                                builder.Append(chStr);
                            }
                        }
                        builder.Append('"');
                        break;
                    }
                case JaysonTypeCode.UInt:
                    {
                        builder.Append(((uint)obj).ToString(FormatingCulture));
                        break;
                    }
                case JaysonTypeCode.ULong:
                    {
                        builder.Append(((ulong)obj).ToString(FormatingCulture));
                        break;
                    }
                case JaysonTypeCode.UShort:
                    {
                        builder.Append(((ushort)obj).ToString(FormatingCulture));
                        break;
                    }
                case JaysonTypeCode.SByte:
                    {
                        builder.Append(((sbyte)obj).ToString(FormatingCulture));
                        break;
                    }
                case JaysonTypeCode.TimeSpan:
                    {
                        builder.Append('"');
#if !(NET3500 || NET3000 || NET2000)
                        builder.Append(((TimeSpan)obj).ToString(TimeSpanFormat, FormatingCulture));
#else
					builder.Append(((TimeSpan)obj).ToString());
#endif
                        builder.Append('"');
                        break;
                    }
                case JaysonTypeCode.TimeSpanNullable:
                    {
                        builder.Append('"');
#if !(NET3500 || NET3000 || NET2000)
                        builder.Append(((TimeSpan?)obj).Value.ToString(TimeSpanFormat, FormatingCulture));
#else
					builder.Append(((TimeSpan?)obj).Value.ToString());
#endif
                        builder.Append('"');
                        break;
                    }
                case JaysonTypeCode.UIntNullable:
                    {
                        builder.Append(((uint?)obj).Value.ToString(FormatingCulture));
                        break;
                    }
                case JaysonTypeCode.ULongNullable:
                    {
                        builder.Append(((ulong?)obj).Value.ToString(FormatingCulture));
                        break;
                    }
                case JaysonTypeCode.UShortNullable:
                    {
                        builder.Append(((ushort?)obj).Value.ToString(FormatingCulture));
                        break;
                    }
                case JaysonTypeCode.SByteNullable:
                    {
                        builder.Append(((sbyte?)obj).Value.ToString(FormatingCulture));
                        break;
                    }
                case JaysonTypeCode.GuidNullable:
                    {
                        Guid guid = ((Guid?)obj).Value;

                        builder.Append('"');
                        if (GuidAsByteArray)
                        {
                            builder.Append('!');
                            builder.Append(Convert.ToBase64String(guid.ToByteArray()));
                        }
                        else if (guid == Guid.Empty)
                        {
                            builder.Append(JaysonConstants.GuidEmpty);
                        }
                        else
                        {
                            builder.Append(JaysonCommon.AsciiToUpper(guid.ToString("D")));
                        }
                        builder.Append('"');
                        break;
                    }
                case JaysonTypeCode.DateTimeOffset:
                    {
                        Format((DateTimeOffset)obj, builder);
                        break;
                    }
                case JaysonTypeCode.DateTimeOffsetNullable:
                    {
                        Format(((DateTimeOffset?)obj).Value, builder);
                        break;
                    }
                default:
                    // format everything else normally
                    FormatString(obj is IFormattable ? ((IFormattable)obj).ToString(null, FormatingCulture) : obj.ToString(),
                        builder, EscapeChars, EscapeUnicodeChars);
                    break;
            }
        }

        public string Format(object obj, Type objType)
        {
            if (obj == null)
            {
                return JaysonConstants.Null;
            }

            JaysonTypeCode jtc = JaysonTypeInfo.GetJTypeCode(objType);

            // Do not change the check order
            switch (jtc)
            {
                case JaysonTypeCode.String:
                    {
                        string str = (string)obj;
                        if (str.Length == 0)
                        {
                            return JaysonConstants.EmptyJsonString;
                        }

                        if (!(EscapeChars || EscapeUnicodeChars))
                        {
                            return "\"" + str + "\"";
                        }

                        return "\"" + EncodeUnicodeString(str, EscapeUnicodeChars) + "\"";
                    }
                case JaysonTypeCode.Int:
                    return Format((int)obj);
                case JaysonTypeCode.Bool:
                    return (bool)obj ? JaysonConstants.True : JaysonConstants.False;
                case JaysonTypeCode.Long:
                    return Format((long)obj);
                case JaysonTypeCode.DateTime:
                    {
                        StringBuilder builder = new StringBuilder(32, int.MaxValue);
                        Format((DateTime)obj, builder);
                        return builder.ToString();
                    }
                case JaysonTypeCode.Double:
                    // if user supplied own format use it
                    return ((double)obj).ToString(NumberFormat, FormatingCulture);
                case JaysonTypeCode.Short:
                    return ((short)obj).ToString(FormatingCulture);
                case JaysonTypeCode.Guid:
                    {
                        Guid guid = (Guid)obj;
                        if (GuidAsByteArray)
                        {
                            return "\"!" + Convert.ToBase64String(guid.ToByteArray()) + "\"";
                        }
                        if (guid == Guid.Empty)
                        {
                            return JaysonConstants.GuidEmptyStr;
                        }
                        return "\"" + JaysonCommon.AsciiToUpper(guid.ToString("D")) + "\"";
                    }
                case JaysonTypeCode.IntNullable:
                    return Format(((int?)obj).Value);
                case JaysonTypeCode.BoolNullable:
                    return ((bool?)obj).Value ? JaysonConstants.True : JaysonConstants.False;
                case JaysonTypeCode.LongNullable:
                    return Format(((long?)obj).Value);
                case JaysonTypeCode.Byte:
                    return ((byte)obj).ToString(FormatingCulture);
                case JaysonTypeCode.Float:
                    // if user supplied own format use it
                    return ((float)obj).ToString(NumberFormat, FormatingCulture);
                case JaysonTypeCode.Decimal:
                    // if user supplied own format use it
                    if (ConvertDecimalToDouble)
                    {
                        return Convert.ToDouble((decimal)obj).ToString(NumberFormat, FormatingCulture);
                    }
                    return ((decimal)obj).ToString(NumberFormat, FormatingCulture);
                case JaysonTypeCode.DateTimeNullable:
                    {
                        StringBuilder builder = new StringBuilder(32, int.MaxValue);
                        Format(((DateTime?)obj).Value, builder);
                        return builder.ToString();
                    }
                case JaysonTypeCode.DoubleNullable:
                    // if user supplied own format use it
                    return ((double?)obj).Value.ToString(NumberFormat, FormatingCulture);
                case JaysonTypeCode.ShortNullable:
                    return ((short?)obj).Value.ToString(FormatingCulture);
                case JaysonTypeCode.ByteNullable:
                    return ((byte?)obj).Value.ToString(FormatingCulture);
                case JaysonTypeCode.FloatNullable:
                    // if user supplied own format use it
                    return ((float?)obj).Value.ToString(NumberFormat, FormatingCulture);
                case JaysonTypeCode.DecimalNullable:
                    // if user supplied own format use it
                    if (ConvertDecimalToDouble)
                    {
                        return Convert.ToDouble(((decimal?)obj).Value).ToString(NumberFormat, FormatingCulture);
                    }
                    return ((decimal?)obj).Value.ToString(NumberFormat, FormatingCulture);
                case JaysonTypeCode.Char:
                case JaysonTypeCode.CharNullable:
                    {
                        if (jtc == JaysonTypeCode.CharNullable)
                        {
                            obj = ((char?)obj).Value;
                        }

                        char ch = (char)obj;

                        if (!(EscapeChars || EscapeUnicodeChars))
                        {
                            return new string(new char[3] { '"', ch, '"' });
                        }
                        else
                        {
                            string chStr = ToJsonChar(ch, EscapeUnicodeChars);
                            if (chStr == null)
                            {
                                return new string(new char[3] { '"', ch, '"' });
                            }

                            if (chStr.Length == 4)
                            {
                                return new string(new char[8] { '"', '\\', 'u', chStr[0], chStr[1],
                                    chStr[2], chStr[3], '"'
                                });
                            }

                            return new string(new char[4] { '"', chStr[0], chStr[1], '"' });
                        }
                    }
                case JaysonTypeCode.UInt:
                    return ((uint)obj).ToString(FormatingCulture);
                case JaysonTypeCode.ULong:
                    return ((ulong)obj).ToString(FormatingCulture);
                case JaysonTypeCode.UShort:
                    return ((ushort)obj).ToString(FormatingCulture);
                case JaysonTypeCode.SByte:
                    return ((sbyte)obj).ToString(FormatingCulture);
                case JaysonTypeCode.TimeSpan:
#if !(NET3500 || NET3000 || NET2000)
                    return "\"" + ((TimeSpan)obj).ToString(TimeSpanFormat, FormatingCulture) + "\"";
#else
				return "\"" + ((TimeSpan)obj).ToString () + "\"";
#endif
                case JaysonTypeCode.TimeSpanNullable:
#if !(NET3500 || NET3000 || NET2000)
                    return "\"" + ((TimeSpan?)obj).Value.ToString(TimeSpanFormat, FormatingCulture) + "\"";
#else
				return "\"" + ((TimeSpan?)obj).Value.ToString () + "\"";
#endif
                case JaysonTypeCode.UIntNullable:
                    return ((uint?)obj).Value.ToString(FormatingCulture);
                case JaysonTypeCode.ULongNullable:
                    return ((ulong?)obj).Value.ToString(FormatingCulture);
                case JaysonTypeCode.UShortNullable:
                    return ((ushort?)obj).Value.ToString(FormatingCulture);
                case JaysonTypeCode.SByteNullable:
                    return ((sbyte?)obj).Value.ToString(FormatingCulture);
                case JaysonTypeCode.GuidNullable:
                    {
                        Guid guid = ((Guid?)obj).Value;
                        if (GuidAsByteArray)
                        {
                            return "\"!" + Convert.ToBase64String(guid.ToByteArray()) + "\"";
                        }
                        if (guid == Guid.Empty)
                        {
                            return JaysonConstants.GuidEmptyStr;
                        }
                        return "\"" + JaysonCommon.AsciiToUpper(guid.ToString("D")) + "\"";
                    }
                case JaysonTypeCode.DateTimeOffset:
                    return "\"" + ((DateTimeOffset)obj).ToString(FormatingCulture) + "\"";
                case JaysonTypeCode.DateTimeOffsetNullable:
                    return "\"" + ((DateTimeOffset?)obj).Value.ToString(FormatingCulture) + "\"";
                default:
                    {
                        // format everything else normally
                        StringBuilder remainingBuilder = new StringBuilder(48, int.MaxValue);
                        FormatString(obj is IFormattable ? ((IFormattable)obj).ToString(null, FormatingCulture) : obj.ToString(),
                            remainingBuilder, EscapeChars, EscapeUnicodeChars);
                        return remainingBuilder.ToString();
                    }
            }
        }

        public void Format(object obj, StringBuilder builder)
        {
            if (obj == null)
            {
                builder.Append(JaysonConstants.Null);
                return;
            }
            Format(obj, obj.GetType(), builder);
        }

        # endregion Instance Methods

        # region Static Methods

        private static string IntToHex(int value, int len = 0)
        {
            char[] chArr = new char[8];
            if (value == 0)
            {
                if (len > 1)
                {
                    for (int i = 0; i < len; i++)
                    {
                        chArr[i] = '0';
                    }
                    return new String(chArr, 0, len);
                }
                return "0";
            }

            if (len > 8)
            {
                len = 8;
            }

            int mod;
            int index = 8;

            while (value != 0)
            {
                mod = value % 16;
                value /= 16;

                chArr[--index] = (char)(mod + (mod < 10 ? ZeroBase : ABase));
            }

            int hlen = 8 - index;
            if (len > 0)
            {
                if (len > hlen)
                {
                    int to = 7 - len;
                    for (int i = 7 - hlen; i > to; i--)
                    {
                        chArr[i] = '0';
                    }
                }
                hlen = len;
            }

            return new String(chArr, 8 - hlen, hlen);
        }

        private static string ToJsonChar(char ch, bool escapeUnicodeChars = false)
        {
            switch (ch)
            {
                case '\\':
                    {
                        return "\\\\";
                    }
                case '"':
                    {
                        return "\\\"";
                    }
                case '/':
                    {
                        return "\\/";
                    }
                case '\b':
                    {
                        return "\\b";
                    }
                case '\f':
                    {
                        return "\\f";
                    }
                case '\n':
                    {
                        return "\\n";
                    }
                case '\r':
                    {
                        return "\\r";
                    }
                case '\t':
                    {
                        return "\\t";
                    }
                default:
                    {
                        if (ch < ' ' || (escapeUnicodeChars && ch > Char255))
                        {
                            return IntToHex(ch, 4);
                        }

                        return null;
                    }
            }
        }

        public static string EscapeChar(char ch)
        {
            string result = ToJsonChar(ch, true);
            if (result == null)
            {
                return ch.ToString();
            }
            if (result.Length == 4)
            {
                return new string(new char[6] { '\\', 'u', result[0], result[1],
                    result[2], result[3] });
            }
            return result;
        }

        public static bool NeedsEscape(string str, bool escapeUnicodeChars, out int escapePosition)
        {
            escapePosition = -1;
            if (str != null)
            {
                int length = str.Length;
                if (length > 0)
                {
                    char ch;
                    for (int i = 0; i < length; i++)
                    {
                        ch = str[i];
                        if (ch < ' ' || (escapeUnicodeChars && ch > Char255))
                        {
                            escapePosition = i;
                            return true;
                        }

                        for (int j = EscapedCharsEndPosition; j > -1; j--)
                        {
                            if (ch == EscapedChars[j])
                            {
                                escapePosition = i;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static string EncodeUnicodeString(string str, bool escapeUnicodeChars = false)
        {
            int escapePosition;
            if (!NeedsEscape(str, escapeUnicodeChars, out escapePosition))
            {
                return str;
            }

            StringBuilder builder = new StringBuilder((str != null ? Math.Max(str.Length, 20) : 20), int.MaxValue);

            EncodeUnicodeStringInternal(str, builder, escapePosition, escapeUnicodeChars);
            return builder.ToString();
        }

        public static void EncodeUnicodeString(string str, StringBuilder builder, bool escapeUnicodeChars = false)
        {
            if (!String.IsNullOrEmpty(str))
            {
                EncodeUnicodeStringInternal(str, builder, 0, escapeUnicodeChars);
            }
        }

        private static void EncodeUnicodeStringInternal(string str, StringBuilder builder, int escapePosition,
            bool escapeUnicodeChars)
        {
            if (escapePosition > 0)
            {
                builder.Append(str, 0, escapePosition);
            }

            int len;
            int length = str.Length;
            int index = escapePosition;
            int startPos = escapePosition;

            string encodedStr;

            for (; index < length; index++)
            {
                encodedStr = ToJsonChar(str[index], escapeUnicodeChars);
                if (encodedStr != null)
                {
                    len = index - startPos;
                    if (len > 1)
                    {
                        builder.Append(str, startPos, len);
                    }
                    else if (len == 1)
                    {
                        builder.Append(str[startPos]);
                    }

                    startPos = index + 1;
                    if (encodedStr.Length == 4)
                    {
                        builder.Append('\\');
                        builder.Append('u');
                    }
                    builder.Append(encodedStr);
                }
            }

            len = index - startPos;
            if (len > 1)
            {
                builder.Append(str, startPos, len);
            }
            else if (len == 1)
            {
                builder.Append(str[startPos]);
            }
        }

        public void Format(string str, StringBuilder builder)
        {
            if (str == null)
            {
                builder.Append(JaysonConstants.Null);
                return;
            }

            if (str.Length == 0)
            {
                builder.Append('"');
                builder.Append('"');
                return;
            }

            builder.Append('"');
            if (!(EscapeChars || EscapeUnicodeChars))
            {
                builder.Append(str);
            }
            else
            {
                EncodeUnicodeStringInternal(str, builder, 0, EscapeUnicodeChars);
            }
            builder.Append('"');
        }

        public static void FormatString(string str, StringBuilder builder, bool escapeChars = true,
            bool escapeUnicodeChars = false)
        {
            if (str == null)
            {
                builder.Append(JaysonConstants.Null);
                return;
            }

            if (str.Length == 0)
            {
                builder.Append('"');
                builder.Append('"');
                return;
            }

            builder.Append('"');
            if (!(escapeChars || escapeUnicodeChars))
            {
                builder.Append(str);
            }
            else
            {
                EncodeUnicodeStringInternal(str, builder, 0, escapeUnicodeChars);
            }
            builder.Append('"');
        }

        public static void Format(int value, StringBuilder builder)
        {
            bool minus = (value < 0);
            if (minus)
            {
                if (value == int.MinValue)
                {
                    builder.Append(JaysonConstants.IntMinValue);
                    return;
                }

                value = -value;
            }

            char[] chArr = new char[IntStringLen];

            int mod;
            int index = IntStringLen;

            do
            {
                mod = (value % 10) + ZeroBase;
                value /= 10;

                chArr[--index] = (char)mod;
            } while (value > 0);

            if (minus)
            {
                chArr[--index] = '-';
            }

            builder.Append(chArr, index, IntStringLen - index);
        }

        public static string Format(int value)
        {
            if (value == 0)
            {
                return "0";
            }

            bool minus = (value < 0);
            if (minus)
            {
                if (value == int.MinValue)
                {
                    return JaysonConstants.IntMinValue;
                }

                value = -value;
            }

            char[] chArr = new char[IntStringLen];

            int mod;
            int index = IntStringLen;

            do
            {
                mod = (value % 10) + ZeroBase;
                value /= 10;

                chArr[--index] = (char)mod;
            } while (value > 0);

            if (minus)
            {
                chArr[--index] = '-';
            }

            return new String(chArr, index, IntStringLen - index);
        }

        public static void Format(long value, StringBuilder builder)
        {
            if (value >= IntMinBarrier && value <= IntMaxBarrier)
            {
                Format((int)value, builder);
                return;
            }

            bool minus = (value < 0L);
            if (minus)
            {
                if (value == long.MinValue)
                {
                    builder.Append(JaysonConstants.LongMinValue);
                    return;
                }

                value = -value;
            }

            char[] chArr = new char[LongStringLen];

            int mod;
            int index = LongStringLen;

            do
            {
                mod = ZeroBase + (int)(value % 10L);
                value /= 10L;

                chArr[--index] = (char)mod;
            } while (value > 0);

            if (minus)
            {
                chArr[--index] = '-';
            }

            builder.Append(chArr, index, LongStringLen - index);
        }

        public static string Format(long value)
        {
            if (value >= IntMinBarrier && value <= IntMaxBarrier)
            {
                return Format((int)value);
            }

            if (value == 0L)
            {
                return "0";
            }

            bool minus = (value < 0L);
            if (minus)
            {
                if (value == long.MinValue)
                {
                    return JaysonConstants.LongMinValue;
                }

                value = -value;
            }

            char[] chArr = new char[LongStringLen];

            int mod;
            int index = LongStringLen;

            do
            {
                mod = ZeroBase + (int)(value % 10L);
                value /= 10L;

                chArr[--index] = (char)mod;
            } while (value > 0L);

            if (minus)
            {
                chArr[--index] = '-';
            }

            return new String(chArr, index, LongStringLen - index);
        }

        public static string ToString(object obj, Type objType)
        {
            if (obj == null)
            {
                return null;
            }

            // Do not change the check order
            if (objType == typeof(string))
            {
                return (string)obj;
            }

            // Do not change the check order
            if (objType == typeof(int))
            {
                return Format((int)obj);
            }

            // Do not change the check order
            if (objType == typeof(bool))
            {
                return (bool)obj ? JaysonConstants.True : JaysonConstants.False;
            }

            // Do not change the check order
            if (objType == typeof(long))
            {
                return Format((long)obj);
            }

            // Do not change the check order
            if (objType == typeof(DateTime))
            {
                return ((DateTime)obj).ToString(DefaultDateTimeFormat, FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(double))
            {
                // if user supplied own format use it
                return ((double)obj).ToString(FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(short))
            {
                return ((short)obj).ToString(FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(int?))
            {
                return Format(((int?)obj).Value);
            }

            // Do not change the check order
            if (objType == typeof(bool?))
            {
                return ((bool?)obj).Value ? JaysonConstants.True : JaysonConstants.False;
            }

            // Do not change the check order
            if (objType == typeof(long?))
            {
                return Format(((long?)obj).Value);
            }

            // Do not change the check order
            if (objType == typeof(byte))
            {
                return ((byte)obj).ToString(FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(float))
            {
                return ((float)obj).ToString(FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(decimal))
            {
                return ((decimal)obj).ToString(FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(double?))
            {
                return ((double?)obj).Value.ToString(FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(short?))
            {
                return ((short?)obj).Value.ToString(FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(DateTime?))
            {
                return ((DateTime?)obj).Value.ToString(DefaultDateTimeFormat, FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(byte?))
            {
                return ((byte?)obj).Value.ToString(FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(float?))
            {
                return ((float?)obj).Value.ToString(FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(decimal?))
            {
                return ((decimal?)obj).Value.ToString(FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(char))
            {
                return ((char)obj).ToString();
            }

            // Do not change the check order
            if (objType == typeof(uint))
            {
                return ((uint)obj).ToString(FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(ulong))
            {
                return ((ulong)obj).ToString(FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(ushort))
            {
                return ((ushort)obj).ToString(FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(sbyte))
            {
                return ((sbyte)obj).ToString(FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(TimeSpan))
            {
                return ((TimeSpan)obj).ToString();
            }

            // Do not change the check order
            if (objType == typeof(TimeSpan?))
            {
                return ((TimeSpan?)obj).Value.ToString();
            }

            // Do not change the check order
            if (objType == typeof(char?))
            {
                return ((char?)obj).Value.ToString();
            }

            // Do not change the check order
            if (objType == typeof(uint?))
            {
                return ((uint?)obj).Value.ToString(FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(ulong?))
            {
                return ((ulong?)obj).Value.ToString(FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(ushort?))
            {
                return ((ushort?)obj).Value.ToString(FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(sbyte?))
            {
                return ((sbyte?)obj).Value.ToString(FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(Guid))
            {
                Guid guid = (Guid)obj;
                if (guid == Guid.Empty)
                {
                    return JaysonConstants.GuidEmpty;
                }
                return JaysonCommon.AsciiToUpper(guid.ToString("D"));
            }

            // Do not change the check order
            if (objType == typeof(Guid?))
            {
                Guid guid = ((Guid?)obj).Value;
                if (guid == Guid.Empty)
                {
                    return JaysonConstants.GuidEmpty;
                }
                return JaysonCommon.AsciiToUpper(guid.ToString("D"));
            }

            // Do not change the check order
            if (objType == typeof(DateTimeOffset))
            {
                return ((DateTimeOffset)obj).ToString(FormatingCulture);
            }

            // Do not change the check order
            if (objType == typeof(DateTimeOffset?))
            {
                return ((DateTimeOffset?)obj).Value.ToString(FormatingCulture);
            }

            // format everything else normally
            return (obj is IFormattable ? ((IFormattable)obj).ToString(null, FormatingCulture) : obj.ToString());
        }

        # endregion Static Methods
    }

    # endregion JsonFormatter
}