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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Sweet.Jayson
{
    # region JaysonObjectConstructor

    internal static class JaysonObjectConstructor
    {
        private static readonly JaysonSynchronizedDictionary<Type, object> s_LockCache = new JaysonSynchronizedDictionary<Type, object>();
        private static readonly JaysonSynchronizedDictionary<Type, Func<object>> s_DefaultCache = new JaysonSynchronizedDictionary<Type, Func<object>>();

        private static object GetLock(Type objType)
        {
            return s_LockCache.GetValueOrUpdate(objType, (t) => new object());
        }

        public static object New(Type objType)
        {
            if ((objType == null) || objType.IsInterface || objType.IsAbstract)
                return null;

            var function = s_DefaultCache.GetValueOrUpdate(objType, (t) => {
                var info = JaysonTypeInfo.GetTypeInfo(objType);

                if (info.JTypeCode == JaysonTypeCode.String)
                    return Expression.Lambda<Func<object>>(Expression.Constant(String.Empty)).Compile();
                
                if (info.Enum)
                    return () => Enum.ToObject(t, 0L);

                if (info.DefaultJConstructor)
                {
                    var newExp = (Expression)Expression.New(t);
                    newExp = !info.ValueType ? newExp : Expression.Convert(newExp, typeof(object));
                    return Expression.Lambda<Func<object>>(newExp).Compile();
                }

                var ctorInfo = JaysonCtorInfo.GetDefaultCtorInfo(t);
                if (ctorInfo.HasCtor && !ctorInfo.HasParam)
                {
                    var newExp = (Expression)Expression.New(t);
                    newExp = !info.ValueType ? newExp : Expression.Convert(newExp, typeof(object));
                    return Expression.Lambda<Func<object>>(newExp).Compile();
                }

                return () => FormatterServices.GetUninitializedObject(t);
            });

            return function();
        }
    }

    # endregion JaysonObjectConstructor
}