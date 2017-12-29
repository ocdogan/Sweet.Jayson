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

namespace Sweet.Jayson
{
    # region JaysonEnumCache

    internal static class JaysonEnumCache
    {
        private static JaysonSynchronizedDictionary<Enum, string> s_EnumNameCache = new JaysonSynchronizedDictionary<Enum, string>();
        private static JaysonSynchronizedDictionary<Enum, string> s_EnumValueCache = new JaysonSynchronizedDictionary<Enum, string>();
        private static JaysonSynchronizedDictionary<Type, JaysonSynchronizedDictionary<string, object>> s_EnumTypeCache = new JaysonSynchronizedDictionary<Type, JaysonSynchronizedDictionary<string, object>>();

        public static string AsIntString(Enum enumValue)
        {
            return s_EnumValueCache.GetValueOrUpdate(enumValue, (ev) => ev.ToString("D"));
        }

        public static string GetName(Enum enumValue)
        {
            return s_EnumNameCache.GetValueOrUpdate(enumValue, (ev) => ev.ToString("F"));
        }

        public static object Parse(string str, Type enumType)
        {
            if (!String.IsNullOrEmpty(str))
            {
                var typeDict = s_EnumTypeCache.GetValueOrUpdate(enumType, (et) => new JaysonSynchronizedDictionary<string, object>());
                return typeDict.GetValueOrUpdate(str, (s) => Enum.Parse(enumType, s));
            }
            return null;
        }
    }

    # endregion JaysonEnumCache
}