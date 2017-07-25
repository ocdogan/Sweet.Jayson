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

            var ch = (int)context.Text[context.Position];
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
                    var str = context.Text;
                    var length = context.Length;

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
    }

    # endregion JsonConverter
}
