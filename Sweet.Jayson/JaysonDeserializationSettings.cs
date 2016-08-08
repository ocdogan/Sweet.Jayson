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
using System.Runtime.Serialization;

namespace Sweet.Jayson
{
    # region JaysonDeserializationSettings

    public class JaysonDeserializationSettings : ICloneable
    {
        # region Constants

        public static readonly JaysonDeserializationSettings Default = new JaysonDeserializationSettings();
        private static readonly JaysonDeserializationSettings Initial = new JaysonDeserializationSettings();

        # endregion Constants

        # region Field Members

        private object m_TypeOverrideLock = new object();
        private Dictionary<Type, JaysonTypeOverride> m_TypeOverrides;

        public SerializationBinder Binder;

        public string DateTimeFormat;
        public string DateTimeOffsetFormat;

        public bool CaseSensitive = true;
        public bool ConvertDecimalToDouble = false;
        public bool IgnoreAnonymousTypes = true;
        public bool RaiseErrorOnMissingMember = false;
        public bool UseDefaultValues = false;

        public int MaxObjectDepth;

        public ArrayDeserializationType ArrayType = ArrayDeserializationType.List;
        public JaysonDateTimeZoneType DateTimeZoneType = JaysonDateTimeZoneType.KeepAsIs;
        public DictionaryDeserializationType DictionaryType = DictionaryDeserializationType.Dictionary;
        public JaysonCommentHandling CommentHandling = JaysonCommentHandling.Ignore;

        public JaysonObjectActivator ObjectActivator;
        public JaysonCtorParamMatcher CtorParamMatcher;

        # endregion Field Members

        public JaysonDeserializationSettings()
            : this(null)
        { }

        public JaysonDeserializationSettings(JaysonTypeOverride[] typeOverrides)
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

        internal void AssignTo(JaysonDeserializationSettings destination)
        {
            destination.ArrayType = ArrayType;
            destination.Binder = Binder;
            destination.CaseSensitive = CaseSensitive;
            destination.CommentHandling = CommentHandling;
            destination.ConvertDecimalToDouble = ConvertDecimalToDouble;
            destination.DateTimeFormat = DateTimeFormat;
            destination.DateTimeOffsetFormat = DateTimeOffsetFormat;
            destination.DateTimeZoneType = DateTimeZoneType;
            destination.DictionaryType = DictionaryType;
            destination.IgnoreAnonymousTypes = IgnoreAnonymousTypes;
            destination.MaxObjectDepth = MaxObjectDepth;
            destination.ObjectActivator = ObjectActivator;
            destination.RaiseErrorOnMissingMember = RaiseErrorOnMissingMember;
            destination.UseDefaultValues = UseDefaultValues;

            AssignTypeOverridesTo(destination);
        }

        public object Clone()
        {
            JaysonDeserializationSettings clone = new JaysonDeserializationSettings();
            AssignTo(clone);

            return clone;
        }

        public void Reset()
        {
            Initial.AssignTo(this);
        }

        public static JaysonDeserializationSettings DefaultClone()
        {
            return (JaysonDeserializationSettings)Default.Clone();
        }

        private void AssignTypeOverridesTo(JaysonDeserializationSettings destination)
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
            if (type != null)
            {
                if (m_TypeOverrides == null || m_TypeOverrides.Count == 0)
                {
                    return JaysonTypeOverrideGlobal.GetTypeOverride(type);
                }

                JaysonTypeOverride result;
                while (type != null)
                {
                    if (!m_TypeOverrides.TryGetValue(type, out result))
                    {
                        return result;
                    }
                    type = type.BaseType;
                }
                return JaysonTypeOverrideGlobal.GetTypeOverride(type);
            }
            return null;
        }

        public JaysonDeserializationSettings AddTypeOverride(JaysonTypeOverride typeOverride)
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

        public JaysonDeserializationSettings AddTypeOverrides(JaysonTypeOverride[] typeOverrides)
        {
            if (typeOverrides != null && typeOverrides.Length > 0)
            {
                if (m_TypeOverrides == null)
                {
                    m_TypeOverrides = new Dictionary<Type, JaysonTypeOverride>();
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

        public JaysonDeserializationSettings RemoveTypeOverride(Type type)
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

    # endregion JaysonDeserializationSettings
}