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
using System.Linq;
using System.Threading;

namespace Sweet.Jayson
{
    internal class JaysonSerializationReferenceMap : IDisposable
    {
        private int m_Ids = 0;
        private Dictionary<object, int> m_ObjectMap = new Dictionary<object, int>();

        public int GetObjectId(object obj)
        {
            int id = 0;
            if (obj != null && !m_ObjectMap.TryGetValue(obj, out id)) {
                id = Interlocked.Increment(ref m_Ids);
                m_ObjectMap[obj] = id;
            }
            return id;
        }

        public int GetObjectId(object obj, out bool alreadyExists)
        {
            int id = 0;
            alreadyExists = false;
            if (obj != null)
            {
                alreadyExists = m_ObjectMap.TryGetValue(obj, out id);
                if (!alreadyExists)
                {
                    id = Interlocked.Increment(ref m_Ids);
                    m_ObjectMap[obj] = id;
                }
            }
            return id;
        }

        public bool TryGetObjectId(object obj, out int id)
        {
            id = 0;
            if (obj != null)
            {
                return m_ObjectMap.TryGetValue(obj, out id);
            }
            return false;
        }

        public void Dispose()
        {
            m_ObjectMap.Clear();
        }
    }
}
