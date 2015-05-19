﻿# region License
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

namespace Sweet.Jayson
{
	public class JaysonSerializationTypeList
	{
		# region Field Members

		private int m_Ref = 1;
		private Dictionary<Type, string> m_Types = new Dictionary<Type, string>();
		private List<JaysonKeyValue<string, Type>> m_OrderedList = new List<JaysonKeyValue<string, Type>>();

		# endregion Field Members

		public int Count
		{
			get { return m_Types.Count; }
		}

		public string Register(Type type)
		{
			string id;
			if (!m_Types.TryGetValue (type, out id)) {
				id = (m_Ref++).ToString(JaysonConstants.InvariantCulture);

				m_Types.Add (type, id);
				m_OrderedList.Add (new JaysonKeyValue<string, Type> { Key = id, Value = type });
			}
			return id;
		}

		internal List<JaysonKeyValue<string, Type>> GetList()
		{
			return m_OrderedList;
		}
	}
}
