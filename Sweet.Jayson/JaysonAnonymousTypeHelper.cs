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
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Sweet.Jayson
{
    # region JaysonAnonymousTypeHelper

    internal static class JaysonAnonymousTypeHelper
    {
        private static object s_SyncObj = new object();
        private static Dictionary<Type, bool> s_AnonymousTypeCache = new Dictionary<Type, bool>(10);

        public static bool IsAnonymousType(Type objType)
        {
            if (objType != null)
            {
                bool result;
                if (!s_AnonymousTypeCache.TryGetValue(objType, out result))
                {
                    lock (s_SyncObj)
                    {
                        if (!s_AnonymousTypeCache.TryGetValue(objType, out result))
                        {
                            result = objType.IsGenericType &&
                                (objType.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic &&
                                IsAnonymousType(objType.Name) &&
                                Attribute.IsDefined(objType, typeof(CompilerGeneratedAttribute), false);

                            s_AnonymousTypeCache[objType] = result;
                            return result;
                        }
                    }
                }
            }
            return false;
        }

        public static bool IsAnonymousType(string typeName)
        {
            return !String.IsNullOrEmpty(typeName) &&
                (typeName.Length > 12) &&
                ((typeName[0] == '<' && typeName[1] == '>') ||
                (typeName[0] == 'V' && typeName[1] == 'B' && typeName[2] == '$')) &&
                (typeName.Contains("AnonType") || typeName.Contains("AnonymousType"));
        }
    }

    # endregion JaysonAnonymousTypeHelper
}