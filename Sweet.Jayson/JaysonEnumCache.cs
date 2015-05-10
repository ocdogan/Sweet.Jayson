using System;
using System.Collections.Generic;

namespace Sweet.Jayson
{
	# region JaysonEnumCache

	internal static class JaysonEnumCache
	{
		private static Dictionary<Enum, string> s_EnumNameCache = new Dictionary<Enum, string>();
		private static Dictionary<Enum, string> s_EnumValueCache = new Dictionary<Enum, string>();
		private static Dictionary<Type, Dictionary<string, object>> s_EnumTypeCache =
			new Dictionary<Type, Dictionary<string, object>>();

		public static string AsIntString(Enum enumValue)
		{
			string result;
			if (!s_EnumValueCache.TryGetValue(enumValue, out result))
			{
				result = enumValue.ToString("D");
				s_EnumValueCache[enumValue] = result;
			}
			return result;
		}

		public static string GetName(Enum enumValue)
		{
			string result;
			if (!s_EnumNameCache.TryGetValue(enumValue, out result))
			{
				result = enumValue.ToString("F");
				s_EnumNameCache[enumValue] = result;
			}
			return result;
		}

		public static object Parse(string str, Type enumType)
		{
			Dictionary<string, object> typeDict;
			if (!s_EnumTypeCache.TryGetValue(enumType, out typeDict))
			{
				typeDict = new Dictionary<string, object>();
				s_EnumTypeCache[enumType] = typeDict;
			}

			object result;
			if (!typeDict.TryGetValue(str, out result))
			{
				result = Enum.Parse(enumType, str);
				typeDict[str] = result;
			}

			return result;
		}
	}

	# endregion JaysonEnumCache
}