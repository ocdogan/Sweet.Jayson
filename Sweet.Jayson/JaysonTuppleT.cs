using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Sweet.Jayson
{
    [Serializable]
    public sealed class JaysonTuple<T1> : IComparable
    {
        private readonly T1 m_Item1;

        public T1 Item1 { get { return m_Item1; } }

        public JaysonTuple(T1 item1)
        {
            m_Item1 = item1;
        }

        public override Boolean Equals(Object obj)
        {
            if (obj == null) { return false; }

            var objTuple = obj as JaysonTuple<T1>;

            if (objTuple == null) {
                return false;
            }

            return EqualityComparer<Object>.Default.Equals(m_Item1, objTuple.m_Item1);
        }

        Int32 IComparable.CompareTo(Object obj)
        {
            if (obj == null) { return 1; }

            var objTuple = obj as JaysonTuple<T1>;

            if (objTuple == null) {
                throw new ArgumentException(String.Format (JaysonError.GivenTypeIsIncorrect, GetType ().ToString()), "obj");
            }

            return Comparer<Object>.Default.Compare(m_Item1, objTuple.m_Item1);
        }

        public override int GetHashCode ()
        {
            return EqualityComparer<Object>.Default.GetHashCode (m_Item1);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("(");
            sb.Append(m_Item1);
            sb.Append(")");
            return sb.ToString();
        }
    }

    [Serializable]
    public sealed class JaysonTuple<T1, T2> : IComparable
    {
        private readonly T1 m_Item1;
        private readonly T2 m_Item2;

        public T1 Item1 { get { return m_Item1; } }
        public T2 Item2 { get { return m_Item2; } }

        public JaysonTuple(T1 item1, T2 item2)
        {
            m_Item1 = item1;
            m_Item2 = item2;
        }

        public override Boolean Equals(Object obj)
        {
            if (obj == null) { return false; }

            var objTuple = obj as JaysonTuple<T1, T2>;
            if (objTuple == null) {
                return false;
            }

            var eComparer = EqualityComparer<Object>.Default;

            return eComparer.Equals(m_Item1, objTuple.m_Item1) &&
                eComparer.Equals(m_Item2, objTuple.m_Item2);
        }

        Int32 IComparable.CompareTo(Object obj)
        {
            if (obj == null) { return 1; }

            var objTuple = obj as JaysonTuple<T1, T2>;
            if (objTuple == null) {
                throw new ArgumentException(String.Format (JaysonError.GivenTypeIsIncorrect, GetType ().ToString()), "obj");
            }

            var comparer = Comparer<Object>.Default;

            var c = comparer.Compare(m_Item1, objTuple.m_Item1);
            if (c != 0) { return c; }

            return comparer.Compare(m_Item2, objTuple.m_Item2);
        }

        public override int GetHashCode ()
        {
            var eComparer = EqualityComparer<Object>.Default;

            return JaysonTuple.CombineHashCodes(eComparer.GetHashCode (m_Item1),
                eComparer.GetHashCode (m_Item2));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("(");
            sb.Append(m_Item1);
            sb.Append(", ");
            sb.Append(m_Item2);
            sb.Append(")");
            return sb.ToString();
        }
    }

    [Serializable]
    public sealed class JaysonTuple<T1, T2, T3> : IComparable
    {
        private readonly T1 m_Item1;
        private readonly T2 m_Item2;
        private readonly T3 m_Item3;

        public T1 Item1 { get { return m_Item1; } }
        public T2 Item2 { get { return m_Item2; } }
        public T3 Item3 { get { return m_Item3; } }

        public JaysonTuple(T1 item1, T2 item2, T3 item3)
        {
            m_Item1 = item1;
            m_Item2 = item2;
            m_Item3 = item3;
        }

        public override Boolean Equals(Object obj)
        {
            if (obj == null) { return false; }

            var objTuple = obj as JaysonTuple<T1, T2, T3>;
            if (objTuple == null) {
                return false;
            }

            var eComparer = EqualityComparer<Object>.Default;

            return eComparer.Equals(m_Item1, objTuple.m_Item1) &&
                eComparer.Equals(m_Item2, objTuple.m_Item2) &&
                eComparer.Equals(m_Item3, objTuple.m_Item3);
        }

        Int32 IComparable.CompareTo(Object obj)
        {
            if (obj == null) { return 1; }

            var objTuple = obj as JaysonTuple<T1, T2, T3>;
            if (objTuple == null) {
                throw new ArgumentException(String.Format (JaysonError.GivenTypeIsIncorrect, GetType ().ToString()), "obj");
            }

            var comparer = Comparer<Object>.Default;

            var c = comparer.Compare(m_Item1, objTuple.m_Item1);
            if (c != 0) { return c; }

            c = comparer.Compare(m_Item2, objTuple.m_Item2);
            if (c != 0) { return c; }

            return comparer.Compare(m_Item3, objTuple.m_Item3);
        }

        public override int GetHashCode ()
        {
            var eComparer = EqualityComparer<Object>.Default;

            return JaysonTuple.CombineHashCodes(eComparer.GetHashCode (m_Item1),
                eComparer.GetHashCode (m_Item2),
                eComparer.GetHashCode (m_Item3));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("(");
            sb.Append(m_Item1);
            sb.Append(", ");
            sb.Append(m_Item2);
            sb.Append(", ");
            sb.Append(m_Item3);
            sb.Append(")");
            return sb.ToString();
        }
    }

    [Serializable]
    public sealed class JaysonTuple<T1, T2, T3, T4> : IComparable
    {
        private readonly T1 m_Item1;
        private readonly T2 m_Item2;
        private readonly T3 m_Item3;
        private readonly T4 m_Item4;

        public T1 Item1 { get { return m_Item1; } }
        public T2 Item2 { get { return m_Item2; } }
        public T3 Item3 { get { return m_Item3; } }
        public T4 Item4 { get { return m_Item4; } }

        public JaysonTuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            m_Item1 = item1;
            m_Item2 = item2;
            m_Item3 = item3;
            m_Item4 = item4;
        }

        public override Boolean Equals(Object obj)
        {
            if (obj == null) { return false; }

            var objTuple = obj as JaysonTuple<T1, T2, T3, T4>;
            if (objTuple == null) {
                return false;
            }

            var eComparer = EqualityComparer<Object>.Default;

            return eComparer.Equals(m_Item1, objTuple.m_Item1) &&
                eComparer.Equals(m_Item2, objTuple.m_Item2) &&
                eComparer.Equals(m_Item3, objTuple.m_Item3) &&
                eComparer.Equals(m_Item4, objTuple.m_Item4);
        }

        Int32 IComparable.CompareTo(Object obj)
        {
            if (obj == null) { return 1; }

            var objTuple = obj as JaysonTuple<T1, T2, T3, T4>;
            if (objTuple == null) {
                throw new ArgumentException(String.Format (JaysonError.GivenTypeIsIncorrect, GetType ().ToString()), "obj");
            }

            var comparer = Comparer<Object>.Default;

            var c = comparer.Compare(m_Item1, objTuple.m_Item1);
            if (c != 0) { return c; }

            c = comparer.Compare(m_Item2, objTuple.m_Item2);
            if (c != 0) { return c; }

            c = comparer.Compare(m_Item3, objTuple.m_Item3);
            if (c != 0) { return c; }

            return comparer.Compare(m_Item4, objTuple.m_Item4);
        }

        public override int GetHashCode ()
        {
            var eComparer = EqualityComparer<Object>.Default;

            return JaysonTuple.CombineHashCodes(eComparer.GetHashCode (m_Item1),
                eComparer.GetHashCode (m_Item2),
                eComparer.GetHashCode (m_Item3),
                eComparer.GetHashCode (m_Item4));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("(");
            sb.Append(m_Item1);
            sb.Append(", ");
            sb.Append(m_Item2);
            sb.Append(", ");
            sb.Append(m_Item3);
            sb.Append(", ");
            sb.Append(m_Item4);
            sb.Append(")");
            return sb.ToString();
        }
    }

    [Serializable]
    public sealed class JaysonTuple<T1, T2, T3, T4, T5> : IComparable
    {
        private readonly T1 m_Item1;
        private readonly T2 m_Item2;
        private readonly T3 m_Item3;
        private readonly T4 m_Item4;
        private readonly T5 m_Item5;

        public T1 Item1 { get { return m_Item1; } }
        public T2 Item2 { get { return m_Item2; } }
        public T3 Item3 { get { return m_Item3; } }
        public T4 Item4 { get { return m_Item4; } }
        public T5 Item5 { get { return m_Item5; } }

        public JaysonTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            m_Item1 = item1;
            m_Item2 = item2;
            m_Item3 = item3;
            m_Item4 = item4;
            m_Item5 = item5;
        }

        public override Boolean Equals(Object obj)
        {
            if (obj == null) { return false; }

            var objTuple = obj as JaysonTuple<T1, T2, T3, T4, T5>;
            if (objTuple == null) {
                return false;
            }

            var eComparer = EqualityComparer<Object>.Default;

            return eComparer.Equals(m_Item1, objTuple.m_Item1) &&
                eComparer.Equals(m_Item2, objTuple.m_Item2) &&
                eComparer.Equals(m_Item3, objTuple.m_Item3) &&
                eComparer.Equals(m_Item4, objTuple.m_Item4) &&
                eComparer.Equals(m_Item5, objTuple.m_Item5);
        }

        Int32 IComparable.CompareTo(Object obj)
        {
            if (obj == null) { return 1; }

            var objTuple = obj as JaysonTuple<T1, T2, T3, T4, T5>;

            if (objTuple == null) {
                throw new ArgumentException(String.Format (JaysonError.GivenTypeIsIncorrect, GetType ().ToString()), "obj");
            }

            var comparer = Comparer<Object>.Default;

            var c = comparer.Compare(m_Item1, objTuple.m_Item1);
            if (c != 0) { return c; }

            c = comparer.Compare(m_Item2, objTuple.m_Item2);
            if (c != 0) { return c; }

            c = comparer.Compare(m_Item3, objTuple.m_Item3);
            if (c != 0) { return c; }

            c = comparer.Compare(m_Item4, objTuple.m_Item4);
            if (c != 0) { return c; }

            return comparer.Compare(m_Item5, objTuple.m_Item5);
        }

        public override int GetHashCode ()
        {
            var eComparer = EqualityComparer<Object>.Default;

            return JaysonTuple.CombineHashCodes(eComparer.GetHashCode (m_Item1),
                eComparer.GetHashCode (m_Item2),
                eComparer.GetHashCode (m_Item3),
                eComparer.GetHashCode (m_Item4),
                eComparer.GetHashCode (m_Item5));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("(");
            sb.Append(m_Item1);
            sb.Append(", ");
            sb.Append(m_Item2);
            sb.Append(", ");
            sb.Append(m_Item3);
            sb.Append(", ");
            sb.Append(m_Item4);
            sb.Append(", ");
            sb.Append(m_Item5);
            sb.Append(")");
            return sb.ToString();
        }
    }

    [Serializable]
    public sealed class JaysonTuple<T1, T2, T3, T4, T5, T6> : IComparable
    {
        private readonly T1 m_Item1;
        private readonly T2 m_Item2;
        private readonly T3 m_Item3;
        private readonly T4 m_Item4;
        private readonly T5 m_Item5;
        private readonly T6 m_Item6;

        public T1 Item1 { get { return m_Item1; } }
        public T2 Item2 { get { return m_Item2; } }
        public T3 Item3 { get { return m_Item3; } }
        public T4 Item4 { get { return m_Item4; } }
        public T5 Item5 { get { return m_Item5; } }
        public T6 Item6 { get { return m_Item6; } }

        public JaysonTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            m_Item1 = item1;
            m_Item2 = item2;
            m_Item3 = item3;
            m_Item4 = item4;
            m_Item5 = item5;
            m_Item6 = item6;
        }

        public override Boolean Equals(Object obj)
        {
            if (obj == null) { return false; }

            var objTuple = obj as JaysonTuple<T1, T2, T3, T4, T5, T6>;
            if (objTuple == null) {
                return false;
            }

            var eComparer = EqualityComparer<Object>.Default;

            return eComparer.Equals(m_Item1, objTuple.m_Item1) &&
                eComparer.Equals(m_Item2, objTuple.m_Item2) &&
                eComparer.Equals(m_Item3, objTuple.m_Item3) &&
                eComparer.Equals(m_Item4, objTuple.m_Item4) &&
                eComparer.Equals(m_Item5, objTuple.m_Item5) &&
                eComparer.Equals(m_Item6, objTuple.m_Item6);
        }

        Int32 IComparable.CompareTo(Object obj)
        {
            if (obj == null) { return 1; }

            var objTuple = obj as JaysonTuple<T1, T2, T3, T4, T5, T6>;
            if (objTuple == null) {
                throw new ArgumentException(String.Format (JaysonError.GivenTypeIsIncorrect, GetType ().ToString()), "obj");
            }

            var comparer = Comparer<Object>.Default;

            var c = comparer.Compare(m_Item1, objTuple.m_Item1);
            if (c != 0) { return c; }

            c = comparer.Compare(m_Item2, objTuple.m_Item2);
            if (c != 0) { return c; }

            c = comparer.Compare(m_Item3, objTuple.m_Item3);
            if (c != 0) { return c; }

            c = comparer.Compare(m_Item4, objTuple.m_Item4);
            if (c != 0) { return c; }

            c = comparer.Compare(m_Item5, objTuple.m_Item5);
            if (c != 0) { return c; }

            return comparer.Compare(m_Item6, objTuple.m_Item6);
        }

        public override int GetHashCode ()
        {
            var eComparer = EqualityComparer<Object>.Default;

            return JaysonTuple.CombineHashCodes(eComparer.GetHashCode (m_Item1),
                eComparer.GetHashCode (m_Item2),
                eComparer.GetHashCode (m_Item3),
                eComparer.GetHashCode (m_Item4),
                eComparer.GetHashCode (m_Item5),
                eComparer.GetHashCode (m_Item6));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("(");
            sb.Append(m_Item1);
            sb.Append(", ");
            sb.Append(m_Item2);
            sb.Append(", ");
            sb.Append(m_Item3);
            sb.Append(", ");
            sb.Append(m_Item4);
            sb.Append(", ");
            sb.Append(m_Item5);
            sb.Append(", ");
            sb.Append(m_Item6);
            sb.Append(")");
            return sb.ToString();
        }
    }

    [Serializable]
    public sealed class JaysonTuple<T1, T2, T3, T4, T5, T6, T7> : IComparable
    {
        private readonly T1 m_Item1;
        private readonly T2 m_Item2;
        private readonly T3 m_Item3;
        private readonly T4 m_Item4;
        private readonly T5 m_Item5;
        private readonly T6 m_Item6;
        private readonly T7 m_Item7;

        public T1 Item1 { get { return m_Item1; } }
        public T2 Item2 { get { return m_Item2; } }
        public T3 Item3 { get { return m_Item3; } }
        public T4 Item4 { get { return m_Item4; } }
        public T5 Item5 { get { return m_Item5; } }
        public T6 Item6 { get { return m_Item6; } }
        public T7 Item7 { get { return m_Item7; } }

        public JaysonTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
        {
            m_Item1 = item1;
            m_Item2 = item2;
            m_Item3 = item3;
            m_Item4 = item4;
            m_Item5 = item5;
            m_Item6 = item6;
            m_Item7 = item7;
        }

        public override Boolean Equals(Object obj)
        {
            if (obj == null) { return false; }

            var objTuple = obj as JaysonTuple<T1, T2, T3, T4, T5, T6, T7>;
            if (objTuple == null) {
                return false;
            }

            var eComparer = EqualityComparer<Object>.Default;

            return eComparer.Equals(m_Item1, objTuple.m_Item1) &&
                eComparer.Equals(m_Item2, objTuple.m_Item2) &&
                eComparer.Equals(m_Item3, objTuple.m_Item3) &&
                eComparer.Equals(m_Item4, objTuple.m_Item4) &&
                eComparer.Equals(m_Item5, objTuple.m_Item5) &&
                eComparer.Equals(m_Item6, objTuple.m_Item6) &&
                eComparer.Equals(m_Item7, objTuple.m_Item7);
        }

        Int32 IComparable.CompareTo(Object obj)
        {
            if (obj == null) { return 1; }

            var objTuple = obj as JaysonTuple<T1, T2, T3, T4, T5, T6, T7>;
            if (objTuple == null) {
                throw new ArgumentException(String.Format (JaysonError.GivenTypeIsIncorrect, GetType ().ToString()), "obj");
            }

            var comparer = Comparer<Object>.Default;

            var c = comparer.Compare(m_Item1, objTuple.m_Item1);
            if (c != 0) { return c; }

            c = comparer.Compare(m_Item2, objTuple.m_Item2);
            if (c != 0) { return c; }

            c = comparer.Compare(m_Item3, objTuple.m_Item3);
            if (c != 0) { return c; }

            c = comparer.Compare(m_Item4, objTuple.m_Item4);
            if (c != 0) { return c; }

            c = comparer.Compare(m_Item5, objTuple.m_Item5);
            if (c != 0) { return c; }

            c = comparer.Compare(m_Item6, objTuple.m_Item6);
            if (c != 0) { return c; }

            return comparer.Compare(m_Item7, objTuple.m_Item7);
        }

        public override int GetHashCode ()
        {
            var eComparer = EqualityComparer<Object>.Default;

            return JaysonTuple.CombineHashCodes(eComparer.GetHashCode (m_Item1),
                eComparer.GetHashCode (m_Item2),
                eComparer.GetHashCode (m_Item3),
                eComparer.GetHashCode (m_Item4),
                eComparer.GetHashCode (m_Item5),
                eComparer.GetHashCode (m_Item6),
                eComparer.GetHashCode (m_Item7));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("(");
            sb.Append(m_Item1);
            sb.Append(", ");
            sb.Append(m_Item2);
            sb.Append(", ");
            sb.Append(m_Item3);
            sb.Append(", ");
            sb.Append(m_Item4);
            sb.Append(", ");
            sb.Append(m_Item5);
            sb.Append(", ");
            sb.Append(m_Item6);
            sb.Append(", ");
            sb.Append(m_Item7);
            sb.Append(")");
            return sb.ToString();
        }
    }
}