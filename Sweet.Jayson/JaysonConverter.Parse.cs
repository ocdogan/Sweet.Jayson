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
using System.Collections;
using System.Collections.Generic;
using System.Data;
#if !(NET3500 || NET3000 || NET2000)
using System.Dynamic;
#endif
using System.Globalization;
using System.Linq;
using System.Text;

namespace Sweet.Jayson
{
	# region JsonConverter

	public static partial class JaysonConverter
	{
		# region Parse

		# region Parse String

		private static int HexToUnicodeChar(string str, int start)
		{
			int result = 0;

			char ch;
			int decimalCh = 0;

			int to = start + 4;
			for (int i = start; i < to; i++)
			{
				ch = str[i];

				if (!(ch < '0' || ch > '9'))
				{
					decimalCh = ch - '0';
				}
				else if (!(ch < 'A' || ch > 'F'))
				{
					decimalCh = ch - JaysonConstants.CharA10;
				}
				else if (!(ch < 'a' || ch > 'f'))
				{
					decimalCh = ch - JaysonConstants.Chara10;
				}
				else
				{
					throw new JaysonException("Invalid unicode escaped character.");
				}

				result *= 16;
				result += decimalCh;
			}

			return result;
		}

		private static string ParseString(string str, ref int pos)
		{
			int length = str.Length;
			if (pos > length - 1)
			{
				throw new JaysonException("Invalid string termination.");
			}

			if (str[pos] == '"')
			{
				return String.Empty;
			}

			if (pos == length - 1)
			{
				throw new JaysonException("Invalid string termination.");
			}

			char ch;
			int unicodeChar;

			int len;
			int start = pos;
			bool terminated = false;

			StringBuilder charStore = new StringBuilder(20, int.MaxValue); // JaysonParserBuilderCache.Acquire(20);
			do
			{
				ch = str[pos];

				if (ch != '\\')
				{
					if (ch == '"')
					{
						terminated = true;
						break;
					}

					pos++;
					continue;
				}

				if (pos > length - 1)
				{
					throw new JaysonException("Invalid character in string.");
				}

				len = pos - start;
				if (len > 0)
				{
					charStore.Append(str, start, len);
				}

				switch (str[++pos])
				{
				case 'n':
					start = ++pos;
					charStore.Append('\n');
					break;
				case 'r':
					start = ++pos;
					charStore.Append('\r');
					break;
				case 't':
					start = ++pos;
					charStore.Append('\t');
					break;
				case '"':
					start = ++pos;
					charStore.Append('"');
					break;
				case '\\':
					start = ++pos;
					charStore.Append('\\');
					break;
				case 'u':
					if (pos < length - 4)
					{
						unicodeChar = HexToUnicodeChar(str, ++pos);

						pos += 4;
						start = pos;

						if (unicodeChar > -1)
						{
							charStore.Append((char)unicodeChar);
							break;
						}
					}

					throw new JaysonException("Invalid unicode string.");
				case '/':
					start = ++pos;
					charStore.Append('/');
					break;
				case 'b':
					start = ++pos;
					charStore.Append('\b');
					break;
				case 'f':
					start = ++pos;
					charStore.Append('\f');
					break;
				default:
					throw new JaysonException("Invalid unicode character.");
				}
			} while (pos < length);

			if (!terminated)
			{
				throw new JaysonException("Invalid string termination.");
			}

			len = pos - start;
			pos++;

			if (len > 0)
			{
				if (charStore.Length == 0)
				{
					return str.Substring(start, len);
				}

				charStore.Append(str, start, len);
			}
			// return JaysonParserBuilderCache.GetStringAndRelease(charStore);
			return charStore.ToString();
		}

		# endregion Parse String

		# region Parse Number

		# region ParseDouble

