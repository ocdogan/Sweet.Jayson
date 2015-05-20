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

namespace Sweet.Jayson
{
	# region JaysonSerializationSettings

	public class JaysonSerializationSettings : ICloneable
	{
		public static readonly JaysonSerializationSettings Default = new JaysonSerializationSettings();
		private static readonly JaysonSerializationSettings Initial = new JaysonSerializationSettings ();

        private object m_TypeOverrideLock = new object();
        private Dictionary<Type, JaysonTypeOverride> m_TypeOverrides;

		public string DateTimeFormat;
		public string DateTimeOffsetFormat;
		public string NumberFormat;
		public string TimeSpanFormat;

		public bool CaseSensitive = true;
		public bool ConvertDecimalToDouble = false;
		public bool EscapeChars = true;
		public bool EscapeUnicodeChars = false;
		public bool Formatting = false;
		public bool GuidAsByteArray = false;
        public bool IgnoreAnonymousTypes = true;
        #if !(NET3500 || NET3000 || NET2000)
        public bool IgnoreDynamicObjects = false;
        public bool IgnoreExpandoObjects = false;
        #endif
        public bool IgnorePrimitiveTypeNames = true;
        public bool IgnoreReadOnlyMembers = false;
		public bool IgnoreNullValues = true;
        public bool IgnoreNullListItems = false;
		public bool OrderNames = false;
		public bool RaiseErrorOnCircularRef = false;
		public bool RaiseErrorOnMaxObjectDepth = true;
		public bool UseEnumNames = true;
		public bool UseGlobalTypeNames = false;

		public int MaxObjectDepth;

		public JaysonDateFormatType DateFormatType = JaysonDateFormatType.Iso8601;
		public JaysonDateTimeZoneType DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
		public JaysonTypeNameSerialization TypeNames = JaysonTypeNameSerialization.None;
        public JaysonTypeNameInfo TypeNameInfo = JaysonTypeNameInfo.TypeNameWithAssembly;

        public JaysonSerializationSettings()
            : this(null)
        { }

        public JaysonSerializationSettings(JaysonTypeOverride[] typeOverrides)
        {
            if (typeOverrides != null && typeOverrides.Length > 0)
            {
                m_TypeOverrides = new Dictionary<Type, JaysonTypeOverride>(typeOverrides.Length);
                foreach (var typeOverride in typeOverrides)
                {
                    if (typeOverride != null)
                    {
                        m_TypeOverrides[typeOverride.Type] = typeOverride;
                    }
                }
            }
        }

		internal void AssignTo(JaysonSerializationSettings destination)
		{
			destination.CaseSensitive = CaseSensitive;
			destination.ConvertDecimalToDouble = ConvertDecimalToDouble;
			destination.DateFormatType = DateFormatType;
			destination.DateTimeFormat = DateTimeFormat;
			destination.DateTimeOffsetFormat = DateTimeOffsetFormat;
			destination.DateTimeZoneType = DateTimeZoneType;
			destination.TimeSpanFormat = TimeSpanFormat;
			destination.IgnoreAnonymousTypes = IgnoreAnonymousTypes;
			#if !(NET3500 || NET3000 || NET2000)
			destination.IgnoreDynamicObjects = IgnoreDynamicObjects;
			destination.IgnoreExpandoObjects = IgnoreExpandoObjects;
			#endif
			destination.EscapeChars = EscapeChars;
			destination.EscapeUnicodeChars = EscapeUnicodeChars;
			destination.Formatting = Formatting;
			destination.GuidAsByteArray = GuidAsByteArray;
			destination.IgnorePrimitiveTypeNames = IgnorePrimitiveTypeNames;
            destination.IgnoreNullListItems = IgnoreNullListItems;
			destination.IgnoreNullValues = IgnoreNullValues;
			destination.IgnoreReadOnlyMembers = IgnoreReadOnlyMembers;
			destination.MaxObjectDepth = MaxObjectDepth;
			destination.NumberFormat = NumberFormat;
			destination.OrderNames = OrderNames;
			destination.RaiseErrorOnCircularRef = RaiseErrorOnCircularRef;
			destination.RaiseErrorOnMaxObjectDepth = RaiseErrorOnMaxObjectDepth;
			destination.UseEnumNames = UseEnumNames;
            destination.TypeNameInfo = TypeNameInfo;
			destination.TypeNames = TypeNames;

            AssignTypeOverridesTo(destination);
		}

