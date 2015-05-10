using System;
using System.Collections.Generic;
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
		private static readonly Dictionary<Type, Func<object>> s_DefaultCache =
			new Dictionary<Type, Func<object>>();

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
                // lock (GetLock(objType))
                lock (info.SyncRoot)
                {
                    if (!s_DefaultCache.TryGetValue(objType, out function))
                    {
                        // if (objType == typeof(string))
                        if (info.JTypeCode == JaysonTypeCode.String)
                        {
                            function = Expression.Lambda<Func<object>>(Expression.Constant(String.Empty)).Compile();
                        }
                        else
                        {
                            // var info = JaysonTypeInfo.GetTypeInfo(objType);
                            if (info.Enum)
                            {
                                return Enum.ToObject(objType, 0L);
                            }
                            else if (info.DefaultJConstructor)
                            {
                                function = Expression.Lambda<Func<object>>(Expression.New(objType)).Compile();
                            }
                            else
                            {
                                function = () => FormatterServices.GetUninitializedObject(objType);
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