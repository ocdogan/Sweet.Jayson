using System;

namespace Sweet.Jayson
{
	public static class JaysonError
	{
		public const string ArgumentMustBeEnum = "Argument must be Enum base type or Enum";
		public const string CircularReferenceOn = "Circular reference on ";
		public const string EmptyString = "Empty string.";
		public const string GivenTypeIsIncorrect = "Given type is incorrect format to compare '{0}' with.";
		public const string InvalidBooleanString = "Invalid boolean string.";
		public const string InvalidCharInString = "Invalid character in string.";
		public const string InvalidDateFormat = "Invalid date format.";
		public const string InvalidDecimalNumber = "Invalid decimal number.";
		public const string InvalidFlotingNumber = "Invalid floating number.";
		public const string InvalidISO8601DateFormat = "Invalid ISO8601 date format.";
		public const string InvalidJson = "Invalid Json.";
		public const string InvalidJsonDateFormat = "Invalid Unix Epoch date format.";
		public const string InvalidJsonList = "Invalid Json list.";
		public const string InvalidJsonListItem = "Invalid Json list item.";
		public const string InvalidJsonObject = "Invalid Json object.";
		public const string InvalidJsonObjectKey = "Invalid Json object key.";
		public const string InvalidJsonObjectValue = "Invalid Json object value.";
		public const string InvalidNumber = "Invalid number.";
		public const string InvalidNumberChar = "Invalid number character.";
		public const string InvalidStringTermination = "Invalid string termination.";
		public const string InvalidTypeName = "Invalid type name.";
		public const string InvalidUnicodeChar = "Invalid unicode character.";
		public const string InvalidUnicodeEscapedChar = "Invalid unicode escaped character.";
		public const string InvalidUnicodeString = "Invalid unicode string.";
		public const string MaximumObjectDepthExceed = "Maximum object depth {0} exceeded.";
		public const string MissingMember = "Missing member: "; 
		public const string MissingMembers = "Missing members."; 
		public const string UnableToCastResult = "Unable to cast result to expected type.";
	}
}
