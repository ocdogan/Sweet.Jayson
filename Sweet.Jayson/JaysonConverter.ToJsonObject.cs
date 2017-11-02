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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
#if !(NET3500 || NET3000 || NET2000)
using System.Dynamic;
#endif
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Sweet.Jayson
{
    # region JsonConverter

    public static partial class JaysonConverter
    {
        # region ToJsonObject

        private static Dictionary<string, object> AsObjectRefence(int id, JaysonSerializationContext context)
        {
            return new Dictionary<string, object> { 
				{ "$ref", id } 
			};
        }

        private static Dictionary<string, object> AsObjectRefence(object obj, JaysonSerializationContext context)
        {
            return new Dictionary<string, object> { 
				{ "$ref", context.ReferenceMap.GetObjectId(obj) } 
			};
        }

        private static Dictionary<string, object> AsDictionaryObjectKey(IDictionary obj, JaysonSerializationContext context)
        {
            if (obj.Count == 0)
            {
                if (context.Settings.UseObjectReferencing)
                {
                    return new Dictionary<string, object> { 
						{ "$id", context.ReferenceMap.GetObjectId (obj) }
					};
                }
                return new Dictionary<string, object>();
            }

            var kvList = new List<object>(obj.Count);

            var result = new Dictionary<string, object>();

            if (context.Settings.UseObjectReferencing)
            {
                result.Add("$id", context.ReferenceMap.GetObjectId(obj));
            }

            result["$kv"] = kvList;

            string key;
            object value;
            object keyObj;

            var filter = context.Filter;
            var canFilter = (filter != null);

            foreach (DictionaryEntry dEntry in obj)
            {
                keyObj = dEntry.Key;
                value = dEntry.Value;

                if (value != null)
                {
                    if (canFilter)
                    {
                        key = (keyObj is string) ? (string)keyObj : keyObj.ToString();
                        value = filter(key, value);
                    }

                    if (value != null)
                    {
                        value = ToJsonObject(value, context);
                    }
                }

                kvList.Add(new Dictionary<string, object> { 
                    { "$k", ToJsonObject(keyObj, context) },
                    { "$v", ToJsonObject(value, context) } 
                });
            }

            return result;
        }

        private static Dictionary<string, object> AsDictionary(IDictionary obj, JaysonSerializationContext context)
        {
            var useStringKey = !context.Settings.UseKVModelForJsonObjects;
            if (!useStringKey)
            {
                var genericArgs = JaysonTypeInfo.GetGenericArguments(obj.GetType());
                useStringKey = (genericArgs != null) && (genericArgs.Length > 0) && (genericArgs[0] == typeof(string));
            }

            if (useStringKey)
            {
                if (obj.Count == 0)
                {
                    if (context.Settings.UseObjectReferencing)
                    {
                        return new Dictionary<string, object> { 
							{ "$id", context.ReferenceMap.GetObjectId (obj) }
						};
                    }
                    return new Dictionary<string, object>();
                }

                var result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

                if (context.Settings.UseObjectReferencing)
                {
                    result.Add("$id", context.ReferenceMap.GetObjectId(obj));
                }

                string key;
                object value;
                object keyObj;

                var filter = context.Filter;
                var canFilter = (filter != null);

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

                    result[key] = value;
                }

                return result;
            }

            return AsDictionaryObjectKey(obj, context);
        }

        private static Dictionary<string, object> AsStringDictionary(StringDictionary obj, JaysonSerializationContext context)
        {
            if ((obj == null) && (obj.Count == 0))
            {
                if (context.Settings.UseObjectReferencing)
                {
                    return new Dictionary<string, object> { 
						{ "$id", context.ReferenceMap.GetObjectId (obj) }
					};
                }
                return new Dictionary<string, object>();
            }

            var result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

            if (context.Settings.UseObjectReferencing)
            {
                result.Add("$id", context.ReferenceMap.GetObjectId(obj));
            }

            string key;
            object value;

            var filter = context.Filter;
            var canFilter = (filter != null);

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

                result.Add(key, value);
            }

            return result;
        }

        private static Dictionary<string, object> AsNameValueCollection(NameValueCollection obj, JaysonSerializationContext context)
        {
            if (obj.Count == 0)
            {
                if (context.Settings.UseObjectReferencing)
                {
                    return new Dictionary<string, object> { 
						{ "$id", context.ReferenceMap.GetObjectId (obj) }
					};
                }
                return new Dictionary<string, object>();
            }

            var result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

            if (context.Settings.UseObjectReferencing)
            {
                result.Add("$id", context.ReferenceMap.GetObjectId(obj));
            }

            object value;

            var filter = context.Filter;
            var canFilter = (filter != null);

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

                result.Add(key, value);
            }

            return result;
        }

