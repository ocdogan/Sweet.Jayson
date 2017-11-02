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
        private static object s_EnumNameCacheLock = new object();
        private static Dictionary<Enum, string> s_EnumNameCache = new Dictionary<Enum, string>();

        private static object s_EnumValueCacheLock = new object();
        private static Dictionary<Enum, string> s_EnumValueCache = new Dictionary<Enum, string>();

        private static object s_EnumTypeCacheLock = new object();
        private static Dictionary<Type, Dictionary<string, object>> s_EnumTypeCache = new Dictionary<Type, Dictionary<string, object>>();

        public static string AsIntString(Enum enumValue)
        {
            var contains = false;
            string result;
            lock (s_EnumValueCacheLock)
            {
                contains = s_EnumValueCache.TryGetValue(enumValue, out result);
            }

            if (!contains)
            {
                lock (s_EnumValueCacheLock) 
                {
                    if (!s_EnumValueCache.TryGetValue(enumValue, out result)) 
                    {
                        result = enumValue.ToString("D");
                        s_EnumValueCache[enumValue] = result;
                    }
                }
            }
            return result;
        }

        public static string GetName(Enum enumValue)
        {
            var contains = false;
            string result;
            lock (s_EnumNameCacheLock)
            {
                contains = s_EnumNameCache.TryGetValue(enumValue, out result);
            }

            if (!contains)
            {
                lock (s_EnumNameCacheLock) 
                {
                    if (!s_EnumNameCache.TryGetValue(enumValue, out result)) 
                    {
                        result = enumValue.ToString("F");
                        s_EnumNameCache[enumValue] = result;
                    }
                }
            }
            return result;
        }

        public static object Parse(string str, Type enumType)
        {
            var contains = false;
            Dictionary<string, object> typeDict;
            lock (s_EnumTypeCacheLock)
            {
                contains = s_EnumTypeCache.TryGetValue(enumType, out typeDict);
            }

            if (!contains)
            {
                lock (s_EnumTypeCacheLock) 
                {
                    if (!s_EnumTypeCache.TryGetValue(enumType, out typeDict)) 
                    {
                        typeDict = new Dictionary<string, object>();
                        s_EnumTypeCache[enumType] = typeDict;
                    }
                }
            }

            object result;
            if (!typeDict.TryGetValue(str, out result)) 
            {
                lock (typeDict) 
                {
                    if (!typeDict.TryGetValue(str, out result)) 
                    {
                        result = Enum.Parse(enumType, str);
                        typeDict[str] = result;
                    }
                }
            }

            return result;
        }
    }

    # endregion JaysonEnumCache
}