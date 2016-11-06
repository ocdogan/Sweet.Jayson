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
    public class JaysonDeserializationTypeList
    {
        # region Field Members

        private Dictionary<int, Type> m_Ids = new Dictionary<int, Type>();

        # endregion Field Members

        public JaysonDeserializationTypeList(IDictionary<string, object> types = null)
        {
            if (types != null)
            {
                Initialize(types);
            }
        }

        public Type GetType(int id)
        {
            Type type;
            m_Ids.TryGetValue(id, out type);
            return type;
        }

        private static int ParseId(string sid)
        {
            if (sid == null)
            {
                throw new JaysonException(JaysonError.InvalidNumber);
            }

            var length = sid.Length;
            if (length == 0)
            {
                throw new JaysonException(JaysonError.InvalidNumber);
            }

            char ch;
            if (length == 1)
            {
                ch = sid[0];
                if (ch < '0' || ch > '9')
                {
                    throw new JaysonException(JaysonError.InvalidNumber);
                }
                return (int)(ch - '0');
            }

            ch = (char)0;
            var value = 0;

            for (int pos = 0; pos < length; pos++)
            {
                ch = sid[pos];
                if (!(ch < '0' || ch > '9'))
                {
                    value = (value * 10) + (int)(ch - '0');
                    continue;
                }

                throw new JaysonException(JaysonError.InvalidNumber);
            }
            return value;
        }

        private void Initialize(IDictionary<string, object> types)
        {
            if (types.Count > 0)
            {
                int id;
                Type type;
                string typeName;

                foreach (var tKvp in types)
                {
                    typeName = tKvp.Value as string;
                    if (String.IsNullOrEmpty(typeName))
                    {
                        throw new JaysonException(JaysonError.InvalidTypeName);
                    }

                    id = ParseId(tKvp.Key);
                    type = JaysonCommon.GetType(typeName);

                    m_Ids.Add(id, type);
                }
            }
        }
    }
}
