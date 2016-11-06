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
        protected string m_NameLower;

        protected Type m_MemberType;
        protected MemberInfo m_MemberInfo;

        protected bool m_Ignored;

        protected bool m_CanRead;
        protected bool m_CanWrite;

        protected bool m_IsPublic;

        protected bool m_BackingField;
        protected bool m_AnonymousField;

        protected object m_DefaultValue;

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

        public string NameLower
        {
            get { return m_NameLower; }
        }

        public abstract JaysonFastMemberType Type { get; }

        public Type MemberType
        {
            get { return m_MemberType; }
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

        public bool Ignores
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

        public JaysonFastMember(MemberInfo mi, bool initGet = true, bool initSet = true)
        {
            m_MemberInfo = mi;

            m_Name = mi.Name;
            m_NameLower = m_Name.ToLower(JaysonConstants.InvariantCulture);

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
            Init(initGet, initSet);
        }

        protected virtual void Init(bool initGet, bool initSet)
        {
            SetDefaultValue();
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

        protected override void SetMemberAttributes()
        {
            if (m_MemberInfo != null)
            {
                var ma = m_MemberInfo.GetCustomAttributes(typeof(JaysonMemberAttribute), true).FirstOrDefault() as JaysonMemberAttribute;
                if (ma != null)
                {
                    m_Alias = ma.Alias;
                    m_Ignored = ma.Ignored;
                }
            }
        }

        protected virtual void SetDefaultValue()
        {
            var mAttr = m_MemberInfo
#if (NET4000 || NET3500 || NET3000 || NET2000)
                .GetCustomAttributes(typeof(JaysonMemberAttribute), true)
                .Cast<JaysonMemberAttribute>()
#else
                .GetCustomAttributes<JaysonMemberAttribute>(true)
#endif
                .Where((attr) => !ReferenceEquals(attr.DefaultValue, null))
                .FirstOrDefault();

            if (mAttr != null)
            {
                m_DefaultValue = mAttr.DefaultValue;
            }
            else
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
                    m_DefaultValue = MemberType != null ? JaysonTypeInfo.GetDefault(MemberType) : null;
                }
                else
                {
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