		private static double ParseDouble(string str, int start, int length, NumberStyles numStyle)
		{
			var pos = start;
			int end = start + length;
			double sign = 1;

			if ((numStyle & NumberStyles.AllowLeadingSign) == NumberStyles.AllowLeadingSign)
			{
				++pos;
				if (pos >= end)
				{
					throw new JaysonException("Invalid floating number.");
				}
				sign = -1;
			}

			char ch;
			double result = 0;

			while (true) // breaks inside on pos >= len or non-digit character
			{
				if (pos >= end)
				{
					return sign * result;
				}

				ch = str[pos++];
				if (ch < '0' || ch > '9')
				{
					break;
				}

				result = (result * 10d) + (ch - '0');
			}

			if (ch == '.')
			{
				double exp = 0.1d;
				while (pos < end)
				{
					ch = str[pos++];
					if (ch < '0' || ch > '9')
					{
						if (ch == 'E' || ch == 'e')
							break;

						throw new JaysonException("Invalid floating number.");
					}

					result += (ch - '0') * exp;
					exp *= 0.1d;
				}
			}

			// scientific part
			if (pos < end)
			{
				if (!(ch == 'e' || ch == 'E'))
				{
					throw new JaysonException("Invalid floating number.");
				}

				int eValue = 0;
				bool eNegative = (str[++pos] == '-');

				for (++pos; pos < end; pos++)
				{
					ch = str[pos];
					if (ch < '0' || ch > '9')
					{
						throw new JaysonException("Invalid floating number.");
					}

					eValue = (eValue * 10) + (int)(ch - '0');
				}

				if (eValue > 0)
				{
					if (eNegative)
					{
						if (eValue < JaysonConstants.PowerOf10Long.Length)
						{
							result /= JaysonConstants.PowerOf10Long[eValue];
						}
						else
						{
							while (eValue-- > 0)
							{
								result /= 10;
							}
						}
					}
					else
					{
						if (eValue < JaysonConstants.PowerOf10Long.Length)
						{
							result *= JaysonConstants.PowerOf10Long[eValue];
						}
						else
						{
							while (eValue-- > 0)
							{
								result *= 10;
							}
						}
					}
				}
			}

			return sign * result;
		}

		# endregion ParseDouble

		# region ParseDecimal

		private static decimal ParseDecimal(string str, int start, int length, NumberStyles numStyle)
		{
			int pos = start;
			int end = start + length;

			bool negative = false;
			if ((numStyle & NumberStyles.AllowLeadingSign) == NumberStyles.AllowLeadingSign)
			{
				++pos;
				negative = true;

				if (pos >= end)
				{
					throw new JaysonException("Invalid floating number.");
				}
			}

			char ch = (char)0;
			long value = 0;
			int decimalPosition = end;

			for (; pos < end; pos++)
			{
				ch = str[pos];
				if (!(ch < '0' || ch > '9'))
				{
					value = (value * 10) + (long)(ch - '0');
					continue;
				}

				if (ch == '.')
				{
					if (decimalPosition != end)
					{
						throw new JaysonException("Invalid decimal number.");
					}
					decimalPosition = pos + 1;
					continue;
				}

				if (ch == 'E' || ch == 'e')
				{
					break;
				}

				throw new JaysonException("Invalid decimal number.");
			}

			int scale = pos - decimalPosition;

			// scientific part
			int eValue = 0;
			if (pos < end)
			{
				if (!(ch == 'e' || ch == 'E'))
				{
					throw new JaysonException("Invalid decimal number.");
				}

				bool eNegative = (str[++pos] == '-');

				for (++pos; pos < end; pos++)
				{
					ch = str[pos];
					if (!(ch < '0' || ch > '9'))
					{
						eValue = (eValue * 10) + (int)(ch - '0');
						continue;
					}

					throw new JaysonException("Invalid decimal number.");
				}

				if (eValue > 0)
				{
					if (eNegative)
					{
						scale += eValue;
						eValue = 0;
					}
					else
					{
						scale -= eValue;
						eValue = 0;

						if (scale < 0)
						{
							eValue = -scale;
							scale = 0;
						}
					}
				}
			}

			decimal result = new decimal((int)value, (int)(value >> 32), 0, negative, (byte)scale);

			if (eValue > 0)
			{
				if (eValue < JaysonConstants.PowerOf10Decimal.Length)
				{
					result *= JaysonConstants.PowerOf10Decimal[eValue];
				}
				else
				{
					while (eValue-- > 0)
					{
						result *= 10;
					}
				}
			}

			return result;
		}

		# endregion ParseDecimal

		# region ParseLong

