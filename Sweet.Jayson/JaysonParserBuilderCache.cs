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
using System.Text;

namespace Sweet.Jayson
{
    # region JaysonParserBuilderCache

    internal static class JaysonParserBuilderCache
    {
#if !(NET3500 || NET3000 || NET2000)
        private const int MAX_BUILDER_SIZE = 360;

        [ThreadStatic]
        private static StringBuilder s_Cached;
#endif

        public static StringBuilder Acquire(int capacity = 20)
        {
#if !(NET3500 || NET3000 || NET2000)
            if (capacity <= MAX_BUILDER_SIZE)
            {
                StringBuilder builder = JaysonParserBuilderCache.s_Cached;
                if (builder != null)
                {
                    if (capacity <= builder.Capacity)
                    {
                        JaysonParserBuilderCache.s_Cached = null;
                        builder.Clear();
                        return builder;
                    }
                }
            }
#endif
            return new StringBuilder(Math.Max(capacity, 20), int.MaxValue);
        }

        public static void Release(StringBuilder builder)
        {
#if !(NET3500 || NET3000 || NET2000)
            if (builder.Capacity <= MAX_BUILDER_SIZE)
            {
                JaysonParserBuilderCache.s_Cached = builder;
            }
#endif
        }

        public static string GetStringAndRelease(StringBuilder builder)
        {
            string result = builder.ToString();
            Release(builder);
            return result;
        }
    }

    # endregion JaysonParserBuilderCache
}