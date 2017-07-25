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
        # region Parse String

        private static int HexToUnicodeChar(string str, int start)
        {
            var result = 0;

            char ch;
            var decimalCh = 0;

            var to = start + 4;
            for (var i = start; i < to; i++)
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
    }

    # endregion JsonConverter
}
