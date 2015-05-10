using System;
using System.Runtime.Serialization;

namespace Sweet.Jayson
{
	# region JsonConverterException

	public class JaysonException : Exception
	{
		public JaysonException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected JaysonException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public JaysonException(string message)
			: base(message)
		{
		}

		public JaysonException()
			: base()
		{
		}
	}

	# endregion JsonConverterException
}

