﻿# region License
//  The MIT License (MIT)
//
//  Copyright (c) 2015, Cagatay Dogan
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//      The above copyright notice and this permission notice shall be included in
//      all copies or substantial portions of the Software.
//
//      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//      IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//      FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//      AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//      LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//      OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//      THE SOFTWARE.
# endregion License

using System.Text;
using System.Globalization;

namespace Sweet.Jayson
{
    # region JaysonNumberParts

    internal class JaysonNumberParts
    {
        # region Field Members

        # region Field Readonly Members

        private readonly JaysonNumberPart[] m_Parts;

        # endregion Field Readonly Members

        public string Text;
        public int Start;
        public int Length;

        public NumberStyles Style;
        public JaysonNumberType Type;

        # endregion Field Members

        # region Properties

        public JaysonNumberPart WholePart { get { return m_Parts[0]; } }
        public byte WholeLength { get { return m_Parts[0] != null ? m_Parts[0].Length : (byte)0; } }
        public bool HasWhole { get { return m_Parts[0] != null ? (m_Parts[0].Length > 0 && m_Parts[0].Start > -1) : false; } }

        public JaysonNumberPart FloatingPart { get { return m_Parts[1]; } private set { m_Parts[1] = value; } }
        public byte FloatingLength { get { return m_Parts[1] != null ? m_Parts[1].Length : (byte)0; } }
        public bool HasFloating { get { return m_Parts[1] != null ? (m_Parts[1].Length > 0 && m_Parts[1].Start > -1) : false; } }

        public JaysonNumberPart ExponentPart { get { return m_Parts[2]; } private set { m_Parts[2] = value; } }
        public byte ExponentLength { get { return m_Parts[2] != null ? m_Parts[2].Length : (byte)0; } }
        public bool HasExponent { get { return m_Parts[2] != null ? (m_Parts[2].Length > 0 && m_Parts[2].Start > -1) : false; } }

        # endregion Properties

        # region .Ctors

        public JaysonNumberParts()
        {
            Type = JaysonNumberType.Long;
            Style = NumberStyles.None;
            m_Parts = new JaysonNumberPart[] {
                    new JaysonNumberPart(), // Whole part
                    null, // Floating part
                    null, // Exponent part
                };
        }

        # endregion .Ctors

        # region Methods

        public JaysonNumberPart SetFloating(int start = -1, byte length = 0, int sign = 1)
        {
            var result = FloatingPart;
            if (result == null)
            {
                result = new JaysonNumberPart
                {
                    Sign = sign,
                    Start = start,
                    Length = length
                };
                FloatingPart = result;
            }
            return result;
        }

        public JaysonNumberPart SetExponent(int start = -1, byte length = 0, int sign = 1)
        {
            var result = ExponentPart;
            if (result == null)
            {
                result = new JaysonNumberPart
                {
                    Sign = sign,
                    Start = start,
                    Length = length
                };
                ExponentPart = result;
            }
            return result;
        }

        public override string ToString()
        {
            if (Text != null) {
                var sBuilder = new StringBuilder();

                var wholePart = WholePart;
                if (wholePart != null) {
                    if (wholePart.Sign == -1) {
                        sBuilder.Append('-');
                    }

                    if ((wholePart.Start > -1) && (wholePart.Length > 0) &&
                        (wholePart.Start < Text.Length)) {
                        sBuilder.Append(Text.Substring(wholePart.Start, wholePart.Length));
                    }
                }

                var floatingPart = FloatingPart;
                if ((floatingPart != null) &&
                    (floatingPart.Start > -1) && (wholePart.Length > 0) &&
                    (floatingPart.Start < Text.Length)) {
                    sBuilder.Append ('.');
                    sBuilder.Append(Text.Substring(floatingPart.Start, floatingPart.Length));
                }

                var exponentPart = ExponentPart;
                if (exponentPart != null) {
                    if (exponentPart.Sign == -1) {
                        sBuilder.Append ('-');
                    }
                    sBuilder.Append('e');

                    if ((exponentPart.Start > -1) && (exponentPart.Length > 0) &&
                        (exponentPart.Start < Text.Length)) {
                        sBuilder.Append(Text.Substring(exponentPart.Start, exponentPart.Length));
                    }
                }
                return sBuilder.ToString();
            }
            return string.Empty;
        }

        # endregion Methods
    }

    # endregion JaysonNumberParts
}
