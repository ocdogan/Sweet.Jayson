using System;

namespace Sweet.Jayson
{
	# region DeserializationContext

	internal sealed class JaysonDeserializationContext
	{
		public string Text;
		public int Length = 0;
		public int Position = 0;
		public int ObjectDepth = 0;
		public bool HasTypeInfo = false;
		public JaysonDeserializationSettings Settings;
	}

	# endregion DeserializationContext
}