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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
#if !(NET3500 || NET3000 || NET2000)
using System.Threading.Tasks;
#endif

namespace Sweet.Jayson
{
    public static class JaysonTypeOverrideGlobal
    {
        # region Static Members

        private static readonly JaysonSynchronizedDictionary<Type, JaysonTypeOverride> s_GlobalTypeOverrides = new JaysonSynchronizedDictionary<Type, JaysonTypeOverride>();

        # endregion Static Members

        # region Methods

        public static JaysonTypeOverride GetTypeOverride(Type type)
        {
            if (type != null)
            {
                return s_GlobalTypeOverrides.GetValueOrUpdate(type, (t) =>
                    {
                        JaysonTypeOverride overrider;
                        while (t != null)
                        {
                            if (s_GlobalTypeOverrides.TryGetValue(t, out overrider))
                                return overrider;
                            t = t.BaseType;
                        }

                        return null;
                    });
            }
            return null;
        }

        public static void SetMemberAlias(Type type, string memberName, string alias)
        {
            if ((type != null) && !String.IsNullOrEmpty(memberName) && !String.IsNullOrEmpty(alias))
            {
                var overrider = s_GlobalTypeOverrides.GetValueOrUpdate(type, (t) =>
                    {
                        var result = JaysonTypeOverride.NewGlobal(t, null);
                        result.SetMemberAlias(memberName, alias);
                        return result;
                    });

                overrider.SetMemberAlias(memberName, alias);
            }
        }

        public static string GetMemberAlias(Type type, string memberName)
        {
            if ((type != null) && !String.IsNullOrEmpty(memberName))
            {
                var overrider = GetTypeOverride(type);
                if (overrider != null)
                {
                    return overrider.GetMemberAlias(memberName);
                }
            }
            return memberName;
        }

        public static string GetAliasMember(Type type, string alias)
        {
            if ((type != null) && !String.IsNullOrEmpty(alias))
            {
                JaysonTypeOverride overrider = GetTypeOverride(type);
                if (overrider != null)
                {
                    return overrider.GetAliasMember(alias);
                }
            }
            return null;
        }

        public static void IgnoreMember(Type type, string memberName)
        {
            if ((type != null) && !String.IsNullOrEmpty(memberName))
            {
                var overrider = s_GlobalTypeOverrides.GetValueOrUpdate(type, (t) =>
                {
                    var result = JaysonTypeOverride.NewGlobal(t, null);
                    result.IgnoreMember(memberName);
                    return result;
                });

                overrider.IgnoreMember(memberName);
            }
        }

        public static void IncludeMember(Type type, string memberName)
        {
            if ((type != null) && !String.IsNullOrEmpty(memberName))
            {
                JaysonTypeOverride overrider;
                if (s_GlobalTypeOverrides.TryGetValue(type, out overrider) && (overrider != null))
                    overrider.IncludeMember(memberName);
            }
        }

        public static bool IsMemberIgnored(Type type, string memberName)
        {
            if ((type != null) && !String.IsNullOrEmpty(memberName))
            {
                var overrider = GetTypeOverride(type);
                if (overrider != null)
                {
                    return overrider.IsMemberIgnored(memberName);
                }
            }
            return false;
        }

        public static void SetDefaultValue(Type type, string memberName, object defaultValue)
        {
            if ((type != null) && !String.IsNullOrEmpty(memberName))
            {
                var overrider = s_GlobalTypeOverrides.GetValueOrUpdate(type, (t) =>
                {
                    var result = JaysonTypeOverride.NewGlobal(t, null);
                    result.SetDefaultValue(memberName, defaultValue);
                    return result;
                });

                overrider.SetDefaultValue(memberName, defaultValue);
            }
        }

        public static object GetDefaultValue(Type type, string memberName)
        {
            if ((type != null) && !String.IsNullOrEmpty(memberName))
            {
                var overrider = GetTypeOverride(type);
                if (overrider != null)
                {
                    return overrider.GetDefaultValue(memberName);
                }
            }
            return null;
        }

        public static bool TryGetDefaultValue(Type type, string memberName, out object defaultValue)
        {
            defaultValue = null;
            if ((type != null) && !String.IsNullOrEmpty(memberName))
            {
                var overrider = GetTypeOverride(type);
                if (overrider != null)
                {
                    return overrider.TryGetDefaultValue(memberName, out defaultValue);
                }
            }
            return false;
        }

        # endregion Methods
    }

    public class JaysonTypeOverride : ICloneable
    {
        # region Field Members

        private Type m_Type;
        private Type m_BindToType;
        private object m_SyncRoot = new object();

        private JaysonSynchronizedDictionary<string, bool> m_IgnoredMembers = new JaysonSynchronizedDictionary<string, bool>();
        private JaysonSynchronizedDictionary<string, string> m_AliasToMemberName = new JaysonSynchronizedDictionary<string, string>();
        private JaysonSynchronizedDictionary<string, string> m_MemberNameToAlias = new JaysonSynchronizedDictionary<string, string>();
        private JaysonSynchronizedDictionary<string, object> m_MemberDefaultValues = new JaysonSynchronizedDictionary<string, object>();

