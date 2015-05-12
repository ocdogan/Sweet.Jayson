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
				builder.Append ("\"$type\": \"");
			}
			else {
				builder.Append ("{\"$type\":\"");
			}
            builder.Append(JaysonTypeInfo.GetTypeName(objType, context.Settings.TypeNameInfo));
			builder.Append ('"');
		}

		private static void WriteListTypeName (Type objType, JaysonSerializationContext context)
		{
			StringBuilder builder = context.Builder;
			if (context.Settings.Formatting) {
				builder.Append ('{');
				builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				builder.Append ("\"$type\": \"");
                builder.Append(JaysonTypeInfo.GetTypeName(objType, context.Settings.TypeNameInfo));
				builder.Append ('"');
				builder.Append (',');
				builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				builder.Append ("\"$values\": [");
			}
			else {
				builder.Append ("{\"$type\": \"");
                builder.Append(JaysonTypeInfo.GetTypeName(objType, context.Settings.TypeNameInfo));
				builder.Append ("\",\"$values\":[");
			}
		}

		private static void WritePrimitiveTypeName (Type objType, JaysonSerializationContext context)
		{
			StringBuilder builder = context.Builder;
			if (context.Settings.Formatting) {
				builder.Append ('{');
				builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				builder.Append ("\"$type\": \"");
				builder.Append(JaysonTypeInfo.GetTypeName(objType, context.Settings.TypeNameInfo));
				builder.Append ('"');
				builder.Append (',');
				builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				builder.Append ("\"$value\": ");
			}
			else {
				builder.Append ("{\"$type\": \"");
				builder.Append(JaysonTypeInfo.GetTypeName(objType, context.Settings.TypeNameInfo));
				builder.Append ("\",\"$value\":");
			}
		}

		private static bool WriteObjectType(object obj, JaysonSerializationContext context)
		{
			if (obj != null) {
				switch (context.Settings.TypeNames) {
				case JaysonTypeNameSerialization.None:
					break;
				case JaysonTypeNameSerialization.Auto: 
					{
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
						WriteObjectTypeName (obj.GetType (), context);
						return true;
					}
				case JaysonTypeNameSerialization.Objects: 
				case JaysonTypeNameSerialization.AllButNoPrimitive:
					{
						JaysonTypeInfo info = JaysonTypeInfo.GetTypeInfo (obj.GetType ());
						if (!info.JPrimitive) {
							WriteObjectTypeName (info.Type, context);
							return true;
						}
					}
					break;
				}
			}
			return false;
		}

		private static bool WriteObjectType(Type objType, JaysonSerializationContext context)
		{
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
					WriteObjectTypeName (objType, context);
					return true;
				}
			case JaysonTypeNameSerialization.Objects: 
			case JaysonTypeNameSerialization.AllButNoPrimitive:
				{
					JaysonTypeInfo info = JaysonTypeInfo.GetTypeInfo (objType);
					if (!info.JPrimitive) {
						WriteObjectTypeName (objType, context);
						return true;
					}
				}
				break;
			}
			return false;
		}

		private static bool WriteListType(object obj, JaysonSerializationContext context)
		{
			switch (context.Settings.TypeNames) {
			case JaysonTypeNameSerialization.None:
				break;
			case JaysonTypeNameSerialization.Auto: 
				{
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
					WriteListTypeName (obj.GetType (), context);
					return true;
				}
			default:
				break;
			}
			return false;
		}

		private static bool WriteListType(Type objType, JaysonSerializationContext context)
		{
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
					WriteListTypeName (objType, context);
					return true;
				}
			default:
				break;
			}
			return false;
		}

		private static bool WriteByteArrayType(JaysonSerializationContext context)
		{
			switch (context.Settings.TypeNames) {
			case JaysonTypeNameSerialization.None:
				break;
			case JaysonTypeNameSerialization.Auto: 
				{
					if (context.CurrentType != typeof(byte[])) {
						WritePrimitiveTypeName (typeof(byte[]), context);
						return true;
					}
				}
				break;
			case JaysonTypeNameSerialization.All: 
			case JaysonTypeNameSerialization.Arrays:
			case JaysonTypeNameSerialization.AllButNoPrimitive:
				{
					WritePrimitiveTypeName (typeof(byte[]), context);
					return true;
				}
			default:
				break;
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
                    throw new JaysonException(String.Format("Maximum object depth {0} exceeded.",
                        context.Settings.MaxObjectDepth));
                }
                return false;
            }
            return true;
        }

		private static bool WriteKeyValueEntry(string key, object value, JaysonSerializationContext context, 
			bool isFirst, bool ignoreEscape = false)
		{
			if ((value != null) && (value != DBNull.Value)) {
				StringBuilder builder = context.Builder;
				JaysonSerializationSettings settings = context.Settings;

				if (!isFirst) {
					builder.Append (',');
				}
				if (settings.Formatting) {
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				}

				builder.Append ('"');

				if (ignoreEscape || !(settings.EscapeChars || settings.EscapeUnicodeChars)) {
					builder.Append (settings.CaseSensitive ? key : key.ToLower(JaysonConstants.InvariantCulture));
				} else {
					JaysonFormatter.EncodeUnicodeString (settings.CaseSensitive ? key : 
						key.ToLower(JaysonConstants.InvariantCulture), builder, settings.EscapeUnicodeChars);
				}

				if (settings.Formatting) {
					builder.Append ("\": ");
				} else {
					builder.Append ('"');
					builder.Append (':');
				}

				var valueType = value.GetType ();
				var jtc = JaysonTypeInfo.GetJTypeCode(valueType);

				if (jtc == JaysonTypeCode.String || jtc == JaysonTypeCode.Bool) {
					context.Formatter.Format (value, valueType, context.Builder);
					return false; // isFirst
				} 

				if (jtc == JaysonTypeCode.Object) {
					WriteJsonObject (value, valueType, context);
					return false; // isFirst
				} 

				JaysonTypeNameSerialization jtns = context.Settings.TypeNames;

				if (jtns == JaysonTypeNameSerialization.All ||
					((jtns == JaysonTypeNameSerialization.Auto ||
						jtns == JaysonTypeNameSerialization.AllButNoPrimitive) &&
						(jtc == JaysonTypeCode.Nullable || 
							((JaysonTypeCode.Primitive & jtc) == jtc &&
								(JaysonTypeCode.Number & jtc) != jtc)))) {
					context.ObjectDepth++;
					try {
						WritePrimitiveTypeName (valueType, context);
						context.Formatter.Format (value, valueType, builder);
					} finally {
						context.ObjectDepth--;
						if (context.Settings.Formatting) {
							builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
						}
						builder.Append ('}');
					}
					return false; // isFirst
				} 

				context.Formatter.Format(value, valueType, context.Builder);
				return false; // isFirst
			} else if (!context.Settings.IgnoreNullValues) {
				StringBuilder builder = context.Builder;
				JaysonSerializationSettings settings = context.Settings;

				if (!isFirst) {
					builder.Append (',');
				}
				if (context.Settings.Formatting) {
					builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
				}

				builder.Append ('"');

				if (ignoreEscape || !(settings.EscapeChars || settings.EscapeUnicodeChars)) {
					builder.Append (settings.CaseSensitive ? key : key.ToLower(JaysonConstants.InvariantCulture));
				} else {
					JaysonFormatter.EncodeUnicodeString (settings.CaseSensitive ? key : 
						key.ToLower(JaysonConstants.InvariantCulture), builder, settings.EscapeUnicodeChars);
				}

				if (settings.Formatting) {
					builder.Append ("\": null");
				} else {
					builder.Append ("\":null");
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

					WriteJsonObject(value, valueType, context);
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

				context.Builder.Append("null");
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
                StringBuilder builder = context.Builder;
                JaysonSerializationSettings settings = context.Settings;
                bool formatting = settings.Formatting;

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
                    if (!ValidObjectDepth(context))
                    {
                        return;
                    }

					DataColumn dataColumn;
                    DataColumnCollection columns = dataTable.Columns;

                    int colCount = columns.Count;
                    List<Tuple<DataColumn, JaysonTypeInfo>> columnsInfo = new List<Tuple<DataColumn, JaysonTypeInfo>>();

                    for (int i = 0; i < colCount; i++)
                    {
						dataColumn = columns[i];
                        columnsInfo.Add(new Tuple<DataColumn, JaysonTypeInfo>(dataColumn, JaysonTypeInfo.GetTypeInfo(dataColumn.DataType)));
                    }

					DataRow dataRow;
					object cellValue;
                    int rowCount = rows.Count;

                    JaysonTypeCode colTypeCode;
					Tuple<DataColumn, JaysonTypeInfo> columnInfo;

                    JaysonFormatter formatter = context.Formatter;
                    bool escapeChars = context.Settings.EscapeChars;
                    bool escapeUnicodeChars = context.Settings.EscapeUnicodeChars;

                    for (int i = 0; i < rowCount; i++)
                    {
                        if (i > 0)
                        {
                            builder.Append(',');
                        }
                        if (formatting)
                        {
                            builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
                        }
                        builder.Append('[');

                        context.ObjectDepth++;
                        try
                        {
                            if (!ValidObjectDepth(context))
                            {
                                return;
                            }

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
                                    builder.Append("null");
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
                                                context.CurrentType = columnInfo.Item2.Type;
                                                try
                                                {
                                                    WriteJsonObject(cellValue, cellValue.GetType(), context);
                                                }
                                                finally
                                                {
                                                    context.CurrentType = null;
                                                }
                                            }
                                            break;
                                    }
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
				return false;
			}

			StringBuilder builder = context.Builder;
			JaysonFormatter formatter = context.Formatter;
			bool formatting = context.Settings.Formatting;

			if (!isFirst) {
				builder.Append (',');
			}
			if (formatting) {
				builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
			}
			if (!context.Settings.CaseSensitive) {
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
				if (!ValidObjectDepth (context)) {
					return true;
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
						builder.Append ("null");
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
			return true;
		}

        private static void WriteDataColumns(DataTable dataTable, JaysonSerializationContext context)
        {
            var columns = dataTable.Columns;
            int columnCount = columns.Count;

            if (columnCount > 0)
            {
                StringBuilder builder = context.Builder;
                JaysonSerializationSettings settings = context.Settings;
                bool formatting = settings.Formatting;

                if (formatting)
                {
                    builder.Append(',');
                    builder.Append(JaysonConstants.Indentation[context.ObjectDepth]);
                    if (!settings.CaseSensitive)
                    {
                        builder.Append("\"columns\": [");
                    }
                    else
                    {
                        builder.Append("\"Columns\": [");
                    }
                }
                else
                {
                    if (!settings.CaseSensitive)
                    {
                        builder.Append(",\"columns\":[");
                    }
                    else
                    {
                        builder.Append(",\"Columns\":[");
                    }
                }

                context.ObjectDepth++;
                try
                {
                    if (!ValidObjectDepth(context))
                    {
                        return;
                    }

                    bool isFirst;
                    DataColumn column;
                    string defaultNamespace = dataTable.Namespace;

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
                            if (!ValidObjectDepth(context))
                            {
                                return;
                            }

                            column = columns[i];

                            isFirst = true;
                            if (!column.AllowDBNull)
                            {
								isFirst = WriteKeyValueEntry("AllowDBNull", column.AllowDBNull, context, isFirst);
                            }
                            if (column.AutoIncrement)
                            {
								isFirst = WriteKeyValueEntry("AutoIncrement", column.AutoIncrement, context, isFirst);
                            }
                            if (column.AutoIncrementSeed != 0)
                            {
								isFirst = WriteKeyValueEntry("AutoIncrementSeed", column.AutoIncrementSeed, context, isFirst);
                            }
                            if (column.AutoIncrementStep != 1)
                            {
								isFirst = WriteKeyValueEntry("AutoIncrementStep", column.AutoIncrementStep, context, isFirst);
                            }
                            if (!String.IsNullOrEmpty(column.Caption) && column.Caption != column.ColumnName)
                            {
								isFirst = WriteKeyValueEntry("Caption", column.Caption, context, isFirst);
                            }
                            if (column.ColumnMapping != MappingType.Element)
                            {
								isFirst = WriteKeyValueEntry("ColumnMapping", column.ColumnMapping, context, isFirst);
                            }
                            if (!String.IsNullOrEmpty(column.ColumnName))
                            {
								isFirst = WriteKeyValueEntry("ColumnName", column.ColumnName, context, isFirst);
                            }
                            isFirst = WriteKeyValueEntry("DataType", JaysonTypeInfo.GetTypeName(column.DataType, JaysonTypeNameInfo.TypeNameWithAssembly), context, isFirst);
                            if (!String.IsNullOrEmpty(column.Expression))
                            {
                                isFirst = WriteKeyValueEntry("Expression", column.Expression, context, isFirst);
                            }
                            if (column.MaxLength != -1)
                            {
								isFirst = WriteKeyValueEntry("MaxLength", column.MaxLength, context, isFirst);
                            }
                            if (!String.IsNullOrEmpty(column.Namespace) && column.Namespace != defaultNamespace)
                            {
								isFirst = WriteKeyValueEntry("Namespace", column.Namespace, context, isFirst);
                            }
							isFirst = WriteKeyValueEntry("Ordinal", column.Ordinal, context, isFirst);
                            if (!String.IsNullOrEmpty(column.Prefix))
                            {
								isFirst = WriteKeyValueEntry("Prefix", column.Prefix, context, isFirst);
                            }
                            if (column.ReadOnly)
                            {
								isFirst = WriteKeyValueEntry("ReadOnly", column.ReadOnly, context, isFirst);
                            }
                            if (column.Unique)
                            {
								isFirst = WriteKeyValueEntry("Unique", column.Unique, context, isFirst);
                            }

                            if (column.ExtendedProperties.Count > 0)
                            {
                                context.CurrentType = typeof(PropertyCollection);
                                try
                                {
                                    isFirst = WriteKeyValueEntry("ExtendedProperties", column.ExtendedProperties, context, isFirst);
                                }
                                finally
                                {
                                    context.CurrentType = null;
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
            context.ObjectDepth++;
            StringBuilder builder = context.Builder;
            bool formatting = context.Settings.Formatting;
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

                if (!ValidObjectDepth(context))
                {
                    return;
                }

                isFirst = WriteKeyValueEntry("$datatype", "DataTable", context, isFirst);
                if (dataTable.CaseSensitive)
                {
					isFirst = WriteKeyValueEntry("CaseSensitive", dataTable.CaseSensitive, context, isFirst);
                }
                if (!String.IsNullOrEmpty(dataTable.DisplayExpression))
                {
					isFirst = WriteKeyValueEntry("DisplayExpression", dataTable.DisplayExpression, context, isFirst);
                }
                if (dataTable.Locale != CultureInfo.InvariantCulture)
                {
					isFirst = WriteKeyValueEntry("Locale", dataTable.Locale.Name, context, isFirst);
                }
                if (!String.IsNullOrEmpty(dataTable.Namespace))
                {
					isFirst = WriteKeyValueEntry("Namespace", dataTable.Namespace, context, isFirst);
                }
                if (!String.IsNullOrEmpty(dataTable.Prefix))
                {
					isFirst = WriteKeyValueEntry("Prefix", dataTable.Prefix, context, isFirst);
                }

				var columns = dataTable.PrimaryKey;
				if (columns != null && columns.Length > 0) 
				{
					isFirst = !WriteDataColumnNames ("PrimaryKey", columns, context, isFirst);
				}

                if (!String.IsNullOrEmpty(dataTable.TableName))
                {
					isFirst = WriteKeyValueEntry("TableName", dataTable.TableName, context, isFirst);
                }

                if (dataTable.ExtendedProperties.Count > 0)
                {
                    context.CurrentType = typeof(PropertyCollection);
                    try
                    {
                        isFirst = WriteKeyValueEntry("ExtendedProperties", dataTable.ExtendedProperties, context, isFirst);
                    }
                    finally
                    {
                        context.CurrentType = null;
                    }
                }

                WriteDataColumns(dataTable, context);
                WriteDataRows(dataTable, context);
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

        private static bool WriteDataRelations(DataSet dataSet, JaysonSerializationContext context, bool isFirst)
        {
            var relations = dataSet.Relations;
            if (relations.Count == 0)
            {
                return false;
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
                    builder.Append("\"relations\": [");
                }
                else
                {
                    builder.Append("\"Relations\": [");
                }
            }
            else
            {
                if (!settings.CaseSensitive)
                {
                    builder.Append("\"relations\":[");
                }
                else
                {
                    builder.Append("\"Relations\":[");
                }
            }

            context.ObjectDepth++;
            try
            {
                if (!ValidObjectDepth(context))
                {
                    return true;
                }

                DataRelation relation;
                string defaultNamespace = dataSet.Namespace;

                for (int i = 0; i < relationCount; i++)
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
                        if (!ValidObjectDepth(context))
                        {
                            return true;
                        }

                        relation = relations[i];

                        isFirst = true;
                        if (!String.IsNullOrEmpty(relation.RelationName))
                        {
                            isFirst = WriteKeyValueEntry("RelationName", relation.RelationName, context, isFirst);
                        }

                        isFirst = !WriteDataColumnNames("ChildColumns", relation.ChildColumns, context, isFirst);

                        if (relation.ChildTable != null)
                        {
                            isFirst = WriteKeyValueEntry("ChildTable", relation.ChildTable.TableName, context, isFirst);
                            if (!String.IsNullOrEmpty(relation.ChildTable.Namespace))
                            {
                                isFirst = WriteKeyValueEntry("ChildTableNamespace", relation.ChildTable.Namespace, context, isFirst);
                            }
                        }

                        if (relation.ExtendedProperties.Count > 0)
                        {
                            context.CurrentType = typeof(PropertyCollection);
                            try
                            {
                                isFirst = WriteKeyValueEntry("ExtendedProperties", relation.ExtendedProperties, context, isFirst);
                            }
                            finally
                            {
                                context.CurrentType = null;
                            }
                        }

                        if (relation.Nested)
                        {
                            isFirst = WriteKeyValueEntry("Nested", relation.Nested, context, isFirst);
                        }

                        isFirst = !WriteDataColumnNames("ParentColumns", relation.ParentColumns, context, isFirst);

                        if (relation.ParentTable != null)
                        {
                            isFirst = WriteKeyValueEntry("ParentTable", relation.ParentTable.TableName, context, isFirst);
                            if (!String.IsNullOrEmpty(relation.ParentTable.Namespace))
                            {
                                isFirst = WriteKeyValueEntry("ParentTableNamespace", relation.ParentTable.Namespace, context, isFirst);
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

            return true;
        }

		private static bool WriteDataTables(DataSet dataSet, JaysonSerializationContext context, bool isFirst)
		{
			var tables = dataSet.Tables;
			if (tables.Count == 0) 
			{
				return false;
			}

			int tableCount = tables.Count;

			StringBuilder builder = context.Builder;
			JaysonSerializationSettings settings = context.Settings;
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
					builder.Append ("\"tables\": [");
				} 
				else 
				{
					builder.Append ("\"Tables\": [");
				}
			} 
			else 
			{
				if (!settings.CaseSensitive) 
				{
					builder.Append ("\"tables\":[");
				} 
				else 
				{
					builder.Append ("\"Tables\":[");
				}
			}

			context.ObjectDepth++;
			try 
			{
				if (!ValidObjectDepth (context)) 
				{
					return true;
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

			return true;
		}

		private static void WriteDataSet(DataSet dataSet, JaysonSerializationContext context)
        {
            context.ObjectDepth++;
            StringBuilder builder = context.Builder;
            bool formatting = context.Settings.Formatting;
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

                if (!ValidObjectDepth(context))
                {
                    return;
                }

                isFirst = WriteKeyValueEntry("$datatype", "DataSet", context, isFirst);
                if (dataSet.CaseSensitive)
                {
                    isFirst = WriteKeyValueEntry("CaseSensitive", dataSet.CaseSensitive, context, isFirst);
                }
                if (!String.IsNullOrEmpty(dataSet.DataSetName))
                {
					isFirst = WriteKeyValueEntry("DataSetName", dataSet.DataSetName, context, isFirst);
                }
                if (!dataSet.EnforceConstraints)
                {
					isFirst = WriteKeyValueEntry("EnforceConstraints", dataSet.EnforceConstraints, context, isFirst);
                }
                if (dataSet.Locale != CultureInfo.InvariantCulture)
                {
					isFirst = WriteKeyValueEntry("Locale", dataSet.Locale.Name, context, isFirst);
                }
                if (!String.IsNullOrEmpty(dataSet.Namespace))
                {
					isFirst = WriteKeyValueEntry("Namespace", dataSet.Namespace, context, isFirst);
                }
                if (!String.IsNullOrEmpty(dataSet.Prefix))
                {
					isFirst = WriteKeyValueEntry("Prefix", dataSet.Prefix, context, isFirst);
                }
                if (dataSet.SchemaSerializationMode != SchemaSerializationMode.IncludeSchema)
                {
					isFirst = WriteKeyValueEntry("SchemaSerializationMode", dataSet.SchemaSerializationMode, context, isFirst);
                }
                if (dataSet.ExtendedProperties.Count > 0)
                {
                    context.CurrentType = typeof(PropertyCollection);
                    try
                    {
                        isFirst = WriteKeyValueEntry("ExtendedProperties", dataSet.ExtendedProperties, context, isFirst);
                    }
                    finally
                    {
                        context.CurrentType = null;
                    }
                }

				isFirst = !WriteDataRelations(dataSet, context, isFirst);
				isFirst = !WriteDataTables(dataSet, context, isFirst);
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

		private static IEnumerable<JaysonKeyValue<string, object>> GetDictionaryEntries(IDictionary source, 
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

        private static void WriteDictionary(IDictionary obj, JaysonSerializationContext context)
		{
			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			try {
				bool isFirst = true;
				if (WriteObjectType (obj, context)) {
					isFirst = false;
				} else {
					builder.Append ('{');
				}

				if (!ValidObjectDepth(context)) {
					return;
				}

				if (obj.Count > 0) {
					string key;
					object value;
					object keyObj;

					Func<string, object, object> filter = context.Filter;
					bool canFilter = (filter != null);

					if (context.Settings.OrderNames) {
						foreach (var kvp in GetDictionaryEntries(obj, context).OrderBy(kvp => kvp.Key)) {
							key = kvp.Key;
							value = kvp.Value;

							if ((value != null) && canFilter) {
								value = filter (key, value);
							}

							isFirst = WriteKeyValueEntry (kvp.Key, kvp.Value, context, isFirst);
						}
					} else {
						foreach (DictionaryEntry dEntry in obj) {
							keyObj = dEntry.Key;
							value = dEntry.Value;

							key = (keyObj is string) ? (string)keyObj : keyObj.ToString ();

							if ((value != null) && canFilter) {
								value = filter (key, value);
							}

							isFirst = WriteKeyValueEntry (key, value, context, isFirst);
						}
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
			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			try {
				bool isFirst = true;
				if (WriteObjectType (obj, context)) {
					isFirst = false;
				} else {
					builder.Append ('{');
				}

				if (!ValidObjectDepth(context)) {
					return;
				}

				if (obj.Count > 0) {
					string key;
					object value;
					object keyObj;

					Func<string, object, object> filter = context.Filter;
					bool canFilter = (filter != null);

					if (context.Settings.OrderNames) {
						foreach (var kvp in GetStringDictionaryEntries(obj, context).OrderBy(kvp => kvp.Key)) {
							key = kvp.Key;
							value = kvp.Value;

							if ((value != null) && canFilter) {
								value = filter (key, value);
							}

							isFirst = WriteKeyValueEntry (kvp.Key, kvp.Value, context, isFirst);
						}
					} else {
						foreach (DictionaryEntry dEntry in obj) {
							keyObj = dEntry.Key;
							value = dEntry.Value;

							key = (keyObj is string) ? (string)keyObj : keyObj.ToString ();

							if ((value != null) && canFilter) {
								value = filter (key, value);
							}

							isFirst = WriteKeyValueEntry (key, value, context, isFirst);
						}
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

		private static void WriteNameValueCollection(NameValueCollection obj, JaysonSerializationContext context)
		{
			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			try {
				bool isFirst = true;
				if (WriteObjectType (obj, context)) {
					isFirst = false;
				} else {
					builder.Append ('{');
				}

				if (!ValidObjectDepth(context)) {
					return;
				}

				if (obj.Count > 0) {
					string[] keys = obj.AllKeys;
					if (context.Settings.OrderNames) {
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

						isFirst = WriteKeyValueEntry (key, value, context, isFirst);
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

		#if !(NET3500 || NET3000 || NET2000)
		private static void WriteDynamicObject(DynamicObject obj, JaysonSerializationContext context)
		{
			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			try {
				bool isFirst = true;
				Type objType = obj.GetType ();
				if (WriteObjectType (objType, context)) {
					isFirst = false;
				} else {
					builder.Append ('{');
				}

				if (!ValidObjectDepth(context)) {
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

					Type currentType = context.CurrentType;
					try {
						string fKey;
						foreach (var memberKvp in members)
						{
							fKey = memberKvp.Key;
							value = memberKvp.Value.Get(obj);

							if ((value != null) && canFilter)
							{
								value = filter(fKey, value);
							}

							context.CurrentType = memberKvp.Value.MemberType;
							isFirst = WriteKeyValueEntry(memberKvp.Key, value, context, isFirst, true);
						}
					} finally {
						context.CurrentType = currentType;
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

					isFirst = WriteKeyValueEntry(dKey, value, context, isFirst, true);
				}
			}
			finally
			{
				context.ObjectDepth--;
				if (context.Settings.Formatting) {
					builder.Append (JaysonConstants.Indentation[context.ObjectDepth]);
				} 
				builder.Append('}');
			}
		}
		#endif

		private static void WriteClassOrStruct(object obj, Type objType, JaysonSerializationContext context)
		{
			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			try {
				bool isFirst = true;
				if (WriteObjectType (objType, context)) {
					isFirst = false;
				} else {
					builder.Append ('{');
				}

				if (!ValidObjectDepth(context)) {
					return;
				}

				var fastDict = JaysonFastMemberCache.GetMembers (objType);
				if (fastDict.Count > 0) {
					string key;
					object value;

					Func<string, object, object> filter = context.Filter;
					bool canFilter = (filter != null);

					IEnumerable<KeyValuePair<string, IJaysonFastMember>> members = fastDict;
					if (context.Settings.OrderNames) {
						members = members.OrderBy (kvp => kvp.Key);
					}

					Type currentType = context.CurrentType;
					try {
						foreach (var memberKvp in members) {
							key = memberKvp.Key;
							value = memberKvp.Value.Get (obj);

							if ((value != null) && canFilter) {
								value = filter (key, value);
							}

							context.CurrentType = memberKvp.Value.MemberType;
							isFirst = WriteKeyValueEntry (memberKvp.Key, value, context, isFirst, true);
						}
					} finally {
						context.CurrentType = currentType;
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

		private static void WriteGenericStringDictionary(IDictionary<string, object> obj, JaysonSerializationContext context)
		{
			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			try {
				bool isFirst = true;
				if (WriteObjectType (obj, context)) {
					isFirst = false;
				} else {
					builder.Append ('{');
				}

				if (!ValidObjectDepth(context)) {
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
					if (context.Settings.OrderNames) {
						eDict = eDict.OrderBy (kvp => kvp.Key);
					}

					foreach (var kvp in obj) {
						key = kvp.Key;
						value = kvp.Value;

						if ((value != null) && canFilter) {
							value = filter (key, value);
						}

						isFirst = WriteKeyValueEntry (key, value, context, isFirst, isExpando);
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

		private static void WriteByteArray (byte[] obj, JaysonSerializationContext context)
		{
			context.ObjectDepth++;
			bool typeWritten = WriteByteArrayType (context);

			StringBuilder builder = context.Builder;
			bool formatting = context.Settings.Formatting;
			try {
				if (typeWritten) {
					context.ObjectDepth++;
					if (formatting) {
						builder.Append (JaysonConstants.Indentation[context.ObjectDepth]);
					}
				}

				if (!ValidObjectDepth(context)) {
					builder.Append("\"\"");
					return;
				}

				builder.Append ('"');
				builder.Append (Convert.ToBase64String (obj, 0, obj.Length, Base64FormattingOptions.None));
				builder.Append ('"');
			}
			finally {
				context.ObjectDepth--;
				if (typeWritten) {
					context.ObjectDepth--;
					if (formatting) {
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					}
					builder.Append ('}');
				}
			}
		}

		static void WriteDBNullArray (JaysonSerializationContext context, int length)
		{
			context.ObjectDepth++;
			bool typeWritten = WriteListType (typeof(DBNull[]), context);

			StringBuilder builder = context.Builder;
			bool formatting = context.Settings.Formatting;
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
						builder.Append ("null");
					} else if (formatting) {
						builder.Append (',');
						builder.Append (JaysonConstants.Indentation[objectDepth]);
						builder.Append ("null");
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
			context.ObjectDepth++;
			bool typeWritten = WriteListType (obj, context);

			StringBuilder builder = context.Builder;
			bool formatting = context.Settings.Formatting;
			try {
				if (!typeWritten) {
					builder.Append ('[');
				} else {
					context.ObjectDepth++;
				}

				if (!ValidObjectDepth(context)) {
					return;
				}

				int objectDepth = context.ObjectDepth;
				JaysonFormatter formatter = context.Formatter;
				bool ignoreNullValues = context.Settings.IgnoreNullValues;

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
								builder.Append ("null");
							} else {
								if (formatting) {
									builder.Append (',');
									builder.Append (JaysonConstants.Indentation[objectDepth]);
									builder.Append ("null");
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

				if (!context.Settings.Formatting) {
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

		private static void WriteArray(Array obj, Type objType, JaysonSerializationContext context)
		{
			int length = obj.Length;
			if (length == 0) {
				if (!WriteListType (obj, context)) {
					context.Builder.Append ('[');
				}
				context.Builder.Append (']');
				return;
			}

            Type arrayType = JaysonTypeInfo.GetElementType(objType);

			if (arrayType == typeof(byte) && length > 1) {
				WriteByteArray ((byte[])obj, context);
				return;
			}

			var info = JaysonTypeInfo.GetTypeInfo(arrayType);

			if (info.JPrimitive || info.Enum) {
				WritePrimitiveArray (obj, arrayType, context);
				return;
			}

			if (arrayType == typeof(DBNull)) {
				WriteDBNullArray (context, length);
				return;
			}

			WriteIList (obj, context);
		}

		private static void WriteIList(IList obj, JaysonSerializationContext context)
		{
			context.ObjectDepth++;
			bool typeWritten = WriteListType (obj, context);

			int count = obj.Count;
			if (count == 0) {
				if (!typeWritten) {
					context.Builder.Append ('[');
				}
				context.Builder.Append (']');
				return;
			}

			StringBuilder builder = context.Builder;
			try {
				if (!typeWritten) {
					builder.Append ('[');
				} else {
					context.ObjectDepth++;
				}

				if (!ValidObjectDepth(context)) {
					return;
				}

				bool isFirst = true;
				for (int i = 0; i < count; i++) {
					isFirst = WriteEnumerableValue (obj [i], context, isFirst);
				}
			} finally {
				context.ObjectDepth--;

				if (!context.Settings.Formatting) {
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

		private static void WriteCountedEnumerable(IEnumerable obj, JaysonSerializationContext context, bool isEmpty)
		{
			context.ObjectDepth++;
			StringBuilder builder = context.Builder;
			bool typeWritten = WriteListType (obj, context);
			if (!typeWritten) {
				builder.Append ('[');
			}

			if (!isEmpty) {
				try {
					if (!ValidObjectDepth(context)) {
						return;
					}

					bool isFirst = true;

					Type entryType;
					JaysonDictionaryType dType = JaysonCommon.GetDictionaryType (obj, out entryType);

					IEnumerator enumerator = obj.GetEnumerator ();
					try {
						if (dType == JaysonDictionaryType.IDictionary) {
							string key;
							object value;
							object keyObj;
							DictionaryEntry dEntry;

							Func<string, object, object> filter = context.Filter;
							bool canFilter = (filter != null);

							if (context.Settings.OrderNames) {
								foreach (var kvp in GetEnumDictionaryEntries(obj, context).OrderBy(kvp => kvp.Key)) {
									key = kvp.Key;
									value = kvp.Value;

									if ((value != null) && canFilter) {
										value = filter (key, value);
									}

									isFirst = WriteKeyValueEntry (kvp.Key, kvp.Value, context, isFirst);
								}
							} else {
								while (enumerator.MoveNext ()) {
									dEntry = (DictionaryEntry)enumerator.Current;

									keyObj = dEntry.Key;
									value = dEntry.Value;

									key = (keyObj is string) ? (string)keyObj : keyObj.ToString ();

									if ((value != null) && canFilter) {
										value = filter (key, value);
									}

									isFirst = WriteKeyValueEntry (key, value, context, isFirst);
								}
							}
						} else if (dType == JaysonDictionaryType.IGenericDictionary) {
							string key;
							object value;

							IJaysonFastMember keyFm;
							IJaysonFastMember valueFm;

							IDictionary<string, IJaysonFastMember> members = JaysonFastMemberCache.GetMembers (entryType);

							if (members.TryGetValue ("Key", out keyFm) && members.TryGetValue ("Value", out valueFm)) {
								object keyObj;

								Func<string, object, object> filter = context.Filter;
								bool canFilter = (filter != null);

								Type currentType = context.CurrentType;
								try {
									while (enumerator.MoveNext ()) {
										keyObj = keyFm.Get (enumerator.Current);
										if (keyObj != null) {
											key = (keyObj is string) ? (string)keyObj : keyObj.ToString ();
											value = valueFm.Get (enumerator.Current);

											if ((value != null) && canFilter) {
												value = filter (key, value);
											}

											context.CurrentType = valueFm.MemberType;
											isFirst = WriteKeyValueEntry (key, value, context, isFirst);
										}
									}
								} finally {
									context.CurrentType = currentType;
								}
							}
						} else {
							context.ObjectDepth++;
							while (enumerator.MoveNext ()) {
								isFirst = WriteEnumerableValue (enumerator.Current, context, isFirst);
							}
							context.ObjectDepth--;
						}
					} finally {
						if (enumerator is IDisposable) {
							((IDisposable)enumerator).Dispose ();
						}
					}
				} finally {
					if (context.Settings.Formatting) {
						builder.Append (JaysonConstants.Indentation[context.ObjectDepth]);
					} 
					builder.Append (']');

					context.ObjectDepth--;
					if (context.Settings.Formatting) {
						builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
					} 
					builder.Append ('}');
				}
			}
		}

		private static void WriteICollection(ICollection obj, JaysonSerializationContext context)
		{
			WriteCountedEnumerable(obj, context, obj.Count > 0);
		}

		private static void WriteEnumerable(IEnumerable obj, Type objType, JaysonSerializationContext context)
		{
			if (obj is Array)
			{
				WriteArray((Array)obj, objType, context);
			}
			else if (obj is IList)
			{
				WriteIList((IList)obj, context);
			}
			else if (obj is ICollection)
			{
				WriteICollection((ICollection)obj, context);
			}
			else
			{
				WriteCountedEnumerable(obj, context, false);
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

		private static void WriteJsonObject(object obj, Type objType, JaysonSerializationContext context)
		{
			if (obj == null)
			{
				if (!context.Settings.IgnoreNullValues)
				{
					context.Builder.Append("null");
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

				if (jtns == JaysonTypeNameSerialization.All ||
					((jtns == JaysonTypeNameSerialization.Auto ||
						jtns == JaysonTypeNameSerialization.AllButNoPrimitive) &&
						(jtc == JaysonTypeCode.Nullable || 
						((JaysonTypeCode.Primitive & jtc) == jtc &&
							(JaysonTypeCode.Number & jtc) != jtc)))) {
					StringBuilder builder = context.Builder;

					context.ObjectDepth++;
					try {
						WritePrimitiveTypeName (objType, context);
						context.Formatter.Format (obj, objType, builder);
					} finally {
						context.ObjectDepth--;
						if (context.Settings.Formatting) {
							builder.Append (JaysonConstants.Indentation [context.ObjectDepth]);
						}
						builder.Append ('}');
					}
					return;
				}

				context.Formatter.Format(obj, objType, context.Builder);
				return;
			}

			if (obj == DBNull.Value)
			{
				if (!context.Settings.IgnoreNullValues)
				{
					context.Builder.Append("null");
				}
				return;
			}

			JaysonStackList stack = null;
			if (info.Class)
			{
				stack = context.Stack;
				if (stack.Contains(obj))
				{
					if (context.Settings.RaiseErrorOnCircularRef) 
					{
						throw new JaysonException ("Circular reference on " + objType.Name);
					}
					if (!context.Settings.IgnoreNullValues)
					{
						context.Builder.Append("null");
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
				#if !(NET3500 || NET3000 || NET2000)
				if (settings.DisableExpandoObjects && (objType == typeof(ExpandoObject)))
				{
					if (!settings.IgnoreNullValues) {
						context.Builder.Append("null");
					} else {
						context.Builder.Append('{');
						context.Builder.Append('}');
					}
					return;
				}
				#endif

                if (settings.DisableAnonymousTypes && JaysonTypeInfo.IsAnonymous(objType))
				{
					if (!settings.IgnoreNullValues) {
						context.Builder.Append("null");
					} else {
						context.Builder.Append('{');
						context.Builder.Append('}');
					}
					return;
				}

				if (obj is IDictionary)
				{
					WriteDictionary((IDictionary)obj, context);
				}
				else if (obj is StringDictionary)
				{
					WriteStringDictionary((StringDictionary)obj, context);
				}
				else if (obj is NameValueCollection)
				{
					WriteNameValueCollection((NameValueCollection)obj, context);
				}
				else if (obj is IDictionary<string, object>)
				{
					WriteGenericStringDictionary((IDictionary<string, object>)obj, context);
				}
				#if !(NET3500 || NET3000 || NET2000)
				else if (obj is DynamicObject)
				{
					if (!settings.DisableDynamicObjects) {
						WriteDynamicObject((DynamicObject)obj, context);
					} else if (!settings.IgnoreNullValues) {
						context.Builder.Append("null");
					} else {
						context.Builder.Append('{');
						context.Builder.Append('}');
					}
				}
				#endif
				else if (obj is DataTable)
				{
					WriteDataTable((DataTable)obj, context);
				}
				else if (obj is DataSet)
				{
					WriteDataSet((DataSet)obj, context);
				}
				else if (obj is IEnumerable)
				{
					WriteEnumerable((IEnumerable)obj, objType, context);
				}
				else
				{
					WriteClassOrStruct(obj, objType, context);
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
				convertDecimalToDouble: settings.ConvertDecimalToDouble
			);

			Type objType = obj.GetType();
			if (JaysonTypeInfo.IsJPrimitive(objType))
			{
				if (settings.TypeNames == JaysonTypeNameSerialization.None) {
					return formatter.Format(obj, objType);
				}

				JaysonStackList primmeStack = JaysonStackList.Get();
				try 
				{
					StringBuilder builder = new StringBuilder(100, int.MaxValue);

					var context = new JaysonSerializationContext (filter: filter,
						settings: settings,
						stack: primmeStack,
						builder: builder,
						formatter: formatter
					);

					context.ObjectDepth++;
					WritePrimitiveTypeName (objType, context);
					formatter.Format (obj, objType, builder);

					context.ObjectDepth--;
					if (settings.Formatting) {
						builder.Append (JaysonConstants.Indentation [0]);
					}
					builder.Append ('}');

					return builder.ToString();
				}
				finally
				{
					JaysonStackList.Release(primmeStack);
				}
			}

			JaysonStackList stack = JaysonStackList.Get();
			try
			{
				StringBuilder builder = new StringBuilder(2048, int.MaxValue);

				WriteJsonObject(obj, objType,
					new JaysonSerializationContext(filter: filter,
						settings: settings,
						stack: stack,
						builder: builder,
						formatter: formatter
					)
				);
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
