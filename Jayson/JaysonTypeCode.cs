using System;

namespace Jayson
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
			DateTimeOffsetNullable
	}

	# endregion JaysonTypeCode
}

