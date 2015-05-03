using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
#if !(NET3500 || NET3000 || NET2000)
using System.Dynamic;
#endif
using System.Linq;
using System.Text;

namespace Jayson
{
	# region JsonConverter

	public static partial class JaysonConverter
	{
		# region ToJsonObject

		private static Dictionary<string, object> AsDictionary(IDictionary obj, JaysonSerializationContext context)
		{
			if (obj.Count == 0)
			{
				return new Dictionary<string, object>();
			}

			Dictionary<string, object> result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

			string key;
			object value;
			object keyObj;
			bool ignoreNullValues = context.Settings.IgnoreNullValues;

			Func<string, object, object> filter = context.Filter;
			bool canFilter = (filter != null);

			foreach (DictionaryEntry dEntry in obj)
			{
				value = dEntry.Value;

				keyObj = dEntry.Key;
				key = (keyObj is string) ? (string)keyObj : keyObj.ToString();

				if (value != null)
				{
					if (canFilter)
					{
						value = filter(key, value);
					}

					if (value != null)
					{
						value = ToJsonObject(value, context);
					}
				}

				if ((value != null) || !ignoreNullValues)
				{
					result[key] = value;
				}
			}

			return result;
		}

		private static Dictionary<string, object> AsStringDictionary(StringDictionary obj, JaysonSerializationContext context)
		{
			if (obj.Count == 0)
			{
				return new Dictionary<string, object>();
			}

			Dictionary<string, object> result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

			string key;
			object value;
			bool ignoreNullValues = context.Settings.IgnoreNullValues;

			Func<string, object, object> filter = context.Filter;
			bool canFilter = (filter != null);

			foreach (DictionaryEntry dEntry in obj)
			{
				key = (string)dEntry.Key;
				value = dEntry.Value;

				if (value != null)
				{
					if (canFilter)
					{
						value = filter(key, value);
					}

					if (value != null)
					{
						value = ToJsonObject(value, context);
					}
				}

				if ((value != null) || !ignoreNullValues)
				{
					result.Add(key, value);
				}
			}

			return result;
		}

		private static Dictionary<string, object> AsNameValueCollection(NameValueCollection obj, JaysonSerializationContext context)
		{
			if (obj.Count == 0)
			{
				return new Dictionary<string, object>();
			}

			Dictionary<string, object> result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

			object value;
			bool ignoreNullValues = context.Settings.IgnoreNullValues;

			Func<string, object, object> filter = context.Filter;
			bool canFilter = (filter != null);

			foreach (var key in obj.AllKeys)
			{
				value = obj[key];

				if (value != null)
				{
					if (canFilter)
					{
						value = filter(key, value);
					}

					if (value != null)
					{
						value = ToJsonObject(value, context);
					}
				}

				if ((value != null) || !ignoreNullValues)
				{
					result.Add(key, value);
				}
			}

			return result;
		}

		#if !(NET3500 || NET3000 || NET2000)
		private static Dictionary<string, object> AsDynamicObject(DynamicObject obj, JaysonSerializationContext context)
		{
			object value;
			bool ignoreNullValues = context.Settings.IgnoreNullValues;

			Dictionary<string, object> result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

			Func<string, object, object> filter = context.Filter;
			bool canFilter = (filter != null);

			string fKey;
			foreach (var memberKvp in JaysonFastMemberCache.GetMembers(obj.GetType()))
			{
				fKey = memberKvp.Key;
				value = memberKvp.Value.Get(obj);

				if (value != null)
				{
					if (canFilter)
					{
						value = filter(fKey, value);
					}

					if (value != null)
					{
						value = ToJsonObject(value, context);
					}
				}

				if ((value != null) || !ignoreNullValues)
				{
					result.Add(fKey, value);
				}
			}

			JaysonDynamicWrapper dObj = new JaysonDynamicWrapper((IDynamicMetaObjectProvider)obj);

			foreach (var dKey in obj.GetDynamicMemberNames())
			{
				value = dObj[dKey];

				if (value != null)
				{
					if (canFilter)
					{
						value = filter(dKey, value);
					}

					if (value != null)
					{
						value = ToJsonObject(value, context);
					}
				}

				if ((value != null) || !ignoreNullValues)
				{
					result.Add(dKey, value);
				}
			}

			return result;
		}
		#endif

