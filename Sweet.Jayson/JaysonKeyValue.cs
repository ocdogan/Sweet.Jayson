using System;

namespace Sweet.Jayson
{
	# region JaysonKeyValue<TKey, TValue>

	internal sealed class JaysonKeyValue<TKey, TValue>
	{
		public TKey Key;
		public TValue Value;
	}

	# endregion JaysonKeyValue<TKey, TValue>
}