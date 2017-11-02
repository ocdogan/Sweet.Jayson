using System;
using System.Collections.Generic;

namespace Sweet.Jayson
{
    # region JaysonNullable

    internal sealed class JaysonNullable
    {
        private static readonly object s_NullablesLock = new object();
        private static readonly Dictionary<Type, JaysonNullable> s_Nullables = new Dictionary<Type, JaysonNullable> ();

        private Func<object[], object> m_LambdaCtor;

        private JaysonNullable(Type type)
        {
            var typeArr = new Type[] { type };
            var ctor = typeof(Nullable<>).MakeGenericType(typeArr).GetConstructor(typeArr);

            m_LambdaCtor = JaysonCommon.CreateActivator(ctor);
        }

        public static object New(Type type, object value)
        {
            if (type != null) 
            {
                var contains = false;
                JaysonNullable ctor;
                lock (s_NullablesLock)
                {
                    contains = s_Nullables.TryGetValue(type, out ctor);
                }

                if (!contains)
                {
                    ctor = new JaysonNullable(type);
                    lock (s_NullablesLock)
                    {
                        s_Nullables[type] = ctor;
                    }
                }
                return ctor.m_LambdaCtor(new object[] { value });
            }
            return null;
        }
    }

    # endregion JaysonNullable
}

