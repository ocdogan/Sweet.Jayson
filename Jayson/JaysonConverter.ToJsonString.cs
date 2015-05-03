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
		# region ToJsonString

		private static void WriteObjectTypeValue (Type objType, JaysonSerializationContext context)
		{
			StringBuilder builder = context.Builder;
			if (context.Settings.Formatting) {
				builder.Append ('{');
				builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				builder.Append ("\"$type\": \"");
			}
			else {
				builder.Append ("{\"$type\":\"");
			}
            builder.Append(JaysonTypeInfo.GetTypeName(objType, context.Settings.TypeNameInfo));
			builder.Append ('"');
		}

		private static void WriteListTypeValue (Type objType, JaysonSerializationContext context)
		{
			StringBuilder builder = context.Builder;
			if (context.Settings.Formatting) {
				builder.Append ('{');
				builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				builder.Append ("\"$type\": \"");
                builder.Append(JaysonTypeInfo.GetTypeName(objType, context.Settings.TypeNameInfo));
				builder.Append ('"');
				builder.Append (',');
				builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				builder.Append ("\"$values\": [");
			}
			else {
				builder.Append ("{\"$type\": \"");
                builder.Append(JaysonTypeInfo.GetTypeName(objType, context.Settings.TypeNameInfo));
				builder.Append ("\",\"$values\":[");
			}
		}

		private static void WritePrimitiveTypeValue (Type objType, JaysonSerializationContext context)
		{
			StringBuilder builder = context.Builder;
			if (context.Settings.Formatting) {
				builder.Append ('{');
				builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				builder.Append ("\"$type\": \"");
				builder.Append(JaysonTypeInfo.GetTypeName(objType, context.Settings.TypeNameInfo));
				builder.Append ('"');
				builder.Append (',');
				builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				builder.Append ("\"$value\": ");
			}
			else {
				builder.Append ("{\"$type\": \"");
				builder.Append(JaysonTypeInfo.GetTypeName(objType, context.Settings.TypeNameInfo));
				builder.Append ("\",\"$value\":");
			}
		}

		private static bool WriteObjectType(object obj, JaysonSerializationContext context)
		{
			switch (context.Settings.TypeNames) {
			case JaysonTypeNameSerialization.None:
				break;
			case JaysonTypeNameSerialization.Auto: 
				{
					Type objType = obj.GetType ();
					if (objType != context.CurrentType &&
						objType != JaysonConstants.DefaultDictionaryType) {
						WriteObjectTypeValue (objType, context);
						return true;
					}
				}
				break;
			case JaysonTypeNameSerialization.All: 
			case JaysonTypeNameSerialization.Objects: 
				{
					WriteObjectTypeValue (obj.GetType(), context);
					return true;
				}
			default:
				break;
			}
			return false;
		}

		private static bool WriteObjectType(Type objType, JaysonSerializationContext context)
		{
			switch (context.Settings.TypeNames) {
			case JaysonTypeNameSerialization.None:
				break;
			case JaysonTypeNameSerialization.Auto: 
				{
					if (objType != context.CurrentType &&
						objType != JaysonConstants.DefaultDictionaryType) {
						WriteObjectTypeValue (objType, context);
						return true;
					}
				}
				break;
			case JaysonTypeNameSerialization.All: 
			case JaysonTypeNameSerialization.Objects: 
				{
					WriteObjectTypeValue (objType, context);
					return true;
				}
			default:
				break;
			}
			return false;
		}

		private static bool WriteListType(object obj, JaysonSerializationContext context)
		{
			switch (context.Settings.TypeNames) {
			case JaysonTypeNameSerialization.None:
				break;
			case JaysonTypeNameSerialization.Auto: 
				{
					Type objType = obj.GetType ();
					if (objType != context.CurrentType &&
						objType != JaysonConstants.DefaultDictionaryType) {
						WriteListTypeValue (objType, context);
						return true;
					}
				}
				break;
			case JaysonTypeNameSerialization.All: 
			case JaysonTypeNameSerialization.Arrays: 
				{
					WriteListTypeValue (obj.GetType (), context);
					return true;
				}
			default:
				break;
			}
			return false;
		}

		private static bool WriteListType(Type objType, JaysonSerializationContext context)
		{
			switch (context.Settings.TypeNames) {
			case JaysonTypeNameSerialization.None:
				break;
			case JaysonTypeNameSerialization.Auto: 
				{
					if (objType != context.CurrentType &&
						objType != JaysonConstants.DefaultDictionaryType) {
						WriteListTypeValue (objType, context);
						return true;
					}
				}
				break;
			case JaysonTypeNameSerialization.All: 
			case JaysonTypeNameSerialization.Arrays: 
				{
					WriteListTypeValue (objType, context);
					return true;
				}
			default:
				break;
			}
			return false;
		}

		private static bool WriteByteArrayType(JaysonSerializationContext context)
		{
			switch (context.Settings.TypeNames) {
			case JaysonTypeNameSerialization.None:
				break;
			case JaysonTypeNameSerialization.Auto: 
				{
					if (context.CurrentType != typeof(byte[])) {
						WritePrimitiveTypeValue (typeof(byte[]), context);
						return true;
					}
				}
				break;
			case JaysonTypeNameSerialization.All: 
			case JaysonTypeNameSerialization.Arrays: 
				{
					WritePrimitiveTypeValue (typeof(byte[]), context);
					return true;
				}
			default:
				break;
			}
			return false;
		}

		private static bool WriteKeyValueEntry(string key, object value, JaysonSerializationContext context, 
			bool isFirst, bool ignoreEscape = false)
		{
			if ((value != null) && (value != DBNull.Value)) {
				Type valueType = value.GetType ();
				if (CanWriteJsonObject (value, valueType, context)) {
					StringBuilder builder = context.Builder;
					JaysonSerializationSettings settings = context.Settings;

					if (!isFirst) {
						builder.Append (',');
					}
					if (settings.Formatting) {
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					}

					builder.Append ('"');

					if (!settings.CaseSensitive) {
						key = key.ToLowerInvariant();
					}

					if (ignoreEscape || !(settings.EscapeChars || settings.EscapeUnicodeChars)) {
						builder.Append (key);
					} else {
						JaysonFormatter.EncodeUnicodeString (key, builder, settings.EscapeUnicodeChars);
					}

					if (settings.Formatting) {
						builder.Append ("\": ");
					} else {
						builder.Append ('"');
						builder.Append (':');
					}

					WriteJsonObject (value, valueType, context);

					return false; // isFirst
				}
			} else if (!context.Settings.IgnoreNullValues) {
				StringBuilder builder = context.Builder;
				JaysonSerializationSettings settings = context.Settings;

				if (!isFirst) {
					builder.Append (',');
				}
				if (context.Settings.Formatting) {
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				}

				builder.Append ('"');

				if (ignoreEscape || !(settings.EscapeChars || settings.EscapeUnicodeChars)) {
					builder.Append (key);
				} else {
					JaysonFormatter.EncodeUnicodeString (key, builder, settings.EscapeUnicodeChars);
				}

				if (settings.Formatting) {
					builder.Append ("\": null");
				} else {
					builder.Append ("\":null");
				}
				return false; // isFirst
			}

			return isFirst;
		}

		private static bool WriteEnumerableValue(object value, JaysonSerializationContext context, bool isFirst)
		{
			if ((value != null) && (value != DBNull.Value))
			{
				Type valueType = value.GetType();
				if (CanWriteJsonObject(value, valueType, context))
				{
					if (!isFirst)
					{
						context.Builder.Append(',');
					}
					if (context.Settings.Formatting) {
						context.Builder.Append (JaysonConstants.Indentation[context.ObjectDepth]);
					}

					WriteJsonObject(value, valueType, context);
					return false; // isFirst
				}
			}
			else if (!context.Settings.IgnoreNullListItems)
			{
				if (!isFirst)
				{
					context.Builder.Append(',');
				}
				if (context.Settings.Formatting) {
					context.Builder.Append (JaysonConstants.Indentation[context.ObjectDepth]);
				}

				context.Builder.Append("null");
				return false; // isFirst
			}

			return isFirst;
		}

		private static IEnumerable<JaysonKeyValue<string, object>> GetDictionaryEntries(IDictionary source, 
			JaysonSerializationContext context)
		{
			string key;
			object value;
			object keyObj;
			bool ignoreNullValues = context.Settings.IgnoreNullValues;

			Func<string, object, object> filter = context.Filter;
			bool canFilter = (filter != null);

			foreach (DictionaryEntry dEntry in source)
			{
				value = dEntry.Value;

				keyObj = dEntry.Key;
				key = (keyObj is string) ? (string)keyObj : keyObj.ToString();

				if ((value != null) && canFilter)
				{
					value = filter(key, value);
				}

				if ((value != null) || !ignoreNullValues)
				{
					yield return new JaysonKeyValue<string, object> { Key = key, Value = value };
				}
			}
		}

		private static bool ValidObjectDepth(JaysonSerializationContext context)
		{
			if (context.Settings.MaxObjectDepth > 0 && 
				context.ObjectDepth > context.Settings.MaxObjectDepth) {
				if (context.Settings.RaiseErrorOnMaxObjectDepth) {
					throw new JaysonException(String.Format("Maximum object depth {0} exceeded.", 
						context.Settings.MaxObjectDepth));
				}
				return false;
			}
			return true;
		}

		private static void WriteDictionary(IDictionary obj, JaysonSerializationContext context)
		{
			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			try {
				bool isFirst = true;
				if (WriteObjectType (obj, context)) {
					isFirst = false;
				} else {
					builder.Append ('{');
				}

				if (!ValidObjectDepth(context)) {
					return;
				}

				if (obj.Count > 0) {
					string key;
					object value;
					object keyObj;

					Func<string, object, object> filter = context.Filter;
					bool canFilter = (filter != null);

					if (context.Settings.OrderNames) {
						foreach (var kvp in GetDictionaryEntries(obj, context).OrderBy(kvp => kvp.Key)) {
							key = kvp.Key;
							value = kvp.Value;

							if ((value != null) && canFilter) {
								value = filter (key, value);
							}

							isFirst = WriteKeyValueEntry (kvp.Key, kvp.Value, context, isFirst);
						}
					} else {
						foreach (DictionaryEntry dEntry in obj) {
							keyObj = dEntry.Key;
							value = dEntry.Value;

							key = (keyObj is string) ? (string)keyObj : keyObj.ToString ();

							if ((value != null) && canFilter) {
								value = filter (key, value);
							}

							isFirst = WriteKeyValueEntry (key, value, context, isFirst);
						}
					}
				}
			} finally {
				context.ObjectDepth--;
				if (context.Settings.Formatting) {
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				} 
				builder.Append ('}');
			}
		}

		private static IEnumerable<JaysonKeyValue<string, object>> GetStringDictionaryEntries(StringDictionary source,
			JaysonSerializationContext context)
		{
			string key;
			object value;
			object keyObj;
			bool ignoreNullValues = context.Settings.IgnoreNullValues;

			Func<string, object, object> filter = context.Filter;
			bool canFilter = (filter != null);

			foreach (DictionaryEntry dEntry in source)
			{
				value = dEntry.Value;

				keyObj = dEntry.Key;
				key = (keyObj is string) ? (string)keyObj : keyObj.ToString();

				if ((value != null) && canFilter)
				{
					value = filter(key, value);
				}

				if ((value != null) || !ignoreNullValues)
				{
					yield return new JaysonKeyValue<string, object> { Key = key, Value = value };
				}
			}
		}

		private static void WriteStringDictionary(StringDictionary obj, JaysonSerializationContext context)
		{
			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			try {
				bool isFirst = true;
				if (WriteObjectType (obj, context)) {
					isFirst = false;
				} else {
					builder.Append ('{');
				}

				if (!ValidObjectDepth(context)) {
					return;
				}

				if (obj.Count > 0) {
					string key;
					object value;
					object keyObj;

					Func<string, object, object> filter = context.Filter;
					bool canFilter = (filter != null);

					if (context.Settings.OrderNames) {
						foreach (var kvp in GetStringDictionaryEntries(obj, context).OrderBy(kvp => kvp.Key)) {
							key = kvp.Key;
							value = kvp.Value;

							if ((value != null) && canFilter) {
								value = filter (key, value);
							}

							isFirst = WriteKeyValueEntry (kvp.Key, kvp.Value, context, isFirst);
						}
					} else {
						foreach (DictionaryEntry dEntry in obj) {
							keyObj = dEntry.Key;
							value = dEntry.Value;

							key = (keyObj is string) ? (string)keyObj : keyObj.ToString ();

							if ((value != null) && canFilter) {
								value = filter (key, value);
							}

							isFirst = WriteKeyValueEntry (key, value, context, isFirst);
						}
					}
				}
			} finally {
				context.ObjectDepth--;
				if (context.Settings.Formatting) {
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				} 
				builder.Append ('}');
			}
		}

		private static void WriteNameValueCollection(NameValueCollection obj, JaysonSerializationContext context)
		{
			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			try {
				bool isFirst = true;
				if (WriteObjectType (obj, context)) {
					isFirst = false;
				} else {
					builder.Append ('{');
				}

				if (!ValidObjectDepth(context)) {
					return;
				}

				if (obj.Count > 0) {
					string[] keys = obj.AllKeys;
					if (context.Settings.OrderNames) {
						keys = keys.OrderBy (_key => _key).ToArray ();
					}

					string key;
					object value;

					Func<string, object, object> filter = context.Filter;
					bool canFilter = (filter != null);

					int len = keys.Length;
					for (int i = 0; i < len; i++) {
						key = keys [i];
						value = obj [key];

						if ((value != null) && canFilter) {
							value = filter (key, value);
						}

						isFirst = WriteKeyValueEntry (key, value, context, isFirst);
					}
				}
			} finally {
				context.ObjectDepth--;
				if (context.Settings.Formatting) {
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				} 
				builder.Append ('}');
			}
		}

		#if !(NET3500 || NET3000 || NET2000)
		private static void WriteDynamicObject(DynamicObject obj, JaysonSerializationContext context)
		{
			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			try {
				bool isFirst = true;
				Type objType = obj.GetType ();
				if (WriteObjectType (objType, context)) {
					isFirst = false;
				} else {
					builder.Append ('{');
				}

				if (!ValidObjectDepth(context)) {
					return;
				}

				object value;

				Func<string, object, object> filter = context.Filter;
				bool canFilter = (filter != null);

				var fastDict = JaysonFastMemberCache.GetMembers(objType);
				if (fastDict.Count > 0)
				{
					IEnumerable<KeyValuePair<string, IJaysonFastMember>> members = fastDict;
					if (context.Settings.OrderNames)
					{
						members = members.OrderBy(kvp => kvp.Key);
					}

					string fKey;

					foreach (var memberKvp in members)
					{
						fKey = memberKvp.Key;
						value = memberKvp.Value.Get(obj);

						if ((value != null) && canFilter)
						{
							value = filter(fKey, value);
						}

						isFirst = WriteKeyValueEntry(memberKvp.Key, value, context, isFirst, true);
					}
				}

				IEnumerable<string> memberNames = obj.GetDynamicMemberNames();
				if (context.Settings.OrderNames)
				{
					memberNames = memberNames.OrderBy(name => name);
				}

				JaysonDynamicWrapper dObj = new JaysonDynamicWrapper((IDynamicMetaObjectProvider)obj);

				foreach (var dKey in memberNames)
				{
					value = dObj[dKey];

					if ((value != null) && canFilter)
					{
						value = filter(dKey, value);
					}

					isFirst = WriteKeyValueEntry(dKey, value, context, isFirst, true);
				}
			}
			finally
			{
				context.ObjectDepth--;
				if (context.Settings.Formatting) {
					builder.Append (JaysonConstants.Indentation[context.ObjectDepth]);
				} 
				builder.Append('}');
			}
		}
		#endif

		private static void WriteClassOrStruct(object obj, Type objType, JaysonSerializationContext context)
		{
			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			try {
				bool isFirst = true;
				if (WriteObjectType (objType, context)) {
					isFirst = false;
				} else {
					builder.Append ('{');
				}

				if (!ValidObjectDepth(context)) {
					return;
				}

				var fastDict = JaysonFastMemberCache.GetMembers (objType);
				if (fastDict.Count > 0) {
					string key;
					object value;

					Func<string, object, object> filter = context.Filter;
					bool canFilter = (filter != null);

					IEnumerable<KeyValuePair<string, IJaysonFastMember>> members = fastDict;
					if (context.Settings.OrderNames) {
						members = members.OrderBy (kvp => kvp.Key);
					}

					foreach (var memberKvp in members) {
						key = memberKvp.Key;
						value = memberKvp.Value.Get (obj);

						if ((value != null) && canFilter) {
							value = filter (key, value);
						}

						isFirst = WriteKeyValueEntry (memberKvp.Key, value, context, isFirst, true);
					}
				}
			} finally {
				context.ObjectDepth--;
				if (context.Settings.Formatting) {
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				} 
				builder.Append ('}');
			}
		}

		private static void WriteGenericStringDictionary(IDictionary<string, object> obj, JaysonSerializationContext context)
		{
			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			try {
				bool isFirst = true;
				if (WriteObjectType (obj, context)) {
					isFirst = false;
				} else {
					builder.Append ('{');
				}

				if (!ValidObjectDepth(context)) {
					return;
				}

				if (obj.Count > 0) {
					string key;
					object value;
					#if !(NET3500 || NET3000 || NET2000)
					bool isExpando = obj is ExpandoObject;
					#else
					bool isExpando = false;
					#endif

					Func<string, object, object> filter = context.Filter;
					bool canFilter = (filter != null);

					IEnumerable<KeyValuePair<string, object>> eDict = obj;
					if (context.Settings.OrderNames) {
						eDict = eDict.OrderBy (kvp => kvp.Key);
					}

					foreach (var kvp in obj) {
						key = kvp.Key;
						value = kvp.Value;

						if ((value != null) && canFilter) {
							value = filter (key, value);
						}

						isFirst = WriteKeyValueEntry (key, value, context, isFirst, isExpando);
					}
				}
			} finally {
				context.ObjectDepth--;
				if (context.Settings.Formatting) {
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				} 
				builder.Append ('}');
			}
		}

		private static void WriteByteArray (byte[] obj, JaysonSerializationContext context)
		{
			context.ObjectDepth++;
			bool typeWritten = WriteByteArrayType (context);

			StringBuilder builder = context.Builder;
			bool formatting = context.Settings.Formatting;
			try {
				if (typeWritten) {
					context.ObjectDepth++;
					if (formatting) {
						builder.Append (JaysonConstants.Indentation[context.ObjectDepth]);
					}
				}

				if (!ValidObjectDepth(context)) {
					builder.Append("\"\"");
					return;
				}

				builder.Append ('"');
				builder.Append (Convert.ToBase64String (obj, 0, obj.Length, Base64FormattingOptions.None));
				builder.Append ('"');
			}
			finally {
				context.ObjectDepth--;
				if (typeWritten) {
					context.ObjectDepth--;
					if (formatting) {
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					}
					builder.Append ('}');
				}
			}
		}

		static void WriteDBNullArray (JaysonSerializationContext context, int length)
		{
			context.ObjectDepth++;
			bool typeWritten = WriteListType (typeof(DBNull[]), context);

			StringBuilder builder = context.Builder;
			bool formatting = context.Settings.Formatting;
			try {
				if (!typeWritten) {
					builder.Append ('[');
				} else {
					context.ObjectDepth++;
				}

				if (context.Settings.IgnoreNullListItems) {
					length = 0;
					return;
				} 

				if (!ValidObjectDepth(context)) {
					return;
				}

				int objectDepth = context.ObjectDepth;

				for (int i = 0; i < length; i++) {
					if (i == 0) {
						if (formatting) {
							builder.Append (JaysonConstants.Indentation[objectDepth]);
						}
						builder.Append ("null");
					} else if (formatting) {
						builder.Append (',');
						builder.Append (JaysonConstants.Indentation[objectDepth]);
						builder.Append ("null");
					} else {
						builder.Append (",null");
					}
				}
			} finally {
				context.ObjectDepth--;
				if (!formatting) {
					builder.Append (']');

					if (typeWritten) {
						context.ObjectDepth--;
						builder.Append ('}');
					}			
				} else {
					if (length > 0) {
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					}
					builder.Append (']');

					if (typeWritten) {
						context.ObjectDepth--;
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
						builder.Append ('}');
					}
				}
			}
		}

		private static void WritePrimitiveArray (Array obj, Type arrayType, JaysonSerializationContext context)
		{
			context.ObjectDepth++;
			bool typeWritten = WriteListType (obj, context);

			StringBuilder builder = context.Builder;
			bool formatting = context.Settings.Formatting;
			try {
				if (!typeWritten) {
					builder.Append ('[');
				} else {
					context.ObjectDepth++;
				}

				if (!ValidObjectDepth(context)) {
					return;
				}

				int objectDepth = context.ObjectDepth;
				JaysonFormatter formatter = context.Formatter;
				bool ignoreNullValues = context.Settings.IgnoreNullValues;

				object item;
				bool isFirst = true;
				var objLen = obj.Length;

				for (int i = 0; i < objLen; i++) {
					item = obj.GetValue (i);

					if (item == null || item == DBNull.Value) {
						if (!ignoreNullValues) {
							if (isFirst) {
								isFirst = false;
								if (formatting) {
									builder.Append (JaysonConstants.Indentation[objectDepth]);
								}
								builder.Append ("null");
							} else {
								if (formatting) {
									builder.Append (',');
									builder.Append (JaysonConstants.Indentation[objectDepth]);
									builder.Append ("null");
								} else {
									builder.Append (",null");
								}
							}
						}
						continue;
					}

					if (!isFirst) {
						builder.Append (',');
					}
					if (formatting) {
						builder.Append (JaysonConstants.Indentation[objectDepth]);
					}

					isFirst = false;
					formatter.Format (item, arrayType, builder);
				}
			}
			finally {
				context.ObjectDepth--;

				if (!context.Settings.Formatting) {
					builder.Append (']');

					if (typeWritten) {
						context.ObjectDepth--;
						builder.Append ('}');
					}			
				} else {
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					builder.Append (']');

					if (typeWritten) {
						context.ObjectDepth--;
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
						builder.Append ('}');
					}
				}
			}
		}

		private static void WriteArray(Array obj, Type objType, JaysonSerializationContext context)
		{
			int length = obj.Length;
			if (length == 0) {
				if (!WriteListType (obj, context)) {
					context.Builder.Append ('[');
				}
				context.Builder.Append (']');
				return;
			}

            Type arrayType = JaysonTypeInfo.GetElementType(objType);

			if (arrayType == typeof(byte) && length > 1) {
				WriteByteArray ((byte[])obj, context);
				return;
			}

			var info = JaysonTypeInfo.GetTypeInfo(arrayType);

			if (info.JPrimitive || info.Enum) {
				WritePrimitiveArray (obj, arrayType, context);
				return;
			}

			if (arrayType == typeof(DBNull)) {
				WriteDBNullArray (context, length);
				return;
			}

			WriteIList (obj, context);
		}

		private static void WriteIList(IList obj, JaysonSerializationContext context)
		{
			context.ObjectDepth++;
			bool typeWritten = WriteListType (obj, context);

			int count = obj.Count;
			if (count == 0) {
				if (!typeWritten) {
					context.Builder.Append ('[');
				}
				context.Builder.Append (']');
				return;
			}

			StringBuilder builder = context.Builder;
			try {
				if (!typeWritten) {
					builder.Append ('[');
				} else {
					context.ObjectDepth++;
				}

				if (!ValidObjectDepth(context)) {
					return;
				}

				bool isFirst = true;
				for (int i = 0; i < count; i++) {
					isFirst = WriteEnumerableValue (obj [i], context, isFirst);
				}
			} finally {
				context.ObjectDepth--;

				if (!context.Settings.Formatting) {
					builder.Append (']');

					if (typeWritten) {
						context.ObjectDepth--;
						builder.Append ('}');
					}			
				} else {
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					builder.Append (']');

					if (typeWritten) {
						context.ObjectDepth--;
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
						builder.Append ('}');
					}
				}
			}
		}

		private static IEnumerable<JaysonKeyValue<string, object>> GetEnumDictionaryEntries(IEnumerable source, 
			JaysonSerializationContext context)
		{
			string key;
			object value;
			object keyObj;
			bool ignoreNullValues = context.Settings.IgnoreNullValues;

			Func<string, object, object> filter = context.Filter;
			bool canFilter = (filter != null);

			foreach (DictionaryEntry dEntry in source)
			{
				value = dEntry.Value;

				keyObj = dEntry.Key;
				key = (keyObj is string) ? (string)keyObj : keyObj.ToString();

				if ((value != null) && canFilter)
				{
					value = filter(key, value);
				}

				if ((value != null) || !ignoreNullValues)
				{
					yield return new JaysonKeyValue<string, object> { Key = key, Value = value };
				}
			}
		}

		private static void WriteCountedEnumerable(IEnumerable obj, JaysonSerializationContext context, bool isEmpty)
		{
			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			bool typeWritten = WriteListType (obj, context);
			if (!typeWritten) {
				builder.Append ('[');
			}

			if (!isEmpty) {
				try {
					if (!ValidObjectDepth(context)) {
						return;
					}

					bool isFirst = true;

					Type entryType;
					JaysonDictionaryType dType = JaysonCommon.GetDictionaryType (obj, out entryType);

					IEnumerator enumerator = obj.GetEnumerator ();
					try {
						if (dType == JaysonDictionaryType.IDictionary) {
							string key;
							object value;
							object keyObj;
							DictionaryEntry dEntry;

							Func<string, object, object> filter = context.Filter;
							bool canFilter = (filter != null);

							if (context.Settings.OrderNames) {
								foreach (var kvp in GetEnumDictionaryEntries(obj, context).OrderBy(kvp => kvp.Key)) {
									key = kvp.Key;
									value = kvp.Value;

									if ((value != null) && canFilter) {
										value = filter (key, value);
									}

									isFirst = WriteKeyValueEntry (kvp.Key, kvp.Value, context, isFirst);
								}
							} else {
								while (enumerator.MoveNext ()) {
									dEntry = (DictionaryEntry)enumerator.Current;

									keyObj = dEntry.Key;
									value = dEntry.Value;

									key = (keyObj is string) ? (string)keyObj : keyObj.ToString ();

									if ((value != null) && canFilter) {
										value = filter (key, value);
									}

									isFirst = WriteKeyValueEntry (key, value, context, isFirst);
								}
							}
						} else if (dType == JaysonDictionaryType.IGenericDictionary) {
							string key;
							object value;

							IJaysonFastMember keyFm;
							IJaysonFastMember valueFm;

							IDictionary<string, IJaysonFastMember> members = JaysonFastMemberCache.GetMembers (entryType);

							if (members.TryGetValue ("Key", out keyFm) && members.TryGetValue ("Value", out valueFm)) {
								object keyObj;

								Func<string, object, object> filter = context.Filter;
								bool canFilter = (filter != null);

								while (enumerator.MoveNext ()) {
									keyObj = keyFm.Get (enumerator.Current);
									if (keyObj != null) {
										key = (keyObj is string) ? (string)keyObj : keyObj.ToString ();
										value = valueFm.Get (enumerator.Current);

										if ((value != null) && canFilter) {
											value = filter (key, value);
										}

										isFirst = WriteKeyValueEntry (key, value, context, isFirst);
									}
								}
							}
						} else {
							context.ObjectDepth++;
							while (enumerator.MoveNext ()) {
								isFirst = WriteEnumerableValue (enumerator.Current, context, isFirst);
							}
							context.ObjectDepth--;
						}
					} finally {
						if (enumerator is IDisposable) {
							((IDisposable)enumerator).Dispose ();
						}
					}
				} finally {
					if (context.Settings.Formatting) {
						builder.Append (JaysonConstants.Indentation[context.ObjectDepth]);
					} 
					builder.Append (']');

					context.ObjectDepth--;
					if (context.Settings.Formatting) {
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					} 
					builder.Append ('}');
				}
			}
		}

		private static void WriteICollection(ICollection obj, JaysonSerializationContext context)
		{
			WriteCountedEnumerable(obj, context, obj.Count > 0);
		}

		private static void WriteEnumerable(IEnumerable obj, Type objType, JaysonSerializationContext context)
		{
			if (obj is Array)
			{
				WriteArray((Array)obj, objType, context);
			}
			else if (obj is IList)
			{
				WriteIList((IList)obj, context);
			}
			else if (obj is ICollection)
			{
				WriteICollection((ICollection)obj, context);
			}
			else
			{
				WriteCountedEnumerable(obj, context, false);
			}
		}

		private static bool CanWriteJsonObject(object obj, Type objType, JaysonSerializationContext context)
		{
			if (obj == null)
			{
				return !context.Settings.IgnoreNullValues;
			}

			if (JaysonTypeInfo.IsJPrimitive(objType ?? obj.GetType()))
			{
				return true;
			}

			if (context.Stack.Contains(obj) || (obj == DBNull.Value) || (obj is DataSet || obj is DataTable))
			{
				return !context.Settings.IgnoreNullValues;
			}

			return true;
		}

		private static void WriteJsonObject(object obj, Type objType, JaysonSerializationContext context)
		{
			if (obj == null)
			{
				if (!context.Settings.IgnoreNullValues)
				{
					context.Builder.Append("null");
				}
				return;
			}

			if (JaysonTypeInfo.IsJPrimitive(objType))
			{
				if (objType != typeof(string) &&
					objType != typeof(bool) &&
					context.Settings.TypeNames != JaysonTypeNameSerialization.None) {
					StringBuilder builder = context.Builder;

					context.ObjectDepth++;
					WritePrimitiveTypeValue (objType, context);
					context.Formatter.Format (obj, objType, builder);

					context.ObjectDepth--;
					if (context.Settings.Formatting) {
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					}
					builder.Append ('}');
					return;
				}

				context.Formatter.Format(obj, objType, context.Builder);
				return;
			}

			if (obj == DBNull.Value)
			{
				if (!context.Settings.IgnoreNullValues)
				{
					context.Builder.Append("null");
				}
				return;
			}

			var info = JaysonTypeInfo.GetTypeInfo(objType);

			JaysonStackList stack = null;
			if (info.Class)
			{
				stack = context.Stack;
				if (stack.Contains(obj) || (obj is DataSet || obj is DataTable))
				{
					if (context.Settings.RaiseErrorOnCircularRef) 
					{
						throw new JaysonException ("Circular reference on " + objType.Name);
					}
					if (!context.Settings.IgnoreNullValues)
					{
						context.Builder.Append("null");
					}
					return;
				}

				stack.Push(obj);
			}
			else if (info.Enum)
			{
				StringBuilder builder = context.Builder;

				builder.Append('"');
				builder.Append(context.Settings.UseEnumNames ? JaysonEnumCache.GetName((Enum)obj) : JaysonEnumCache.AsIntString((Enum)obj));
				builder.Append('"');
				return;
			}

			try
			{
				JaysonSerializationSettings settings = context.Settings;
				#if !(NET3500 || NET3000 || NET2000)
				if (settings.DisableExpandoObjects && (objType == typeof(ExpandoObject)))
				{
					if (!settings.IgnoreNullValues) {
						context.Builder.Append("null");
					} else {
						context.Builder.Append('{');
						context.Builder.Append('}');
					}
					return;
				}
				#endif

                if (settings.DisableAnonymousTypes && JaysonTypeInfo.IsAnonymous(objType))
				{
					if (!settings.IgnoreNullValues) {
						context.Builder.Append("null");
					} else {
						context.Builder.Append('{');
						context.Builder.Append('}');
					}
					return;
				}

				if (obj is IDictionary)
				{
					WriteDictionary((IDictionary)obj, context);
				}
				else if (obj is StringDictionary)
				{
					WriteStringDictionary((StringDictionary)obj, context);
				}
				else if (obj is NameValueCollection)
				{
					WriteNameValueCollection((NameValueCollection)obj, context);
				}
				else if (obj is IDictionary<string, object>)
				{
					WriteGenericStringDictionary((IDictionary<string, object>)obj, context);
				}
				#if !(NET3500 || NET3000 || NET2000)
				else if (obj is DynamicObject)
				{
					if (!settings.DisableDynamicObjects) {
						WriteDynamicObject((DynamicObject)obj, context);
					} else if (!settings.IgnoreNullValues) {
						context.Builder.Append("null");
					} else {
						context.Builder.Append('{');
						context.Builder.Append('}');
					}
				}
				#endif
				else if (obj is IEnumerable)
				{
					WriteEnumerable((IEnumerable)obj, objType, context);
				}
				else
				{
					WriteClassOrStruct(obj, objType, context);
				}
			}
			finally
			{
				if (info.Class)
				{
					stack.Pop();
				}
			}
		}

		public static string ToJsonString(object obj, JaysonSerializationSettings settings = null,
			Func<string, object, object> filter = null)
		{
			if (obj == null)
			{
				return null;
			}

			settings = settings ?? JaysonSerializationSettings.Default;

			JaysonFormatter formatter = new JaysonFormatter(
				numberFormat: settings.NumberFormat,
				timeSpanFormat: settings.TimeSpanFormat,
				dateFormatType: settings.DateFormatType,
				dateTimeFormat: settings.DateTimeFormat,
				dateTimeZoneType: settings.DateTimeZoneType,
				useEnumNames: settings.UseEnumNames,
				escapeChars: settings.EscapeChars,
				escapeUnicodeChars: settings.EscapeUnicodeChars,
				convertDecimalToDouble: settings.ConvertDecimalToDouble
			);

			Type objType = obj.GetType();
			if (JaysonTypeInfo.IsJPrimitive(objType))
			{
				if (settings.TypeNames == JaysonTypeNameSerialization.None) {
					return formatter.Format(obj, objType);
				}

				JaysonStackList primmeStack = JaysonStackList.Get();
				try 
				{
					StringBuilder builder = new StringBuilder(100, int.MaxValue);

					var context = new JaysonSerializationContext (filter: filter,
						settings: settings,
						stack: primmeStack,
						builder: builder,
						formatter: formatter
					);

					context.ObjectDepth++;
					WritePrimitiveTypeValue (objType, context);
					formatter.Format (obj, objType, builder);

					context.ObjectDepth--;
					if (settings.Formatting) {
						builder.Append (JaysonConstants.Indentation [0]);
					}
					builder.Append ('}');

					return builder.ToString();
				}
				finally
				{
					JaysonStackList.Release(primmeStack);
				}
			}

			JaysonStackList stack = JaysonStackList.Get();
			try
			{
				StringBuilder builder = new StringBuilder(2048, int.MaxValue);

				WriteJsonObject(obj, objType,
					new JaysonSerializationContext(filter: filter,
						settings: settings,
						stack: stack,
						builder: builder,
						formatter: formatter
					)
				);
				return builder.ToString();
			}
			finally
			{
				JaysonStackList.Release(stack);
			}
		}

		# endregion ToJsonString
	}

	# endregion JsonConverter
}
