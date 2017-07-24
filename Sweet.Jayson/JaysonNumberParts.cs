# region License
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
        public string Text;
        public int Start;
        public int Length;

        public NumberStyles Style;
        public JaysonNumberType Type;
        public readonly JaysonNumberPart [] Parts;

        public JaysonNumberPart WholePart { get { return Parts[0]; } }
        public byte WholeLength { get { return Parts[0].Length; } }

        public JaysonNumberPart FloatingPart { get { return Parts[1]; } }
        public byte FloatingLength { get { return Parts[1] != null ? Parts[1].Length : (byte)0; } }

        public JaysonNumberPart ExponentPart { get { return Parts[2]; } }
        public byte ExponentLength { get { return Parts[2] != null ? Parts[2].Length : (byte)0; } }

        public JaysonNumberParts()
        {
            Type = JaysonNumberType.Long;
            Style = NumberStyles.None;
            Parts = new JaysonNumberPart[] {
                    new JaysonNumberPart(), // Whole part
                    null, // Floating part
                    null, // Exponent part
                };
        }

        public override string ToString ()
        {
            if (Text != null) {
                var sBuilder = new StringBuilder();

                var wholePart = Parts[0];
                if (wholePart != null) {
                    if (wholePart.Sign == -1) {
                        sBuilder.Append('-');
                    }

                    if ((wholePart.Start > -1) && (wholePart.Length > 0) &&
                        (wholePart.Start < Text.Length)) {
                        sBuilder.Append(Text.Substring(wholePart.Start, wholePart.Length));
                    }
                }

                var floatingPart = Parts[1];
                if ((floatingPart != null) &&
                    (floatingPart.Start > -1) && (wholePart.Length > 0) &&
                    (floatingPart.Start < Text.Length)) {
                    sBuilder.Append ('.');
                    sBuilder.Append(Text.Substring(floatingPart.Start, floatingPart.Length));
                }

                var exponentPart = Parts[2];
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
    }

    # endregion JaysonNumberParts
}
