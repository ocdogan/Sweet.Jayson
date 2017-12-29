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
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Sweet.Jayson
{
    # region JaysonAnonymousTypeHelper

    internal static class JaysonAnonymousTypeHelper
    {
        private static JaysonSynchronizedDictionary<Type, bool> s_AnonymousTypeCache = new JaysonSynchronizedDictionary<Type, bool>(10);

        public static bool IsAnonymousType(Type objType)
        {
            if (objType != null)
            {
                return s_AnonymousTypeCache.GetValueOrUpdate(objType, (t) =>
                    {
                        return t.IsGenericType &&
                                (t.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic &&
                                IsAnonymousType(t.Name) &&
                                Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute), false);
                    });
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