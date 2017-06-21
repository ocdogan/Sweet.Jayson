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
        Int = 1L,
        Bool = 2L,
        Long = 4L,
        DateTime = 8L,
        Double = 16L,
        Short = 32L,
        Byte = 64L,
        Float = 128L,
        Decimal = 256L,
        UInt = 512L,
        ULong = 1024L,
        Char = 2048L,
        UShort = 4096L,
        SByte = 8192L,
        Guid = 16384L,
        TimeSpan = 32768L,
        DateTimeOffset = 65536L,
        IntNullable = 131072L,
        BoolNullable = 262144L,
        LongNullable = 524288L,
        DateTimeNullable = 1048576L,
        DoubleNullable = 2097152L,
        ShortNullable = 4194304L,
        ByteNullable = 8388608L,
        FloatNullable = 16777216L,
        DecimalNullable = 33554432L,
        UIntNullable = 67108864L,
        ULongNullable = 134217728L,
        CharNullable = 268435456L,
        UShortNullable = 536870912L,
        SByteNullable = 1073741824L,
        GuidNullable = 2147483648L,
        TimeSpanNullable = 4294967296L,
        DateTimeOffsetNullable = 8589934592L,
        Object = 17179869184L,

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

        AutoTyped =
            ((Number & ~Int) & ~Double) |
            Nullable,

        JsonKnown =
            Bool |
            Int |
            Double |
            String,

        JsonUnknown =
            All & ~JsonKnown
    }

    # endregion JaysonTypeCode
}
