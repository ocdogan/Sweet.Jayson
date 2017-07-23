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

namespace Sweet.Jayson
{
    public class JaysonNumber
    {
        public JaysonNumber(byte[] data, JaysonNumberKind kind = JaysonNumberKind.Undefined)
        {
            Data = data;
            Kind = kind;
        }

        public byte[] Data { get; private set; }

        public JaysonNumberKind Kind { get; private set; }

        public object Value
        {
            get
            {
                var data = Data;
                var kind = Kind;

                if ((data == null) || (data.Length == 0))
                {
                    kind = JaysonNumberKind.Undefined;
                }

                switch (kind)
                {
                    case JaysonNumberKind.Int:
                        break;
                    case JaysonNumberKind.Long:
                        break;
                    case JaysonNumberKind.Double:
                        break;
                    case JaysonNumberKind.Short:
                        break;
                    case JaysonNumberKind.Float:
                        break;
                    case JaysonNumberKind.Decimal:
                        break;
                    case JaysonNumberKind.Byte:
                        break;
                    case JaysonNumberKind.UInt:
                        break;
                    case JaysonNumberKind.ULong:
                        break;
                    case JaysonNumberKind.UShort:
                        break;
                    case JaysonNumberKind.SByte:
                        break;
                    default:
                        break;
                }

                return null;
            }
        }
    }
}