		private static long ParseLong(string str, int start, int length, NumberStyles numberStyle)
		{
			char ch;
			if (length == 1)
			{
				ch = str[start];
				if (ch < '0' || ch > '9')
				{
					throw new JaysonException("Invalid number.");
				}
				return (long)(ch - '0');
			}

			int pos = start;
			int end = start + length;
			int sign = 1;

			// ch = str[start];
			// if (ch == '-')
			if ((numberStyle & NumberStyles.AllowLeadingSign) == NumberStyles.AllowLeadingSign)
			{
				pos++;
				sign = -1;
			}

			ch = (char)0;
			long value = 0;

			for (; pos < end; pos++)
			{
				ch = str[pos];
				if (!(ch < '0' || ch > '9'))
				{
					value = (value * 10) + (long)(ch - '0');
					continue;
				}

				if (ch == 'E' || ch == 'e')
				{
					break;
				}

				throw new JaysonException("Invalid number.");
			}

			// scientific part
			if (pos < end)
			{
				if (!(ch == 'e' || ch == 'E'))
				{
					throw new JaysonException("Invalid number.");
				}

				int eValue = 0;
				bool eNegative = (str[++pos] == '-');

				for (++pos; pos < end; pos++)
				{
					ch = str[pos];
					if (ch < '0' || ch > '9')
					{
						throw new JaysonException("Invalid number.");
					}

					eValue = (eValue * 10) + (int)(ch - '0');
				}

				if (eValue > 0)
				{
					if (eNegative)
					{
						if (eValue < JaysonConstants.PowerOf10Long.Length)
						{
							value /= JaysonConstants.PowerOf10Long[eValue];
						}
						else
						{
							value /= (long)Math.Pow(10, eValue);
						}
					}
					else
					{
						if (eValue < JaysonConstants.PowerOf10Long.Length)
						{
							value /= JaysonConstants.PowerOf10Long[eValue];
						}
						else
						{
							value *= (long)Math.Pow(10, eValue);
						}
					}
				}
			}

			return value * sign;
		}

		# endregion ParseLong

		private static object ParseNumber(JaysonDeserializationContext context, JaysonSerializationToken currToken)
		{
			int length = context.Length;
			if (context.Position < length)
			{
				JaysonNumberType numType = JaysonNumberType.Long;
				NumberStyles numStyle = NumberStyles.None;

				string str = context.Text;
				int start = context.Position;

				char ch = str[start];
				if (ch == '-')
				{
					context.Position++;
					numStyle |= NumberStyles.AllowLeadingSign;
				}
				else if (ch < '0' || ch > '9')
				{
					throw new JaysonException("Invalid number character.");
				}

				int digitCount = 0;
				bool exponent = false;
				char startChar = str[context.Position];

				do
				{
					ch = str[context.Position];

					if (!(ch < '0' || ch > '9'))
					{
						context.Position++;
						if (!exponent)
						{
							digitCount++;
						}
						continue;
					}

					if (ch == '.')
					{
						if (numType != JaysonNumberType.Long)
						{
							throw new JaysonException("Invalid number character.");
						}

						context.Position++;
						numType = JaysonNumberType.Double;
						numStyle |= NumberStyles.AllowDecimalPoint;
						continue;
					}

					if (ch == 'e' || ch == 'E')
					{
						if (numType == JaysonNumberType.Decimal)
						{
							throw new JaysonException("Invalid number character.");
						}

						context.Position++;
						numType = JaysonNumberType.Decimal;
						numStyle |= NumberStyles.AllowExponent;
						continue;
					}

					if (ch == '-' || ch == '+')
					{
						if (numType != JaysonNumberType.Decimal)
						{
							throw new JaysonException("Invalid number character.");
						}

						context.Position++;
						continue;
					}

					if ((currToken == JaysonSerializationToken.Value) &&
						(ch == ',' || ch == ']' || ch == '}' || JaysonCommon.IsWhiteSpace(ch)))
					{
						break;
					}

					throw new JaysonException("Invalid number character.");
				} while (context.Position < length);

				int len = context.Position - start;
				if (len > 0)
				{
					if (digitCount > 19 || (digitCount == 19 && startChar == '9'))
					{
						decimal d;
						if (!decimal.TryParse(str.Substring(start, len), numStyle, JaysonConstants.InvariantCulture, out d))
						{
							throw new JaysonException("Invalid decimal number.");
						}

						if ((numStyle == NumberStyles.None || numStyle == NumberStyles.AllowTrailingSign) &&
							d <= JaysonConstants.LongMaxValueAsDecimal && d >= JaysonConstants.LongMinValueAsDecimal)
						{
							return Convert.ToInt64(d);
						}

						if (context.Settings.ConvertDecimalToDouble) {
							return Convert.ToDouble(d);
						}
						return d;
					}

					switch (numType)
					{
					case JaysonNumberType.Long:
						long l = ParseLong(str, start, len, numStyle);
						if (l <= JaysonConstants.IntMaxValueAsLong && l >= JaysonConstants.IntMinValueAsLong)
						{
							return (int)l;
						}
						return l;
					case JaysonNumberType.Double:
						if (digitCount > 16)
						{
							decimal d = ParseDecimal(str, start, len, numStyle);

							if ((numStyle == NumberStyles.None || numStyle == NumberStyles.AllowTrailingSign) &&
								d <= JaysonConstants.LongMaxValueAsDecimal && d >= JaysonConstants.LongMinValueAsDecimal)
							{
								return decimal.ToInt64(d);
							}

							if (context.Settings.ConvertDecimalToDouble) {
								return Convert.ToDouble(d);
							}
							return d;
						}
						return ParseDouble(str, start, len, numStyle);
					case JaysonNumberType.Decimal:
						{
							decimal d = ParseDecimal(str, start, len, numStyle);
							if (context.Settings.ConvertDecimalToDouble) {
								return Convert.ToDouble(d);
							}
							return d;
						}
					default:
						break;
					}
				}
			}

