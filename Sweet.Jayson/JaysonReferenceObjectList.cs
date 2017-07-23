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
	public class JaysonReferenceObjectList
	{
		# region Field Members

		private int m_Ref = 1;
		private Dictionary<int, object> m_Ids = new Dictionary<int, object>();
		private Dictionary<object, int> m_Objects = new Dictionary<object, int>();

		# endregion Field Members

		public int Register(object obj, out bool exists)
		{
			exists = false;
			if (obj != null) {
				int id;
				if (m_Objects.TryGetValue (obj, out id)) {
					exists = true;
					return id;
				}

				id = m_Ref++;
				m_Ids.Add (id, obj);
				m_Objects.Add (obj, id);
				return id;
			}
			return -1;
		}

		public object GetObject(int id)
		{
			object obj;
			if (m_Ids.TryGetValue (id, out obj)) {
				return obj;
			}
			return null;
		}
	}
}

