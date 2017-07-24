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
using System.Data;
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

        # region Parse

        # region Parse String

        private static int HexToUnicodeChar(string str, int start)
        {
            var result = 0;

            char ch;
            var decimalCh = 0;

            var to = start + 4;
            for (int i = start; i < to; i++)
            {
                ch = str[i];

                if (!(ch < '0' || ch > '9'))
                {
                    decimalCh = ch - '0';
                }
                else if (!(ch < 'A' || ch > 'F'))
                {
                    decimalCh = ch - JaysonConstants.CharA10;
                }
                else if (!(ch < 'a' || ch > 'f'))
                {
                    decimalCh = ch - JaysonConstants.Chara10;
                }
                else
                {
                    throw new JaysonException(JaysonError.InvalidUnicodeEscapedChar);
                }

                result *= 16;
                result += decimalCh;
            }

            return result;
        }

        private static void ParseComment(JaysonDeserializationContext context)
        {
            if (context.Settings.CommentHandling == JaysonCommentHandling.ThrowError)
            {
                throw new JaysonException(JaysonError.InvalidComment);
            }

            var str = context.Text;
            var length = str.Length;

            if (context.Position > length - 1)
            {
                throw new JaysonException(JaysonError.InvalidCommentTermination);
            }

            var singleLine = str[context.Position] == '/';
            if (!(singleLine || str[context.Position] == '*'))
            {
                throw new JaysonException(JaysonError.InvalidCommentStart);
            }

            context.Position++;

            if (context.Position >= length - 1)
            {
                if (!singleLine)
                {
                    throw new JaysonException(JaysonError.InvalidCommentTermination);
                }
                return;
            }

            char ch;
            do
            {
                ch = str[context.Position];

                switch (ch)
                {
                    case '\r':
                    case '\n':
                        if (singleLine)
                        {
                            EatWhites(context);
                            return;
                        }
                        break;
                    case '*':
                        if (!singleLine)
                        {
                            context.Position++;
                            if (context.Position >= length || str[context.Position] != '/')
                            {
                                throw new JaysonException(JaysonError.InvalidCommentTermination);
                            }
                            context.Position++;
                            EatWhites(context);
                            return;
                        }
                        break;
                }

                context.Position++;
            } while (context.Position < length);

            if (!singleLine)
            {
                throw new JaysonException(JaysonError.InvalidCommentTermination);
            }
        }

        private static string ParseString(string str, ref int pos)
        {
            var length = str.Length;
            if (pos > length - 1)
            {
                throw new JaysonException(JaysonError.InvalidStringTermination);
            }

            if (str[pos] == '"')
            {
                pos++;
                return String.Empty;
            }

            if (pos == length - 1)
            {
                throw new JaysonException(JaysonError.InvalidStringTermination);
            }

            char ch;
            int unicodeChar;

            int len;
            var start = pos;
            var terminated = false;

            var charStore = new StringBuilder(20, int.MaxValue);
            do
            {
                ch = str[pos];

                if (ch != '\\')
                {
                    if (ch == '"')
                    {
                        terminated = true;
                        break;
                    }

                    pos++;
                    continue;
                }

                if (pos > length - 1)
                {
                    throw new JaysonException(JaysonError.InvalidCharInString);
                }

                len = pos - start;
                if (len > 0)
                {
                    charStore.Append(str, start, len);
                }

                switch (str[++pos])
                {
                    case 'n':
                        start = ++pos;
                        charStore.Append('\n');
                        break;
                    case 'r':
                        start = ++pos;
                        charStore.Append('\r');
                        break;
                    case 't':
                        start = ++pos;
                        charStore.Append('\t');
                        break;
                    case '"':
                        start = ++pos;
                        charStore.Append('"');
                        break;
                    case '\\':
                        start = ++pos;
                        charStore.Append('\\');
                        break;
                    case 'u':
                        if (pos < length - 4)
                        {
                            unicodeChar = HexToUnicodeChar(str, ++pos);

                            pos += 4;
                            start = pos;

                            if (unicodeChar > -1)
                            {
                                charStore.Append((char)unicodeChar);
                                break;
                            }
                        }

                        throw new JaysonException(JaysonError.InvalidUnicodeString);
                    case '/':
                        start = ++pos;
                        charStore.Append('/');
                        break;
                    case 'b':
                        start = ++pos;
                        charStore.Append('\b');
                        break;
                    case 'f':
                        start = ++pos;
                        charStore.Append('\f');
                        break;
                    default:
                        throw new JaysonException(JaysonError.InvalidUnicodeChar);
                }
            } while (pos < length);

            if (!terminated)
            {
                throw new JaysonException(JaysonError.InvalidStringTermination);
            }

            len = pos - start;
            pos++;

            if (len > 0)
            {
                if (charStore.Length == 0)
                {
                    return str.Substring(start, len);
                }

                charStore.Append(str, start, len);
            }
            return charStore.ToString();
        }

        # endregion Parse String

        # region Parse Number

        private static object ParseAsUnordinaryNumber(JaysonNumberParts number)
        {
            string tStr;
            Tuple<string, object> t;

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

            var wholePart = number.Parts[0];
            var wholeLen = wholePart.Length;

            var floatingPart = number.Parts[1];
            var floatingLen = 0;

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

            var expPart = number.Parts[2];
            if (expPart != null)
            {
                var len = expPart.Length;
                if (len > 0)
                {
                    if ((number.Type == JaysonNumberType.Long) ||
                        (number.Type == JaysonNumberType.Int) && (expPart.Sign == -1))
                    {
                        number.Type = JaysonNumberType.Double;
                    }

                    var exp = ParseAsLong(number.Text, expPart.Start, len);
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
            int length = context.Length;
            if (context.Position > length - 1)
            {
                throw new JaysonException(JaysonError.InvalidNumber);
            }

            var number = new JaysonNumberParts
            {
                Text = context.Text,
                Start = context.Position,
            };
            number.Parts[0].Start = context.Position;

            char ch;
            var currState = JaysonNumberPartType.Start;

            do
            {
                ch = number.Text[context.Position];

                if ((ch >= '0') && (ch <= '9'))
                {
                    switch (currState)
                    {
                        case JaysonNumberPartType.Whole:
                            number.Parts[0].Length++;
                            break;
                        case JaysonNumberPartType.Floating:
                            number.Parts[1].Length++;
                            break;
                        case JaysonNumberPartType.Exponent:
                            number.Parts[2].Length++;
                            break;
                        case JaysonNumberPartType.Start:
                            currState = JaysonNumberPartType.Whole;

                            var wholePart1 = number.Parts[0];

                            wholePart1.Sign = 1;
                            wholePart1.Start = context.Position;
                            wholePart1.Length = 1;
                            break;
                        case JaysonNumberPartType.WholeSign:
                            currState = JaysonNumberPartType.Whole;
                            var signCh1 = number.Text[context.Position - 1];

                            number.Style |= NumberStyles.AllowLeadingSign;

                            var wholePart2 = number.Parts[0];

                            wholePart2.Sign = (signCh1 == '-') ? -1 : 1;
                            wholePart2.Start = context.Position;
                            wholePart2.Length = 1;
                            break;
                        case JaysonNumberPartType.FloatingSign:
                            currState = JaysonNumberPartType.Floating;
                            number.Type = JaysonNumberType.Double;
                            number.Style |= NumberStyles.AllowDecimalPoint;

                            number.Parts[1] = new JaysonNumberPart
                            {
                                Start = context.Position,
                                Length = 1
                            };
                            break;
                        case JaysonNumberPartType.ExponentChar:
                            currState = JaysonNumberPartType.Exponent;

                            number.Style |= NumberStyles.AllowExponent;

                            number.Parts[2] = new JaysonNumberPart
                            {
                                Sign = 1,
                                Start = context.Position,
                                Length = 1
                            };
                            break;
                        case JaysonNumberPartType.ExponentSign:
                            currState = JaysonNumberPartType.Exponent;

                            number.Style |= NumberStyles.AllowExponent;

                            var signCh2 = number.Text[context.Position - 1];

                            number.Parts[2] = new JaysonNumberPart
                            {
                                Sign = (signCh2 == '-') ? -1 : 1,
                                Start = context.Position,
                                Length = 1
                            };
                            break;
                        default:
                            throw new JaysonException(JaysonError.InvalidNumberChar);
                    }

                    context.Position++;
                    continue;
                }

                if (ch == '.')
                {
                    if (!((currState == JaysonNumberPartType.Whole) ||
                          (currState == JaysonNumberPartType.WholeSign) ||
                          (currState == JaysonNumberPartType.Start)))
                    {
                        throw new JaysonException(JaysonError.InvalidNumberChar);
                    }

                    currState = JaysonNumberPartType.FloatingSign;

                    context.Position++;
                    continue;
                }

                if ((ch == '-') || (ch == '+'))
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
                    if (!((currState == JaysonNumberPartType.Whole) ||
                          (currState == JaysonNumberPartType.Floating)))
                    {
                        throw new JaysonException(JaysonError.InvalidNumberChar);
                    }

                    if ((number.Parts[0].Length == 0) && ((number.Parts[1] == null) || (number.Parts[1].Length == 0)))
                    {
                        throw new JaysonException(JaysonError.InvalidNumberChar);
                    }

                    currState = JaysonNumberPartType.ExponentChar;

                    context.Position++;
                    continue;
                }

                if ((currToken == JaysonSerializationToken.Value) &&
                    (ch == ',' || ch == ']' || ch == '}' || JaysonCommon.IsWhiteSpace(ch)))
                {
                    break;
                }

                throw new JaysonException(JaysonError.InvalidNumberChar);
            } while (context.Position < length);

            if ((number.FloatingLength == 0) && 
                ((number.WholeLength == 0) || (number.FloatingPart != null)))
            {
                throw new JaysonException(JaysonError.InvalidNumberChar);
            }

            number.Length = context.Position - number.Start;
            SetNumberType(number);

            return number;
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

            var wholePart = number.Parts[0];
            if ((wholePart != null) && (wholePart.Length > 0) && (wholePart.Start > -1))
            {
                value = ParseAsDouble(number.Text, wholePart.Start, wholePart.Length);
            }

            var floatingPart = number.Parts[1];
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

            var exponentPart = number.Parts[2];
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
            if ((value < float.MinValue) || (value > float.MaxValue))
            {
                throw new JaysonException(JaysonError.InvalidNumber);
            }
            return (float)value;
        }

        private static decimal ParseAsDecimal(JaysonNumberParts number)
        {
            var value = 0m;

            var wholePart = number.Parts[0];
            if ((wholePart != null) && (wholePart.Length > 0) && (wholePart.Start > -1))
            {
                value = ParseAsDecimal(number.Text, wholePart.Start, wholePart.Length);
            }

            var floatingPart = number.Parts[1];
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

            var exponentPart = number.Parts[2];
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

            var wholePart = number.Parts[0];
            if ((wholePart != null) && (wholePart.Length > 0) && (wholePart.Start > -1))
            {
                value = ParseAsLong(number.Text, wholePart.Start, wholePart.Length);
            }

            var floatingPart = number.Parts[1];
            var exponentPart = number.Parts[2];

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

            var wholePart = number.Parts[0];
            if ((wholePart != null) && (wholePart.Length > 0) && (wholePart.Start > -1))
            {
                value = ParseAsULong(number.Text, wholePart.Start, wholePart.Length);
            }

            var floatingPart = number.Parts[1];
            var exponentPart = number.Parts[2];

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

        private static object ParseNumber(JaysonDeserializationContext context, JaysonSerializationToken currToken)
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

        # region Helper Methods

        private static void ValidateObjectDepth(JaysonDeserializationContext context)
        {
            if (context.Settings.MaxObjectDepth > 0 &&
                context.ObjectDepth > context.Settings.MaxObjectDepth)
            {
                throw new JaysonException(String.Format(JaysonError.MaximumObjectDepthExceed,
                    context.Settings.MaxObjectDepth));
            }
        }

        private static void EatWhites(JaysonDeserializationContext context)
        {
            var str = context.Text;
            var length = context.Length;

            while (context.Position < length)
            {
                if (!JaysonCommon.IsWhiteSpace(str[context.Position]))
                    return;
                context.Position++;
            }
        }

        private static void EatWhitesAndCheckChar(JaysonDeserializationContext context, int charToCheck)
        {
            if (context.Position >= context.Length - 1)
            {
                throw new JaysonException(JaysonError.InvalidJson);
            }

            int ch = (int)context.Text[context.Position];
            if (ch == charToCheck)
            {
                context.Position++;
            }
            else
            {
                if (JaysonCommon.IsWhiteSpace(ch))
                {
                    var str = context.Text;
                    var length = context.Length;

                    do
                    {
                        context.Position++;
                        if (context.Position < length)
                        {
                            ch = (int)str[context.Position];
                            if (ch == charToCheck)
                                return;

                            if (JaysonCommon.IsWhiteSpace(ch))
                                continue;
                        }
                        throw new JaysonException(JaysonError.InvalidJson);
                    } while (true);
                }
                throw new JaysonException(JaysonError.InvalidJson);
            }
        }

        private static void EatWhitesAndCheckChars(JaysonDeserializationContext context, int[] charsToCheck)
        {
            if (context.Position >= context.Length - 1)
            {
                throw new JaysonException(JaysonError.InvalidJson);
            }

            var chLen = charsToCheck == null ? 0 : charsToCheck.Length;
            if (chLen == 0)
            {
                return;
            }

            if (chLen == 1)
            {
                EatWhitesAndCheckChar(context, charsToCheck[0]);
                return;
            }

            int ch = (int)context.Text[context.Position];
            if (charsToCheck.Contains(ch))
            {
                context.Position++;
            }
            else
            {
                if (JaysonCommon.IsWhiteSpace(ch))
                {
                    string str = context.Text;
                    int length = context.Length;

                    do
                    {
                        context.Position++;
                        if (context.Position < length)
                        {
                            ch = (int)str[context.Position];
                            if (charsToCheck.Contains(ch))
                                return;

                            if (JaysonCommon.IsWhiteSpace(ch))
                                continue;
                        }
                        throw new JaysonException(JaysonError.InvalidJson);
                    } while (true);
                }
                throw new JaysonException(JaysonError.InvalidJson);
            }
        }

        # endregion Helper Methods

        # region Parse List

        private static IList ListAsArray(IList list)
        {
            int count = list.Count;
            if (count > 0 && count <= 50)
            {
                object item;
                bool hasNull = false;
                Type arrayType = null;

                for (int i = 0; i < count; i++)
                {
                    item = list[i];
                    if (item == null)
                    {
                        hasNull = true;
                    }
                    else if (arrayType == null)
                    {
                        arrayType = item.GetType();
                    }
                    else if (arrayType != item.GetType())
                    {
                        arrayType = typeof(object);
                        break;
                    }
                }

                if (arrayType == null ||
                    (hasNull && !JaysonTypeInfo.IsClass(arrayType)))
                {
                    arrayType = typeof(object);
                }

                if (list is IList<object> && arrayType == typeof(object))
                {
                    return ((IList<object>)list).ToArray();
                }

                Array typedArray = Array.CreateInstance(arrayType, count);
                list.CopyTo(typedArray, 0);
                return typedArray;
            }

            if (count == 0)
            {
                return new object[0];
            }

            if (list is IList<object>)
            {
                return ((IList<object>)list).ToArray();
            }

            Array objectArray = Array.CreateInstance(typeof(object), count);
            list.CopyTo(objectArray, 0);
            return objectArray;
        }

        private static IList ParseList(JaysonDeserializationContext context)
        {
            string str = context.Text;
            int length = context.Length;

            bool startedWithComment = false;
            if (context.Settings.CommentHandling == JaysonCommentHandling.ThrowError)
            {
                EatWhitesAndCheckChar(context, '[');
            }
            else
            {
                EatWhitesAndCheckChars(context, new int[] { '[', '/' });
                startedWithComment = (str[context.Position - 1] == '/') &&
                    (str[context.Position] == '/' || str[context.Position] == '*');

                if (startedWithComment)
                {
                    context.Position--;
                }
            }

            context.ObjectDepth++;
            ValidateObjectDepth(context);

            IList result =
                context.Settings.ArrayType == ArrayDeserializationType.ArrayList ?
                (IList)(new ArrayList(10)) :
                new List<object>(10);

            char ch;
            JaysonSerializationToken token = JaysonSerializationToken.Value;

            while (context.Position < length)
            {
                ch = str[context.Position++];
                if (token == JaysonSerializationToken.Comma)
                {
                    token = JaysonSerializationToken.Value;
                    if (ch == ',' || ch == ']' || ch == '}')
                    {
                        throw new JaysonException(JaysonError.InvalidJsonList);
                    }
                }

                switch (ch)
                {
                    case '"':
                        result.Add(ParseString(str, ref context.Position));
                        break;
                    case ',':
                        if (result.Count == 0)
                        {
                            throw new JaysonException(JaysonError.InvalidJsonListItem);
                        }
                        token = JaysonSerializationToken.Comma;
                        break;
                    case ']':
                        if (context.Settings.ArrayType == ArrayDeserializationType.Array)
                        {
                            return ((IList<object>)result).ToArray();
                        }
                        if (context.Settings.ArrayType == ArrayDeserializationType.ArrayDefined)
                        {
                            return ListAsArray(result);
                        }
                        return result;
                    case '{':
                        context.Position--;
                        result.Add(ParseDictionary(context));
                        break;
                    case 'n':
                        {
                            int pos = context.Position;
                            if (pos < length - 3 &&
                                str[pos] == 'u' &&
                                str[pos + 1] == 'l' &&
                                str[pos + 2] == 'l')
                            {
                                result.Add(null);
                                context.Position += 3;
                                break;
                            }
                            throw new JaysonException(JaysonError.InvalidJsonListItem);
                        }
                    case 't':
                        {
                            int pos = context.Position;
                            if (pos < length - 3 &&
                                str[pos] == 'r' &&
                                str[pos + 1] == 'u' &&
                                str[pos + 2] == 'e')
                            {
                                result.Add(true);
                                context.Position += 3;
                                break;
                            }
                            throw new JaysonException(JaysonError.InvalidJsonListItem);
                        }
                    case 'f':
                        {
                            int pos = context.Position;
                            if (pos < length - 4 &&
                                str[pos] == 'a' &&
                                str[pos + 1] == 'l' &&
                                str[pos + 2] == 's' &&
                                str[pos + 3] == 'e')
                            {
                                result.Add(false);
                                context.Position += 4;
                                break;
                            }
                            throw new JaysonException(JaysonError.InvalidJsonListItem);
                        }
                    case '[':
                        context.Position--;
                        result.Add(ParseList(context));
                        break;
                    case '/':
                        ParseComment(context);
                        if (startedWithComment)
                        {
                            startedWithComment = false;
                            if (str[context.Position++] != '[')
                            {
                                throw new JaysonException(JaysonError.InvalidJsonObjectKey);
                            }
                        }
                        continue;
                    default:
                        if (!(ch < '0' || ch > '9') || ch == '-' || ch == '.')
                        {
                            context.Position--;
                            result.Add(ParseNumber(context, JaysonSerializationToken.Value));
                            break;
                        }
                        if (JaysonCommon.IsWhiteSpace(ch))
                            continue;
                        throw new JaysonException(JaysonError.InvalidJsonListItem);
                }
            }

            throw new JaysonException(JaysonError.InvalidJsonList);
        }

        # endregion Parse List

        # region Parse Dictionary

        private static IDictionary<string, object> ParseDictionary(JaysonDeserializationContext context)
        {
            var str = context.Text;
            var length = context.Length;

            var startedWithComment = false;
            if (context.Settings.CommentHandling == JaysonCommentHandling.ThrowError)
            {
                EatWhitesAndCheckChar(context, '{');
            }
            else
            {
                EatWhitesAndCheckChars(context, new int[] { '{', '/' });
                startedWithComment = (str[context.Position - 1] == '/') &&
                    (str[context.Position] == '/' || str[context.Position] == '*');

                if (startedWithComment)
                {
                    context.Position--;
                }
            }

            context.ObjectDepth++;
            ValidateObjectDepth(context);

#if !(NET3500 || NET3000 || NET2000)
            var result =
                (context.Settings.DictionaryType == DictionaryDeserializationType.Expando) ?
                (IDictionary<string, object>)(new ExpandoObject()) :
                (context.Settings.CaseSensitive ? new Dictionary<string, object>(10) :
                    new Dictionary<string, object>(10, StringComparer.OrdinalIgnoreCase));
#else
			var result = context.Settings.CaseSensitive ? new Dictionary<string, object>(10) :
				new Dictionary<string, object>(10, StringComparer.OrdinalIgnoreCase);
#endif

            char ch;
            object value;
            string key = null;

            var token = JaysonSerializationToken.Key;
            var prevToken = JaysonSerializationToken.Undefined;

            while (context.Position < length)
            {
                ch = str[context.Position++];

                switch (token)
                {
                    case JaysonSerializationToken.Key:
                        switch (ch)
                        {
                            case '"':
                                if (prevToken == JaysonSerializationToken.Value)
                                {
                                    throw new JaysonException(JaysonError.InvalidJsonObjectKey);
                                }

                                key = ParseString(str, ref context.Position);
                                if (key == null)
                                {
                                    throw new JaysonException(JaysonError.InvalidJsonObjectKey);
                                }

                                prevToken = token;
                                token = JaysonSerializationToken.Colon;
                                continue;
                            case ',':
                                if (prevToken != JaysonSerializationToken.Value)
                                {
                                    throw new JaysonException(JaysonError.InvalidJsonObjectKey);
                                }
                                prevToken = JaysonSerializationToken.Comma;
                                break;
                            case '}':
                                context.ObjectDepth--;
                                return result;
                            case '/':
                                ParseComment(context);
                                if (startedWithComment)
                                {
                                    startedWithComment = false;
                                    if (str[context.Position++] != '{')
                                    {
                                        throw new JaysonException(JaysonError.InvalidJsonObjectKey);
                                    }
                                }
                                continue;
                            default:
                                if (JaysonCommon.IsWhiteSpace(ch))
                                    continue;
                                throw new JaysonException(JaysonError.InvalidJsonObjectKey);
                        }
                        break;
                    case JaysonSerializationToken.Colon:
                        if (ch == ':')
                        {
                            prevToken = token;
                            token = JaysonSerializationToken.Value;
                            break;
                        }
                        if (JaysonCommon.IsWhiteSpace(ch))
                            continue;
                        throw new JaysonException(JaysonError.InvalidJsonObjectValue);
                    case JaysonSerializationToken.Value:
                        switch (ch)
                        {
                            case '"':
                                result[key] = ParseString(str, ref context.Position);
                                if (key == "$type")
                                {
                                    context.HasTypeInfo = true;
                                }
                                break;
                            case '{':
                                context.Position--;
                                result[key] = ParseDictionary(context);
                                break;
                            case 'n':
                                {
                                    int pos = context.Position;
                                    if (pos < length - 3 &&
                                        str[pos] == 'u' &&
                                        str[pos + 1] == 'l' &&
                                        str[pos + 2] == 'l')
                                    {
                                        result[key] = null;
                                        context.Position += 3;
                                        break;
                                    }
                                    throw new JaysonException(JaysonError.InvalidJsonObjectValue);
                                }
                            case 't':
                                {
                                    int pos = context.Position;
                                    if (pos < length - 3 &&
                                        str[pos] == 'r' &&
                                        str[pos + 1] == 'u' &&
                                        str[pos + 2] == 'e')
                                    {
                                        result[key] = true;
                                        context.Position += 3;
                                        break;
                                    }
                                    throw new JaysonException(JaysonError.InvalidJsonObjectValue);
                                }
                            case 'f':
                                {
                                    int pos = context.Position;
                                    if (pos < length - 4 &&
                                        str[pos] == 'a' &&
                                        str[pos + 1] == 'l' &&
                                        str[pos + 2] == 's' &&
                                        str[pos + 3] == 'e')
                                    {
                                        result[key] = false;
                                        context.Position += 4;
                                        break;
                                    }
                                    throw new JaysonException(JaysonError.InvalidJsonObjectValue);
                                }
                            case '[':
                                context.Position--;
                                result[key] = ParseList(context);
                                break;
                            case '/':
                                ParseComment(context);
                                continue;
                            default:
                                if (!(ch < '0' || ch > '9') || ch == '-' || ch == '.')
                                {
                                    context.Position--;
                                    value = ParseNumber(context, JaysonSerializationToken.Value);

                                    result[key] = value;
                                    break;
                                }
                                if (JaysonCommon.IsWhiteSpace(ch))
                                    continue;
                                throw new JaysonException(JaysonError.InvalidJsonObjectValue);
                        }

                        key = null;
                        token = JaysonSerializationToken.Key;
                        prevToken = JaysonSerializationToken.Value;
                        break;
                }
            }

            throw new JaysonException(JaysonError.InvalidJsonObject);
        }

        # endregion Parse Dictionary

        public static object Parse(string str, JaysonDeserializationSettings settings = null)
        {
            if (String.IsNullOrEmpty(str))
            {
                throw new JaysonException(JaysonError.EmptyString);
            }

            using (var context = new JaysonDeserializationContext
            {
                Text = str,
                Length = str.Length,
                Settings = settings ?? JaysonDeserializationSettings.Default
            })
            {
                return Parse(context);
            }
        }

        private static object Parse(JaysonDeserializationContext context)
        {
            var str = context.Text;
            var length = context.Length;

            while ((context.Position < length) &&
                JaysonCommon.IsWhiteSpace(str[context.Position]))
            {
                context.Position++;
            }

            if (context.Position >= length)
            {
                throw new JaysonException(JaysonError.InvalidJson);
            }

            var ch = str[context.Position];

            switch (ch)
            {
                case '{':
                    {
                        return ParseDictionary(context);
                    }
                case '[':
                    {
                        return ParseList(context);
                    }
                case '"':
                    context.Position++;
                    return ParseString(str, ref context.Position);
                case 't':
                    {
                        int pos = context.Position;
                        if (pos == length - 4 &&
                            str[pos + 1] == 'r' &&
                            str[pos + 2] == 'u' &&
                            str[pos + 3] == 'e')
                        {
                            return true;
                        }
                        throw new JaysonException(JaysonError.InvalidJson);
                    }
                case 'f':
                    {
                        int pos = context.Position;
                        if (pos == length - 5 &&
                            str[pos + 1] == 'a' &&
                            str[pos + 2] == 'l' &&
                            str[pos + 3] == 's' &&
                            str[pos + 4] == 'e')
                        {
                            return false;
                        }
                        throw new JaysonException(JaysonError.InvalidJson);
                    }
                case 'n':
                    {
                        int pos = context.Position;
                        if (pos == length - 4 &&
                            str[pos + 1] == 'u' &&
                            str[pos + 2] == 'l' &&
                            str[pos + 3] == 'l')
                        {
                            return null;
                        }
                        throw new JaysonException(JaysonError.InvalidJson);
                    }
                case '/':
                    {
                        context.Position++;
                        ParseComment(context);
                        if (context.Position < length)
                        {
                            return Parse(context);
                        }
                        return null;
                    }
                default:
                    if (!(ch < '0' || ch > '9') || ch == '-' || ch == '.')
                    {
                        return ParseNumber(context, JaysonSerializationToken.Undefined);
                    }
                    throw new JaysonException(JaysonError.InvalidJson);
            }
        }

        # endregion Parse
    }

    # endregion JsonConverter
}