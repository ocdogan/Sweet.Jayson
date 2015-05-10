using System;

namespace Sweet.Jayson
{
	# region JaysonSerializationToken

	internal enum JaysonSerializationToken
	{
		Undefined = 0,
		Object = 1,
		List = 2,
		Comma = 3,
		Key = 4,
		Colon = 5,
		Value = 6
	}

	# endregion JaysonSerializationToken
}