#if !(NET3500 || NET3000 || NET2000)
        private static Dictionary<string, object> AsDynamicObject(DynamicObject obj, Type objType, JaysonSerializationContext context)
        {
            var settings = context.Settings;
            var typeOverride = settings.GetTypeOverride(objType);

            var result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

            if (settings.UseObjectReferencing)
            {
                result.Add("$id", context.ReferenceMap.GetObjectId(obj));
            }

            string key;
            object value;
            string aliasKey;

            var filter = context.Filter;
            var canFilter = (filter != null);

            var members = JaysonFastMemberCache.GetMembers(obj.GetType());
            if ((members != null) && (members.Length > 0))
            {
                foreach (var member in members)
                {
                    if ((!settings.IgnoreReadOnlyMembers || member.CanWrite) &&
                        !(member.AnonymousField || (settings.IgnoreBackingFields && member.BackingField) ||
                         (settings.IgnoreFields && (member.Type == JaysonFastMemberType.Field)) ||
                         (settings.IgnoreNonPublicFields && !member.IsPublic && (member.Type == JaysonFastMemberType.Field)) ||
                         (settings.IgnoreNonPublicProperties && !member.IsPublic && (member.Type == JaysonFastMemberType.Property))))
                    {
                        key = member.Name;

                        if (!settings.IsMemberIgnored(objType, key) && 
                            (typeOverride == null || !typeOverride.IsMemberIgnored(key)))
                        {
                            value = member.Get(obj);

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

                            if ((value != null) || !settings.IgnoreNullValues)
                            {
                                if (settings.IgnoreEmptyCollections)
                                {
                                    var collection = value as ICollection;
                                    if ((collection != null) && (collection.Count == 0))
                                    {
                                        continue;
                                    }
                                }

                                if (typeOverride != null)
                                {
                                    aliasKey = typeOverride.GetMemberAlias(key);
                                    if (!String.IsNullOrEmpty(aliasKey))
                                    {
                                        key = aliasKey;
                                    }
                                }

                                result.Add(key, value);
                            }
                        }
                    }
                }
            }

            var dObj = new JaysonDynamicWrapper((IDynamicMetaObjectProvider)obj);

            foreach (var dKey in obj.GetDynamicMemberNames())
            {
                key = dKey;
                if (!settings.IsMemberIgnored(objType, key) && 
                    (typeOverride == null || !typeOverride.IsMemberIgnored(key)))
                {
                    value = dObj[key];

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

                    if ((value != null) || !settings.IgnoreNullValues)
                    {
                        if (settings.IgnoreEmptyCollections)
                        {
                            var collection = value as ICollection;
                            if ((collection != null) && (collection.Count == 0))
                            {
                                continue;
                            }
                        }

                        if (typeOverride != null)
                        {
                            aliasKey = typeOverride.GetMemberAlias(key);
                            if (!String.IsNullOrEmpty(aliasKey))
                            {
                                key = aliasKey;
                            }
                        }

                        result.Add(key, value);
                    }
                }
            }

            return result;
        }