			throw new JaysonException("Invalid number.");
		}

		# endregion Parse Number

		# region Helper Methods

		private static void EatWhitesAndCheckChar(JaysonDeserializationContext context, int charToCheck)
		{
			if (context.Position >= context.Length - 1)
			{
				throw new JaysonException("Invalid Json.");
			}

			int ch = (int)context.Text[context.Position];
			if (ch == charToCheck)
			{
				context.Position++;
			}
			else
			{
				if (JaysonCommon.IsWhiteSpace(ch))
				{
					string str = context.Text;
					int length = context.Length;

					do
					{
						context.Position++;
						if (context.Position < length)
						{
							ch = (int)str[context.Position];
							if (ch == charToCheck)
								return;

							if (JaysonCommon.IsWhiteSpace(ch))
								continue;
						}
						throw new JaysonException("Invalid Json.");
					} while (true);
				}
				throw new JaysonException("Invalid Json.");
			}
		}

		# endregion Helper Methods

		# region Parse List

		private static IList ListAsArray(IList list)
		{
			int count = list.Count;
			if (count > 0 && count <= 50) {
				object item;
				bool hasNull = false;
				Type arrayType = null;

				for (int i = 0; i < count; i++) {
					item = list [i];
					if (item == null) {
						hasNull = true;
					} else if (arrayType == null) {
						arrayType = item.GetType ();
					} else if (arrayType != item.GetType ()) {
						arrayType = typeof(object);
						break;
					}
				}

				if (arrayType == null ||
					(hasNull && !JaysonTypeInfo.IsClass (arrayType))) {
					arrayType = typeof(object);
				}

				if (list is IList<object> && arrayType == typeof(object)) {
					return ((IList<object>)list).ToArray();
				}

				Array typedArray = Array.CreateInstance (arrayType, count);
				list.CopyTo (typedArray, 0);
				return typedArray;
			}

			if (count == 0) {
				return new object[0];
			}

			if (list is IList<object>) {
				return ((IList<object>)list).ToArray ();
			}

			Array objectArray = Array.CreateInstance (typeof(object), count);
			list.CopyTo (objectArray, 0);
			return objectArray;
		}

