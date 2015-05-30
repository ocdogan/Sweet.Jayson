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
		private static readonly object s_SyncRoot = new object();

		private static readonly Dictionary<Type, object> s_LockCache = new Dictionary<Type, object>();
		private static readonly Dictionary<Type, Func<object>> s_DefaultCache = new Dictionary<Type, Func<object>>();

		private static object GetLock(Type objType)
		{
			object result;
			if (!s_LockCache.TryGetValue(objType, out result))
			{
				lock (s_SyncRoot)
				{
					if (!s_LockCache.TryGetValue(objType, out result))
					{
						result = new object();
						s_LockCache[objType] = result;
					}
				}
			}
			return result;
		}

		public static object New(Type objType)
		{
            Func<object> function;
            if (!s_DefaultCache.TryGetValue(objType, out function))
            {
                var info = JaysonTypeInfo.GetTypeInfo(objType);
                lock (info.SyncRoot)
                {
                    if (!s_DefaultCache.TryGetValue(objType, out function))
                    {
                        if (info.JTypeCode == JaysonTypeCode.String) {
                            function = Expression.Lambda<Func<object>>(Expression.Constant(String.Empty)).Compile();
                        }
                        else if (info.Enum) {
							function = () => Enum.ToObject(objType, 0L);
                        }                            
						else if (info.DefaultJConstructor) {
							function = Expression.Lambda<Func<object>> (Expression.New (objType)).Compile ();
						} else {
							JaysonCtorInfo ctorInfo = JaysonCtorInfo.GetDefaultCtorInfo(objType);
							if (ctorInfo.HasCtor && !ctorInfo.HasParam) {
								function = Expression.Lambda<Func<object>> (Expression.New (objType)).Compile ();
							} else {
								function = () => FormatterServices.GetUninitializedObject (objType);
							}
						}
						s_DefaultCache[objType] = function;
                    }
                }
            }
            return function();
        }
	}

	# endregion JaysonObjectConstructor
}