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
#if !(NET3500 || NET3000 || NET2000)
using System.Dynamic;
#endif
using System.Globalization;
using System.Linq;
using System.Text;

namespace Sweet.Jayson
{
    # region JsonConverter

    public static partial class JaysonConverter
    {
        # region Constants

        private const string IntMinValueText = "2147483648";
        private const string IntMaxValueText = "2147483647";

        private const string LongMinValueText = "9223372036854775808";
        private const string LongMaxValueText = "9223372036854775807";

        private const string ULongMaxValueText = "18446744073709551615";

        # endregion Constants

        # region Parse Number

        private static object ParseAsUnordinaryNumber(JaysonNumberParts number)
        {
            string tStr;
            JaysonTuple<string, object> t;

            var str = number.Text;
            var len = JaysonConstants.UnordinaryNumbers.Length;

            for (var i = 0; i < len; i++)
            {
                t = JaysonConstants.UnordinaryNumbers[i];

                tStr = t.Item1;
                if (tStr.Length == number.Length)
                {
                    var matched = true;

                    var tPos = 0;
                    var end = number.Start + number.Length;

                    for (var strPos = number.Start; strPos < end; strPos++)
                    {
                        if (str[strPos] != tStr[tPos++])
                        {
                            matched = false;
                            break;
                        }
                    }

                    if (matched)
                    {
                        return t.Item2;
                    }
                }
            }
            return null;
        }

