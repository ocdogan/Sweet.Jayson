using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Jayson
{
	# region JaysonAnonymousTypeHelper

	internal static class JaysonAnonymousTypeHelper
	{
		private static Dictionary<Type, bool> s_AnonymousTypeCache = new Dictionary<Type, bool>(10);

		public static bool IsAnonymousType(Type objType)
		{
			if (objType != null)
			{
				bool result;

				if (!s_AnonymousTypeCache.TryGetValue(objType, out result) && objType.IsGenericType &&
					(objType.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic)
				{
                    string typeName = objType.Name;
                    
                    result = (typeName.Length > 12) &&
						((typeName[0] == '<' && typeName[1] == '>') ||
							(typeName[0] == 'V' && typeName[1] == 'B' && typeName[2] == '$')) &&
						(typeName.Contains ("AnonType") || typeName.Contains("AnonymousType")) &&
						Attribute.IsDefined(objType, typeof(CompilerGeneratedAttribute), false);

					s_AnonymousTypeCache[objType] = result;
					return result;
				}
			}
			return false;
		}

		public static bool IsAnonymousType(string typeName)
		{
			return !String.IsNullOrEmpty (typeName) &&
				(typeName.Length > 12) &&
				((typeName [0] == '<' && typeName [1] == '>') ||
				(typeName [0] == 'V' && typeName [1] == 'B' && typeName [2] == '$')) &&
				(typeName.Contains ("AnonType") || typeName.Contains ("AnonymousType"));
		}
	}

	# endregion JaysonAnonymousTypeHelper
}