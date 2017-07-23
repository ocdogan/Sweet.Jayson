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
using System.Threading;

namespace Sweet.Jayson
{
    # region JaysonStackList

    internal sealed class JaysonStackList<T> where T : class
    {
        # region Constants

        private const int FrameSize = 16;
        private const int QueueLength = 40;

        # endregion Constants

        # region Static Members

        private static int s_QueueIndex = -1;
        private static object s_QueueLock = new object();
        private static JaysonStackList<T>[] s_Queue = new JaysonStackList<T>[QueueLength];

        # endregion Static Members

        private int m_Index = -1;
        private int m_Length = FrameSize;
        private T[] m_Items = new T[FrameSize];

        private JaysonStackList()
        {
            Init(true);
        }

        private void Init(bool ctor)
        {
            m_Index = -1;
            if (!ctor && m_Length > 2 * FrameSize)
            {
                m_Items = new T[FrameSize];
                m_Length = FrameSize;
            }
        }

        public static JaysonStackList<T> Get()
        {
            lock (s_QueueLock)
            {
                if (s_QueueIndex > -1)
                {
                    var result = s_Queue[s_QueueIndex];
                    s_Queue[s_QueueIndex--] = null;
                    return result;
                }
            }
            return new JaysonStackList<T>();
        }

        public static void Release(JaysonStackList<T> stack)
        {
            if (stack != null)
            {
                lock (s_QueueLock)
                {
                    if (s_QueueIndex < QueueLength - 1)
                    {
                        stack.Init(false);
                        s_Queue[++s_QueueIndex] = stack;
                    }
                }
            }
        }

        private void EnsureCapacity()
        {
            if (m_Index >= m_Length - 1)
            {
                m_Length += FrameSize;

                T[] newArray = new T[m_Length];
                Array.Copy(m_Items, 0, newArray, 0, m_Items.Length);
                m_Items = newArray;
            }
        }

        public void Push(T obj)
        {
            EnsureCapacity();
            m_Items[++m_Index] = obj;
        }

        public void Pop()
        {
            if (m_Index > -1)
            {
                m_Items[m_Index--] = default(T);
            }
        }

        public T Peek()
        {
            if (m_Index > -1) {
                return m_Items[m_Index];
            }
            return default(T);
        }

        public bool Contains(T obj)
        {
            if (obj != null)
            {
                for (int i = m_Index; i > -1; i--)
                {
                    if (obj == m_Items[i])
                        return true;
                }
            }
            return false;
        }
    }

    # endregion JaysonStackList
}