        private static void SetNumberType(JaysonNumberParts number)
        {
            number.Type = JaysonNumberType.Long;

            var wholePart = number.WholePart;
            var floatingPart = number.FloatingPart;
            var exponentPart = number.ExponentPart;

            var wholeLen = wholePart.Length;
            var floatingLen = 0;

            if (wholeLen > 0)
            {
                var ch = number.Text[wholePart.Start];
                if (ch == 'N' || ch == 'n' || ch == 'I' || ch == 'i' || ch == '∞')
                {
                    number.Type = JaysonNumberType.Double;
                    return;
                }
            }

            if (floatingPart != null)
            {
                floatingLen = floatingPart.Length;

                if (floatingLen > 0)
                {
                    var significancy = floatingLen + wholeLen;
                    if (significancy > JaysonConstants.DoubleSignificantDigits)
                    {
                        number.Type = JaysonNumberType.Decimal;
                    }
                    else if (significancy > JaysonConstants.FloatSignificantDigits)
                    {
                        number.Type = JaysonNumberType.Double;
                    }
                    else
                    {
                        number.Type = JaysonNumberType.Float;
                    }
                }
                else if (wholePart.Sign == -1)
                {
                    if (wholeLen >= 19)
                    {
                        number.Type = JaysonNumberType.Decimal;
                    }
                    else if (wholeLen >= 10)
                    {
                        number.Type = JaysonNumberType.Double;
                    }
                }
            }

            if (exponentPart != null)
            {
                var len = exponentPart.Length;
                if (len > 0)
                {
                    if ((number.Type == JaysonNumberType.Long) ||
                        (number.Type == JaysonNumberType.Int) && (exponentPart.Sign == -1))
                    {
                        number.Type = JaysonNumberType.Double;
                    }

                    var exp = ParseAsLong(number.Text, exponentPart.Start, len);
                    if (exp > 0)
                    {
                        if (exp > JaysonConstants.FloatMaxExponent)
                        {
                            number.Type = JaysonNumberType.Double;
                        }
                        else if (exp > JaysonConstants.DecimalMaxExponent)
                        {
                            if (number.Type == JaysonNumberType.Decimal)
                            {
                                number.Type = JaysonNumberType.Float;
                            }
                            else
                            {
                                number.Type = JaysonNumberType.Double;
                            }
                        }
                        else if (number.Type != JaysonNumberType.Double)
                        {
                            number.Type = JaysonNumberType.Decimal;
                        }
                    }
                }
            }

            if (number.Type == JaysonNumberType.Long)
            {
                var comparisonText = LongMaxValueText;
                var maxWholeLen = LongMaxValueText.Length;

                if (wholeLen > maxWholeLen)
                {
                    number.Type = JaysonNumberType.ULong;

                    comparisonText = ULongMaxValueText;
                    maxWholeLen = ULongMaxValueText.Length;
                }

                if ((wholeLen > maxWholeLen) ||
                    ((number.Type == JaysonNumberType.ULong) && (wholePart.Sign == -1)))
                {
                    number.Type = JaysonNumberType.Decimal;
                }
                else if (wholeLen == maxWholeLen)
                {
                    var str = number.Text;

                    int comparison;
                    var start = wholePart.Start;

                    for (var i = 0; i < maxWholeLen; i++)
                    {
                        comparison = str[start + i] - comparisonText[i];
                        if (comparison < 0)
                        {
                            break;
                        }

                        if (comparison > 0)
                        {
                            if ((number.Type == JaysonNumberType.Long) && (i == maxWholeLen - 1) &&
                                (wholePart.Sign == -1) && (str[start + i] == '8'))
                            {
                                continue;
                            }
                            number.Type = JaysonNumberType.Decimal;
                            break;
                        }
                    }
                }
                else
                {
                    var maxIntLen = IntMaxValueText.Length;
                    if (wholeLen < maxIntLen)
                    {
                        number.Type = JaysonNumberType.Int;
                    }
                    else if (wholeLen == maxIntLen)
                    {
                        number.Type = JaysonNumberType.Int;
                        var str = number.Text;

                        int comparison;
                        var start = wholePart.Start;

                        for (var i = 0; i < maxIntLen; i++)
                        {
                            comparison = str[start + i] - IntMaxValueText[0];
                            if (comparison < 0)
                            {
                                break;
                            }

                            if (comparison > 0)
                            {
                                if ((i == maxWholeLen - 1) && (wholePart.Sign == -1) && (str[start + i] == '8'))
                                {
                                    continue;
                                }
                                number.Type = JaysonNumberType.Long;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static JaysonNumberParts ParseNumberParts(JaysonDeserializationContext context, JaysonSerializationToken currToken)
        {
            var length = context.Length;
            if (context.Position > length - 1)
            {
                throw new JaysonException(JaysonError.InvalidNumber);
            }

            var number = new JaysonNumberParts
            {
                Text = context.Text,
                Start = context.Position,
            };
            number.WholePart.Start = context.Position;

            char ch;
            var currState = JaysonNumberPartType.Start;

            do
            {
                ch = number.Text[context.Position];

                if (ch >= '0' && ch <= '9')
                {
                    switch (currState)
                    {
                        case JaysonNumberPartType.Whole:
                            number.WholePart.Length++;
                            break;
                        case JaysonNumberPartType.Floating:
                            number.FloatingPart.Length++;
                            break;
                        case JaysonNumberPartType.Exponent:
                            number.ExponentPart.Length++;
                            break;
                        case JaysonNumberPartType.Start:
                            {
                                currState = JaysonNumberPartType.Whole;

                                var wholePart = number.WholePart;

                                wholePart.Sign = 1;
                                wholePart.Start = context.Position;
                                wholePart.Length = 1;
                            }
                            break;
                        case JaysonNumberPartType.WholeSign:
                            {
                                currState = JaysonNumberPartType.Whole;

                                var signCh = number.Text[context.Position - 1];

                                var wholePart = number.WholePart;

                                wholePart.Sign = (signCh == '-') ? -1 : 1;
                                wholePart.Start = context.Position;
                                wholePart.Length = 1;
                            }
                            break;
                        case JaysonNumberPartType.FloatingSign:
                            {
                                currState = JaysonNumberPartType.Floating;

                                number.Type = JaysonNumberType.Double;
                                number.SetFloating(start: context.Position, length: (byte)1);
                            }
                            break;
                        case JaysonNumberPartType.ExponentChar:
                            {
                                currState = JaysonNumberPartType.Exponent;

                                number.SetExponent(start: context.Position, length: (byte)1);
                            }
                            break;
                        case JaysonNumberPartType.ExponentSign:
                            {
                                currState = JaysonNumberPartType.Exponent;

                                var signCh = number.Text[context.Position - 1];
                                number.SetExponent(start: context.Position, length: (byte)1, sign: (signCh == '-') ? -1 : 1);
                            }
                            break;
                        default:
                            throw new JaysonException(JaysonError.InvalidNumberChar);
                    }

                    context.Position++;
                    continue;
                }

                if (ch == 'N' || ch == 'n' || ch == 'I' || ch == 'i' || ch == '∞')
                {
                    // Possibly state will be at Start (NaN e.g.), but for negative infinity it should be at WholeSign ([-|+]Infinity or [-|+]∞ e.g.)
                    if (!(currState == JaysonNumberPartType.Start ||
                          currState == JaysonNumberPartType.WholeSign))
                    {
                        throw new JaysonException(JaysonError.InvalidNumberChar);
                    }

                    var expectedText = ((ch == 'N') || (ch == 'n')) ? "NaN" : (ch == '∞' ? "∞" : "Infinity");

                    if (String.Compare(number.Text, context.Position, expectedText, 0, expectedText.Length,
                        JaysonConstants.InvariantCulture, CompareOptions.IgnoreCase) != 0)
                    {
                        throw new JaysonException(JaysonError.InvalidNumberChar);
                    }

                    currState = JaysonNumberPartType.End;

                    number.Type = JaysonNumberType.Double;

                    var signCh = (ch == '∞' && context.Position > 0) ? number.Text[context.Position - 1] : '+';

                    var wholePart = number.WholePart;

                    wholePart.Sign = (signCh == '-') ? -1 : 1;
                    wholePart.Start = context.Position;
                    wholePart.Length = (byte)expectedText.Length;

                    context.Position += expectedText.Length;
                    if ((ch != '∞') && (context.Position < length))
                    {
                        ch = number.Text[context.Position];
                        if (!IsNumberEnd(ch, currToken))
                        {
                            throw new JaysonException(JaysonError.InvalidNumberChar);
                        }
                    }

                    continue;
                }

                if (ch == '.')
                {
                    // Possibly state will be at Whole (.01 e.g.), but for negative infinity it should be at WholeSign ([-|+].01 e.g.) or Start
                    if (!(currState == JaysonNumberPartType.Whole ||
                          currState == JaysonNumberPartType.WholeSign ||
                          currState == JaysonNumberPartType.Start))
                    {
                        throw new JaysonException(JaysonError.InvalidNumberChar);
                    }

                    currState = JaysonNumberPartType.FloatingSign;

                    context.Position++;
                    continue;
                }

                if (ch == '-' || ch == '+')
                {
                    switch (currState)
                    {
                        case JaysonNumberPartType.Start:
                            currState = JaysonNumberPartType.WholeSign;
                            break;
                        case JaysonNumberPartType.ExponentChar:
                            currState = JaysonNumberPartType.ExponentSign;
                            break;
                        default:
                            throw new JaysonException(JaysonError.InvalidNumberChar);
                    }

                    context.Position++;
                    continue;
                }

                if (ch == 'e' || ch == 'E')
                {
                    // Possibly state will be at Floating (3.01e+2 e.g.), but for extraordinary cases it should be at Whole (3e+2 e.g.)
                    if (!(currState == JaysonNumberPartType.Floating ||
                          currState == JaysonNumberPartType.Whole))
                    {
                        throw new JaysonException(JaysonError.InvalidNumberChar);
                    }

                    if ((number.WholePart.Length == 0) && !number.HasFloating)
                    {
                        throw new JaysonException(JaysonError.InvalidNumberChar);
                    }

                    currState = JaysonNumberPartType.ExponentChar;

                    context.Position++;
                    continue;
                }

                if (IsNumberEnd(ch, currToken))
                {
                    break;
                }

                throw new JaysonException(JaysonError.InvalidNumberChar);

            } while ((context.Position < length) && (currState != JaysonNumberPartType.End));

            if ((number.FloatingLength == 0) &&
                ((number.WholeLength == 0) || (number.FloatingPart != null)))
            {
                throw new JaysonException(JaysonError.InvalidNumberChar);
            }

            number.Length = context.Position - number.Start;
            SetNumberType(number);

            return number;
        }

        private static bool IsNumberEnd(char ch, JaysonSerializationToken currToken)
        {
            return (currToken == JaysonSerializationToken.Value) &&
                   (ch == ',' || ch == ']' || ch == '}' || ch == '"' || JaysonCommon.IsWhiteSpace(ch));
        }

        private static int ParseAsInt(string s, int start, int length)
        {
            var value = ParseAsLong(s, start, length);
            if ((value < int.MinValue) || (value > int.MaxValue))
            {
                throw new JaysonException(JaysonError.InvalidNumber);
            }
            return (int)value;
        }

        private static long ParseAsLong(string s, int start, int length)
        {
            var value = 0L;
            if ((s != null) && (start > -1) && (length > 0))
            {
                var end = start + length;
                if (end <= s.Length)
                {
                    char ch;
                    for (var i = start; i < end; i++)
                    {
                        ch = s[i];
                        if ((ch < '0') || (ch > '9'))
                        {
                            throw new JaysonException(JaysonError.InvalidNumber);
                        }
                        value = (10 * value) + (ch - '0');
                    }
                }
            }
            return value;
        }

        private static uint ParseAsUInt(string s, int start, int length)
        {
            var l = ParseAsULong(s, start, length);
            if (l > uint.MaxValue)
            {
                throw new JaysonException(JaysonError.InvalidNumber);
            }
            return (uint)l;
        }

        private static ulong ParseAsULong(string s, int start, int length)
        {
            var value = (ulong)0;
            if ((s != null) && (start > -1) && (length > 0))
            {
                var end = start + length;
                if (end <= s.Length)
                {
                    char ch;
                    for (var i = start; i < end; i++)
                    {
                        ch = s[i];
                        if ((ch < '0') || (ch > '9'))
                        {
                            throw new JaysonException(JaysonError.InvalidNumber);
                        }
                        value = (10 * value) + (ulong)(ch - '0');
                    }
                }
            }
            return value;
        }

        private static float ParseAsFloat(string s, int start, int length)
        {
            var value = ParseAsDouble(s, start, length);

            if (double.IsNaN(value))
                return float.NaN;

            if (double.IsPositiveInfinity(value))
                return float.PositiveInfinity;

            if ((value < float.MinValue) || (value > float.MaxValue))
            {
                throw new JaysonException(JaysonError.InvalidNumber);
            }
            return (float)value;
        }

        private static double ParseAsDouble(string s, int start, int length)
        {
            var value = 0d;
            if ((s != null) && (start > -1) && (length > 0))
            {
                var end = start + length;
                if (end <= s.Length)
                {
                    char ch = s[start];
                    switch (ch)
                    {
                        case 'N':
                        case 'n':
                            return double.NaN;
                        case 'I':
                        case 'i':
                        case '∞':
                            return double.PositiveInfinity;
                    }

                    for (var i = start; i < end; i++)
                    {
                        ch = s[i];
                        if ((ch < '0') || (ch > '9'))
                        {
                            throw new JaysonException(JaysonError.InvalidNumber);
                        }
                        value = (10 * value) + (ch - '0');
                    }
                }
            }
            return value;
        }

        private static decimal ParseAsDecimal(string s, int start, int length)
        {
            var value = 0m;
            if ((s != null) && (start > -1) && (length > 0))
            {
                var end = start + length;
                if (end <= s.Length)
                {
                    char ch;
                    for (var i = start; i < end; i++)
                    {
                        ch = s[i];
                        if ((ch < '0') || (ch > '9'))
                        {
                            throw new JaysonException(JaysonError.InvalidNumber);
                        }
                        value = (10 * value) + (ch - '0');
                    }
                }
            }
            return value;
        }

        private static double ParseAsDouble(JaysonNumberParts number)
        {
            var value = 0d;

            var wholePart = number.WholePart;
            var floatingPart = number.FloatingPart;
            var exponentPart = number.ExponentPart;

            if ((wholePart != null) && (wholePart.Length > 0) && (wholePart.Start > -1))
            {
                value = ParseAsDouble(number.Text, wholePart.Start, wholePart.Length);

                if (double.IsNaN(value))
                    return value;

                if (double.IsPositiveInfinity(value))
                {
                    if (wholePart.Sign == -1)
                        return double.NegativeInfinity;

                    return value;
                }
            }

            if ((floatingPart != null) && (floatingPart.Length > 0) && (floatingPart.Start > -1))
            {
                var floatingLen = Math.Min((int)floatingPart.Length, JaysonConstants.DoubleSignificantDigits - wholePart.Length);
                if (floatingLen > 0)
                {
                    var floating = ParseAsDouble(number.Text, floatingPart.Start, floatingLen);
                    floating /= JaysonConstants.PowerOf10Double[floatingLen];

                    value += floating;
                }
            }

            if ((exponentPart != null) && (exponentPart.Length > 0) && (exponentPart.Start > -1))
            {
                var exp = ParseAsInt(number.Text, exponentPart.Start, exponentPart.Length);
                if (exp > JaysonConstants.PowerOf10Double.Length)
                {
                    throw new JaysonException(JaysonError.InvalidNumber);
                }

                if (exponentPart.Sign == -1)
                {
                    value /= JaysonConstants.PowerOf10Double[exp];
                }
                else
                {
                    value *= JaysonConstants.PowerOf10Double[exp];
                }
            }

            return wholePart.Sign * value;
        }

        private static float ParseAsFloat(JaysonNumberParts number)
        {
            var value = ParseAsDouble(number);

            if (double.IsNaN(value))
                return float.NaN;

            if (double.IsPositiveInfinity(value))
                return float.PositiveInfinity;

            if (double.IsNegativeInfinity(value))
                return float.NegativeInfinity;

            if ((value < float.MinValue) || (value > float.MaxValue))
            {
                throw new JaysonException(JaysonError.InvalidNumber);
            }
            return (float)value;
        }

        private static decimal ParseAsDecimal(JaysonNumberParts number)
        {
            var value = 0m;

            var wholePart = number.WholePart;
            var floatingPart = number.FloatingPart;
            var exponentPart = number.ExponentPart;

            if ((wholePart != null) && (wholePart.Length > 0) && (wholePart.Start > -1))
            {
                value = ParseAsDecimal(number.Text, wholePart.Start, wholePart.Length);
            }

            if ((floatingPart != null) && (floatingPart.Length > 0) && (floatingPart.Start > -1))
            {
                var floatingLen = Math.Min((int)floatingPart.Length, JaysonConstants.DecimalSignificantDigits - wholePart.Length);
                if (floatingLen > 0)
                {
                    var floating = ParseAsDecimal(number.Text, floatingPart.Start, floatingLen);
                    floating /= JaysonConstants.PowerOf10Decimal[floatingLen];

                    value += floating;
                }
            }

            if ((exponentPart != null) && (exponentPart.Length > 0) && (exponentPart.Start > -1))
            {
                var exp = ParseAsInt(number.Text, exponentPart.Start, exponentPart.Length);
                if (exp > JaysonConstants.PowerOf10Decimal.Length)
                {
                    throw new JaysonException(JaysonError.InvalidNumber);
                }

                if (exponentPart.Sign == -1)
                {
                    value /= JaysonConstants.PowerOf10Decimal[exp];
                }
                else
                {
                    value *= JaysonConstants.PowerOf10Decimal[exp];
                }
            }

            return wholePart.Sign * value;
        }

        private static long ParseAsLong(JaysonNumberParts number)
        {
            var value = 0L;

            var wholePart = number.WholePart;
            var floatingPart = number.FloatingPart;
            var exponentPart = number.ExponentPart;

            if ((wholePart != null) && (wholePart.Length > 0) && (wholePart.Start > -1))
            {
                value = ParseAsLong(number.Text, wholePart.Start, wholePart.Length);
            }

            if ((exponentPart != null) && (exponentPart.Length > 0) && (exponentPart.Start > -1))
            {
                if (exponentPart.Sign == -1)
                {
                    throw new JaysonException(JaysonError.InvalidNumber);
                }

                var exp = ParseAsInt(number.Text, exponentPart.Start, exponentPart.Length);
                if (exp > JaysonConstants.PowerOf10Long.Length)
                {
                    throw new JaysonException(JaysonError.InvalidNumber);
                }
                value *= JaysonConstants.PowerOf10Long[exp];

                if ((floatingPart != null) && (floatingPart.Length > 0) && (floatingPart.Start > -1))
                {
                    if (exp == 0)
                    {
                        throw new JaysonException(JaysonError.InvalidNumber);
                    }

                    var floatingLen = Math.Min(exp, floatingPart.Length);
                    floatingLen = Math.Min(floatingLen, JaysonConstants.LongMaxExponent - wholePart.Length);

                    if (floatingLen > 0)
                    {
                        var floating = ParseAsLong(number.Text, floatingPart.Start, floatingLen);
                        floating *= JaysonConstants.PowerOf10Long[exp];

                        value += floating;
                    }
                }
            }
            else if ((floatingPart != null) && (floatingPart.Length > 0) && (floatingPart.Start > -1))
            {
                throw new JaysonException(JaysonError.InvalidNumber);
            }

            return wholePart.Sign * value;
        }

        private static int ParseAsInt(JaysonNumberParts number)
        {
            var value = ParseAsLong(number);
            if ((value > int.MaxValue) || (value < int.MinValue))
            {
                throw new JaysonException(JaysonError.InvalidNumber);
            }
            return (int)value;
        }

        private static ulong ParseAsULong(JaysonNumberParts number)
        {
            var value = (ulong)0L;

            var wholePart = number.WholePart;
            var floatingPart = number.FloatingPart;
            var exponentPart = number.ExponentPart;

            if ((wholePart != null) && (wholePart.Length > 0) && (wholePart.Start > -1))
            {
                value = ParseAsULong(number.Text, wholePart.Start, wholePart.Length);
            }

            if ((exponentPart != null) && (exponentPart.Length > 0) && (exponentPart.Start > -1))
            {
                if (exponentPart.Sign == -1)
                {
                    throw new JaysonException(JaysonError.InvalidNumber);
                }

                var exp = ParseAsInt(number.Text, exponentPart.Start, exponentPart.Length);
                if (exp > JaysonConstants.PowerOf10ULong.Length)
                {
                    throw new JaysonException(JaysonError.InvalidNumber);
                }
                value *= JaysonConstants.PowerOf10ULong[exp];

                if ((floatingPart != null) && (floatingPart.Length > 0) && (floatingPart.Start > -1))
                {
                    if (exp == 0)
                    {
                        throw new JaysonException(JaysonError.InvalidNumber);
                    }

                    var floatingLen = Math.Min(exp, floatingPart.Length);
                    floatingLen = Math.Min(floatingLen, JaysonConstants.ULongMaxExponent - wholePart.Length);

                    if (floatingLen > 0)
                    {
                        var floating = ParseAsULong(number.Text, floatingPart.Start, floatingLen);
                        floating *= JaysonConstants.PowerOf10ULong[exp];

                        value += floating;
                    }
                }
            }
            else if ((floatingPart != null) && (floatingPart.Length > 0) && (floatingPart.Start > -1))
            {
                throw new JaysonException(JaysonError.InvalidNumber);
            }

            return value;
        }

        private static object ParseNumber(JaysonDeserializationContext context, JaysonSerializationToken currToken = JaysonSerializationToken.Value)
        {
            if (context.Position > context.Length - 1)
            {
                throw new JaysonException(JaysonError.InvalidNumber);
            }

            var start = context.Position;
            var number = ParseNumberParts(context, currToken);

            if ((number != null) && (number.Length > 0))
            {
                var value = ParseAsUnordinaryNumber(number);
                if (value != null)
                {
                    return value;
                }

                switch (number.Type)
                {
                    case JaysonNumberType.Long:
                    case JaysonNumberType.Int:
                        var l = ParseAsLong(number);
                        if (l <= JaysonConstants.IntMaxValueAsLong && l >= JaysonConstants.IntMinValueAsLong)
                        {
                            return (int)l;
                        }
                        return l;
                    case JaysonNumberType.Double:
                    case JaysonNumberType.Float:
                        return ParseAsDouble(number);
                    case JaysonNumberType.Decimal:
                        var d = ParseAsDecimal(number);
                        if (context.Settings.ConvertDecimalToDouble)
                        {
                            return (double)d;
                        }
                        return d;
                    case JaysonNumberType.ULong:
                        return ParseAsULong(number);
                    default:
                        break;
                }
            }

            throw new JaysonException(JaysonError.InvalidNumber);
        }

        # endregion Parse Number
    }

    # endregion JsonConverter
}