		private static IList ParseList(JaysonDeserializationContext context)
		{
			EatWhitesAndCheckChar(context, '[');

			context.ObjectDepth++;

			if (context.Settings.MaxObjectDepth > 0 && 
				context.ObjectDepth > context.Settings.MaxObjectDepth) {
				throw new JaysonException(String.Format("Maximum object depth {0} exceeded.", 
					context.Settings.MaxObjectDepth));
			}

			IList result = 
				context.Settings.ArrayType == ArrayDeserializationType.ArrayList ? 
				(IList)(new ArrayList(10)) :
				new List<object>(10);

			string str = context.Text;
			int length = context.Length;

			char ch;
			JaysonSerializationToken token = JaysonSerializationToken.Value;

			while (context.Position < length)
			{
				ch = str[context.Position++];
				if (token == JaysonSerializationToken.Comma)
				{
					token = JaysonSerializationToken.Value;
					if (ch == ',' || ch == ']' || ch == '}')
					{
						throw new JaysonException("Invalid Json list.");
					}
				}

				switch (ch)
				{
				case '"':
					result.Add(ParseString(str, ref context.Position));
					break;
				case ',':
					if (result.Count == 0)
					{
						throw new JaysonException("Invalid Json list item.");
					}
					token = JaysonSerializationToken.Comma;
					break;
				case ']':
					if (context.Settings.ArrayType == ArrayDeserializationType.Array)
					{
						return ((IList<object>)result).ToArray ();
					}
					if (context.Settings.ArrayType == ArrayDeserializationType.ArrayDefined)
					{
						return ListAsArray (result);
					}
					return result;
				case '{':
					context.Position--;
					result.Add(ParseDictionary(context));
					break;
				case 'n':
					if (context.Position < length - 4 &&
						str[context.Position] == 'u' &&
						str[context.Position + 1] == 'l' &&
						str[context.Position + 2] == 'l')
					{
						result.Add(null);
						context.Position += 3;
						break;
					}
					throw new JaysonException("Invalid Json list item.");
				case 't':
					if (context.Position < length - 4 &&
						str[context.Position] == 'r' &&
						str[context.Position + 1] == 'u' &&
						str[context.Position + 2] == 'e')
					{
						result.Add(true);
						context.Position += 3;
						break;
					}
					throw new JaysonException("Invalid Json list item.");
				case 'f':
					if (context.Position < length - 5 &&
						str[context.Position] == 'a' &&
						str[context.Position + 1] == 'l' &&
						str[context.Position + 2] == 's' &&
						str[context.Position + 3] == 'e')
					{
						result.Add(false);
						context.Position += 4;
						break;
					}
					throw new JaysonException("Invalid Json list item.");
				case '[':
					context.Position--;
					result.Add(ParseList(context));
					break;
				default:
					if (!(ch < '0' || ch > '9') || ch == '-')
					{
						context.Position--;
						result.Add(ParseNumber(context, JaysonSerializationToken.Value));
						break;
					}
					if (JaysonCommon.IsWhiteSpace(ch))
						continue;
					throw new JaysonException("Invalid Json list item.");
				}
			}

			throw new JaysonException("Invalid Json list.");
		}

		# endregion Parse List

		# region Parse Dictionary