        # endregion Field Members

        # region Properties

        public Type Type
        {
            get { return m_Type; }
        }

        public Type BindToType
        {
            get { return m_BindToType; }
            set { m_BindToType = value; }
        }

        # endregion Properties

        public JaysonTypeOverride(Type type, Type bindToType = null)
        {
            m_Type = type;
            m_BindToType = bindToType;
        }

        internal static JaysonTypeOverride NewGlobal(Type type, Type bindToType = null)
        {
            return new JaysonTypeOverride(type, bindToType) { IsGlobal = true };
        }

        public bool IsGlobal { get; private set; }

        public virtual bool IsMemberIgnored(string memberName)
        {
            bool result;
            if (!m_IgnoredMembers.TryGetValue(memberName, out result) && !IsGlobal)
            {
                return JaysonTypeOverrideGlobal.IsMemberIgnored(m_Type, null);
            }
            return false;
        }

        public virtual JaysonTypeOverride IgnoreMember(string memberName)
        {
            m_IgnoredMembers[memberName] = true;
            return this;
        }

        public virtual JaysonTypeOverride IncludeMember(string memberName)
        {
            m_IgnoredMembers[memberName] = false;
            return this;
        }

        public virtual JaysonTypeOverride SetMemberAlias(string memberName, string alias)
        {
            if (!String.IsNullOrEmpty(alias))
            {
                lock (m_SyncRoot)
                {
                    m_MemberNameToAlias[memberName] = alias;
                    m_AliasToMemberName[alias] = memberName;
                }
                return this;
            }
            return RemoveMemberAlias(memberName);
        }

        public virtual JaysonTypeOverride RemoveMemberAlias(string memberName)
        {
            lock (m_SyncRoot)
            {
                string alias;
                if (m_MemberNameToAlias.TryGetValue(memberName, out alias))
                {
                    m_MemberNameToAlias.Remove(memberName);
                    if (m_AliasToMemberName.ContainsKey(alias))
                    {
                        m_AliasToMemberName.Remove(alias);
                    }
                }
            }
            return this;
        }

        public virtual string GetMemberAlias(string memberName)
        {
            string result;
            if (!m_MemberNameToAlias.TryGetValue(memberName, out result) && !IsGlobal)
            {
                return JaysonTypeOverrideGlobal.GetMemberAlias(m_Type, memberName);
            }
            return result;
        }

        public virtual string GetAliasMember(string alias)
        {
            string result;
            if (!m_AliasToMemberName.TryGetValue(alias, out result) && !IsGlobal)
            {
                return JaysonTypeOverrideGlobal.GetAliasMember(m_Type, alias);
            }
            return result;
        }

        public virtual JaysonTypeOverride SetDefaultValue(string memberName, object defaultValue)
        {
            if (defaultValue != null)
            {
                m_MemberDefaultValues[memberName] = defaultValue;
                return this;
            }
            return RemoveDefaultValue(memberName);
        }

        public virtual JaysonTypeOverride RemoveDefaultValue(string memberName)
        {
            lock (((ICollection)m_MemberDefaultValues).SyncRoot)
            {
                object defaultValue;
                if (m_MemberDefaultValues.TryGetValue(memberName, out defaultValue))
                {
                    m_MemberDefaultValues.Remove(memberName);
                }
            }
            return this;
        }

        public virtual object GetDefaultValue(string memberName)
        {
            object result;
            if (!m_MemberDefaultValues.TryGetValue(memberName, out result) && !IsGlobal)
            {
                return JaysonTypeOverrideGlobal.GetDefaultValue(m_Type, memberName);
            }
            return result;
        }

        public virtual bool TryGetDefaultValue(string memberName, out object defaultValue)
        {
            defaultValue = null;
            if (!m_MemberDefaultValues.TryGetValue(memberName, out defaultValue))
            {
                if (!IsGlobal)
                {
                    return JaysonTypeOverrideGlobal.TryGetDefaultValue(m_Type, memberName, out defaultValue);
                }
                return false;
            }
            return true;
        }

        public virtual object Clone()
        {
            var result = new JaysonTypeOverride(m_Type, m_BindToType);
            foreach (var iKvp in m_IgnoredMembers)
            {
                result.m_IgnoredMembers.Add(iKvp.Key, iKvp.Value);
            }

            foreach (var aKvp in m_AliasToMemberName)
            {
                result.m_AliasToMemberName.Add(aKvp.Key, aKvp.Value);
            }

            foreach (var aKvp in m_MemberNameToAlias)
            {
                result.m_MemberNameToAlias.Add(aKvp.Key, aKvp.Value);
            }

            foreach (var iKvp in m_MemberDefaultValues)
            {
                result.m_MemberDefaultValues.Add(iKvp.Key, iKvp.Value);
            }

            return result;
        }

        public virtual JaysonTypeOverride Clone(Type insteadType)
        {
            var result = (JaysonTypeOverride)Clone();
            result.m_BindToType = insteadType;
            return result;
        }
    }
}
