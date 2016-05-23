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
        public const string InvalidComment = "Comment not allowed.";
        public const string InvalidCommentStart = "Invalid comment start.";
        public const string InvalidCommentTermination = "Invalid comment termination.";
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
