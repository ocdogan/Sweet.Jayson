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

    internal sealed class JaysonFastField : IJaysonFastMember
    {
        private delegate void ByRefAction(ref object instance, object value);

        private bool m_Get;
        private bool m_Set;

        private Type m_MemberType;
        private FieldInfo m_FieldInfo;

        private bool m_CanRead;
        private bool m_CanWrite;

        private bool m_InvokeOnSet;

        private ByRefAction m_SetDelegate;
        private Func<object, object> m_GetDelegate;

        public JaysonFastMemberType Type
        {
            get { return JaysonFastMemberType.Property; }
        }

        public Type MemberType
        {
            get { return m_MemberType; }
        }

        public bool CanRead
        {
            get { return m_CanRead; }
        }

        public bool CanWrite
        {
            get { return m_CanWrite || m_InvokeOnSet; }
        }

        public JaysonFastField(FieldInfo fi, bool initGet = true, bool initSet = true)
        {
            m_FieldInfo = fi;
            m_MemberType = m_FieldInfo.FieldType;

            m_CanRead = true;
            m_CanWrite = !(m_FieldInfo.IsInitOnly || m_FieldInfo.IsStatic);
            m_InvokeOnSet = m_FieldInfo.IsInitOnly && !m_FieldInfo.IsStatic;

            if (initGet)
            {
                m_Get = true;
                InitializeGet(fi);
            }
            if (initSet)
            {
                m_Set = true;
                InitializeSet(fi);
            }
        }

        private void InitializeGet(FieldInfo fi)
        {
            if (m_CanRead)
            {
                var instance = Expression.Parameter(typeof(object), "instance");

                Type declaringT = fi.DeclaringType;

                UnaryExpression instanceCast = !declaringT.IsValueType ?
                    Expression.TypeAs(instance, declaringT) :
                    Expression.Convert(instance, declaringT);

                Expression fieldExp = Expression.Field(instanceCast, fi);
                Expression toObjectExp = Expression.TypeAs(fieldExp, typeof(object));

                m_GetDelegate = Expression.Lambda<Func<object, object>>(toObjectExp, instance).Compile();
            }
        }

        private void InitializeSet(FieldInfo fi)
        {
            if (m_CanWrite)
            {
                Type declaringT = fi.DeclaringType;

#if (NET3500 || NET3000 || NET2000)
				m_SetDelegate = delegate(ref object instance, object value) {
					m_FieldInfo.SetValue (instance, value);
				};
#else
                var instanceExp = Expression.Parameter(typeof(object).MakeByRefType(), "instance");
                var valueExp = Expression.Parameter(typeof(object), "value");

                // value as T is slightly faster than (T)value, so if it's not a value type, use that
                UnaryExpression instanceCast = !declaringT.IsValueType ?
                    Expression.TypeAs(instanceExp, declaringT) :
                    Expression.Convert(instanceExp, declaringT);

                UnaryExpression valueCast = !m_MemberType.IsValueType ?
                    Expression.TypeAs(valueExp, m_MemberType) :
                    Expression.Convert(valueExp, m_MemberType);

                List<Expression> expBlockList = new List<Expression>();

                var instanceRefExp = Expression.Variable(declaringT, "instanceRef");
                expBlockList.Add(Expression.Assign(instanceRefExp, instanceCast));

                MemberExpression fieldExp = Expression.Field(instanceRefExp, fi);
                BinaryExpression assignExp = Expression.Assign(fieldExp, valueCast);

                expBlockList.Add(assignExp);

                Expression toObjectExp = Expression.Assign(instanceExp,
                    Expression.Convert(instanceRefExp, typeof(object)));
                expBlockList.Add(toObjectExp);

                Expression blockExp = Expression.Block(new[] { instanceRefExp }, expBlockList);

                m_SetDelegate = Expression.Lambda<ByRefAction>(blockExp,
                    new ParameterExpression[] { instanceExp, valueExp }).Compile();
#endif
            }
        }

        public object Get(object instance)
        {
            if (m_CanRead)
            {
                if (!m_Get)
                {
                    m_Get = true;
                    InitializeGet(m_FieldInfo);
                }
                return m_GetDelegate(instance);
            }
            return null;
        }

        public void Set(ref object instance, object value)
        {
            if (m_CanWrite)
            {
                if (!m_Set)
                {
                    m_Set = true;
                    InitializeSet(m_FieldInfo);
                }
                if (m_SetDelegate != null)
                {
                    m_SetDelegate(ref instance, value);
                }
            }
            else if (m_InvokeOnSet)
            {
                m_FieldInfo.SetValue(instance, value);
            }
        }
    }

    # endregion JaysonFastField
}