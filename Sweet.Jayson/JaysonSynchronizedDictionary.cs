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
    public class JaysonSynchronizedDictionary<T, K> : IDictionary<T, K>, IDictionary
    {
        #region Field Members

        private readonly object m_SyncRoot = new object();
        private readonly Dictionary<T, K> m_InnerDictionary;

        #endregion Field Members

        #region .Ctors

        public JaysonSynchronizedDictionary()
            : this(0, null) 
        { }
 
        public JaysonSynchronizedDictionary(int capacity)
            : this(capacity, null) 
        { }

        public JaysonSynchronizedDictionary(IEqualityComparer<T> comparer)
            : this(0, comparer) 
        { }

        public JaysonSynchronizedDictionary(int capacity, IEqualityComparer<T> comparer) 
        {
            m_InnerDictionary = new Dictionary<T, K>(capacity, comparer);
        }

        public JaysonSynchronizedDictionary(IDictionary<T, K> dictionary)
            : this(dictionary, null) 
        { }

        public JaysonSynchronizedDictionary(IDictionary<T, K> dictionary, IEqualityComparer<T> comparer) :
            this(dictionary != null? dictionary.Count: 0, comparer) 
        {
            m_InnerDictionary = new Dictionary<T, K>(dictionary, comparer);
        }

        #endregion .Ctors

        #region Properties

        object ICollection.SyncRoot
        {
            get { return m_SyncRoot; }
        }

        public ICollection<T> Keys
        {
            get 
            {
                lock (m_SyncRoot)
                {
                    return m_InnerDictionary.Keys;
                }
            }
        }

        public ICollection<K> Values
        {
            get 
            {
                lock (m_SyncRoot)
                {
                    return m_InnerDictionary.Values;
                }
            }
        }

        public K this[T key]
        {
            get
            {
                lock (m_SyncRoot)
                {
                    return m_InnerDictionary[key];
                }
            }
            set
            {
                lock (m_SyncRoot)
                {
                    m_InnerDictionary[key] = value;
                }
            }
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public int Count
        {
            get 
            {
                lock (m_SyncRoot)
                {
                    return m_InnerDictionary.Count;
                } 
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        ICollection IDictionary.Keys
        {
            get 
            {
                lock (m_SyncRoot)
                {
                    return ((IDictionary)m_InnerDictionary).Keys;
                }
            }
        }

        ICollection IDictionary.Values
        {
            get 
            {
                lock (m_SyncRoot)
                {
                    return ((IDictionary)m_InnerDictionary).Values;
                }
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                lock (m_SyncRoot)
                {
                    return ((IDictionary)m_InnerDictionary)[key];
                }
            }
            set
            {
                lock (m_SyncRoot)
                {
                    ((IDictionary)m_InnerDictionary)[key] = value;
                }
            }
        }

        public bool IsSynchronized
        {
            get { return true; }
        }

        #endregion Properties

        #region Methods

        public void Add(T key, K value)
        {
            lock (m_SyncRoot)
            {
                m_InnerDictionary.Add(key, value);
            }
        }

        public bool ContainsKey(T key)
        {
            lock (m_SyncRoot)
            {
                return m_InnerDictionary.ContainsKey(key);
            }
        }

        public bool Remove(T key)
        {
            lock (m_SyncRoot)
            {
                return m_InnerDictionary.Remove(key);
            }
        }

        public bool TryGetValue(T key, out K value)
        {
            lock (m_SyncRoot)
            {
                return m_InnerDictionary.TryGetValue(key, out value);
            }
        }

        public K GetValueOrUpdate(T key, Func<T, K> f)
        {
            K result;
            if (!TryGetValue(key, out result))
            {
                lock (m_SyncRoot)
                {
                    if (!TryGetValue(key, out result))
                    {
                        m_InnerDictionary[key] = (result = f(key));
                    }
                }
            }
            return result;
        }

        void ICollection<KeyValuePair<T, K>>.Add(KeyValuePair<T, K> item)
        {
            lock (m_SyncRoot)
            {
                ((ICollection<KeyValuePair<T, K>>)m_InnerDictionary).Add(item);
            }
        }

        public void Clear()
        {
            lock (m_SyncRoot)
            {
                m_InnerDictionary.Clear();
            }
        }

        bool ICollection<KeyValuePair<T, K>>.Contains(KeyValuePair<T, K> item)
        {
            lock (m_SyncRoot)
            {
                return ((ICollection<KeyValuePair<T, K>>)m_InnerDictionary).Contains(item);
            }
        }

        void ICollection<KeyValuePair<T, K>>.CopyTo(KeyValuePair<T, K>[] array, int arrayIndex)
        {
            lock (m_SyncRoot)
            {
                ((ICollection<KeyValuePair<T, K>>)m_InnerDictionary).CopyTo(array, arrayIndex);
            }
        }

        bool ICollection<KeyValuePair<T, K>>.Remove(KeyValuePair<T, K> item)
        {
            lock (m_SyncRoot)
            {
                return ((ICollection<KeyValuePair<T, K>>)m_InnerDictionary).Remove(item);
            }
        }

        public IEnumerator<KeyValuePair<T, K>> GetEnumerator()
        {
            lock (m_SyncRoot)
            {
                return m_InnerDictionary.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (m_SyncRoot)
            {
                return ((IEnumerable)m_InnerDictionary).GetEnumerator();
            }
        }

        void IDictionary.Add(object key, object value)
        {
            lock (m_SyncRoot)
            {
                ((IDictionary)m_InnerDictionary).Add(key, value);
            }
        }

        bool IDictionary.Contains(object key)
        {
            lock (m_SyncRoot)
            {
                return ((IDictionary)m_InnerDictionary).Contains(key);
            }
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            lock (m_SyncRoot)
            {
                return ((IDictionary)m_InnerDictionary).GetEnumerator();
            }
        }

        void IDictionary.Remove(object key)
        {
            lock (m_SyncRoot)
            {
                ((IDictionary)m_InnerDictionary).Remove(key);
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            lock (m_SyncRoot)
            {
                ((ICollection)m_InnerDictionary).CopyTo(array, index);
            }
        }

        #endregion Methods
    }
}
