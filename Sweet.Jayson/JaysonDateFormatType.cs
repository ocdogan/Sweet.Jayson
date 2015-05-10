using System;

namespace Sweet.Jayson
{
	# region JaysonDateFormatType

	public enum JaysonDateFormatType
	{
		Iso8601 = 0,
		Microsoft = 1,
		JScript = 2,
		UnixEpoch = 3,
		CustomDate = 5,
		CustomUnixEpoch = 6
	}

	# endregion JaysonDateFormatType
}