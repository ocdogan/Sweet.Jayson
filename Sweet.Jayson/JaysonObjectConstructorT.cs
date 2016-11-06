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
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Sweet.Jayson
{
    # region JaysonObjectConstructor<T>

    internal static class JaysonObjectConstructor<T>
    {
        private static readonly Func<T> function = Creator();

        private static Func<T> Creator()
        {
            var objType = typeof(T);
            if (objType == typeof(string))
            {
                return Expression.Lambda<Func<T>>(Expression.Constant(String.Empty)).Compile();
            }

            var info = JaysonTypeInfo.GetTypeInfo(objType);
            if (info.Enum)
            {
                return () => (T)Enum.ToObject(objType, 0L);
            }

            if (info.DefaultJConstructor)
            {
                return Expression.Lambda<Func<T>>(Expression.New(objType)).Compile();
            }

            var ctorInfo = JaysonCtorInfo.GetDefaultCtorInfo(objType);
            if (ctorInfo.HasCtor && !ctorInfo.HasParam)
            {
                return Expression.Lambda<Func<T>>(Expression.New(objType)).Compile();
            }

            return () => (T)FormatterServices.GetUninitializedObject(objType);
        }

        public static T New()
        {
            return function();
        }
    }

    # endregion JaysonObjectConstructor<T>
}