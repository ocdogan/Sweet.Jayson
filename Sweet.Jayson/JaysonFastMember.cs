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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Sweet.Jayson
{
    # region JaysonFastMember

    internal abstract class JaysonFastMember : IJaysonFastMember
    {
        protected delegate void ByRefAction(ref object instance, object value);

        protected bool m_Get;
        protected bool m_Set;

        protected bool m_Overriden;

        protected string m_Alias;
        protected string m_Name;

        protected Type m_MemberType;
        protected MemberInfo m_MemberInfo;

        protected bool m_Ignored;

        protected bool m_CanRead;
        protected bool m_CanWrite;

        protected bool m_IsPublic;

        protected bool m_BackingField;
        protected bool m_AnonymousField;

        protected object m_DefaultValue;

        protected JaysonMemberAttribute[] m_MemberAttributes;

#if (NET3500 || NET3000 || NET2000)
        protected bool m_IsValueType;
#endif

        protected ByRefAction m_SetRefDelegate;
        protected Func<object, object> m_GetDelegate;

        public string Alias
        {
            get { return m_Alias; }
        }

        public string Name
        {
            get { return m_Name; }
        }

        public bool AnonymousField
        {
            get { return m_AnonymousField; }
        }

        public bool BackingField
        {
            get { return m_BackingField; }
        }

        public virtual bool CanRead
        {
            get { return m_CanRead; }
        }

        public virtual bool CanWrite
        {
            get { return m_CanWrite; }
        }

        public bool Ignored
        {
            get { return m_Ignored; }
        }

        public bool IsPublic
        {
            get { return m_IsPublic; }
        }

        public bool Overriden
        {
            get { return m_Overriden; }
        }

        public object DefaultValue
        {
            get { return m_DefaultValue; }
        }

        public bool HasDefaultAttribute { get; private set; }

        public Type MemberType
        {
            get { return m_MemberType; }
        }

        public abstract JaysonFastMemberType Type { get; }

        public MemberInfo Info
        {
            get { return m_MemberInfo; }
        }

        public JaysonMemberAttribute[] MemberAttributes
        {
            get { return m_MemberAttributes; }
        }

        public JaysonFastMember(MemberInfo mi, bool initGet = true, bool initSet = true)
        {
            m_MemberInfo = mi;

            m_Name = mi.Name;

            if (mi is PropertyInfo)
            {
                m_MemberType = ((PropertyInfo)mi).PropertyType;
            }
            else if (mi is FieldInfo)
            {
                m_MemberType = ((FieldInfo)mi).FieldType;
            }

#if (NET3500 || NET3000 || NET2000)
            m_IsValueType = mi.DeclaringType.IsValueType;
#endif

            SetMemberAttributes();
            SetDefaultValue();

            Init(initGet, initSet);
        }

        protected virtual void Init(bool initGet, bool initSet)
        {            
            InitCanReadWrite();

            if (initGet)
            {
                m_Get = true;
                InitializeGet();
            }
            if (initSet)
            {
                m_Set = true;
                InitializeSet();
            }
        }

        protected virtual void SetMemberAttributes()
        {
            if (m_MemberInfo != null)
            {
                m_MemberAttributes = m_MemberInfo
#if (NET4000 || NET3500 || NET3000 || NET2000)
                    .GetCustomAttributes(typeof(JaysonMemberAttribute), true)
                    .Cast<JaysonMemberAttribute>()
#else
                    .GetCustomAttributes<JaysonMemberAttribute>(true)
#endif
                    .ToArray();

                if ((m_MemberAttributes != null) && (m_MemberAttributes.Length > 0))
                {
                    if (m_MemberAttributes.Length == 1)
                    {
                        var mAttr = m_MemberAttributes[0];

                        m_Alias = mAttr.Alias;
                        m_Ignored = mAttr.Ignored.HasValue ? mAttr.Ignored.Value : false;

                        m_DefaultValue = mAttr.DefaultValue;
                        HasDefaultAttribute = mAttr.HasDefaultValue;
                    }
                    else
                    {
                        var mAttr = m_MemberAttributes
                            .Where((attr) => (attr.Alias != null))
                            .FirstOrDefault();

                        if (mAttr != null)
                        {
                            m_Alias = mAttr.Alias;
                        }

                        mAttr = m_MemberAttributes
                            .Where((attr) => attr.HasDefaultValue)
                            .FirstOrDefault();

                        if (mAttr != null)
                        {
                            HasDefaultAttribute = true;
                            m_DefaultValue = mAttr.DefaultValue;
                        }

                        mAttr = m_MemberAttributes
                            .Where((attr) => attr.Ignored.HasValue)
                            .FirstOrDefault();

                        m_Ignored = (mAttr != null) ? mAttr.Ignored.Value : false;
                    }

                    if (m_Alias != null)
                    {
                        m_Alias = m_Alias.Trim();
                        if (m_Alias == String.Empty)
                        {
                            m_Alias = null;
                        }
                    }
                }
            }
        }

        protected virtual void SetDefaultValue()
        {
            if ((m_MemberInfo != null) && !HasDefaultAttribute)
            {
                var dAttr = m_MemberInfo
#if (NET4000 || NET3500 || NET3000 || NET2000)
                    .GetCustomAttributes(typeof(DefaultValueAttribute), true)
                    .Cast<DefaultValueAttribute>()
#else
                    .GetCustomAttributes<DefaultValueAttribute>(true)
#endif
                    .FirstOrDefault();

                if (dAttr == null)
                {
                    HasDefaultAttribute = false;
                    m_DefaultValue = (MemberType != null) ? JaysonTypeInfo.GetDefault(MemberType) : null;
                }
                else
                {
                    HasDefaultAttribute = true;
                    m_DefaultValue = ReferenceEquals(dAttr.Value, null) ? JaysonNull.Value : dAttr.Value;
                }
            }
        }

        protected abstract void InitCanReadWrite();

        protected abstract void InitializeGet();

        protected abstract void InitializeSet();

        public virtual object Get(object instance)
        {
            if (m_CanRead)
            {
                if (!m_Get)
                {
                    m_Get = true;
                    InitializeGet();
                }
                return m_GetDelegate(instance);
            }
            return null;
        }

        public virtual void Set(ref object instance, object value)
        {
            if (m_CanWrite)
            {
                if (!m_Set)
                {
                    m_Set = true;
                    InitializeSet();
                }
                if (m_SetRefDelegate != null)
                {
                    m_SetRefDelegate(ref instance, value);
                }
            }
        }
    }

    # endregion JaysonFastMember
}