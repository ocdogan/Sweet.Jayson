using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Jayson
{
	# region JaysonObjectConstructor<T>

	internal static class JaysonObjectConstructor<T>
	{
		private static readonly Func<T> function = Creator();

		private static Func<T> Creator()
		{
			Type objType = typeof(T);
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

			return () => (T)FormatterServices.GetUninitializedObject(objType);
		}

		public static T New()
		{
			return function();
		}
	}

	# endregion JaysonObjectConstructor<T>
}