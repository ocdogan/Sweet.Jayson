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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sweet.Jayson
{
    internal class JaysonOrderedDictionary<T, K> : IDictionary<T, K>
    {
        private readonly object m_SyncRoot = new object();

        private readonly List<KeyValuePair<T, K>> m_OrderedItems;
        private readonly JaysonSynchronizedDictionary<T, K> m_Items;

        public JaysonOrderedDictionary()
        {
            m_Items = new JaysonSynchronizedDictionary<T, K>();
            m_OrderedItems = new List<KeyValuePair<T, K>>();
        }

        public JaysonOrderedDictionary(int capacity)
        {
            m_Items = new JaysonSynchronizedDictionary<T, K>(capacity);
            m_OrderedItems = new List<KeyValuePair<T, K>>(capacity);
        }

        public void Add(T key, K value)
        {
            lock (m_SyncRoot)
            {
                m_Items.Add(key, value);
                m_OrderedItems.Add(new KeyValuePair<T, K>(key, value));
            }
        }

        public bool ContainsKey(T key)
        {
            return m_Items.ContainsKey(key);
        }

        public ICollection<T> Keys
        {
            get { return m_Items.Keys; }
        }

        public bool Remove(T key)
        {
            lock (m_SyncRoot)
            {
                if (m_Items.Remove(key))
                {
                    m_OrderedItems.RemoveAll((kvp) => kvp.Key.Equals(key));
                    return true;
                }
            }
            return false;
        }

        public bool TryGetValue(T key, out K value)
        {
            return m_Items.TryGetValue(key, out value);
        }

        public ICollection<K> Values
        {
            get { return m_Items.Values; }
        }

        public K this[T key]
        {
            get
            {
                return m_Items[key];
            }
            set
            {
                lock (m_SyncRoot)
                {
                    if (m_Items.ContainsKey(key))
                    {
                        m_Items[key] = value;

                        int count = m_OrderedItems.Count;
                        for (int i = 0; i < count; i++)
                        {
                            if (m_OrderedItems[i].Key.Equals(key))
                            {
                                m_OrderedItems[i] = new KeyValuePair<T, K>(key, value);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void Add(KeyValuePair<T, K> item)
        {
            lock (m_SyncRoot)
            {
                if (!m_Items.ContainsKey(item.Key))
                {
                    ((ICollection<KeyValuePair<T, K>>)m_Items).Add(item);
                    m_OrderedItems.Add(item);
                }
            }
        }

        public void Clear()
        {
            lock (m_SyncRoot)
            {
                m_Items.Clear();
                m_OrderedItems.Clear();
            }
        }

        public bool Contains(KeyValuePair<T, K> item)
        {
            return m_Items.Contains(item);
        }

        public void CopyTo(KeyValuePair<T, K>[] array, int arrayIndex)
        {
            lock (m_SyncRoot)
            {
                m_OrderedItems.CopyTo(array, arrayIndex);
            }
        }

        public int Count
        {
            get { return m_OrderedItems.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<T, K> item)
        {
            lock (m_SyncRoot)
            {
                if (((ICollection<KeyValuePair<T, K>>)m_Items).Remove(item))
                {
                    T key = item.Key;
                    int count = m_OrderedItems.Count;

                    for (int i = 0; i < count; i++)
                    {
                        if (m_OrderedItems[i].Key.Equals(key))
                        {
                            m_OrderedItems.RemoveAt(i);
                            break;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public IEnumerator<KeyValuePair<T, K>> GetEnumerator()
        {
            return m_OrderedItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_OrderedItems.GetEnumerator();
        }
    }
}
