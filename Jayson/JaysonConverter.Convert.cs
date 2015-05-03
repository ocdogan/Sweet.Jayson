using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
#if !(NET3500 || NET3000 || NET2000)
using System.Dynamic;
#endif
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Jayson
{
	# region JsonConverter

	public static partial class JaysonConverter
    {
        # region EvaluatedDictionaryType
        
        private class EvaluatedDictionaryType
        {
            public bool AsDictionary;
            public bool AsReadOnly;
            public Type EvaluatedType;
        }

        # endregion EvaluatedDictionaryType

        # region EvaluatedListType

        private class EvaluatedListType
        {
            public bool AsList;
            public bool AsArray;
            public bool AsReadOnly;
            public Type EvaluatedType;
        }

        # endregion EvaluatedListType

        # region Static Members

        private static readonly Dictionary<Type, ConstructorInfo> s_CtorCache = new Dictionary<Type, ConstructorInfo>();
        private static readonly Dictionary<Type, Func<object[], object>> s_ActivatorCache = new Dictionary<Type, Func<object[], object>>();

        private static readonly Dictionary<Type, EvaluatedListType> s_EvaluatedListTypeCache = new Dictionary<Type, EvaluatedListType>();
        private static readonly Dictionary<Type, EvaluatedDictionaryType> s_EvaluatedDictionaryTypeCache = new Dictionary<Type, EvaluatedDictionaryType>();

		private static readonly Dictionary<Type, Type> s_GenericUnderlyingCache = new Dictionary<Type, Type>();

		# endregion Static Members

		# region Convert

		private static Type GetGenericUnderlyingList(JaysonTypeInfo info)
		{
			Type result;
			if (!s_GenericUnderlyingCache.TryGetValue(info.Type, out result))
			{
				Type[] argTypes = info.GenericArguments;
				if (argTypes[0] == typeof(object)) {
					result = typeof(List<object>);
				} else {
					result = typeof(List<>).MakeGenericType(argTypes);			
				}

				s_GenericUnderlyingCache[info.Type] = result;
			}
			return result;
		}

		private static Type GetGenericUnderlyingDictionary(JaysonTypeInfo info) 
		{
			Type result;
			if (!s_GenericUnderlyingCache.TryGetValue(info.Type, out result))
			{
				Type[] argTypes = info.GenericArguments;
				if (argTypes[0] == typeof(string) && argTypes[1] == typeof(object)) {
					result = typeof(Dictionary<string, object>);
				} else {
					result = typeof(Dictionary<,>).MakeGenericType(argTypes);
				}

				s_GenericUnderlyingCache[info.Type] = result;
			}
			return result;
		}

		private static ConstructorInfo GetReadOnlyCollectionCtor(Type rocType)  
		{
			ConstructorInfo ctor;
			if (!s_CtorCache.TryGetValue(rocType, out ctor)) 
			{
				Type[] argTypes = JaysonTypeInfo.GetTypeInfo(rocType).GenericArguments;

				rocType = typeof(ReadOnlyCollection<>).MakeGenericType(argTypes);
				ctor = rocType.GetConstructor(new Type[] { typeof(IList<>).MakeGenericType(argTypes) });

				s_CtorCache[rocType] = ctor;
			}
			return ctor;
		}

        private static Func<object[], object> GetReadOnlyCollectionActivator(Type rocType)
        {
            Func<object[], object> result;
            if (!s_ActivatorCache.TryGetValue(rocType, out result))
            {
                result = JaysonCommon.CreateActivator(GetReadOnlyCollectionCtor(rocType));
                s_ActivatorCache[rocType] = result;
            }
            return result;
        }

		#if !(NET4000 || NET3500 || NET3000 || NET2000)
		private static ConstructorInfo GetReadOnlyDictionaryCtor(Type rodType)  
		{
			ConstructorInfo ctor;
			if (!s_CtorCache.TryGetValue(rodType, out ctor)) 
			{
				Type[] argTypes = JaysonTypeInfo.GetTypeInfo(rodType).GenericArguments;

				rodType = typeof(ReadOnlyDictionary<,>).MakeGenericType(argTypes);
				ctor = rodType.GetConstructor(new Type[] { typeof(IDictionary<,>).MakeGenericType(argTypes) });

				s_CtorCache[rodType] = ctor;
			}
			return ctor;
		}

        private static Func<object[], object> GetReadOnlyDictionaryActivator(Type rodType)
        {
            Func<object[], object> result;
            if (!s_ActivatorCache.TryGetValue(rodType, out result))
            {
                result = JaysonCommon.CreateActivator(GetReadOnlyDictionaryCtor(rodType));
                s_ActivatorCache[rodType] = result;
            }
            return result;
        }
		#endif

		private static void SetDictionary(IDictionary<string, object> obj, object instance,
            JaysonDeserializationSettings settings)
        {
			if (instance == null || obj == null || obj.Count == 0 || instance is DataTable ||
                instance is DataSet || instance is DBNull)
            {
                return;
            }

			bool hasStype = obj.ContainsKey("$type");

            if (instance is IDictionary<string, object>)
            {
                IDictionary<string, object> instanceDict = (IDictionary<string, object>)instance;

                string key;
                foreach (var entry in obj)
                {
                    key = entry.Key;
					if (!hasStype || key != "$type")
                    {
                        instanceDict[key] = ConvertObject(entry.Value, typeof(object), settings);
                    }
                }
            }
            else if (instance is IDictionary)
            {
                IDictionary instanceDict = (IDictionary)instance;

                object key;
                Type[] genArgs = JaysonCommon.GetGenericDictionaryArgs(instance.GetType());

                if (genArgs != null)
                {
					bool changeValue = hasStype || (genArgs[1] != typeof(object));
                    bool changeKey = !(genArgs[0] == typeof(object) || genArgs[0] == typeof(string));

                    Type keyType = genArgs[0];
                    Type valType = genArgs[1];

                    foreach (var entry in obj)
                    {
						if (!hasStype || entry.Key != "$type")
                        {
							key = changeKey ? ConvertObject(entry.Key, keyType, settings) : entry.Key;
							instanceDict[key] = !changeValue ? entry.Value : ConvertObject(entry.Value, valType, settings); 
                        }
                    }
                    return;
				}

                foreach (var entry in obj)
                {
					if (!hasStype || entry.Key != "$type")
                    {
						instanceDict[entry.Key] = ConvertObject(entry.Value, typeof(object), settings); 
                    }
                }
            }
            else if (instance is NameValueCollection)
            {
                NameValueCollection nvcollection = (NameValueCollection)instance;

                string key;
                object value;
                Type valueType;
                foreach (var item in obj)
                {
                    key = item.Key;
					if (!hasStype || key != "$type")
                    {
                        value = item.Value;
                        if (value == null || value is string)
                        {
                            nvcollection.Add(key, (string)value);
                        }
                        else
                        {
                            valueType = value.GetType();
							if (JaysonTypeInfo.IsJPrimitive(valueType))
                            {
                                nvcollection.Add(key, JaysonFormatter.ToString(value, valueType));
                            }
                            else if (value is IFormattable)
                            {
                                nvcollection.Add(key, ((IFormattable)value).ToString(null, JaysonConstants.ParseCulture));
                            }
                            else
                            {
                                nvcollection.Add(key, ToJsonString(value));
                            }
                        }
                    }
                }
            }
            else if (instance is StringDictionary)
            {
                StringDictionary sidic = (StringDictionary)instance;

                string key;
                object value;
                Type valueType;
                foreach (var item in obj)
                {
                    key = item.Key;
					if (!hasStype || key != "$type")
                    {
                        value = item.Value;
                        if (value == null || value is string)
                        {
                            sidic.Add(key, (string)value);
                        }
                        else
                        {
                            valueType = value.GetType();
							if (JaysonTypeInfo.IsJPrimitive(valueType))
                            {
                                sidic.Add(key, JaysonFormatter.ToString(value, valueType));
                            }
                            else if (value is IFormattable)
                            {
                                sidic.Add(key, ((IFormattable)value).ToString(null, JaysonConstants.ParseCulture));
                            }
                            else
                            {
                                sidic.Add(key, ToJsonString(value));
                            }
                        }
                    }
                }
            }
            else
            {
                Type instanceType = instance.GetType();

                if (JaysonTypeInfo.IsAnonymous(instanceType))
                {
                    if (settings.IgnoreAnonymousTypes)
                    {
                        return;
                    }

					bool caseSensitive = settings.CaseSensitive;
                    bool raiseErrorOnMissingMember = settings.RaiseErrorOnMissingMember;

                    IJaysonFastMember member;
					IDictionary<string, IJaysonFastMember> members = 
						JaysonFastMemberCache.GetAllFieldMembers(instanceType, caseSensitive);

					string memberName;
					object memberValue;

					foreach (var entry in obj)
                    {
						if (!hasStype || entry.Key != "$type")
                        {
							memberName = "<" + (caseSensitive ? entry.Key : entry.Key.ToLower(JaysonConstants.ParseCulture)) + ">";
							if (members.TryGetValue(memberName, out member))
                            {
                                if (member.CanWrite)
                                {
                                    member.Set(instance, ConvertObject(entry.Value, member.MemberType, settings));
                                }
                                else if (entry.Value != null)
                                {
                                    memberValue = member.Get(instance);
                                    if (instance != null)
                                    {
                                        SetObject(memberValue, entry.Value, settings);
                                    }
                                }
                            }
                            else if (raiseErrorOnMissingMember)
                            {
                                throw new JaysonException("Missing member: " + entry.Key);
                            }
                        }
                    }
                }
                else
                {
                    Type[] genArgs = JaysonCommon.GetGenericDictionaryArgs(instanceType);
                    if (genArgs != null)
                    {
                        Action<object, object[]> addMethod = JaysonCommon.GetIDictionaryAddMethod(instanceType);
                        if (addMethod != null)
                        {
                            bool changeVal = genArgs[1] != typeof(object);
                            bool changeKey = !(genArgs[0] == typeof(object) || genArgs[0] == typeof(string));

                            object key;
                            object value;

                            Type keyType = genArgs[0];
                            Type valType = genArgs[1];

                            foreach (var entry in obj)
                            {
								if (!hasStype || entry.Key != "$type")
                                {
									key = changeKey ? ConvertObject(entry.Key, keyType, settings) : entry.Key;
                                    value = changeVal ? ConvertObject(entry.Value, valType, settings) : entry.Value;

                                    addMethod(instance, new object[] { key, value });
                                }
                            }
                            return;
                        }
                    }

					if (!JaysonTypeInfo.IsJPrimitive(instanceType))
                    {
						bool caseSensitive = settings.CaseSensitive;
                        bool raiseErrorOnMissingMember = settings.RaiseErrorOnMissingMember;

                        IJaysonFastMember member;
						IDictionary<string, IJaysonFastMember> members = JaysonFastMemberCache.GetMembers(instanceType, caseSensitive);

						string memberName;
						object memberValue;

						foreach (var entry in obj)
                        {
                            memberName = caseSensitive ? entry.Key : entry.Key.ToLower(JaysonConstants.ParseCulture);

							if (members.TryGetValue(memberName, out member))
                            {
                                if (member.CanWrite)
                                {
                                    member.Set(instance, ConvertObject(entry.Value, member.MemberType, settings));
                                }
                                else if (entry.Value != null)
                                {
                                    memberValue = member.Get(instance);
                                    if (instance != null)
                                    {
                                        SetObject(memberValue, entry.Value, settings);
                                    }
                                }
                            }
                            else if (raiseErrorOnMissingMember)
                            {
                                throw new JaysonException("Missing member: " + entry.Key);
                            }
                        }
                    }
                }
            }
        }

		private static void SetList(IList<object> obj, object instance, JaysonDeserializationSettings settings)
		{
			if (instance == null || obj == null || obj.Count == 0 || instance is DataTable ||
				instance is DataSet || instance is DBNull)
			{
				return;
			}

            Type instanceType = instance.GetType();
            if (instanceType.IsArray)
            {
                Array aResult = (Array)instance;
                Type arrayType = JaysonTypeInfo.GetElementType(instanceType);

                int count = obj.Count;
                for (int i = 0; i < count; i++)
                {
                    aResult.SetValue(ConvertObject(obj[i], arrayType, settings), i);
                }

                return;
            }

            if (instance is IList)
            {
				int count = obj.Count;
                IList lResult = (IList)instance;

                Type argType = JaysonCommon.GetGenericListArgs(instanceType);
                if (argType != null)
                {
                    for (int i = 0; i < count; i++)
                    {
                        lResult.Add(ConvertObject(obj[i], argType, settings));
                    }
					return;
                }

				object item;
                for (int i = 0; i < count; i++)
                {
                    item = obj[i];
                    if (item is IDictionary<string, object>)
                    {
                        item = ConvertDictionary((IDictionary<string, object>)item, typeof(object), settings);
                    }
                    else if (item is IList<object>)
                    {
                        item = ConvertList((IList<object>)item, typeof(object), settings);
                    }

                    lResult.Add(item);
                }
            }
            else
            {
                Type argType = JaysonCommon.GetGenericCollectionArgs(instanceType);
                if (argType != null)
                {
                    Action<object, object[]> methodInfo = JaysonCommon.GetICollectionAddMethod(instanceType);
                    if (methodInfo != null)
                    {
                        int count = obj.Count;
                        for (int i = 0; i < count; i++)
                        {
                            methodInfo(instance, new object[] { ConvertObject(obj[i], argType, settings) });
                        }
                    }
                }
            }
		}

		private static void SetObject(object obj, object instance, JaysonDeserializationSettings settings)
		{
			if (instance != null && obj != null)
			{
				Type instanceType = instance.GetType();
				if (!JaysonTypeInfo.IsJPrimitive(instanceType))
				{
					if (obj is IDictionary<string, object>)
					{
						SetDictionary((IDictionary<string, object>)obj, instance, settings);
					}
					else if (obj is IList<object>)
					{
						SetList((IList<object>)obj, instance, settings);
					}
				}
			}
		}

		private static Type EvaluateListType(Type type, out bool asList, out bool asArray, out bool asReadOnly)
		{
			asList = false;
			asArray = false;
			asReadOnly = false;

			var info = JaysonTypeInfo.GetTypeInfo(type);
			if (info.Array) {
				asArray = true;
				return type;
			}

			if (type == typeof(List<object>)) {
				asList = true;
				return type;
			}

			if (type == typeof(IList) || type == typeof(ArrayList)) {
				asList = true;
				return typeof(ArrayList);
			}

			if (!info.Generic) {
				return type;
			} 

			var genericType = info.GenericTypeDefinitionType;

			if (genericType == typeof(ReadOnlyCollection<>)) {
				asList = true;
				asReadOnly = true;
				return GetGenericUnderlyingList(info);
			} 

			if (info.Interface) {
				#if !(NET4000 || NET3500 || NET3000 || NET2000)
				if (genericType == typeof(IList<>) ||
					genericType == typeof(ICollection<>) || 
					genericType == typeof(IEnumerable<>) ||
					genericType == typeof(IReadOnlyCollection<>)) {
					asList = true;
					return GetGenericUnderlyingList(info);
				}
				#else
				if (genericType == typeof(IList<>) ||
					genericType == typeof(ICollection<>) || 
					genericType == typeof(IEnumerable<>)) {
					asList = true;
					return GetGenericUnderlyingList(info);
				}
				#endif
			}
			return type;
		}

        private static Type GetEvaluatedListType(Type type, out bool asList, out bool asArray, out bool asReadOnly)
        {
            EvaluatedListType elt;
            if (!s_EvaluatedListTypeCache.TryGetValue(type, out elt))
            {
                elt = new EvaluatedListType();
                elt.EvaluatedType = EvaluateListType(type, out asList, out asArray, out asReadOnly);
                elt.AsList = asList;
                elt.AsArray = asArray;
                elt.AsReadOnly = asReadOnly;
                s_EvaluatedListTypeCache[type] = elt;

                return elt.EvaluatedType;
            }

            asList = elt.AsList;
            asArray = elt.AsArray;
            asReadOnly = elt.AsReadOnly;

            return elt.EvaluatedType;
        }

		private static Type EvaluateDictionaryType(Type type, out bool asDictionary, out bool asReadOnly)
		{
			asReadOnly = false;
			asDictionary = false;
			if (type == typeof(Dictionary<string, object>)) {
				asDictionary = true;
				return type;
			}

			if (type == typeof(IDictionary) || type == typeof(Hashtable)) {
				asDictionary = true;
				return typeof(Hashtable);
			}

			var info = JaysonTypeInfo.GetTypeInfo(type);
			if (!info.Generic) {
				return type;
			} 

			var genericType = info.GenericTypeDefinitionType;

			#if !(NET4000 || NET3500 || NET3000 || NET2000)
			if (genericType == typeof(ReadOnlyDictionary<,>)) {
				asReadOnly = true;
				asDictionary = true;
				return GetGenericUnderlyingDictionary(info);
			} 
			#endif

			if (info.Interface) {
				#if !(NET4000 || NET3500 || NET3000 || NET2000)
				if (genericType == typeof(IDictionary<,>) ||
					genericType == typeof(IReadOnlyDictionary<,>)) {
					asDictionary = true;
					return GetGenericUnderlyingDictionary(info);
				} 
				#else
				if (genericType == typeof(IDictionary<,>)) {
					asDictionary = true;
					return GetGenericUnderlyingDictionary(info);
				} 
				#endif

				if (genericType == typeof(ICollection<>) || genericType == typeof(IEnumerable<>)) {
					Type firstArgType = info.GenericArguments[0];

					if (firstArgType == typeof(KeyValuePair<string,object>) ||
						firstArgType == typeof(KeyValuePair<object,object>)) {
						asDictionary = true;
						return GetGenericUnderlyingDictionary(info);
					}

					var argInfo = JaysonTypeInfo.GetTypeInfo(firstArgType);
					if (argInfo.Generic && argInfo.GenericTypeDefinitionType == typeof(KeyValuePair<,>)) {
						asDictionary = true;
						return GetGenericUnderlyingDictionary(info);
					}
				}
			}
			return type;
		}

        private static Type GetEvaluatedDictionaryType(Type type, out bool asDictionary, out bool asReadOnly)
        {
            EvaluatedDictionaryType edt;
            if (!s_EvaluatedDictionaryTypeCache.TryGetValue(type, out edt))
            {
                edt = new EvaluatedDictionaryType();
                edt.EvaluatedType = EvaluateDictionaryType(type, out asDictionary, out asReadOnly);
                edt.AsDictionary = asDictionary;
                edt.AsReadOnly = asReadOnly;
                s_EvaluatedDictionaryTypeCache[type] = edt;
                
                return edt.EvaluatedType;
            }

            asDictionary = edt.AsDictionary;
            asReadOnly = edt.AsReadOnly;
            
            return edt.EvaluatedType;
        }

		private static object ConvertDictionary(IDictionary<string, object> obj, Type toType, 
			JaysonDeserializationSettings settings, bool forceType = false)
		{
			object Stype;
			bool binded = false;

			if (obj.TryGetValue ("$type", out Stype) && Stype != null && (Stype is string)) {
				string typeName = (string)Stype;

				if (!String.IsNullOrEmpty (typeName)) {
                    if (settings.IgnoreAnonymousTypes && JaysonTypeInfo.IsAnonymous(typeName))
                    {
                        return null;
                    }

					if (!forceType || toType == typeof(object)) {
						binded = true;
						Type instanceType = JaysonCommon.GetType (typeName, settings.Binder);
						if (instanceType != null) {
							toType = instanceType;
						}
					}
				}

				object Svalues;
				if (obj.TryGetValue ("$value", out Svalues)) {
					return ConvertObject(Svalues, toType, settings);
				}

				if (obj.TryGetValue ("$values", out Svalues) && (Svalues is IList<object>)) {
					return ConvertList ((IList<object>)Svalues, toType, settings);
				}
			}

			bool asReadOnly, asDictionary;
            toType = GetEvaluatedDictionaryType(toType, out asDictionary, out asReadOnly);

			if (!binded) {
				toType = BindToType(settings, toType);
			}

            object result;
            if (toType != typeof(object))
            {
                result = JaysonObjectConstructor.New(toType);
            }
            else
            {
				#if !(NET3500 || NET3000 || NET2000)
                if (settings.DictionaryType == DictionaryDeserializationType.Expando)
                {
                    result = new ExpandoObject();
                }
                else
                {
                    result = new Dictionary<string, object>(10);
                }
				#else
				result = new Dictionary<string, object>(10);
				#endif
            }
            
            SetDictionary(obj, result, settings);

			#if !(NET4000 || NET3500 || NET3000 || NET2000)
			if (asReadOnly) {
                return GetReadOnlyDictionaryActivator(toType)(new object[] { result });
            }
			#endif

			return result;
		}

		private static Type BindToType (JaysonDeserializationSettings settings, Type type)
		{
			#if !(NET3500 || NET3000 || NET2000)
			if (settings.Binder != null) 
			{
				string typeName;
				string assemblyName;
			
				settings.Binder.BindToName (type, out assemblyName, out typeName);
				if (!String.IsNullOrEmpty (typeName)) 
				{
					Type instanceType = settings.Binder.BindToType (assemblyName, typeName);
					if (instanceType != null) 
					{
						return instanceType;
					}
				}
			}
			#endif
			return type;
		}

		private static object ConvertList(IList<object> obj, Type toType, JaysonDeserializationSettings settings)
		{
			if (toType == typeof(byte[]) && obj.Count == 1) 
			{
				object item = obj [0];
				if (item is string) 
				{
					return Convert.FromBase64String((string)item);
				}
			}

			toType = BindToType(settings, toType);

			if (toType == typeof(object)) {
				if (toType == typeof(object)) {
					switch (settings.ArrayType) {
					case ArrayDeserializationType.ArrayList:
						toType = typeof(ArrayList);
						break;
					case ArrayDeserializationType.Array:
					case ArrayDeserializationType.ArrayDefined:
						toType = typeof(object[]);
						break;
					default:
						toType = typeof(List<object>);
						break;
					}
				}
				toType = typeof(List<object>);
			}

			bool asList, asArray, asReadOnly;
            Type listType = GetEvaluatedListType(toType, out asList, out asArray, out asReadOnly);

			object result;
			if (JaysonTypeInfo.IsArray(listType)) 
			{
				result = Array.CreateInstance (listType, obj.Count);
			} 
			else 
			{
				result = JaysonObjectConstructor.New(listType);
			}

			SetList(obj, result, settings);
			if (result == null) {
                return result;
            }

            Type resultType = result.GetType();
			if (toType == resultType || 
                toType.IsAssignableFrom(resultType)) {
				return result;
			}

			if (asReadOnly) {
                return GetReadOnlyCollectionActivator(toType)(new object[] { result });
			}

			return result;
		}

        /*
		private static DateTime? ConvertToDateTime(string str)
		{
			int length = str.Length;
			if (length == 0) {
				return null;
			}

			char ch;
			int timeZonePos = -1;
			int timeZoneSign = 1;

			for (int i = 1; i < length; i++) {
				ch = str [i];
				if (ch == '-') {
					timeZonePos = i;
					timeZoneSign = -1;
					break;
				}
				if (ch == '+') {
					timeZonePos = i;
					break;
				}
			}

			long l;
			if (timeZonePos == -1) {
				if (!long.TryParse (str, out l)) {
					throw new JaysonException ("Invalid Unix Epoch date format.");
				}

				DateTime dt1 = JaysonCommon.FromUnixTimeMsec (l);
				if (dt1 > JaysonConstants.DateTimeUnixEpochMaxValue) {
					throw new JaysonException ("Invalid Unix Epoch date format.");
				}
				return dt1;
			}

			if (!long.TryParse (str.Substring (0, timeZonePos), out l)) {
				throw new JaysonException ("Invalid Unix Epoch date format.");
			}

			TimeSpan tz = new TimeSpan (int.Parse (str.Substring (length - 4, 2)),
				int.Parse (str.Substring (length - 2, 2)), 0);

			if (timeZoneSign == -1) {
				tz = new TimeSpan (-tz.Ticks);
			}

			DateTime dt2 = JaysonCommon.FromUnixTimeMsec (l, tz);
			if (dt2 > JaysonConstants.DateTimeUnixEpochMaxValue) {
				throw new JaysonException ("Invalid Unix Epoch date format.");
			}
			return dt2;
		}
        */
		        
        private static object ConvertObject(object obj, Type toType, JaysonDeserializationSettings settings)
		{
			toType = BindToType(settings, toType);
			if (toType == typeof(object))
			{
				if (obj != null) {
					if (obj is IDictionary<string, object>) {
						obj = ConvertDictionary ((IDictionary<string, object>)obj, toType, settings);
					} else if (obj is IList<object>) {
						obj = ConvertList ((IList<object>)obj, toType, settings);
					}
				}

				return obj;
			}

			if (obj == null)
			{
				var info = JaysonTypeInfo.GetTypeInfo(toType);
                if (info.Class || info.Nullable)
				{
					return null;
				}

				return JaysonObjectConstructor.New(toType);
			}

			Type objType = obj.GetType();
			if (toType == objType || toType.IsAssignableFrom(objType))
			{
				return obj;
			}

			if (toType == typeof(string))
			{
				if (obj is IFormattable)
				{
					return ((IFormattable)obj).ToString(null, JaysonConstants.ParseCulture);
				}
				if (obj is IConvertible)
				{
					return ((IConvertible)obj).ToString(JaysonConstants.ParseCulture);
				}
				return obj.ToString();
			}

			if (obj is IDictionary<string, object>)
			{
				return ConvertDictionary((IDictionary<string, object>)obj, toType, settings);
			}

			if (obj is IList<object>)
			{
				return ConvertList((IList<object>)obj, toType, settings);
			}

            if (!JaysonTypeInfo.IsClass(toType))
			{
                if (toType == typeof(DateTime) || toType == typeof(DateTime?))
                {
                    string str = obj as string;
                    if (str != null)
                    {
                        if (str.Length == 0)
                        {
                            if (toType == typeof(DateTime?))
                            {
                                return null;
                            }
                            return default(DateTime);
                        }

                        if ((JaysonCommon.StartsWith(str, "/Date(") && JaysonCommon.EndsWith(str, ")/")))
                        {
                            str = str.Substring("/Date(".Length, str.Length - "/Date()/".Length);
                            DateTime? date = JaysonCommon.ParseUnixEpoch(str);
                            if (toType == typeof(DateTime?))
                            {
                                return date;
                            }
                            return date.HasValue ? date.Value : default(DateTime);
                        }
                    }
                }

				bool converted;
				object result = JaysonCommon.ConvertToPrimitive(obj, toType, out converted);
				if (converted)
				{
					return result;
				}

				if (JaysonTypeInfo.IsNullable(toType))
				{
					Type argType = JaysonTypeInfo.GetGenericArguments(toType)[0];
					object value = ConvertObject(obj, argType, settings);

					var constructor = toType.GetConstructor(new Type[] { argType });
					if (constructor != null)
					{
						value = constructor.Invoke(new object[] { value });
					}

					return value;
				}
			}

			if (toType == typeof(byte[]) && objType == typeof(string))
			{
				return Convert.FromBase64String((string)obj);
			}

            if (settings.IgnoreAnonymousTypes && JaysonTypeInfo.IsAnonymous(toType))
            {
                return null;
            }

			#if !(NET3500 || NET3000 || NET2000)
			if (settings.Binder != null)
			{
				string typeName;
				string assemblyName;

				settings.Binder.BindToName(toType, out assemblyName, out typeName);
				if (!String.IsNullOrEmpty(typeName))
				{
					Type instanceType = settings.Binder.BindToType(assemblyName, typeName);
					if (instanceType != null)
					{
						toType = instanceType;
					}
				}
			}
			#endif

			return JaysonObjectConstructor.New(toType);
		}

		# endregion Convert

		# region ToX

		public static IDictionary<string, object> ToDictionary(string str, JaysonDeserializationSettings settings = null)
		{
			if (String.IsNullOrEmpty(str))
			{
				throw new JaysonException("Empty string.");
			}

			if (settings == null) {
				settings = new JaysonDeserializationSettings ();
				JaysonDeserializationSettings.Default.AssignTo (settings);
				settings.DictionaryType = DictionaryDeserializationType.Dictionary;
			}

			return ParseDictionary(new JaysonDeserializationContext
				{
					Text = str,
					Length = str.Length,
					Settings = settings ?? JaysonDeserializationSettings.Default
				});
		}

		public static T ToObject<T>(string str, JaysonDeserializationSettings settings = null)
		{
			if (String.IsNullOrEmpty(str))
			{
				return default(T);
			}
			return (T)ToObject(str, typeof(T), settings);
		}

		public static object ToObject(string str, JaysonDeserializationSettings settings = null)
		{
			return ToObject(str, typeof(object), settings);
		}

		public static object ToObject(string str, Type toType, JaysonDeserializationSettings settings = null)
		{
			if (String.IsNullOrEmpty(str))
			{
				return JaysonTypeInfo.GetDefault(toType);
			}

			var context = new JaysonDeserializationContext
			{
				Text = str,
				Length = str.Length,
				Position = 0,
				Settings = settings ?? JaysonDeserializationSettings.Default
			};

			object result = Parse(context);
			if (result == null) {
				return JaysonTypeInfo.GetDefault(toType);
			}

			var instanceType = result.GetType();

			if (!context.HasTypeInfo) {
				if (toType == instanceType ||
					toType == typeof(object) ||
					toType.IsAssignableFrom(instanceType)) {
					return result;
				}
				return ConvertObject(result, toType, context.Settings);
			}

			var toInfo = JaysonTypeInfo.GetTypeInfo(toType);
			var instanceInfo = JaysonTypeInfo.GetTypeInfo(instanceType);

			if (toInfo.JPrimitive) {
				if (toInfo.Type == instanceInfo.Type) {
					return result;
				}

				if (context.HasTypeInfo) {
					IDictionary<string, object> primeDict = result as IDictionary<string, object>;
					if (primeDict != null) {
						return ConvertDictionary (primeDict, toType, 
							settings ?? JaysonDeserializationSettings.Default, true);
					}
				}

				bool converted;
				return JaysonCommon.ConvertToPrimitive(result, toType, out converted);
			}

			bool asReadOnly = false;
            if (result is IList<object>)
            {
                bool asList, asArray;
                Type listType = GetEvaluatedListType(toType, out asList, out asArray, out asReadOnly);

                result = ConvertList((IList<object>)result, listType, context.Settings);
                if (result == null) {
                    return result;
                }

                Type resultType = result.GetType();
                if (toType == resultType ||
                    toType.IsAssignableFrom(resultType)) {
                    return result;
                }

                if (asReadOnly) {
                    return GetReadOnlyCollectionActivator(toType)(new object[] { result });
                }
                return result;
            }

			if (result is IDictionary<string, object>) {
				bool asDictionary;
                var dictionaryType = GetEvaluatedDictionaryType(toType, out asDictionary, out asReadOnly);
				result = ConvertDictionary((IDictionary<string, object>)result, dictionaryType, context.Settings, true);
			} else if (result is IDictionary) {
				bool asDictionary;
                var dictionaryType = GetEvaluatedDictionaryType(toType, out asDictionary, out asReadOnly);
				result = ConvertDictionary((IDictionary<string, object>)result, dictionaryType, context.Settings);
			} else if (result is IList) {
				result = ConvertList((IList<object>)result, toType, context.Settings);
			} else {
				result = ConvertObject(result, toType, context.Settings);
			}

            if (result == null) {
                return result;
            }

            Type resultTypeD = result.GetType();
            if (toType == resultTypeD ||
                toType.IsAssignableFrom(resultTypeD)) {
                return result;
            }

			#if !(NET4000 || NET3500 || NET3000 || NET2000)
			if (asReadOnly) {
                return GetReadOnlyDictionaryActivator(toType)(new object[] { result });
            }
			#endif

			throw new JaysonException("Unable to cast result to expected type.");
		}

		#if !(NET3500 || NET3000 || NET2000)
		public static dynamic ToDynamic(string str)
		{
			return ToExpando(str);
		}

		public static ExpandoObject ToExpando(string str, JaysonDeserializationSettings settings = null)
		{
			if (settings == null) {
				settings = new JaysonDeserializationSettings ();
				JaysonDeserializationSettings.Default.AssignTo (settings);
				settings.DictionaryType = DictionaryDeserializationType.Expando;
			} else if (settings.DictionaryType != DictionaryDeserializationType.Expando) {
				var temp = new JaysonDeserializationSettings ();
				settings.AssignTo (temp);
				temp = settings;
				settings.DictionaryType = DictionaryDeserializationType.Expando;
			}

			return (ExpandoObject)ToDictionary(str, settings);
		}
		#endif

		# endregion ToX
	}

	# endregion JsonConverter
}
