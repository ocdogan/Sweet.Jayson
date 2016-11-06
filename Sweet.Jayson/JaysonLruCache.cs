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
using System.Linq.Expressions;
using System.Threading;

namespace Sweet.Jayson
{
    # region JaysonLruCache

    public sealed class JaysonLruCache<TKey, TValue>
    {
        # region LruEntry

        private class LruEntry
        {
            public LruEntry Next;
            public LruEntry Previous;
            public TKey Key { get; set; }
            public TValue Value { get; set; }
        }

        # endregion LruEntry

        # region Constants

        private const int DefaultCapacity = -1;

        # endregion Constants

        # region Field Members

        private LruEntry Head;
        private LruEntry Tail;

        private int m_Capacity;
        private bool m_CheckCapacity;
        private Dictionary<TKey, LruEntry> m_Entries;

        private readonly object syncRoot = new object();

        # endregion Field Members

        public JaysonLruCache(int capacity = DefaultCapacity)
        {
            Head = null;
            Tail = null;
            m_Capacity = capacity <= 0 ? DefaultCapacity : capacity;
            m_CheckCapacity = m_Capacity > 0;

            var initialCap = m_CheckCapacity ? Math.Min(1000, m_Capacity) : 1000;
            m_Entries = new Dictionary<TKey, LruEntry>(initialCap);
        }

        public int Capacity
        {
            get { return m_Capacity; }
        }

        public int Count
        {
            get
            {
                Monitor.Enter(syncRoot);
                try
                {
                    return m_Entries.Count;
                }
                finally
                {
                    Monitor.Exit(syncRoot);
                }
            }
        }

        public void SetCapacity(int capacity)
        {
            Monitor.Enter(syncRoot);
            try
            {
                if ((capacity > 0) && (m_Capacity < capacity))
                {
                    m_Capacity = Math.Max(capacity, m_Entries.Count);
                    m_CheckCapacity = m_Capacity > 0;
                }
            }
            finally
            {
                Monitor.Exit(syncRoot);
            }
        }

        private void SetHead(LruEntry entry)
        {
            if (m_CheckCapacity && (entry != Head))
            {
                var next = entry.Next;
                var previous = entry.Previous;

                if (next != null)
                {
                    next.Previous = previous;
                }
                if (previous != null)
                {
                    previous.Next = next;
                }

                entry.Previous = null;
                entry.Next = Head;

                if (Head != null)
                {
                    Head.Previous = entry;
                }
                Head = entry;

                if (Tail == entry)
                {
                    Tail = previous;
                }
            }
        }

        public bool Add(TKey key, TValue value)
        {
            Monitor.Enter(syncRoot);
            try
            {
                LruEntry entry;
                if (!m_Entries.TryGetValue(key, out entry))
                {
                    entry = new LruEntry { Key = key, Value = value };

                    if (m_CheckCapacity && (m_Entries.Count == m_Capacity))
                    {
                        m_Entries.Remove(Tail.Key);

                        Tail = Tail.Previous;
                        if (Tail != null)
                        {
                            Tail.Next = null;
                        }
                    }

                    m_Entries.Add(key, entry);

                    if (m_CheckCapacity)
                    {
                        SetHead(entry);
                        if (Tail == null)
                        {
                            Tail = Head;
                        }
                    }
                    return true;
                }
            }
            finally
            {
                Monitor.Exit(syncRoot);
            }
            return false;
        }

        public void AddOrUpdate(TKey key, TValue value)
        {
            Monitor.Enter(syncRoot);
            try
            {
                LruEntry entry;
                if (m_Entries.TryGetValue(key, out entry))
                {
                    entry.Value = value;
                }
                else
                {
                    entry = new LruEntry { Key = key, Value = value };

                    if (m_CheckCapacity && (m_Entries.Count == m_Capacity))
                    {
                        m_Entries.Remove(Tail.Key);

                        Tail = Tail.Previous;
                        if (Tail != null)
                        {
                            Tail.Next = null;
                        }
                    }
                    m_Entries.Add(key, entry);
                }

                if (m_CheckCapacity)
                {
                    SetHead(entry);
                    if (Tail == null)
                    {
                        Tail = Head;
                    }
                }
            }
            finally
            {
                Monitor.Exit(syncRoot);
            }
        }

        public bool Update(TKey key, TValue value)
        {
            Monitor.Enter(syncRoot);
            try
            {
                LruEntry entry;
                if (m_Entries.TryGetValue(key, out entry))
                {
                    entry.Value = value;

                    if (m_CheckCapacity)
                    {
                        SetHead(entry);
                        if (Tail == null)
                        {
                            Tail = Head;
                        }
                    }
                    return true;
                }
            }
            finally
            {
                Monitor.Exit(syncRoot);
            }
            return false;
        }

        private void EntryRemoved(LruEntry entry)
        {
            if (m_CheckCapacity)
            {
                var next = entry.Next;
                var previous = entry.Previous;

                if (previous != null)
                {
                    previous.Next = next;
                }
                if (next != null)
                {
                    next.Previous = previous;
                }

                if (entry == Tail)
                {
                    Tail = previous;
                    if (Tail != null)
                    {
                        Tail.Next = null;
                    }
                }

                if (entry == Head)
                {
                    Head = next;
                    if (Head != null)
                    {
                        Head.Previous = null;
                    }
                }

                entry.Next = null;
                entry.Previous = null;
            }
        }

        public TValue Remove(TKey key)
        {
            Monitor.Enter(syncRoot);
            try
            {
                LruEntry entry;
                if (m_Entries.TryGetValue(key, out entry) && m_Entries.Remove(key))
                {
                    EntryRemoved(entry);
                    return entry.Value;
                }
            }
            finally
            {
                Monitor.Exit(syncRoot);
            }

            return default(TValue);
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            Monitor.Enter(syncRoot);
            try
            {
                LruEntry entry;
                if (m_Entries.TryGetValue(key, out entry) && m_Entries.Remove(key))
                {
                    EntryRemoved(entry);
                    value = entry.Value;
                    return true;
                }
            }
            finally
            {
                Monitor.Exit(syncRoot);
            }

            value = default(TValue);
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            Monitor.Enter(syncRoot);
            try
            {
                LruEntry entry;
                if (!m_Entries.TryGetValue(key, out entry))
                {
                    value = default(TValue);
                    return false;
                }

                SetHead(entry);
                value = entry.Value;
            }
            finally
            {
                Monitor.Exit(syncRoot);
            }

            return true;
        }

        public void Clear()
        {
            Monitor.Enter(syncRoot);
            try
            {
                m_Entries.Clear();
                m_Entries = new Dictionary<TKey, LruEntry>();

                if (m_CheckCapacity)
                {
                    LruEntry nodeTmp;
                    LruEntry node = Head;

                    while (node != null)
                    {
                        nodeTmp = node;
                        node = node.Next;

                        nodeTmp.Next = null;
                        nodeTmp.Previous = null;
                    }
                }
            }
            finally
            {
                Head = null;
                Tail = null;

                Monitor.Exit(syncRoot);
            }
        }
    }

    # endregion JaysonLruCache
}