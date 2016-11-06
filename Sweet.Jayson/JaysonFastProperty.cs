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
using System.Linq.Expressions;
using System.Reflection;

namespace Sweet.Jayson
{
    # region JaysonFastProperty

    internal sealed class JaysonFastProperty : JaysonFastMember
    {
#if (NET3500 || NET3000 || NET2000)
		private Action<object, object> m_SetDelegate;
#endif

        private MethodInfo m_GetMethod;
        private MethodInfo m_SetMethod;

        public override JaysonFastMemberType Type
        {
            get { return JaysonFastMemberType.Property; }
        }

        public JaysonFastProperty(PropertyInfo pi, bool initGet = true, bool initSet = true)
            : base(pi, initGet, initSet)
        { }

        protected override void Init(bool initGet, bool initSet)
        {
            var pi = (PropertyInfo)m_MemberInfo;
            
            m_MemberType = pi.PropertyType;
            m_IsPublic = (pi.CanRead && (pi.GetGetMethod() != null)) ||
                (!pi.CanRead && pi.CanWrite && (pi.GetSetMethod() != null));

            base.Init(initGet, initSet);
        }

        protected override void InitCanReadWrite()
        {
            var pi = (PropertyInfo)m_MemberInfo;

            m_CanRead = pi.CanRead;
            if (m_CanRead)
            {
                m_GetMethod = pi.GetGetMethod(true);
            }

            m_CanWrite = pi.CanWrite;
            if (m_CanWrite)
            {
                m_SetMethod = pi.GetSetMethod(true);
            }
        }

        protected override void InitializeGet()
        {
            var pi = (PropertyInfo)m_MemberInfo;
            if (m_GetMethod != null)
            {
                var instanceVar = Expression.Parameter(typeof(object), "instance");

                var declaringT = pi.DeclaringType;
                var instanceCast = !declaringT.IsValueType ?
                    Expression.TypeAs(instanceVar, declaringT) :
                    Expression.Convert(instanceVar, declaringT);

                var callExp = Expression.Call(instanceCast, m_GetMethod);
                var toObjectExp = Expression.TypeAs(callExp, typeof(object));

                var overridingMetod = JaysonCommon.GetOverridingMethod(pi);
                if (overridingMetod != null)
                {
                    m_Overriden = true;
                    var memberName = pi.Name;
                    var memberNameExp = Expression.Constant(memberName, typeof(string));
                    callExp = Expression.Call(overridingMetod, memberNameExp, toObjectExp);
                    toObjectExp = Expression.TypeAs(callExp, typeof(object));
                }

                var lmd = Expression.Lambda<Func<object, object>>(toObjectExp, instanceVar);
                m_GetDelegate = lmd.Compile();
            }
        }

        protected override void InitializeSet()
        {
            var pi = (PropertyInfo)m_MemberInfo;
            if (m_SetMethod != null)
            {
                var declaringT = pi.DeclaringType;

#if (NET3500 || NET3000 || NET2000)
				if (declaringT.IsValueType) {
					m_SetRefDelegate = delegate(ref object instance, object value) {
						pi.SetValue (instance, value, null);
					};
				} else {
					var instanceExp = Expression.Parameter (typeof(object), "instance");
					var valueExp = Expression.Parameter (typeof(object), "value");

					// value as T is slightly faster than (T)value, so if it's not a value type, use that
					var instanceCast = Expression.TypeAs (instanceExp, declaringT);

					var valueCast = !m_MemberType.IsValueType ?
					    Expression.TypeAs (valueExp, m_MemberType) : 
					    Expression.Convert (valueExp, m_MemberType);

					var callExp = Expression.Call(instanceCast, m_SetMethod, valueCast);

					m_SetDelegate = Expression.Lambda<Action<object, object>> (callExp, 
						new ParameterExpression[] { instanceExp, valueExp }).Compile ();
				}
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

                var propertyExp = Expression.Property(instanceRefExp, pi);
                var assignExp = Expression.Assign(propertyExp, valueCast);

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
#if (NET3500 || NET3000 || NET2000)
                if (m_IsValueType) {
                    if (m_SetRefDelegate != null) {
                        m_SetRefDelegate(ref instance, value);
                    }
                } else if (m_SetDelegate != null) {
                    m_SetDelegate(instance, value);
                }
#else
                if (m_SetRefDelegate != null)
                {
                    m_SetRefDelegate(ref instance, value);
                }
#endif
            }
        }
    }

    # endregion JaysonFastProperty
}