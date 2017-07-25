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

        # region Parse List

        private static IList ListAsArray(IList list)
        {
            var count = list.Count;
            if (count > 0 && count <= 50)
            {
                object item;
                var hasNull = false;
                Type arrayType = null;

                for (var i = 0; i < count; i++)
                {
                    item = list[i];
                    if (item == null)
                    {
                        hasNull = true;
                    }
                    else if (arrayType == null)
                    {
                        arrayType = item.GetType();
                    }
                    else if (arrayType != item.GetType())
                    {
                        arrayType = typeof(object);
                        break;
                    }
                }

                if (arrayType == null ||
                    (hasNull && !JaysonTypeInfo.IsClass(arrayType)))
                {
                    arrayType = typeof(object);
                }

                if (list is IList<object> && arrayType == typeof(object))
                {
                    return ((IList<object>)list).ToArray();
                }

                var typedArray = Array.CreateInstance(arrayType, count);
                list.CopyTo(typedArray, 0);

                return typedArray;
            }

            if (count == 0)
            {
                return new object[0];
            }

            if (list is IList<object>)
            {
                return ((IList<object>)list).ToArray();
            }

            var objectArray = Array.CreateInstance(typeof(object), count);
            list.CopyTo(objectArray, 0);

            return objectArray;
        }

        private static IList ParseList(JaysonDeserializationContext context)
        {
            var str = context.Text;
            var length = context.Length;

            var startedWithComment = false;
            if (context.Settings.CommentHandling == JaysonCommentHandling.ThrowError)
            {
                EatWhitesAndCheckChar(context, '[');
            }
            else
            {
                EatWhitesAndCheckChars(context, new int[] { '[', '/' });
                startedWithComment = (str[context.Position - 1] == '/') &&
                    (str[context.Position] == '/' || str[context.Position] == '*');

                if (startedWithComment)
                {
                    context.Position--;
                }
            }

            context.ObjectDepth++;
            ValidateObjectDepth(context);

            IList result =
                context.Settings.ArrayType == ArrayDeserializationType.ArrayList ?
                (IList)(new ArrayList(10)) :
                new List<object>(10);

            char ch;
            var token = JaysonSerializationToken.Value;

            while (context.Position < length)
            {
                ch = str[context.Position++];
                if (token == JaysonSerializationToken.Comma)
                {
                    token = JaysonSerializationToken.Value;
                    if (ch == ',' || ch == ']' || ch == '}')
                    {
                        throw new JaysonException(JaysonError.InvalidJsonList);
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
                            throw new JaysonException(JaysonError.InvalidJsonListItem);
                        }
                        token = JaysonSerializationToken.Comma;
                        break;
                    case ']':
                        if (context.Settings.ArrayType == ArrayDeserializationType.Array)
                        {
                            return ((IList<object>)result).ToArray();
                        }
                        if (context.Settings.ArrayType == ArrayDeserializationType.ArrayDefined)
                        {
                            return ListAsArray(result);
                        }
                        return result;
                    case '{':
                        context.Position--;
                        result.Add(ParseDictionary(context));
                        break;
                    case 'n':
                        {
                            var pos = context.Position;
                            if (pos < length - 3 &&
                                str[pos] == 'u' &&
                                str[pos + 1] == 'l' &&
                                str[pos + 2] == 'l')
                            {
                                result.Add(null);
                                context.Position += 3;
                                break;
                            }
                            throw new JaysonException(JaysonError.InvalidJsonListItem);
                        }
                    case 't':
                        {
                            var pos = context.Position;
                            if (pos < length - 3 &&
                                str[pos] == 'r' &&
                                str[pos + 1] == 'u' &&
                                str[pos + 2] == 'e')
                            {
                                result.Add(true);
                                context.Position += 3;
                                break;
                            }
                            throw new JaysonException(JaysonError.InvalidJsonListItem);
                        }
                    case 'f':
                        {
                            var pos = context.Position;
                            if (pos < length - 4 &&
                                str[pos] == 'a' &&
                                str[pos + 1] == 'l' &&
                                str[pos + 2] == 's' &&
                                str[pos + 3] == 'e')
                            {
                                result.Add(false);
                                context.Position += 4;
                                break;
                            }
                            throw new JaysonException(JaysonError.InvalidJsonListItem);
                        }
                    case '[':
                        context.Position--;
                        result.Add(ParseList(context));
                        break;
                    case '/':
                        ParseComment(context);
                        if (startedWithComment)
                        {
                            startedWithComment = false;
                            if (str[context.Position++] != '[')
                            {
                                throw new JaysonException(JaysonError.InvalidJsonObjectKey);
                            }
                        }
                        continue;
                    default:
                        if (!(ch < '0' || ch > '9') || ch == '-' || ch == '.' || 
                             ch == 'N' || ch == 'n' || ch == 'I' || ch == 'i' || ch == '∞')
                        {
                            context.Position--;
                            result.Add(ParseNumber(context));
                            break;
                        }
                        if (JaysonCommon.IsWhiteSpace(ch))
                            continue;
                        throw new JaysonException(JaysonError.InvalidJsonListItem);
                }
            }

            throw new JaysonException(JaysonError.InvalidJsonList);
        }

        # endregion Parse List

        # region Parse Dictionary

        private static IDictionary<string, object> ParseDictionary(JaysonDeserializationContext context)
        {
            var str = context.Text;
            var length = context.Length;

            var startedWithComment = false;
            if (context.Settings.CommentHandling == JaysonCommentHandling.ThrowError)
            {
                EatWhitesAndCheckChar(context, '{');
            }
            else
            {
                EatWhitesAndCheckChars(context, new int[] { '{', '/' });
                startedWithComment = (str[context.Position - 1] == '/') &&
                    (str[context.Position] == '/' || str[context.Position] == '*');

                if (startedWithComment)
                {
                    context.Position--;
                }
            }

            context.ObjectDepth++;
            ValidateObjectDepth(context);

#if !(NET3500 || NET3000 || NET2000)
            var result =
                (context.Settings.DictionaryType == DictionaryDeserializationType.Expando) ?
                (IDictionary<string, object>)(new ExpandoObject()) :
                (context.Settings.CaseSensitive ? new Dictionary<string, object>(10) :
                    new Dictionary<string, object>(10, StringComparer.OrdinalIgnoreCase));
#else
			var result = context.Settings.CaseSensitive ? new Dictionary<string, object>(10) :
				new Dictionary<string, object>(10, StringComparer.OrdinalIgnoreCase);
#endif

            char ch;
            object value;
            string key = null;

            var token = JaysonSerializationToken.Key;
            var prevToken = JaysonSerializationToken.Undefined;

            while (context.Position < length)
            {
                ch = str[context.Position++];

                switch (token)
                {
                    case JaysonSerializationToken.Key:
                        switch (ch)
                        {
                            case '"':
                                if (prevToken == JaysonSerializationToken.Value)
                                {
                                    throw new JaysonException(JaysonError.InvalidJsonObjectKey);
                                }

                                key = ParseString(str, ref context.Position);
                                if (key == null)
                                {
                                    throw new JaysonException(JaysonError.InvalidJsonObjectKey);
                                }

                                prevToken = token;
                                token = JaysonSerializationToken.Colon;
                                continue;
                            case ',':
                                if (prevToken != JaysonSerializationToken.Value)
                                {
                                    throw new JaysonException(JaysonError.InvalidJsonObjectKey);
                                }
                                prevToken = JaysonSerializationToken.Comma;
                                break;
                            case '}':
                                context.ObjectDepth--;
                                return result;
                            case '/':
                                ParseComment(context);
                                if (startedWithComment)
                                {
                                    startedWithComment = false;
                                    if (str[context.Position++] != '{')
                                    {
                                        throw new JaysonException(JaysonError.InvalidJsonObjectKey);
                                    }
                                }
                                continue;
                            default:
                                if (JaysonCommon.IsWhiteSpace(ch))
                                    continue;
                                throw new JaysonException(JaysonError.InvalidJsonObjectKey);
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
                        throw new JaysonException(JaysonError.InvalidJsonObjectValue);
                    case JaysonSerializationToken.Value:
                        switch (ch)
                        {
                            case '"':
                                result[key] = ParseString(str, ref context.Position);
                                if (key == "$type")
                                {
                                    context.HasTypeInfo = true;
                                }
                                break;
                            case '{':
                                context.Position--;
                                result[key] = ParseDictionary(context);
                                break;
                            case 'n':
                                {
                                    var pos = context.Position;
                                    if (pos < length - 3 &&
                                        str[pos] == 'u' &&
                                        str[pos + 1] == 'l' &&
                                        str[pos + 2] == 'l')
                                    {
                                        result[key] = null;
                                        context.Position += 3;
                                        break;
                                    }
                                    throw new JaysonException(JaysonError.InvalidJsonObjectValue);
                                }
                            case 't':
                                {
                                    var pos = context.Position;
                                    if (pos < length - 3 &&
                                        str[pos] == 'r' &&
                                        str[pos + 1] == 'u' &&
                                        str[pos + 2] == 'e')
                                    {
                                        result[key] = true;
                                        context.Position += 3;
                                        break;
                                    }
                                    throw new JaysonException(JaysonError.InvalidJsonObjectValue);
                                }
                            case 'f':
                                {
                                    var pos = context.Position;
                                    if (pos < length - 4 &&
                                        str[pos] == 'a' &&
                                        str[pos + 1] == 'l' &&
                                        str[pos + 2] == 's' &&
                                        str[pos + 3] == 'e')
                                    {
                                        result[key] = false;
                                        context.Position += 4;
                                        break;
                                    }
                                    throw new JaysonException(JaysonError.InvalidJsonObjectValue);
                                }
                            case '[':
                                context.Position--;
                                result[key] = ParseList(context);
                                break;
                            case '/':
                                ParseComment(context);
                                continue;
                            default:
                                if (!(ch < '0' || ch > '9') || ch == '-' || ch == '.' ||
                                     ch == 'N' || ch == 'n' || ch == 'I' || ch == 'i' || ch == '∞')
                                {
                                    context.Position--;
                                    value = ParseNumber(context);

                                    result[key] = value;
                                    break;
                                }
                                if (JaysonCommon.IsWhiteSpace(ch))
                                    continue;
                                throw new JaysonException(JaysonError.InvalidJsonObjectValue);
                        }

                        key = null;
                        token = JaysonSerializationToken.Key;
                        prevToken = JaysonSerializationToken.Value;
                        break;
                }
            }

            throw new JaysonException(JaysonError.InvalidJsonObject);
        }

        # endregion Parse Dictionary

        public static object Parse(string str, JaysonDeserializationSettings settings = null)
        {
            if (String.IsNullOrEmpty(str))
            {
                throw new JaysonException(JaysonError.EmptyString);
            }

            using (var context = new JaysonDeserializationContext
            {
                Text = str,
                Length = str.Length,
                Settings = settings ?? JaysonDeserializationSettings.Default
            })
            {
                return Parse(context);
            }
        }

        private static object Parse(JaysonDeserializationContext context)
        {
            var str = context.Text;
            var length = context.Length;

            while ((context.Position < length) &&
                JaysonCommon.IsWhiteSpace(str[context.Position]))
            {
                context.Position++;
            }

            if (context.Position >= length)
            {
                throw new JaysonException(JaysonError.InvalidJson);
            }

            var ch = str[context.Position];

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
                case 't':
                    {
                        var pos = context.Position;
                        if (pos == length - 4 &&
                            str[pos + 1] == 'r' &&
                            str[pos + 2] == 'u' &&
                            str[pos + 3] == 'e')
                        {
                            return true;
                        }
                        throw new JaysonException(JaysonError.InvalidJson);
                    }
                case 'f':
                    {
                        var pos = context.Position;
                        if (pos == length - 5 &&
                            str[pos + 1] == 'a' &&
                            str[pos + 2] == 'l' &&
                            str[pos + 3] == 's' &&
                            str[pos + 4] == 'e')
                        {
                            return false;
                        }
                        throw new JaysonException(JaysonError.InvalidJson);
                    }
                case 'n':
                    {
                        var pos = context.Position;
                        if (pos == length - 4 &&
                            str[pos + 1] == 'u' &&
                            str[pos + 2] == 'l' &&
                            str[pos + 3] == 'l')
                        {
                            return null;
                        }
                        throw new JaysonException(JaysonError.InvalidJson);
                    }
                case '/':
                    {
                        context.Position++;
                        ParseComment(context);
                        if (context.Position < length)
                        {
                            return Parse(context);
                        }
                        return null;
                    }
                default:
                    if (!(ch < '0' || ch > '9') || ch == '-' || ch == '.' ||
                         ch == 'N' || ch == 'n' || ch == 'I' || ch == 'i' || ch == '∞')
                    {
                        return ParseNumber(context, JaysonSerializationToken.Undefined);
                    }
                    throw new JaysonException(JaysonError.InvalidJson);
            }
        }

        # endregion Parse
    }

    # endregion JsonConverter
}