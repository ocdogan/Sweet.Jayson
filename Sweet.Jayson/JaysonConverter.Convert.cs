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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
#if !(NET3500 || NET3000 || NET2000)
using System.Dynamic;
#endif
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Sweet.Jayson
{
	# region JsonConverter

	public static partial class JaysonConverter
    {
        # region EvaluatedDictionaryType
        
        private class EvaluatedDictionaryType
        {
            public bool AsDictionary;
            public bool AsReadOnly;
            public Type EvaluatedType;
        }

        # endregion EvaluatedDictionaryType

        # region EvaluatedListType

        private class EvaluatedListType
        {
            public bool AsList;
            public bool AsArray;
            public bool AsReadOnly;
            public Type EvaluatedType;
        }

        # endregion EvaluatedListType

        # region Static Members

        private static readonly Dictionary<Type, ConstructorInfo> s_CtorCache = new Dictionary<Type, ConstructorInfo>();
        private static readonly Dictionary<Type, Func<object[], object>> s_ActivatorCache = new Dictionary<Type, Func<object[], object>>();

        private static readonly Dictionary<Type, EvaluatedListType> s_EvaluatedListTypeCache = new Dictionary<Type, EvaluatedListType>();
        private static readonly Dictionary<Type, EvaluatedDictionaryType> s_EvaluatedDictionaryTypeCache = new Dictionary<Type, EvaluatedDictionaryType>();

		private static readonly Dictionary<Type, Type> s_GenericUnderlyingCache = new Dictionary<Type, Type>();

		# endregion Static Members

		# region Convert

		private static Type GetGenericUnderlyingList(JaysonTypeInfo info)
		{
			Type result;
			if (!s_GenericUnderlyingCache.TryGetValue(info.Type, out result))
			{
				Type[] argTypes = info.GenericArguments;
				if (argTypes[0] == typeof(object)) {
					result = typeof(List<object>);
				} else {
					result = typeof(List<>).MakeGenericType(argTypes);			
				}

				s_GenericUnderlyingCache[info.Type] = result;
			}
			return result;
		}

		private static Type GetGenericUnderlyingDictionary(JaysonTypeInfo info) 
		{
			Type result;
			if (!s_GenericUnderlyingCache.TryGetValue(info.Type, out result))
			{
				Type[] argTypes = info.GenericArguments;
				if (argTypes[0] == typeof(string) && argTypes[1] == typeof(object)) {
					result = typeof(Dictionary<string, object>);
				} else {
					result = typeof(Dictionary<,>).MakeGenericType(argTypes);
				}

				s_GenericUnderlyingCache[info.Type] = result;
			}
			return result;
		}

		private static ConstructorInfo GetReadOnlyCollectionCtor(Type rocType)  
		{
			ConstructorInfo ctor;
			if (!s_CtorCache.TryGetValue(rocType, out ctor)) 
			{
				Type[] argTypes = JaysonTypeInfo.GetTypeInfo(rocType).GenericArguments;

				rocType = typeof(ReadOnlyCollection<>).MakeGenericType(argTypes);
				ctor = rocType.GetConstructor(new Type[] { typeof(IList<>).MakeGenericType(argTypes) });

				s_CtorCache[rocType] = ctor;
			}
			return ctor;
		}

        private static Func<object[], object> GetReadOnlyCollectionActivator(Type rocType)
        {
            Func<object[], object> result;
            if (!s_ActivatorCache.TryGetValue(rocType, out result))
            {
                result = JaysonCommon.CreateActivator(GetReadOnlyCollectionCtor(rocType));
                s_ActivatorCache[rocType] = result;
            }
            return result;
        }

		#if !(NET4000 || NET3500 || NET3000 || NET2000)
		private static ConstructorInfo GetReadOnlyDictionaryCtor(Type rodType)  
		{
			ConstructorInfo ctor;
			if (!s_CtorCache.TryGetValue(rodType, out ctor)) 
			{
				Type[] argTypes = JaysonTypeInfo.GetTypeInfo(rodType).GenericArguments;

				rodType = typeof(ReadOnlyDictionary<,>).MakeGenericType(argTypes);
				ctor = rodType.GetConstructor(new Type[] { typeof(IDictionary<,>).MakeGenericType(argTypes) });

				s_CtorCache[rodType] = ctor;
			}
			return ctor;
		}

        private static Func<object[], object> GetReadOnlyDictionaryActivator(Type rodType)
        {
            Func<object[], object> result;
            if (!s_ActivatorCache.TryGetValue(rodType, out result))
            {
                result = JaysonCommon.CreateActivator(GetReadOnlyDictionaryCtor(rodType));
                s_ActivatorCache[rodType] = result;
            }
            return result;
        }
		#endif

        # region Convert DataTable & DataSet

        private static void SetExtendedProperties(PropertyCollection extendedProperties, object obj, 
            JaysonDeserializationSettings settings)
        {
            if (obj is Hashtable)
            {
                foreach (DictionaryEntry ekvp in (Hashtable)obj)
                {
                    if (!(ekvp.Key is string) || (string)ekvp.Key != "$type")
                    {
                        extendedProperties.Add(ekvp.Key, ConvertObject(ekvp.Value, typeof(object), settings));
                    }
                }
            }
            else if (obj is IDictionary<string, object>)
            {
                foreach (var ekvp in (IDictionary<string, object>)obj)
                {
                    if (ekvp.Key != "$type")
                    {
                        extendedProperties.Add(ekvp.Key, ConvertObject(ekvp.Value, typeof(object), settings));
                    }
                }
            }
        }

        private static void SetDataTableProperties(IDictionary<string, object> obj, DataTable dataTable,
            JaysonDeserializationSettings settings)
        {
            object propValue;
			if (dataTable.ChildRelations.Count == 0 && dataTable.ParentRelations.Count == 0) 
			{
				if (obj.TryGetValue ("CaseSensitive", out propValue)) 
				{
					if (propValue is bool) 
					{
						dataTable.CaseSensitive = (bool)propValue;
					} 
					else if (propValue is string) 
					{
						dataTable.CaseSensitive = (string)propValue == "true";
					}
				}

				if (obj.TryGetValue ("Locale", out propValue)) 
				{
					dataTable.Locale = CultureInfo.GetCultureInfo ((string)propValue);
				}
			}

            if (obj.TryGetValue("DisplayExpression", out propValue))
            {
                dataTable.DisplayExpression = (string)propValue;
            }

            if (obj.TryGetValue("Namespace", out propValue))
            {
                dataTable.Namespace = (string)propValue;
            }

            if (obj.TryGetValue("Prefix", out propValue))
            {
                dataTable.Prefix = (string)propValue;
            }

            if (obj.TryGetValue("TableName", out propValue))
            {
                dataTable.TableName = (string)propValue;
            }

            if (obj.TryGetValue("ExtendedProperties", out propValue))
            {
                SetExtendedProperties(dataTable.ExtendedProperties, propValue, settings);
            }
        }

        private static void SetDataTableColumns(IDictionary<string, object> obj, DataTable dataTable,
            JaysonDeserializationSettings settings)
        {
            object columnsObj;
            if (obj.TryGetValue("Columns", out columnsObj))
            {
				var columnList = (IList)columnsObj;

                int ordinal;
                Type dataType;
                string expression;
                MappingType mappingType;
                Dictionary<string, object> columnInfo;

                object propValue;
                DataColumn column;
                string columnName;

                List<DataColumn> unordinalColumnList = new List<DataColumn>();
                DataColumn[] ordinalColumnList = new DataColumn[columnList.Count];

                foreach (var columnInfoObj in columnList)
                {
                    columnInfo = (Dictionary<string, object>)columnInfoObj;

                    columnName = null;
                    if (columnInfo.TryGetValue("ColumnName", out propValue))
                    {
                        columnName = (string)propValue ?? String.Empty;
                    }

                    if (columnName != null && dataTable.Columns.Contains(columnName))
                    {
                        column = dataTable.Columns[columnName];
                    }
                    else
                    {
                        if (columnInfo.TryGetValue("DataType", out propValue) && propValue != null)
                        {
							dataType = GetTypeOverride(JaysonCommon.GetType((string)propValue, settings.Binder), settings);
                        }
                        else
                        {
                            dataType = typeof(object);
                        }

                        expression = null;
                        if (columnInfo.TryGetValue("Expression", out propValue))
                        {
                            expression = (string)propValue;
                        }

                        if (columnInfo.TryGetValue("ColumnMapping", out propValue) && propValue != null)
                        {
                            mappingType = (MappingType)JaysonEnumCache.Parse((string)propValue, typeof(MappingType));
                        }
                        else
                        {
                            mappingType = MappingType.Element;
                        }

                        column = new DataColumn(columnName, dataType, expression, mappingType);

                        ordinal = -1;
                        if (columnInfo.TryGetValue("Ordinal", out propValue))
                        {
                            if (propValue is int)
                            {
                                ordinal = (int)propValue;
                            }
                            else
                                if (propValue is long)
                                {
                                    ordinal = (int)((long)propValue);
                                }
                                else
                                    if (propValue is string)
                                    {
                                        ordinal = int.Parse((string)propValue, JaysonConstants.InvariantCulture);
                                    }
                        }

                        if (ordinal > -1)
                        {
                            ordinalColumnList[ordinal] = column;
                        }
                        else
                        {
                            unordinalColumnList.Add(column);
                        }
                    }

                    if (columnInfo.TryGetValue("Namespace", out propValue))
                    {
                        column.Namespace = (string)propValue;
                    }

                    if (columnInfo.TryGetValue("AllowDBNull", out propValue))
                    {
                        if (propValue is bool)
                        {
                            column.AllowDBNull = (bool)propValue;
                        }
                        else
                            if (propValue is string)
                            {
                                column.AllowDBNull = (string)propValue == "true";
                            }
                    }

                    if (columnInfo.TryGetValue("AutoIncrement", out propValue))
                    {
                        if (propValue is bool)
                        {
                            column.AutoIncrement = (bool)propValue;
                        }
                        else
                            if (propValue is string)
                            {
                                column.AutoIncrement = (string)propValue == "true";
                            }
                    }

                    if (columnInfo.TryGetValue("AutoIncrementSeed", out propValue))
                    {
                        if (propValue is int)
                        {
                            column.AutoIncrementSeed = (long)((int)propValue);
                        }
                        else
                            if (propValue is long)
                            {
                                column.AutoIncrementSeed = (long)propValue;
                            }
                            else
                                if (propValue is string)
                                {
                                    column.AutoIncrementSeed = long.Parse((string)propValue, JaysonConstants.InvariantCulture);
                                }
                    }

                    if (columnInfo.TryGetValue("AutoIncrementStep", out propValue))
                    {
                        if (propValue is int)
                        {
                            column.AutoIncrementSeed = (long)((int)propValue);
                        }
                        else
                            if (propValue is long)
                            {
                                column.AutoIncrementStep = (long)propValue;
                            }
                            else
                                if (propValue is string)
                                {
                                    column.AutoIncrementStep = long.Parse((string)propValue, JaysonConstants.InvariantCulture);
                                }
                    }

                    if (columnInfo.TryGetValue("MaxLength", out propValue))
                    {
                        if (propValue is int)
                        {
                            column.MaxLength = (int)propValue;
                        }
                        else
                            if (propValue is long)
                            {
                                column.MaxLength = (int)((long)propValue);
                            }
                            else
                                if (propValue is string)
                                {
                                    column.MaxLength = int.Parse((string)propValue, JaysonConstants.InvariantCulture);
                                }
                    }

                    if (columnInfo.TryGetValue("Caption", out propValue))
                    {
                        column.Caption = (string)propValue;
                    }

                    if (columnInfo.TryGetValue("Prefix", out propValue))
                    {
                        column.Prefix = (string)propValue;
                    }

                    if (columnInfo.TryGetValue("ReadOnly", out propValue))
                    {
                        if (propValue is bool)
                        {
                            column.ReadOnly = (bool)propValue;
                        }
                        else
                            if (propValue is string)
                            {
                                column.ReadOnly = (string)propValue == "true";
                            }
                    }

                    if (columnInfo.TryGetValue("Unique", out propValue))
                    {
                        if (propValue is bool)
                        {
                            column.Unique = (bool)propValue;
                        }
                        else
                            if (propValue is string)
                            {
                                column.Unique = (string)propValue == "true";
                            }
                    }

                    if (columnInfo.TryGetValue("ExtendedProperties", out propValue))
                    {
                        SetExtendedProperties(column.ExtendedProperties, propValue, settings);
                    }
                }

                if (unordinalColumnList.Count > 0)
                {
                    int columnCount = ordinalColumnList.Length;
                    int unordColPos = unordinalColumnList.Count - 1;

                    for (int i = columnCount - 1; i > -1; i--)
                    {
                        if (ordinalColumnList[i] == null)
                        {
                            ordinalColumnList[i] = unordinalColumnList[unordColPos--];
                        }
                    }
                }

                foreach (var dataCol in ordinalColumnList)
                {
                    if (dataCol != null)
                    {
                        dataTable.Columns.Add(dataCol);
                    }
                }
            }
        }

        private static void SetDataTablePrimaryKey(IDictionary<string, object> obj, DataTable dataTable,
            JaysonDeserializationSettings settings)
        {
            object primaryKey;
            if (obj.TryGetValue("PrimaryKey", out primaryKey) && (primaryKey != null))
            {
                var currPrimaryKey = dataTable.PrimaryKey;
                if (currPrimaryKey == null || currPrimaryKey.Length == 0)
                {
                    var primaryKeyList = (IList)primaryKey;
                    if (primaryKeyList.Count > 0)
                    {
                        string columnName;
                        var columns = dataTable.Columns;
                        List<DataColumn> primaryKeyColumns = new List<DataColumn>();

                        foreach (var column in primaryKeyList)
                        {
                            columnName = (string)column;
                            if (columnName != null && columns.Contains(columnName))
                            {
                                primaryKeyColumns.Add(columns[columnName]);
                            }
                        }

                        if (primaryKeyColumns.Count > 0)
                        {
                            dataTable.PrimaryKey = primaryKeyColumns.ToArray();
                        }
                    }
                }
            }
        }

        private static void SetDataTableRows(IDictionary<string, object> obj, DataTable dataTable,
            JaysonDeserializationSettings settings)
        {
            object rowsObj;
            if (obj.TryGetValue("Rows", out rowsObj))
            {
				var rowsList = (IList)rowsObj;
                if (rowsList.Count > 0)
                {
                    int columnCount = dataTable.Columns.Count;
                    Type[] columnTypes = new Type[columnCount];

                    for (int i = 0; i < columnCount; i++)
                    {
                        columnTypes[i] = dataTable.Columns[i].DataType;
                    }

					IList rowList;
                    int itemCount;
                    object[] items;
                    Type columnType;
                    object rowValue;

                    foreach (var row in rowsList)
                    {
						rowList = (IList)row;

                        itemCount = rowList.Count;
                        items = new object[itemCount];

                        for (int i = 0; i < itemCount; i++)
                        {
                            rowValue = rowList[i];
                            columnType = columnTypes[i];

                            if (rowValue == null || rowValue.GetType() != columnType)
                            {
                                items[i] = ConvertObject(rowValue, columnType, settings);
                            }
                            else
                            {
                                items[i] = rowValue;
                            }
                        }

                        dataTable.Rows.Add(items);
                    }
                }
            }
        }

		private static void SetDataTable (IDictionary<string, object> obj, DataTable dataTable, 
			JaysonDeserializationSettings settings)
		{
			SetDataTableProperties (obj, dataTable, settings);
            SetDataTableColumns(obj, dataTable, settings);
            SetDataTablePrimaryKey(obj, dataTable, settings);
			SetDataTableRows (obj, dataTable, settings);
		}

        private static void SetDataRelations(IDictionary<string, object> obj, DataSet dataSet,
            JaysonDeserializationSettings settings)
        {
            object relationsObj;
            if (obj.TryGetValue("Relations", out relationsObj))
            {
				var relationsList = (IList)relationsObj;

                object propValue;
                string relationName;
                string tableName;
                string columnName;
                string tableNamespace;
                Dictionary<string, object> relationInfo;

                DataTable childTable;
                DataTable parentTable;
                DataColumnCollection columns;

                List<DataColumn> childColumns = new List<DataColumn>();
                List<DataColumn> parentColumns = new List<DataColumn>();

                foreach (var relationInfoObj in relationsList)
                {
                    relationInfo = (Dictionary<string, object>)relationInfoObj;

                    relationName = null;
                    if (relationInfo.TryGetValue("RelationName", out propValue) && propValue != null)
                    {
                        relationName = (string)propValue;
                        if (dataSet.Relations.Contains(relationName))
                            continue;

                        tableName = null;
                        if (relationInfo.TryGetValue("ChildTable", out propValue) && propValue != null)
                        {
                            tableName = (string)propValue;

                            tableNamespace = null;
                            if (relationInfo.TryGetValue("ChildTableNamespace", out propValue) && propValue != null)
                            {
                                tableNamespace = (string)propValue;
                            }

                            childTable = String.IsNullOrEmpty(tableNamespace) ? (dataSet.Tables.Contains(tableName) ? dataSet.Tables[tableName] : null) :
                                (dataSet.Tables.Contains(tableName, tableNamespace) ? dataSet.Tables[tableName, tableNamespace] : null);

                            if (childTable != null)
                            {
                                childColumns.Clear();
                                if (relationInfo.TryGetValue("ChildColumns", out propValue))
                                {
                                    columns = childTable.Columns;
                                    foreach (var columnNameObj in (IList)propValue)
                                    {
                                        columnName = (string)columnNameObj;
                                        if (columns.Contains(columnName))
                                        {
                                            childColumns.Add(columns[columnName]);
                                        }
                                    }
                                }

                                if (childColumns.Count > 0)
                                {
                                    tableName = null;
                                    if (relationInfo.TryGetValue("ParentTable", out propValue) && propValue != null)
                                    {
                                        tableName = (string)propValue;

                                        tableNamespace = null;
                                        if (relationInfo.TryGetValue("ParentTableNamespace", out propValue) && propValue != null)
                                        {
                                            tableNamespace = (string)propValue;
                                        }

                                        parentTable = String.IsNullOrEmpty(tableNamespace) ? (dataSet.Tables.Contains(tableName) ? dataSet.Tables[tableName] : null) :
                                            (dataSet.Tables.Contains(tableName, tableNamespace) ? dataSet.Tables[tableName, tableNamespace] : null);

                                        if (parentTable != null)
                                        {
                                            parentColumns.Clear();
                                            if (relationInfo.TryGetValue("ParentColumns", out propValue))
                                            {
                                                columns = parentTable.Columns;
                                                foreach (var columnNameObj in (IList)propValue)
                                                {
                                                    columnName = (string)columnNameObj;
                                                    if (columns.Contains(columnName))
                                                    {
                                                        parentColumns.Add(columns[columnName]);
                                                    }
                                                }
                                            }

                                            if (parentColumns.Count > 0)
                                            {
                                                dataSet.Relations.Add(relationName, parentColumns.ToArray(), childColumns.ToArray());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void SetDataSetProperties(IDictionary<string, object> obj, DataSet dataSet, 
            JaysonDeserializationSettings settings)
        {
            object propValue;
            if (obj.TryGetValue("CaseSensitive", out propValue))
            {
                if (propValue is bool)
                {
                    dataSet.CaseSensitive = (bool)propValue;
                }
                else
                    if (propValue is string)
                    {
                        dataSet.CaseSensitive = (string)propValue == "true";
                    }
            }

            if (obj.TryGetValue("DataSetName", out propValue))
            {
                dataSet.DataSetName = (string)propValue;
            }

            if (obj.TryGetValue("EnforceConstraints", out propValue))
            {
                if (propValue is bool)
                {
                    dataSet.EnforceConstraints = (bool)propValue;
                }
                else
                    if (propValue is string)
                    {
                        dataSet.EnforceConstraints = (string)propValue == "true";
                    }
            }

            if (obj.TryGetValue("Locale", out propValue))
            {
                dataSet.Locale = CultureInfo.GetCultureInfo((string)propValue);
            }

            if (obj.TryGetValue("Namespace", out propValue))
            {
                dataSet.Namespace = (string)propValue;
            }

            if (obj.TryGetValue("Prefix", out propValue))
            {
                dataSet.Prefix = (string)propValue;
            }

            if (obj.TryGetValue("SchemaSerializationMode", out propValue))
            {
                if (propValue is SchemaSerializationMode)
                {
                    dataSet.SchemaSerializationMode = (SchemaSerializationMode)propValue;
                }
                else
                    if (propValue is string)
                    {
                        dataSet.SchemaSerializationMode = (SchemaSerializationMode)JaysonEnumCache.Parse((string)propValue, typeof(SchemaSerializationMode));
                    }
            }

            if (obj.TryGetValue("ExtendedProperties", out propValue))
            {
                SetExtendedProperties(dataSet.ExtendedProperties, propValue, settings);
            }
        }

        private static bool GetTableName(IDictionary<string, object> tableObj, out string tableName, out string tableNamespace)
        {
            tableName = null;
            tableNamespace = null;

            object propValue;

            if (tableObj.TryGetValue("TableName", out propValue))
            {
                tableName = (string)propValue;
                if (tableObj.TryGetValue("Namespace", out propValue))
                {
                    tableNamespace = (string)propValue;
                }
                return true;
            }
            return false;
        }

        private static void SetDataSet(IDictionary<string, object> obj, DataSet dataSet,
            JaysonDeserializationSettings settings)
        {
            SetDataSetProperties(obj, dataSet, settings);

            object tablesObj;
            if (obj.TryGetValue("Tables", out tablesObj))
            {
                var tableList = (IList)tablesObj;
                if (tableList.Count > 0)
                {
                    DataTable dataTable;
                    string tableName;
                    string tableNamespace;
                    IDictionary<string, object> table;

                    foreach (var tableObj in tableList)
                    {
                        dataTable = null;
                        table = tableObj as IDictionary<string, object>;

                        if (GetTableName(table, out tableName, out tableNamespace))
                        {
                            if (String.IsNullOrEmpty(tableNamespace))
                            {
                                if (dataSet.Tables.Contains(tableName))
                                {
                                    dataTable = dataSet.Tables[tableName];
                                }
                            }
                            else if (dataSet.Tables.Contains(tableName, tableNamespace))
                            {
                                dataTable = dataSet.Tables[tableName, tableNamespace];
                            }
                        }

                        if (dataTable != null)
                        {
                            SetDataTable(table, dataTable, settings);
                        }
                        else
                        {
                            dataTable = ConvertObject(tableObj, typeof(DataTable), settings) as DataTable;
                            if (dataTable != null)
                            {
                                if (String.IsNullOrEmpty(dataTable.Namespace))
                                {
                                    if (!dataSet.Tables.Contains(dataTable.TableName))
                                    {
                                        dataSet.Tables.Add(dataTable);
                                    }
                                }
                                else if (!dataSet.Tables.Contains(dataTable.TableName, dataTable.Namespace))
                                {
                                    dataSet.Tables.Add(dataTable);
                                }
                            }
                        }
                    }
                }
            }
            SetDataRelations(obj, dataSet, settings);
        }

        # endregion Convert DataTable & DataSet

        private static void SetDictionary(IDictionary<string, object> obj, object instance,
            JaysonDeserializationSettings settings)
        {
            if (instance == null || obj == null || obj.Count == 0 || instance is DBNull)
            {
                return;
            }

            bool hasStype = obj.ContainsKey("$type");

            if (instance is IDictionary<string, object>)
            {
                IDictionary<string, object> instanceDict = (IDictionary<string, object>)instance;

                string key;
                foreach (var entry in obj)
                {
                    key = entry.Key;
                    if (!hasStype || key != "$type")
                    {
                        instanceDict[key] = ConvertObject(entry.Value, typeof(object), settings);
                    }
                }
            }
            else if (instance is IDictionary)
            {
                IDictionary instanceDict = (IDictionary)instance;

                object key;
                Type[] genArgs = JaysonCommon.GetGenericDictionaryArgs(instance.GetType());

                if (genArgs != null)
                {
                    bool changeValue = hasStype || (genArgs[1] != typeof(object));
                    bool changeKey = !(genArgs[0] == typeof(object) || genArgs[0] == typeof(string));

                    Type keyType = genArgs[0];
                    Type valType = genArgs[1];

                    foreach (var entry in obj)
                    {
                        if (!hasStype || entry.Key != "$type")
                        {
                            key = changeKey ? ConvertObject(entry.Key, keyType, settings) : entry.Key;
                            instanceDict[key] = !changeValue ? entry.Value : ConvertObject(entry.Value, valType, settings);
                        }
                    }
                    return;
                }

                foreach (var entry in obj)
                {
                    if (!hasStype || entry.Key != "$type")
                    {
                        instanceDict[entry.Key] = ConvertObject(entry.Value, typeof(object), settings);
                    }
                }
            }
            else if (instance is NameValueCollection)
            {
                NameValueCollection nvcollection = (NameValueCollection)instance;

                string key;
                object value;
                Type valueType;
                foreach (var item in obj)
                {
                    key = item.Key;
                    if (!hasStype || key != "$type")
                    {
                        value = item.Value;
                        if (value == null || value is string)
                        {
                            nvcollection.Add(key, (string)value);
                        }
                        else
                        {
                            valueType = value.GetType();
                            if (JaysonTypeInfo.IsJPrimitive(valueType))
                            {
                                nvcollection.Add(key, JaysonFormatter.ToString(value, valueType));
                            }
                            else if (value is IFormattable)
                            {
                                nvcollection.Add(key, ((IFormattable)value).ToString(null, JaysonConstants.InvariantCulture));
                            }
                            else
                            {
                                nvcollection.Add(key, ToJsonString(value));
                            }
                        }
                    }
                }
            }
            else if (instance is StringDictionary)
            {
                StringDictionary sidic = (StringDictionary)instance;

                string key;
                object value;
                Type valueType;
                foreach (var item in obj)
                {
                    key = item.Key;
                    if (!hasStype || key != "$type")
                    {
                        value = item.Value;
                        if (value == null || value is string)
                        {
                            sidic.Add(key, (string)value);
                        }
                        else
                        {
                            valueType = value.GetType();
                            if (JaysonTypeInfo.IsJPrimitive(valueType))
                            {
                                sidic.Add(key, JaysonFormatter.ToString(value, valueType));
                            }
                            else if (value is IFormattable)
                            {
                                sidic.Add(key, ((IFormattable)value).ToString(null, JaysonConstants.InvariantCulture));
                            }
                            else
                            {
                                sidic.Add(key, ToJsonString(value));
                            }
                        }
                    }
                }
            }
            else if (instance is DataTable)
            {
                SetDataTable(obj, (DataTable)instance, settings);
            }
            else if (instance is DataSet)
            {
                SetDataSet(obj, (DataSet)instance, settings);
            }
            else
            {
                Type instanceType = instance.GetType();
                JaysonTypeOverride typeOverride = settings.GetTypeOverride(instanceType);

                if (JaysonTypeInfo.IsAnonymous(instanceType))
                {
                    if (settings.IgnoreAnonymousTypes)
                    {
                        return;
                    }

                    bool caseSensitive = settings.CaseSensitive;
                    bool raiseErrorOnMissingMember = settings.RaiseErrorOnMissingMember;

                    IJaysonFastMember member;
                    IDictionary<string, IJaysonFastMember> members =
                        JaysonFastMemberCache.GetAllFieldMembers(instanceType, caseSensitive);

                    string key;
                    string memberName;
                    object memberValue;

                    foreach (var entry in obj)
                    {
                        if (!hasStype || entry.Key != "$type")
                        {
                            key = entry.Key;
                            if (!hasStype || key != "$type")
                            {
                                memberName = "<" + (caseSensitive ? key : key.ToLower(JaysonConstants.InvariantCulture)) + ">";
                                if (!members.TryGetValue(memberName, out member) && typeOverride != null)
                                {
                                    key = typeOverride.GetAliasMember(key);
                                    if (!String.IsNullOrEmpty(key))
                                    {
                                        memberName = "<" + (caseSensitive ? key : key.ToLower(JaysonConstants.InvariantCulture)) + ">";
                                        members.TryGetValue(memberName, out member);
                                    }
                                }

                                if (member != null)
                                {
                                    if (typeOverride == null || !typeOverride.IsMemberIgnored(memberName))
                                    {
                                        if (member.CanWrite)
                                        {
                                            member.Set(instance, ConvertObject(entry.Value, member.MemberType, settings));
                                        }
                                        else if (entry.Value != null)
                                        {
                                            memberValue = member.Get(instance);
                                            if (instance != null)
                                            {
                                                SetObject(memberValue, entry.Value, settings);
                                            }
                                        }
                                    }
                                }
                                else if (raiseErrorOnMissingMember)
                                {
                                    throw new JaysonException("Missing member: " + entry.Key);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Type[] genArgs = JaysonCommon.GetGenericDictionaryArgs(instanceType);
                    if (genArgs != null)
                    {
                        Action<object, object[]> addMethod = JaysonCommon.GetIDictionaryAddMethod(instanceType);
                        if (addMethod != null)
                        {
                            bool changeVal = genArgs[1] != typeof(object);
                            bool changeKey = !(genArgs[0] == typeof(object) || genArgs[0] == typeof(string));

                            object key;
                            object value;

                            Type keyType = genArgs[0];
                            Type valType = genArgs[1];

                            foreach (var entry in obj)
                            {
                                if (!hasStype || entry.Key != "$type")
                                {
                                    key = changeKey ? ConvertObject(entry.Key, keyType, settings) : entry.Key;
                                    value = changeVal ? ConvertObject(entry.Value, valType, settings) : entry.Value;

                                    addMethod(instance, new object[] { key, value });
                                }
                            }
                            return;
                        }
                    }

                    if (!JaysonTypeInfo.IsJPrimitive(instanceType))
                    {
                        bool caseSensitive = settings.CaseSensitive;
                        bool raiseErrorOnMissingMember = settings.RaiseErrorOnMissingMember;

                        IJaysonFastMember member;
                        IDictionary<string, IJaysonFastMember> members = 
                            JaysonFastMemberCache.GetMembers(instanceType, caseSensitive);

                        string memberName;
                        object memberValue;

                        foreach (var entry in obj)
                        {
                            memberName = caseSensitive ? entry.Key : entry.Key.ToLower(JaysonConstants.InvariantCulture);
                            if (!hasStype || memberName != "$type")
                            {
                                if (!members.TryGetValue(memberName, out member) && typeOverride != null)
                                {
                                    memberName = typeOverride.GetAliasMember(memberName);
                                    if (!String.IsNullOrEmpty(memberName))
                                    {
                                        if (!caseSensitive)
                                        {
                                            memberName = memberName.ToLower(JaysonConstants.InvariantCulture);
                                        }
                                        members.TryGetValue(memberName, out member);
                                    }
                                }

                                if (member != null)
                                {
                                    if (typeOverride == null || !typeOverride.IsMemberIgnored(memberName))
                                    {
                                        if (member.CanWrite)
                                        {
                                            member.Set(instance, ConvertObject(entry.Value, member.MemberType, settings));
                                        }
                                        else if (entry.Value != null)
                                        {
                                            memberValue = member.Get(instance);
                                            if (instance != null)
                                            {
                                                SetObject(memberValue, entry.Value, settings);
                                            }
                                        }
                                    }
                                }
                                else if (raiseErrorOnMissingMember)
                                {
                                    throw new JaysonException("Missing member: " + entry.Key);
                                }
                            }
                        }
                    }
                }
            }
        }

		private static void SetList(IList<object> obj, object instance, JaysonDeserializationSettings settings)
		{
            if (instance == null || obj == null || obj.Count == 0 || instance is DataTable || 
                instance is DataSet || instance is DBNull)
			{
				return;
			}

            Type instanceType = instance.GetType();
            if (instanceType.IsArray)
            {
                Array aResult = (Array)instance;
                Type arrayType = JaysonTypeInfo.GetElementType(instanceType);

                int count = obj.Count;
                for (int i = 0; i < count; i++)
                {
                    aResult.SetValue(ConvertObject(obj[i], arrayType, settings), i);
                }

                return;
            }

            if (instance is IList)
            {
				int count = obj.Count;
                IList lResult = (IList)instance;

                Type argType = JaysonCommon.GetGenericListArgs(instanceType);
                if (argType != null)
                {
                    for (int i = 0; i < count; i++)
                    {
                        lResult.Add(ConvertObject(obj[i], argType, settings));
                    }
					return;
                }

				object item;
                for (int i = 0; i < count; i++)
                {
                    item = obj[i];
                    if (item is IDictionary<string, object>)
                    {
                        item = ConvertDictionary((IDictionary<string, object>)item, typeof(object), settings);
                    }
                    else if (item is IList<object>)
                    {
                        item = ConvertList((IList<object>)item, typeof(object), settings);
                    }

                    lResult.Add(item);
                }
            }
            else
            {
                Type argType = JaysonCommon.GetGenericCollectionArgs(instanceType);
                if (argType != null)
                {
                    Action<object, object[]> methodInfo = JaysonCommon.GetICollectionAddMethod(instanceType);
                    if (methodInfo != null)
                    {
                        int count = obj.Count;
                        for (int i = 0; i < count; i++)
                        {
                            methodInfo(instance, new object[] { ConvertObject(obj[i], argType, settings) });
                        }
                    }
                }
            }
		}

		private static void SetObject(object obj, object instance, JaysonDeserializationSettings settings)
		{
			if (instance != null && obj != null)
			{
				Type instanceType = instance.GetType();
				if (!JaysonTypeInfo.IsJPrimitive(instanceType))
				{
					if (obj is IDictionary<string, object>)
					{
						SetDictionary((IDictionary<string, object>)obj, instance, settings);
					}
					else if (obj is IList<object>)
					{
						SetList((IList<object>)obj, instance, settings);
					}
				}
			}
		}

		private static Type EvaluateListType(Type type, out bool asList, out bool asArray, out bool asReadOnly)
		{
			asList = false;
			asArray = false;
			asReadOnly = false;

			var info = JaysonTypeInfo.GetTypeInfo(type);
			if (info.Array) {
				asArray = true;
				return type;
			}

			if (type == typeof(List<object>)) {
				asList = true;
				return type;
			}

			if (type == typeof(IList) || type == typeof(ArrayList)) {
				asList = true;
				return typeof(ArrayList);
			}

			if (!info.Generic) {
				return type;
			} 

			var genericType = info.GenericTypeDefinitionType;

			if (genericType == typeof(ReadOnlyCollection<>)) {
				asList = true;
				asReadOnly = true;
				return GetGenericUnderlyingList(info);
			} 

			if (info.Interface) {
				#if !(NET4000 || NET3500 || NET3000 || NET2000)
				if (genericType == typeof(IList<>) ||
					genericType == typeof(ICollection<>) || 
					genericType == typeof(IEnumerable<>) ||
					genericType == typeof(IReadOnlyCollection<>)) {
					asList = true;
					return GetGenericUnderlyingList(info);
				}
				#else
				if (genericType == typeof(IList<>) ||
					genericType == typeof(ICollection<>) || 
					genericType == typeof(IEnumerable<>)) {
					asList = true;
					return GetGenericUnderlyingList(info);
				}
				#endif
			}
			return type;
		}

        private static Type GetEvaluatedListType(Type type, out bool asList, out bool asArray, out bool asReadOnly)
        {
            EvaluatedListType elt;
            if (!s_EvaluatedListTypeCache.TryGetValue(type, out elt))
            {
                elt = new EvaluatedListType();
                elt.EvaluatedType = EvaluateListType(type, out asList, out asArray, out asReadOnly);
                elt.AsList = asList;
                elt.AsArray = asArray;
                elt.AsReadOnly = asReadOnly;
                s_EvaluatedListTypeCache[type] = elt;

                return elt.EvaluatedType;
            }

            asList = elt.AsList;
            asArray = elt.AsArray;
            asReadOnly = elt.AsReadOnly;

            return elt.EvaluatedType;
        }

		private static Type EvaluateDictionaryType(Type type, out bool asDictionary, out bool asReadOnly)
		{
			asReadOnly = false;
			asDictionary = false;
			if (type == typeof(Dictionary<string, object>)) {
				asDictionary = true;
				return type;
			}

			if (type == typeof(IDictionary) || type == typeof(Hashtable)) {
				asDictionary = true;
				return typeof(Hashtable);
			}

			var info = JaysonTypeInfo.GetTypeInfo(type);
			if (!info.Generic) {
				return type;
			} 

			var genericType = info.GenericTypeDefinitionType;

			#if !(NET4000 || NET3500 || NET3000 || NET2000)
			if (genericType == typeof(ReadOnlyDictionary<,>)) {
				asReadOnly = true;
				asDictionary = true;
				return GetGenericUnderlyingDictionary(info);
			} 
			#endif

			if (info.Interface) {
				#if !(NET4000 || NET3500 || NET3000 || NET2000)
				if (genericType == typeof(IDictionary<,>) ||
					genericType == typeof(IReadOnlyDictionary<,>)) {
					asDictionary = true;
					return GetGenericUnderlyingDictionary(info);
				} 
				#else
				if (genericType == typeof(IDictionary<,>)) {
					asDictionary = true;
					return GetGenericUnderlyingDictionary(info);
				} 
				#endif

				if (genericType == typeof(ICollection<>) || genericType == typeof(IEnumerable<>)) {
					Type firstArgType = info.GenericArguments[0];

					if (firstArgType == typeof(KeyValuePair<string,object>) ||
						firstArgType == typeof(KeyValuePair<object,object>)) {
						asDictionary = true;
						return GetGenericUnderlyingDictionary(info);
					}

					var argInfo = JaysonTypeInfo.GetTypeInfo(firstArgType);
					if (argInfo.Generic && argInfo.GenericTypeDefinitionType == typeof(KeyValuePair<,>)) {
						asDictionary = true;
						return GetGenericUnderlyingDictionary(info);
					}
				}
			}
			return type;
		}

        private static Type GetEvaluatedDictionaryType(Type type, out bool asDictionary, out bool asReadOnly)
        {
            EvaluatedDictionaryType edt;
            if (!s_EvaluatedDictionaryTypeCache.TryGetValue(type, out edt))
            {
                edt = new EvaluatedDictionaryType();
                edt.EvaluatedType = EvaluateDictionaryType(type, out asDictionary, out asReadOnly);
                edt.AsDictionary = asDictionary;
                edt.AsReadOnly = asReadOnly;
                s_EvaluatedDictionaryTypeCache[type] = edt;
                
                return edt.EvaluatedType;
            }

            asDictionary = edt.AsDictionary;
            asReadOnly = edt.AsReadOnly;
            
            return edt.EvaluatedType;
        }

		private static object ConvertDictionary(IDictionary<string, object> obj, Type toType, 
			JaysonDeserializationSettings settings, bool forceType = false)
		{
			object Stype;
			bool binded = false;

			JaysonTypeInfo info = null;
			if (obj.TryGetValue ("$type", out Stype) && Stype != null) {
				string typeName = Stype as string;
				if (typeName != null) {
					info = JaysonTypeInfo.GetTypeInfo (toType);
					if (typeName != String.Empty) {
						if (!forceType || toType == typeof(object) || info.Interface) {
							binded = true;
							Type instanceType = GetTypeOverride(JaysonCommon.GetType (typeName, settings.Binder), settings);
                            if (instanceType != null)
                            {
                                if (instanceType != null && instanceType != toType)
                                {
                                    toType = instanceType;
                                    info = JaysonTypeInfo.GetTypeInfo(toType);
                                }
                            }
						}
					}

					if (settings.IgnoreAnonymousTypes && info.Anonymous) {
						return null;
					}

					object Svalues;
					if (obj.TryGetValue ("$value", out Svalues)) {
						return ConvertObject (Svalues, toType, settings);
					}

					if (obj.TryGetValue ("$values", out Svalues) && (Svalues is IList<object>)) {
						return ConvertList ((IList<object>)Svalues, toType, settings);
					}
				}
			} 

			if (obj.TryGetValue ("$datatype", out Stype) && Stype != null) {
				string dataType = Stype as string;
				if (dataType != null) {
					if (dataType.Equals ("DataTable", StringComparison.OrdinalIgnoreCase)) {
						if (!typeof(DataTable).IsAssignableFrom (toType)) {
							toType = typeof(DataTable);
						}
					} else if (dataType.Equals ("DataSet", StringComparison.OrdinalIgnoreCase)) {
						if (!typeof(DataSet).IsAssignableFrom (toType)) {
							toType = typeof(DataSet);
						}
					}
				}
			}

			bool asReadOnly, asDictionary;
            toType = GetEvaluatedDictionaryType(toType, out asDictionary, out asReadOnly);

			if (!binded) {
				Type instanceType = BindToType(settings, toType, forceType);
				if (instanceType != null && instanceType != toType) {
					toType = instanceType;

					info = JaysonTypeInfo.GetTypeInfo (toType);
					if (settings.IgnoreAnonymousTypes && info.Anonymous) {
						return null;
					}
				}
			}

            object result;
            if (toType != typeof(object))
            {
				if (settings.ObjectActivator == null) {
					result = JaysonObjectConstructor.New(toType);
				} else {
					bool useDefaultCtor;
					result = settings.ObjectActivator (toType, obj, out useDefaultCtor);
					if (useDefaultCtor) {
						result = JaysonObjectConstructor.New(toType);
					}
				} 
            }
            else
            {
				#if !(NET3500 || NET3000 || NET2000)
                if (settings.DictionaryType == DictionaryDeserializationType.Expando)
                {
                    result = new ExpandoObject();
                }
                else
                {
                    result = new Dictionary<string, object>(10);
                }
				#else
				result = new Dictionary<string, object>(10);
				#endif
            }
            
            SetDictionary(obj, result, settings);

			#if !(NET4000 || NET3500 || NET3000 || NET2000)
			if (asReadOnly) {
                return GetReadOnlyDictionaryActivator(toType)(new object[] { result });
            }
			#endif

			return result;
		}

		private static Type GetTypeOverride (Type type, JaysonDeserializationSettings settings)
		{
			if (type != null) 
			{
				var typeOverride = settings.GetTypeOverride (type);
				if (typeOverride != null) 
				{
					return typeOverride.BindToType ?? type;
				}
			}
			return type;
		}

		private static Type BindToType (JaysonDeserializationSettings settings, Type type, bool dontOverride = false)
		{
			#if !(NET3500 || NET3000 || NET2000)
			if (settings.Binder != null)
			{
				string typeName;
				string assemblyName;			
				Type instanceType = null;

				settings.Binder.BindToName (type, out assemblyName, out typeName);
				if (String.IsNullOrEmpty (typeName)) 
				{
					instanceType = settings.Binder.BindToType (type.Assembly.FullName, type.FullName);
				} else {
					instanceType = settings.Binder.BindToType (assemblyName, typeName);
				}

				if (instanceType != null) 
				{
					return instanceType;
				}
			}

			if (!dontOverride)
			{
				return GetTypeOverride (type, settings);
			}
			#endif
			return type;
		}

		private static object ConvertList(IList<object> obj, Type toType, JaysonDeserializationSettings settings)
		{
			if (toType == typeof(byte[]) && obj.Count == 1) 
			{
				object item = obj [0];
				if (item is string) 
				{
					return Convert.FromBase64String((string)item);
				}
			}

			toType = BindToType(settings, toType);

			if (toType == typeof(object)) {
				if (toType == typeof(object)) {
					switch (settings.ArrayType) {
					case ArrayDeserializationType.ArrayList:
						toType = typeof(ArrayList);
						break;
					case ArrayDeserializationType.Array:
					case ArrayDeserializationType.ArrayDefined:
						toType = typeof(object[]);
						break;
					default:
						toType = typeof(List<object>);
						break;
					}
				}
				toType = typeof(List<object>);
			}

			bool asList, asArray, asReadOnly;
            Type listType = GetEvaluatedListType(toType, out asList, out asArray, out asReadOnly);

			object result;
			if (JaysonTypeInfo.IsArray(listType)) 
			{
				result = Array.CreateInstance (listType, obj.Count);
			} 
			else if (settings.ObjectActivator == null) {
				result = JaysonObjectConstructor.New(listType);
			} else {
				bool useDefaultCtor;
				result = settings.ObjectActivator (listType, null, out useDefaultCtor);
				if (useDefaultCtor) {
					result = JaysonObjectConstructor.New(listType);
				}
			} 

			SetList(obj, result, settings);
			if (result == null) {
                return result;
            }

            Type resultType = result.GetType();
			if (toType == resultType || 
                toType.IsAssignableFrom(resultType)) {
				return result;
			}

			if (asReadOnly) {
                return GetReadOnlyCollectionActivator(toType)(new object[] { result });
			}

			return result;
		}
				        
        private static object ConvertObject(object obj, Type toType, JaysonDeserializationSettings settings)
		{
			toType = BindToType(settings, toType);
			if (toType == typeof(object))
			{
				if (obj != null) {
					if (obj is IDictionary<string, object>) {
						obj = ConvertDictionary ((IDictionary<string, object>)obj, toType, settings);
					} else if (obj is IList<object>) {
						obj = ConvertList ((IList<object>)obj, toType, settings);
					}
				}

				return obj;
			}

			if (obj == null)
			{
				var info = JaysonTypeInfo.GetTypeInfo(toType);
                if (info.Class || info.Nullable)
				{
					return null;
				}

				if (settings.ObjectActivator == null) {
					return JaysonObjectConstructor.New(toType);
				} else {
					bool useDefaultCtor;
					object result = settings.ObjectActivator (toType, null, out useDefaultCtor);
					if (useDefaultCtor) {
						return JaysonObjectConstructor.New(toType);
					}
					return result;
				} 
			}

			Type objType = obj.GetType();
			if (toType == objType || toType.IsAssignableFrom(objType))
			{
				return obj;
			}

			if (toType == typeof(string))
			{
				if (obj is IFormattable)
				{
					return ((IFormattable)obj).ToString(null, JaysonConstants.InvariantCulture);
				}
				if (obj is IConvertible)
				{
					return ((IConvertible)obj).ToString(JaysonConstants.InvariantCulture);
				}
				return obj.ToString();
			}

			if (obj is IDictionary<string, object>)
			{
				return ConvertDictionary((IDictionary<string, object>)obj, toType, settings);
			}

			if (obj is IList<object>)
			{
				return ConvertList((IList<object>)obj, toType, settings);
			}

			var tInfo = JaysonTypeInfo.GetTypeInfo(toType);
			if (!tInfo.Class)
			{
				bool converted;

				JaysonTypeCode jtc = tInfo.JTypeCode;
				if (jtc == JaysonTypeCode.DateTime)
                {
					return JaysonCommon.TryConvertDateTime (obj, settings.DateTimeFormat, settings.DateTimeZoneType);
                }

				if (jtc == JaysonTypeCode.DateTimeNullable)
				{
					DateTime dt = JaysonCommon.TryConvertDateTime (obj, settings.DateTimeFormat, settings.DateTimeZoneType);
					if (dt == default(DateTime)) {
						return null;
					}
					return (DateTime?)dt;
				}

				object result = JaysonCommon.ConvertToPrimitive(obj, toType, out converted);
				if (converted)
				{
					return result;
				}

				if (JaysonTypeInfo.IsNullable(toType))
				{
					Type argType = JaysonTypeInfo.GetGenericArguments(toType)[0];
					object value = ConvertObject(obj, argType, settings);

					var constructor = toType.GetConstructor(new Type[] { argType });
					if (constructor != null)
					{
						value = constructor.Invoke(new object[] { value });
					}

					return value;
				}
			}

			if (toType == typeof(byte[]) && objType == typeof(string))
			{
				return Convert.FromBase64String((string)obj);
			}

            if (settings.IgnoreAnonymousTypes && JaysonTypeInfo.IsAnonymous(toType))
            {
                return null;
            }

			#if !(NET3500 || NET3000 || NET2000)
			if (settings.Binder != null)
			{
				string typeName;
				string assemblyName;

				settings.Binder.BindToName(toType, out assemblyName, out typeName);
				if (!String.IsNullOrEmpty(typeName))
				{
					Type instanceType = settings.Binder.BindToType(assemblyName, typeName);
					if (instanceType != null)
					{
						toType = instanceType;
					}
				}
			}
			#endif

			if (settings.ObjectActivator == null) {
				return JaysonObjectConstructor.New(toType);
			} else {
				bool useDefaultCtor;
				object result = settings.ObjectActivator (toType, null, out useDefaultCtor);
				if (useDefaultCtor) {
					return JaysonObjectConstructor.New(toType);
				}
				return result;
			} 
		}

		# endregion Convert

		# region ToX

		public static IDictionary<string, object> ToDictionary(string str, JaysonDeserializationSettings settings = null)
		{
			if (String.IsNullOrEmpty(str))
			{
				throw new JaysonException("Empty string.");
			}

			if (settings == null) {
				settings = new JaysonDeserializationSettings ();
				JaysonDeserializationSettings.Default.AssignTo (settings);
				settings.DictionaryType = DictionaryDeserializationType.Dictionary;
			}

			return ParseDictionary(new JaysonDeserializationContext {
				Text = str,
				Length = str.Length,
				Settings = settings ?? JaysonDeserializationSettings.Default
			});
		}

		public static T ToObject<T>(string str, JaysonDeserializationSettings settings = null)
		{
			if (String.IsNullOrEmpty(str))
			{
				return default(T);
			}
			return (T)ToObject(str, typeof(T), settings);
		}

		public static object ToObject(string str, JaysonDeserializationSettings settings = null)
		{
			return ToObject(str, typeof(object), settings);
		}

        public static object ToObject(string str, Type toType, JaysonDeserializationSettings settings = null)
        {
            if (String.IsNullOrEmpty(str))
            {
                return JaysonTypeInfo.GetDefault(toType);
            }

            var context = new JaysonDeserializationContext
            {
                Text = str,
                Length = str.Length,
                Position = 0,
                Settings = settings ?? JaysonDeserializationSettings.Default
            };

            object result = Parse(context);
            if (result == null)
            {
                return JaysonTypeInfo.GetDefault(toType);
            }

            var instanceType = result.GetType();

            if (!context.HasTypeInfo)
            {
                if (toType == instanceType ||
                    toType == typeof(object) ||
                    toType.IsAssignableFrom(instanceType))
                {
                    return result;
                }
                return ConvertObject(result, toType, context.Settings);
            }

            var toInfo = JaysonTypeInfo.GetTypeInfo(toType);
            var instanceInfo = JaysonTypeInfo.GetTypeInfo(instanceType);

            if (toInfo.JPrimitive)
            {
                if (toInfo.Type == instanceInfo.Type)
                {
                    return result;
                }

                if (context.HasTypeInfo)
                {
                    IDictionary<string, object> primeDict = result as IDictionary<string, object>;
                    if (primeDict != null)
                    {
                        return ConvertDictionary(primeDict, toType,
                            settings ?? JaysonDeserializationSettings.Default, true);
                    }
                }

                bool converted;
                return JaysonCommon.ConvertToPrimitive(result, toType, out converted);
            }

            bool asReadOnly = false;
            if (result is IList<object>)
            {
                bool asList, asArray;
                Type listType = GetEvaluatedListType(toType, out asList, out asArray, out asReadOnly);

                result = ConvertList((IList<object>)result, listType, context.Settings);
                if (result == null)
                {
                    return result;
                }

                Type resultType = result.GetType();
                if (toType == resultType ||
                    toType.IsAssignableFrom(resultType))
                {
                    return result;
                }

                if (asReadOnly)
                {
                    return GetReadOnlyCollectionActivator(toType)(new object[] { result });
                }
                return result;
            }

            if (result is IDictionary<string, object>)
            {
                bool asDictionary;
                var dictionaryType = GetEvaluatedDictionaryType(toType, out asDictionary, out asReadOnly);
                result = ConvertDictionary((IDictionary<string, object>)result, dictionaryType, context.Settings, true);
            }
            else if (result is IDictionary)
            {
                bool asDictionary;
                var dictionaryType = GetEvaluatedDictionaryType(toType, out asDictionary, out asReadOnly);
                result = ConvertDictionary((IDictionary<string, object>)result, dictionaryType, context.Settings);
            }
            else if (result is IList)
            {
                result = ConvertList((IList<object>)result, toType, context.Settings);
            }
            else
            {
                result = ConvertObject(result, toType, context.Settings);
            }

            if (result == null)
            {
                return result;
            }

            Type resultTypeD = result.GetType();
            if (toType == resultTypeD ||
                toType.IsAssignableFrom(resultTypeD))
            {
                return result;
            }

            #if !(NET4000 || NET3500 || NET3000 || NET2000)
            if (asReadOnly)
            {
                return GetReadOnlyDictionaryActivator(toType)(new object[] { result });
            }
            #endif

            throw new JaysonException("Unable to cast result to expected type.");
        }

		#if !(NET3500 || NET3000 || NET2000)
		public static dynamic ToDynamic(string str)
		{
			return ToExpando(str);
		}

		public static ExpandoObject ToExpando(string str, JaysonDeserializationSettings settings = null)
		{
			if (settings == null) {
				settings = new JaysonDeserializationSettings ();
				JaysonDeserializationSettings.Default.AssignTo (settings);
				settings.DictionaryType = DictionaryDeserializationType.Expando;
			} else if (settings.DictionaryType != DictionaryDeserializationType.Expando) {
				var temp = new JaysonDeserializationSettings ();
				settings.AssignTo (temp);
				temp = settings;
				settings.DictionaryType = DictionaryDeserializationType.Expando;
			}

			return (ExpandoObject)ToDictionary(str, settings);
		}
		#endif

		# endregion ToX
	}

	# endregion JsonConverter
}
