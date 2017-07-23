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
using System.Text;

namespace Sweet.Jayson
{
    public class JaysonSerDeserSettings
    {
        # region Private Field Members

        protected object m_TypeOverrideLock = new object();
        protected Dictionary<Type, JaysonTypeOverride> m_TypeOverrides;

        protected readonly List<Func<Type, string, MemberInfo, bool>> m_IgnoreMembers = new List<Func<Type, string, MemberInfo, bool>>();
        protected readonly List<Func<Type, string, MemberInfo, object, bool>> m_IgnoreMemberValues = new List<Func<Type, string, MemberInfo, object, bool>>();

        # endregion Private Field Members

        public JaysonSerDeserSettings()
            : this(null)
        { }

        public JaysonSerDeserSettings(JaysonTypeOverride[] typeOverrides)
        {
            InitTypeOverrides(typeOverrides);
        }

        # region Member ignore

        public virtual void IgnoreMember(Func<Type, string, MemberInfo, bool> f)
        {
            if ((f != null) && !m_IgnoreMembers.Contains(f))
            {
                m_IgnoreMembers.Add(f);
            }
        }

        public virtual void IncludeMember(Func<Type, string, MemberInfo, bool> f)
        {
            if (f != null)
            {
                m_IgnoreMembers.Remove(f);
            }
        }

        public virtual void IgnoreMember(Func<Type, string, MemberInfo, object, bool> f)
        {
            if ((f != null) && !m_IgnoreMemberValues.Contains(f))
            {
                m_IgnoreMemberValues.Add(f);
            }
        }

        public virtual void IncludeMember(Func<Type, string, MemberInfo, object, bool> f)
        {
            if (f != null)
            {
                m_IgnoreMemberValues.Remove(f);
            }
        }

        public virtual bool IsMemberIgnored(Type type, string memberName)
        {
            if (m_IgnoreMembers.Count > 0)
            {
                var cache = JaysonFastMemberCache.GetCache(type);
                if (cache != null)
                {
                    var member = cache.GetAnyMember(memberName, false);
                    MemberInfo info = (member != null) ? member.Info : null;

                    foreach (var f in m_IgnoreMembers)
                    {
                        try
                        {
                            if (f(type, memberName, info))
                            {
                                return true;
                            }
                        }
                        catch (Exception)
                        { }
                    }
                }
            }
            return false;
        }

        public virtual bool IsMemberIgnored(Type type, string memberName, object value)
        {
            if ((m_IgnoreMembers.Count > 0) || (m_IgnoreMemberValues.Count > 0))
            {
                var cache = JaysonFastMemberCache.GetCache(type);
                if (cache != null)
                {
                    var member = cache.GetAnyMember(memberName, false);
                    MemberInfo info = (member != null) ? member.Info : null;

                    if (m_IgnoreMemberValues.Count > 0)
                    {
                        foreach (var f in m_IgnoreMemberValues)
                        {
                            try
                            {
                                if (f(type, memberName, info, value))
                                {
                                    return true;
                                }
                            }
                            catch (Exception)
                            { }
                        }
                    }

                    if (m_IgnoreMembers.Count > 0)
                    {
                        foreach (var f in m_IgnoreMembers)
                        {
                            try
                            {
                                if (f(type, memberName, info))
                                {
                                    return true;
                                }
                            }
                            catch (Exception)
                            { }
                        }
                    }
                }
            }
            return false;
        }

        # endregion Member ignore

        # region Assign

        protected internal virtual void AssignTo(JaysonSerDeserSettings destination)
        {
            AssignTypeOverridesTo(destination);

            destination.m_IgnoreMembers.Clear();
            if (m_IgnoreMembers.Count > 0)
            {
                destination.m_IgnoreMembers.AddRange(m_IgnoreMembers);
            }

            destination.m_IgnoreMemberValues.Clear();
            if (m_IgnoreMemberValues.Count > 0)
            {
                destination.m_IgnoreMemberValues.AddRange(m_IgnoreMemberValues);
            }
        }

        # endregion Assign

        # region Type overrides

        protected void InitTypeOverrides(JaysonTypeOverride[] typeOverrides)
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

        protected void AssignTypeOverridesTo(JaysonSerDeserSettings destination)
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
                    if (m_TypeOverrides.TryGetValue(type, out result))
                    {
                        return result;
                    }
                    type = type.BaseType;
                }
                return JaysonTypeOverrideGlobal.GetTypeOverride(type);
            }
            return null;
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
            var typeOverride = AddTypeOverride(typeof(T));
            if (typeOverride != null)
            {
                typeOverride.BindToType = bindToType;
            }
            return typeOverride;
        }

        public JaysonTypeOverride AddTypeOverride(Type type, Type bindToType)
        {
            var typeOverride = AddTypeOverride(type);
            if (typeOverride != null)
            {
                typeOverride.BindToType = bindToType;
            }
            return typeOverride;
        }

        public JaysonSerDeserSettings AddTypeOverride(JaysonTypeOverride typeOverride)
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

        public JaysonSerDeserSettings AddTypeOverrides(JaysonTypeOverride[] typeOverrides)
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

        public JaysonSerDeserSettings RemoveTypeOverride(Type type)
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
        
        # endregion Type overrides
    }
}
