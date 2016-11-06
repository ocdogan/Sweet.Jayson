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
using System.Linq.Expressions;
using System.Reflection;

namespace Sweet.Jayson
{
    # region JaysonFastField

    internal sealed class JaysonFastField : JaysonFastMember
    {
        private bool m_InvokeOnSet;

        public override bool CanWrite
        {
            get { return m_CanWrite | m_InvokeOnSet; }
        }

        public override JaysonFastMemberType Type
        {
            get { return JaysonFastMemberType.Field; }
        }

        public JaysonFastField(FieldInfo fi, bool initGet = true, bool initSet = true)
            : base(fi, initGet, initSet)
        { }

        protected override void Init(bool initGet, bool initSet)
        {
            var fi = (FieldInfo)m_MemberInfo;

            m_MemberType = fi.FieldType;
            m_IsPublic = fi.IsPublic && !fi.IsPrivate;

            if (!m_IsPublic && fi.IsPrivate)
            {
                var pos = m_Name.IndexOf('<');
                if (pos > -1)
                {
                    pos = m_Name.IndexOf('>', pos);
                    m_AnonymousField = (m_Name.IndexOf("__Field", pos, StringComparison.OrdinalIgnoreCase) > -1);
                    m_BackingField = (pos > -1) &&
                        (m_AnonymousField || (m_Name.IndexOf("__BackingField", pos, StringComparison.OrdinalIgnoreCase) > -1));
                }
            }

            base.Init(initGet, initSet);
        }

        protected override void InitCanReadWrite()
        {
            var fi = (FieldInfo)m_MemberInfo;
            m_MemberType = fi.FieldType;

            m_CanRead = true;
            m_CanWrite = !(fi.IsInitOnly || fi.IsStatic);

            m_InvokeOnSet = fi.IsInitOnly && !fi.IsStatic;
        }

        protected override void InitializeGet()
        {
            if (m_CanRead)
            {
                var fi = (FieldInfo)m_MemberInfo;
                var instance = Expression.Parameter(typeof(object), "instance");

                var declaringT = fi.DeclaringType;

                var instanceCast = !declaringT.IsValueType ?
                    Expression.TypeAs(instance, declaringT) :
                    Expression.Convert(instance, declaringT);

                var fieldExp = Expression.Field(instanceCast, fi);
                var toObjectExp = Expression.TypeAs(fieldExp, typeof(object));

                var overridingMetod = JaysonCommon.GetOverridingMethod(fi);
                if (overridingMetod != null)
                {
                    m_Overriden = true;
                    var memberName = fi.Name;
                    var memberNameExp = Expression.Constant(memberName, typeof(string));
                    var callExp = Expression.Call(overridingMetod, memberNameExp, toObjectExp);
                    toObjectExp = Expression.TypeAs(callExp, typeof(object));
                }

                var lmd = Expression.Lambda<Func<object, object>>(toObjectExp, instance);
                m_GetDelegate = lmd.Compile();
            }
        }

        protected override void InitializeSet()
        {
            if (m_CanWrite)
            {
                var fi = (FieldInfo)m_MemberInfo;
                Type declaringT = fi.DeclaringType;

#if (NET3500 || NET3000 || NET2000)
				m_SetRefDelegate = delegate(ref object instance, object value) {
					fi.SetValue (instance, value);
				};
#else
                var instanceExp = Expression.Parameter(typeof(object).MakeByRefType(), "instance");
                var valueExp = Expression.Parameter(typeof(object), "value");

                // value as T is slightly faster than (T)value, so if it's not a value type, use that
                var instanceCast = !declaringT.IsValueType ?
                    Expression.TypeAs(instanceExp, declaringT) :
                    Expression.Convert(instanceExp, declaringT);

                var valueCast = !m_MemberType.IsValueType ?
                    Expression.TypeAs(valueExp, m_MemberType) :
                    Expression.Convert(valueExp, m_MemberType);

                var expBlockList = new List<Expression>();

                var instanceRefExp = Expression.Variable(declaringT, "instanceRef");
                expBlockList.Add(Expression.Assign(instanceRefExp, instanceCast));

                var fieldExp = Expression.Field(instanceRefExp, fi);
                var assignExp = Expression.Assign(fieldExp, valueCast);

                expBlockList.Add(assignExp);

                var toObjectExp = Expression.Assign(instanceExp,
                    Expression.Convert(instanceRefExp, typeof(object)));
                expBlockList.Add(toObjectExp);

                var blockExp = Expression.Block(new[] { instanceRefExp }, expBlockList);

                var lmd = Expression.Lambda<ByRefAction>(blockExp, new ParameterExpression[] { instanceExp, valueExp });
                m_SetRefDelegate = lmd.Compile();
#endif
            }
        }

        public override void Set(ref object instance, object value)
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
            else if (m_InvokeOnSet)
            {               
                ((FieldInfo)m_MemberInfo).SetValue(instance, value);
            }
        }
    }

    # endregion JaysonFastField
}