#endif

        private static Dictionary<string, object> AsObject(object obj, Type objType, JaysonSerializationContext context)
        {
            var result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);
            var settings = context.Settings;

            if (settings.UseObjectReferencing)
            {
                result.Add("$id", context.ReferenceMap.GetObjectId(obj));
            }

            var members = JaysonFastMemberCache.GetMembers(objType);
            if ((members != null) && (members.Length > 0))
            {
                string key;
                object value;
                string aliasKey;

                object defaultValue;

                var typeOverride = settings.GetTypeOverride(objType);

                var filter = context.Filter;
                var canFilter = (filter != null);

                foreach (var member in members)
                {
                    if ((!settings.IgnoreReadOnlyMembers || member.CanWrite) &&
                        !(member.AnonymousField || (settings.IgnoreBackingFields && member.BackingField) ||
                         (settings.IgnoreFields && (member.Type == JaysonFastMemberType.Field)) ||
                         (settings.IgnoreNonPublicFields && !member.IsPublic && (member.Type == JaysonFastMemberType.Field)) ||
                         (settings.IgnoreNonPublicProperties && !member.IsPublic && (member.Type == JaysonFastMemberType.Property))))
                    {
                        key = member.Name;

                        if (!settings.IsMemberIgnored(objType, key) && 
                            (typeOverride == null || !typeOverride.IsMemberIgnored(key)))
                        {
                            value = member.Get(obj);

                            if (settings.IgnoreDefaultValues)
                            {
                                defaultValue = null;
                                if (typeOverride == null || !typeOverride.TryGetDefaultValue(key, out defaultValue))
                                {
                                    defaultValue = member.DefaultValue;
                                }

                                if (((defaultValue == null) && (value == null)) ||
                                    ((defaultValue != null) && (defaultValue.Equals(value) ||
                                     ((defaultValue is IComparable) && ((IComparable)defaultValue).CompareTo(value) == 0))))
                                    continue;
                            }

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

                            if ((value != null) || !settings.IgnoreNullValues)
                            {
                                if (settings.IgnoreEmptyCollections)
                                {
                                    var collection = value as ICollection;
                                    if ((collection != null) && (collection.Count == 0))
                                    {
                                        continue;
                                    }
                                }

                                if (typeOverride != null)
                                {
                                    aliasKey = typeOverride.GetMemberAlias(key);
                                    if (!String.IsNullOrEmpty(aliasKey))
                                    {
                                        key = aliasKey;
                                    }
                                }

                                result.Add(key, value);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static Dictionary<string, object> AsType(Type obj, Type objType, JaysonSerializationContext context)
        {
            var result = new Dictionary<string, object>();

            if (context.Settings.UseObjectReferencing)
            {
                result.Add("$id", context.ReferenceMap.GetObjectId(obj));
            }

            result.Add("QualifiedName", JaysonTypeInfo.GetTypeName(obj, JaysonTypeNameInfo.TypeNameWithAssembly));

            return result;
        }

        private static Dictionary<string, object> AsMemberInfo(MemberInfo obj, Type objType, JaysonSerializationContext context)
        {
            var result = new Dictionary<string, object>();

            if (context.Settings.UseObjectReferencing)
            {
                result.Add("$id", context.ReferenceMap.GetObjectId(obj));
            }

            result.Add("QualifiedName", JaysonTypeInfo.GetTypeName(obj.DeclaringType, JaysonTypeNameInfo.TypeNameWithAssembly));

            if (obj.ReflectedType != null && obj.ReflectedType != obj.DeclaringType)
            {
                result.Add("ReflectedType", JaysonTypeInfo.GetTypeName(obj.ReflectedType, JaysonTypeNameInfo.TypeNameWithAssembly));
            }
            
            result.Add("MemberName", obj.Name);

            return result;
        }

        private static Dictionary<string, object> AsParameterInfo(ParameterInfo obj, Type objType, JaysonSerializationContext context)
        {
            var result = new Dictionary<string, object>();

            if (context.Settings.UseObjectReferencing)
            {
                result.Add("$id", context.ReferenceMap.GetObjectId(obj));
            }

            result.Add("QualifiedName", JaysonTypeInfo.GetTypeName(obj.ParameterType, JaysonTypeNameInfo.TypeNameWithAssembly));

            return result;
        }

        private static Dictionary<string, object> AsConstructorInfo(ConstructorInfo obj, Type objType, JaysonSerializationContext context)
        {
            var result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

            if (context.Settings.UseObjectReferencing)
            {
                result.Add("$id", context.ReferenceMap.GetObjectId(obj));
            }

            result.Add("QualifiedName", JaysonTypeInfo.GetTypeName(obj.DeclaringType, JaysonTypeNameInfo.TypeNameWithAssembly));

            if (obj.ReflectedType != null && obj.ReflectedType != obj.DeclaringType)
            {
                result.Add("ReflectedType", JaysonTypeInfo.GetTypeName(obj.ReflectedType, JaysonTypeNameInfo.TypeNameWithAssembly));
            }

            var parameters = obj.GetParameters();
            if (parameters != null && parameters.Length > 0)
            {
                Type[] parameterTypes = new Type[parameters.Length];
                for (int i = parameters.Length - 1; i > -1; i--)
                {
                    parameterTypes[i] = parameters[i].ParameterType;
                }

                result.Add("Params", AsObject(parameterTypes, typeof(Type[]), context));
            }

            return result;
        }

        private static Dictionary<string, object> AsMethodInfo(MethodInfo obj, Type objType, JaysonSerializationContext context)
        {
            var result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

            if (context.Settings.UseObjectReferencing)
            {
                result.Add("$id", context.ReferenceMap.GetObjectId(obj));
            }

            result.Add("QualifiedName", JaysonTypeInfo.GetTypeName(obj.DeclaringType, JaysonTypeNameInfo.TypeNameWithAssembly));

            if (obj.ReflectedType != null && obj.ReflectedType != obj.DeclaringType)
            {
                result.Add("ReflectedType", JaysonTypeInfo.GetTypeName(obj.ReflectedType, JaysonTypeNameInfo.TypeNameWithAssembly));
            }

            result.Add("MemberName", obj.Name);

            var parameters = obj.GetParameters();
            if (parameters != null && parameters.Length > 0)
            {
                Type[] parameterTypes = new Type[parameters.Length];
                for (int i = parameters.Length - 1; i > -1; i--)
                {
                    parameterTypes[i] = parameters[i].ParameterType;
                }

                result.Add("Params", AsObject(parameterTypes, typeof(Type[]), context));
            }

            return result;
        }

        private static object AsISerializable(ISerializable obj, Type objType, JaysonSerializationContext context)
        {
            var settings = context.Settings;
            if (settings.UseKVModelForISerializable)
            {
                return AsISerializableKVMode(obj, objType, context);
            }

            var result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

            if (settings.UseObjectReferencing)
            {
                result.Add("$id", context.ReferenceMap.GetObjectId(obj));
            }

            var info = new SerializationInfo(objType, JaysonCommon.FormatterConverter);
            obj.GetObjectData(info, context.StreamingContext);

            if (info.MemberCount > 0)
            {
                string key;
                object value;
                string aliasKey;

                var typeOverride = settings.GetTypeOverride(objType);

                var filter = context.Filter;
                var canFilter = (filter != null);

                foreach (SerializationEntry se in info)
                {
                    key = se.Name;
                    if (!settings.IsMemberIgnored(objType, key) && 
                        (typeOverride == null || !typeOverride.IsMemberIgnored(key)))
                    {
                        value = se.Value;

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

                        if ((value != null) || !settings.IgnoreNullValues)
                        {
                            if (typeOverride != null)
                            {
                                aliasKey = typeOverride.GetMemberAlias(key);
                                if (!String.IsNullOrEmpty(aliasKey))
                                {
                                    key = aliasKey;
                                }
                            }

                            result.Add(key, value);
                        }
                    }
                }
            }

            return result;
        }

        private static object AsISerializableKVMode(ISerializable obj, Type objType, JaysonSerializationContext context)
        {
            var result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

            var settings = context.Settings;

            if (settings.UseObjectReferencing)
            {
                result.Add("$id", context.ReferenceMap.GetObjectId(obj));
            }

            var info = new SerializationInfo(objType, JaysonCommon.FormatterConverter);
            obj.GetObjectData(info, context.StreamingContext);

            if (info.MemberCount > 0)
            {
                string key;
                object value;
                string aliasKey;

                var ctx = new List<object>(info.MemberCount);

                var typeOverride = settings.GetTypeOverride(objType);

                var filter = context.Filter;
                var canFilter = (filter != null);

                foreach (SerializationEntry se in info)
                {
                    key = se.Name;
                    if (!settings.IsMemberIgnored(objType, key) && 
                        (typeOverride == null || !typeOverride.IsMemberIgnored(key)))
                    {
                        value = se.Value;

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

                        if ((value != null) || !settings.IgnoreNullValues)
                        {
                            if (typeOverride != null)
                            {
                                aliasKey = typeOverride.GetMemberAlias(key);
                                if (!String.IsNullOrEmpty(aliasKey))
                                {
                                    key = aliasKey;
                                }
                            }

                            ctx.Add(new Dictionary<string, object>() { { "$k", key } });
                            ctx.Add(new Dictionary<string, object>() { { "$v", value } });
                        }
                    }
                }

                if (ctx.Count > 0)
                {
                    result.Add("$ctx", ctx);
                }
            }

            return result;
        }

        private static Dictionary<string, object> AsGenericStringDictionary(IDictionary<string, object> obj,
            JaysonSerializationContext context)
        {
            if (obj.Count == 0)
            {
                if (context.Settings.UseObjectReferencing)
                {
                    return new Dictionary<string, object> { 
						{ "$id", context.ReferenceMap.GetObjectId (obj) }
					};
                }
                return new Dictionary<string, object>();
            }

            var result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

            if (context.Settings.UseObjectReferencing)
            {
                result.Add("$id", context.ReferenceMap.GetObjectId(obj));
            }

            string key;
            object value;

            var filter = context.Filter;
            var canFilter = (filter != null);

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

                result.Add(key, value);
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

            var result = new object[count];
            var settings = context.Settings;

            object item;

            for (int i = 0; i < count; i++)
            {
                item = obj[i];
                if (item != null)
                {
                    item = ToJsonObject(item, context);
                }

                if ((item != null) || !settings.IgnoreNullValues)
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

            var result = new object[count];
            obj.CopyTo(result, 0);

            object item;
            var settings = context.Settings;

            for (int i = 0; i < count; i++)
            {
                item = result[i];
                if (item != null)
                {
                    item = ToJsonObject(item, context);
                }

                if ((item != null) || !settings.IgnoreNullValues)
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
                var settings = context.Settings;

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

                        var result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

                        if (context.Settings.UseObjectReferencing)
                        {
                            result.Add("$id", context.ReferenceMap.GetObjectId(obj));
                        }

                        var filter = context.Filter;
                        var canFilter = (filter != null);

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

                            if ((value != null) || !settings.IgnoreNullValues)
                            {
                                result[key] = value;
                            }
                        }

                        obj = result;
                    }
                    else if (dType == JaysonDictionaryType.IGenericDictionary)
                    {
                        var result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

                        if (context.Settings.UseObjectReferencing)
                        {
                            result.Add("$id", context.ReferenceMap.GetObjectId(obj));
                        }

                        var cache = JaysonFastMemberCache.GetCache(entryType);
                        if (cache != null)
                        {
                            var keyFm = cache.GetAnyMember("Key");
                            if (keyFm != null)
                            {
                                var valueFm = JaysonFastMemberCache.GetAnyMember(entryType, "Value");
                                if (valueFm != null)
                                {
                                    string key;
                                    object value;
                                    object keyObj;

                                    var filter = context.Filter;
                                    var canFilter = (filter != null);

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

                                            if ((value != null) || !settings.IgnoreNullValues)
                                            {
                                                if (settings.IgnoreEmptyCollections)
                                                {
                                                    var collection = value as ICollection;
                                                    if ((collection != null) && (collection.Count == 0))
                                                    {
                                                        continue;
                                                    }
                                                }

                                                result[key] = value;
                                            }
                                        }
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
                        var result = new List<object>();

                        while (enumerator.MoveNext())
                        {
                            item = enumerator.Current;
                            if (item != null)
                            {
                                item = ToJsonObject(item, context);
                            }

                            if ((item != null) || !settings.IgnoreNullValues)
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

        # region DataTable & DataSet

        private static List<string> AsColumnNames(DataColumn[] columns, JaysonSerializationContext context)
        {
            if (columns != null && columns.Length > 0)
            {
                List<string> result = new List<string>(columns.Length);
                foreach (var column in columns)
                {
                    if (column.ColumnName != null)
                    {
                        result.Add(column.ColumnName);
                    }
                }
                return result;
            }

            return null;
        }

        private static List<object> AsDataTableRows(DataTable dataTable, JaysonSerializationContext context)
        {
            var rows = dataTable.Rows;
            int rowCount = rows.Count;

            if (rowCount > 0)
            {
                var columns = dataTable.Columns;
                int columnCount = columns.Count;

                if (columnCount > 0)
                {
                    var result = new List<object>(rowCount);

                    DataColumn dataColumn;
                    List<JaysonTuple<DataColumn, JaysonTypeInfo>> columnsInfo = new List<JaysonTuple<DataColumn, JaysonTypeInfo>>();

                    for (int i = 0; i < columnCount; i++)
                    {
                        dataColumn = columns[i];
                        columnsInfo.Add(new JaysonTuple<DataColumn, JaysonTypeInfo>(dataColumn,
                            JaysonTypeInfo.GetTypeInfo(dataColumn.DataType)));
                    }

                    var filter = context.Filter;
                    var canFilter = (filter != null);

                    DataRow dataRow;
                    object cellValue;

                    List<object> cellList;
                    JaysonTuple<DataColumn, JaysonTypeInfo> columnInfo;

                    var tableName = dataTable.TableName;
                    if (dataTable.DataSet != null)
                    {
                        tableName += "@" + dataTable.DataSet.DataSetName + "::";
                    }

                    for (int i = 0; i < rowCount; i++)
                    {
                        dataRow = rows[i];

                        cellList = new List<object>(columnCount);
                        result.Add(cellList);

                        for (int j = 0; j < columnCount; j++)
                        {
                            columnInfo = columnsInfo[j];
                            cellValue = dataRow[columnInfo.Item1];

                            if (cellValue == null || cellValue == DBNull.Value)
                            {
                                cellList.Add(cellValue);
                            }
                            else
                            {
                                if (canFilter)
                                {
                                    cellValue = filter(tableName + columnInfo.Item1.ColumnName, cellValue);
                                }

                                if (cellValue != null)
                                {
                                    cellValue = ToJsonObject(cellValue, context);
                                }

                                cellList.Add(cellValue);
                            }
                        }
                    }
                    return result;
                }
            }
            return null;
        }

        private static Dictionary<string, object> AsDataTableColumn(DataColumn dataColumn, JaysonSerializationContext context)
        {
            var result = new Dictionary<string, object>(16);

            string defaultNamespace = dataColumn.Table.Namespace;

            if (!dataColumn.AllowDBNull)
            {
                result.Add("AllowDBNull", dataColumn.AllowDBNull);
            }
            if (dataColumn.AutoIncrement)
            {
                result.Add("AutoIncrement", dataColumn.AutoIncrement);
            }
            if (dataColumn.AutoIncrementSeed != 0)
            {
                result.Add("AutoIncrementSeed", dataColumn.AutoIncrementSeed);
            }
            if (dataColumn.AutoIncrementStep != 1)
            {
                result.Add("AutoIncrementStep", dataColumn.AutoIncrementStep);
            }
            if (!String.IsNullOrEmpty(dataColumn.Caption) && dataColumn.Caption != dataColumn.ColumnName)
            {
                result.Add("Caption", dataColumn.Caption);
            }
            if (dataColumn.ColumnMapping != MappingType.Element)
            {
                result.Add("ColumnMapping", dataColumn.ColumnMapping);
            }
            if (!String.IsNullOrEmpty(dataColumn.ColumnName))
            {
                result.Add("ColumnName", dataColumn.ColumnName);
            }
            result.Add("DataType", JaysonTypeInfo.GetTypeName(dataColumn.DataType, JaysonTypeNameInfo.TypeNameWithAssembly));
            if (!String.IsNullOrEmpty(dataColumn.Expression))
            {
                result.Add("Expression", dataColumn.Expression);
            }
            if (dataColumn.MaxLength != -1)
            {
                result.Add("MaxLength", dataColumn.MaxLength);
            }
            if (!String.IsNullOrEmpty(dataColumn.Namespace) && dataColumn.Namespace != defaultNamespace)
            {
                result.Add("Namespace", dataColumn.Namespace);
            }
            result.Add("Ordinal", dataColumn.Ordinal);
            if (!String.IsNullOrEmpty(dataColumn.Prefix))
            {
                result.Add("Prefix", dataColumn.Prefix);
            }
            if (dataColumn.ReadOnly)
            {
                result.Add("ReadOnly", dataColumn.ReadOnly);
            }
            if (dataColumn.Unique)
            {
                result.Add("Unique", dataColumn.Unique);
            }
            if (dataColumn.ExtendedProperties.Count > 0)
            {
                result.Add("ExtendedProperties", AsDictionary(dataColumn.ExtendedProperties, context));
            }

            return result;
        }

        private static List<object> AsDataTableColumns(DataTable dataTable, JaysonSerializationContext context)
        {
            var columns = dataTable.Columns;
            if (columns.Count > 0)
            {
                var result = new List<object>(columns.Count);
                foreach (DataColumn dataColumn in columns)
                {
                    result.Add(AsDataTableColumn(dataColumn, context));
                }

                return result;
            }
            return null;
        }

        private static Dictionary<string, object> AsDataTable(DataTable dataTable, JaysonSerializationContext context)
        {
            var result = new Dictionary<string, object>(10);

            if (context.Settings.UseObjectReferencing)
            {
                result.Add("$id", context.ReferenceMap.GetObjectId(dataTable));
            }

            if (dataTable.CaseSensitive)
            {
                result.Add("CaseSensitive", dataTable.CaseSensitive);
            }

            var columns = AsDataTableColumns(dataTable, context);
            if (columns != null)
            {
                result.Add("Columns", columns);
            }

            if (!String.IsNullOrEmpty(dataTable.DisplayExpression))
            {
                result.Add("DisplayExpression", dataTable.DisplayExpression);
            }
            if (dataTable.Locale != CultureInfo.InvariantCulture)
            {
                result.Add("Locale", dataTable.Locale.Name);
            }
            if (!String.IsNullOrEmpty(dataTable.Namespace))
            {
                result.Add("Namespace", dataTable.Namespace);
            }
            if (!String.IsNullOrEmpty(dataTable.Prefix))
            {
                result.Add("Prefix", dataTable.Prefix);
            }

            var primaryKey = AsColumnNames(dataTable.PrimaryKey, context);
            if (primaryKey != null)
            {
                result.Add("PrimaryKey", primaryKey);
            }

            var rows = AsDataTableRows(dataTable, context);
            if (columns != null)
            {
                result.Add("Rows", rows);
            }
            if (!String.IsNullOrEmpty(dataTable.TableName))
            {
                result.Add("TableName", dataTable.TableName);
            }
            if (dataTable.ExtendedProperties.Count > 0)
            {
                result.Add("ExtendedProperties", AsDictionary(dataTable.ExtendedProperties, context));
            }

            return result;
        }

        private static List<object> AsDataTables(DataSet dataSet, JaysonSerializationContext context)
        {
            var tables = dataSet.Tables;
            if (tables.Count > 0)
            {
                var result = new List<object>(tables.Count);
                foreach (DataTable table in tables)
                {
                    result.Add(AsDataTable(table, context));
                }
                return result;
            }
            return null;
        }

        private static Dictionary<string, object> AsDataRelation(DataRelation relation, JaysonSerializationContext context)
        {
            var result = new Dictionary<string, object>(10);

            if (!String.IsNullOrEmpty(relation.RelationName))
            {
                result.Add("RelationName", relation.RelationName);
            }

            var columnNames = AsColumnNames(relation.ChildColumns, context);
            if (columnNames != null)
            {
                result.Add("ChildColumns", columnNames);
            }

            if (relation.ChildTable != null)
            {
                if (relation.ChildTable.TableName != null)
                {
                    result.Add("ChildTable", relation.ChildTable.TableName);
                }

                if (!String.IsNullOrEmpty(relation.ChildTable.Namespace))
                {
                    result.Add("ChildTableNamespace", relation.ChildTable.Namespace);
                }
            }

            if (relation.ExtendedProperties.Count > 0)
            {
                result.Add("ExtendedProperties", AsDictionary(relation.ExtendedProperties, context));
            }

            if (relation.Nested)
            {
                result.Add("Nested", relation.Nested);
            }

            columnNames = AsColumnNames(relation.ParentColumns, context);
            if (columnNames != null)
            {
                result.Add("ParentColumns", columnNames);
            }

            if (relation.ParentTable != null)
            {
                if (relation.ParentTable.TableName != null)
                {
                    result.Add("ParentTable", relation.ParentTable.TableName);
                }

                if (!String.IsNullOrEmpty(relation.ParentTable.Namespace))
                {
                    result.Add("ParentTableNamespace", relation.ParentTable.Namespace);
                }
            }

            return result;
        }

        private static List<object> AsDataRelations(DataRelationCollection relations, JaysonSerializationContext context)
        {
            if (relations.Count > 0)
            {
                int relationCount = relations.Count;
                var result = new List<object>(relationCount);

                for (int i = 0; i < relationCount; i++)
                {
                    result.Add(AsDataRelation(relations[i], context));
                }

                return result;
            }
            return null;
        }

        private static Dictionary<string, object> AsDataSet(DataSet dataSet, JaysonSerializationContext context)
        {
            var result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

            if (context.Settings.UseObjectReferencing)
            {
                result.Add("$id", context.ReferenceMap.GetObjectId(dataSet));
            }

            if (dataSet.CaseSensitive)
            {
                result.Add("CaseSensitive", dataSet.CaseSensitive);
            }
            if (!String.IsNullOrEmpty(dataSet.DataSetName))
            {
                result.Add("DataSetName", dataSet.DataSetName);
            }
            if (!dataSet.EnforceConstraints)
            {
                result.Add("EnforceConstraints", dataSet.EnforceConstraints);
            }
            if (dataSet.Locale != CultureInfo.InvariantCulture)
            {
                result.Add("Locale", dataSet.Locale.Name);
            }
            if (!String.IsNullOrEmpty(dataSet.Namespace))
            {
                result.Add("Namespace", dataSet.Namespace);
            }
            if (!String.IsNullOrEmpty(dataSet.Prefix))
            {
                result.Add("Prefix", dataSet.Prefix);
            }
            if (dataSet.SchemaSerializationMode != SchemaSerializationMode.IncludeSchema)
            {
                result.Add("SchemaSerializationMode", dataSet.SchemaSerializationMode);
            }
            if (dataSet.ExtendedProperties.Count > 0)
            {
                result.Add("ExtendedProperties", AsDictionary(dataSet.ExtendedProperties, context));
            }

            var relations = AsDataRelations(dataSet.Relations, context);
            if (relations != null)
            {
                result.Add("Relations", relations);
            }

            var tables = AsDataTables(dataSet, context);
            if (tables != null)
            {
                result.Add("Tables", tables);
            }

            return result;
        }

        # endregion DataTable & DataSet

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

                if (obj == DBNull.Value)
                {
                    return null;
                }

                JaysonStackList<object> stack = null;
                if (info.Class)
                {
                    stack = context.Stack;
                    if (stack.Contains(obj))
                    {
                        if (context.Settings.UseObjectReferencing)
                        {
                            return AsObjectRefence(obj, context);
                        }

                        if (context.Settings.RaiseErrorOnCircularRef)
                        {
                            throw new JaysonException(JaysonError.CircularReferenceOn + info.Type.Name);
                        }
                        return null;
                    }

                    stack.Push(obj);

                    if (context.Settings.UseObjectReferencing)
                    {
                        int id;
                        if (context.ReferenceMap.TryGetObjectId(obj, out id))
                        {
                            return AsObjectRefence(id, context);
                        }
                    }
                }

                try
                {
                    var serializationType = info.SerializationType;

                    switch (serializationType)
                    {
                        case JaysonTypeSerializationType.IDictionaryGeneric:
                            {
#if !(NET3500 || NET3000 || NET2000)
                                if (context.Settings.IgnoreExpandoObjects && (info.Type == typeof(ExpandoObject)))
                                {
                                    return null;
                                }
#endif

                                if (obj is IDictionary<string, object>)
                                {
                                    return AsGenericStringDictionary((IDictionary<string, object>)obj, context);
                                }

                                if (obj is IDictionary)
                                {
                                    return AsDictionary((IDictionary)obj, context);
                                }
                                break;
                            }
                        case JaysonTypeSerializationType.IDictionary:
                            {
                                return AsDictionary((IDictionary)obj, context);
                            }
                        case JaysonTypeSerializationType.DataSet:
                            {
                                return AsDataSet((DataSet)obj, context);
                            }
                        case JaysonTypeSerializationType.DataTable:
                            {
                                return AsDataTable((DataTable)obj, context);
                            }
#if !(NET3500 || NET3000 || NET2000)
                        case JaysonTypeSerializationType.DynamicObject:
                            {
                                if (!context.Settings.IgnoreDynamicObjects)
                                {
                                    return AsDynamicObject((DynamicObject)obj, info.Type, context);
                                }
                                return null;
                            }
#endif
                        case JaysonTypeSerializationType.StringDictionary:
                            {
                                return AsStringDictionary((StringDictionary)obj, context);
                            }
                        case JaysonTypeSerializationType.NameValueCollection:
                            {
                                return AsNameValueCollection((NameValueCollection)obj, context);
                            }
                        case JaysonTypeSerializationType.IEnumerable:
                            {
                                return AsEnumerable((IEnumerable)obj, info.Type, context);
                            }
                        case JaysonTypeSerializationType.ISerializable:
                            {
                                return AsISerializable((ISerializable)obj, info.Type, context);
                            }
                        case JaysonTypeSerializationType.Type:
                            {
                                return AsType((Type)obj, info.Type, context);
                            }
                        case JaysonTypeSerializationType.FieldInfo:
                        case JaysonTypeSerializationType.PropertyInfo:
                            {
                                return AsMemberInfo((MemberInfo)obj, info.Type, context);
                            }
                        case JaysonTypeSerializationType.MethodInfo:
                            {
                                return AsMethodInfo((MethodInfo)obj, info.Type, context);
                            }
                        case JaysonTypeSerializationType.ConstructorInfo:
                            {
                                return AsConstructorInfo((ConstructorInfo)obj, info.Type, context);
                            }
                        case JaysonTypeSerializationType.ParameterInfo:
                            {
                                return AsParameterInfo((ParameterInfo)obj, info.Type, context);
                            }
                        case JaysonTypeSerializationType.Anonymous:
                            {
                                if (context.Settings.IgnoreAnonymousTypes && info.Anonymous)
                                {
                                    return null;
                                }
                                return AsObject(obj, info.Type, context);
                            }
                        default:
                            {
                                return AsObject(obj, info.Type, context);
                            }
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

            return obj;
        }

        public static object ToJsonObject(object obj, JaysonSerializationSettings settings = null,
            Func<string, object, object> filter = null)
        {
            var stack = JaysonStackList<object>.Get();
            try
            {
                using (var context = new JaysonSerializationContext(filter: filter,
                    settings: settings ?? JaysonSerializationSettings.Default,
                    stack: stack
                ))
                {
                    return ToJsonObject(obj, context);
                }
            }
            finally
            {
                JaysonStackList<object>.Release(stack);
            }
        }

        # endregion ToJsonObject
    }

    # endregion JsonConverter
}
