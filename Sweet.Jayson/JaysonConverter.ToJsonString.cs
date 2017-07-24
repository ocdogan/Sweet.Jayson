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
using System.Collections.Specialized;
using System.Data;
#if !(NET3500 || NET3000 || NET2000)
using System.Dynamic;
#endif
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Sweet.Jayson
{
    # region JsonConverter

    public static partial class JaysonConverter
    {
        # region ToJsonString

        # region Write Type Name

        private static void WriteIndent(JaysonSerializationContext context)
        {
            if (context.Settings.Formatting != JaysonFormatting.None)
            {
                context.Builder.Append(context.Settings.Formatting == JaysonFormatting.Tab ?
                    JaysonConstants.IndentationTabbed[context.ObjectDepth] :
                    JaysonConstants.Indentation[context.ObjectDepth]);
            }
        }

        private static void WriteIndent(JaysonSerializationContext context, int indentBy)
        {
            if (context.Settings.Formatting != JaysonFormatting.None)
            {
                context.Builder.Append(context.Settings.Formatting == JaysonFormatting.Tab ?
                    JaysonConstants.IndentationTabbed[indentBy] :
                    JaysonConstants.Indentation[indentBy]);
            }
        }

        private static void WriteObjectTypeName(Type objType, JaysonSerializationContext context)
        {
            var settings = context.Settings;

            var builder = context.Builder;
            if (settings.Formatting != JaysonFormatting.None)
            {
                builder.Append('{');
                WriteIndent(context);

                if (settings.UseGlobalTypeNames)
                {
                    builder.Append("\"$type\": ");
                    builder.Append(context.GlobalTypes.Register(objType));
                }
                else
                {
                    builder.Append("\"$type\": \"");
                    builder.Append(JaysonTypeInfo.GetTypeName(objType, settings.TypeNameInfo));
                    builder.Append('"');
                }
            }
            else
            {
                if (settings.UseGlobalTypeNames)
                {
                    builder.Append("{\"$type\":");
                    builder.Append(context.GlobalTypes.Register(objType));
                }
                else
                {
                    builder.Append("{\"$type\":\"");
                    builder.Append(JaysonTypeInfo.GetTypeName(objType, settings.TypeNameInfo));
                    builder.Append('"');
                }
            }
        }

        private static void WriteListTypeName(Type objType, JaysonSerializationContext context)
        {
            var settings = context.Settings;

            var builder = context.Builder;
            if (settings.Formatting != JaysonFormatting.None)
            {
                builder.Append('{');
                WriteIndent(context);

                if (settings.UseGlobalTypeNames)
                {
                    builder.Append("\"$type\": ");
                    builder.Append(context.GlobalTypes.Register(objType));
                }
                else
                {
                    builder.Append("\"$type\": \"");
                    builder.Append(JaysonTypeInfo.GetTypeName(objType, settings.TypeNameInfo));
                    builder.Append('"');
                }

                builder.Append(',');
                WriteIndent(context);
                builder.Append("\"$values\": [");
            }
            else
            {
                if (settings.UseGlobalTypeNames)
                {
                    builder.Append("{\"$type\":");
                    builder.Append(context.GlobalTypes.Register(objType));
                    builder.Append(",\"$values\":[");
                }
                else
                {
                    builder.Append("{\"$type\":\"");
                    builder.Append(JaysonTypeInfo.GetTypeName(objType, settings.TypeNameInfo));
                    builder.Append("\",\"$values\":[");
                }
            }
        }

        private static void WriteListTypeNameAndId(object obj, Type objType, JaysonSerializationContext context)
        {
            var settings = context.Settings;

            var builder = context.Builder;
            if (settings.Formatting != JaysonFormatting.None)
            {
                builder.Append('{');
                WriteIndent(context);

                if (settings.UseGlobalTypeNames)
                {
                    builder.Append("\"$type\": ");
                    builder.Append(context.GlobalTypes.Register(objType));
                }
                else
                {
                    builder.Append("\"$type\": \"");
                    builder.Append(JaysonTypeInfo.GetTypeName(objType, settings.TypeNameInfo));
                    builder.Append('"');
                }

                if (context.Settings.UseObjectReferencing)
                {
                    builder.Append(',');
                    WriteIndent(context);

                    builder.Append("\"$id\": ");
                    JaysonFormatter.Format(context.ReferenceMap.GetObjectId(obj), builder);
                }

                builder.Append(',');
                WriteIndent(context);
                builder.Append("\"$values\": [");
            }
            else
            {
                if (context.Settings.UseGlobalTypeNames)
                {
                    builder.Append("{\"$type\":");
                    builder.Append(context.GlobalTypes.Register(objType));
                }
                else
                {
                    builder.Append("{\"$type\":\"");
                    builder.Append(JaysonTypeInfo.GetTypeName(objType, settings.TypeNameInfo));
                    builder.Append('"');
                }

                if (context.Settings.UseObjectReferencing)
                {
                    builder.Append(",\"$id\": ");
                    JaysonFormatter.Format(context.ReferenceMap.GetObjectId(obj), builder);
                }

                builder.Append(",\"$values\":[");
            }
        }

        private static bool WritePrimitiveTypeName(Type objType, JaysonSerializationContext context)
        {
            try
            {
                if (context.SkipCurrentType &&
                    context.CurrentType == objType)
                {
                    return false;
                }

                var settings = context.Settings;

                var builder = context.Builder;
                if (settings.Formatting != JaysonFormatting.None)
                {
                    builder.Append('{');
                    WriteIndent(context);

                    if (settings.UseGlobalTypeNames)
                    {
                        builder.Append("\"$type\": ");
                        builder.Append(context.GlobalTypes.Register(objType));
                    }
                    else
                    {
                        builder.Append("\"$type\": \"");
                        builder.Append(JaysonTypeInfo.GetTypeName(objType, settings.TypeNameInfo));
                        builder.Append('"');
                    }

                    builder.Append(',');
                    WriteIndent(context);
                    builder.Append("\"$value\": ");
                }
                else
                {
                    if (settings.UseGlobalTypeNames)
                    {
                        builder.Append("{\"$type\":");
                        builder.Append(context.GlobalTypes.Register(objType));
                        builder.Append(",\"$value\":");
                    }
                    else
                    {
                        builder.Append("{\"$type\":\"");
                        builder.Append(JaysonTypeInfo.GetTypeName(objType, settings.TypeNameInfo));
                        builder.Append("\",\"$value\":");
                    }
                }
            }
            finally
            {
                context.CurrentType = null;
                context.SkipCurrentType = false;
            }
            return true;
        }

        private static bool WriteObjectType(object obj, JaysonSerializationContext context)
        {
            try
            {
                switch (context.Settings.TypeNames)
                {
                    case JaysonTypeNameSerialization.None:
                        break;
                    case JaysonTypeNameSerialization.Auto:
                        {
                            if (obj == null)
                            {
                                return false;
                            }

                            var objType = obj.GetType();

                            if (objType != context.CurrentType &&
                                objType != JaysonConstants.DefaultDictionaryType)
                            {
                                WriteObjectTypeName(objType, context);
                                return true;
                            }
                        }
                        break;
                    case JaysonTypeNameSerialization.All:
                        {
                            if (obj == null)
                            {
                                return false;
                            }

                            var objType = obj.GetType();
                            if (context.SkipCurrentType
                                && context.CurrentType == objType)
                            {
                                return false;
                            }

                            WriteObjectTypeName(objType, context);
                            return true;
                        }
                    case JaysonTypeNameSerialization.Objects:
                    case JaysonTypeNameSerialization.AllButNoPrimitive:
                        {
                            if (obj == null)
                            {
                                return false;
                            }

                            var objType = obj.GetType();
                            JaysonTypeInfo info = JaysonTypeInfo.GetTypeInfo(objType);

                            if (!info.JPrimitive)
                            {
                                if (context.SkipCurrentType
                                    && context.CurrentType == objType)
                                {
                                    return false;
                                }

                                WriteObjectTypeName(objType, context);
                                return true;
                            }
                        }
                        break;
                }
            }
            finally
            {
                context.CurrentType = null;
                context.SkipCurrentType = false;
            }
            return false;
        }

        private static bool WriteObjectType(Type objType, JaysonSerializationContext context)
        {
            try
            {
                switch (context.Settings.TypeNames)
                {
                    case JaysonTypeNameSerialization.None:
                        break;
                    case JaysonTypeNameSerialization.Auto:
                        {
                            if (objType != context.CurrentType &&
                                objType != JaysonConstants.DefaultDictionaryType)
                            {
                                WriteObjectTypeName(objType, context);
                                return true;
                            }
                        }
                        break;
                    case JaysonTypeNameSerialization.All:
                        {
                            if (context.SkipCurrentType
                                && context.CurrentType == objType)
                            {
                                return false;
                            }

                            WriteObjectTypeName(objType, context);
                            return true;
                        }
                    case JaysonTypeNameSerialization.Objects:
                    case JaysonTypeNameSerialization.AllButNoPrimitive:
                        {
                            JaysonTypeInfo info = JaysonTypeInfo.GetTypeInfo(objType);
                            if (!info.JPrimitive)
                            {
                                if (context.SkipCurrentType
                                    && context.CurrentType == objType)
                                {
                                    return false;
                                }

                                WriteObjectTypeName(objType, context);
                                return true;
                            }
                        }
                        break;
                }
            }
            finally
            {
                context.CurrentType = null;
                context.SkipCurrentType = false;
            }
            return false;
        }

        private static bool WriteListTypeAndId(object obj, JaysonSerializationContext context)
        {
            try
            {
                switch (context.Settings.TypeNames)
                {
                    case JaysonTypeNameSerialization.None:
                        {
                            if (context.Settings.UseObjectReferencing)
                            {
                                WriteListObjectId(context.ReferenceMap.GetObjectId(obj), context);
                                return true;
                            }
                        }
                        break;
                    case JaysonTypeNameSerialization.Auto:
                        {
                            if (obj == null)
                            {
                                return false;
                            }

                            var objType = obj.GetType();

                            if (objType != context.CurrentType &&
                                objType != JaysonConstants.DefaultListType)
                            {
                                WriteListTypeNameAndId(obj, objType, context);
                                return true;
                            }

                            if (context.Settings.UseObjectReferencing)
                            {
                                WriteListObjectId(context.ReferenceMap.GetObjectId(obj), context);
                                return true;
                            }
                        }
                        break;
                    case JaysonTypeNameSerialization.All:
                    case JaysonTypeNameSerialization.Arrays:
                    case JaysonTypeNameSerialization.AllButNoPrimitive:
                        {
                            if (obj == null)
                            {
                                return false;
                            }

                            var objType = obj.GetType();

                            if (context.SkipCurrentType &&
                                context.CurrentType == objType)
                            {
                                if (context.Settings.UseObjectReferencing)
                                {
                                    WriteListObjectId(context.ReferenceMap.GetObjectId(obj), context);
                                    return true;
                                }
                                return false;
                            }

                            WriteListTypeNameAndId(obj, objType, context);
                            return true;
                        }
                    default:
                        if (context.Settings.UseObjectReferencing)
                        {
                            WriteListObjectId(context.ReferenceMap.GetObjectId(obj), context);
                            return true;
                        }
                        break;
                }
            }
            finally
            {
                context.CurrentType = null;
                context.SkipCurrentType = false;
            }
            return false;
        }

        private static bool WriteListType(Type objType, JaysonSerializationContext context)
        {
            try
            {
                switch (context.Settings.TypeNames)
                {
                    case JaysonTypeNameSerialization.None:
                        break;
                    case JaysonTypeNameSerialization.Auto:
                        {
                            if (objType != context.CurrentType &&
                                objType != JaysonConstants.DefaultListType)
                            {
                                WriteListTypeName(objType, context);
                                return true;
                            }
                        }
                        break;
                    case JaysonTypeNameSerialization.All:
                    case JaysonTypeNameSerialization.Arrays:
                    case JaysonTypeNameSerialization.AllButNoPrimitive:
                        {
                            if (context.SkipCurrentType
                                && context.CurrentType == objType)
                            {
                                return false;
                            }

                            WriteListTypeName(objType, context);
                            return true;
                        }
                    default:
                        context.SkipCurrentType = false;
                        break;
                }
            }
            finally
            {
                context.CurrentType = null;
                context.SkipCurrentType = false;
            }
            return false;
        }

        private static bool WriteByteArrayType(JaysonSerializationContext context)
        {
            try
            {
                switch (context.Settings.TypeNames)
                {
                    case JaysonTypeNameSerialization.None:
                        break;
                    case JaysonTypeNameSerialization.Auto:
                        {
                            if (context.CurrentType != typeof(byte[]))
                            {
                                return WritePrimitiveTypeName(typeof(byte[]), context);
                            }
                        }
                        break;
                    case JaysonTypeNameSerialization.All:
                    case JaysonTypeNameSerialization.Arrays:
                    case JaysonTypeNameSerialization.AllButNoPrimitive:
                        {
                            if (context.SkipCurrentType
                                && context.CurrentType == typeof(byte[]))
                            {
                                return false;
                            }

                            return WritePrimitiveTypeName(typeof(byte[]), context);
                        }
                    default:
                        break;
                }
            }
            finally
            {
                context.CurrentType = null;
                context.SkipCurrentType = false;
            }
            return false;
        }

        # endregion Write Type Name

        # region Write Object Reference & Id

        private static void WriteObjectRefence(int id, JaysonSerializationContext context)
        {
            var builder = context.Builder;
            if (context.Settings.Formatting != JaysonFormatting.None)
            {
                builder.Append('{');
                WriteIndent(context, context.ObjectDepth + 1);

                builder.Append("\"$ref\": ");
                JaysonFormatter.Format(id, builder);
                WriteIndent(context);
                builder.Append('}');
            }
            else
            {
                builder.Append("{\"$ref\":");
                JaysonFormatter.Format(id, builder);
                builder.Append('}');
            }
        }

        private static void WriteListObjectId(int id, JaysonSerializationContext context)
        {
            var builder = context.Builder;
            if (context.Settings.Formatting != JaysonFormatting.None)
            {
                builder.Append('{');
                WriteIndent(context);

                builder.Append("\"$id\": ");
                JaysonFormatter.Format(id, builder);

                builder.Append(',');
                WriteIndent(context);
                builder.Append("\"$values\": [");
            }
            else
            {
                builder.Append("{\"$id\":");
                JaysonFormatter.Format(id, builder);
                builder.Append(",\"$values\":[");
            }
        }

        private static bool WriteReferenceOrId(object obj, JaysonSerializationContext context, bool isFirst, out bool referenced)
        {
            referenced = false;
            if (context.Settings.UseObjectReferencing)
            {
                var builder = context.Builder;

                var id = context.ReferenceMap.GetObjectId(obj, out referenced);
                if (context.Settings.Formatting != JaysonFormatting.None)
                {
                    if (!isFirst)
                    {
                        builder.Append(',');
                    }

                    WriteIndent(context);
                    builder.Append(referenced ? "\"$ref\": " : "\"$id\": ");
                }
                else
                {
                    if (isFirst)
                    {
                        builder.Append(referenced ? "\"$ref\":" : "\"$id\":");
                    }
                    else
                    {
                        builder.Append(referenced ? ",\"$ref\":" : ",\"$id\":");
                    }
                }

                JaysonFormatter.Format(id, builder);
                return false; // isFirst
            }
            return isFirst;
        }

        # endregion Write Object Reference & Id

        private static bool ValidObjectDepth(JaysonSerializationContext context)
        {
            var settings = context.Settings;
            if ((settings.MaxObjectDepth > 0) && (context.ObjectDepth > settings.MaxObjectDepth))
            {
                if (settings.RaiseErrorOnMaxObjectDepth)
                {
                    throw new JaysonException(String.Format(JaysonError.MaximumObjectDepthExceed, settings.MaxObjectDepth));
                }
                return false;
            }
            return true;
        }

        private static bool ValidObjectDepth(int currDepth, int maxDepth, bool raiseError)
        {
            if ((maxDepth > 0) && (currDepth > maxDepth))
            {
                if (raiseError)
                {
                    throw new JaysonException(String.Format(JaysonError.MaximumObjectDepthExceed, maxDepth));
                }
                return false;
            }
            return true;
        }

        private static void WriteKeyFast(string key, JaysonSerializationContext context, bool isFirst)
        {
            var settings = context.Settings;

            var builder = context.Builder;
            if (!isFirst)
            {
                builder.Append(',');
            }
            WriteIndent(context);

            builder.Append('"');
            builder.Append(settings.CaseSensitive ? key : JaysonCommon.AsciiToLower(key));

            if (settings.Formatting != JaysonFormatting.None)
            {
                builder.Append("\": ");
            }
            else
            {
                builder.Append('"');
                builder.Append(':');
            }
        }

        private static bool WriteKeyValueEntryFast(string key, bool value, JaysonSerializationContext context, bool isFirst)
        {
            WriteKeyFast(key, context, isFirst);
            context.Builder.Append(value ? JaysonConstants.True : JaysonConstants.False);
            return false; // isFirst
        }

        private static bool WriteKeyValueEntryFast(string key, int value, JaysonSerializationContext context, bool isFirst)
        {
            WriteKeyFast(key, context, isFirst);
            context.Builder.Append(value.ToString(JaysonConstants.InvariantCulture));
            return false; // isFirst
        }

        private static bool WriteKeyValueEntryFast(string key, long value, JaysonSerializationContext context, bool isFirst)
        {
            WriteKeyFast(key, context, isFirst);
            context.Builder.Append(value.ToString(JaysonConstants.InvariantCulture));
            return false; // isFirst
        }

        private static bool WriteKeyValueEntryFast(string key, string value, JaysonSerializationContext context, bool isFirst)
        {
            WriteKeyFast(key, context, isFirst);

            var builder = context.Builder;

            builder.Append('"');
            builder.Append(value);
            builder.Append('"');
            return false; // isFirst
        }

        private static bool WriteProperty(string propertyName, object value, Type expectedValueType,
            JaysonSerializationContext context, bool isFirst, bool forceNullValues = false)
        {
            if (!JaysonCommon.IsNull(value, context.Settings.FloatNanStrategy, context.Settings.FloatInfinityStrategy))
            {
                var builder = context.Builder;
                var settings = context.Settings;

                if (settings.IgnoreEmptyCollections)
                {
                    var collection = value as ICollection;
                    if ((collection != null) && (collection.Count == 0))
                    {
                        return isFirst;
                    }
                }

                if (!isFirst)
                {
                    builder.Append(',');
                }
                WriteIndent(context);

                builder.Append('"');
                builder.Append(settings.CaseSensitive ? propertyName : JaysonCommon.AsciiToLower(propertyName));

                if (settings.Formatting != JaysonFormatting.None)
                {
                    builder.Append("\": ");
                }
                else
                {
                    builder.Append('"');
                    builder.Append(':');
                }

                var valueType = value.GetType();
                var jtc = JaysonTypeInfo.GetJTypeCode(valueType);

                if (jtc == JaysonTypeCode.String || jtc == JaysonTypeCode.Bool)
                {
                    Format(value, valueType, context.Builder, settings, context.Formatter);
                    return false; // isFirst
                }

                if (jtc == JaysonTypeCode.Object)
                {
                    WriteJsonObject(value, valueType, expectedValueType, context);
                    return false; // isFirst
                }

                JaysonTypeNameSerialization jtns = settings.TypeNames;

                if (jtns != JaysonTypeNameSerialization.None &&
                    (expectedValueType == null || valueType != expectedValueType) &&
                    ((jtns == JaysonTypeNameSerialization.All && (JaysonTypeCode.JsonUnknown & jtc) == jtc) ||
                        (jtns == JaysonTypeNameSerialization.Auto && (JaysonTypeCode.AutoTyped & jtc) == jtc) ||
                        (jtns == JaysonTypeNameSerialization.AllButNoPrimitive && (JaysonTypeCode.Nullable & jtc) == jtc)))
                {
                    var typeWritten = false;
                    context.ObjectDepth++;
                    try
                    {
                        typeWritten = WritePrimitiveTypeName(valueType, context);
                        Format(value, valueType, builder, settings, context.Formatter);
                    }
                    finally
                    {
                        context.ObjectDepth--;
                        if (typeWritten)
                        {
                            WriteIndent(context);
                            builder.Append('}');
                        }
                    }
                    return false; // isFirst
                }

                Format(value, valueType, context.Builder, settings, context.Formatter);
                return false; // isFirst
            }
            
            if (forceNullValues || !context.Settings.IgnoreNullValues)
            {
                var builder = context.Builder;
                var settings = context.Settings;

                if (!isFirst)
                {
                    builder.Append(',');
                }
                WriteIndent(context);

                builder.Append('"');
                builder.Append(settings.CaseSensitive ? propertyName : JaysonCommon.AsciiToLower(propertyName));

                if (settings.Formatting != JaysonFormatting.None)
                {
                    builder.Append("\": null");
                }
                else
                {
                    builder.Append("\":null");
                }
                return false; // isFirst
            }

            return isFirst;
        }

        private static bool WriteKeyValueEntry(string key, object value, Type expectedValueType, JaysonSerializationContext context,
            bool isFirst, bool forceNullValues = false, bool ignoreEscape = false)
        {
            if (!JaysonCommon.IsNull(value, context.Settings.FloatNanStrategy, context.Settings.FloatInfinityStrategy))
            {
                var builder = context.Builder;
                var settings = context.Settings;

                if (!isFirst)
                {
                    builder.Append(',');
                }
                WriteIndent(context);

                builder.Append('"');

                if (ignoreEscape || !(settings.EscapeChars || settings.EscapeUnicodeChars))
                {
                    builder.Append(settings.CaseSensitive ? key : key.ToLower(JaysonConstants.InvariantCulture));
                }
                else
                {
                    JaysonFormatter.EncodeUnicodeString(settings.CaseSensitive ? key :
                        key.ToLower(JaysonConstants.InvariantCulture), builder, settings.EscapeUnicodeChars);
                }

                if (settings.Formatting != JaysonFormatting.None)
                {
                    builder.Append("\": ");
                }
                else
                {
                    builder.Append('"');
                    builder.Append(':');
                }

                var valueType = value.GetType();
                var jtc = JaysonTypeInfo.GetJTypeCode(valueType);

                if (jtc == JaysonTypeCode.String || jtc == JaysonTypeCode.Bool)
                {
                    Format(value, valueType, context.Builder, settings, context.Formatter);
                    return false; // isFirst
                }

                if (jtc == JaysonTypeCode.Object)
                {
                    WriteJsonObject(value, valueType, expectedValueType, context);
                    return false; // isFirst
                }

                var jtns = settings.TypeNames;

                if ((jtns != JaysonTypeNameSerialization.None) &&
                    (expectedValueType == null || valueType != expectedValueType) &&
                    ((jtns == JaysonTypeNameSerialization.All && (JaysonTypeCode.JsonUnknown & jtc) == jtc) ||
                        (jtns == JaysonTypeNameSerialization.Auto && (JaysonTypeCode.AutoTyped & jtc) == jtc) ||
                        (jtns == JaysonTypeNameSerialization.AllButNoPrimitive && (JaysonTypeCode.Nullable & jtc) == jtc)))
                {
                    var typeWritten = false;
                    context.ObjectDepth++;
                    try
                    {
                        typeWritten = WritePrimitiveTypeName(valueType, context);
                        Format(value, valueType, builder, settings, context.Formatter);
                    }
                    finally
                    {
                        context.ObjectDepth--;
                        if (typeWritten)
                        {
                            WriteIndent(context);
                            builder.Append('}');
                        }
                    }
                    return false; // isFirst
                }

                Format(value, valueType, context.Builder, settings, context.Formatter);
                return false; // isFirst
            }

            if (forceNullValues || !context.Settings.IgnoreNullValues)
            {
                var builder = context.Builder;
                var settings = context.Settings;

                if (!isFirst)
                {
                    builder.Append(',');
                }
                WriteIndent(context);

                builder.Append('"');

                if (ignoreEscape || !(settings.EscapeChars || settings.EscapeUnicodeChars))
                {
                    builder.Append(settings.CaseSensitive ? key : key.ToLower(JaysonConstants.InvariantCulture));
                }
                else
                {
                    JaysonFormatter.EncodeUnicodeString(settings.CaseSensitive ? key :
                        key.ToLower(JaysonConstants.InvariantCulture), builder, settings.EscapeUnicodeChars);
                }

                if (settings.Formatting != JaysonFormatting.None)
                {
                    builder.Append("\": null");
                }
                else
                {
                    builder.Append("\":null");
                }
                return false; // isFirst
            }

            return isFirst;
        }

        private static bool WriteEnumerableValue(object value, JaysonSerializationContext context, bool isFirst)
        {
            if (!JaysonCommon.IsNull(value, context.Settings.FloatNanStrategy, context.Settings.FloatInfinityStrategy))
            {
                Type valueType = value.GetType();
                if (CanWriteJsonObject(value, valueType, context))
                {
                    if (!isFirst)
                    {
                        context.Builder.Append(',');
                    }
                    WriteIndent(context);

                    WriteJsonObject(value, valueType, null, context);
                    return false; // isFirst
                }
            }
            else if (!context.Settings.IgnoreNullListItems)
            {
                if (!isFirst)
                {
                    context.Builder.Append(',');
                }
                WriteIndent(context);

                context.Builder.Append(JaysonConstants.Null);
                return false; // isFirst
            }

            return isFirst;
        }

        # region DataSet & DataTable

        private static void WriteDataRows(DataTable dataTable, JaysonSerializationContext context)
        {
            var rows = dataTable.Rows;
            if (rows.Count > 0)
            {
                var settings = context.Settings;
                var formatting = settings.Formatting != JaysonFormatting.None;

                var builder = context.Builder;
                if (formatting)
                {
                    builder.Append(',');
                    WriteIndent(context);
                    if (!settings.CaseSensitive)
                    {
                        builder.Append("\"rows\": [");
                    }
                    else
                    {
                        builder.Append("\"Rows\": [");
                    }
                }
                else
                {
                    if (!settings.CaseSensitive)
                    {
                        builder.Append(",\"rows\":[");
                    }
                    else
                    {
                        builder.Append(",\"Rows\":[");
                    }
                }

                var builderLen = builder.Length;

                context.ObjectDepth++;
                try
                {
                    if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                    {
                        return;
                    }

                    DataColumn dataColumn;
                    var columns = dataTable.Columns;

                    var colCount = columns.Count;
                    var columnsInfo = new List<JaysonTuple<DataColumn, JaysonTypeInfo>>();

                    for (var i = 0; i < colCount; i++)
                    {
                        dataColumn = columns[i];
                        columnsInfo.Add(new JaysonTuple<DataColumn, JaysonTypeInfo>(dataColumn,
                            JaysonTypeInfo.GetTypeInfo(dataColumn.DataType)));
                    }

                    DataRow dataRow;
                    object cellValue;

                    JaysonTypeCode colTypeCode;
                    JaysonTuple<DataColumn, JaysonTypeInfo> columnInfo;

                    var rowCount = rows.Count;
                    var formatter = context.Formatter;
                    var escapeChars = settings.EscapeChars;
                    var escapeUnicodeChars = settings.EscapeUnicodeChars;

                    context.ObjectDepth++;
                    try
                    {
                        for (var i = 0; i < rowCount; i++)
                        {
                            if (i > 0)
                            {
                                builder.Append(',');
                            }
                            if (formatting)
                            {
                                WriteIndent(context, context.ObjectDepth - 1);
                            }
                            builder.Append('[');

                            try
                            {
                                dataRow = rows[i];
                                for (var j = 0; j < colCount; j++)
                                {
                                    if (j > 0)
                                    {
                                        builder.Append(',');
                                    }
                                    if (formatting)
                                    {
                                        WriteIndent(context);
                                    }

                                    columnInfo = columnsInfo[j];
                                    cellValue = dataRow[columnInfo.Item1];

                                    if (JaysonCommon.IsNull(cellValue, settings.FloatNanStrategy, settings.FloatInfinityStrategy))
                                    {
                                        builder.Append(JaysonConstants.Null);
                                    }
                                    else
                                    {
                                        colTypeCode = columnInfo.Item2.JTypeCode;

                                        switch (colTypeCode)
                                        {
                                            case JaysonTypeCode.String:
                                                JaysonFormatter.FormatString((string)cellValue, builder,
                                                    escapeChars, escapeUnicodeChars);
                                                break;
                                            case JaysonTypeCode.Bool:
                                                builder.Append((bool)cellValue ? JaysonConstants.True : JaysonConstants.False);
                                                break;
                                            case JaysonTypeCode.BoolNullable:
                                                builder.Append(((bool?)cellValue).Value ? JaysonConstants.True : JaysonConstants.False);
                                                break;
                                            case JaysonTypeCode.DateTime:
                                                formatter.Format((DateTime)cellValue, builder);
                                                break;
                                            case JaysonTypeCode.DateTimeNullable:
                                                formatter.Format(((DateTime?)cellValue).Value, builder);
                                                break;
                                            default:
                                                if (colTypeCode != JaysonTypeCode.Object)
                                                {
                                                    formatter.Format(cellValue, builder);
                                                }
                                                else
                                                {
                                                    context.SkipCurrentType = true;
                                                    context.CurrentType = columnInfo.Item2.Type;

                                                    WriteJsonObject(cellValue, cellValue.GetType(), null, context);
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                if (formatting && (colCount > 0))
                                {
                                    WriteIndent(context, context.ObjectDepth - 1);
                                }
                                builder.Append(']');
                            }
                        }
                    }
                    finally
                    {
                        context.ObjectDepth--;
                    }
                }
                finally
                {
                    context.ObjectDepth--;
                    if (formatting && (builder.Length > builderLen))
                    {
                        WriteIndent(context);
                    }
                    builder.Append(']');
                }
            }
        }

        private static bool WriteDataColumnNames(string columnsName, DataColumn[] columns,
            JaysonSerializationContext context, bool isFirst)
        {
            if (columns == null || columns.Length == 0)
            {
                return isFirst;
            }

            var settings = context.Settings;
            var formatting = settings.Formatting != JaysonFormatting.None;

            var builder = context.Builder;
            var formatter = context.Formatter;

            if (!isFirst)
            {
                builder.Append(',');
            }
            if (formatting)
            {
                WriteIndent(context);
            }
            if (!settings.CaseSensitive)
            {
                formatter.Format(columnsName.ToLower(JaysonConstants.InvariantCulture), builder);
            }
            else
            {
                formatter.Format(columnsName, builder);
            }
            if (formatting)
            {
                builder.Append(": [");
            }
            else
            {
                builder.Append(":[");
            }

            var builderLen = builder.Length;

            context.ObjectDepth++;
            try
            {
                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return false; // isFirst
                }

                DataColumn column;
                var columnCount = columns.Length;

                for (var i = 0; i < columnCount; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(',');
                    }
                    if (formatting)
                    {
                        WriteIndent(context);
                    }

                    column = columns[i];
                    if (column.ColumnName == null)
                    {
                        builder.Append(JaysonConstants.Null);
                    }
                    else
                    {
                        formatter.Format(column.ColumnName, builder);
                    }
                }
            }
            finally
            {
                context.ObjectDepth--;
                if (formatting && (builder.Length > builderLen))
                {
                    WriteIndent(context);
                }
                builder.Append(']');
            }
            return false; // isFirst
        }

        private static void WriteDataColumns(DataTable dataTable, JaysonSerializationContext context)
        {
            var columns = dataTable.Columns;
            var columnCount = columns.Count;

            if (columnCount > 0)
            {
                var settings = context.Settings;
                var formatting = settings.Formatting != JaysonFormatting.None;

                var builder = context.Builder;
                if (formatting)
                {
                    builder.Append(',');
                    WriteIndent(context);
                    if (!settings.CaseSensitive)
                    {
                        builder.Append("\"cols\": [");
                    }
                    else
                    {
                        builder.Append("\"Cols\": [");
                    }
                }
                else
                {
                    if (!settings.CaseSensitive)
                    {
                        builder.Append(",\"cols\":[");
                    }
                    else
                    {
                        builder.Append(",\"Cols\":[");
                    }
                }

                var builderLen = builder.Length;

                context.ObjectDepth++;
                try
                {
                    if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                    {
                        return;
                    }

                    bool isFirst;
                    DataColumn column;
                    var defaultNamespace = dataTable.Namespace;

                    int iValue;
                    bool bValue;
                    long lValue;
                    string sValue;
                    MappingType mValue;

                    for (var i = 0; i < columnCount; i++)
                    {
                        if (i > 0)
                        {
                            builder.Append(',');
                        }
                        if (formatting)
                        {
                            WriteIndent(context);
                        }
                        builder.Append('{');

                        context.ObjectDepth++;
                        try
                        {
                            column = columns[i];

                            isFirst = true;
                            bValue = column.AllowDBNull;
                            if (!bValue)
                            {
                                isFirst = WriteKeyValueEntryFast("Adbn", bValue, context, isFirst);
                            }

                            bValue = column.AutoIncrement;
                            if (column.AutoIncrement)
                            {
                                isFirst = WriteKeyValueEntryFast("Ai", bValue, context, isFirst);
                            }

                            lValue = column.AutoIncrementSeed;
                            if (lValue != 0)
                            {
                                isFirst = WriteKeyValueEntryFast("Aisd", lValue, context, isFirst);
                            }

                            lValue = column.AutoIncrementStep;
                            if (lValue != 1)
                            {
                                isFirst = WriteKeyValueEntryFast("Aistp", lValue, context, isFirst);
                            }

                            sValue = column.Caption;
                            if (!String.IsNullOrEmpty(sValue) && sValue != column.ColumnName)
                            {
                                isFirst = WriteProperty("Cap", sValue, null, context, isFirst);
                            }

                            mValue = column.ColumnMapping;
                            if (mValue != MappingType.Element)
                            {
                                isFirst = WriteProperty("Cm", mValue, null, context, isFirst);
                            }

                            sValue = column.ColumnName;
                            if (!String.IsNullOrEmpty(sValue))
                            {
                                isFirst = WriteProperty("Cn", sValue, null, context, isFirst);
                            }

                            isFirst = WriteKeyValueEntryFast("Dt",
                                JaysonTypeInfo.GetTypeName(column.DataType, JaysonTypeNameInfo.TypeNameWithAssembly),
                                context, isFirst);

                            sValue = column.Expression;
                            if (!String.IsNullOrEmpty(sValue))
                            {
                                isFirst = WriteProperty("Exp", sValue, null, context, isFirst);
                            }

                            iValue = column.MaxLength;
                            if (iValue != -1)
                            {
                                isFirst = WriteKeyValueEntryFast("Ml", iValue, context, isFirst);
                            }

                            sValue = column.Namespace;
                            if (!String.IsNullOrEmpty(sValue) && sValue != defaultNamespace)
                            {
                                isFirst = WriteKeyValueEntry("Ns", column.Namespace, null, context, isFirst);
                            }

                            isFirst = WriteKeyValueEntryFast("Ord", column.Ordinal, context, isFirst);

                            sValue = column.Prefix;
                            if (!String.IsNullOrEmpty(sValue))
                            {
                                isFirst = WriteProperty("Pfx", sValue, null, context, isFirst);
                            }

                            bValue = column.ReadOnly;
                            if (bValue)
                            {
                                isFirst = WriteKeyValueEntryFast("Ro", bValue, context, isFirst);
                            }

                            bValue = column.Unique;
                            if (bValue)
                            {
                                isFirst = WriteKeyValueEntryFast("Uq", bValue, context, isFirst);
                            }

                            if (column.ExtendedProperties.Count > 0)
                            {
                                context.SkipCurrentType = true;
                                context.CurrentType = typeof(PropertyCollection);
                                isFirst = WriteProperty("Ep", column.ExtendedProperties, null, context, isFirst);
                            }
                        }
                        finally
                        {
                            context.ObjectDepth--;
                            if (formatting)
                            {
                                WriteIndent(context);
                            }
                            builder.Append('}');
                        }
                    }
                }
                finally
                {
                    context.ObjectDepth--;
                    if (formatting && (builder.Length > builderLen))
                    {
                        WriteIndent(context);
                    }
                    builder.Append(']');
                }
            }
        }

        private static void WriteDataTable(DataTable dataTable, JaysonSerializationContext context)
        {
            var settings = context.Settings;

            context.ObjectDepth++;
            var builder = context.Builder;
            try
            {
                var isFirst = !WriteObjectType(dataTable, context);
                if (isFirst)
                {
                    builder.Append('{');
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                bool referenced;
                isFirst = WriteReferenceOrId(dataTable, context, isFirst, out referenced);
                if (referenced)
                {
                    return;
                }

                isFirst = WriteKeyValueEntry("$dt", "Tbl", null, context, isFirst);
                
                var bValue = dataTable.CaseSensitive;
                if (bValue)
                {
                    isFirst = WriteKeyValueEntryFast("Cs", bValue, context, isFirst);
                }

                var sValue = dataTable.DisplayExpression;
                if (!String.IsNullOrEmpty(sValue))
                {
                    isFirst = WriteProperty("De", sValue, null, context, isFirst);
                }

                var cValue = dataTable.Locale;
                if (!ReferenceEquals(cValue, CultureInfo.InvariantCulture))
                {
                    isFirst = WriteKeyValueEntryFast("Lcl", cValue.Name, context, isFirst);
                }

                sValue = dataTable.Namespace;
                if (!String.IsNullOrEmpty(sValue))
                {
                    isFirst = WriteProperty("Ns", sValue, null, context, isFirst);
                }

                sValue = dataTable.Prefix;
                if (!String.IsNullOrEmpty(sValue))
                {
                    isFirst = WriteProperty("Pfx", sValue, null, context, isFirst);
                }

                var columns = dataTable.PrimaryKey;
                if (columns != null && columns.Length > 0)
                {
                    isFirst = WriteDataColumnNames("Pk", columns, context, isFirst);
                }

                sValue = dataTable.TableName;
                if (!String.IsNullOrEmpty(sValue))
                {
                    isFirst = WriteProperty("Tn", sValue, null, context, isFirst);
                }

                if (dataTable.ExtendedProperties.Count > 0)
                {
                    context.SkipCurrentType = true;
                    context.CurrentType = typeof(PropertyCollection);
                    isFirst = WriteProperty("Ep", dataTable.ExtendedProperties, null, context, isFirst);
                }

                WriteDataColumns(dataTable, context);
                WriteDataRows(dataTable, context);
            }
            finally
            {
                context.ObjectDepth--;
                WriteIndent(context);
                builder.Append('}');
            }
        }

        private static bool WriteDataRelations(DataSet dataSet, JaysonSerializationContext context, bool isFirst)
        {
            var relations = dataSet.Relations;
            if (relations.Count == 0)
            {
                return isFirst;
            }

            var relationCount = relations.Count;

            var builder = context.Builder;
            var settings = context.Settings;
            var formatting = settings.Formatting != JaysonFormatting.None;

            if (!isFirst)
            {
                builder.Append(',');
            }
            if (formatting)
            {
                WriteIndent(context);
                if (!settings.CaseSensitive)
                {
                    builder.Append("\"rel\": [");
                }
                else
                {
                    builder.Append("\"Rel\": [");
                }
            }
            else
            {
                if (!settings.CaseSensitive)
                {
                    builder.Append("\"rel\":[");
                }
                else
                {
                    builder.Append("\"Rel\":[");
                }
            }

            var builderLen = builder.Length;

            context.ObjectDepth++;
            try
            {
                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return false; // isFirst
                }

                bool bValue;
                string sValue;
                DataTable dValue;
                DataRelation relation;

                var defaultNamespace = dataSet.Namespace;

                context.ObjectDepth++;
                try
                {
                    for (var i = 0; i < relationCount; i++)
                    {
                        if (i > 0)
                        {
                            builder.Append(',');
                        }
                        if (formatting)
                        {
                            WriteIndent(context, context.ObjectDepth - 1);
                        }
                        builder.Append('{');

                        try
                        {
                            relation = relations[i];

                            isFirst = true;
                            sValue = relation.RelationName;
                            if (!String.IsNullOrEmpty(sValue))
                            {
                                isFirst = WriteProperty("Rn", sValue, null, context, isFirst);
                            }

                            isFirst = WriteDataColumnNames("CCol", relation.ChildColumns, context, isFirst);

                            dValue = relation.ChildTable;
                            if (dValue != null)
                            {
                                isFirst = WriteProperty("CTab", dValue.TableName, null, context, isFirst);

                                sValue = dValue.Namespace;
                                if (!String.IsNullOrEmpty(sValue))
                                {
                                    isFirst = WriteProperty("CTabNs", sValue, null, context, isFirst);
                                }
                            }

                            bValue = relation.Nested;
                            if (bValue)
                            {
                                isFirst = WriteKeyValueEntryFast("Nst", bValue, context, isFirst);
                            }

                            isFirst = WriteDataColumnNames("PCol", relation.ParentColumns, context, isFirst);

                            dValue = relation.ParentTable;
                            if (dValue != null)
                            {
                                isFirst = WriteProperty("PTab", dValue.TableName, null, context, isFirst);

                                sValue = dValue.Namespace;
                                if (!String.IsNullOrEmpty(sValue))
                                {
                                    isFirst = WriteProperty("PTabNs", sValue, null, context, isFirst);
                                }
                            }

                            if (relation.ExtendedProperties.Count > 0)
                            {
                                context.SkipCurrentType = true;
                                context.CurrentType = typeof(PropertyCollection);
                                isFirst = WriteProperty("Ep", relation.ExtendedProperties, null, context, isFirst);
                            }
                        }
                        finally
                        {
                            if (formatting)
                            {
                                WriteIndent(context, context.ObjectDepth - 1);
                            }
                            builder.Append('}');
                        }
                    }
                }
                finally
                {
                    context.ObjectDepth--;
                }
            }
            finally
            {
                context.ObjectDepth--;
                if (formatting && (builder.Length > builderLen))
                {
                    WriteIndent(context);
                }
                builder.Append(']');
            }
            return false; // isFirst
        }

        private static bool WriteDataTables(DataSet dataSet, JaysonSerializationContext context, bool isFirst)
        {
            var tables = dataSet.Tables;
            if (tables.Count == 0)
            {
                return isFirst;
            }

            var tableCount = tables.Count;

            var builder = context.Builder;
            var settings = context.Settings;
            var formatting = settings.Formatting != JaysonFormatting.None;

            if (!isFirst)
            {
                builder.Append(',');
            }
            if (formatting)
            {
                WriteIndent(context);
                if (!settings.CaseSensitive)
                {
                    builder.Append("\"tab\": [");
                }
                else
                {
                    builder.Append("\"Tab\": [");
                }
            }
            else
            {
                if (!settings.CaseSensitive)
                {
                    builder.Append("\"tab\":[");
                }
                else
                {
                    builder.Append("\"Tab\":[");
                }
            }

            var builderLen = builder.Length;

            context.ObjectDepth++;
            try
            {
                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return false; // isFirst
                }

                for (var i = 0; i < tableCount; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(',');
                    }
                    if (formatting)
                    {
                        WriteIndent(context);
                    }

                    WriteDataTable(tables[i], context);
                }
            }
            finally
            {
                context.ObjectDepth--;
                if (formatting && (builder.Length > builderLen))
                {
                    WriteIndent(context);
                }
                builder.Append(']');
            }
            return false; // isFirst
        }

        private static void WriteDataSet(DataSet dataSet, JaysonSerializationContext context)
        {
            var settings = context.Settings;
            var formatting = settings.Formatting != JaysonFormatting.None;

            context.ObjectDepth++;
            var builder = context.Builder;
            try
            {
                var isFirst = !WriteObjectType(dataSet, context);
                if (isFirst)
                {
                    builder.Append('{');
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                bool referenced;
                isFirst = WriteReferenceOrId(dataSet, context, isFirst, out referenced);
                if (referenced)
                {
                    return;
                }

                isFirst = WriteKeyValueEntry("$dt", "Ds", null, context, isFirst);
                
                var bValue = dataSet.CaseSensitive;
                if (bValue)
                {
                    isFirst = WriteKeyValueEntryFast("Cs", bValue, context, isFirst);
                }

                var sValue = dataSet.DataSetName;
                if (!String.IsNullOrEmpty(sValue))
                {
                    isFirst = WriteProperty("Dsn", sValue, null, context, isFirst);
                }

                bValue = dataSet.EnforceConstraints;
                if (!bValue)
                {
                    isFirst = WriteKeyValueEntryFast("Ec", bValue, context, isFirst);
                }

                var cValue = dataSet.Locale;
                if (!ReferenceEquals(cValue, CultureInfo.InvariantCulture))
                {
                    isFirst = WriteKeyValueEntryFast("Lcl", cValue.Name, context, isFirst);
                }

                sValue = dataSet.Namespace;
                if (!String.IsNullOrEmpty(sValue))
                {
                    isFirst = WriteProperty("Ns", sValue, null, context, isFirst);
                }

                sValue = dataSet.Prefix;
                if (!String.IsNullOrEmpty(sValue))
                {
                    isFirst = WriteProperty("Prefix", sValue, null, context, isFirst);
                }

                var rValue = dataSet.RemotingFormat;
                if (rValue != default(SerializationFormat))
                {
                    isFirst = WriteProperty("Rf", rValue, null, context, isFirst);
                }

                SchemaSerializationMode scValue = dataSet.SchemaSerializationMode;
                if (scValue != SchemaSerializationMode.IncludeSchema)
                {
                    isFirst = WriteProperty("Ssm", scValue, null, context, isFirst);
                }

                if (dataSet.ExtendedProperties.Count > 0)
                {
                    context.SkipCurrentType = true;
                    context.CurrentType = typeof(PropertyCollection);
                    isFirst = WriteProperty("Ep", dataSet.ExtendedProperties, null, context, isFirst);
                }

                isFirst = WriteDataRelations(dataSet, context, isFirst);
                isFirst = WriteDataTables(dataSet, context, isFirst);
            }
            finally
            {
                context.ObjectDepth--;
                if (formatting)
                {
                    WriteIndent(context);
                }
                builder.Append('}');
            }
        }

        # endregion DataSet & DataTable

        private static IEnumerable<JaysonKeyValue<object, object>> GetDictionaryEntries(IDictionary source,
            JaysonSerializationContext context)
        {
            string key;
            object value;
            object keyObj;

            var settings = context.Settings;

            var filter = context.Filter;
            var canFilter = (filter != null);

            foreach (DictionaryEntry dEntry in source)
            {
                value = dEntry.Value;

                keyObj = dEntry.Key;
                key = (keyObj as string) ?? keyObj.ToString();

                if ((value != null) && canFilter)
                {
                    value = filter(key, value);
                }

                if ((value != null) || !settings.IgnoreNullValues)
                {
                    yield return new JaysonKeyValue<object, object> { Key = keyObj, Value = value };
                }
            }
        }

        private static void WriteType(Type obj, JaysonSerializationContext context)
        {
            var settings = context.Settings;

            context.ObjectDepth++;
            var builder = context.Builder;
            try
            {
                var isFirst = !WriteObjectType((object)obj, context);
                if (isFirst)
                {
                    builder.Append('{');
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                bool referenced;
                isFirst = WriteReferenceOrId(obj, context, isFirst, out referenced);
                if (referenced)
                {
                    return;
                }

                WriteKeyValueEntry("QualifiedName", JaysonTypeInfo.GetTypeName(obj, JaysonTypeNameInfo.TypeNameWithAssembly),
                    typeof(string), context, isFirst);
            }
            finally
            {
                context.ObjectDepth--;
                WriteIndent(context);
                builder.Append('}');
            }
        }

        private static void WriteMemberInfo(MemberInfo obj, JaysonSerializationContext context)
        {
            var settings = context.Settings;

            context.ObjectDepth++;
            var builder = context.Builder;
            try
            {
                var isFirst = !WriteObjectType((object)obj, context);
                if (isFirst)
                {
                    builder.Append('{');
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                bool referenced;
                isFirst = WriteReferenceOrId(obj, context, isFirst, out referenced);
                if (referenced)
                {
                    return;
                }

                isFirst = WriteKeyValueEntry("QualifiedName", JaysonTypeInfo.GetTypeName(obj.DeclaringType, JaysonTypeNameInfo.TypeNameWithAssembly),
                    typeof(string), context, isFirst);

                if (obj.ReflectedType != null && obj.ReflectedType != obj.DeclaringType)
                {
                    isFirst = WriteKeyValueEntry("ReflectedType", JaysonTypeInfo.GetTypeName(obj.ReflectedType, JaysonTypeNameInfo.TypeNameWithAssembly),
                        typeof(string), context, isFirst);
                }

                isFirst = WriteKeyValueEntry("MemberName", obj.Name, typeof(string), context, isFirst);
            }
            finally
            {
                context.ObjectDepth--;
                WriteIndent(context);
                builder.Append('}');
            }
        }

        private static void WriteParameterInfo(ParameterInfo obj, JaysonSerializationContext context)
        {
            var settings = context.Settings;

            context.ObjectDepth++;
            var builder = context.Builder;
            try
            {
                var isFirst = !WriteObjectType((object)obj, context);
                if (isFirst)
                {
                    builder.Append('{');
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                bool referenced;
                isFirst = WriteReferenceOrId(obj, context, isFirst, out referenced);
                if (referenced)
                {
                    return;
                }

                WriteKeyValueEntry("QualifiedName", JaysonTypeInfo.GetTypeName(obj.ParameterType, JaysonTypeNameInfo.TypeNameWithAssembly),
                    typeof(string), context, isFirst);
            }
            finally
            {
                context.ObjectDepth--;
                WriteIndent(context);
                builder.Append('}');
            }
        }

        private static void WriteConstructorInfo(ConstructorInfo obj, JaysonSerializationContext context)
        {
            var settings = context.Settings;

            context.ObjectDepth++;
            var builder = context.Builder;
            try
            {
                var isFirst = !WriteObjectType((object)obj, context);
                if (isFirst)
                {
                    builder.Append('{');
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                bool referenced;
                isFirst = WriteReferenceOrId(obj, context, isFirst, out referenced);
                if (referenced)
                {
                    return;
                }

                isFirst = WriteKeyValueEntry("QualifiedName", JaysonTypeInfo.GetTypeName(obj.DeclaringType, JaysonTypeNameInfo.TypeNameWithAssembly),
                    typeof(string), context, isFirst);

                if (obj.ReflectedType != null && obj.ReflectedType != obj.DeclaringType)
                {
                    isFirst = WriteKeyValueEntry("ReflectedType", JaysonTypeInfo.GetTypeName(obj.ReflectedType, JaysonTypeNameInfo.TypeNameWithAssembly),
                        typeof(string), context, isFirst);
                }

                var parameters = obj.GetParameters();
                if (parameters != null && parameters.Length > 0)
                {
                    Type[] parameterTypes = new Type[parameters.Length];
                    for (var i = parameters.Length - 1; i > -1; i--)
                    {
                        parameterTypes[i] = parameters[i].ParameterType;
                    }

                    WriteKeyValueEntry("Params", parameterTypes, typeof(object), context, isFirst);
                }
            }
            finally
            {
                context.ObjectDepth--;
                WriteIndent(context);
                builder.Append('}');
            }
        }

        private static void WriteMethodInfo(MethodInfo obj, JaysonSerializationContext context)
        {
            var settings = context.Settings;

            context.ObjectDepth++;
            var builder = context.Builder;
            try
            {
                var isFirst = !WriteObjectType((object)obj, context);
                if (isFirst)
                {
                    builder.Append('{');
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                bool referenced;
                isFirst = WriteReferenceOrId(obj, context, isFirst, out referenced);
                if (referenced)
                {
                    return;
                }

                isFirst = WriteKeyValueEntry("QualifiedName", JaysonTypeInfo.GetTypeName(obj.DeclaringType, JaysonTypeNameInfo.TypeNameWithAssembly),
                    typeof(string), context, isFirst);

                if (obj.ReflectedType != null && obj.ReflectedType != obj.DeclaringType)
                {
                    isFirst = WriteKeyValueEntry("ReflectedType", JaysonTypeInfo.GetTypeName(obj.ReflectedType, JaysonTypeNameInfo.TypeNameWithAssembly),
                        typeof(string), context, isFirst);
                }

                isFirst = WriteKeyValueEntry("MemberName", obj.Name, typeof(string), context, isFirst);

                var parameters = obj.GetParameters();
                if (parameters != null && parameters.Length > 0)
                {
                    Type[] parameterTypes = new Type[parameters.Length];
                    for (var i = parameters.Length - 1; i > -1; i--)
                    {
                        parameterTypes[i] = parameters[i].ParameterType;
                    }

                    WriteKeyValueEntry("Params", parameterTypes, typeof(object), context, isFirst);
                }
            }
            finally
            {
                context.ObjectDepth--;
                WriteIndent(context);
                builder.Append('}');
            }
        }

        private static void WriteDictionaryObject(IDictionary obj, Type keyType, Type valueType, JaysonSerializationContext context)
        {
            var settings = context.Settings;
            var formatting = settings.Formatting != JaysonFormatting.None;

            context.ObjectDepth++;
            var builder = context.Builder;
            try
            {
                var isFirst = !WriteObjectType(obj, context);
                if (isFirst)
                {
                    builder.Append('{');
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                bool referenced;
                isFirst = WriteReferenceOrId(obj, context, isFirst, out referenced);
                if (referenced)
                {
                    return;
                }

                if (obj.Count > 0)
                {
                    if (!isFirst)
                    {
                        builder.Append(',');
                    }
                    if (formatting)
                    {
                        WriteIndent(context);
                    }

                    var builderLen = 0;

                    context.ObjectDepth++;
                    try
                    {
                        if (formatting)
                        {
                            builder.Append("\"$kv\": [");
                        }
                        else
                        {
                            builder.Append("\"$kv\":[");
                        }

                        builderLen = builder.Length;

                        context.ObjectDepth++;
                        try
                        {
                            var isFirstItem = true;

                            if (context.Settings.OrderNames)
                            {
                                foreach (var kvp in GetDictionaryEntries(obj, context).OrderBy(kvp => kvp.Key))
                                {
                                    if (!isFirstItem)
                                    {
                                        builder.Append(',');
                                    }
                                    if (formatting)
                                    {
                                        WriteIndent(context, context.ObjectDepth - 1);
                                    }

                                    try
                                    {
                                        builder.Append('{');

                                        WriteKeyValueEntry(key: "$k", value: kvp.Key, expectedValueType: keyType, context: context,
                                            isFirst: true, forceNullValues: true);
                                        WriteKeyValueEntry(key: "$v", value: kvp.Value, expectedValueType: valueType, context: context,
                                            isFirst: false, forceNullValues: true);
                                    }
                                    finally
                                    {
                                        isFirstItem = false;
                                        if (formatting)
                                        {
                                            WriteIndent(context, context.ObjectDepth - 1);
                                        }
                                        builder.Append('}');
                                    }
                                }
                            }
                            else
                            {
                                string key;
                                object value;
                                object keyObj;

                                var filter = context.Filter;
                                var canFilter = (filter != null);

                                foreach (DictionaryEntry dEntry in obj)
                                {
                                    keyObj = dEntry.Key;
                                    value = dEntry.Value;

                                    if ((value != null) && canFilter)
                                    {
                                        key = (keyObj is string) ? (string)keyObj : keyObj.ToString();
                                        value = filter(key, value);
                                    }

                                    if (!isFirstItem)
                                    {
                                        builder.Append(',');
                                    }
                                    if (formatting)
                                    {
                                        WriteIndent(context, context.ObjectDepth - 1);
                                    }

                                    try
                                    {
                                        builder.Append('{');

                                        WriteKeyValueEntry(key: "$k", value: keyObj, expectedValueType: keyType, context: context,
                                            isFirst: true, forceNullValues: true);
                                        WriteKeyValueEntry(key: "$v", value: value, expectedValueType: valueType, context: context,
                                            isFirst: false, forceNullValues: true);
                                    }
                                    finally
                                    {
                                        isFirstItem = false;
                                        if (formatting)
                                        {
                                            WriteIndent(context, context.ObjectDepth - 1);
                                        }
                                        builder.Append('}');
                                    }
                                }
                            }
                        }
                        finally
                        {
                            context.ObjectDepth--;
                        }
                    }
                    finally
                    {
                        context.ObjectDepth--;
                        if (formatting && (builder.Length > builderLen))
                        {
                            WriteIndent(context);
                        }
                        builder.Append(']');
                    }
                }
            }
            finally
            {
                context.ObjectDepth--;
                if (formatting)
                {
                    WriteIndent(context);
                }
                builder.Append('}');
            }
        }

        private static void WriteDictionaryStringKey(IDictionary obj, JaysonSerializationContext context)
        {
            var settings = context.Settings;

            context.ObjectDepth++;
            var builder = context.Builder;
            try
            {
                var isFirst = !WriteObjectType(obj, context);
                if (isFirst)
                {
                    builder.Append('{');
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                bool referenced;
                isFirst = WriteReferenceOrId(obj, context, isFirst, out referenced);
                if (referenced)
                {
                    return;
                }

                if (obj.Count > 0)
                {
                    string key;
                    object keyObj;

                    if (settings.OrderNames)
                    {
                        foreach (var kvp in GetDictionaryEntries(obj, context).OrderBy(kvp => kvp.Key))
                        {
                            keyObj = kvp.Key;
                            key = (keyObj as string) ?? keyObj.ToString();

                            isFirst = WriteKeyValueEntry(key: key, value: kvp.Value, expectedValueType: null, context: context,
                                isFirst: isFirst, forceNullValues: true);
                        }
                    }
                    else
                    {
                        object value;

                        var filter = context.Filter;
                        var canFilter = (filter != null);

                        foreach (DictionaryEntry dEntry in obj)
                        {
                            keyObj = dEntry.Key;
                            value = dEntry.Value;

                            key = (keyObj as string) ?? keyObj.ToString();

                            if ((value != null) && canFilter)
                            {
                                value = filter(key, value);
                            }

                            isFirst = WriteKeyValueEntry(key: key, value: value, expectedValueType: null, context: context,
                                isFirst: isFirst, forceNullValues: true);
                        }
                    }
                }
            }
            finally
            {
                context.ObjectDepth--;
                WriteIndent(context);
                builder.Append('}');
            }
        }

        private static IEnumerable<JaysonKeyValue<string, object>> GetStringDictionaryEntries(StringDictionary source,
            JaysonSerializationContext context)
        {
            string key;
            object value;
            object keyObj;

            var settings = context.Settings;

            var filter = context.Filter;
            var canFilter = (filter != null);

            foreach (DictionaryEntry dEntry in source)
            {
                value = dEntry.Value;

                keyObj = dEntry.Key;
                key = (keyObj is string) ? (string)keyObj : keyObj.ToString();

                if ((value != null) && canFilter)
                {
                    value = filter(key, value);
                }

                if ((value != null) || !settings.IgnoreNullValues)
                {
                    yield return new JaysonKeyValue<string, object> { Key = key, Value = value };
                }
            }
        }

        private static void WriteStringDictionary(StringDictionary obj, JaysonSerializationContext context)
        {
            var settings = context.Settings;

            context.ObjectDepth++;
            var builder = context.Builder;
            try
            {
                var isFirst = !WriteObjectType(obj, context);
                if (isFirst)
                {
                    builder.Append('{');
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                bool referenced;
                isFirst = WriteReferenceOrId(obj, context, isFirst, out referenced);
                if (referenced)
                {
                    return;
                }

                if (obj.Count > 0)
                {
                    if (settings.OrderNames)
                    {
                        foreach (var kvp in GetStringDictionaryEntries(obj, context).OrderBy(kvp => kvp.Key))
                        {
                            isFirst = WriteKeyValueEntry(kvp.Key, kvp.Value, typeof(string), context, isFirst);
                        }
                    }
                    else
                    {
                        string key;
                        object value;
                        object keyObj;

                        var filter = context.Filter;
                        var canFilter = (filter != null);

                        foreach (DictionaryEntry dEntry in obj)
                        {
                            keyObj = dEntry.Key;
                            value = dEntry.Value;

                            key = (keyObj is string) ? (string)keyObj : keyObj.ToString();

                            if ((value != null) && canFilter)
                            {
                                value = filter(key, value);
                            }

                            isFirst = WriteKeyValueEntry(key, value, typeof(string), context, isFirst);
                        }
                    }
                }
            }
            finally
            {
                context.ObjectDepth--;
                WriteIndent(context);
                builder.Append('}');
            }
        }

        private static void WriteNameValueCollection(NameValueCollection obj, JaysonSerializationContext context)
        {
            var settings = context.Settings;

            context.ObjectDepth++;
            var builder = context.Builder;
            try
            {
                var isFirst = !WriteObjectType(obj, context);
                if (isFirst)
                {
                    builder.Append('{');
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                bool referenced;
                isFirst = WriteReferenceOrId(obj, context, isFirst, out referenced);
                if (referenced)
                {
                    return;
                }

                if (obj.Count > 0)
                {
                    var keys = obj.AllKeys;
                    if (settings.OrderNames)
                    {
                        keys = keys.OrderBy(_key => _key).ToArray();
                    }

                    string key;
                    object value;

                    var filter = context.Filter;
                    var canFilter = (filter != null);

                    var len = keys.Length;
                    for (var i = 0; i < len; i++)
                    {
                        key = keys[i];
                        value = obj[key];

                        if ((value != null) && canFilter)
                        {
                            value = filter(key, value);
                        }

                        isFirst = WriteKeyValueEntry(key, value, typeof(string), context, isFirst);
                    }
                }
            }
            finally
            {
                context.ObjectDepth--;
                WriteIndent(context);
                builder.Append('}');
            }
        }

#if !(NET3500 || NET3000 || NET2000)
        private static void WriteDynamicObject(DynamicObject obj, JaysonSerializationContext context)
        {
            var settings = context.Settings;

            context.ObjectDepth++;
            var builder = context.Builder;
            try
            {
                var objType = obj.GetType();

                var isFirst = !WriteObjectType(objType, context);
                if (isFirst)
                {
                    builder.Append('{');
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                bool referenced;
                isFirst = WriteReferenceOrId(obj, context, isFirst, out referenced);
                if (referenced)
                {
                    return;
                }

                string key;
                object value;
                string aliasKey;

                var filter = context.Filter;
                var canFilter = (filter != null);

                var typeOverride = settings.GetTypeOverride(objType);

                var members = JaysonFastMemberCache.GetMembers(objType);
                if ((members != null) && (members.Length > 0))
                {
                    foreach (var member in members)
                    {
                        if ((!settings.IgnoreReadOnlyMembers || member.CanWrite) &&
                            !(member.AnonymousField || (settings.IgnoreBackingFields && member.BackingField) ||
                             (settings.IgnoreNonPublicFields && !member.IsPublic && (member.Type == JaysonFastMemberType.Field)) ||
                             (settings.IgnoreNonPublicProperties && !member.IsPublic && (member.Type == JaysonFastMemberType.Property))))
                        {
                            key = member.Name;

                            if (!settings.IsMemberIgnored(objType, key) && 
                                (typeOverride == null || !typeOverride.IsMemberIgnored(key)))
                            {
                                value = member.Get(obj);

                                if ((value != null) && canFilter)
                                {
                                    value = filter(key, value);
                                }

                                if (typeOverride != null)
                                {
                                    aliasKey = typeOverride.GetMemberAlias(key);
                                    if (!String.IsNullOrEmpty(aliasKey))
                                    {
                                        key = aliasKey;
                                    }
                                }

                                if (settings.IgnoreEmptyCollections)
                                {
                                    var collection = value as ICollection;
                                    if ((collection != null) && (collection.Count == 0))
                                    {
                                        continue;
                                    }
                                }

                                if (!settings.IsMemberIgnored(objType, key, value))
                                {
                                    context.CurrentType = member.MemberType;
                                    isFirst = WriteProperty(key, value, null, context, isFirst);
                                }
                            }
                        }
                    }
                }

                var memberNames = obj.GetDynamicMemberNames();
                if (context.Settings.OrderNames)
                {
                    memberNames = memberNames.OrderBy(name => name);
                }

                var dObj = new JaysonDynamicWrapper((IDynamicMetaObjectProvider)obj);

                foreach (var dKey in memberNames)
                {
                    key = dKey;
                    if (!settings.IsMemberIgnored(objType, key) && 
                        (typeOverride == null || !typeOverride.IsMemberIgnored(key)))
                    {
                        value = dObj[key];

                        if ((value != null) && canFilter)
                        {
                            value = filter(key, value);
                        }

                        if (typeOverride != null)
                        {
                            aliasKey = typeOverride.GetMemberAlias(key);
                            if (!String.IsNullOrEmpty(aliasKey))
                            {
                                key = aliasKey;
                            }
                        }

                        if (settings.IgnoreEmptyCollections)
                        {
                            var collection = value as ICollection;
                            if ((collection != null) && (collection.Count == 0))
                            {
                                continue;
                            }
                        }

                        if (!settings.IsMemberIgnored(objType, key, value))
                        {
                            isFirst = WriteProperty(key, value, null, context, isFirst);
                        }
                    }
                }
            }
            finally
            {
                context.ObjectDepth--;
                WriteIndent(context);
                builder.Append('}');
            }
        }
#endif

        private static void WriteClassOrStruct(object obj, Type objType, JaysonSerializationContext context)
        {
            var settings = context.Settings;

            context.ObjectDepth++;
            var builder = context.Builder;
            try
            {
                var isFirst = !WriteObjectType(objType, context);
                if (isFirst)
                {
                    builder.Append('{');
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                bool referenced;
                isFirst = WriteReferenceOrId(obj, context, isFirst, out referenced);
                if (referenced)
                {
                    return;
                }

                var members = JaysonFastMemberCache.GetMembers(objType);
                if ((members != null) && (members.Length > 0))
                {
                    string key;
                    object value;
                    string aliasKey;

                    var filter = context.Filter;
                    var canFilter = (filter != null);

                    object defaultValue;

                    var typeOverride = settings.GetTypeOverride(objType);

                    foreach (var member in members)
                    {
                        if ((!settings.IgnoreReadOnlyMembers || member.CanWrite) &&
                            !(member.AnonymousField || (settings.IgnoreBackingFields && member.BackingField) ||
                             (settings.IgnoreNonPublicFields && !member.IsPublic && (member.Type == JaysonFastMemberType.Field)) ||
                             (settings.IgnoreNonPublicProperties && !member.IsPublic && (member.Type == JaysonFastMemberType.Property))))
                        {
                            key = member.Name;

                            if (!settings.IsMemberIgnored(objType, key) && 
                                (typeOverride == null || !typeOverride.IsMemberIgnored(key)))
                            {
                                value = member.Get(obj);

                                if (settings.IgnoreDefaultValues)
                                {
                                    defaultValue = null;
                                    if ((typeOverride == null) || !typeOverride.TryGetDefaultValue(key, out defaultValue))
                                    {
                                        defaultValue = member.DefaultValue;
                                    }

                                    if (((defaultValue == null) && (value == null)) ||
                                        ((defaultValue != null) && (defaultValue.Equals(value) || 
                                         ((defaultValue is IComparable) && ((IComparable)defaultValue).CompareTo(value) == 0))))
                                        continue;
                                }

                                if ((value != null) && canFilter)
                                {
                                    value = filter(key, value);
                                }

                                if (typeOverride != null)
                                {
                                    aliasKey = typeOverride.GetMemberAlias(key);
                                    if (!String.IsNullOrEmpty(aliasKey))
                                    {
                                        key = aliasKey;
                                    }
                                }

                                if (settings.IgnoreEmptyCollections)
                                {
                                    var collection = value as ICollection;
                                    if ((collection != null) && (collection.Count == 0))
                                    {
                                        continue;
                                    }
                                }

                                if (!settings.IsMemberIgnored(objType, key, value))
                                {
                                    context.CurrentType = member.MemberType;
                                    isFirst = WriteProperty(key, value, null, context, isFirst);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                context.ObjectDepth--;
                WriteIndent(context);
                builder.Append('}');
            }
        }

        private static void WriteISerializable(ISerializable obj, Type objType, JaysonSerializationContext context)
        {
            var settings = context.Settings;
            if (settings.UseKVModelForISerializable)
            {
                WriteISerializableKVMode(obj, objType, context);
                return;
            }

            context.ObjectDepth++;
            var builder = context.Builder;
            try
            {
                var isFirst = !WriteObjectType(objType, context);
                if (isFirst)
                {
                    builder.Append('{');
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                bool referenced;
                isFirst = WriteReferenceOrId(obj, context, isFirst, out referenced);
                if (referenced)
                {
                    return;
                }

                var info = new SerializationInfo(objType, JaysonCommon.FormatterConverter);
                obj.GetObjectData(info, context.StreamingContext);

                if (info.MemberCount > 0)
                {
                    string key;
                    object value;
                    string aliasKey;

                    var filter = context.Filter;
                    var canFilter = (filter != null);

                    var typeOverride = settings.GetTypeOverride(objType);

                    foreach (SerializationEntry se in info)
                    {
                        key = se.Name;
                        if (!settings.IsMemberIgnored(objType, key) && 
                            (typeOverride == null || !typeOverride.IsMemberIgnored(key)))
                        {
                            value = se.Value;

                            if ((value != null) && canFilter)
                            {
                                value = filter(key, value);
                            }

                            if (typeOverride != null)
                            {
                                aliasKey = typeOverride.GetMemberAlias(key);
                                if (!String.IsNullOrEmpty(aliasKey))
                                {
                                    key = aliasKey;
                                }
                            }

                            if (!settings.IsMemberIgnored(objType, key, value))
                            {
                                context.CurrentType = null;
                                isFirst = WriteProperty(key, value, null, context, isFirst, true);
                            }
                        }
                    }
                }
            }
            finally
            {
                context.ObjectDepth--;
                WriteIndent(context);
                builder.Append('}');
            }
        }

        private static void WriteISerializableKVMode(ISerializable obj, Type objType, JaysonSerializationContext context)
        {
            var settings = context.Settings;

            context.ObjectDepth++;
            var builder = context.Builder;
            try
            {
                var isFirst = !WriteObjectType(objType, context);
                if (isFirst)
                {
                    builder.Append('{');
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                bool referenced;
                isFirst = WriteReferenceOrId(obj, context, isFirst, out referenced);
                if (referenced)
                {
                    return;
                }

                var info = new SerializationInfo(objType, JaysonCommon.FormatterConverter);
                obj.GetObjectData(info, context.StreamingContext);

                if (info.MemberCount > 0)
                {
                    if (!isFirst)
                    {
                        builder.Append(',');
                    }

                    var formatting = settings.Formatting != JaysonFormatting.None;
                    if (formatting)
                    {
                        WriteIndent(context);

                        context.ObjectDepth += 2;
                        if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                        {
                            return;
                        }

                        builder.Append("\"$ctx\": [");
                    }
                    else
                    {
                        builder.Append("\"$ctx\":[");
                    }

                    var builderLen = builder.Length;
                    try
                    {
                        string key;
                        object value;
                        string aliasKey;

                        var filter = context.Filter;
                        var canFilter = (filter != null);

                        var typeOverride = settings.GetTypeOverride(objType);

                        var isFirstItem = true;

                        foreach (SerializationEntry se in info)
                        {
                            key = se.Name;
                            if (!settings.IsMemberIgnored(objType, key) && 
                                (typeOverride == null || !typeOverride.IsMemberIgnored(key)))
                            {
                                value = se.Value;

                                if ((value != null) && canFilter)
                                {
                                    value = filter(key, value);
                                }

                                if (typeOverride != null)
                                {
                                    aliasKey = typeOverride.GetMemberAlias(key);
                                    if (!String.IsNullOrEmpty(aliasKey))
                                    {
                                        key = aliasKey;
                                    }
                                }

                                if (!settings.IsMemberIgnored(objType, key, value))
                                {
                                    context.CurrentType = null;

                                    if (!isFirstItem)
                                    {
                                        builder.Append(',');
                                    }
                                    if (formatting)
                                    {
                                        WriteIndent(context, context.ObjectDepth - 1);
                                    }

                                    try
                                    {
                                        builder.Append('{');

                                        WriteKeyValueEntry(key: "$k", value: key, expectedValueType: typeof(string), context: context,
                                            isFirst: true, forceNullValues: true);
                                        WriteKeyValueEntry(key: "$v", value: value, expectedValueType: null, context: context,
                                            isFirst: false, forceNullValues: true);
                                    }
                                    finally
                                    {
                                        isFirstItem = false;
                                        if (formatting)
                                        {
                                            WriteIndent(context, context.ObjectDepth - 1);
                                        }
                                        builder.Append('}');
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        context.ObjectDepth -= 2;
                        if (formatting && (builder.Length > builderLen))
                        {
                            WriteIndent(context);
                        }
                        builder.Append(']');
                    }
                }
            }
            finally
            {
                context.ObjectDepth--;
                WriteIndent(context);
                builder.Append('}');
            }
        }

        private static void WriteGenericStringDictionary(IDictionary<string, object> obj, JaysonSerializationContext context)
        {
            var settings = context.Settings;

            context.ObjectDepth++;
            var builder = context.Builder;
            try
            {
                var isFirst = !WriteObjectType(obj, context);
                if (isFirst)
                {
                    builder.Append('{');
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                bool referenced;
                isFirst = WriteReferenceOrId(obj, context, isFirst, out referenced);
                if (referenced)
                {
                    return;
                }

                if (obj.Count > 0)
                {
                    string key;
                    object value;
#if !(NET3500 || NET3000 || NET2000)
                    var isExpando = obj is ExpandoObject;
#else
					bool isExpando = false;
#endif

                    var filter = context.Filter;
                    var canFilter = (filter != null);

                    IEnumerable<KeyValuePair<string, object>> eDict = obj;
                    if (settings.OrderNames)
                    {
                        eDict = eDict.OrderBy(kvp => kvp.Key);

                        foreach (var kvp in obj)
                        {
                            key = kvp.Key;
                            value = kvp.Value;

                            if ((value != null) && canFilter)
                            {
                                value = filter(key, value);
                            }

                            isFirst = WriteKeyValueEntry(key: key, value: value, expectedValueType: null, context: context,
                                isFirst: isFirst, forceNullValues: false, ignoreEscape: isExpando);
                        }

                        return;
                    }

                    foreach (var kvp in obj)
                    {
                        key = kvp.Key;
                        value = kvp.Value;

                        if ((value != null) && canFilter)
                        {
                            value = filter(key, value);
                        }

                        isFirst = WriteKeyValueEntry(key: key, value: value, expectedValueType: null, context: context,
                            isFirst: isFirst, forceNullValues: false, ignoreEscape: isExpando);
                    }
                }
            }
            finally
            {
                context.ObjectDepth--;
                if (context.Settings.Formatting != JaysonFormatting.None)
                {
                    WriteIndent(context);
                }
                builder.Append('}');
            }
        }

        private static void WriteByteArray(byte[] obj, JaysonSerializationContext context)
        {
            context.ObjectDepth++;
            var typeWritten = WriteByteArrayType(context);

            var formatting = context.Settings.Formatting != JaysonFormatting.None;
            var builder = context.Builder;
            try
            {
                if (typeWritten)
                {
                    context.ObjectDepth++;
                }

                if (!ValidObjectDepth(context))
                {
                    builder.Append(JaysonConstants.EmptyJsonString);
                    return;
                }

                builder.Append('"');
                builder.Append(Convert.ToBase64String(obj, 0, obj.Length, Base64FormattingOptions.None));
                builder.Append('"');
            }
            finally
            {
                context.ObjectDepth--;
                if (typeWritten)
                {
                    context.ObjectDepth--;
                    if (formatting)
                    {
                        WriteIndent(context);
                    }
                    builder.Append('}');
                }
            }
        }

        static void WriteDBNullArray(JaysonSerializationContext context, int length)
        {
            context.ObjectDepth++;
            var objectStarted = WriteListType(typeof(DBNull[]), context);

            var formatting = context.Settings.Formatting != JaysonFormatting.None;
            var builder = context.Builder;
            try
            {
                if (!objectStarted)
                {
                    builder.Append('[');
                }
                else
                {
                    context.ObjectDepth++;
                }

                if (context.Settings.IgnoreNullListItems)
                {
                    length = 0;
                    return;
                }

                if (!ValidObjectDepth(context))
                {
                    return;
                }

                var objectDepth = context.ObjectDepth;

                for (var i = 0; i < length; i++)
                {
                    if (i == 0)
                    {
                        if (formatting)
                        {
                            builder.Append(JaysonConstants.Indentation[objectDepth]);
                        }
                        builder.Append(JaysonConstants.Null);
                    }
                    else if (formatting)
                    {
                        builder.Append(',');
                        builder.Append(JaysonConstants.Indentation[objectDepth]);
                        builder.Append(JaysonConstants.Null);
                    }
                    else
                    {
                        builder.Append(",null");
                    }
                }
            }
            finally
            {
                context.ObjectDepth--;
                if (!formatting)
                {
                    builder.Append(']');

                    if (objectStarted)
                    {
                        context.ObjectDepth--;
                        builder.Append('}');
                    }
                }
                else
                {
                    if (length > 0)
                    {
                        WriteIndent(context);
                    }
                    builder.Append(']');

                    if (objectStarted)
                    {
                        context.ObjectDepth--;
                        WriteIndent(context);
                        builder.Append('}');
                    }
                }
            }
        }

        private static void WritePrimitiveArray(Array obj, Type arrayType, JaysonSerializationContext context)
        {
            var settings = context.Settings;

            context.ObjectDepth++;
            var objectStarted = WriteListTypeAndId(obj, context);

            var builderLen = 0;
            var builder = context.Builder;
            var formatting = settings.Formatting != JaysonFormatting.None;
            try
            {
                if (!objectStarted)
                {
                    builder.Append('[');
                }
                else
                {
                    context.ObjectDepth++;
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                builderLen = builder.Length;

                object item;

                var isFirst = true;
                var objLen = obj.Length;

                var objectDepth = context.ObjectDepth;
                var formatter = context.Formatter;

                for (var i = 0; i < objLen; i++)
                {
                    item = obj.GetValue(i);

                    if (JaysonCommon.IsNull(item, settings.FloatNanStrategy, settings.FloatInfinityStrategy))
                    {
                        if (!settings.IgnoreNullValues)
                        {
                            if (isFirst)
                            {
                                isFirst = false;
                                if (formatting)
                                {
                                    builder.Append(JaysonConstants.Indentation[objectDepth]);
                                }
                                builder.Append(JaysonConstants.Null);
                            }
                            else
                            {
                                if (formatting)
                                {
                                    builder.Append(',');
                                    builder.Append(JaysonConstants.Indentation[objectDepth]);
                                    builder.Append(JaysonConstants.Null);
                                }
                                else
                                {
                                    builder.Append(",null");
                                }
                            }
                        }
                        continue;
                    }

                    if (!isFirst)
                    {
                        builder.Append(',');
                    }
                    if (formatting)
                    {
                        builder.Append(JaysonConstants.Indentation[objectDepth]);
                    }

                    isFirst = false;
                    formatter.Format(item, arrayType, builder);
                }
            }
            finally
            {
                context.ObjectDepth--;

                if (!formatting)
                {
                    builder.Append(']');

                    if (objectStarted)
                    {
                        context.ObjectDepth--;
                        builder.Append('}');
                    }
                }
                else
                {
                    if (builder.Length > builderLen)
                    {
                        WriteIndent(context);
                    }
                    builder.Append(']');

                    if (objectStarted)
                    {
                        context.ObjectDepth--;
                        WriteIndent(context);
                        builder.Append('}');
                    }
                }
            }
        }

        private static void WriteMultiDimensionalArray(Array obj, Type objType, bool isRoot,
            int[] rankLengths, int[] rankIndices, int currRank, JaysonSerializationContext context,
            bool isFirst)
        {
            var settings = context.Settings;

            var length = rankLengths[currRank];
            var formatting = settings.Formatting != JaysonFormatting.None;

            var builderLen = 0;

            context.ObjectDepth++;
            var builder = context.Builder;
            try
            {
                if (currRank > 0)
                {
                    builder.Append('[');
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                builderLen = builder.Length;

                var isFirstInner = true;
                for (var i = 0; i < length; i++)
                {
                    rankIndices[currRank] = i;

                    if (currRank == rankIndices.Length - 1)
                    {
                        isFirstInner = WriteEnumerableValue(obj.GetValue(rankIndices), context, isFirstInner);
                    }
                    else
                    {
                        if (!isFirstInner)
                        {
                            builder.Append(',');
                        }

                        if (formatting)
                        {
                            WriteIndent(context);
                        }

                        WriteMultiDimensionalArray(obj, objType, isRoot, rankLengths, rankIndices,
                            currRank + 1, context, isFirstInner);
                        isFirstInner = false;
                    }
                }
            }
            finally
            {
                context.ObjectDepth--;
                if (currRank > 0)
                {
                    if (formatting && (length > 0) && (builder.Length > builderLen))
                    {
                        WriteIndent(context);
                    }
                    builder.Append(']');
                }
            }
        }

        private static void WriteMultiDimensionalArray(Array obj, Type objType, JaysonSerializationContext context)
        {
            var settings = context.Settings;

            var isRoot = context.ObjectDepth == 0;

            context.ObjectDepth++;
            var objectStarted = WriteListTypeAndId(obj, context);

            var builderLen = 0;

            var isEmpty = true;
            var builder = context.Builder;
            try
            {
                if (!objectStarted)
                {
                    context.ObjectDepth--;
                    builder.Append('[');
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                builderLen = builder.Length;

                var rank = obj.Rank;
                var rankLengths = new int[rank];

                for (var i = 0; i < rank; i++)
                {
                    rankLengths[i] = obj.GetLength(i);
                    if (isEmpty)
                    {
                        isEmpty = rankLengths[i] == 0;
                    }
                }

                WriteMultiDimensionalArray(obj, objType, isRoot, rankLengths, new int[rank], 0, context, true);
            }
            finally
            {
                if (!objectStarted)
                {
                    if (settings.Formatting != JaysonFormatting.None &&
                        !isEmpty && (builder.Length > builderLen))
                    {
                        WriteIndent(context);
                    }
                    builder.Append(']');
                }
                else
                {
                    if (settings.Formatting == JaysonFormatting.None)
                    {
                        builder.Append(']');
                        context.ObjectDepth--;
                    }
                    else
                    {
                        if (!isEmpty && (builder.Length > builderLen))
                        {
                            WriteIndent(context);
                        }
                        builder.Append(']');

                        context.ObjectDepth--;
                        WriteIndent(context);
                    }
                    builder.Append('}');
                }
            }
        }

        private static void WriteEmptyArray(Array obj, Type objType, JaysonSerializationContext context)
        {
            context.ObjectDepth++;
            var objectStarted = WriteListTypeAndId(obj, context);

            var builder = context.Builder;
            try
            {
                if (!objectStarted)
                {
                    builder.Append('[');
                }

                if (!ValidObjectDepth(context))
                {
                    return;
                }
            }
            finally
            {
                context.ObjectDepth--;

                builder.Append(']');
                if (objectStarted)
                {
                    builder.Append('}');
                }
            }
        }

        private static void WriteArray(Array obj, Type objType, JaysonSerializationContext context)
        {
            if (obj.Rank > 1)
            {
                WriteMultiDimensionalArray(obj, objType, context);
                return;
            }

            var length = obj.Length;
            if (length == 0)
            {
                WriteEmptyArray(obj, objType, context);
                return;
            }

            var arrayType = JaysonTypeInfo.GetElementType(objType);
            if (arrayType == typeof(byte))
            {
                WriteByteArray((byte[])obj, context);
                return;
            }

            var info = JaysonTypeInfo.GetTypeInfo(arrayType);
            if (info.JPrimitive || info.Enum)
            {
                WritePrimitiveArray(obj, arrayType, context);
                return;
            }

            if (arrayType == typeof(DBNull))
            {
                WriteDBNullArray(context, length);
                return;
            }

            WriteIList(obj, objType, context);
        }

        private static void WriteIList(IList obj, Type objType, JaysonSerializationContext context)
        {
            var settings = context.Settings;

            context.ObjectDepth++;
            var objectStarted = WriteListTypeAndId(obj, context);

            var builderLen = 0;

            var builder = context.Builder;
            try
            {
                if (!objectStarted)
                {
                    builder.Append('[');
                }
                else
                {
                    context.ObjectDepth++;
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                builderLen = builder.Length;

                var count = obj.Count;
                if (count > 0)
                {
                    var isFirst = true;
                    for (var i = 0; i < count; i++)
                    {
                        isFirst = WriteEnumerableValue(obj[i], context, isFirst);
                    }
                }
            }
            finally
            {
                context.ObjectDepth--;

                if (settings.Formatting == JaysonFormatting.None)
                {
                    builder.Append(']');

                    if (objectStarted)
                    {
                        context.ObjectDepth--;
                        builder.Append('}');
                    }
                }
                else
                {
                    if (builder.Length > builderLen)
                    {
                        WriteIndent(context);
                    }
                    builder.Append(']');

                    if (objectStarted)
                    {
                        context.ObjectDepth--;
                        WriteIndent(context);
                        builder.Append('}');
                    }
                }
            }
        }

        private static IEnumerable<JaysonKeyValue<string, object>> GetEnumDictionaryEntries(IEnumerable source,
            JaysonSerializationContext context)
        {
            string key;
            object value;
            object keyObj;

            var settings = context.Settings;

            var filter = context.Filter;
            var canFilter = (filter != null);

            foreach (DictionaryEntry dEntry in source)
            {
                value = dEntry.Value;

                keyObj = dEntry.Key;
                key = (keyObj is string) ? (string)keyObj : keyObj.ToString();

                if ((value != null) && canFilter)
                {
                    value = filter(key, value);
                }

                if ((value != null) || !settings.IgnoreNullValues)
                {
                    yield return new JaysonKeyValue<string, object> { Key = key, Value = value };
                }
            }
        }

        private static void WriteCountedEnumerable(IEnumerable obj, Type objType, JaysonSerializationContext context, bool isEmpty)
        {
            var settings = context.Settings;

            context.ObjectDepth++;
            var objectStarted = WriteListTypeAndId(obj, context);

            var builderLen = 0;

            var builder = context.Builder;
            try
            {
                if (!objectStarted)
                {
                    builder.Append('[');
                }
                else
                {
                    context.ObjectDepth++;
                }

                if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                {
                    return;
                }

                builderLen = builder.Length;

                if (!isEmpty)
                {
                    var isFirst = true;

                    Type entryType;
                    JaysonDictionaryType dType = JaysonCommon.GetDictionaryType(obj, out entryType);

                    IEnumerator enumerator = obj.GetEnumerator();
                    try
                    {
                        if (dType == JaysonDictionaryType.IDictionary)
                        {
                            if (settings.OrderNames)
                            {
                                foreach (var kvp in GetEnumDictionaryEntries(obj, context).OrderBy(kvp => kvp.Key))
                                {
                                    isFirst = WriteKeyValueEntry(kvp.Key, kvp.Value, null, context, isFirst);
                                }
                            }
                            else
                            {
                                string key;
                                object value;
                                object keyObj;
                                DictionaryEntry dEntry;

                                var filter = context.Filter;
                                var canFilter = (filter != null);

                                while (enumerator.MoveNext())
                                {
                                    dEntry = (DictionaryEntry)enumerator.Current;

                                    keyObj = dEntry.Key;
                                    value = dEntry.Value;

                                    key = (keyObj is string) ? (string)keyObj : keyObj.ToString();

                                    if ((value != null) && canFilter)
                                    {
                                        value = filter(key, value);
                                    }

                                    isFirst = WriteKeyValueEntry(key, value, null, context, isFirst);
                                }
                            }
                        }
                        else if (dType == JaysonDictionaryType.IGenericDictionary)
                        {
                            var cache = JaysonFastMemberCache.GetCache(entryType);
                            if (cache != null)
                            {
                                var keyFm = cache.GetAnyMember("Key");
                                if (keyFm != null)
                                {
                                    var valueFm = cache.GetAnyMember("Value");
                                    if (valueFm != null)
                                    {
                                        string key;
                                        object value;
                                        object keyObj;

                                        var filter = context.Filter;
                                        var canFilter = (filter != null);

                                        while (enumerator.MoveNext())
                                        {
                                            keyObj = keyFm.Get(enumerator.Current);
                                            if (keyObj != null)
                                            {
                                                key = (keyObj is string) ? (string)keyObj : keyObj.ToString();
                                                value = valueFm.Get(enumerator.Current);

                                                if ((value != null) && canFilter)
                                                {
                                                    value = filter(key, value);
                                                }

                                                if (settings.IgnoreEmptyCollections)
                                                {
                                                    var collection = value as ICollection;
                                                    if ((collection != null) && (collection.Count == 0))
                                                    {
                                                        continue;
                                                    }
                                                }

                                                context.CurrentType = valueFm.MemberType;
                                                isFirst = WriteKeyValueEntry(key, value, null, context, isFirst);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            while (enumerator.MoveNext())
                            {
                                isFirst = WriteEnumerableValue(enumerator.Current, context, isFirst);
                            }
                        }
                    }
                    finally
                    {
                        if (enumerator is IDisposable)
                        {
                            ((IDisposable)enumerator).Dispose();
                        }
                    }
                }
            }
            finally
            {
                context.ObjectDepth--;

                if (settings.Formatting == JaysonFormatting.None)
                {
                    builder.Append(']');

                    if (objectStarted)
                    {
                        context.ObjectDepth--;
                        builder.Append('}');
                    }
                }
                else
                {
                    if (builder.Length > builderLen)
                    {
                        WriteIndent(context);
                    }
                    builder.Append(']');

                    if (objectStarted)
                    {
                        context.ObjectDepth--;
                        WriteIndent(context);
                        builder.Append('}');
                    }
                }
            }
        }

        private static void WriteEnumerable(IEnumerable obj, Type objType, JaysonSerializationContext context)
        {
            if (obj is Array)
            {
                WriteArray((Array)obj, objType, context);
            }
            else if (obj is IList)
            {
                WriteIList((IList)obj, objType, context);
            }
            else if (obj is ICollection)
            {
                WriteCountedEnumerable(obj, objType, context, ((ICollection)obj).Count == 0);
            }
            else
            {
                WriteCountedEnumerable(obj, objType, context, false);
            }
        }

        private static bool CanWriteJsonObject(object obj, Type objType, JaysonSerializationContext context)
        {
            if (obj == null)
            {
                return !context.Settings.IgnoreNullValues;
            }

            if (JaysonTypeInfo.IsJPrimitive(objType ?? obj.GetType()))
            {
                return true;
            }

            if (obj == DBNull.Value)
            {
                return !context.Settings.IgnoreNullValues;
            }

            if (obj is double)
            {
                var d = (double)obj;
                return !((double.IsNaN(d) && context.Settings.FloatNanStrategy == JaysonFloatSerStrategy.ToNull) ||
                         (double.IsInfinity(d) && context.Settings.FloatInfinityStrategy == JaysonFloatSerStrategy.ToNull));
            }

            if (obj is float)
            {
                var f = (float)obj;
                return !((float.IsNaN(f) && context.Settings.FloatNanStrategy == JaysonFloatSerStrategy.ToNull) ||
                         (float.IsInfinity(f) && context.Settings.FloatInfinityStrategy == JaysonFloatSerStrategy.ToNull));
            }

            if (context.Stack.Contains(obj))
            {
                return context.Settings.UseObjectReferencing ||
                    !context.Settings.IgnoreNullValues;
            }

            return true;
        }

        private static void Format(object obj, Type objType, StringBuilder builder, JaysonSerializationSettings settings,
            JaysonFormatter formatter)
        {
            if (objType == typeof(double))
            {
                var d = (double)obj;
                if (double.IsNaN(d))
                {
                    switch (settings.FloatNanStrategy)
                    {
                        case JaysonFloatSerStrategy.ToNull:
                            builder.Append(JaysonConstants.Null);
                            return;
                        case JaysonFloatSerStrategy.ToString:
                            builder.Append('"');
                            builder.Append(d.ToString(JaysonConstants.InvariantCulture));
                            builder.Append('"');
                            return;
                        case JaysonFloatSerStrategy.Error:
                            throw new JaysonException(JaysonError.NaNError);
                    }
                }
                else if (double.IsInfinity(d))
                {
                    switch (settings.FloatInfinityStrategy)
                    {
                        case JaysonFloatSerStrategy.ToNull:
                            builder.Append(JaysonConstants.Null);
                            return;
                        case JaysonFloatSerStrategy.ToString:
                            builder.Append('"');
                            builder.Append(d.ToString(JaysonConstants.InvariantCulture));
                            builder.Append('"');
                            return;
                        case JaysonFloatSerStrategy.Error:
                            throw new JaysonException(JaysonError.NaNError);
                    }
                }
            }
            else if (objType == typeof(float))
            {
                var f = (float)obj;
                if (float.IsNaN(f))
                {
                    switch (settings.FloatNanStrategy)
                    {
                        case JaysonFloatSerStrategy.ToNull:
                            builder.Append(JaysonConstants.Null);
                            return;
                        case JaysonFloatSerStrategy.ToString:
                            builder.Append('"');
                            builder.Append(f.ToString(JaysonConstants.InvariantCulture));
                            builder.Append('"');
                            return;
                        case JaysonFloatSerStrategy.Error:
                            throw new JaysonException(JaysonError.NaNError);
                    }
                }
                else if (float.IsInfinity(f))
                {
                    switch (settings.FloatInfinityStrategy)
                    {
                        case JaysonFloatSerStrategy.ToNull:
                            builder.Append(JaysonConstants.Null);
                            return;
                        case JaysonFloatSerStrategy.ToString:
                            builder.Append('"');
                            builder.Append(f.ToString(JaysonConstants.InvariantCulture));
                            builder.Append('"');
                            return;
                        case JaysonFloatSerStrategy.Error:
                            throw new JaysonException(JaysonError.NaNError);
                    }
                }
            }

            formatter.Format(obj, objType, builder);
        }

        private static void WriteJsonObject(object obj, Type objType, Type expectedObjType, JaysonSerializationContext context)
        {
            if (obj == null)
            {
                if (!context.Settings.IgnoreNullValues)
                {
                    context.Builder.Append(JaysonConstants.Null);
                }
                return;
            }

            var info = JaysonTypeInfo.GetTypeInfo(objType);

            if (info.JPrimitive)
            {
                var jtc = info.JTypeCode;

                if (jtc == JaysonTypeCode.String || jtc == JaysonTypeCode.Bool)
                {
                    Format(obj, objType, context.Builder, context.Settings, context.Formatter);
                    return;
                }

                JaysonTypeNameSerialization jtns = context.Settings.TypeNames;

                if (jtns != JaysonTypeNameSerialization.None &&
                    (expectedObjType == null || objType != expectedObjType) &&
                    ((jtns == JaysonTypeNameSerialization.All && (JaysonTypeCode.JsonUnknown & jtc) == jtc) ||
                        (jtns == JaysonTypeNameSerialization.Auto && (JaysonTypeCode.AutoTyped & jtc) == jtc) ||
                        (jtns == JaysonTypeNameSerialization.AllButNoPrimitive && (JaysonTypeCode.Nullable & jtc) == jtc)))
                {
                    var builder = context.Builder;

                    var typeWritten = false;
                    context.ObjectDepth++;
                    try
                    {
                        typeWritten = WritePrimitiveTypeName(objType, context);
                        Format(obj, objType, builder, context.Settings, context.Formatter);
                    }
                    finally
                    {
                        context.ObjectDepth--;
                        if (typeWritten)
                        {
                            if (context.Settings.Formatting != JaysonFormatting.None)
                            {
                                WriteIndent(context);
                            }
                            builder.Append('}');
                        }
                    }
                    return;
                }

                Format(obj, objType, context.Builder, context.Settings, context.Formatter);
                return;
            }

            JaysonStackList<object> stack = null;
            if (info.Class)
            {
                if (obj == DBNull.Value)
                {
                    if (!context.Settings.IgnoreNullValues)
                    {
                        context.Builder.Append(JaysonConstants.Null);
                    }
                    return;
                }

                stack = context.Stack;
                if (stack.Contains(obj))
                {
                    if (context.Settings.UseObjectReferencing)
                    {
                        WriteObjectRefence(context.ReferenceMap.GetObjectId(obj), context);
                        return;
                    }
                    if (context.Settings.RaiseErrorOnCircularRef)
                    {
                        throw new JaysonException(JaysonError.CircularReferenceOn + objType.Name);
                    }
                    if (!context.Settings.IgnoreNullValues)
                    {
                        context.Builder.Append(JaysonConstants.Null);
                    }
                    return;
                }

                stack.Push(obj);

                if (context.Settings.UseObjectReferencing)
                {
                    int id;
                    if (context.ReferenceMap.TryGetObjectId(obj, out id))
                    {
                        WriteObjectRefence(id, context);
                        return;
                    }
                }
            }

            try
            {
                var settings = context.Settings;
                var serializationType = info.SerializationType;

                switch (serializationType)
                {
                    case JaysonTypeSerializationType.Enum:
                        {
                            var builder = context.Builder;

                            builder.Append('"');
                            builder.Append(context.Settings.UseEnumNames ? JaysonEnumCache.GetName((Enum)obj) : JaysonEnumCache.AsIntString((Enum)obj));
                            builder.Append('"');
                            return;
                        }
                    case JaysonTypeSerializationType.IDictionaryGeneric:
                        {
#if !(NET3500 || NET3000 || NET2000)
                            if (settings.IgnoreExpandoObjects && (objType == typeof(ExpandoObject)))
                            {
                                if (!settings.IgnoreNullValues)
                                {
                                    context.Builder.Append(JaysonConstants.Null);
                                }
                                else
                                {
                                    context.Builder.Append('{');
                                    context.Builder.Append('}');
                                }
                                return;
                            }
#endif

                            WriteGenericStringDictionary((IDictionary<string, object>)obj, context);
                            return;
                        }
                    case JaysonTypeSerializationType.IDictionary:
                        {
                            if (!settings.UseKVModelForJsonObjects)
                            {
                                WriteDictionaryStringKey((IDictionary)obj, context);
                            }
                            else
                            {
                                var genericArgs = info.GenericArguments;
                                if (genericArgs != null && genericArgs.Length > 0)
                                {
                                    if (genericArgs[0] == typeof(string))
                                    {
                                        WriteDictionaryStringKey((IDictionary)obj, context);
                                    }
                                    else
                                    {
                                        WriteDictionaryObject((IDictionary)obj, genericArgs[0], genericArgs[1], context);
                                    }
                                }
                                else
                                {
                                    WriteDictionaryObject((IDictionary)obj, null, null, context);
                                }
                            }
                            return;
                        }
#if !(NET3500 || NET3000 || NET2000)
                    case JaysonTypeSerializationType.DynamicObject:
                        {
                            if (!settings.IgnoreDynamicObjects)
                            {
                                WriteDynamicObject((DynamicObject)obj, context);
                            }
                            else if (!settings.IgnoreNullValues)
                            {
                                context.Builder.Append(JaysonConstants.Null);
                            }
                            else
                            {
                                context.Builder.Append('{');
                                context.Builder.Append('}');
                            }
                            return;
                        }
#endif
                    case JaysonTypeSerializationType.DataTable:
                        {
                            WriteDataTable((DataTable)obj, context);
                            return;
                        }
                    case JaysonTypeSerializationType.DataSet:
                        {
                            WriteDataSet((DataSet)obj, context);
                            return;
                        }
                    case JaysonTypeSerializationType.StringDictionary:
                        {
                            WriteStringDictionary((StringDictionary)obj, context);
                            return;
                        }
                    case JaysonTypeSerializationType.NameValueCollection:
                        {
                            WriteNameValueCollection((NameValueCollection)obj, context);
                            return;
                        }
                    case JaysonTypeSerializationType.IEnumerable:
                        {
                            WriteEnumerable((IEnumerable)obj, objType, context);
                            return;
                        }
                    case JaysonTypeSerializationType.Type:
                        {
                            WriteType((Type)obj, context);
                            return;
                        }
                    case JaysonTypeSerializationType.FieldInfo:
                    case JaysonTypeSerializationType.PropertyInfo:
                        {
                            WriteMemberInfo((MemberInfo)obj, context);
                            return;
                        }
                    case JaysonTypeSerializationType.MethodInfo:
                        {
                            WriteMethodInfo((MethodInfo)obj, context);
                            return;
                        }
                    case JaysonTypeSerializationType.ConstructorInfo:
                        {
                            WriteConstructorInfo((ConstructorInfo)obj, context);
                            return;
                        }
                    case JaysonTypeSerializationType.ParameterInfo:
                        {
                            WriteParameterInfo((ParameterInfo)obj, context);
                            return;
                        }
                    default:
                        {
                            if (settings.IgnoreAnonymousTypes && (serializationType == JaysonTypeSerializationType.Anonymous))
                            {
                                if (!settings.IgnoreNullValues)
                                {
                                    context.Builder.Append(JaysonConstants.Null);
                                }
                                else
                                {
                                    context.Builder.Append('{');
                                    context.Builder.Append('}');
                                }
                                return;
                            }

                            if (serializationType == JaysonTypeSerializationType.ISerializable)
                            {
                                WriteISerializable((ISerializable)obj, objType, context);
                                return;
                            }

                            WriteClassOrStruct(obj, objType, context);
                            break;
                        }
                }
            }
            finally
            {
                if (info.Class)
                {
                    stack.Pop();
                }
            }
        }

        private static void WriteGlobalTypes(JaysonSerializationContext context, bool isFirst)
        {
            if (context.GlobalTypes != null)
            {
                var formatting = context.Settings.Formatting != JaysonFormatting.None;

                context.ObjectDepth++;
                var builder = context.Builder;
                try
                {
                    if (formatting)
                    {
                        if (!isFirst)
                        {
                            builder.Append(',');
                        }
                        WriteIndent(context);
                        builder.Append("\"$types\": {");
                    }
                    else
                    {
                        if (isFirst)
                        {
                            builder.Append("\"$types\":{");
                        }
                        else
                        {
                            builder.Append(",\"$types\":{");
                        }
                    }

                    var globalTypes = context.GlobalTypes.GetList();
                    if (globalTypes.Count > 0)
                    {
                        context.ObjectDepth++;
                        try
                        {
                            JaysonKeyValue<string, Type> kvp;
                            var typeNameInfo = context.Settings.TypeNameInfo;

                            var count = globalTypes.Count;
                            for (var i = 0; i < count; i++)
                            {
                                kvp = globalTypes[i];

                                if (formatting)
                                {
                                    if (i > 0)
                                    {
                                        builder.Append(',');
                                    }
                                    WriteIndent(context);
                                    builder.Append('"');
                                    builder.Append(kvp.Key.ToString(JaysonConstants.InvariantCulture));
                                    builder.Append("\": ");
                                }
                                else
                                {
                                    if (i == 0)
                                    {
                                        builder.Append('"');
                                    }
                                    else
                                    {
                                        builder.Append(",\"");
                                    }
                                    builder.Append(kvp.Key.ToString(JaysonConstants.InvariantCulture));
                                    builder.Append("\":");
                                }

                                builder.Append('"');
                                builder.Append(JaysonTypeInfo.GetTypeName(kvp.Value, typeNameInfo));
                                builder.Append('"');
                            }
                        }
                        finally
                        {
                            context.ObjectDepth--;
                        }
                    }
                }
                finally
                {
                    if (formatting)
                    {
                        WriteIndent(context);
                    }
                    builder.Append('}');
                    context.ObjectDepth--;
                }
            }
        }

        public static string ToJsonString(object obj, JaysonSerializationSettings settings = null,
            Func<string, object, object> filter = null)
        {
            var builder = ToJsonString(obj, null, settings, filter);
            return builder.ToString();
        }

        public static StringBuilder ToJsonString(object obj, StringBuilder builder,
            JaysonSerializationSettings settings = null, Func<string, object, object> filter = null)
        {
            if (obj == null)
            {
                return null;
            }

            settings = settings ?? JaysonSerializationSettings.Default;

            var formatter = new JaysonFormatter(
                numberFormat: settings.NumberFormat,
                timeSpanFormat: settings.TimeSpanFormat,
                dateFormatType: settings.DateFormatType,
                dateTimeFormat: settings.DateTimeFormat,
                dateTimeUTCFormat: settings.DateTimeUTCFormat,
                dateTimeZoneType: settings.DateTimeZoneType,
                dateTimeOffsetFormat: settings.DateTimeOffsetFormat,
                useEnumNames: settings.UseEnumNames,
                escapeChars: settings.EscapeChars,
                escapeUnicodeChars: settings.EscapeUnicodeChars,
                convertDecimalToDouble: settings.ConvertDecimalToDouble,
                guidAsByteArray: settings.GuidAsByteArray,
                floatNanStrategy: settings.FloatNanStrategy,
                floatInfinityStrategy: settings.FloatInfinityStrategy
            );

            var objType = obj.GetType();
            if (JaysonTypeInfo.IsJPrimitive(objType))
            {
                builder = builder ?? new StringBuilder(100, int.MaxValue);

                if (settings.TypeNames == JaysonTypeNameSerialization.None)
                {
                    Format(obj, objType, builder, settings, formatter);
                    return builder;
                }

                var primeStack = JaysonStackList<object>.Get();
                try
                {
                    using (var context = new JaysonSerializationContext(
                        filter: filter,
                        builder: builder,
                        formatter: formatter,
                        globalTypes: settings.UseGlobalTypeNames ? new JaysonSerializationTypeList() : null,
                        settings: settings,
                        stack: primeStack
                    ))
                    {
                        if (settings.UseGlobalTypeNames)
                        {
                            context.ObjectDepth++;

                            if (settings.Formatting != JaysonFormatting.None)
                            {
                                builder.Append('{');
                                WriteIndent(context);
                                builder.Append("\"$value\": ");
                            }
                            else
                            {
                                builder.Append("{\"$value\":");
                            }
                        }

                        context.ObjectDepth++;
                        WritePrimitiveTypeName(objType, context);
                        formatter.Format(obj, objType, builder);

                        context.ObjectDepth--;
                        WriteIndent(context);
                        builder.Append('}');

                        if (settings.UseGlobalTypeNames)
                        {
                            context.ObjectDepth--;

                            WriteGlobalTypes(context, false);

                            WriteIndent(context);
                            builder.Append('}');
                        }
                    }
                    return builder;
                }
                finally
                {
                    JaysonStackList<object>.Release(primeStack);
                }
            }

            var stack = JaysonStackList<object>.Get();
            try
            {
                builder = builder ?? new StringBuilder(2048, int.MaxValue);

                using (var context = new JaysonSerializationContext(
                    filter: filter,
                    builder: builder,
                    formatter: formatter,
                    globalTypes: settings.UseGlobalTypeNames ? new JaysonSerializationTypeList() : null,
                    settings: settings,
                    stack: stack
                ))
                {
                    if (settings.UseGlobalTypeNames)
                    {
                        context.ObjectDepth++;

                        if (settings.Formatting != JaysonFormatting.None)
                        {
                            builder.Append('{');
                            WriteIndent(context);
                            builder.Append("\"$value\": ");
                        }
                        else
                        {
                            builder.Append("{\"$value\":");
                        }
                    }

                    WriteJsonObject(obj, objType, null, context);

                    if (settings.UseGlobalTypeNames)
                    {
                        context.ObjectDepth--;

                        WriteGlobalTypes(context, false);

                        WriteIndent(context);
                        builder.Append('}');
                    }
                }
                return builder;
            }
            finally
            {
                JaysonStackList<object>.Release(stack);
            }
        }

        # endregion ToJsonString
    }

    # endregion JsonConverter
}
