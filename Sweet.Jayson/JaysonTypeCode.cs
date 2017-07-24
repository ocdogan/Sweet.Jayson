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
    # region JaysonTypeCode

    public enum JaysonTypeCode : long
    {
        String = 0L,
        Int = 1L << 0,
        Bool = 1L << 1,
        Long = 1L << 2,
        DateTime = 1L << 3,
        Double = 1L << 4,
        Short = 1L << 5,
        Byte = 1L << 6,
        Float = 1L << 7,
        Decimal = 1L << 8,
        UInt = 1L << 9,
        ULong = 1L << 10,
        Char = 1L << 11,
        UShort = 1L << 12,
        SByte = 1L << 13,
        Guid = 1L << 14,
        TimeSpan = 1L << 15,
        DateTimeOffset = 1L << 16,
        IntNullable = 1L << 17,
        BoolNullable = 1L << 18,
        LongNullable = 1L << 19,
        DateTimeNullable = 1L << 20,
        DoubleNullable = 1L << 21,
        ShortNullable = 1L << 22,
        ByteNullable = 1L << 23,
        FloatNullable = 1L << 24,
        DecimalNullable = 1L << 25,
        UIntNullable = 1L << 26,
        ULongNullable = 1L << 27,
        CharNullable = 1L << 28,
        UShortNullable = 1L << 29,
        SByteNullable = 1L << 30,
        GuidNullable = 1L << 31,
        TimeSpanNullable = 1L << 32,
        DateTimeOffsetNullable = 1L << 33,
        Object = 1L << 34,

        All =
            String |
            Int |
            Bool |
            Long |
            DateTime |
            Double |
            Short |
            Byte |
            Float |
            Decimal |
            UInt |
            ULong |
            Char |
            UShort |
            SByte |
            Guid |
            TimeSpan |
            DateTimeOffset |
            IntNullable |
            BoolNullable |
            LongNullable |
            DateTimeNullable |
            DoubleNullable |
            ShortNullable |
            ByteNullable |
            FloatNullable |
            DecimalNullable |
            UIntNullable |
            ULongNullable |
            CharNullable |
            UShortNullable |
            SByteNullable |
            GuidNullable |
            TimeSpanNullable |
            DateTimeOffsetNullable |
            Object,

        Primitive =
            Short |
            Int |
            Long |
            Bool |
            DateTime |
            Double |
            Byte |
            Float |
            Decimal |
            UInt |
            ULong |
            UShort |
            SByte |
            Char |
            TimeSpan,

        Number =
            Int |
            Long |
            Double |
            Short |
            Byte |
            Float |
            Decimal |
            UInt |
            ULong |
            UShort |
            SByte |
            IntNullable |
            LongNullable |
            DoubleNullable |
            ShortNullable |
            ByteNullable |
            FloatNullable |
            DecimalNullable |
            UIntNullable |
            ULongNullable |
            UShortNullable |
            SByteNullable,

        PrimitiveNumber =
            Int |
            Long |
            Double |
            Short |
            Byte |
            Float |
            Decimal |
            UInt |
            ULong |
            UShort |
            SByte,

        NullableNumber =
            IntNullable |
            LongNullable |
            DoubleNullable |
            ShortNullable |
            ByteNullable |
            FloatNullable |
            DecimalNullable |
            UIntNullable |
            ULongNullable |
            UShortNullable |
            SByteNullable,

        Nullable =
            IntNullable |
            BoolNullable |
            LongNullable |
            DateTimeNullable |
            DoubleNullable |
            ShortNullable |
            ByteNullable |
            FloatNullable |
            DecimalNullable |
            UIntNullable |
            ULongNullable |
            CharNullable |
            UShortNullable |
            SByteNullable |
            GuidNullable |
            TimeSpanNullable |
            DateTimeOffsetNullable,

        JsonKnown =
            Bool |
            Int |
            Long |
            Double |
            String,

        JsonUnknown =
            All & ~JsonKnown,

        AutoTyped =
            All & ~JsonKnown,
    }

    # endregion JaysonTypeCode
}