		public object Clone()
		{
			JaysonSerializationSettings clone = new JaysonSerializationSettings();
			AssignTo (clone);

			return clone;
		}

		public void Reset()
		{
			Initial.AssignTo (this);
		}

		public static JaysonSerializationSettings DefaultClone()
		{
			return (JaysonSerializationSettings)Default.Clone ();
		}

        private void AssignTypeOverridesTo(JaysonSerializationSettings destination)
        {
            lock (m_TypeOverrideLock)
            {
                lock (destination.m_TypeOverrideLock)
                {
                    if (m_TypeOverrides == null || m_TypeOverrides.Count == 0)
                    {
                        destination.m_TypeOverrides = null;
                    }
                    else
                    {
                        if (destination.m_TypeOverrides == null)
                        {
                            destination.m_TypeOverrides = new Dictionary<Type, JaysonTypeOverride>(m_TypeOverrides.Count);
                        }
                        else
                        {
                            destination.m_TypeOverrides.Clear();
                        }

                        foreach (var toKvp in m_TypeOverrides)
                        {
                            destination.m_TypeOverrides.Add(toKvp.Key, (JaysonTypeOverride)toKvp.Value.Clone());
                        }
                    }
                }
            }
        }

        public JaysonTypeOverride GetTypeOverride(Type type)
        {
            if (m_TypeOverrides != null)
            {
                JaysonTypeOverride result = null;
                m_TypeOverrides.TryGetValue(type, out result);
                return result;
            }
            return null;
        }

        public JaysonSerializationSettings AddTypeOverride(JaysonTypeOverride typeOverride)
        {
            if (typeOverride != null)
            {
                if (m_TypeOverrides == null)
                {
                    m_TypeOverrides = new Dictionary<Type, JaysonTypeOverride>();
                }
                m_TypeOverrides[typeOverride.Type] = typeOverride;
            }
            return this;
        }

        public JaysonTypeOverride AddTypeOverride<T>()
        {
            return AddTypeOverride(typeof(T));
        }

        public JaysonTypeOverride AddTypeOverride(Type type)
        {
            if (type != null)
            {
                JaysonTypeOverride typeOverride = null;
                if (m_TypeOverrides == null)
                {
                    m_TypeOverrides = new Dictionary<Type, JaysonTypeOverride>();
                }
                else
                {
                    m_TypeOverrides.TryGetValue(type, out typeOverride);
                }

                if (typeOverride == null)
                {
                    typeOverride = new JaysonTypeOverride(type);
                    m_TypeOverrides[type] = typeOverride;
                }

                return typeOverride;
            }
            return null;
        }

        public JaysonTypeOverride AddTypeOverride<T>(Type bindToType)
        {
            JaysonTypeOverride typeOverride = AddTypeOverride(typeof(T));
            if (typeOverride != null)
            {
                typeOverride.BindToType = bindToType;
            }
            return typeOverride;
        }

        public JaysonTypeOverride AddTypeOverride(Type type, Type bindToType)
        {
            JaysonTypeOverride typeOverride = AddTypeOverride(type);
            if (typeOverride != null)
            {
                typeOverride.BindToType = bindToType;
            }
            return typeOverride;
        }

        public JaysonSerializationSettings AddTypeOverrides(JaysonTypeOverride[] typeOverrides)
        {
            if (typeOverrides != null && typeOverrides.Length > 0)
            {
                if (m_TypeOverrides == null)
                {
                    m_TypeOverrides = new Dictionary<Type,JaysonTypeOverride>();
                }

                foreach (var typeOverride in typeOverrides)
                {
                    if (typeOverride != null)
                    {
                        m_TypeOverrides[typeOverride.Type] = typeOverride;
                    }
                }
            }
            return this;
        }

        public JaysonSerializationSettings RemoveTypeOverride(Type type)
        {
            if (type != null && m_TypeOverrides != null)
            {
                lock (m_TypeOverrideLock)
                {
                    if (m_TypeOverrides.ContainsKey(type))
                    {
                        m_TypeOverrides.Remove(type);
                    }
                }
            }
            return this;
        }
    }

	# endregion JaysonSerializationSettings
}