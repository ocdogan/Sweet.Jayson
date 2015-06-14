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
		# region ToJsonObject

        private static Dictionary<string, object> AsDictionaryObjectKey(IDictionary obj, JaysonSerializationContext context)
        {
            if (obj.Count == 0)
            {
                return new Dictionary<string, object>();
            }

            List<object> kvList = new List<object>(obj.Count);
            Dictionary<string, object> result = new Dictionary<string, object>();

            result["$kv"] = kvList;

            string key;
            object value;
            object keyObj;

            Func<string, object, object> filter = context.Filter;
            bool canFilter = (filter != null);

            foreach (DictionaryEntry dEntry in obj)
            {
                keyObj = dEntry.Key;
                value = dEntry.Value;

                if (value != null)
                {
                    if (canFilter)
                    {
                        key = (keyObj is string) ? (string)keyObj : keyObj.ToString();
                        value = filter(key, value);
                    }

                    if (value != null)
                    {
                        value = ToJsonObject(value, context);
                    }
                }


                kvList.Add(new Dictionary<string, object> { 
                    { "$k", ToJsonObject(keyObj, context) },
                    { "$v", ToJsonObject(value, context) } 
                });
            }

            return result;
        }

        private static Dictionary<string, object> AsDictionary(IDictionary obj, JaysonSerializationContext context)
		{
			bool useStringKey = !context.Settings.UseKVModelForJsonObjects;
			if (!useStringKey) {
				var genericArgs = JaysonTypeInfo.GetGenericArguments (obj.GetType ());
				useStringKey = (genericArgs != null) && (genericArgs.Length > 0) && (genericArgs [0] == typeof(string));
			}

			if (useStringKey)
            {
                if (obj.Count == 0)
                {
                    return new Dictionary<string, object>();
                }

                Dictionary<string, object> result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

                string key;
                object value;
                object keyObj;

                Func<string, object, object> filter = context.Filter;
                bool canFilter = (filter != null);

                foreach (DictionaryEntry dEntry in obj)
                {
                    value = dEntry.Value;

                    keyObj = dEntry.Key;
                    key = (keyObj is string) ? (string)keyObj : keyObj.ToString();

                    if (value != null)
                    {
                        if (canFilter)
                        {
                            value = filter(key, value);
                        }

                        if (value != null)
                        {
                            value = ToJsonObject(value, context);
                        }
                    }

                    result[key] = value;
                }

                return result;
            }

            return AsDictionaryObjectKey(obj, context);
		}

		private static Dictionary<string, object> AsStringDictionary(StringDictionary obj, JaysonSerializationContext context)
		{
			if (obj.Count == 0)
			{
				return new Dictionary<string, object>();
			}

			Dictionary<string, object> result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

			string key;
			object value;

			Func<string, object, object> filter = context.Filter;
			bool canFilter = (filter != null);

			foreach (DictionaryEntry dEntry in obj)
			{
				key = (string)dEntry.Key;
				value = dEntry.Value;

				if (value != null)
				{
					if (canFilter)
					{
						value = filter(key, value);
					}

					if (value != null)
					{
						value = ToJsonObject(value, context);
					}
				}

    			result.Add(key, value);
			}

			return result;
		}

        private static Dictionary<string, object> AsNameValueCollection(NameValueCollection obj, JaysonSerializationContext context)
        {
            if (obj.Count == 0)
            {
                return new Dictionary<string, object>();
            }

            Dictionary<string, object> result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

            object value;

            Func<string, object, object> filter = context.Filter;
            bool canFilter = (filter != null);

            foreach (var key in obj.AllKeys)
            {
                value = obj[key];

                if (value != null)
                {
                    if (canFilter)
                    {
                        value = filter(key, value);
                    }

                    if (value != null)
                    {
                        value = ToJsonObject(value, context);
                    }
                }

                result.Add(key, value);
            }

            return result;
        }

		#if !(NET3500 || NET3000 || NET2000)
		private static Dictionary<string, object> AsDynamicObject(DynamicObject obj, JaysonSerializationContext context)
		{
			object value;
			bool ignoreNullValues = context.Settings.IgnoreNullValues;

			Dictionary<string, object> result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

			Func<string, object, object> filter = context.Filter;
			bool canFilter = (filter != null);

			var members = JaysonFastMemberCache.GetMembers(obj.GetType());
			if (members.Count > 0) 
			{
				bool ignoreReadOnlyMembers = context.Settings.IgnoreReadOnlyMembers;

				string fKey;
				foreach (var memberKvp in members) 
				{
					if (!ignoreReadOnlyMembers || memberKvp.Value.CanWrite) 
					{
						fKey = memberKvp.Key;
						value = memberKvp.Value.Get (obj);

						if (value != null) 
						{
							if (canFilter) 
							{
								value = filter (fKey, value);
							}

							if (value != null) 
							{
								value = ToJsonObject (value, context);
							}
						}

						if ((value != null) || !ignoreNullValues) 
						{
							result.Add (fKey, value);
						}
					}
				}
			}

			JaysonDynamicWrapper dObj = new JaysonDynamicWrapper((IDynamicMetaObjectProvider)obj);

			foreach (var dKey in obj.GetDynamicMemberNames())
			{
				value = dObj[dKey];

				if (value != null)
				{
					if (canFilter)
					{
						value = filter(dKey, value);
					}

					if (value != null)
					{
						value = ToJsonObject(value, context);
					}
				}

				if ((value != null) || !ignoreNullValues)
				{
					result.Add(dKey, value);
				}
			}

			return result;
		}
		#endif

		private static Dictionary<string, object> AsObject(object obj, Type objType, JaysonSerializationContext context)
		{
			Dictionary<string, object> result = new Dictionary<string, object> (JaysonConstants.DictionaryCapacity);

			var members = JaysonFastMemberCache.GetMembers (objType);
			if (members.Count > 0) 
			{
				string key;
				object value;
				bool ignoreNullValues = context.Settings.IgnoreNullValues;
				bool ignoreReadOnlyMembers = context.Settings.IgnoreReadOnlyMembers;

				Func<string, object, object> filter = context.Filter;
				bool canFilter = (filter != null);

				foreach (var memberKvp in members) 
				{
					if (!ignoreReadOnlyMembers || memberKvp.Value.CanWrite) 
					{
						key = memberKvp.Key;
						value = memberKvp.Value.Get (obj);

						if (value != null) 
						{
							if (canFilter) 
							{
								value = filter (key, value);
							}

							if (value != null) 
							{
								value = ToJsonObject (value, context);
							}
						}

						if ((value != null) || !ignoreNullValues) 
						{
							result.Add (key, value);
						}
					}
				}
			}

			return result;
		}

		private static Dictionary<string, object> AsGenericStringDictionary(IDictionary<string, object> obj,
			JaysonSerializationContext context)
		{
			if (obj.Count == 0)
			{
				return new Dictionary<string, object>();
			}

			Dictionary<string, object> result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

			string key;
			object value;

			Func<string, object, object> filter = context.Filter;
			bool canFilter = (filter != null);

			foreach (var kvp in obj)
			{
				key = kvp.Key;
				value = kvp.Value;

				if (value != null)
				{
					if (canFilter)
					{
						value = filter(key, value);
					}

					if (value != null)
					{
						value = ToJsonObject(value, context);
					}
				}

				result.Add(key, value);
			}

			return result;
		}

		private static Array AsArray(Array obj, Type objType, JaysonSerializationContext context)
		{
            var info = JaysonTypeInfo.GetTypeInfo(JaysonTypeInfo.GetElementType(objType));
            if (info.JPrimitive || info.Enum)
			{
				return obj;
			}

			if (info.Type == typeof(DBNull))
			{
				if (context.Settings.IgnoreNullValues)
				{
					return JaysonConstants.EmptyObjArray;
				}
				return new object[obj.Length];
			}

			return AsIList(obj, context);
		}

		private static object[] AsIList(IList obj, JaysonSerializationContext context)
		{
			int count = obj.Count;
			if (count == 0)
			{
				return JaysonConstants.EmptyObjArray;
			}

			object[] result = new object[count];
			bool ignoreNullValues = context.Settings.IgnoreNullValues;

			object item;

			for (int i = 0; i < count; i++)
			{
				item = obj[i];
				if (item != null)
				{
					item = ToJsonObject(item, context);
				}

				if ((item != null) || !ignoreNullValues)
				{
					result[i] = item;
				}
			}

			return result;
		}

		private static object[] AsICollection(ICollection obj, JaysonSerializationContext context)
		{
			int count = obj.Count;
			if (count == 0)
			{
				return JaysonConstants.EmptyObjArray;
			}

			object[] result = new object[count];
			obj.CopyTo(result, 0);

			object item;
			bool ignoreNullValues = context.Settings.IgnoreNullValues;

			for (int i = 0; i < count; i++)
			{
				item = result[i];
				if (item != null)
				{
					item = ToJsonObject(item, context);
				}

				if ((item != null) || !ignoreNullValues)
				{
					result[i] = item;
				}
			}

			return result;
		}

		private static object AsEnumerable(IEnumerable obj, Type objType, JaysonSerializationContext context)
		{
			if (obj is Array)
			{
				obj = AsArray((Array)obj, objType, context);
			}
			else if (obj is IList)
			{
				obj = AsIList((IList)obj, context);
			}
			else
			{
				bool ignoreNullValues = context.Settings.IgnoreNullValues;

				Type entryType;
				JaysonDictionaryType dType = JaysonCommon.GetDictionaryType(obj, out entryType);

				IEnumerator enumerator = obj.GetEnumerator();
				try
				{
					if (dType == JaysonDictionaryType.IDictionary)
					{
						string key;
						object value;
						object keyObj;
						DictionaryEntry dEntry;

						Dictionary<string, object> result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

						Func<string, object, object> filter = context.Filter;
						bool canFilter = (filter != null);

						while (enumerator.MoveNext())
						{
							dEntry = (DictionaryEntry)enumerator.Current;

							value = dEntry.Value;

							keyObj = dEntry.Key;
							key = (keyObj is string) ? (string)keyObj : keyObj.ToString();

							if (value != null)
							{
								if (canFilter)
								{
									value = filter(key, value);
								}

								if (value != null)
								{
									value = ToJsonObject(value, context);
								}
							}

							if ((value != null) || !ignoreNullValues)
							{
								result[key] = value;
							}
						}

						obj = result;
					}
					else if (dType == JaysonDictionaryType.IGenericDictionary)
					{
						Dictionary<string, object> result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);
						IDictionary<string, IJaysonFastMember> members = JaysonFastMemberCache.GetMembers(entryType);

						IJaysonFastMember keyFm;
						IJaysonFastMember valueFm;

						if (members.TryGetValue("Key", out keyFm) && members.TryGetValue("Value", out valueFm))
						{
							string key;
							object value;
							object keyObj;

							Func<string, object, object> filter = context.Filter;
							bool canFilter = (filter != null);

							while (enumerator.MoveNext())
							{
								keyObj = keyFm.Get(enumerator.Current);
								if (keyObj != null)
								{
									key = (keyObj is string) ? (string)keyObj : keyObj.ToString();
									value = valueFm.Get(enumerator.Current);

									if (value != null)
									{
										if (canFilter)
										{
											value = filter(key, value);
										}

										if (value != null)
										{
											value = ToJsonObject(value, context);
										}
									}

									if ((value != null) || !ignoreNullValues)
									{
										result[key] = value;
									}
								}
							}
						}

						obj = result;
					}
					else if (obj is ICollection)
					{
						obj = AsICollection((ICollection)obj, context);
					}
					else
					{
						object item;
						List<object> result = new List<object>();

						while (enumerator.MoveNext())
						{
							item = enumerator.Current;
							if (item != null)
							{
								item = ToJsonObject(item, context);
							}

							if ((item != null) || !ignoreNullValues)
							{
								result.Add(item);
							}
						}
						obj = result.ToArray();
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
			return obj;
		}

		# region DataTable & DataSet

        private static List<string> AsColumnNames(DataColumn[] columns, JaysonSerializationContext context)
        {
            if (columns != null && columns.Length > 0)
            {
                List<string> result = new List<string>(columns.Length);
                foreach (var column in columns)
                {
                    if (column.ColumnName != null)
                    {
                        result.Add(column.ColumnName);
                    }
                }
                return result;
            }

            return null;
        }

		private static List<object> AsDataTableRows(DataTable dataTable, JaysonSerializationContext context)
		{
			var rows = dataTable.Rows;
			int rowCount = rows.Count;

			if (rowCount > 0) 
			{
				var columns = dataTable.Columns;
				int columnCount = columns.Count;

				if (columnCount > 0) 
				{
					List<object> result = new List<object> (rowCount);

					DataColumn dataColumn;
                    List<JaysonTuple<DataColumn, JaysonTypeInfo>> columnsInfo = new List<JaysonTuple<DataColumn, JaysonTypeInfo>>();

					for (int i = 0; i < columnCount; i++) 
					{
						dataColumn = columns [i];
                        columnsInfo.Add(new JaysonTuple<DataColumn, JaysonTypeInfo>(dataColumn, 
							JaysonTypeInfo.GetTypeInfo (dataColumn.DataType)));
					}

					Func<string, object, object> filter = context.Filter;
					bool canFilter = (filter != null);

					DataRow dataRow;
					object cellValue;

					List<object> cellList;
                    JaysonTuple<DataColumn, JaysonTypeInfo> columnInfo;

					var tableName = dataTable.TableName;
					if (dataTable.DataSet != null) 
					{
						tableName += "@" + dataTable.DataSet.DataSetName + "::";
					}

					for (int i = 0; i < rowCount; i++) 
					{
						dataRow = rows [i];

						cellList = new List<object> (columnCount);
						result.Add (cellList);

						for (int j = 0; j < columnCount; j++) 
						{
							columnInfo = columnsInfo [j];
							cellValue = dataRow [columnInfo.Item1];

							if (cellValue == null || cellValue == DBNull.Value) 
							{
								cellList.Add (cellValue);
							} 
							else 
							{
								if (canFilter)
								{
									cellValue = filter(tableName + columnInfo.Item1.ColumnName, cellValue);
								}

								if (cellValue != null)
								{
									cellValue = ToJsonObject(cellValue, context);
								}

								cellList.Add(cellValue);
							}
						}
					}
					return result;
				}
			}
			return null;
		}

        private static Dictionary<string, object> AsDataTableColumn(DataColumn dataColumn, JaysonSerializationContext context)
        {
            Dictionary<string, object> result = new Dictionary<string, object>(16);

            string defaultNamespace = dataColumn.Table.Namespace;

            if (!dataColumn.AllowDBNull)
            {
                result.Add("AllowDBNull", dataColumn.AllowDBNull);
            }
            if (dataColumn.AutoIncrement)
            {
                result.Add("AutoIncrement", dataColumn.AutoIncrement);
            }
            if (dataColumn.AutoIncrementSeed != 0)
            {
                result.Add("AutoIncrementSeed", dataColumn.AutoIncrementSeed);
            }
            if (dataColumn.AutoIncrementStep != 1)
            {
                result.Add("AutoIncrementStep", dataColumn.AutoIncrementStep);
            }
            if (!String.IsNullOrEmpty(dataColumn.Caption) && dataColumn.Caption != dataColumn.ColumnName)
            {
                result.Add("Caption", dataColumn.Caption);
            }
            if (dataColumn.ColumnMapping != MappingType.Element)
            {
                result.Add("ColumnMapping", dataColumn.ColumnMapping);
            }
            if (!String.IsNullOrEmpty(dataColumn.ColumnName))
            {
                result.Add("ColumnName", dataColumn.ColumnName);
            }
            result.Add("DataType", JaysonTypeInfo.GetTypeName(dataColumn.DataType, JaysonTypeNameInfo.TypeNameWithAssembly));
            if (!String.IsNullOrEmpty(dataColumn.Expression))
            {
                result.Add("Expression", dataColumn.Expression);
            }
            if (dataColumn.MaxLength != -1)
            {
                result.Add("MaxLength", dataColumn.MaxLength);
            }
            if (!String.IsNullOrEmpty(dataColumn.Namespace) && dataColumn.Namespace != defaultNamespace)
            {
                result.Add("Namespace", dataColumn.Namespace);
            }
            result.Add("Ordinal", dataColumn.Ordinal);
            if (!String.IsNullOrEmpty(dataColumn.Prefix))
            {
                result.Add("Prefix", dataColumn.Prefix);
            }
            if (dataColumn.ReadOnly)
            {
                result.Add("ReadOnly", dataColumn.ReadOnly);
            }
            if (dataColumn.Unique)
            {
                result.Add("Unique", dataColumn.Unique);
            }
            if (dataColumn.ExtendedProperties.Count > 0)
            {
				result.Add("ExtendedProperties", AsDictionary(dataColumn.ExtendedProperties, context));
            }

            return result;
        }

        private static List<object> AsDataTableColumns(DataTable dataTable, JaysonSerializationContext context)
        {
            var columns = dataTable.Columns;
            if (columns.Count > 0)
            {
				List<object> result = new List<object>(columns.Count);
                foreach (DataColumn dataColumn in columns)
                {
                    result.Add(AsDataTableColumn(dataColumn, context));
                }

                return result;
            }
            return null;
        }

        private static Dictionary<string, object> AsDataTable(DataTable dataTable, JaysonSerializationContext context)
        {
            Dictionary<string, object> result = new Dictionary<string, object>(10);

            if (dataTable.CaseSensitive)
            {
                result.Add("CaseSensitive", dataTable.CaseSensitive);
            }

			var columns = AsDataTableColumns (dataTable, context);
			if (columns != null) 
			{
				result.Add ("Columns", columns);
			}

            if (!String.IsNullOrEmpty(dataTable.DisplayExpression))
            {
                result.Add("DisplayExpression", dataTable.DisplayExpression);
            }
            if (dataTable.Locale != CultureInfo.InvariantCulture)
            {
                result.Add("Locale", dataTable.Locale.Name);
            }
            if (!String.IsNullOrEmpty(dataTable.Namespace))
            {
                result.Add("Namespace", dataTable.Namespace);
            }
            if (!String.IsNullOrEmpty(dataTable.Prefix))
            {
                result.Add("Prefix", dataTable.Prefix);
            }

            var primaryKey = AsColumnNames(dataTable.PrimaryKey, context);
			if (primaryKey != null)
            {
				result.Add("PrimaryKey", primaryKey);
            }

			var rows = AsDataTableRows (dataTable, context);
			if (columns != null) 
			{
				result.Add ("Rows", rows);
			}
            if (!String.IsNullOrEmpty(dataTable.TableName))
            {
                result.Add("TableName", dataTable.TableName);
            }
            if (dataTable.ExtendedProperties.Count > 0)
            {
				result.Add("ExtendedProperties", AsDictionary(dataTable.ExtendedProperties, context));
            }

            return result;
        }

        private static List<object> AsDataTables(DataSet dataSet, JaysonSerializationContext context)
        {
            var tables = dataSet.Tables;
            if (tables.Count > 0)
            {
                List<object> result = new List<object>(tables.Count);
                foreach (DataTable table in tables)
                {
                    result.Add(AsDataTable(table, context));
                }
                return result;
            }
            return null;
        }

		private static Dictionary<string, object> AsDataRelation(DataRelation relation, JaysonSerializationContext context)
		{
			Dictionary<string, object> result = new Dictionary<string, object>(10);

			if (!String.IsNullOrEmpty(relation.RelationName))
			{
				result.Add("RelationName", relation.RelationName);
			}

			var columnNames = AsColumnNames(relation.ChildColumns, context);
			if (columnNames != null)
			{
				result.Add("ChildColumns", columnNames);
			}

			if (relation.ChildTable != null)
			{
				if (relation.ChildTable.TableName != null)
				{
					result.Add("ChildTable", relation.ChildTable.TableName);
				}

				if (!String.IsNullOrEmpty(relation.ChildTable.Namespace))
				{
					result.Add("ChildTableNamespace", relation.ChildTable.Namespace);
				}
			}

			if (relation.ExtendedProperties.Count > 0)
			{
				result.Add("ExtendedProperties", AsDictionary(relation.ExtendedProperties, context));
			}

			if (relation.Nested)
			{
				result.Add("Nested", relation.Nested);
			}

			columnNames = AsColumnNames(relation.ParentColumns, context);
			if (columnNames != null)
			{
				result.Add("ParentColumns", columnNames);
			}

			if (relation.ParentTable != null)
			{
				if (relation.ParentTable.TableName != null)
				{
					result.Add("ParentTable", relation.ParentTable.TableName);
				}

				if (!String.IsNullOrEmpty(relation.ParentTable.Namespace))
				{
					result.Add("ParentTableNamespace", relation.ParentTable.Namespace);
				}
			}

			return result;
		}

        private static List<object> AsDataRelations(DataRelationCollection relations, JaysonSerializationContext context)
        {
            if (relations.Count > 0)
            {
				int relationCount = relations.Count;
				List<object> result = new List<object>(relationCount);

				for (int i = 0; i < relationCount; i++) 
				{
					result.Add (AsDataRelation(relations [i], context));
				}

                return result;
            }
            return null;
        }

        private static Dictionary<string, object> AsDataSet(DataSet dataSet, JaysonSerializationContext context)
        {
            Dictionary<string, object> result = new Dictionary<string, object>(JaysonConstants.DictionaryCapacity);

            if (dataSet.CaseSensitive)
            {
                result.Add("CaseSensitive", dataSet.CaseSensitive);
            }
            if (!String.IsNullOrEmpty(dataSet.DataSetName))
            {
                result.Add("DataSetName", dataSet.DataSetName);
            }
            if (!dataSet.EnforceConstraints)
            {
                result.Add("EnforceConstraints", dataSet.EnforceConstraints);
            }
            if (dataSet.Locale != CultureInfo.InvariantCulture)
            {
                result.Add("Locale", dataSet.Locale.Name);
            }
            if (!String.IsNullOrEmpty(dataSet.Namespace))
            {
                result.Add("Namespace", dataSet.Namespace);
            }
            if (!String.IsNullOrEmpty(dataSet.Prefix))
            {
                result.Add("Prefix", dataSet.Prefix);
            }
            if (dataSet.SchemaSerializationMode != SchemaSerializationMode.IncludeSchema)
            {
                result.Add("SchemaSerializationMode", dataSet.SchemaSerializationMode);
            }
            if (dataSet.ExtendedProperties.Count > 0)
            {
				result.Add("ExtendedProperties", AsDictionary(dataSet.ExtendedProperties, context));
            }

            var relations = AsDataRelations(dataSet.Relations, context);
            if (relations != null)
            {
                result.Add("Relations", relations);
            }

            var tables = AsDataTables(dataSet, context);
            if (tables != null)
            {
                result.Add("Tables", tables);
            }

            return result;
        }

		# endregion DataTable & DataSet

		private static object ToJsonObject(object obj, JaysonSerializationContext context)
		{
			if (obj != null)
			{
				if (JaysonTypeInfo.IsJPrimitiveObject(obj))
				{
					return obj;
				}

				var info = JaysonTypeInfo.GetTypeInfo(obj.GetType());
				if (info.Enum)
				{
					return obj;
				}

				if (obj == DBNull.Value)
				{
					return null;
				}

				JaysonStackList stack = null;
				if (info.Class)
				{
					stack = context.Stack;
					if (stack.Contains(obj))
					{
						if (context.Settings.RaiseErrorOnCircularRef) 
						{
							throw new JaysonException (JaysonError.CircularReferenceOn + info.Type.Name);
						}
						return null;
					}

					stack.Push(obj);
				}

				try
				{
					JaysonSerializationSettings settings = context.Settings;
					#if !(NET3500 || NET3000 || NET2000)
					if (settings.IgnoreExpandoObjects && (info.Type == typeof(ExpandoObject)))
					{
						return null;
					}
					#endif

                    if (settings.IgnoreAnonymousTypes && info.Anonymous)
					{
						return null;
					}

					if (obj is IDictionary<string, object>)
					{
						return AsGenericStringDictionary((IDictionary<string, object>)obj, context);
					}

					if (obj is IDictionary)
					{
						return AsDictionary((IDictionary)obj, context);
					}

					if (obj is DataSet)
					{
						return AsDataSet((DataSet)obj, context);
					}

					if (obj is DataTable)
					{
						return AsDataTable ((DataTable)obj, context);
					}

					#if !(NET3500 || NET3000 || NET2000)
					if (obj is DynamicObject)
					{
						if (!settings.IgnoreDynamicObjects)
						{
							return AsDynamicObject((DynamicObject)obj, context);
						}
						return null;
					}
					#endif

					if (obj is StringDictionary)
					{
						return AsStringDictionary((StringDictionary)obj, context);
					}

					if (obj is NameValueCollection)
					{
						return AsNameValueCollection((NameValueCollection)obj, context);
					}

					if (obj is IEnumerable)
					{
						return AsEnumerable((IEnumerable)obj, info.Type, context);
					}

					return AsObject(obj, info.Type, context);
				}
				finally
				{
					if (info.Class)
					{
						stack.Pop();
					}
				}
			}

			return obj;
		}

		public static object ToJsonObject(object obj, JaysonSerializationSettings settings = null,
			Func<string, object, object> filter = null)
		{
			JaysonStackList stack = JaysonStackList.Get();
			try
			{
				return ToJsonObject(obj, new JaysonSerializationContext(filter: filter,
					settings: settings ?? JaysonSerializationSettings.Default,
					stack: stack
				));
			}
			finally
			{
				JaysonStackList.Release(stack);
			}
		}

		# endregion ToJsonObject
	}

	# endregion JsonConverter
}