		private static Dictionary<string, object> AsObject(object obj, Type objType, JaysonSerializationContext context)
		{
			string key;
			object value;
			bool ignoreNullValues = context.Settings.IgnoreNullValues;

			Dictionary<string, object> result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

			Func<string, object, object> filter = context.Filter;
			bool canFilter = (filter != null);

			foreach (var memberKvp in JaysonFastMemberCache.GetMembers(objType))
			{
				key = memberKvp.Key;
				value = memberKvp.Value.Get(obj);

				if (value != null)
				{
					if (canFilter)
					{
						value = filter(key, value);
					}

					if (value != null)
					{
						value = ToJsonObject(value, context);
					}
				}

				if ((value != null) || !ignoreNullValues)
				{
					result.Add(key, value);
				}
			}

			return result;
		}

		private static Dictionary<string, object> AsGenericStringDictionary(IDictionary<string, object> obj,
			JaysonSerializationContext context)
		{
			if (obj.Count == 0)
			{
				return new Dictionary<string, object>();
			}

			Dictionary<string, object> result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

			string key;
			object value;
			bool ignoreNullValues = context.Settings.IgnoreNullValues;

			Func<string, object, object> filter = context.Filter;
			bool canFilter = (filter != null);

			foreach (var kvp in obj)
			{
				key = kvp.Key;
				value = kvp.Value;

				if (value != null)
				{
					if (canFilter)
					{
						value = filter(key, value);
					}

					if (value != null)
					{
						value = ToJsonObject(value, context);
					}
				}

				if ((value != null) || !ignoreNullValues)
				{
					result.Add(key, value);
				}
			}

			return result;
		}

		private static Array AsArray(Array obj, Type objType, JaysonSerializationContext context)
		{
            var info = JaysonTypeInfo.GetTypeInfo(JaysonTypeInfo.GetElementType(objType));
            if (info.JPrimitive || info.Enum)
			{
				return obj;
			}

			if (info.Type == typeof(DBNull))
			{
				if (context.Settings.IgnoreNullValues)
				{
					return JaysonConstants.EmptyObjArray;
				}
				return new object[obj.Length];
			}

			return AsIList(obj, context);
		}

		private static object[] AsIList(IList obj, JaysonSerializationContext context)
		{
			int count = obj.Count;
			if (count == 0)
			{
				return JaysonConstants.EmptyObjArray;
			}

			object[] result = new object[count];
			bool ignoreNullValues = context.Settings.IgnoreNullValues;

			object item;

			for (int i = 0; i < count; i++)
			{
				item = obj[i];
				if (item != null)
				{
					item = ToJsonObject(item, context);
				}

				if ((item != null) || !ignoreNullValues)
				{
					result[i] = item;
				}
			}

			return result;
		}

		private static object[] AsICollection(ICollection obj, JaysonSerializationContext context)
		{
			int count = obj.Count;
			if (count == 0)
			{
				return JaysonConstants.EmptyObjArray;
			}

			object[] result = new object[count];
			obj.CopyTo(result, 0);

			object item;
			bool ignoreNullValues = context.Settings.IgnoreNullValues;

			for (int i = 0; i < count; i++)
			{
				item = result[i];
				if (item != null)
				{
					item = ToJsonObject(item, context);
				}

				if ((item != null) || !ignoreNullValues)
				{
					result[i] = item;
				}
			}

			return result;
		}

