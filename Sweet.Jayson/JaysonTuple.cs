using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Sweet.Jayson
{
    public static class JaysonTuple
    {
        public static JaysonTuple<T1> Create<T1>(T1 item1)
        {
            return new JaysonTuple<T1>(item1);
        }

        public static JaysonTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new JaysonTuple<T1, T2>(item1, item2);
        }

        public static JaysonTuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
        {
            return new JaysonTuple<T1, T2, T3>(item1, item2, item3);
        }

        public static JaysonTuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            return new JaysonTuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }

        public static JaysonTuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            return new JaysonTuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        }

        public static JaysonTuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            return new JaysonTuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
        }

        public static JaysonTuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
        {
            return new JaysonTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
        }

        internal static int CombineHashCodes(int h1, int h2)
        {
            return (((h1 << 5) + h1) ^ h2);
        }

        internal static int CombineHashCodes(int h1, int h2, int h3)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2), h3);
        }

        internal static int CombineHashCodes(int h1, int h2, int h3, int h4)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2), CombineHashCodes(h3, h4));
        }

        internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), h5);
        }

        internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), CombineHashCodes(h5, h6));
        }

        internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6, int h7)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), CombineHashCodes(h5, h6, h7));
        }

        internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6, int h7, int h8)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), CombineHashCodes(h5, h6, h7, h8));
        }
    }

    [Serializable]
    public class JaysonTuple<T1> : IComparable
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

            JaysonTuple<T1> objTuple = obj as JaysonTuple<T1>;

            if (objTuple == null)
            {
                return false;
            }

            return EqualityComparer<Object>.Default.Equals(m_Item1, objTuple.m_Item1);
        }

        Int32 IComparable.CompareTo(Object obj)
        {
            if (obj == null) { return 1; }

            JaysonTuple<T1> objTuple = obj as JaysonTuple<T1>;

            if (objTuple == null)
            {
                throw new ArgumentException(String.Format("Given type is incorrect format to compare '{0}' with.", GetType().ToString()), "obj");
            }

            return Comparer<Object>.Default.Compare(m_Item1, objTuple.m_Item1);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<Object>.Default.GetHashCode(m_Item1);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            sb.Append(m_Item1);
            sb.Append(")");
            return sb.ToString();
        }
    }

    [Serializable]
    public class JaysonTuple<T1, T2> : IComparable
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
            if (obj == null)  {  return false;  }

            JaysonTuple<T1, T2> objTuple = obj as JaysonTuple<T1, T2>;
            if (objTuple == null)
            {
                return false;
            }

            return EqualityComparer<Object>.Default.Equals(m_Item1, objTuple.m_Item1) &&
                EqualityComparer<Object>.Default.Equals(m_Item2, objTuple.m_Item2);
        }

        Int32 IComparable.CompareTo(Object obj)
        {
            if (obj == null) { return 1; }

            JaysonTuple<T1, T2> objTuple = obj as JaysonTuple<T1, T2>;
            if (objTuple == null)
            {
                throw new ArgumentException(String.Format("Given type is incorrect format to compare '{0}' with.", GetType().ToString()), "obj");
            }

            int c = Comparer<Object>.Default.Compare(m_Item1, objTuple.m_Item1);
            if (c != 0) { return c; }

            return Comparer<Object>.Default.Compare(m_Item2, objTuple.m_Item2);
        }

        public override int GetHashCode()
        {
            return JaysonTuple.CombineHashCodes(EqualityComparer<Object>.Default.GetHashCode(m_Item1),
                EqualityComparer<Object>.Default.GetHashCode(m_Item2));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            sb.Append(m_Item1);
            sb.Append(", ");
            sb.Append(m_Item2);
            sb.Append(")");
            return sb.ToString();
        }
    }

    [Serializable]
    public class JaysonTuple<T1, T2, T3> : IComparable
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

            JaysonTuple<T1, T2, T3> objTuple = obj as JaysonTuple<T1, T2, T3>;
            if (objTuple == null)
            {
                return false;
            }

            return EqualityComparer<Object>.Default.Equals(m_Item1, objTuple.m_Item1) &&
                EqualityComparer<Object>.Default.Equals(m_Item2, objTuple.m_Item2) &&
                EqualityComparer<Object>.Default.Equals(m_Item3, objTuple.m_Item3);
        }

        Int32 IComparable.CompareTo(Object obj)
        {
            if (obj == null) { return 1; }

            JaysonTuple<T1, T2, T3> objTuple = obj as JaysonTuple<T1, T2, T3>;
            if (objTuple == null)
            {
                throw new ArgumentException(String.Format("Given type is incorrect format to compare '{0}' with.", GetType().ToString()), "obj");
            }

            int c = Comparer<Object>.Default.Compare(m_Item1, objTuple.m_Item1);
            if (c != 0) { return c; }

            c = Comparer<Object>.Default.Compare(m_Item2, objTuple.m_Item2);
            if (c != 0) { return c; }

            return Comparer<Object>.Default.Compare(m_Item3, objTuple.m_Item3);
        }

        public override int GetHashCode()
        {
            return JaysonTuple.CombineHashCodes(EqualityComparer<Object>.Default.GetHashCode(m_Item1),
                EqualityComparer<Object>.Default.GetHashCode(m_Item2),
                EqualityComparer<Object>.Default.GetHashCode(m_Item3));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
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
    public class JaysonTuple<T1, T2, T3, T4> : IComparable
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

            JaysonTuple<T1, T2, T3, T4> objTuple = obj as JaysonTuple<T1, T2, T3, T4>;
            if (objTuple == null)
            {
                return false;
            }

            return EqualityComparer<Object>.Default.Equals(m_Item1, objTuple.m_Item1) &&
                EqualityComparer<Object>.Default.Equals(m_Item2, objTuple.m_Item2) &&
                EqualityComparer<Object>.Default.Equals(m_Item3, objTuple.m_Item3) &&
                EqualityComparer<Object>.Default.Equals(m_Item4, objTuple.m_Item4);
        }

        Int32 IComparable.CompareTo(Object obj)
        {
            if (obj == null) { return 1; }

            JaysonTuple<T1, T2, T3, T4> objTuple = obj as JaysonTuple<T1, T2, T3, T4>;
            if (objTuple == null)
            {
                throw new ArgumentException(String.Format("Given type is incorrect format to compare '{0}' with.", GetType().ToString()), "obj");
            }

            int c = Comparer<Object>.Default.Compare(m_Item1, objTuple.m_Item1);
            if (c != 0) { return c; }

            c = Comparer<Object>.Default.Compare(m_Item2, objTuple.m_Item2);
            if (c != 0) { return c; }

            c = Comparer<Object>.Default.Compare(m_Item3, objTuple.m_Item3);
            if (c != 0) { return c; }

            return Comparer<Object>.Default.Compare(m_Item4, objTuple.m_Item4);
        }

        public override int GetHashCode()
        {
            return JaysonTuple.CombineHashCodes(EqualityComparer<Object>.Default.GetHashCode(m_Item1),
                EqualityComparer<Object>.Default.GetHashCode(m_Item2),
                EqualityComparer<Object>.Default.GetHashCode(m_Item3),
                EqualityComparer<Object>.Default.GetHashCode(m_Item4));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
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
    public class JaysonTuple<T1, T2, T3, T4, T5> : IComparable
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

            JaysonTuple<T1, T2, T3, T4, T5> objTuple = obj as JaysonTuple<T1, T2, T3, T4, T5>;
            if (objTuple == null)
            {
                return false;
            }

            return EqualityComparer<Object>.Default.Equals(m_Item1, objTuple.m_Item1) &&
                EqualityComparer<Object>.Default.Equals(m_Item2, objTuple.m_Item2) &&
                EqualityComparer<Object>.Default.Equals(m_Item3, objTuple.m_Item3) &&
                EqualityComparer<Object>.Default.Equals(m_Item4, objTuple.m_Item4) &&
                EqualityComparer<Object>.Default.Equals(m_Item5, objTuple.m_Item5);
        }

        Int32 IComparable.CompareTo(Object obj)
        {
            if (obj == null) { return 1; }

            JaysonTuple<T1, T2, T3, T4, T5> objTuple = obj as JaysonTuple<T1, T2, T3, T4, T5>;

            if (objTuple == null)
            {
                throw new ArgumentException(String.Format("Given type is incorrect format to compare '{0}' with.", GetType().ToString()), "obj");
            }

            int c = Comparer<Object>.Default.Compare(m_Item1, objTuple.m_Item1);
            if (c != 0) { return c; }

            c = Comparer<Object>.Default.Compare(m_Item2, objTuple.m_Item2);
            if (c != 0) { return c; }

            c = Comparer<Object>.Default.Compare(m_Item3, objTuple.m_Item3);
            if (c != 0) { return c; }

            c = Comparer<Object>.Default.Compare(m_Item4, objTuple.m_Item4);
            if (c != 0) { return c; }

            return Comparer<Object>.Default.Compare(m_Item5, objTuple.m_Item5);
        }

        public override int GetHashCode()
        {
            return JaysonTuple.CombineHashCodes(EqualityComparer<Object>.Default.GetHashCode(m_Item1),
                EqualityComparer<Object>.Default.GetHashCode(m_Item2),
                EqualityComparer<Object>.Default.GetHashCode(m_Item3),
                EqualityComparer<Object>.Default.GetHashCode(m_Item4),
                EqualityComparer<Object>.Default.GetHashCode(m_Item5));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
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
    public class JaysonTuple<T1, T2, T3, T4, T5, T6> : IComparable
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

            JaysonTuple<T1, T2, T3, T4, T5, T6> objTuple = obj as JaysonTuple<T1, T2, T3, T4, T5, T6>;
            if (objTuple == null)
            {
                return false;
            }

            return EqualityComparer<Object>.Default.Equals(m_Item1, objTuple.m_Item1) &&
                EqualityComparer<Object>.Default.Equals(m_Item2, objTuple.m_Item2) &&
                EqualityComparer<Object>.Default.Equals(m_Item3, objTuple.m_Item3) &&
                EqualityComparer<Object>.Default.Equals(m_Item4, objTuple.m_Item4) &&
                EqualityComparer<Object>.Default.Equals(m_Item5, objTuple.m_Item5) &&
                EqualityComparer<Object>.Default.Equals(m_Item6, objTuple.m_Item6);
        }

        Int32 IComparable.CompareTo(Object obj)
        {
            if (obj == null) { return 1; }

            JaysonTuple<T1, T2, T3, T4, T5, T6> objTuple = obj as JaysonTuple<T1, T2, T3, T4, T5, T6>;
            if (objTuple == null)
            {
                throw new ArgumentException(String.Format("Given type is incorrect format to compare '{0}' with.", GetType().ToString()), "obj");
            }

            int c = Comparer<Object>.Default.Compare(m_Item1, objTuple.m_Item1);
            if (c != 0) { return c; }

            c = Comparer<Object>.Default.Compare(m_Item2, objTuple.m_Item2);
            if (c != 0) { return c; }

            c = Comparer<Object>.Default.Compare(m_Item3, objTuple.m_Item3);
            if (c != 0) { return c; }

            c = Comparer<Object>.Default.Compare(m_Item4, objTuple.m_Item4);
            if (c != 0) { return c; }

            c = Comparer<Object>.Default.Compare(m_Item5, objTuple.m_Item5);
            if (c != 0) { return c; }

            return Comparer<Object>.Default.Compare(m_Item6, objTuple.m_Item6);
        }

        public override int GetHashCode()
        {
            return JaysonTuple.CombineHashCodes(EqualityComparer<Object>.Default.GetHashCode(m_Item1),
                EqualityComparer<Object>.Default.GetHashCode(m_Item2),
                EqualityComparer<Object>.Default.GetHashCode(m_Item3),
                EqualityComparer<Object>.Default.GetHashCode(m_Item4),
                EqualityComparer<Object>.Default.GetHashCode(m_Item5),
                EqualityComparer<Object>.Default.GetHashCode(m_Item6));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
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
    public class JaysonTuple<T1, T2, T3, T4, T5, T6, T7> : IComparable
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

            JaysonTuple<T1, T2, T3, T4, T5, T6, T7> objTuple = obj as JaysonTuple<T1, T2, T3, T4, T5, T6, T7>;
            if (objTuple == null)
            {
                return false;
            }

            return EqualityComparer<Object>.Default.Equals(m_Item1, objTuple.m_Item1) &&
                EqualityComparer<Object>.Default.Equals(m_Item2, objTuple.m_Item2) &&
                EqualityComparer<Object>.Default.Equals(m_Item3, objTuple.m_Item3) &&
                EqualityComparer<Object>.Default.Equals(m_Item4, objTuple.m_Item4) &&
                EqualityComparer<Object>.Default.Equals(m_Item5, objTuple.m_Item5) &&
                EqualityComparer<Object>.Default.Equals(m_Item6, objTuple.m_Item6) &&
                EqualityComparer<Object>.Default.Equals(m_Item7, objTuple.m_Item7);
        }

        Int32 IComparable.CompareTo(Object obj)
        {
            if (obj == null) { return 1; }

            JaysonTuple<T1, T2, T3, T4, T5, T6, T7> objTuple = obj as JaysonTuple<T1, T2, T3, T4, T5, T6, T7>;
            if (objTuple == null)
            {
                throw new ArgumentException(String.Format("Given type is incorrect format to compare '{0}' with.", GetType().ToString()), "obj");
            }

            int c = Comparer<Object>.Default.Compare(m_Item1, objTuple.m_Item1);
            if (c != 0) { return c; }

            c = Comparer<Object>.Default.Compare(m_Item2, objTuple.m_Item2);
            if (c != 0) { return c; }

            c = Comparer<Object>.Default.Compare(m_Item3, objTuple.m_Item3);
            if (c != 0) { return c; }

            c = Comparer<Object>.Default.Compare(m_Item4, objTuple.m_Item4);
            if (c != 0) { return c; }

            c = Comparer<Object>.Default.Compare(m_Item5, objTuple.m_Item5);
            if (c != 0) { return c; }

            c = Comparer<Object>.Default.Compare(m_Item6, objTuple.m_Item6);
            if (c != 0) { return c; }

            return Comparer<Object>.Default.Compare(m_Item7, objTuple.m_Item7);
        }

        public override int GetHashCode()
        {
            return JaysonTuple.CombineHashCodes(EqualityComparer<Object>.Default.GetHashCode(m_Item1),
                EqualityComparer<Object>.Default.GetHashCode(m_Item2),
                EqualityComparer<Object>.Default.GetHashCode(m_Item3),
                EqualityComparer<Object>.Default.GetHashCode(m_Item4),
                EqualityComparer<Object>.Default.GetHashCode(m_Item5),
                EqualityComparer<Object>.Default.GetHashCode(m_Item6),
                EqualityComparer<Object>.Default.GetHashCode(m_Item7));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
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