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
using System.Reflection;

namespace Sweet.Jayson
{
	# region JaysonFastMemberCache

	internal static class JaysonFastMemberCache
	{
		# region Constants

		private const int CacheCapacity = 10000;

		# endregion Constants

		# region Static Members

		private static readonly object s_TypeMembersLock = new object();

		private static Dictionary<Type, IDictionary<string, IJaysonFastMember>> s_TypeMembers =
            new Dictionary<Type, IDictionary<string, IJaysonFastMember>>(JaysonConstants.CacheInitialCapacity);
		private static Dictionary<Type, IDictionary<string, IJaysonFastMember>> s_TypeAllFieldMembers =
            new Dictionary<Type, IDictionary<string, IJaysonFastMember>>(JaysonConstants.CacheInitialCapacity);

		private static Dictionary<Type, IDictionary<string, IJaysonFastMember>> s_TypeInvariantMembers =
            new Dictionary<Type, IDictionary<string, IJaysonFastMember>>(JaysonConstants.CacheInitialCapacity);
		private static Dictionary<Type, IDictionary<string, IJaysonFastMember>> s_TypeInvariantAllFieldMembers =
            new Dictionary<Type, IDictionary<string, IJaysonFastMember>>(JaysonConstants.CacheInitialCapacity);

		# endregion Static Members

		public static IJaysonFastMember GetMember(Type objType, string memberName, bool caseSensitive = true)
		{
			IJaysonFastMember result = null;
			if ((objType != null) && (memberName != null) && (memberName.Length > 0))
			{
				IDictionary<string, IJaysonFastMember> members = GetMembers(objType, caseSensitive);
				members.TryGetValue(memberName, out result);
			}
			return result;
		}

		private static void FillMembers(Type objType, out IDictionary<string, IJaysonFastMember> members,
			out IDictionary<string, IJaysonFastMember> membersInvariant)
		{
			members = new Dictionary<string, IJaysonFastMember>(5);
			membersInvariant = new Dictionary<string, IJaysonFastMember>(5);

			IJaysonFastMember member;

			PropertyInfo[] pis = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.GetProperty | BindingFlags.SetProperty);

			PropertyInfo pi;
			for (int i = pis.Length - 1; i > -1; i--)
			{
				pi = pis[i];
				if (pi.CanRead)
				{
					var iParams = pi.GetIndexParameters();

					if ((iParams == null || iParams.Length == 0) &&
						pi.GetCustomAttributes(typeof(JaysonIgnoreMemberAttribute), true).FirstOrDefault () == null)
					{
						member = new JaysonFastProperty(pi, true, true);

						members.Add(pi.Name, member);
						membersInvariant.Add(pi.Name.ToLower(JaysonConstants.InvariantCulture), member);
					}
				}
			}

			FieldInfo[] fis = objType.GetFields(BindingFlags.Instance | BindingFlags.Public);

			FieldInfo fi;
			for (int i = fis.Length - 1; i > -1; i--)
			{
				fi = fis[i];
				if (fi.GetCustomAttributes(typeof(JaysonIgnoreMemberAttribute), true).FirstOrDefault () == null)
				{
					member = new JaysonFastField(fi, true, true);

					members.Add(fi.Name, member);
                    membersInvariant.Add(fi.Name.ToLower(JaysonConstants.InvariantCulture), member);
				}
			}
		}

		public static IDictionary<string, IJaysonFastMember> GetMembers(Type objType, bool caseSensitive = true)
		{
			if (objType != null)
			{
				IDictionary<string, IJaysonFastMember> members = null;

				if (caseSensitive) {
					if (!s_TypeMembers.TryGetValue(objType, out members))
					{
						lock (s_TypeMembersLock)
						{
							if (!s_TypeMembers.TryGetValue(objType, out members))
							{
								IDictionary<string, IJaysonFastMember> members1;
								FillMembers(objType, out members, out members1);

								s_TypeMembers[objType] = members;
								s_TypeInvariantMembers[objType] = members1;
							}
						}
					}

					return members;
				} 

				if (!s_TypeInvariantMembers.TryGetValue(objType, out members))
				{
					lock (s_TypeMembersLock)
					{
						if (!s_TypeInvariantMembers.TryGetValue(objType, out members))
						{
							IDictionary<string, IJaysonFastMember> members2;
							FillMembers(objType, out members2, out members);

							s_TypeMembers[objType] = members2;
							s_TypeInvariantMembers[objType] = members;
						}
					}
				}
				return members;
			}
			return null;
		}

		private static void FillAllFieldMembers(Type objType, out IDictionary<string, IJaysonFastMember> members, 
			out IDictionary<string, IJaysonFastMember> membersInvariant)
		{
			members = new Dictionary<string, IJaysonFastMember>(5);
			membersInvariant = new Dictionary<string, IJaysonFastMember>(5);

			FieldInfo[] fis = objType.GetFields(BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.NonPublic);

			FieldInfo fi;
			JaysonFastField fastField;
			for (int i = fis.Length - 1; i > -1; i--)
			{
				fi = fis[i];
				if (fi.GetCustomAttributes(typeof(JaysonIgnoreMemberAttribute), true).FirstOrDefault () == null)
				{
					fastField = new JaysonFastField(fi, true, true);

					members.Add(fi.Name, fastField);
                    membersInvariant.Add(fi.Name.ToLower(JaysonConstants.InvariantCulture), fastField);
				}
			}
		}

		public static IDictionary<string, IJaysonFastMember> GetAllFieldMembers(Type objType, bool caseSensitive = true)
		{
			if (objType != null)
			{
				IDictionary<string, IJaysonFastMember> members = null;

				if (caseSensitive) 
				{
					if (!s_TypeAllFieldMembers.TryGetValue(objType, out members))
					{
						lock (s_TypeMembersLock)
						{
							if (!s_TypeAllFieldMembers.TryGetValue(objType, out members))
							{
								IDictionary<string, IJaysonFastMember> members1;
								FillAllFieldMembers(objType, out members, out members1);

								s_TypeAllFieldMembers[objType] = members;
								s_TypeInvariantAllFieldMembers[objType] = members1;
							}
						}
					}
					return members;
				}
	
				if (!s_TypeInvariantAllFieldMembers.TryGetValue(objType, out members))
				{
					lock (s_TypeMembersLock)
					{
						if (!s_TypeInvariantAllFieldMembers.TryGetValue(objType, out members))
						{
							IDictionary<string, IJaysonFastMember> members1;
							FillAllFieldMembers(objType, out members1, out members);

							s_TypeAllFieldMembers[objType] = members1;
							s_TypeInvariantAllFieldMembers[objType] = members;
						}
					}
				}
				return members;
			}
			return null;
		}
	}

	# endregion JaysonFastMemberCache
}