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
using System.Text;

namespace Sweet.Jayson
{
	# region JsonConverter

	public static partial class JaysonConverter
	{
		# region ToJsonString

        # region Write Type Name

        private static void WriteObjectTypeName (Type objType, JaysonSerializationContext context)
		{
			StringBuilder builder = context.Builder;
			if (context.Settings.Formatting) {
				builder.Append ('{');
				builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);

				if (context.Settings.UseGlobalTypeNames) {
					builder.Append ("\"$type\": ");
					builder.Append (context.GlobalTypes.Register (objType));
				} else {
					builder.Append ("\"$type\": \"");
					builder.Append(JaysonTypeInfo.GetTypeName(objType, context.Settings.TypeNameInfo));
					builder.Append ('"');
				}
			}
			else {
				if (context.Settings.UseGlobalTypeNames) {
					builder.Append ("{\"$type\":");
					builder.Append (context.GlobalTypes.Register (objType));
				} else {
					builder.Append ("{\"$type\":\"");
					builder.Append(JaysonTypeInfo.GetTypeName(objType, context.Settings.TypeNameInfo));
					builder.Append ('"');
				}
			}
		}

		private static void WriteListTypeName (Type objType, JaysonSerializationContext context)
		{
			StringBuilder builder = context.Builder;
			if (context.Settings.Formatting) {
				builder.Append ('{');
				builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);

				if (context.Settings.UseGlobalTypeNames) {
					builder.Append ("\"$type\": ");
					builder.Append (context.GlobalTypes.Register (objType));
				} else {
					builder.Append ("\"$type\": \"");
					builder.Append (JaysonTypeInfo.GetTypeName (objType, context.Settings.TypeNameInfo));
					builder.Append ('"');
				}

				builder.Append (',');
				builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				builder.Append ("\"$values\": [");
			}
			else {
				if (context.Settings.UseGlobalTypeNames) {
					builder.Append ("{\"$type\":");
					builder.Append (context.GlobalTypes.Register (objType));
					builder.Append (",\"$values\":[");
				} else {
					builder.Append ("{\"$type\":\"");
					builder.Append (JaysonTypeInfo.GetTypeName (objType, context.Settings.TypeNameInfo));
					builder.Append ("\",\"$values\":[");
				}
			}
		}

		private static bool WritePrimitiveTypeName (Type objType, JaysonSerializationContext context)
		{
			try {
				if (context.SkipCurrentType && 
                    context.CurrentType == objType) {
					return false;
				}

				StringBuilder builder = context.Builder;
				if (context.Settings.Formatting) {
					builder.Append ('{');
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);

					if (context.Settings.UseGlobalTypeNames) {
						builder.Append ("\"$type\": ");
						builder.Append (context.GlobalTypes.Register (objType));
					} else {
						builder.Append ("\"$type\": \"");
						builder.Append (JaysonTypeInfo.GetTypeName (objType, context.Settings.TypeNameInfo));
						builder.Append ('"');
					}

					builder.Append (',');
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					builder.Append ("\"$value\": ");
				}
				else {
					if (context.Settings.UseGlobalTypeNames) {
						builder.Append ("{\"$type\":");
						builder.Append (context.GlobalTypes.Register (objType));
						builder.Append (",\"$value\":");
					} else {
						builder.Append ("{\"$type\":\"");
						builder.Append (JaysonTypeInfo.GetTypeName (objType, context.Settings.TypeNameInfo));
						builder.Append ("\",\"$value\":");
					}
				}
			} finally {
				context.CurrentType = null;
				context.SkipCurrentType = false;
			}
			return true;
		}

		private static bool WriteObjectType(object obj, JaysonSerializationContext context)
		{
			try {
				switch (context.Settings.TypeNames) {
				case JaysonTypeNameSerialization.None:
					break;
				case JaysonTypeNameSerialization.Auto: 
					{
						if (obj == null) {
							return false;
						}

						Type objType = obj.GetType ();

						if (objType != context.CurrentType && 
							objType != JaysonConstants.DefaultDictionaryType) {
							WriteObjectTypeName (objType, context);
							return true;
						}
					}
					break;
				case JaysonTypeNameSerialization.All: 
					{
						if (obj == null) {
							return false;
						}

						Type objType = obj.GetType ();
						if (context.SkipCurrentType
							&& context.CurrentType == objType) {
							return false;
						}

						WriteObjectTypeName (objType, context);
						return true;
					}
				case JaysonTypeNameSerialization.Objects: 
				case JaysonTypeNameSerialization.AllButNoPrimitive:
					{
						if (obj == null) {
							return false;
						}

						Type objType = obj.GetType ();
						JaysonTypeInfo info = JaysonTypeInfo.GetTypeInfo (objType);

						if (!info.JPrimitive) {
							if (context.SkipCurrentType
								&& context.CurrentType == objType) {
								return false;
							}

							WriteObjectTypeName (objType, context);
							return true;
						}
					}
					break;
				}
			} finally {
				context.CurrentType = null;
				context.SkipCurrentType = false;
			}
			return false;
		}

		private static bool WriteObjectType(Type objType, JaysonSerializationContext context)
		{
			try {
				switch (context.Settings.TypeNames) {
				case JaysonTypeNameSerialization.None:
					break;
				case JaysonTypeNameSerialization.Auto: 
					{
						if (objType != context.CurrentType &&
							objType != JaysonConstants.DefaultDictionaryType) {
							WriteObjectTypeName (objType, context);
							return true;
						}
					}
					break;
				case JaysonTypeNameSerialization.All: 
					{
						if (context.SkipCurrentType
							&& context.CurrentType == objType) {
							return false;
						}

						WriteObjectTypeName (objType, context);
						return true;
					}
				case JaysonTypeNameSerialization.Objects: 
				case JaysonTypeNameSerialization.AllButNoPrimitive:
					{
						JaysonTypeInfo info = JaysonTypeInfo.GetTypeInfo (objType);
						if (!info.JPrimitive) {
							if (context.SkipCurrentType
								&& context.CurrentType == objType) {
								return false;
							}

							WriteObjectTypeName (objType, context);
							return true;
						}
					}
					break;
				}
			} finally {
				context.CurrentType = null;
				context.SkipCurrentType = false;
			}
			return false;
		}

		private static bool WriteListType(object obj, JaysonSerializationContext context)
		{
			try {
				switch (context.Settings.TypeNames) {
				case JaysonTypeNameSerialization.None:
					break;
				case JaysonTypeNameSerialization.Auto: 
					{
						if (obj == null) {
							return false;
						}

						Type objType = obj.GetType ();

						if (objType != context.CurrentType &&
							objType != JaysonConstants.DefaultDictionaryType) {
							WriteListTypeName (objType, context);
							return true;
						}
					}
					break;
				case JaysonTypeNameSerialization.All: 
				case JaysonTypeNameSerialization.Arrays: 
				case JaysonTypeNameSerialization.AllButNoPrimitive:
					{
						if (obj == null) {
							return false;
						}

						Type objType = obj.GetType ();
						if (context.SkipCurrentType
							&& context.CurrentType == objType) {
							return false;
						}

						WriteListTypeName (objType, context);
						return true;
					}
				default:
					break;
				}
			} finally {
				context.CurrentType = null;
				context.SkipCurrentType = false;
			}
			return false;
		}

		private static bool WriteListType(Type objType, JaysonSerializationContext context)
		{
			try {
				switch (context.Settings.TypeNames) {
				case JaysonTypeNameSerialization.None:
					break;
				case JaysonTypeNameSerialization.Auto: 
					{
						if (objType != context.CurrentType &&
							objType != JaysonConstants.DefaultDictionaryType) {
							WriteListTypeName (objType, context);
							return true;
						}
					}
					break;
				case JaysonTypeNameSerialization.All: 
				case JaysonTypeNameSerialization.Arrays: 
				case JaysonTypeNameSerialization.AllButNoPrimitive:
					{
						if (context.SkipCurrentType
							&& context.CurrentType == objType) {
							return false;
						}

						WriteListTypeName (objType, context);
						return true;
					}
				default:
					context.SkipCurrentType = false;
					break;
				}
			} finally {
				context.CurrentType = null;
				context.SkipCurrentType = false;
			}
			return false;
		}

		private static bool WriteByteArrayType(JaysonSerializationContext context)
		{
			try {
				switch (context.Settings.TypeNames) {
				case JaysonTypeNameSerialization.None:
					break;
				case JaysonTypeNameSerialization.Auto: 
					{
						if (context.CurrentType != typeof(byte[])) {
							return WritePrimitiveTypeName (typeof(byte[]), context);
						}
					}
					break;
				case JaysonTypeNameSerialization.All: 
				case JaysonTypeNameSerialization.Arrays:
				case JaysonTypeNameSerialization.AllButNoPrimitive:
					{
						if (context.SkipCurrentType
							&& context.CurrentType == typeof(byte[])) {
							return false;
						}

						return WritePrimitiveTypeName (typeof(byte[]), context);
					}
				default:
					break;
				}
			} finally {
				context.CurrentType = null;
				context.SkipCurrentType = false;
			}
			return false;
		}

        # endregion Write Type Name

        private static bool ValidObjectDepth(JaysonSerializationContext context)
        {
            if (context.Settings.MaxObjectDepth > 0 &&
                context.ObjectDepth > context.Settings.MaxObjectDepth)
            {
                if (context.Settings.RaiseErrorOnMaxObjectDepth)
                {
					throw new JaysonException(String.Format(JaysonError.MaximumObjectDepthExceed,
                        context.Settings.MaxObjectDepth));
                }
                return false;
            }
            return true;
        }

		private static bool ValidObjectDepth(int currDepth, int maxDepth, bool raiseError)
		{
			if (maxDepth > 0 && currDepth > maxDepth)
			{
				if (raiseError)
				{
					throw new JaysonException(String.Format(JaysonError.MaximumObjectDepthExceed,
						maxDepth));
				}
				return false;
			}
			return true;
		}

		private static void WriteKeyFast(string key, JaysonSerializationContext context, bool isFirst)
		{
			StringBuilder builder = context.Builder;
			JaysonSerializationSettings settings = context.Settings;

			if (!isFirst)
			{
				builder.Append(',');
			}
			if (settings.Formatting)
			{
				builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
			}

			builder.Append('"');
			builder.Append(settings.CaseSensitive ? key : JaysonCommon.AsciiToLower (key));

			if (settings.Formatting) {
				builder.Append ("\": ");
			} else {
				builder.Append ('"');
				builder.Append (':');
			}
		}

		private static bool WriteKeyValueEntryFast(string key, bool value, JaysonSerializationContext context, bool isFirst)
		{
			WriteKeyFast (key, context, isFirst);
			context.Builder.Append (value ? "true" : "false");
			return false; // isFirst
		}

		private static bool WriteKeyValueEntryFast(string key, int value, JaysonSerializationContext context, bool isFirst)
		{
			WriteKeyFast (key, context, isFirst);
			context.Builder.Append (value.ToString (JaysonConstants.InvariantCulture));
			return false; // isFirst
		}

		private static bool WriteKeyValueEntryFast(string key, long value, JaysonSerializationContext context, bool isFirst)
		{
			WriteKeyFast (key, context, isFirst);
			context.Builder.Append (value.ToString (JaysonConstants.InvariantCulture));
			return false; // isFirst
		}

		private static bool WriteKeyValueEntryFast(string key, string value, JaysonSerializationContext context, bool isFirst)
		{
			WriteKeyFast (key, context, isFirst);

			StringBuilder builder = context.Builder;

			builder.Append ('"');
			builder.Append (value);
			builder.Append ('"');
			return false; // isFirst
		}

		private static bool WriteProperty(string propertyName, object value, Type expectedValueType, 
			JaysonSerializationContext context, bool isFirst)
		{
			if ((value != null) && (value != DBNull.Value))
			{
				StringBuilder builder = context.Builder;
				JaysonSerializationSettings settings = context.Settings;

				if (!isFirst)
				{
					builder.Append(',');
				}
				if (settings.Formatting)
				{
					builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
				}

				builder.Append('"');
				builder.Append(settings.CaseSensitive ? propertyName : JaysonCommon.AsciiToLower (propertyName));

				if (settings.Formatting)
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
					context.Formatter.Format(value, valueType, context.Builder);
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
					bool typeWritten = false;
					context.ObjectDepth++;
					try
					{
						typeWritten = WritePrimitiveTypeName(valueType, context);
						context.Formatter.Format(value, valueType, builder);
					}
					finally
					{
						context.ObjectDepth--;
						if (typeWritten) 
						{
							if (context.Settings.Formatting) 
							{
								builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
							}
							builder.Append ('}');
						}
					}
					return false; // isFirst
				}

				context.Formatter.Format(value, valueType, context.Builder);
				return false; // isFirst
			}
			else if (!context.Settings.IgnoreNullValues)
			{
				StringBuilder builder = context.Builder;
				JaysonSerializationSettings settings = context.Settings;

				if (!isFirst)
				{
					builder.Append(',');
				}
				if (context.Settings.Formatting)
				{
					builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
				}

				builder.Append('"');
				builder.Append(settings.CaseSensitive ? propertyName : JaysonCommon.AsciiToLower (propertyName));

				if (settings.Formatting)
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
            if ((value != null) && (value != DBNull.Value))
            {
                StringBuilder builder = context.Builder;
                JaysonSerializationSettings settings = context.Settings;

                if (!isFirst)
                {
                    builder.Append(',');
                }
                if (settings.Formatting)
                {
                    builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
                }

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

                if (settings.Formatting)
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
                    context.Formatter.Format(value, valueType, context.Builder);
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
					bool typeWritten = false;
                    context.ObjectDepth++;
                    try
                    {
                        typeWritten = WritePrimitiveTypeName(valueType, context);
                        context.Formatter.Format(value, valueType, builder);
                    }
                    finally
                    {
                        context.ObjectDepth--;
						if (typeWritten) 
						{
							if (context.Settings.Formatting) 
							{
								builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
							}
							builder.Append ('}');
						}
                    }
                    return false; // isFirst
                }

                context.Formatter.Format(value, valueType, context.Builder);
                return false; // isFirst
            }
            else if (forceNullValues || !context.Settings.IgnoreNullValues)
            {
                StringBuilder builder = context.Builder;
                JaysonSerializationSettings settings = context.Settings;

                if (!isFirst)
                {
                    builder.Append(',');
                }
                if (context.Settings.Formatting)
                {
                    builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
                }

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

                if (settings.Formatting)
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
			if ((value != null) && (value != DBNull.Value))
			{
				Type valueType = value.GetType();
				if (CanWriteJsonObject(value, valueType, context))
				{
					if (!isFirst)
					{
						context.Builder.Append(',');
					}
					if (context.Settings.Formatting) {
						context.Builder.Append (JaysonConstants.Indentation[context.ObjectDepth]);
					}

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
				if (context.Settings.Formatting) {
					context.Builder.Append (JaysonConstants.Indentation[context.ObjectDepth]);
				}

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
                JaysonSerializationSettings settings = context.Settings;
                bool formatting = settings.Formatting;

				StringBuilder builder = context.Builder;
                if (formatting)
                {
                    builder.Append(',');
                    builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
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

                context.ObjectDepth++;
                try
                {
					if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth))
                    {
                        return;
                    }

					DataColumn dataColumn;
                    DataColumnCollection columns = dataTable.Columns;

                    int colCount = columns.Count;
                    List<JaysonTuple<DataColumn, JaysonTypeInfo>> columnsInfo = new List<JaysonTuple<DataColumn, JaysonTypeInfo>>();

                    for (int i = 0; i < colCount; i++)
                    {
						dataColumn = columns[i];
                        columnsInfo.Add(new JaysonTuple<DataColumn, JaysonTypeInfo>(dataColumn, 
							JaysonTypeInfo.GetTypeInfo(dataColumn.DataType)));
                    }

					DataRow dataRow;
					object cellValue;
                    int rowCount = rows.Count;

                    JaysonTypeCode colTypeCode;
                    JaysonTuple<DataColumn, JaysonTypeInfo> columnInfo;

                    JaysonFormatter formatter = context.Formatter;
                    bool escapeChars = settings.EscapeChars;
                    bool escapeUnicodeChars = settings.EscapeUnicodeChars;

					context.ObjectDepth++;
					try
					{
	                    for (int i = 0; i < rowCount; i++)
	                    {
	                        if (i > 0)
	                        {
	                            builder.Append(',');
	                        }
	                        if (formatting)
	                        {
	                            builder.Append(JaysonConstants.Indentation[context.ObjectDepth-1]);
	                        }
	                        builder.Append('[');

	                        try
	                        {
	                            dataRow = rows[i];
	                            for (int j = 0; j < colCount; j++)
	                            {
	                                if (j > 0)
	                                {
	                                    builder.Append(',');
	                                }
	                                if (formatting)
	                                {
	                                    builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
	                                }

	                                columnInfo = columnsInfo[j];
	                                cellValue = dataRow[columnInfo.Item1];

	                                if (cellValue == null || cellValue == DBNull.Value)
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
	                                            builder.Append((bool)cellValue ? "true" : "false");
	                                            break;
	                                        case JaysonTypeCode.BoolNullable:
	                                            builder.Append(((bool?)cellValue).Value ? "true" : "false");
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
													
													WriteJsonObject(cellValue, cellValue.GetType (), null, context);
	                                            }
	                                            break;
	                                    }
	                                }
	                            }
	                        }
	                        finally
	                        {
	                            if (formatting)
	                            {
	                                builder.Append(JaysonConstants.Indentation[context.ObjectDepth-1]);
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
                    if (formatting)
                    {
                        builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
                    }
                    builder.Append(']');
                }
            }
        }

		private static bool WriteDataColumnNames(string columnsName, DataColumn[] columns, 
			JaysonSerializationContext context, bool isFirst)
		{
			if (columns == null || columns.Length == 0) {
				return isFirst;
			}

			var settings = context.Settings;
			bool formatting = settings.Formatting;

			StringBuilder builder = context.Builder;
			JaysonFormatter formatter = context.Formatter;

			if (!isFirst) {
				builder.Append (',');
			}
			if (formatting) {
				builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
			}
			if (!settings.CaseSensitive) {
				formatter.Format (columnsName.ToLower (JaysonConstants.InvariantCulture), builder);
			} else {
				formatter.Format (columnsName, builder);
			}
			if (formatting) {
				builder.Append (": [");
			} else {
				builder.Append (":[");
			}

			context.ObjectDepth++;
			try {
				if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth)) {
					return false; // isFirst
				}

				DataColumn column;
				int columnCount = columns.Length;

				for (int i = 0; i < columnCount; i++) {
					if (i > 0) {
						builder.Append (',');
					}
					if (formatting) {
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					}

					column = columns [i];
					if (column.ColumnName == null) {
						builder.Append (JaysonConstants.Null);
					} else {
						formatter.Format (column.ColumnName, builder);
					}
				}
			} finally {
				context.ObjectDepth--;
				if (formatting) {
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				}
				builder.Append (']');
			}
			return false; // isFirst
		}

        private static void WriteDataColumns(DataTable dataTable, JaysonSerializationContext context)
        {
            var columns = dataTable.Columns;
            int columnCount = columns.Count;

            if (columnCount > 0)
            {
                JaysonSerializationSettings settings = context.Settings;
                bool formatting = settings.Formatting;

				StringBuilder builder = context.Builder;
                if (formatting)
                {
                    builder.Append(',');
                    builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
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

                context.ObjectDepth++;
                try
                {
					if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth)) {
                        return;
                    }

                    bool isFirst;
                    DataColumn column;
                    string defaultNamespace = dataTable.Namespace;

                    int iValue;
                    bool bValue;
                    long lValue;
                    string sValue;
                    MappingType mValue;

                    for (int i = 0; i < columnCount; i++)
                    {
                        if (i > 0)
                        {
                            builder.Append(',');
                        }
                        if (formatting)
                        {
                            builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
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
                                builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
                            }
                            builder.Append('}');
                        }
                    }
                }
                finally
                {
                    context.ObjectDepth--;
                    if (formatting)
                    {
                        builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
                    }
                    builder.Append(']');
                }
            }
        }

        private static void WriteDataTable(DataTable dataTable, JaysonSerializationContext context)
        {
			var settings = context.Settings;

			context.ObjectDepth++;
            StringBuilder builder = context.Builder;
            try
            {
                bool isFirst = true;
                if (WriteObjectType(dataTable, context))
                {
                    isFirst = false;
                }
                else
                {
                    builder.Append('{');
                }

				if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth)) {
                    return;
                }

                isFirst = WriteKeyValueEntry("$dt", "Tbl", null, context, isFirst);
                bool bValue = dataTable.CaseSensitive;
                if (bValue)
                {
					isFirst = WriteKeyValueEntryFast("Cs", bValue, context, isFirst);
                }
                
				string sValue = dataTable.DisplayExpression;
                if (!String.IsNullOrEmpty(sValue))
                {
					isFirst = WriteProperty("De", sValue, null, context, isFirst);
                }
                
				CultureInfo cValue = dataTable.Locale;
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
					isFirst = WriteDataColumnNames ("Pk", columns, context, isFirst);
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
                if (settings.Formatting)
                {
                    builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
                }
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

            int relationCount = relations.Count;

            StringBuilder builder = context.Builder;
            JaysonSerializationSettings settings = context.Settings;
            bool formatting = settings.Formatting;

            if (!isFirst)
            {
                builder.Append(',');
            }
            if (formatting)
            {
                builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
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

            context.ObjectDepth++;
            try
            {
				if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth)) {
                    return false; // isFirst
                }

                bool bValue;
                string sValue;
                DataTable dValue;

                DataRelation relation;
                string defaultNamespace = dataSet.Namespace;

				context.ObjectDepth++; 
				try {
	                for (int i = 0; i < relationCount; i++)
	                {
	                    if (i > 0)
	                    {
	                        builder.Append(',');
	                    }
	                    if (formatting)
	                    {
	                        builder.Append(JaysonConstants.Indentation[context.ObjectDepth-1]);
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
	                            builder.Append(JaysonConstants.Indentation[context.ObjectDepth-1]);
	                        }
	                        builder.Append('}');
	                    }
	                }
				}finally {
					context.ObjectDepth--;
				}
            }
            finally
            {
                context.ObjectDepth--;
                if (formatting)
                {
                    builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
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

			int tableCount = tables.Count;

			StringBuilder builder = context.Builder;
			var settings = context.Settings;
			bool formatting = settings.Formatting;

			if (!isFirst) 
			{
				builder.Append (',');
			}
			if (formatting) 
			{
				builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				if (!settings.CaseSensitive) 
				{
					builder.Append ("\"tab\": [");
				} 
				else 
				{
					builder.Append ("\"Tab\": [");
				}
			} 
			else 
			{
				if (!settings.CaseSensitive) 
				{
					builder.Append ("\"tab\":[");
				} 
				else 
				{
					builder.Append ("\"Tab\":[");
				}
			}

			context.ObjectDepth++;
			try 
			{
				if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth)) {
					return false; // isFirst
				}

				for (int i = 0; i < tableCount; i++) 
				{
					if (i > 0) 
					{
						builder.Append (',');
					}
					if (formatting) 
					{
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					}

					WriteDataTable (tables[i], context);
				}
			} 
			finally 
			{
				context.ObjectDepth--;
				if (formatting) 
				{
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				}
				builder.Append (']');
			}
			return false; // isFirst
		}

        private static void WriteDataSet(DataSet dataSet, JaysonSerializationContext context)
        {
			var settings = context.Settings;
			bool formatting = settings.Formatting;

			context.ObjectDepth++;
            StringBuilder builder = context.Builder;
            try
            {
                bool isFirst = true;
                if (WriteObjectType(dataSet, context))
                {
                    isFirst = false;
                }
                else
                {
                    builder.Append('{');
                }

				if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth)) {
                    return;
                }

                isFirst = WriteKeyValueEntry("$dt", "Ds", null, context, isFirst);
                bool bValue = dataSet.CaseSensitive;
                if (bValue)
                {
					isFirst = WriteKeyValueEntryFast("Cs", bValue, context, isFirst);
                }

                string sValue = dataSet.DataSetName;
                if (!String.IsNullOrEmpty(sValue))
                {
					isFirst = WriteProperty("Dsn", sValue, null, context, isFirst);
                }
                
				bValue = dataSet.EnforceConstraints;
                if (!bValue)
                {
					isFirst = WriteKeyValueEntryFast("Ec", bValue, context, isFirst);
                }
                
				CultureInfo cValue = dataSet.Locale;
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
                    builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
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
			bool ignoreNullValues = context.Settings.IgnoreNullValues;

			Func<string, object, object> filter = context.Filter;
			bool canFilter = (filter != null);

			foreach (DictionaryEntry dEntry in source)
			{
				value = dEntry.Value;

				keyObj = dEntry.Key;
				key = (keyObj as string) ?? keyObj.ToString ();

				if ((value != null) && canFilter)
				{
					value = filter(key, value);
				}

				if ((value != null) || !ignoreNullValues)
				{
					yield return new JaysonKeyValue<object, object> { Key = keyObj, Value = value };
				}
			}
		}

        private static void WriteDictionaryObject(IDictionary obj, Type keyType, Type valueType, JaysonSerializationContext context)
        {
			var settings = context.Settings;
			bool formatting = settings.Formatting;

			bool typeWritten = false;

			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
            try
            {
                typeWritten = WriteObjectType(obj, context);
                if (!typeWritten)
                {
					builder.Append('{');
                }

				if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth)) {
                    return;
                }

                if (obj.Count > 0)
                {
                    if (typeWritten) {
                        builder.Append(',');
                    }
					if (formatting) {
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					}

                    context.ObjectDepth++;
                    try
                    {
						if (formatting) {
							builder.Append("\"$kv\": [");
						} else {
							builder.Append("\"$kv\":[");
						}

						context.ObjectDepth++;
						try 
						{
							bool isFirstItem = true;

	                        if (context.Settings.OrderNames)
	                        {
	                            foreach (var kvp in GetDictionaryEntries(obj, context).OrderBy(kvp => kvp.Key))
	                            {
									if (!isFirstItem) {
										builder.Append (',');
									}
									if (formatting) {
										builder.Append (JaysonConstants.Indentation [context.ObjectDepth-1]);
									}

									try {
										builder.Append ('{');

                                        WriteKeyValueEntry(key: "$k", value: kvp.Key, expectedValueType: keyType, context: context, 
                                            isFirst: true, forceNullValues: true);
                                        WriteKeyValueEntry(key: "$v", value: kvp.Value, expectedValueType: valueType, context: context, 
                                            isFirst: false, forceNullValues: true);
									} finally {
										isFirstItem = false;
										if (formatting) {
											builder.Append (JaysonConstants.Indentation [context.ObjectDepth-1]);
										}
										builder.Append ('}');
									}
	                            }
	                        }
	                        else
	                        {
								string key;
								object value;
								object keyObj;

								Func<string, object, object> filter = context.Filter;
								bool canFilter = (filter != null);

	                            foreach (DictionaryEntry dEntry in obj)
	                            {
	                                keyObj = dEntry.Key;
	                                value = dEntry.Value;

	                                if ((value != null) && canFilter)
	                                {
										key = (keyObj is string) ? (string)keyObj : keyObj.ToString();
	                                    value = filter(key, value);
	                                }

									if (!isFirstItem) {
										builder.Append (',');
									}
									if (formatting) {
										builder.Append (JaysonConstants.Indentation [context.ObjectDepth - 1]);
									}

									try {
										builder.Append ('{');

                                        WriteKeyValueEntry(key: "$k", value: keyObj, expectedValueType: keyType, context: context, 
                                            isFirst: true, forceNullValues: true);
                                        WriteKeyValueEntry(key: "$v", value: value, expectedValueType: valueType, context: context, 
                                            isFirst: false, forceNullValues: true);
									}
									finally {
										isFirstItem = false;
										if (formatting) {
											builder.Append (JaysonConstants.Indentation [context.ObjectDepth - 1]);
										}
										builder.Append ('}');
									}
	                            }
	                        }
						} finally {
							context.ObjectDepth--;
						}
                    }
                    finally
                    {
                        context.ObjectDepth--;
						if (formatting) {
							builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
						}
						builder.Append (']');
                    }
                }
            }
            finally
            {
                context.ObjectDepth--;
                if (formatting)
                {
                    builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
                }
				builder.Append('}');
            }
        }

		private static void WriteDictionaryStringKey(IDictionary obj, JaysonSerializationContext context)
		{
			var settings = context.Settings;

			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			try {
				bool isFirst = true;
				if (WriteObjectType (obj, context)) {
					isFirst = false;
				} else {
					builder.Append ('{');
				}

				if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth)) {
					return;
				}

				if (obj.Count > 0) {
					string key;
					object keyObj;

					if (settings.OrderNames) {
						foreach (var kvp in GetDictionaryEntries(obj, context).OrderBy(kvp => kvp.Key)) {
							keyObj = kvp.Key;
							key = (keyObj as string) ?? keyObj.ToString ();

							isFirst = WriteKeyValueEntry (key: key, value: kvp.Value, expectedValueType: null, context: context, 
								isFirst: isFirst, forceNullValues: true);
						}
					} else {
						object value;

						Func<string, object, object> filter = context.Filter;
						bool canFilter = (filter != null);

						foreach (DictionaryEntry dEntry in obj) {
							keyObj = dEntry.Key;
							value = dEntry.Value;

							key = (keyObj as string) ?? keyObj.ToString ();

							if ((value != null) && canFilter) {
								value = filter (key, value);
							}

                            isFirst = WriteKeyValueEntry(key: key, value: value, expectedValueType: null, context: context, 
								isFirst: isFirst, forceNullValues: true);
						}
					}
				}
			} finally {
				context.ObjectDepth--;
				if (settings.Formatting) {
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				} 
				builder.Append ('}');
			}
		}

		private static IEnumerable<JaysonKeyValue<string, object>> GetStringDictionaryEntries(StringDictionary source,
			JaysonSerializationContext context)
		{
			string key;
			object value;
			object keyObj;
			bool ignoreNullValues = context.Settings.IgnoreNullValues;

			Func<string, object, object> filter = context.Filter;
			bool canFilter = (filter != null);

			foreach (DictionaryEntry dEntry in source)
			{
				value = dEntry.Value;

				keyObj = dEntry.Key;
				key = (keyObj is string) ? (string)keyObj : keyObj.ToString();

				if ((value != null) && canFilter)
				{
					value = filter(key, value);
				}

				if ((value != null) || !ignoreNullValues)
				{
					yield return new JaysonKeyValue<string, object> { Key = key, Value = value };
				}
			}
		}

		private static void WriteStringDictionary(StringDictionary obj, JaysonSerializationContext context)
		{
			var settings = context.Settings;

			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			try {
				bool isFirst = true;
				if (WriteObjectType (obj, context)) {
					isFirst = false;
				} else {
					builder.Append ('{');
				}

				if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth)) {
					return;
				}

				if (obj.Count > 0) {
					if (settings.OrderNames) {
						foreach (var kvp in GetStringDictionaryEntries(obj, context).OrderBy(kvp => kvp.Key)) {
                            isFirst = WriteKeyValueEntry(kvp.Key, kvp.Value, typeof(string), context, isFirst);
						}
					} else {
                        string key;
                        object value;
                        object keyObj;

                        Func<string, object, object> filter = context.Filter;
                        bool canFilter = (filter != null);

                        foreach (DictionaryEntry dEntry in obj) {
							keyObj = dEntry.Key;
							value = dEntry.Value;

							key = (keyObj is string) ? (string)keyObj : keyObj.ToString ();

							if ((value != null) && canFilter) {
								value = filter (key, value);
							}

                            isFirst = WriteKeyValueEntry(key, value, typeof(string), context, isFirst);
						}
					}
				}
			} finally {
				context.ObjectDepth--;
				if (settings.Formatting) {
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				} 
				builder.Append ('}');
			}
		}

		private static void WriteNameValueCollection(NameValueCollection obj, JaysonSerializationContext context)
		{
			var settings = context.Settings;

			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			try {
				bool isFirst = true;
				if (WriteObjectType (obj, context)) {
					isFirst = false;
				} else {
					builder.Append ('{');
				}

				if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth)) {
					return;
				}

				if (obj.Count > 0) {
					string[] keys = obj.AllKeys;
					if (settings.OrderNames) {
						keys = keys.OrderBy (_key => _key).ToArray ();
					}

					string key;
					object value;

					Func<string, object, object> filter = context.Filter;
					bool canFilter = (filter != null);

					int len = keys.Length;
					for (int i = 0; i < len; i++) {
						key = keys [i];
						value = obj [key];

						if ((value != null) && canFilter) {
							value = filter (key, value);
						}

						isFirst = WriteKeyValueEntry (key, value, typeof(string), context, isFirst);
					}
				}
			} finally {
				context.ObjectDepth--;
				if (settings.Formatting) {
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				} 
				builder.Append ('}');
			}
		}

		#if !(NET3500 || NET3000 || NET2000)
		private static void WriteDynamicObject(DynamicObject obj, JaysonSerializationContext context)
		{
			var settings = context.Settings;

			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			try {
				Type objType = obj.GetType ();

				bool isFirst = true;
				if (WriteObjectType (objType, context)) {
					isFirst = false;
				} else {
					builder.Append ('{');
				}

				if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth)) {
					return;
				}

				object value;

				Func<string, object, object> filter = context.Filter;
				bool canFilter = (filter != null);

				var fastDict = JaysonFastMemberCache.GetMembers(objType);
				if (fastDict.Count > 0)
				{
					IEnumerable<KeyValuePair<string, IJaysonFastMember>> members = fastDict;
					if (context.Settings.OrderNames)
					{
						members = members.OrderBy(kvp => kvp.Key);
					}

					bool ignoreReadOnlyMembers = settings.IgnoreReadOnlyMembers;
                    JaysonTypeOverride typeOverride = settings.GetTypeOverride(objType);
                    
                    string fKey;
                    string aliasKey;
					foreach (var memberKvp in members)
					{
						if (!ignoreReadOnlyMembers || memberKvp.Value.CanWrite)
						{
							fKey = memberKvp.Key;
                            if (typeOverride == null || !typeOverride.IsMemberIgnored(fKey))
                            {
                                value = memberKvp.Value.Get(obj);

                                if ((value != null) && canFilter)
                                {
                                    value = filter(fKey, value);
                                }

                                if (typeOverride != null)
                                {
                                    aliasKey = typeOverride.GetMemberAlias(fKey);
                                    if (!String.IsNullOrEmpty(aliasKey))
                                    {
                                        fKey = aliasKey;
                                    }
                                }

								context.CurrentType = memberKvp.Value.MemberType;
								isFirst = WriteProperty(fKey, value, null, context, isFirst);
                            }
						}
					}
				}

				IEnumerable<string> memberNames = obj.GetDynamicMemberNames();
				if (context.Settings.OrderNames)
				{
					memberNames = memberNames.OrderBy(name => name);
				}

				JaysonDynamicWrapper dObj = new JaysonDynamicWrapper((IDynamicMetaObjectProvider)obj);

				foreach (var dKey in memberNames)
				{
					value = dObj[dKey];

					if ((value != null) && canFilter)
					{
						value = filter(dKey, value);
					}

					isFirst = WriteProperty(dKey, value, null, context, isFirst);
				}
			}
			finally
			{
				context.ObjectDepth--;
				if (settings.Formatting) {
					builder.Append (JaysonConstants.Indentation[context.ObjectDepth]);
				} 
				builder.Append('}');
			}
		}
		#endif

		private static void WriteClassOrStruct(object obj, Type objType, JaysonSerializationContext context)
		{
			var settings = context.Settings;

			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			try {
				bool isFirst = true;
				if (WriteObjectType (objType, context)) {
					isFirst = false;
				} else {
					builder.Append ('{');
				}

				if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth)) {
					return;
				}

				var fastDict = JaysonFastMemberCache.GetMembers (objType);
				if (fastDict.Count > 0) {
					string key;
					object value;
                    string aliasKey;

					Func<string, object, object> filter = context.Filter;
					bool canFilter = (filter != null);

					IEnumerable<KeyValuePair<string, IJaysonFastMember>> members = fastDict;
					if (context.Settings.OrderNames) {
						members = members.OrderBy (kvp => kvp.Key);
					}

					bool ignoreReadOnlyMembers = settings.IgnoreReadOnlyMembers;
                    JaysonTypeOverride typeOverride = settings.GetTypeOverride(objType);

					foreach (var memberKvp in members) {
						if (!ignoreReadOnlyMembers || memberKvp.Value.CanWrite)
						{
							key = memberKvp.Key;
                            if (typeOverride == null || !typeOverride.IsMemberIgnored(key))
                            {
                                value = memberKvp.Value.Get(obj);

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

								context.CurrentType = memberKvp.Value.MemberType;
								isFirst = WriteProperty(key, value, null, context, isFirst);
                            }
						}
					}
				}
			} finally {
				context.ObjectDepth--;
				if (settings.Formatting) {
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				} 
				builder.Append ('}');
			}
		}

		private static void WriteGenericStringDictionary(IDictionary<string, object> obj, JaysonSerializationContext context)
		{
			var settings = context.Settings;

			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			try {
				bool isFirst = true;
				if (WriteObjectType (obj, context)) {
					isFirst = false;
				} else {
					builder.Append ('{');
				}

				if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth)) {
					return;
				}

				if (obj.Count > 0) {
					string key;
					object value;
					#if !(NET3500 || NET3000 || NET2000)
					bool isExpando = obj is ExpandoObject;
					#else
					bool isExpando = false;
					#endif

					Func<string, object, object> filter = context.Filter;
					bool canFilter = (filter != null);

					IEnumerable<KeyValuePair<string, object>> eDict = obj;
					if (settings.OrderNames) {
						eDict = eDict.OrderBy (kvp => kvp.Key);

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

					foreach (var kvp in obj) {
						key = kvp.Key;
						value = kvp.Value;

						if ((value != null) && canFilter) {
							value = filter (key, value);
						}

                        isFirst = WriteKeyValueEntry(key: key, value: value, expectedValueType: null, context: context, 
                            isFirst: isFirst, forceNullValues: false, ignoreEscape: isExpando);
					}
				}
			} finally {
				context.ObjectDepth--;
				if (context.Settings.Formatting) {
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				} 
				builder.Append ('}');
			}
		}

		private static void WriteByteArray(byte[] obj, JaysonSerializationContext context)
        {
			context.ObjectDepth++;
            bool typeWritten = WriteByteArrayType(context);

            bool formatting = context.Settings.Formatting;
			StringBuilder builder = context.Builder;
            try
            {
                if (typeWritten)
                {
                    context.ObjectDepth++;
                }

                if (!ValidObjectDepth(context))
                {
                    builder.Append("\"\"");
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
                        builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
                    }
                    builder.Append('}');
                }
            }
        }

		static void WriteDBNullArray (JaysonSerializationContext context, int length)
		{
			context.ObjectDepth++;
			bool typeWritten = WriteListType (typeof(DBNull[]), context);

			bool formatting = context.Settings.Formatting;
			StringBuilder builder = context.Builder;
			try {
				if (!typeWritten) {
					builder.Append ('[');
				} else {
					context.ObjectDepth++;
				}

				if (context.Settings.IgnoreNullListItems) {
					length = 0;
					return;
				} 

				if (!ValidObjectDepth(context)) {
					return;
				}

				int objectDepth = context.ObjectDepth;

				for (int i = 0; i < length; i++) {
					if (i == 0) {
						if (formatting) {
							builder.Append (JaysonConstants.Indentation[objectDepth]);
						}
						builder.Append (JaysonConstants.Null);
					} else if (formatting) {
						builder.Append (',');
						builder.Append (JaysonConstants.Indentation[objectDepth]);
						builder.Append (JaysonConstants.Null);
					} else {
						builder.Append (",null");
					}
				}
			} finally {
				context.ObjectDepth--;
				if (!formatting) {
					builder.Append (']');

					if (typeWritten) {
						context.ObjectDepth--;
						builder.Append ('}');
					}			
				} else {
					if (length > 0) {
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					}
					builder.Append (']');

					if (typeWritten) {
						context.ObjectDepth--;
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
						builder.Append ('}');
					}
				}
			}
		}

		private static void WritePrimitiveArray (Array obj, Type arrayType, JaysonSerializationContext context)
		{
			var settings = context.Settings;

			context.ObjectDepth++;
			bool typeWritten = WriteListType (obj, context);

			StringBuilder builder = context.Builder;
			bool formatting = settings.Formatting;
			try {
				if (!typeWritten) {
					builder.Append ('[');
				} else {
					context.ObjectDepth++;
				}

				if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth)) {
					return;
				}

				int objectDepth = context.ObjectDepth;
				JaysonFormatter formatter = context.Formatter;
				bool ignoreNullValues = settings.IgnoreNullValues;

				object item;
				bool isFirst = true;
				var objLen = obj.Length;

				for (int i = 0; i < objLen; i++) {
					item = obj.GetValue (i);

					if (item == null || item == DBNull.Value) {
						if (!ignoreNullValues) {
							if (isFirst) {
								isFirst = false;
								if (formatting) {
									builder.Append (JaysonConstants.Indentation[objectDepth]);
								}
								builder.Append (JaysonConstants.Null);
							} else {
								if (formatting) {
									builder.Append (',');
									builder.Append (JaysonConstants.Indentation[objectDepth]);
									builder.Append (JaysonConstants.Null);
								} else {
									builder.Append (",null");
								}
							}
						}
						continue;
					}

					if (!isFirst) {
						builder.Append (',');
					}
					if (formatting) {
						builder.Append (JaysonConstants.Indentation[objectDepth]);
					}

					isFirst = false;
					formatter.Format (item, arrayType, builder);
				}
			}
			finally {
				context.ObjectDepth--;

				if (!settings.Formatting) {
					builder.Append (']');

					if (typeWritten) {
						context.ObjectDepth--;
						builder.Append ('}');
					}			
				} else {
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					builder.Append (']');

					if (typeWritten) {
						context.ObjectDepth--;
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
						builder.Append ('}');
					}
				}
			}
		}

		private static void WriteMultiDimensionalArray(Array obj, Type objType, bool isRoot, 
            int[] rankLengths, int[] rankIndices, int currRank, JaysonSerializationContext context, 
            bool isFirst)
		{
			var settings = context.Settings;

			int length = rankLengths[currRank];
			bool formatting = settings.Formatting;

			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			try {
				if (currRank > 0) {
					builder.Append ('[');
				}

				if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth)) {
					return;
				}

				bool isFirstInner = true;
				for (int i = 0; i < length; i++) {
					rankIndices [currRank] = i;

					if (currRank == rankIndices.Length - 1) {
						isFirstInner = WriteEnumerableValue (obj.GetValue (rankIndices), context, isFirstInner);
					} else {
						if (!isFirstInner) {
							builder.Append (',');
						} 

						if (formatting) {
							builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
						}

						WriteMultiDimensionalArray (obj, objType, isRoot, rankLengths, rankIndices, 
							currRank + 1, context, isFirstInner);
						isFirstInner = false;
					}
				}
			} finally {
				context.ObjectDepth--;
				if (currRank > 0) {
					if (formatting && length > 0) {
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					}
					builder.Append (']');
				}
			}
		}

		private static void WriteMultiDimensionalArray(Array obj, Type objType, JaysonSerializationContext context)
		{
			var settings = context.Settings;

			bool isRoot = context.ObjectDepth == 0;

			context.ObjectDepth++;
			bool typeWritten = WriteListType (objType, context);

			bool isEmpty = true;
			StringBuilder builder = context.Builder;
			try {
				if (!typeWritten) {
					context.ObjectDepth--;
					builder.Append ('[');
				}

				if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth)) {
					return;
				}

				int rank = obj.Rank;
				int[] rankLengths = new int[rank];

                for (int i = 0; i < rank; i++) {
                    rankLengths[i] = obj.GetLength(i);
                    if (isEmpty) {
                        isEmpty = rankLengths[i] == 0;
                    }
                }

				WriteMultiDimensionalArray (obj, objType, isRoot, rankLengths, new int[rank], 0, context, true);
			} finally {
				if (!typeWritten) {
					if (settings.Formatting && !isEmpty) {
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					}
					builder.Append (']');
				} else {
					if (!settings.Formatting) {
						builder.Append (']');
						context.ObjectDepth--;
					} else {
                        if (!isEmpty) {
                            builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
                        }
						builder.Append (']');

						context.ObjectDepth--;
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					}
					builder.Append ('}');
				}
			}
		}

		private static void WriteEmptyArray(Array obj, Type objType, JaysonSerializationContext context)
		{
			context.ObjectDepth++;
			bool typeWritten = WriteListType (objType, context);

			StringBuilder builder = context.Builder;
			try {
				if (!typeWritten) {
					builder.Append ('[');
				} 

				if (!ValidObjectDepth(context)) {
					return;
				}
			} finally {
				context.ObjectDepth--;

                builder.Append(']');
                if (typeWritten)
                {
                    if (context.Settings.Formatting)
                    {
                        builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
                    }
                    builder.Append('}');
                }
            }
		}

		private static void WriteArray(Array obj, Type objType, JaysonSerializationContext context)
		{
			if (obj.Rank > 1) 
			{
				WriteMultiDimensionalArray (obj, objType, context);
				return;
			}

			int length = obj.Length;
			if (length == 0) 
			{
				WriteEmptyArray(obj, objType, context);
				return;
			}

            Type arrayType = JaysonTypeInfo.GetElementType(objType);
			if (arrayType == typeof(byte)) 
			{
				WriteByteArray ((byte[])obj, context);
				return;
			}

            var info = JaysonTypeInfo.GetTypeInfo(arrayType);
            if (info.JPrimitive || info.Enum) 
			{
				WritePrimitiveArray (obj, arrayType, context);
				return;
			}

			if (arrayType == typeof(DBNull)) 
			{
				WriteDBNullArray (context, length);
				return;
			}

			WriteIList (obj, objType, context);
		}

		private static void WriteIList(IList obj, Type objType, JaysonSerializationContext context)
		{
			var settings = context.Settings;

			context.ObjectDepth++;
			bool typeWritten = WriteListType (objType, context);

			StringBuilder builder = context.Builder;
			try {
				if (!typeWritten) {
					builder.Append ('[');
				} else {
					context.ObjectDepth++;
				}

				if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth)) {
					return;
				}

				int count = obj.Count;
				if (count > 0) {
					bool isFirst = true;
					for (int i = 0; i < count; i++) {
						isFirst = WriteEnumerableValue (obj [i], context, isFirst);
					}
				}
			} finally {
				context.ObjectDepth--;

				if (!settings.Formatting) {
					builder.Append (']');

					if (typeWritten) {
						context.ObjectDepth--;
						builder.Append ('}');
					}			
				} else {
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					builder.Append (']');

					if (typeWritten) {
						context.ObjectDepth--;
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
						builder.Append ('}');
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
			bool ignoreNullValues = context.Settings.IgnoreNullValues;

			Func<string, object, object> filter = context.Filter;
			bool canFilter = (filter != null);

			foreach (DictionaryEntry dEntry in source)
			{
				value = dEntry.Value;

				keyObj = dEntry.Key;
				key = (keyObj is string) ? (string)keyObj : keyObj.ToString();

				if ((value != null) && canFilter)
				{
					value = filter(key, value);
				}

				if ((value != null) || !ignoreNullValues)
				{
					yield return new JaysonKeyValue<string, object> { Key = key, Value = value };
				}
			}
		}

		private static void WriteCountedEnumerable(IEnumerable obj, Type objType, JaysonSerializationContext context, bool isEmpty)
		{
			var settings = context.Settings;

			context.ObjectDepth++;
			bool typeWritten = WriteListType (objType, context);

			StringBuilder builder = context.Builder;
			try {
				if (!typeWritten) {
					builder.Append ('[');
				} else {
					context.ObjectDepth++;
				}

				if (!ValidObjectDepth(context.ObjectDepth, settings.MaxObjectDepth, settings.RaiseErrorOnMaxObjectDepth)) {
					return;
				}

				if (!isEmpty) {
					bool isFirst = true;

					Type entryType;
					JaysonDictionaryType dType = JaysonCommon.GetDictionaryType (obj, out entryType);

					IEnumerator enumerator = obj.GetEnumerator ();
					try {
						if (dType == JaysonDictionaryType.IDictionary) {
							if (settings.OrderNames) {
								foreach (var kvp in GetEnumDictionaryEntries(obj, context).OrderBy(kvp => kvp.Key)) {
									isFirst = WriteKeyValueEntry (kvp.Key, kvp.Value, null, context, isFirst);
								}
							} else {
                                string key;
                                object value;
                                object keyObj;
                                DictionaryEntry dEntry;

                                Func<string, object, object> filter = context.Filter;
                                bool canFilter = (filter != null);

                                while (enumerator.MoveNext())
                                {
									dEntry = (DictionaryEntry)enumerator.Current;

									keyObj = dEntry.Key;
									value = dEntry.Value;

									key = (keyObj is string) ? (string)keyObj : keyObj.ToString ();

									if ((value != null) && canFilter) {
										value = filter (key, value);
									}

									isFirst = WriteKeyValueEntry (key, value, null, context, isFirst);
								}
							}
						} else if (dType == JaysonDictionaryType.IGenericDictionary) {
							IJaysonFastMember keyFm;
							IJaysonFastMember valueFm;

							IDictionary<string, IJaysonFastMember> members = JaysonFastMemberCache.GetMembers (entryType);

							if (members.TryGetValue ("Key", out keyFm) && members.TryGetValue ("Value", out valueFm)) {
                                string key;
                                object value;
                                object keyObj;

								Func<string, object, object> filter = context.Filter;
								bool canFilter = (filter != null);

								while (enumerator.MoveNext ()) {
									keyObj = keyFm.Get (enumerator.Current);
									if (keyObj != null) {
										key = (keyObj is string) ? (string)keyObj : keyObj.ToString ();
										value = valueFm.Get (enumerator.Current);

										if ((value != null) && canFilter) {
											value = filter (key, value);
										}

										context.CurrentType = valueFm.MemberType;
										isFirst = WriteKeyValueEntry (key, value, null, context, isFirst);
									}
								}
							}
						} else {
							while (enumerator.MoveNext ()) {
								isFirst = WriteEnumerableValue (enumerator.Current, context, isFirst);
							}
						}
					} finally {
						if (enumerator is IDisposable) {
							((IDisposable)enumerator).Dispose ();
						}
					}
				}
			} finally {
				context.ObjectDepth--;

				if (!settings.Formatting) {
					builder.Append (']');

					if (typeWritten) {
						context.ObjectDepth--;
						builder.Append ('}');
					}			
				} else {
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					builder.Append (']');

					if (typeWritten) {
						context.ObjectDepth--;
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
						builder.Append ('}');
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

			if (context.Stack.Contains(obj) || (obj == DBNull.Value))
			{
				return !context.Settings.IgnoreNullValues;
			}

			return true;
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
				JaysonTypeCode jtc = info.JTypeCode;

				if (jtc == JaysonTypeCode.String || jtc == JaysonTypeCode.Bool) {
					context.Formatter.Format(obj, objType, context.Builder);
					return;
				}

				JaysonTypeNameSerialization jtns = context.Settings.TypeNames;

                if (jtns != JaysonTypeNameSerialization.None &&
					(expectedObjType == null || objType != expectedObjType) &&
                    ((jtns == JaysonTypeNameSerialization.All && (JaysonTypeCode.JsonUnknown & jtc) == jtc) ||
                    	(jtns == JaysonTypeNameSerialization.Auto && (JaysonTypeCode.AutoTyped & jtc) == jtc) ||
                    	(jtns == JaysonTypeNameSerialization.AllButNoPrimitive && (JaysonTypeCode.Nullable & jtc) == jtc))) 
				{
					StringBuilder builder = context.Builder;

					bool typeWritten = false;
					context.ObjectDepth++;
					try {
						typeWritten = WritePrimitiveTypeName (objType, context);
						context.Formatter.Format (obj, objType, builder);
					} finally {
						context.ObjectDepth--;
						if (typeWritten) 
						{
							if (context.Settings.Formatting) 
							{
								builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
							}
							builder.Append ('}');
						}
					}
					return;
				}

				context.Formatter.Format(obj, objType, context.Builder);
				return;
			}

			JaysonStackList stack = null;
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
					if (context.Settings.RaiseErrorOnCircularRef) 
					{
						throw new JaysonException (JaysonError.CircularReferenceOn + objType.Name);
					}
					if (!context.Settings.IgnoreNullValues)
					{
						context.Builder.Append(JaysonConstants.Null);
					}
					return;
				}

				stack.Push(obj);
			}
			else if (info.Enum)
			{
				StringBuilder builder = context.Builder;

				builder.Append('"');
				builder.Append(context.Settings.UseEnumNames ? JaysonEnumCache.GetName((Enum)obj) : JaysonEnumCache.AsIntString((Enum)obj));
				builder.Append('"');
				return;
			}

			try
			{
				JaysonSerializationSettings settings = context.Settings;
				if (obj is IDictionary<string, object>)
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

				if (obj is IDictionary)
				{
                    var genericArgs = info.GenericArguments;
                    if (genericArgs != null && genericArgs.Length > 0) {
                        if (genericArgs[0] == typeof(string)) {
                            WriteDictionaryStringKey((IDictionary)obj, context);
                        } else {
                            WriteDictionaryObject((IDictionary)obj, genericArgs[0], genericArgs[1], context);
                        }
                    } else {
                        WriteDictionaryObject((IDictionary)obj, null, null, context);
                    }
					return;
				}

				#if !(NET3500 || NET3000 || NET2000)
				if (obj is DynamicObject)
				{
					if (!settings.IgnoreDynamicObjects) {
						WriteDynamicObject((DynamicObject)obj, context);
					} else if (!settings.IgnoreNullValues) {
						context.Builder.Append(JaysonConstants.Null);
					} else {
						context.Builder.Append('{');
						context.Builder.Append('}');
					}
					return;
				}
				#endif

				if (obj is DataTable)
				{
					WriteDataTable((DataTable)obj, context);
					return;
				}

				if (obj is DataSet)
				{
					WriteDataSet((DataSet)obj, context);
					return;
				}

				if (obj is StringDictionary)
				{
					WriteStringDictionary((StringDictionary)obj, context);
					return;
				}

				if (obj is NameValueCollection)
				{
					WriteNameValueCollection((NameValueCollection)obj, context);
					return;
				}

				if (obj is IEnumerable)
				{
					WriteEnumerable((IEnumerable)obj, objType, context);
					return;
				}

                if (settings.IgnoreAnonymousTypes && JaysonTypeInfo.IsAnonymous(objType))
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

				WriteClassOrStruct(obj, objType, context);
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
				bool formatting = context.Settings.Formatting;

				context.ObjectDepth++;
				StringBuilder builder = context.Builder;
				try {
					if (formatting) {
						if (!isFirst) {
							builder.Append (',');
						}
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
						builder.Append ("\"$types\": {");
					} else {
						if (isFirst) {
							builder.Append ("\"$types\":{");
						} else {
							builder.Append (",\"$types\":{");
						}
					}

					var globalTypes = context.GlobalTypes.GetList();
					if (globalTypes.Count > 0)
					{
						context.ObjectDepth++;
						try {
							JaysonKeyValue<string, Type> kvp;
							JaysonTypeNameInfo typeNameInfo = context.Settings.TypeNameInfo;

							int count = globalTypes.Count;
							for (int i = 0; i < count; i++) {
								kvp = globalTypes[i];

								if (formatting) {
									if (i > 0) {
										builder.Append (',');
									}
									builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
									builder.Append ('"');
									builder.Append (kvp.Key.ToString (JaysonConstants.InvariantCulture));
									builder.Append ("\": ");
								} else {
									if (i == 0) {
										builder.Append ('"');
									} else {
										builder.Append (",\"");
									}
									builder.Append (kvp.Key.ToString (JaysonConstants.InvariantCulture));
									builder.Append ("\":");
								}

								builder.Append ('"');
								builder.Append (JaysonTypeInfo.GetTypeName (kvp.Value, typeNameInfo));
								builder.Append ('"');
							}
						} finally {
							context.ObjectDepth--;
						}
					}
				} finally {
					if (formatting) {
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					} 
					builder.Append ('}');
					context.ObjectDepth--;
				}
			}
		}

		public static string ToJsonString(object obj, JaysonSerializationSettings settings = null,
			Func<string, object, object> filter = null)
		{
			if (obj == null)
			{
				return null;
			}

			settings = settings ?? JaysonSerializationSettings.Default;

			JaysonFormatter formatter = new JaysonFormatter(
				numberFormat: settings.NumberFormat,
				timeSpanFormat: settings.TimeSpanFormat,
				dateFormatType: settings.DateFormatType,
				dateTimeFormat: settings.DateTimeFormat,
				dateTimeZoneType: settings.DateTimeZoneType,
				dateTimeOffsetFormat: settings.DateTimeOffsetFormat,
				useEnumNames: settings.UseEnumNames,
				escapeChars: settings.EscapeChars,
				escapeUnicodeChars: settings.EscapeUnicodeChars,
				convertDecimalToDouble: settings.ConvertDecimalToDouble,
				guidAsByteArray: settings.GuidAsByteArray
			);

			Type objType = obj.GetType();
			if (JaysonTypeInfo.IsJPrimitive(objType))
			{
				if (settings.TypeNames == JaysonTypeNameSerialization.None) {
					return formatter.Format(obj, objType);
				}

				JaysonStackList primeStack = JaysonStackList.Get();
				try 
				{
					StringBuilder builder = new StringBuilder(100, int.MaxValue);

					var context = new JaysonSerializationContext (
						filter: filter,
						builder: builder,
						formatter: formatter,
						globalTypes: settings.UseGlobalTypeNames ? new JaysonSerializationTypeList () : null,
						settings: settings,
						stack: primeStack
					);

					if (settings.UseGlobalTypeNames) {
						context.ObjectDepth++;

						if (settings.Formatting) {
							builder.Append ('{');
							builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
							builder.Append ("\"$value\": ");
						} else {
							builder.Append ("{\"$value\":");
						}
					}

					context.ObjectDepth++;
					WritePrimitiveTypeName (objType, context);
					formatter.Format (obj, objType, builder);

					context.ObjectDepth--;
					if (settings.Formatting) {
						builder.Append (JaysonConstants.Indentation [0]);
					}
					builder.Append ('}');

					if (settings.UseGlobalTypeNames) {
						context.ObjectDepth--;

						WriteGlobalTypes (context, false);

						if (settings.Formatting) {
							builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
						}
						builder.Append ('}');
					}

					return builder.ToString();
				}
				finally
				{
					JaysonStackList.Release(primeStack);
				}
			}

			JaysonStackList stack = JaysonStackList.Get();
			try
			{
				StringBuilder builder = new StringBuilder(2048, int.MaxValue);

				var context = new JaysonSerializationContext(
					filter: filter,
					builder: builder,
					formatter: formatter,
					globalTypes: settings.UseGlobalTypeNames ? new JaysonSerializationTypeList () : null,
					settings: settings,
					stack: stack
				);

				if (settings.UseGlobalTypeNames) {
					context.ObjectDepth++;

					if (settings.Formatting) {
						builder.Append ('{');
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
						builder.Append ("\"$value\": ");
					} else {
						builder.Append ("{\"$value\":");
					}
				}

				WriteJsonObject(obj, objType, null, context);

				if (settings.UseGlobalTypeNames) {
					context.ObjectDepth--;

					WriteGlobalTypes (context, false);

					if (settings.Formatting) {
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					}
					builder.Append ('}');
				}

				return builder.ToString();
			}
			finally
			{
				JaysonStackList.Release(stack);
			}
		}

		# endregion ToJsonString
	}

	# endregion JsonConverter
}