		private static object AsEnumerable(IEnumerable obj, Type objType, JaysonSerializationContext context)
		{
			if (obj is Array)
			{
				obj = AsArray((Array)obj, objType, context);
			}
			else if (obj is IList)
			{
				obj = AsIList((IList)obj, context);
			}
			else
			{
				bool ignoreNullValues = context.Settings.IgnoreNullValues;

				Type entryType;
				JaysonDictionaryType dType = JaysonCommon.GetDictionaryType(obj, out entryType);

				IEnumerator enumerator = obj.GetEnumerator();
				try
				{
					if (dType == JaysonDictionaryType.IDictionary)
					{
						string key;
						object value;
						object keyObj;
						DictionaryEntry dEntry;

						Dictionary<string, object> result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

						Func<string, object, object> filter = context.Filter;
						bool canFilter = (filter != null);

						while (enumerator.MoveNext())
						{
							dEntry = (DictionaryEntry)enumerator.Current;

							value = dEntry.Value;

							keyObj = dEntry.Key;
							key = (keyObj is string) ? (string)keyObj : keyObj.ToString();

							if (value != null)
							{
								if (canFilter)
								{
									value = filter(key, value);
								}

								if (value != null)
								{
									value = ToJsonObject(value, context);
								}
							}

							if ((value != null) || !ignoreNullValues)
							{
								result[key] = value;
							}
						}

						obj = result;
					}
					else if (dType == JaysonDictionaryType.IGenericDictionary)
					{
						Dictionary<string, object> result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);
						IDictionary<string, IJaysonFastMember> members = JaysonFastMemberCache.GetMembers(entryType);

						IJaysonFastMember keyFm;
						IJaysonFastMember valueFm;

						if (members.TryGetValue("Key", out keyFm) && members.TryGetValue("Value", out valueFm))
						{
							string key;
							object value;
							object keyObj;

							Func<string, object, object> filter = context.Filter;
							bool canFilter = (filter != null);

							while (enumerator.MoveNext())
							{
								keyObj = keyFm.Get(enumerator.Current);
								if (keyObj != null)
								{
									key = (keyObj is string) ? (string)keyObj : keyObj.ToString();
									value = valueFm.Get(enumerator.Current);

									if (value != null)
									{
										if (canFilter)
										{
											value = filter(key, value);
										}

										if (value != null)
										{
											value = ToJsonObject(value, context);
										}
									}

									if ((value != null) || !ignoreNullValues)
									{
										result[key] = value;
									}
								}
							}
						}

						obj = result;
					}
					else if (obj is ICollection)
					{
						obj = AsICollection((ICollection)obj, context);
					}
					else
					{
						object item;
						List<object> result = new List<object>();

						while (enumerator.MoveNext())
						{
							item = enumerator.Current;
							if (item != null)
							{
								item = ToJsonObject(item, context);
							}

							if ((item != null) || !ignoreNullValues)
							{
								result.Add(item);
							}
						}
						obj = result.ToArray();
					}
				}
				finally
				{
					if (enumerator is IDisposable)
					{
						((IDisposable)enumerator).Dispose();
					}
				}
			}
			return obj;
		}

		private static object ToJsonObject(object obj, JaysonSerializationContext context)
		{
			if (obj != null)
			{
				if (JaysonTypeInfo.IsJPrimitiveObject(obj))
				{
					return obj;
				}

				var info = JaysonTypeInfo.GetTypeInfo(obj.GetType());
				if (info.Enum)
				{
					return obj;
				}

				if (!(obj is DataSet || obj is DataTable))
				{
					if (obj == DBNull.Value)
					{
						return null;
					}

					JaysonStackList stack = null;
					if (info.Class)
					{
						stack = context.Stack;
						if (stack.Contains(obj))
						{
							if (context.Settings.RaiseErrorOnCircularRef) 
							{
								throw new JaysonException ("Circular reference on " + info.Type.Name);
							}
							return null;
						}

						stack.Push(obj);
					}

					try
					{
						JaysonSerializationSettings settings = context.Settings;
						#if !(NET3500 || NET3000 || NET2000)
						if (settings.DisableExpandoObjects && (info.Type == typeof(ExpandoObject)))
						{
							return null;
						}
						#endif

                        if (settings.DisableAnonymousTypes && info.Anonymous)
						{
							return null;
						}

						if (obj is IDictionary<string, object>)
						{
							return AsGenericStringDictionary((IDictionary<string, object>)obj, context);
						}

						if (obj is IDictionary)
						{
							return AsDictionary((IDictionary)obj, context);
						}

						if (obj is StringDictionary)
						{
							return AsStringDictionary((StringDictionary)obj, context);
						}

						if (obj is NameValueCollection)
						{
							return AsNameValueCollection((NameValueCollection)obj, context);
						}

						#if !(NET3500 || NET3000 || NET2000)
						if (obj is DynamicObject)
						{
							if (!settings.DisableDynamicObjects)
							{
								return AsDynamicObject((DynamicObject)obj, context);
							}
							return null;
						}
						#endif

						if (obj is IEnumerable)
						{
							return AsEnumerable((IEnumerable)obj, info.Type, context);
						}

						return AsObject(obj, info.Type, context);
					}
					finally
					{
						if (info.Class)
						{
							stack.Pop();
						}
					}
				}
			}

			return obj;
		}

		public static object ToJsonObject(object obj, JaysonSerializationSettings settings = null,
			Func<string, object, object> filter = null)
		{
			JaysonStackList stack = JaysonStackList.Get();
			try
			{
				return ToJsonObject(obj, new JaysonSerializationContext(filter: filter,
					settings: settings ?? JaysonSerializationSettings.Default,
					stack: stack
				));
			}
			finally
			{
				JaysonStackList.Release(stack);
			}
		}

		# endregion ToJsonObject
	}

	# endregion JsonConverter
}
