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
using System.Linq.Expressions;
using System.Reflection;

namespace Sweet.Jayson
{
	# region JaysonFastProperty

    internal sealed class JaysonFastProperty : IJaysonFastMember
	{
		private bool m_Get;
		private bool m_Set;

		private Type m_MemberType;
		private PropertyInfo m_PropInfo;

		private bool m_CanRead;
		private bool m_CanWrite;

		private Func<object, object> m_GetDelegate;
		private Action<object, object> m_SetDelegate;

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
			get { return m_CanWrite; }
		}

		public JaysonFastProperty(PropertyInfo pi, bool initGet = true, bool initSet = true)
		{
			m_PropInfo = pi;
			m_MemberType = m_PropInfo.PropertyType;

			m_CanRead = m_PropInfo.CanRead;
			m_CanWrite = m_PropInfo.CanWrite;

			if (initGet)
			{
				m_Get = true;
				InitializeGet(pi);
			}

			if (initSet)
			{
				m_Set = true;
				InitializeSet(pi);
			}
		}

		private void InitializeGet(PropertyInfo pi)
		{
			MethodInfo getMethod = pi.GetGetMethod();
			if (getMethod != null)
			{
				var instanceVar = Expression.Parameter(typeof(object), "instance");

				Type declaringT = pi.DeclaringType;
				UnaryExpression instanceCast = !declaringT.IsValueType ?
					Expression.TypeAs(instanceVar, declaringT) : Expression.Convert(instanceVar, declaringT);

				m_GetDelegate = Expression.Lambda<Func<object, object>>(Expression.TypeAs(Expression.Call(instanceCast,
					getMethod), typeof(object)), instanceVar).Compile();
			}
		}

		private void InitializeSet(PropertyInfo pi)
		{
			MethodInfo setMethod = pi.GetSetMethod();
			if (setMethod != null)
			{
				var instanceVar = Expression.Parameter(typeof(object), "instance");
				var valueParam = Expression.Parameter(typeof(object), "value");

				// value as T is slightly faster than (T)value, so if it's not a value type, use that
				Type declaringT = pi.DeclaringType;
				UnaryExpression instanceCast = !declaringT.IsValueType ?
					Expression.TypeAs(instanceVar, declaringT) : Expression.Convert(instanceVar, declaringT);

				UnaryExpression valueCast = (!pi.PropertyType.IsValueType) ?
					Expression.TypeAs(valueParam, pi.PropertyType) : Expression.Convert(valueParam, pi.PropertyType);

				m_SetDelegate = Expression.Lambda<Action<object, object>>(Expression.Call(instanceCast,
					setMethod, valueCast), new ParameterExpression[] { instanceVar, valueParam }).Compile();
			}
		}

		public object Get(object instance)
		{
			if (m_CanRead)
			{
				if (!m_Get)
				{
					m_Get = true;
					InitializeGet(m_PropInfo);
				}
				return m_GetDelegate(instance);
			}
			return null;
		}

		public void Set(object instance, object value)
		{
			if (m_CanWrite)
			{
				if (!m_Set)
				{
					m_Set = true;
					InitializeSet(m_PropInfo);
				}
				if (m_SetDelegate != null)
				{
					m_SetDelegate(instance, value);
				}
			}
		}
	}

	# endregion JaysonFastProperty
}