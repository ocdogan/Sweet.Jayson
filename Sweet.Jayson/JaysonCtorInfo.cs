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
	# region JaysonCtorInfo

	internal sealed class JaysonCtorInfo
	{
		private static readonly Dictionary<Type, JaysonCtorInfo> s_CtorInfos = new Dictionary<Type, JaysonCtorInfo>();

		private Func<object[], object> m_LambdaCtor;

		public readonly bool HasCtor;
		public readonly bool HasParam;
		public readonly int ParamLength;

		public readonly ConstructorInfo Ctor;
		public readonly ParameterInfo[] CtorParams;

		private JaysonCtorInfo(ConstructorInfo ctor)
		{
			Ctor = ctor;
			HasCtor = ctor != null;
			CtorParams = !HasCtor ? new ParameterInfo[0] : (ctor.GetParameters () ?? new ParameterInfo[0]);
			ParamLength = CtorParams.Length;
			HasParam = ParamLength > 0;
		}

		private JaysonCtorInfo(ConstructorInfo ctor, ParameterInfo[] ctorParams)
		{
			Ctor = ctor;
			HasCtor = ctor != null;
			CtorParams = !HasCtor ? new ParameterInfo[0] : (ctorParams ?? new ParameterInfo[0]);
			ParamLength = CtorParams.Length;
			HasParam = ParamLength > 0;
		}

		public static JaysonCtorInfo GetDefaultCtorInfo(Type objType)
		{
			JaysonCtorInfo result;
			if (!s_CtorInfos.TryGetValue (objType, out result)) 
			{
				var ctors = objType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
				if (ctors.Length == 0) {
					ctors = objType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
				}

				if (ctors.Length == 0) {
					result = new JaysonCtorInfo (null); 
				} else {
					ParameterInfo[] ctorParams;

					foreach (var ctor in ctors) {
						ctorParams = ctor.GetParameters ();
						if (ctorParams.Length == 0) {
							result = new JaysonCtorInfo (ctor, ctorParams);
							break;
						}
					}

					if (result == null) {
						result = new JaysonCtorInfo(ctors [0]);
					}
				}

				s_CtorInfos[objType] = result;
			}

			return result;
		}

		public object New(object[] args)
		{
			if (m_LambdaCtor == null) {
				m_LambdaCtor = JaysonCommon.CreateActivator (Ctor);
			}
			return m_LambdaCtor(args);
		}
	}

	# endregion JaysonCtorInfo
}

