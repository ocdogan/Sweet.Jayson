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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace Sweet.Jayson
{
    # region JaysonTypeInfo

    internal sealed class JaysonTypeInfo
    {
        # region InfoItem

        private class InfoItem<T>
        {
            public T Value;
            public bool HasValue = false;
        }

        # endregion InfoItem

        # region JaysonTypeNameResolver

        public sealed class JaysonTypeNameResolver
        {
            # region Static Readonly Members

            private static readonly int s_NameLength;
            private static readonly Assembly SystemAssembly = typeof(Uri).Assembly;
            private static readonly Assembly MSCoreLibAssembly = typeof(int).Assembly;

            # endregion Static Readonly Members

            # region Field Members

            private Type m_Type;
            private JaysonTypeInfo m_Info;
            private JaysonTypeName m_TypeName;
            private string[] m_TypeNames;

            # endregion Field Members

            static JaysonTypeNameResolver()
            {
                s_NameLength = System.Enum.GetValues(typeof(JaysonTypeNameInfo)).Length;
            }

            public JaysonTypeNameResolver(JaysonTypeInfo info)
            {
                m_Info = info;
                m_Type = info.Type;
                m_TypeNames = new string[s_NameLength];
                m_TypeName = JaysonTypeName.GetTypeName(m_Type);
            }

            public string this[int indexer]
            {
                get
                {
                    var result = m_TypeNames[indexer];
                    if (result == null)
                    {
                        switch (indexer)
                        {
                            case (int)JaysonTypeNameInfo.Auto:
                            case (int)JaysonTypeNameInfo.TypeName:
                                if (!m_Info.Generic)
                                {
                                    var asm = m_Type.Assembly;
                                    if (asm == MSCoreLibAssembly || asm == SystemAssembly)
                                    {
                                        m_TypeNames[indexer] = m_TypeName.FormatTypeName(JaysonTypeNameFormatFlags.Simple);
                                        break;
                                    }
                                }
                                m_TypeNames[indexer] = m_TypeName.FormatTypeName(JaysonTypeNameFormatFlags.Simple | JaysonTypeNameFormatFlags.Assembly);
                                break;
                            case (int)JaysonTypeNameInfo.TypeNameWithAssemblyAndVersion:
                                m_TypeNames[indexer] = m_TypeName.AssemblyQualifiedName;
                                break;
                            case (int)JaysonTypeNameInfo.TypeNameWithAssembly:
                                m_TypeNames[indexer] = m_TypeName.FormatTypeName(JaysonTypeNameFormatFlags.Simple | JaysonTypeNameFormatFlags.Assembly);
                                break;
                            default:
                                return null;
                        }
                        return m_TypeNames[indexer];
                    }
                    return result;
                }
            }
        }

        # endregion JaysonTypeNameResolver

        # region Static Members

        private static readonly object s_InfoCacheLock = new object();
        private static Dictionary<Type, JaysonTypeInfo> s_InfoCache = new Dictionary<Type, JaysonTypeInfo>();

        # endregion Static Members

        # region Field Members

        private int? m_ArrayDepth;
        private int? m_ArrayRank;
        private bool? m_IsArray;
        private bool? m_IsClass;
        private bool? m_IsEnum;
        private bool? m_IsAnonymous;
        private bool? m_IsNullable;
        private bool? m_IsGeneric;
        private bool? m_IsGenericTypeDefinition;
        private bool? m_IsInterface;
        private bool? m_IsNumber;
        private bool? m_IsJPrimitive;
        private bool? m_IsPrimitive;
        private bool? m_IsValueType;
        private bool? m_IsISerializable;
        private bool? m_DefaultJConstructor;
        private Type[] m_GenericArguments;
        private Type[] m_Interfaces;
        private TypeCode? m_TypeCode;
        private JaysonTypeNameResolver m_TypeName;
        private JaysonTypeCode? m_JTypeCode;
        private JaysonTypeSerializationType? m_SerializationType;
        private InfoItem<object> m_Default = new InfoItem<object>();
        private InfoItem<Type> m_ElementType = new InfoItem<Type>();
        private InfoItem<Type> m_ElementRootType = new InfoItem<Type>();
        private InfoItem<Type> m_GenericTypeDefinition = new InfoItem<Type>();
        private InfoItem<Type> m_UnderlyingType = new InfoItem<Type>();

        private object m_SyncRoot;

        public readonly Type Type;

        # endregion Field Members

        private JaysonTypeInfo(Type type)
        {
            Type = type;
        }

        public bool Anonymous
        {
            get
            {
                if (!m_IsAnonymous.HasValue)
                {
                    if (((Type.Attributes & TypeAttributes.Sealed) == TypeAttributes.Sealed) &&
                        ((Type.Attributes & TypeAttributes.BeforeFieldInit) == TypeAttributes.BeforeFieldInit))
                    {
                        string typeName = Type.Name;

                        m_IsAnonymous = (typeName.Length > 12) &&
                            ((typeName[0] == '<' && typeName[1] == '>') ||
                                (typeName[0] == 'V' && typeName[1] == 'B' && typeName[2] == '$')) &&
                            (typeName.Contains("AnonType") || typeName.Contains("AnonymousType")) &&
                            Attribute.IsDefined(Type, typeof(CompilerGeneratedAttribute), false);

                        return m_IsAnonymous.Value;
                    }

                    m_IsAnonymous = false;
                }
                return m_IsAnonymous.Value;
            }
        }

        public bool Array
        {
            get
            {
                if (!m_IsArray.HasValue)
                {
                    m_IsArray = Type.IsArray;
                }
                return m_IsArray.Value;
            }
        }

        public int ArrayDepth
        {
            get
            {
                if (!m_ArrayDepth.HasValue)
                {
                    m_ArrayDepth = !Array ? 0 : GetTypeInfo(ElementType).ArrayDepth + 1;
                }
                return m_ArrayDepth.Value;
            }
        }

        public int ArrayRank
        {
            get
            {
                if (!m_ArrayRank.HasValue)
                {
                    m_ArrayRank = Array ? Type.GetArrayRank() : 0;
                }
                return m_ArrayRank.Value;
            }
        }

        public bool Class
        {
            get
            {
                if (!m_IsClass.HasValue)
                {
                    m_IsClass = Type.IsClass;
                }
                return m_IsClass.Value;
            }
        }

        public object Default
        {
            get
            {
                if (!m_Default.HasValue)
                {
                    if (Type == typeof(void*))
                    {
                        m_Default.Value = null;
                    }
                    else
                    {
                        var defaultMi = typeof(JaysonTypeInfo).GetMethod("GetDefaultValue",
                            BindingFlags.Static |
                            BindingFlags.NonPublic |
                            BindingFlags.InvokeMethod).MakeGenericMethod(new Type[] { Type });

                        m_Default.Value = defaultMi.Invoke(null, null);
                    }
                    m_Default.HasValue = true;
                }
                return m_Default.Value;
            }
        }

        public bool DefaultJConstructor
        {
            get
            {
                if (!m_DefaultJConstructor.HasValue)
                {
                    m_DefaultJConstructor = (ValueType && !Enum && !Primitive &&
                        !(Generic && GenericTypeDefinitionType == typeof(Nullable<>))) ||
                        Type.GetConstructor(Type.EmptyTypes) != null;
                }
                return m_DefaultJConstructor.Value;
            }
        }

        public Type ElementRootType
        {
            get
            {
                if (!m_ElementRootType.HasValue)
                {
                    if (!Array)
                    {
                        m_ElementRootType.Value = Type;
                    }
                    else
                    {
                        var info = GetTypeInfo(ElementType);
                        m_ElementRootType.Value = info.ElementRootType;
                    }
                    m_ElementRootType.HasValue = true;
                }
                return m_ElementRootType.Value;
            }
        }

        public Type ElementType
        {
            get
            {
                if (!m_ElementType.HasValue)
                {
                    m_ElementType.Value = Type.GetElementType();
                    m_ElementType.HasValue = true;
                }
                return m_ElementType.Value;
            }
        }

        public bool Enum
        {
            get
            {
                if (!m_IsEnum.HasValue)
                {
                    m_IsEnum = Type.IsEnum;
                }
                return m_IsEnum.Value;
            }
        }

        public bool Generic
        {
            get
            {
                if (!m_IsGeneric.HasValue)
                {
                    m_IsGeneric = Type.IsGenericType;
                }
                return m_IsGeneric.Value;
            }
        }

        public Type[] GenericArguments
        {
            get
            {
                if (m_GenericArguments == null && (Generic || GenericTypeDefinition))
                {
                    m_GenericArguments = Type.GetGenericArguments() ?? new Type[0];
                }
                return m_GenericArguments;
            }
        }

        public bool GenericTypeDefinition
        {
            get
            {
                if (!m_IsGenericTypeDefinition.HasValue)
                {
                    m_IsGenericTypeDefinition = Type.IsGenericTypeDefinition;
                }
                return m_IsGenericTypeDefinition.Value;
            }
        }

        public Type GenericTypeDefinitionType
        {
            get
            {
                if (!m_GenericTypeDefinition.HasValue)
                {
                    m_GenericTypeDefinition.Value = Type.GetGenericTypeDefinition();
                    m_GenericTypeDefinition.HasValue = true;
                }
                return m_GenericTypeDefinition.Value;
            }
        }

        public bool Interface
        {
            get
            {
                if (!m_IsInterface.HasValue)
                {
                    m_IsInterface = Type.IsInterface;
                }
                return m_IsInterface.Value;
            }
        }

        public Type[] Interfaces
        {
            get
            {
                if (m_Interfaces == null)
                {
                    m_Interfaces = Type.GetInterfaces() ?? new Type[0];
                }
                return m_Interfaces;
            }
        }

        public bool ISerializable
        {
            get
            {
                if (m_IsISerializable == null)
                {
                    m_IsISerializable = typeof(ISerializable).IsAssignableFrom(Type);
                }
                return m_IsISerializable.Value;
            }
        }

        public bool JPrimitive
        {
            get
            {
                if (!m_IsJPrimitive.HasValue)
                {
                    m_IsJPrimitive = IsJPrimitiveType(Type);
                }
                return m_IsJPrimitive.Value;
            }
        }

        public JaysonTypeCode JTypeCode
        {
            get
            {
                if (!m_JTypeCode.HasValue)
                {
                    m_JTypeCode = JTypeCodeOf(Type);
                }
                return m_JTypeCode.Value;
            }
        }

        public bool Nullable
        {
            get
            {
                if (!m_IsNullable.HasValue)
                {
                    m_IsNullable = (Generic &&
                        GenericTypeDefinitionType == typeof(Nullable<>));
                }
                return m_IsNullable.Value;
            }
        }

        public bool Number
        {
            get
            {
                if (!m_IsNumber.HasValue)
                {
                    m_IsNumber = IsNumberType(Type);
                }
                return m_IsNumber.Value;
            }
        }

        public bool Primitive
        {
            get
            {
                if (!m_IsPrimitive.HasValue)
                {
                    m_IsPrimitive = ((JaysonTypeCode.Primitive & JTypeCode) == JTypeCode) ||
                        (Type != null ? Type.IsPrimitive : false);
                }
                return m_IsPrimitive.Value;
            }
        }

        public JaysonTypeSerializationType SerializationType
        {
            get
            {
                if (m_SerializationType == null)
                {
                    if (Enum)
                    {
                        m_SerializationType = JaysonTypeSerializationType.Enum;
                    }
                    else if (typeof(IDictionary<string, object>).IsAssignableFrom(Type))
                    {
                        m_SerializationType = JaysonTypeSerializationType.IDictionaryGeneric;
                    }
                    else if (typeof(IDictionary).IsAssignableFrom(Type))
                    {
                        m_SerializationType = JaysonTypeSerializationType.IDictionary;
                    }
#if !(NET3500 || NET3000 || NET2000)
                    else if (typeof(DynamicObject).IsAssignableFrom(Type))
                    {
                        m_SerializationType = JaysonTypeSerializationType.DynamicObject;
                    }
#endif
                    else if (typeof(DataTable).IsAssignableFrom(Type))
                    {
                        m_SerializationType = JaysonTypeSerializationType.DataTable;
                    }
                    else if (typeof(DataSet).IsAssignableFrom(Type))
                    {
                        m_SerializationType = JaysonTypeSerializationType.DataSet;
                    }
                    else if (typeof(StringDictionary).IsAssignableFrom(Type))
                    {
                        m_SerializationType = JaysonTypeSerializationType.StringDictionary;
                    }
                    else if (typeof(NameValueCollection).IsAssignableFrom(Type))
                    {
                        m_SerializationType = JaysonTypeSerializationType.NameValueCollection;
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(Type))
                    {
                        m_SerializationType = JaysonTypeSerializationType.IEnumerable;
                    }
                    else if (Anonymous)
                    {
                        m_SerializationType = JaysonTypeSerializationType.Anonymous;
                    }
                    else if (typeof(FieldInfo).IsAssignableFrom(Type))
                    {
                        m_SerializationType = JaysonTypeSerializationType.FieldInfo;
                    }
                    else if (typeof(PropertyInfo).IsAssignableFrom(Type))
                    {
                        m_SerializationType = JaysonTypeSerializationType.PropertyInfo;
                    }
                    else if (typeof(ConstructorInfo).IsAssignableFrom(Type))
                    {
                        m_SerializationType = JaysonTypeSerializationType.ConstructorInfo;
                    }
                    else if (typeof(MethodInfo).IsAssignableFrom(Type))
                    {
                        m_SerializationType = JaysonTypeSerializationType.MethodInfo;
                    }
                    else if (typeof(ParameterInfo).IsAssignableFrom(Type))
                    {
                        m_SerializationType = JaysonTypeSerializationType.ParameterInfo;
                    }
                    else if (typeof(Type).IsAssignableFrom(Type))
                    {
                        m_SerializationType = JaysonTypeSerializationType.Type;
                    }
                    else if (typeof(ISerializable).IsAssignableFrom(Type))
                    {
                        m_SerializationType = JaysonTypeSerializationType.ISerializable;
                    }
                    else
                    {
                        m_SerializationType = JaysonTypeSerializationType.ClassOrStruct;
                    }
                }
                return m_SerializationType.Value;
            }
        }

        public object SyncRoot
        {
            get
            {
                if (m_SyncRoot == null)
                {
                    Interlocked.CompareExchange(ref m_SyncRoot, new object(), null);
                }
                return m_SyncRoot;
            }
        }

        public TypeCode TypeCode
        {
            get
            {
                if (!m_TypeCode.HasValue)
                {
                    m_TypeCode = Type.GetTypeCode(Type);
                }
                return m_TypeCode.Value;
            }
        }

        public JaysonTypeNameResolver TypeName
        {
            get
            {
                if (m_TypeName == null)
                {
                    m_TypeName = new JaysonTypeNameResolver(this);
                }
                return m_TypeName;
            }
        }

        public Type UnderlyingType
        {
            get
            {
                if (!m_UnderlyingType.HasValue)
                {
                    m_UnderlyingType.Value = Nullable ? System.Nullable.GetUnderlyingType(Type) : null;
                    m_UnderlyingType.HasValue = true;
                }
                return m_UnderlyingType.Value;
            }
        }

        public bool ValueType
        {
            get
            {
                if (!m_IsValueType.HasValue)
                {
                    m_IsValueType = Type.IsValueType;
                }
                return m_IsValueType.Value;
            }
        }

        public static int GetArrayRank(Type type)
        {
            return GetTypeInfo(type).ArrayRank;
        }

        public static object GetDefault(Type type)
        {
            return GetTypeInfo(type).Default;
        }

        private static T GetDefaultValue<T>()
        {
            return default(T);
        }

        public static Type GetElementRootType(Type type)
        {
            return GetTypeInfo(type).ElementRootType;
        }

        public static Type GetElementType(Type type)
        {
            return GetTypeInfo(type).ElementType;
        }

        public static Type[] GetGenericArguments(Type type)
        {
            return GetTypeInfo(type).GenericArguments;
        }

        public static Type GetGenericTypeDefinition(Type type)
        {
            return GetTypeInfo(type).GenericTypeDefinitionType;
        }

        public static Type[] GetInterfaces(Type type)
        {
            return GetTypeInfo(type).Interfaces;
        }

        public static JaysonTypeCode GetJTypeCode(Type type)
        {
            return GetTypeInfo(type).JTypeCode;
        }

        public static JaysonTypeSerializationType GetSerializationType(Type type)
        {
            return GetTypeInfo(type).SerializationType;
        }

        public static TypeCode GetTypeCode(Type type)
        {
            return GetTypeInfo(type).TypeCode;
        }

        public static JaysonTypeInfo GetTypeInfo(Type type)
        {
            var contains = false;
            JaysonTypeInfo info;
            lock (s_InfoCacheLock)
            {
                contains = s_InfoCache.TryGetValue(type, out info);
            }

            if (!contains)
            {
                lock (s_InfoCacheLock)
                {
                    if (!s_InfoCache.TryGetValue(type, out info))
                    {
                        info = new JaysonTypeInfo(type);
                        s_InfoCache[type] = info;
                    }
                }
            }
            return info;
        }

        public static string GetTypeName(Type type, JaysonTypeNameInfo nameInfo)
        {
            return GetTypeInfo(type).TypeName[(int)nameInfo];
        }

        public static Type GetUnderlyingType(Type type)
        {
            return GetTypeInfo(type).UnderlyingType;
        }

        public static bool HasDefaultJConstructor(Type type)
        {
            return GetTypeInfo(type).DefaultJConstructor;
        }

        public static bool IsAnonymous(Type type)
        {
            return GetTypeInfo(type).Anonymous;
        }

        public static bool IsAnonymous(string typeName)
        {
            return !String.IsNullOrEmpty(typeName) &&
                (typeName.Length > 12) &&
                ((typeName[0] == '<' && typeName[1] == '>') ||
                    (typeName[0] == 'V' && typeName[1] == 'B' && typeName[2] == '$')) &&
                (typeName.Contains("AnonType") || typeName.Contains("AnonymousType"));
        }

        public static bool IsArray(Type type)
        {
            return GetTypeInfo(type).Array;
        }

        public static bool IsClass(Type type)
        {
            return GetTypeInfo(type).Class;
        }

        public static bool IsEnum(Type type)
        {
            return GetTypeInfo(type).Enum;
        }

        public static bool IsGeneric(Type type)
        {
            return GetTypeInfo(type).Generic;
        }

        public static bool IsGenericTypeDefinition(Type type)
        {
            return GetTypeInfo(type).GenericTypeDefinition;
        }

        public static bool IsInterface(Type type)
        {
            return GetTypeInfo(type).Interface;
        }

        public static bool IsISerializable(Type type)
        {
            return GetTypeInfo(type).ISerializable;
        }

        public static bool IsJPrimitive(Type type)
        {
            return GetTypeInfo(type).JPrimitive;
        }

        public static bool IsJPrimitiveObject(object obj)
        {
            return obj != null && GetJTypeCode(obj.GetType()) != JaysonTypeCode.Object;
        }

        private static bool IsJPrimitiveType(Type type)
        {
            return JTypeCodeOf(type) != JaysonTypeCode.Object;
        }

        public static bool IsNullable(Type type)
        {
            return GetTypeInfo(type).Nullable;
        }

        private static bool IsNumberType(Type type)
        {
            JaysonTypeCode jtc = JTypeCodeOf(type);
            return (JaysonTypeCode.Number & jtc) == jtc;
        }

        public static bool IsNumber(Type type)
        {
            return GetTypeInfo(type).Number;
        }

        public static bool IsPrimitive(Type type)
        {
            return GetTypeInfo(type).Primitive;
        }

        public static bool IsValue(Type type)
        {
            return GetTypeInfo(type).ValueType;
        }

        private static JaysonTypeCode JTypeCodeOf(Type type)
        {
            // Do not change the check order
            if (type == typeof(string)) { return JaysonTypeCode.String; }
            if (type == typeof(int)) { return JaysonTypeCode.Int; }
            if (type == typeof(bool)) { return JaysonTypeCode.Bool; }
            if (type == typeof(long)) { return JaysonTypeCode.Long; }
            if (type == typeof(DateTime)) { return JaysonTypeCode.DateTime; }
            if (type == typeof(double)) { return JaysonTypeCode.Double; }
            if (type == typeof(int?)) { return JaysonTypeCode.IntNullable; }
            if (type == typeof(bool?)) { return JaysonTypeCode.BoolNullable; }
            if (type == typeof(long?)) { return JaysonTypeCode.LongNullable; }
            if (type == typeof(short)) { return JaysonTypeCode.Short; }
            if (type == typeof(byte)) { return JaysonTypeCode.Byte; }
            if (type == typeof(float)) { return JaysonTypeCode.Float; }
            if (type == typeof(DateTime?)) { return JaysonTypeCode.DateTimeNullable; }
            if (type == typeof(double?)) { return JaysonTypeCode.DoubleNullable; }
            if (type == typeof(decimal)) { return JaysonTypeCode.Decimal; }
            if (type == typeof(decimal?)) { return JaysonTypeCode.DecimalNullable; }
            if (type == typeof(byte?)) { return JaysonTypeCode.ByteNullable; }
            if (type == typeof(float?)) { return JaysonTypeCode.FloatNullable; }
            if (type == typeof(uint)) { return JaysonTypeCode.UInt; }
            if (type == typeof(ulong)) { return JaysonTypeCode.ULong; }
            if (type == typeof(uint?)) { return JaysonTypeCode.UIntNullable; }
            if (type == typeof(ulong?)) { return JaysonTypeCode.ULongNullable; }
            if (type == typeof(char)) { return JaysonTypeCode.Char; }
            if (type == typeof(char?)) { return JaysonTypeCode.CharNullable; }
            if (type == typeof(ushort)) { return JaysonTypeCode.UShortNullable; }
            if (type == typeof(sbyte)) { return JaysonTypeCode.SByte; }
            if (type == typeof(ushort?)) { return JaysonTypeCode.UShortNullable; }
            if (type == typeof(sbyte?)) { return JaysonTypeCode.SByteNullable; }
            if (type == typeof(Guid)) { return JaysonTypeCode.Guid; }
            if (type == typeof(Guid?)) { return JaysonTypeCode.GuidNullable; }
            if (type == typeof(TimeSpan)) { return JaysonTypeCode.TimeSpan; }
            if (type == typeof(TimeSpan?)) { return JaysonTypeCode.TimeSpanNullable; }
            if (type == typeof(DateTimeOffset)) { return JaysonTypeCode.DateTimeOffset; }
            if (type == typeof(DateTimeOffset?)) { return JaysonTypeCode.DateTimeNullable; }
            return JaysonTypeCode.Object;
        }

        public override string ToString()
        {
            return (Type != null) ? Type.ToString() : String.Empty;
        }

        public override bool Equals(object obj)
        {
            if (obj is JaysonTypeInfo)
            {
                return ((JaysonTypeInfo)obj).Type == Type;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }
    }

    # endregion JaysonTypeInfo
}