		private static IDictionary<string, object> ParseDictionary(JaysonDeserializationContext context)
		{
			EatWhitesAndCheckChar(context, '{');

			context.ObjectDepth++;

			if (context.Settings.MaxObjectDepth > 0 && 
				context.ObjectDepth > context.Settings.MaxObjectDepth) {
				throw new JaysonException(String.Format("Maximum object depth {0} exceeded.", 
					context.Settings.MaxObjectDepth));
			}

			#if !(NET3500 || NET3000 || NET2000)
			IDictionary<string, object> result = 
				(context.Settings.DictionaryType == DictionaryDeserializationType.Expando) ? 
				(IDictionary<string, object>)(new ExpandoObject()) :
				(context.Settings.CaseSensitive ? new Dictionary<string, object>(10) :
					new Dictionary<string, object>(10, StringComparer.OrdinalIgnoreCase));
			#else
			IDictionary<string, object> result = context.Settings.CaseSensitive ? new Dictionary<string, object>(10) :
				new Dictionary<string, object>(10, StringComparer.OrdinalIgnoreCase);
			#endif

			char ch;
			object value;
			string key = null;

			string str = context.Text;
			int length = context.Length;

			JaysonSerializationToken token = JaysonSerializationToken.Key;
			JaysonSerializationToken prevToken = JaysonSerializationToken.Undefined;

			while (context.Position < length)
			{
				ch = str[context.Position++];

				switch (token)
				{
				case JaysonSerializationToken.Key:
					switch (ch)
					{
					case '"':
						key = ParseString(str, ref context.Position);
						if (key == null)
						{
							throw new JaysonException("Invalid Json object key.");
						}

						prevToken = token;
						token = JaysonSerializationToken.Colon;
						continue;
					case ',':
						if (prevToken != JaysonSerializationToken.Value)
						{
							throw new JaysonException("Invalid Json object key.");
						}
						prevToken = JaysonSerializationToken.Comma;
						break;
					case '}':
						context.ObjectDepth--;
						return result;
					default:
						if (JaysonCommon.IsWhiteSpace(ch))
							continue;
						throw new JaysonException("Invalid Json object key.");
					}
					break;
				case JaysonSerializationToken.Colon:
					if (ch == ':')
					{
						prevToken = token;
						token = JaysonSerializationToken.Value;
						break;
					}
					if (JaysonCommon.IsWhiteSpace(ch))
						continue;
					throw new JaysonException("Invalid Json object value.");
				case JaysonSerializationToken.Value:
					switch (ch)
					{
					case '"':
						result[key] = ParseString(str, ref context.Position);
						if (key == "$type") {
							context.HasTypeInfo = true;
						}
						break;
					case '{':
						context.Position--;
						result[key] = ParseDictionary(context);
						break;
					case 'n':
						if (context.Position < length - 4 &&
							str[context.Position] == 'u' &&
							str[context.Position + 1] == 'l' &&
							str[context.Position + 2] == 'l')
						{
							result[key] = null;
							context.Position += 3;
							break;
						}
						throw new JaysonException("Invalid Json object value.");
					case 't':
						if (context.Position < length - 4 &&
							str[context.Position] == 'r' &&
							str[context.Position + 1] == 'u' &&
							str[context.Position + 2] == 'e')
						{
							result[key] = true;
							context.Position += 3;
							break;
						}
						throw new JaysonException("Invalid Json object value.");
					case 'f':
						if (context.Position < length - 5 &&
							str[context.Position] == 'a' &&
							str[context.Position + 1] == 'l' &&
							str[context.Position + 2] == 's' &&
							str[context.Position + 3] == 'e')
						{
							result[key] = false;
							context.Position += 4;
							break;
						}
						throw new JaysonException("Invalid Json object value.");
					case '[':
						context.Position--;
						result[key] = ParseList(context);
						break;
					default:
						if (!(ch < '0' || ch > '9') || ch == '-')
						{
							context.Position--;
							value = ParseNumber(context, JaysonSerializationToken.Value);

							result[key] = value;
							break;
						}
						if (JaysonCommon.IsWhiteSpace(ch))
							continue;
						throw new JaysonException("Invalid Json object value.");
					}

					key = null;
					token = JaysonSerializationToken.Key;
					prevToken = JaysonSerializationToken.Value;
					break;
				}
			}

			throw new JaysonException("Invalid Json object.");
		}

		# endregion Parse Dictionary

		public static object Parse(string str, JaysonDeserializationSettings settings = null)
		{
			if (String.IsNullOrEmpty(str))
			{
				throw new JaysonException("Empty string.");
			}

			return Parse(new JaysonDeserializationContext {
				Text = str,
				Length = str.Length,
				Settings = settings ?? JaysonDeserializationSettings.Default
			});
		}

		private static object Parse(JaysonDeserializationContext context)
		{
			string str = context.Text;
			int length = context.Length;

			while ((context.Position < length) &&
				JaysonCommon.IsWhiteSpace(str[context.Position]))
			{
				context.Position++;
			}

			if (context.Position >= length - 1)
			{
				throw new JaysonException("Invalid Json text.");
			}

			char ch = str[context.Position];

			switch (ch)
			{
			case '{':
				{
					return ParseDictionary(context);
				}
			case '[':
				{
					return ParseList(context);
				}
			case '"':
				context.Position++;
				return ParseString(str, ref context.Position);
			case 'n':
				if (context.Position == length - 4 &&
					str[context.Position + 1] == 'u' &&
					str[context.Position + 2] == 'l' &&
					str[context.Position + 3] == 'l')
				{
					return null;
				}
				throw new JaysonException("Invalid Json text.");
			case 't':
				if (context.Position == length - 4 &&
					str[context.Position + 1] == 'r' &&
					str[context.Position + 2] == 'u' &&
					str[context.Position + 3] == 'e')
				{
					return true;
				}
				throw new JaysonException("Invalid Json text.");
			case 'f':
				if (context.Position == length - 5 &&
					str[context.Position + 1] == 'a' &&
					str[context.Position + 2] == 'l' &&
					str[context.Position + 3] == 's' &&
					str[context.Position + 4] == 'e')
				{
					return false;
				}
				throw new JaysonException("Invalid Json text.");
			default:
				if (!(ch < '0' || ch > '9') || ch == '-')
				{
					object value = ParseNumber(context, JaysonSerializationToken.Undefined);

					return value;
				}
				throw new JaysonException("Invalid Json text.");
			}
		}

		# endregion Parse
	}

	# endregion JsonConverter
}