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
	public class JaysonGlobalTypeList
	{
		# region Field Members

		private int m_IdRef = 1;
		private Dictionary<int, Type> m_IdAndTypes = new Dictionary<int, Type>();
		private Dictionary<Type, int> m_TypeAndIds = new Dictionary<Type, int>();

		# endregion Field Members

		public JaysonGlobalTypeList(IDictionary<string, object> types = null)
		{
			if (types != null) {
				Initialize (types);
			}
		}

		public int Register(Type type)
		{
			int id;
			if (!m_TypeAndIds.TryGetValue (type, out id)) {
				id = m_IdRef++;
				m_IdAndTypes.Add (id, type);
				m_TypeAndIds.Add (type, id);
			}
			return id;
		}

		public Type GetType(int id)
		{
			Type type;
			if (m_IdAndTypes.TryGetValue (id, out type)) {
				return type;
			}
			return null;
		}

		public Dictionary<int, Type> GetIdList()
		{
			return m_IdAndTypes;
		}

		private static int ParseId(string sid)
		{
			if (sid == null) {
				throw new JaysonException("Invalid number.");
			}

			int length = sid.Length;
			if (length == 0) {
				throw new JaysonException("Invalid number.");
			}

			char ch;
			if (length == 1)
			{
				ch = sid[0];
				if (ch < '0' || ch > '9')
				{
					throw new JaysonException("Invalid number.");
				}
				return (int)(ch - '0');
			}

			ch = (char)0;
			int value = 0;

			for (int pos = 0; pos < length; pos++)
			{
				ch = sid[pos];
				if (!(ch < '0' || ch > '9'))
				{
					value = (value * 10) + (int)(ch - '0');
					continue;
				}

				throw new JaysonException("Invalid number.");
			}
			return value;
		}

		private void Initialize(IDictionary<string, object> types)
		{
			if (types.Count > 0) {
				int id;
				Type type;
				string typeName;

				foreach (var tKvp in types) {
					typeName = tKvp.Value as string;
					if (String.IsNullOrEmpty (typeName)) {
						throw new JaysonException("Invalid type name.");
					}

					id = ParseId(tKvp.Key);
					type = Type.GetType (typeName);

					m_IdAndTypes.Add (id, type);
					m_TypeAndIds.Add (type, id);

					if (id > m_IdRef) {
						m_IdRef = id + 1;
					}
				}
			}
		}
	}
}

