using System;
using System.Collections.Generic;

namespace Sweet.Jayson
{
    # region JaysonNullable

    internal sealed class JaysonNullable
    {
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
                JaysonNullable ctor;
                if (!s_Nullables.TryGetValue(type, out ctor)) 
                {
                    ctor = new JaysonNullable(type);
                    s_Nullables[type] = ctor;
                }
                return ctor.m_LambdaCtor(new object[] { value });
            }
            return null;
        }
    }

    # endregion JaysonNullable
}

