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

namespace Sweet.Jayson
{
    # region JaysonNullable

    internal sealed class JaysonNullable
    {
        #region Static Members

        private static readonly JaysonSynchronizedDictionary<Type, JaysonNullable> s_Nullables = new JaysonSynchronizedDictionary<Type, JaysonNullable>();

        #endregion Static Members

        #region Field Members

        private Func<object[], object> m_LambdaCtor;

        #endregion Field Members

        #region .Ctors

        private JaysonNullable(Type type)
        {
            var typeArr = new Type[] { type };
            var ctor = typeof(Nullable<>).MakeGenericType(typeArr).GetConstructor(typeArr);

            m_LambdaCtor = JaysonCommon.CreateActivator(ctor);
        }

        #endregion .Ctors

        #region Methods

        public static object New(Type type, object value)
        {
            if (type != null) 
            {
                var ctor = s_Nullables.GetValueOrUpdate(type, (t) => new JaysonNullable(t));
                return ctor.m_LambdaCtor(new object[] { value });
            }
            return null;
        }

        #endregion Methods
    }

    # endregion JaysonNullable
}

