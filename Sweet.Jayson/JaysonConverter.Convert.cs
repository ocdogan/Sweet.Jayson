﻿# region License
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
#if !(NET3500 || NET3000 || NET2000)
using System.Collections.Concurrent;
#endif
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

        private static readonly JaysonSynchronizedDictionary<Type, ConstructorInfo> s_CtorCache = new JaysonSynchronizedDictionary<Type, ConstructorInfo>();
        private static readonly JaysonSynchronizedDictionary<Type, Func<object[], object>> s_ActivatorCache = new JaysonSynchronizedDictionary<Type, Func<object[], object>>();
        private static readonly JaysonSynchronizedDictionary<Type, EvaluatedListType> s_EvaluatedListTypeCache = new JaysonSynchronizedDictionary<Type, EvaluatedListType>();
        private static readonly JaysonSynchronizedDictionary<Type, EvaluatedDictionaryType> s_EvaluatedDictionaryTypeCache = new JaysonSynchronizedDictionary<Type, EvaluatedDictionaryType>();
        private static readonly JaysonSynchronizedDictionary<Type, Type> s_GenericUnderlyingCache = new JaysonSynchronizedDictionary<Type, Type>();

        private static readonly JaysonCtorParamMatcher DefaultMatcher = CtorParamMatcher;

        # endregion Static Members

        # region Convert

        private static Type GetGenericUnderlyingList(JaysonTypeInfo info)
        {
            Type result;
            if (!s_GenericUnderlyingCache.TryGetValue(info.Type, out result))
            {
                lock (((ICollection)s_GenericUnderlyingCache).SyncRoot)
                {
                    if (!s_GenericUnderlyingCache.TryGetValue(info.Type, out result))
                    {
                        var argTypes = info.GenericArguments;
                        if (argTypes[0] == typeof(object))
                        {
                            result = typeof(List<object>);
                        }
                        else
                        {
                            result = typeof(List<>).MakeGenericType(argTypes);
                        }

                        s_GenericUnderlyingCache[info.Type] = result;
                    }
                }
            }
            return result;
        }

        private static Type GetGenericUnderlyingDictionary(JaysonTypeInfo info)
        {
            Type result;
            if (!s_GenericUnderlyingCache.TryGetValue(info.Type, out result))
            {
                lock (((ICollection)s_GenericUnderlyingCache).SyncRoot)
                {
                    if (!s_GenericUnderlyingCache.TryGetValue(info.Type, out result))
                    {
                        var argTypes = info.GenericArguments;
                        if (argTypes[0] == typeof(string) && argTypes[1] == typeof(object))
                        {
                            result = typeof(Dictionary<string, object>);
                        }
                        else
                        {
                            result = typeof(Dictionary<,>).MakeGenericType(argTypes);
                        }

                        s_GenericUnderlyingCache[info.Type] = result;
                    }
                }
            }
            return result;
        }

        private static ConstructorInfo NewReadOnlyCollectionCtor(Type rocType) 
        {
            var argTypes = JaysonTypeInfo.GetTypeInfo(rocType).GenericArguments;

            rocType = typeof(ReadOnlyCollection<>).MakeGenericType(argTypes);
            return rocType.GetConstructor(new Type[] { typeof(IList<>).MakeGenericType(argTypes) });
        }

        private static ConstructorInfo GetReadOnlyCollectionCtor(Type rocType)
        {
            return s_CtorCache.GetValueOrUpdate(rocType, NewReadOnlyCollectionCtor);
        }

        private static Func<object[], object> GetReadOnlyCollectionActivator(Type rocType)
        {
            return s_ActivatorCache.GetValueOrUpdate(rocType, (rt) => { return JaysonCommon.CreateActivator(GetReadOnlyCollectionCtor(rocType)); });
        }

#if !(NET4000 || NET3500 || NET3000 || NET2000)
        private static ConstructorInfo GetReadOnlyDictionaryCtor(Type rodType)
        {
            ConstructorInfo ctor;
            if (!s_CtorCache.TryGetValue(rodType, out ctor))
            {
                lock (((ICollection)s_CtorCache).SyncRoot)
                {
                    if (!s_CtorCache.TryGetValue(rodType, out ctor))
                    {
                        var argTypes = JaysonTypeInfo.GetTypeInfo(rodType).GenericArguments;

                        rodType = typeof(ReadOnlyDictionary<,>).MakeGenericType(argTypes);
                        ctor = rodType.GetConstructor(new Type[] { typeof(IDictionary<,>).MakeGenericType(argTypes) });

                        s_CtorCache[rodType] = ctor;
                    }
                }
            }
            return ctor;
        }

        private static Func<object[], object> GetReadOnlyDictionaryActivator(Type rodType)
        {
            Func<object[], object> result;
            if (!s_ActivatorCache.TryGetValue(rodType, out result))
            {
                lock (((ICollection)s_ActivatorCache).SyncRoot)
                {
                    if (!s_ActivatorCache.TryGetValue(rocType, out result))
                    {
                        result = JaysonCommon.CreateActivator(GetReadOnlyDictionaryCtor(rodType));
                        s_ActivatorCache[rodType] = result;
                    }
                }
            }
            return result;
        }
#endif

        # region Convert DataTable & DataSet

        private static void SetExtendedProperties(PropertyCollection extendedProperties, object obj,
            JaysonDeserializationContext context)
        {
            var dct = obj as IDictionary<string, object>;
            if (dct != null)
            {
                object kvListObj;
                if (dct.TryGetValue("$kv", out kvListObj))
                {
                    var kvList = kvListObj as List<object>;
                    if (kvList != null)
                    {
                        var count = kvList.Count;
                        if (count > 0)
                        {
                            IDictionary<string, object> kvp;
                            for (var i = 0; i < count; i++)
                            {
                                kvp = (IDictionary<string, object>)kvList[i];

                                extendedProperties.Add(ConvertObject(kvp["$k"], typeof(object), context),
                                    ConvertObject(kvp["$v"], typeof(object), context));
                            }
                        }
                    }
                    return;
                }

                var hasSkeyword = dct.ContainsKey("$type") || dct.ContainsKey("$id") || dct.ContainsKey("$ref");
                foreach (var ekvp in dct)
                {
                    if (!hasSkeyword || !(ekvp.Key == "$type" || ekvp.Key == "$id" || ekvp.Key == "$ref"))
                    {
                        extendedProperties.Add(ConvertObject(ekvp.Key, typeof(object), context),
                            ConvertObject(ekvp.Value, typeof(object), context));
                    }
                }
            }
        }

        private static void SetDataTableProperties(IDictionary<string, object> obj, DataTable dataTable,
            JaysonDeserializationContext context)
        {
            object propValue;
            if (dataTable.ChildRelations.Count == 0 && dataTable.ParentRelations.Count == 0)
            {
                if (obj.TryGetValue("Cs", out propValue) || obj.TryGetValue("cs", out propValue))
                {
                    if (propValue is bool)
                    {
                        dataTable.CaseSensitive = (bool)propValue;
                    }
                    else if (propValue is string)
                    {
                        dataTable.CaseSensitive = (string)propValue == JaysonConstants.True;
                    }
                }

                if (obj.TryGetValue("Lcl", out propValue) || obj.TryGetValue("lcl", out propValue))
                {
                    dataTable.Locale = CultureInfo.GetCultureInfo((string)propValue);
                }
            }

            if (obj.TryGetValue("De", out propValue) || obj.TryGetValue("de", out propValue))
            {
                dataTable.DisplayExpression = (string)propValue;
            }

            if (obj.TryGetValue("Ns", out propValue) || obj.TryGetValue("ns", out propValue))
            {
                dataTable.Namespace = (string)propValue;
            }

            if (obj.TryGetValue("Pfx", out propValue) || obj.TryGetValue("pfx", out propValue))
            {
                dataTable.Prefix = (string)propValue;
            }

            if (obj.TryGetValue("Tn", out propValue) || obj.TryGetValue("tn", out propValue))
            {
                dataTable.TableName = (string)propValue;
            }

            if (obj.TryGetValue("Ep", out propValue) || obj.TryGetValue("ep", out propValue))
            {
                SetExtendedProperties(dataTable.ExtendedProperties, propValue, context);
            }
        }

        private static void SetDataTableColumns(IDictionary<string, object> obj, DataTable dataTable,
            JaysonDeserializationContext context)
        {
            object columnsObj;
            if (obj.TryGetValue("Cols", out columnsObj) || obj.TryGetValue("cols", out columnsObj))
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

                var ordinalColumnList = new DataColumn[columnList.Count];
                var unordinalColumnList = new List<DataColumn>();

                var settings = context.Settings;

                foreach (var columnInfoObj in columnList)
                {
                    columnInfo = (Dictionary<string, object>)columnInfoObj;

                    columnName = null;
                    if (columnInfo.TryGetValue("Cn", out propValue) || columnInfo.TryGetValue("cn", out propValue))
                    {
                        columnName = (string)propValue ?? String.Empty;
                    }

                    if (columnName != null && dataTable.Columns.Contains(columnName))
                    {
                        column = dataTable.Columns[columnName];
                    }
                    else
                    {
                        if ((columnInfo.TryGetValue("Dt", out propValue) ||
                            columnInfo.TryGetValue("dt", out propValue)) && propValue != null)
                        {
                            dataType = JaysonCommon.GetType((string)propValue, settings.Binder);

                            var typeOverride = settings.GetTypeOverride(dataType);
                            if (typeOverride != null)
                            {
                                var bindToType = typeOverride.BindToType;
                                if (bindToType != null)
                                {
                                    dataType = bindToType;
                                }
                            }
                        }
                        else
                        {
                            dataType = typeof(object);
                        }

                        expression = null;
                        if (columnInfo.TryGetValue("Exp", out propValue) || columnInfo.TryGetValue("exp", out propValue))
                        {
                            expression = (string)propValue;
                        }

                        if ((columnInfo.TryGetValue("Cm", out propValue) ||
                            columnInfo.TryGetValue("cm", out propValue)) && propValue != null)
                        {
                            mappingType = (MappingType)JaysonEnumCache.Parse((string)propValue, typeof(MappingType));
                        }
                        else
                        {
                            mappingType = MappingType.Element;
                        }

                        column = new DataColumn(columnName, dataType, expression, mappingType);

                        ordinal = -1;
                        if (columnInfo.TryGetValue("Ord", out propValue) || columnInfo.TryGetValue("ord", out propValue))
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

                    if (columnInfo.TryGetValue("Ns", out propValue) || columnInfo.TryGetValue("ns", out propValue))
                    {
                        column.Namespace = (string)propValue;
                    }

                    if (columnInfo.TryGetValue("Adbn", out propValue) || columnInfo.TryGetValue("adbn", out propValue))
                    {
                        if (propValue is bool)
                        {
                            column.AllowDBNull = (bool)propValue;
                        }
                        else
                            if (propValue is string)
                            {
                                column.AllowDBNull = (string)propValue == JaysonConstants.True;
                            }
                    }

                    if (columnInfo.TryGetValue("Ai", out propValue) || columnInfo.TryGetValue("ai", out propValue))
                    {
                        if (propValue is bool)
                        {
                            column.AutoIncrement = (bool)propValue;
                        }
                        else
                            if (propValue is string)
                            {
                                column.AutoIncrement = (string)propValue == JaysonConstants.True;
                            }
                    }

                    if (columnInfo.TryGetValue("Aisd", out propValue) || columnInfo.TryGetValue("aisd", out propValue))
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

                    if (columnInfo.TryGetValue("Aistp", out propValue) || columnInfo.TryGetValue("aistp", out propValue))
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

                    if (columnInfo.TryGetValue("Ml", out propValue) || columnInfo.TryGetValue("ml", out propValue))
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

                    if (columnInfo.TryGetValue("Cap", out propValue) || columnInfo.TryGetValue("cap", out propValue))
                    {
                        column.Caption = (string)propValue;
                    }

                    if (columnInfo.TryGetValue("Pfx", out propValue) || columnInfo.TryGetValue("pfx", out propValue))
                    {
                        column.Prefix = (string)propValue;
                    }

                    if (columnInfo.TryGetValue("Ro", out propValue) || columnInfo.TryGetValue("ro", out propValue))
                    {
                        if (propValue is bool)
                        {
                            column.ReadOnly = (bool)propValue;
                        }
                        else
                            if (propValue is string)
                            {
                                column.ReadOnly = (string)propValue == JaysonConstants.True;
                            }
                    }

                    if (columnInfo.TryGetValue("Uq", out propValue) || columnInfo.TryGetValue("uq", out propValue))
                    {
                        if (propValue is bool)
                        {
                            column.Unique = (bool)propValue;
                        }
                        else
                            if (propValue is string)
                            {
                                column.Unique = (string)propValue == JaysonConstants.True;
                            }
                    }

                    if (columnInfo.TryGetValue("Ep", out propValue) || columnInfo.TryGetValue("ep", out propValue))
                    {
                        SetExtendedProperties(column.ExtendedProperties, propValue, context);
                    }
                }

                if (unordinalColumnList.Count > 0)
                {
                    var columnCount = ordinalColumnList.Length;
                    var unordColPos = unordinalColumnList.Count - 1;

                    for (var i = columnCount - 1; i > -1; i--)
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
            JaysonDeserializationContext context)
        {
            object primaryKey;
            if ((obj.TryGetValue("Pk", out primaryKey) ||
                obj.TryGetValue("pk", out primaryKey)) && (primaryKey != null))
            {
                var currPrimaryKey = dataTable.PrimaryKey;
                if (currPrimaryKey == null || currPrimaryKey.Length == 0)
                {
                    var primaryKeyList = (IList)primaryKey;
                    if (primaryKeyList.Count > 0)
                    {
                        string columnName;
                        
                        var columns = dataTable.Columns;
                        var primaryKeyColumns = new List<DataColumn>();

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
            JaysonDeserializationContext context)
        {
            object rowsObj;
            if (obj.TryGetValue("Rows", out rowsObj))
            {
                var rowsList = (IList)rowsObj;
                if (rowsList.Count > 0)
                {
                    dataTable.BeginLoadData();
                    try
                    {
                        var columnCount = dataTable.Columns.Count;
                        var columnTypes = new Type[columnCount];

                        for (var i = 0; i < columnCount; i++)
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

                            for (var i = 0; i < itemCount; i++)
                            {
                                rowValue = rowList[i];
                                columnType = columnTypes[i];

                                if (rowValue == null || rowValue.GetType() != columnType)
                                {
                                    items[i] = ConvertObject(rowValue, columnType, context);
                                }
                                else
                                {
                                    items[i] = rowValue;
                                }
                            }

                            dataTable.Rows.Add(items);
                        }
                    }
                    finally
                    {
                        dataTable.EndLoadData();
                    }
                }
            }
        }

        private static void SetDataTable(IDictionary<string, object> obj, DataTable dataTable,
            JaysonDeserializationContext context)
        {
            dataTable.BeginInit();
            try
            {
                SetDataTableProperties(obj, dataTable, context);
                SetDataTableColumns(obj, dataTable, context);
                SetDataTablePrimaryKey(obj, dataTable, context);
                SetDataTableRows(obj, dataTable, context);
            }
            finally
            {
                dataTable.EndInit();
            }
        }

        private static void SetDataRelations(IDictionary<string, object> obj, DataSet dataSet,
            JaysonDeserializationContext context)
        {
            object relationsObj;
            if (obj.TryGetValue("Rel", out relationsObj) || obj.TryGetValue("rel", out relationsObj))
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

                var childColumns = new List<DataColumn>();
                var parentColumns = new List<DataColumn>();

                foreach (var relationInfoObj in relationsList)
                {
                    relationInfo = (Dictionary<string, object>)relationInfoObj;

                    relationName = null;
                    if ((relationInfo.TryGetValue("Rn", out propValue) ||
                        relationInfo.TryGetValue("rn", out propValue)) && propValue != null)
                    {
                        relationName = (string)propValue;
                        if (dataSet.Relations.Contains(relationName))
                            continue;

                        tableName = null;
                        if ((relationInfo.TryGetValue("CTab", out propValue) ||
                            relationInfo.TryGetValue("ctab", out propValue)) && propValue != null)
                        {
                            tableName = (string)propValue;

                            tableNamespace = null;
                            if ((relationInfo.TryGetValue("CTabNs", out propValue) ||
                                relationInfo.TryGetValue("ctabns", out propValue)) && propValue != null)
                            {
                                tableNamespace = (string)propValue;
                            }

                            childTable = String.IsNullOrEmpty(tableNamespace) ? (dataSet.Tables.Contains(tableName) ? dataSet.Tables[tableName] : null) :
                                (dataSet.Tables.Contains(tableName, tableNamespace) ? dataSet.Tables[tableName, tableNamespace] : null);

                            if (childTable != null)
                            {
                                childColumns.Clear();
                                if (relationInfo.TryGetValue("CCol", out propValue) || relationInfo.TryGetValue("ccol", out propValue))
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
                                    if ((relationInfo.TryGetValue("PTab", out propValue) ||
                                        relationInfo.TryGetValue("ptab", out propValue)) && propValue != null)
                                    {
                                        tableName = (string)propValue;

                                        tableNamespace = null;
                                        if ((relationInfo.TryGetValue("PTabNs", out propValue) ||
                                            relationInfo.TryGetValue("ptabns", out propValue)) && propValue != null)
                                        {
                                            tableNamespace = (string)propValue;
                                        }

                                        parentTable = String.IsNullOrEmpty(tableNamespace) ? (dataSet.Tables.Contains(tableName) ? dataSet.Tables[tableName] : null) :
                                            (dataSet.Tables.Contains(tableName, tableNamespace) ? dataSet.Tables[tableName, tableNamespace] : null);

                                        if (parentTable != null)
                                        {
                                            parentColumns.Clear();
                                            if (relationInfo.TryGetValue("PCol", out propValue) || relationInfo.TryGetValue("pcol", out propValue))
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
            JaysonDeserializationContext context)
        {
            object propValue;
            if (obj.TryGetValue("Cs", out propValue) || obj.TryGetValue("cs", out propValue))
            {
                if (propValue is bool)
                {
                    dataSet.CaseSensitive = (bool)propValue;
                }
                else
                    if (propValue is string)
                    {
                        dataSet.CaseSensitive = (string)propValue == JaysonConstants.True;
                    }
            }

            if (obj.TryGetValue("Dsn", out propValue) || obj.TryGetValue("dsn", out propValue))
            {
                dataSet.DataSetName = (string)propValue;
            }

            if (obj.TryGetValue("Ec", out propValue) || obj.TryGetValue("ec", out propValue))
            {
                if (propValue is bool)
                {
                    dataSet.EnforceConstraints = (bool)propValue;
                }
                else
                    if (propValue is string)
                    {
                        dataSet.EnforceConstraints = (string)propValue == JaysonConstants.True;
                    }
            }

            if (obj.TryGetValue("Lcl", out propValue) || obj.TryGetValue("lcl", out propValue))
            {
                dataSet.Locale = CultureInfo.GetCultureInfo((string)propValue);
            }

            if (obj.TryGetValue("Ns", out propValue) || obj.TryGetValue("ns", out propValue))
            {
                dataSet.Namespace = (string)propValue;
            }

            if (obj.TryGetValue("Pfx", out propValue) || obj.TryGetValue("pfx", out propValue))
            {
                dataSet.Prefix = (string)propValue;
            }

            if (obj.TryGetValue("Ssm", out propValue) || obj.TryGetValue("ssm", out propValue))
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

            if (obj.TryGetValue("Ep", out propValue) || obj.TryGetValue("ep", out propValue))
            {
                SetExtendedProperties(dataSet.ExtendedProperties, propValue, context);
            }
        }

        private static bool GetTableName(IDictionary<string, object> tableObj, out string tableName, out string tableNamespace)
        {
            tableName = null;
            tableNamespace = null;

            object propValue;

            if (tableObj.TryGetValue("Tn", out propValue) || tableObj.TryGetValue("tn", out propValue))
            {
                tableName = (string)propValue;
                if (tableObj.TryGetValue("Ns", out propValue) || tableObj.TryGetValue("ns", out propValue))
                {
                    tableNamespace = (string)propValue;
                }
                return true;
            }
            return false;
        }

        private static void SetDataTables(IDictionary<string, object> obj, DataSet dataSet,
            JaysonDeserializationContext context)
        {
            object tablesObj;
            if (obj.TryGetValue("Tab", out tablesObj) || obj.TryGetValue("tab", out tablesObj))
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
                            else
                                if (dataSet.Tables.Contains(tableName, tableNamespace))
                                {
                                    dataTable = dataSet.Tables[tableName, tableNamespace];
                                }
                        }

                        if (dataTable != null)
                        {
                            SetDataTable(table, dataTable, context);
                        }
                        else
                        {
                            dataTable = ConvertObject(tableObj, typeof(DataTable), context) as DataTable;
                            if (dataTable != null)
                            {
                                if (String.IsNullOrEmpty(dataTable.Namespace))
                                {
                                    if (!dataSet.Tables.Contains(dataTable.TableName))
                                    {
                                        dataSet.Tables.Add(dataTable);
                                    }
                                }
                                else
                                    if (!dataSet.Tables.Contains(dataTable.TableName, dataTable.Namespace))
                                    {
                                        dataSet.Tables.Add(dataTable);
                                    }
                            }
                        }
                    }
                }
            }
        }

        private static void SetDataSet(IDictionary<string, object> obj, DataSet dataSet,
            JaysonDeserializationContext context)
        {
            dataSet.BeginInit();
            try
            {
                SetDataSetProperties(obj, dataSet, context);
                SetDataTables(obj, dataSet, context);
                SetDataRelations(obj, dataSet, context);
            }
            finally
            {
                dataSet.EndInit();
            }
        }

        # endregion Convert DataTable & DataSet

        private static void SetDictionary(IDictionary<string, object> obj, ref object instance, JaysonDeserializationContext context)
        {
            if ((instance != null) && (obj != null) && (obj.Count > 0) && !(instance is DBNull))
            {
                var info = JaysonTypeInfo.GetTypeInfo(instance.GetType());
                var serializationType = info.SerializationType;

                switch (serializationType)
                {
                    case JaysonTypeSerializationType.IDictionaryGeneric:
                        {
                            SetDictionaryStringKey(obj, (IDictionary<string, object>)instance, context);
                            return;
                        }
                    case JaysonTypeSerializationType.IDictionary:
                        {
                            SetDictionaryIDictionary(obj, (IDictionary)instance, context);
                            return;
                        }
                    case JaysonTypeSerializationType.DataTable:
                        {
                            SetDataTable(obj, (DataTable)instance, context);
                            return;
                        }
                    case JaysonTypeSerializationType.DataSet:
                        {
                            SetDataSet(obj, (DataSet)instance, context);
                            return;
                        }
                    case JaysonTypeSerializationType.NameValueCollection:
                        {
                            SetDictionaryNameValueCollection(obj, (NameValueCollection)instance, context);
                            return;
                        }
                    case JaysonTypeSerializationType.StringDictionary:
                        {
                            SetDictionaryStringDictionary(obj, (StringDictionary)instance, context);
                            return;
                        }
                    case JaysonTypeSerializationType.Anonymous:
                        {
                            SetDictionaryAnonymousType(obj, instance, info.Type, context);
                            return;
                        }
                    default:
                        {
                            if (SetDictionaryGeneric(obj, instance, info.Type, context))
                            {
                                return;
                            }

                            SetDictionaryClassOrStruct(obj, ref instance, info.Type, context);
                            return;
                        }
                }
            }
        }

        private static void SetDictionaryClassOrStruct(IDictionary<string, object> obj, ref object instance,
            Type instanceType, JaysonDeserializationContext context)
        {
            var info = JaysonTypeInfo.GetTypeInfo(instanceType);
            if (info.JPrimitive)
            {
                return;
            }

            var settings = context.Settings;

            IJaysonFastMember member;
            var cache = JaysonFastMemberCache.GetCache(instanceType);

            string memberName;
            object memberValue;

            var typeOverride = settings.GetTypeOverride(instanceType);
            var hasSkeyword = obj.ContainsKey("$type") || obj.ContainsKey("$id") || obj.ContainsKey("$ref");

            var existingValues = settings.UseDefaultValues ? new Dictionary<string, object>() : null;

            foreach (var entry in obj)
            {
                try
                {
                    memberName = entry.Key;

                    if (!hasSkeyword || !(memberName == "$type" || memberName == "$id" || memberName == "$ref"))
                    {
                        member = cache.GetAnyMember(memberName, settings.CaseSensitive);

                        if ((member == null) && (typeOverride != null))
                        {
                            var aliasName = typeOverride.GetAliasMember(memberName);
                            if (!String.IsNullOrEmpty(aliasName))
                            {
                                member = cache.GetAnyMember(aliasName, settings.CaseSensitive);
                                memberName = (member != null) ? member.Name : entry.Key;
                            }
                        }

                        if (member != null)
                        {
                            if (settings.UseDefaultValues)
                            {
                                existingValues[member.Name] = null;
                            }

                            if (member.AnonymousField || (settings.IgnoreBackingFields && member.BackingField) ||
                                (settings.IgnoreFields && (member.Type == JaysonFastMemberType.Field)) ||
                                (settings.IgnoreNonPublicFields && !member.IsPublic && (member.Type == JaysonFastMemberType.Field)) ||
                                (settings.IgnoreNonPublicProperties && !member.IsPublic && (member.Type == JaysonFastMemberType.Property)))
                                continue;

                            if (!settings.IsMemberIgnored(instanceType, memberName) && 
                                (typeOverride == null || !typeOverride.IsMemberIgnored(memberName)))
                            {
                                if (member.CanWrite)
                                {
                                    memberValue = ConvertObject(entry.Value, member.MemberType, context);
                                    if (!settings.IsMemberIgnored(instanceType, memberName, memberValue))
                                    {
                                        member.Set(ref instance, memberValue);
                                    }
                                }
                                else if (entry.Value != null)
                                {
                                    memberValue = member.Get(instance);
                                    if (instance != null)
                                    {
                                        if (!settings.IsMemberIgnored(instanceType, memberName, memberValue))
                                        {
                                            SetObject(memberValue, entry.Value, context);
                                        }
                                    }
                                }
                            }
                        }
                        else if (settings.RaiseErrorOnMissingMember)
                        {
                            throw new JaysonException(JaysonError.MissingMember + memberName);
                        }
                    }
                }
                catch (Exception e)
                {
                    var errorHandler = context.Settings.ErrorHandler;
                    if (errorHandler == null)
                        throw;

                    bool handled;
                    errorHandler(instance, entry.Key, e, out handled);
                    if (!handled)
                        throw;
                }
            }

            if (settings.UseDefaultValues && (existingValues != null))
            {
                var existingCount = existingValues.Count;
                if (existingCount < cache.Count)
                {
                    string key;
                    object value;
                    object defaultValue;

                    var members = cache.AllMembers;
                    var setPrivateFieldDefaults = info.SerializationType != JaysonTypeSerializationType.ISerializable;

                    foreach (var tm in members)
                    {
                        if (tm != null)
                        {
                            member = tm;
                            try
                            {
                                // Don't check as (settings.IgnoreBackingFields && member.BackingField), some backingfield set to default is causing crash for ISerializable types, such as System.Uri
                                if ((member != null) && member.CanWrite && 
                                    (setPrivateFieldDefaults || member.IsPublic) &&
                                    !(member.AnonymousField || member.BackingField ||
                                     (settings.IgnoreFields && (member.Type == JaysonFastMemberType.Field)) ||
                                     (settings.IgnoreNonPublicFields && !member.IsPublic && (member.Type == JaysonFastMemberType.Field)) ||
                                     (settings.IgnoreNonPublicProperties && !member.IsPublic && (member.Type == JaysonFastMemberType.Property))))
                                {
                                    key = member.Name;

                                    if ((existingCount > 0) && !existingValues.ContainsKey(key))
                                    {
                                        defaultValue = null;
                                        if ((typeOverride == null) || !typeOverride.TryGetDefaultValue(key, out defaultValue))
                                        {
                                            defaultValue = member.DefaultValue;
                                        }

                                        if (!ReferenceEquals(defaultValue, null))
                                        {
                                            value = member.Get(instance);

                                            if ((!ReferenceEquals(value, null) && !defaultValue.Equals(value)) ||
                                                (!ReferenceEquals(defaultValue, null) && !defaultValue.Equals(value)))
                                            {
                                                member.Set(ref instance, defaultValue);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                var errorHandler = context.Settings.ErrorHandler;
                                if (errorHandler == null)
                                    throw;

                                bool handled;
                                errorHandler(instance, member.Name, e, out handled);
                                if (!handled)
                                    throw;
                            }
                        }
                    }
                }
            }
        }

        private static bool SetDictionaryGeneric(IDictionary<string, object> obj, object instance, Type instanceType,
            JaysonDeserializationContext context)
        {
            var genArgs = JaysonCommon.GetGenericDictionaryArgs(instanceType);
            if (genArgs != null)
            {
                Action<object, object[]> addMethod = JaysonCommon.GetIDictionaryAddMethod(instanceType);
                if (addMethod != null)
                {
                    object key;
                    object value;

                    var keyType = genArgs[0];
                    var valType = genArgs[1];

                    object kvListObj;
                    if (obj.TryGetValue("$kv", out kvListObj))
                    {
                        var kvList = kvListObj as List<object>;
                        if (kvList != null)
                        {
                            var count = kvList.Count;
                            if (count > 0)
                            {
                                IDictionary<string, object> kvp;

                                for (var i = 0; i < count; i++)
                                {
                                    kvp = (IDictionary<string, object>)kvList[i];

                                    key = ConvertObject(kvp["$k"], keyType, context);
                                    value = ConvertObject(kvp["$v"], valType, context);

                                    addMethod(instance, new object[] { key, value });
                                }
                            }
                        }
                        return true;
                    }

                    var hasSkeyword = obj.ContainsKey("$type") || obj.ContainsKey("$id") || obj.ContainsKey("$ref");
                    foreach (var entry in obj)
                    {
                        if (!hasSkeyword || !(entry.Key == "$type" || entry.Key == "$id" || entry.Key == "$ref"))
                        {
                            key = ConvertObject(entry.Key, keyType, context);
                            value = ConvertObject(entry.Value, valType, context);

                            addMethod(instance, new object[] { key, value });
                        }
                    }

                    return true;
                }
            }
            return false;
        }

        private static void SetDictionaryAnonymousType(IDictionary<string, object> obj, object instance, Type instanceType,
            JaysonDeserializationContext context)
        {
            var settings = context.Settings;
            if (!settings.IgnoreAnonymousTypes)
            {
                var cache = JaysonFastMemberCache.GetCache(instanceType);

                if ((cache == null) || (cache.Count == 0))
                {
                    if (settings.RaiseErrorOnMissingMember)
                    {
                        throw new JaysonException(JaysonError.MissingMembers);
                    }
                    return;
                }

                string key;
                string memberName;
                object memberValue;

                IJaysonFastMember member;

                var typeOverride = settings.GetTypeOverride(instanceType);
                var hasSkeyword = obj.ContainsKey("$type") || obj.ContainsKey("$id") || obj.ContainsKey("$ref");

                foreach (var entry in obj)
                {
                    try
                    {
                        if (!hasSkeyword || !(entry.Key == "$type" || entry.Key == "$id" || entry.Key == "$ref"))
                        {
                            member = null;
                            key = entry.Key;

                            member = cache.GetAnyMember("<" + key + ">", settings.CaseSensitive);

                            if ((member == null) && (typeOverride != null))
                            {
                                key = typeOverride.GetAliasMember(key);
                                if (!String.IsNullOrEmpty(key))
                                {
                                    member = cache.GetAnyMember("<" + key + ">");
                                }
                            }

                            if (member != null)
                            {
                                if (member.AnonymousField || (settings.IgnoreBackingFields && member.BackingField) ||
                                    (settings.IgnoreFields && (member.Type == JaysonFastMemberType.Field)) ||
                                    (settings.IgnoreNonPublicFields && !member.IsPublic && (member.Type == JaysonFastMemberType.Field)) ||
                                    (settings.IgnoreNonPublicProperties && !member.IsPublic && (member.Type == JaysonFastMemberType.Property)))
                                    continue;

                                memberName = member.Name;

                                if (!settings.IsMemberIgnored(instanceType, memberName) &&
                                    (typeOverride == null || !typeOverride.IsMemberIgnored(memberName)))
                                {
                                    if (member.CanWrite)
                                    {
                                        memberValue = ConvertObject(entry.Value, member.MemberType, context);
                                        if (!settings.IsMemberIgnored(instanceType, memberName, memberValue))
                                        {
                                            member.Set(ref instance, memberValue);
                                        }
                                    }
                                    else if (entry.Value != null)
                                    {
                                        memberValue = member.Get(instance);
                                        if (instance != null)
                                        {
                                            if (!settings.IsMemberIgnored(instanceType, memberName, memberValue))
                                            {
                                                SetObject(memberValue, entry.Value, context);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (settings.RaiseErrorOnMissingMember)
                            {
                                throw new JaysonException(JaysonError.MissingMember + entry.Key);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        var errorHandler = context.Settings.ErrorHandler;
                        if (errorHandler == null)
                            throw;

                        bool handled;
                        errorHandler(instance, entry.Key, e, out handled);
                        if (!handled)
                            throw;
                    }
                }
            }
        }

        private static void SetDictionaryStringDictionary(IDictionary<string, object> obj, StringDictionary instance,
            JaysonDeserializationContext context)
        {
            string key;
            object value;
            Type valueType;

            object kvListObj;
            if (obj.TryGetValue("$kv", out kvListObj))
            {
                var kvList = kvListObj as List<object>;

                if (kvList != null)
                {
                    var count = kvList.Count;
                    if (count > 0)
                    {
                        object keyObj;
                        IDictionary<string, object> kvp;

                        for (var i = 0; i < count; i++)
                        {
                            kvp = (IDictionary<string, object>)kvList[i];

                            keyObj = ConvertObject(kvp["$k"], typeof(object), context);
                            key = keyObj as string ?? keyObj.ToString();

                            value = ConvertObject(kvp["$v"], typeof(object), context);

                            if (value == null || value is string)
                            {
                                instance.Add(key, (string)value);
                            }
                            else
                            {
                                valueType = value.GetType();
                                if (JaysonTypeInfo.IsJPrimitive(valueType))
                                {
                                    instance.Add(key, JaysonFormatter.ToString(value, valueType));
                                }
                                else if (value is IFormattable)
                                {
                                    instance.Add(key, ((IFormattable)value).ToString(null, JaysonConstants.InvariantCulture));
                                }
                                else
                                {
                                    instance.Add(key, ToJsonString(value));
                                }
                            }
                        }
                    }
                }
                return;
            }

            var hasSkeyword = obj.ContainsKey("$type") || obj.ContainsKey("$id") || obj.ContainsKey("$ref");
            foreach (var item in obj)
            {
                key = item.Key;
                if (!hasSkeyword || !(key == "$type" || key == "$id" || key == "$ref"))
                {
                    value = item.Value;
                    if (value == null || value is string)
                    {
                        instance.Add(key, (string)value);
                    }
                    else
                    {
                        valueType = value.GetType();
                        if (JaysonTypeInfo.IsJPrimitive(valueType))
                        {
                            instance.Add(key, JaysonFormatter.ToString(value, valueType));
                        }
                        else if (value is IFormattable)
                        {
                            instance.Add(key, ((IFormattable)value).ToString(null, JaysonConstants.InvariantCulture));
                        }
                        else
                        {
                            instance.Add(key, ToJsonString(value));
                        }
                    }
                }
            }
        }

        private static void SetDictionaryNameValueCollection(IDictionary<string, object> obj, NameValueCollection instance,
            JaysonDeserializationContext context)
        {
            string key;
            object value;
            Type valueType;

            object kvListObj;
            if (obj.TryGetValue("$kv", out kvListObj))
            {
                var kvList = kvListObj as List<object>;

                if (kvList != null)
                {
                    var count = kvList.Count;
                    if (count > 0)
                    {
                        object keyObj;
                        IDictionary<string, object> kvp;

                        for (var i = 0; i < count; i++)
                        {
                            kvp = (IDictionary<string, object>)kvList[i];

                            keyObj = ConvertObject(kvp["$k"], typeof(object), context);
                            key = keyObj as string ?? keyObj.ToString();

                            value = ConvertObject(kvp["$v"], typeof(object), context);

                            if (value == null || value is string)
                            {
                                instance.Add(key, (string)value);
                            }
                            else
                            {
                                valueType = value.GetType();
                                if (JaysonTypeInfo.IsJPrimitive(valueType))
                                {
                                    instance.Add(key, JaysonFormatter.ToString(value, valueType));
                                }
                                else if (value is IFormattable)
                                {
                                    instance.Add(key, ((IFormattable)value).ToString(null, JaysonConstants.InvariantCulture));
                                }
                                else
                                {
                                    instance.Add(key, ToJsonString(value));
                                }
                            }

                        }
                    }
                }
                return;
            }

            var hasSkeyword = obj.ContainsKey("$type") || obj.ContainsKey("$id") || obj.ContainsKey("$ref");
            foreach (var item in obj)
            {
                key = item.Key;
                if (!hasSkeyword || !(key == "$type" || key == "$id" || key == "$ref"))
                {
                    value = item.Value;
                    if (value == null || value is string)
                    {
                        instance.Add(key, (string)value);
                    }
                    else
                    {
                        valueType = value.GetType();
                        if (JaysonTypeInfo.IsJPrimitive(valueType))
                        {
                            instance.Add(key, JaysonFormatter.ToString(value, valueType));
                        }
                        else if (value is IFormattable)
                        {
                            instance.Add(key, ((IFormattable)value).ToString(null, JaysonConstants.InvariantCulture));
                        }
                        else
                        {
                            instance.Add(key, ToJsonString(value));
                        }
                    }
                }
            }
        }

        private static void SetDictionaryIDictionary(IDictionary<string, object> obj, IDictionary instance, JaysonDeserializationContext context)
        {
            object key;
            object kvListObj;
            bool hasSkeyword;

            var genArgs = JaysonCommon.GetGenericDictionaryArgs(instance.GetType());

            if (genArgs != null)
            {
                var keyType = genArgs[0];
                var valType = genArgs[1];

                if (obj.TryGetValue("$kv", out kvListObj))
                {
                    var kvList = kvListObj as List<object>;
                    if (kvList != null)
                    {
                        var count = kvList.Count;
                        if (count > 0)
                        {
                            object keyObj;
                            IDictionary<string, object> kvp;

                            for (var i = 0; i < count; i++)
                            {
                                kvp = (IDictionary<string, object>)kvList[i];

                                keyObj = ConvertObject(kvp["$k"], keyType, context);
                                instance[keyObj] = ConvertObject(kvp["$v"], valType, context);
                            }
                        }
                    }
                    return;
                }

                hasSkeyword = obj.ContainsKey("$type") || obj.ContainsKey("$id") || obj.ContainsKey("$ref");
                foreach (var entry in obj)
                {
                    if (!hasSkeyword || !(entry.Key == "$type" || entry.Key == "$id" || entry.Key == "$ref"))
                    {
                        key = ConvertObject(entry.Key, keyType, context);
                        instance[key] = ConvertObject(entry.Value, valType, context);
                    }
                }
                return;
            }

            if (obj.TryGetValue("$kv", out kvListObj))
            {
                var kvList = kvListObj as List<object>;
                if (kvList != null)
                {
                    var count = kvList.Count;
                    if (count > 0)
                    {
                        object keyObj;
                        IDictionary<string, object> kvp;

                        for (var i = 0; i < count; i++)
                        {
                            kvp = (IDictionary<string, object>)kvList[i];

                            keyObj = ConvertObject(kvp["$k"], typeof(object), context);
                            instance[keyObj] = ConvertObject(kvp["$v"], typeof(object), context);
                        }
                    }
                }
                return;
            }

            object keyObj2;

            hasSkeyword = obj.ContainsKey("$type") || obj.ContainsKey("$id") || obj.ContainsKey("$ref");
            foreach (var entry in obj)
            {
                if (!hasSkeyword || !(entry.Key == "$type" || entry.Key == "$id" || entry.Key == "$ref"))
                {
                    keyObj2 = ConvertObject(entry.Key, typeof(object), context);
                    instance[keyObj2] = ConvertObject(entry.Value, typeof(object), context);
                }
            }
        }

        private static void SetDictionaryStringKey(IDictionary<string, object> obj, IDictionary<string, object> instance,
            JaysonDeserializationContext context)
        {
            string key;
            object kvListObj;
            if (obj.TryGetValue("$kv", out kvListObj))
            {
                var kvList = kvListObj as List<object>;
                if (kvList != null)
                {
                    var count = kvList.Count;
                    if (count > 0)
                    {
                        object keyObj;
                        IDictionary<string, object> kvp;

                        for (var i = 0; i < count; i++)
                        {
                            kvp = (IDictionary<string, object>)kvList[i];

                            keyObj = ConvertObject(kvp["$k"], typeof(object), context);
                            key = keyObj as string ?? keyObj.ToString();

                            instance[key] = ConvertObject(kvp["$v"], typeof(object), context);
                        }
                    }
                }
                return;
            }

            var hasSkeyword = obj.ContainsKey("$type") || obj.ContainsKey("$id") || obj.ContainsKey("$ref");
            foreach (var entry in obj)
            {
                key = entry.Key;
                if (!hasSkeyword || !(key == "$type" || key == "$id" || key == "$ref"))
                {
                    instance[key] = ConvertObject(entry.Value, typeof(object), context);
                }
            }
        }

        private static void SetMultiDimensionalArray(IList obj, Array instance, Type arrayType,
            int rank, int currRank, int[] rankIndices, JaysonDeserializationContext context)
        {
            var length = Math.Min(obj.Count, instance.GetLength(currRank));
            if (length > 0)
            {
                if (currRank == rank - 1)
                {
                    for (var i = 0; i < length; i++)
                    {
                        rankIndices[currRank] = i;
                        instance.SetValue(ConvertObject(obj[i], arrayType, context), rankIndices);
                    }
                }
                else
                {
                    object child;
                    for (var i = 0; i < length; i++)
                    {
                        rankIndices[currRank] = i;
                        child = obj[i];

                        if (child is IList)
                        {
                            SetMultiDimensionalArray((IList)child, instance, arrayType, rank, currRank + 1,
                                rankIndices, context);
                        }
                        else if (child is IList<object>)
                        {
                            SetMultiDimensionalArray((IList<object>)child, instance, arrayType, rank, currRank + 1,
                                rankIndices, context);
                        }
                    }
                }
            }
        }

        private static void SetMultiDimensionalArray(IList<object> obj, Array instance, Type arrayType,
            int rank, int currRank, int[] rankIndices, JaysonDeserializationContext context)
        {
            var length = Math.Min(obj.Count, instance.GetLength(currRank));
            if (length > 0)
            {
                if (currRank == rank - 1)
                {
                    for (var i = 0; i < length; i++)
                    {
                        rankIndices[currRank] = i;
                        instance.SetValue(ConvertObject(obj[i], arrayType, context), rankIndices);
                    }
                }
                else
                {
                    object child;
                    for (var i = 0; i < length; i++)
                    {
                        rankIndices[currRank] = i;
                        child = obj[i];

                        if (child is IList)
                        {
                            SetMultiDimensionalArray((IList)child, instance, arrayType, rank, currRank + 1,
                                rankIndices, context);
                        }
                        else if (child is IList<object>)
                        {
                            SetMultiDimensionalArray((IList<object>)child, instance, arrayType, rank, currRank + 1,
                                rankIndices, context);
                        }
                    }
                }
            }
        }

        private static void SetList(IList<object> obj, object instance, JaysonDeserializationContext context)
        {
            if (instance == null || obj == null || obj.Count == 0 || instance is DBNull)
            {
                return;
            }

            var info = JaysonTypeInfo.GetTypeInfo(instance.GetType());
            if (info.Array)
            {
                var aResult = (Array)instance;
                var arrayType = info.ElementType;

                var rank = info.ArrayRank;
                if (rank == 1)
                {
                    var count = obj.Count;
                    for (var i = 0; i < count; i++)
                    {
                        aResult.SetValue(ConvertObject(obj[i], arrayType, context), i);
                    }
                }
                else
                    if (aResult.GetLength(rank - 1) > 0)
                    {
                        SetMultiDimensionalArray(obj, aResult, arrayType, rank, 0, new int[rank], context);
                    }

                return;
            }

            if (instance is IList)
            {
                var count = obj.Count;
                var lResult = (IList)instance;

                object item;
                var argType = JaysonCommon.GetGenericListArgs(info.Type);
                if (argType != null)
                {
#if (NET3500 || NET3000 || NET2000)
					bool isNullable = JaysonTypeInfo.IsNullable(argType);

					Action<object, object[]> add = null;
					if (isNullable)
					{
						add = JaysonCommon.GetICollectionAddMethod(info.Type);
					}
#endif

                    for (var i = 0; i < count; i++)
                    {
#if !(NET3500 || NET3000 || NET2000)
                        lResult.Add(ConvertObject(obj[i], argType, context));
#else
						if (!isNullable)
						{
							lResult.Add(ConvertObject(obj[i], argType, context));
						}
						else
						{
							item = ConvertObject(obj[i], argType, context);
							if (item == null)
							{
								add(lResult, new object[] { null });
							}
							else
							{
								lResult.Add(item);
							}
						}
#endif
                    }
                    return;
                }

                for (var i = 0; i < count; i++)
                {
                    item = ConvertObject(obj[i], typeof(object), context);

                    lResult.Add(item);
                }
            }
            else
            {
                var argType = JaysonCommon.GetGenericCollectionArgs(info.Type);
                if (argType != null)
                {
                    var methodInfo = JaysonCommon.GetICollectionAddMethod(info.Type);
                    if (methodInfo != null)
                    {
                        var count = obj.Count;
                        for (var i = 0; i < count; i++)
                        {
                            methodInfo(instance, new object[] { ConvertObject(obj[i], argType, context) });
                        }
                        return;
                    }
                }

                if (instance is Stack)
                {
                    var stack = (Stack)instance;

                    var count = obj.Count;
                    for (var i = count - 1; i > -1; i--)
                    {
                        stack.Push(ConvertObject(obj[i], typeof(object), context));
                    }
                    return;
                }

                if (instance is Queue)
                {
                    var queue = (Queue)instance;

                    var count = obj.Count;
                    for (var i = 0; i < count; i++)
                    {
                        queue.Enqueue(ConvertObject(obj[i], typeof(object), context));
                    }
                    return;
                }

                var argTypes = info.GenericArguments;

                if (argTypes != null && argTypes.Length == 1)
                {
                    Action<object, object[]> methodInfo;

                    methodInfo = JaysonCommon.GetStackPushMethod(info.Type);
                    if (methodInfo != null)
                    {
                        argType = argTypes[0];
                        var count = obj.Count;

                        for (var i = count - 1; i > -1; i--)
                        {
                            methodInfo(instance, new object[] { ConvertObject(obj[i], argType, context) });
                        }
                        return;
                    }

                    methodInfo = JaysonCommon.GetQueueEnqueueMethod(info.Type);
                    if (methodInfo != null)
                    {
                        argType = argTypes[0];
                        var count = obj.Count;

                        for (var i = 0; i < count; i++)
                        {
                            methodInfo(instance, new object[] { ConvertObject(obj[i], argType, context) });
                        }
                        return;
                    }

#if !(NET3500 || NET3000 || NET2000)
                    methodInfo = JaysonCommon.GetConcurrentBagMethod(info.Type);
                    if (methodInfo != null)
                    {
                        argType = argTypes[0];
                        var count = obj.Count;

                        for (var i = count - 1; i > -1; i--)
                        {
                            methodInfo(instance, new object[] { ConvertObject(obj[i], argType, context) });
                        }
                        return;
                    }

                    if (JaysonCommon.IsProducerConsumerCollection(info.Type))
                    {
                        methodInfo = JaysonCommon.GetIProducerConsumerCollectionAddMethod(info.Type);

                        if (methodInfo != null)
                        {
                            argType = argTypes[0];
                            var count = obj.Count;

                            for (var i = 0; i < count; i++)
                            {
                                methodInfo(instance, new object[] { ConvertObject(obj[i], argType, context) });
                            }
                            return;
                        }
                    }
#endif
                }
            }
        }

        private static void SetObject(object obj, object instance, JaysonDeserializationContext context)
        {
            if ((obj != null) && (instance != null) && !JaysonTypeInfo.IsJPrimitive(instance.GetType()))
            {
                var dict = obj as IDictionary<string, object>;
                if (dict != null)
                {
                    SetDictionary(dict, ref instance, context);
                }
                else
                {
                    var list = obj as IList<object>;
                    if (list != null)
                    {
                        SetList(list, instance, context);
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
            if (info.Array)
            {
                asArray = true;
                return type;
            }

            if (type == typeof(List<object>))
            {
                asList = true;
                return type;
            }

            if (type == typeof(IList) || type == typeof(ArrayList))
            {
                asList = true;
                return typeof(ArrayList);
            }

            if (!info.Generic)
            {
                return type;
            }

            var genericType = info.GenericTypeDefinitionType;

            if (genericType == typeof(ReadOnlyCollection<>))
            {
                asList = true;
                asReadOnly = true;
                return GetGenericUnderlyingList(info);
            }

            if (info.Interface)
            {
#if !(NET4000 || NET3500 || NET3000 || NET2000)
                if (genericType == typeof(IList<>) ||
                    genericType == typeof(ICollection<>) ||
                    genericType == typeof(IEnumerable<>) ||
                    genericType == typeof(IReadOnlyCollection<>))
                {
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
                lock (((ICollection)s_EvaluatedListTypeCache).SyncRoot)
                {
                    if (!s_EvaluatedListTypeCache.TryGetValue(type, out elt))
                    {
                        var eType = EvaluateListType(type, out asList, out asArray, out asReadOnly);
                        s_EvaluatedListTypeCache[type] = new EvaluatedListType
                        {
                            EvaluatedType = eType,
                            AsList = asList,
                            AsArray = asArray,
                            AsReadOnly = asReadOnly,
                        };
                        return eType;
                    }
                }
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
            if (type == typeof(Dictionary<string, object>))
            {
                asDictionary = true;
                return type;
            }

            if (type == typeof(IDictionary) || type == typeof(Hashtable))
            {
                asDictionary = true;
                return typeof(Hashtable);
            }

            var info = JaysonTypeInfo.GetTypeInfo(type);
            if (!info.Generic)
            {
                return type;
            }

            var genericType = info.GenericTypeDefinitionType;

#if !(NET4000 || NET3500 || NET3000 || NET2000)
            if (genericType == typeof(ReadOnlyDictionary<,>))
            {
                asReadOnly = true;
                asDictionary = true;
                return GetGenericUnderlyingDictionary(info);
            }
#endif

            if (info.Interface)
            {
#if !(NET4000 || NET3500 || NET3000 || NET2000)
                if (genericType == typeof(IDictionary<,>) ||
                    genericType == typeof(IReadOnlyDictionary<,>))
                {
                    asDictionary = true;
                    return GetGenericUnderlyingDictionary(info);
                }
#else
				if (genericType == typeof(IDictionary<,>)) {
				asDictionary = true;
				return GetGenericUnderlyingDictionary(info);
				} 
#endif

                if (genericType == typeof(ICollection<>) || genericType == typeof(IEnumerable<>))
                {
                    var firstArgType = info.GenericArguments[0];

                    if (firstArgType == typeof(KeyValuePair<string, object>) ||
                        firstArgType == typeof(KeyValuePair<object, object>))
                    {
                        asDictionary = true;
                        return GetGenericUnderlyingDictionary(info);
                    }

                    var argInfo = JaysonTypeInfo.GetTypeInfo(firstArgType);
                    if (argInfo.Generic && argInfo.GenericTypeDefinitionType == typeof(KeyValuePair<,>))
                    {
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
                lock (((ICollection)s_EvaluatedDictionaryTypeCache).SyncRoot)
                {
                    if (!s_EvaluatedDictionaryTypeCache.TryGetValue(type, out edt))
                    {
                        var eType = EvaluateDictionaryType(type, out asDictionary, out asReadOnly);

                        s_EvaluatedDictionaryTypeCache[type] = new EvaluatedDictionaryType
                        {
                            EvaluatedType = eType,
                            AsDictionary = asDictionary,
                            AsReadOnly = asReadOnly
                        };
                        return eType;
                    }
                }
            }

            asDictionary = edt.AsDictionary;
            asReadOnly = edt.AsReadOnly;

            return edt.EvaluatedType;
        }

        private static object CtorParamMatcher(string ctorParamName, IDictionary<string, object> obj)
        {
            return obj.FirstOrDefault(kvp =>
                ctorParamName.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase)).Value;
        }

        private static object ConvertDictionary(IDictionary<string, object> obj, Type toType, JaysonDeserializationContext context, bool forceType = false)
        {
            var hasSkeyword = false;

            object Sref;
            if (obj.TryGetValue("$ref", out Sref))
            {
                if (Sref is long)
                {
                    Sref = (int)((long)Sref);
                }
                return context.ReferenceMap[(int)Sref];
            }

            var id = 0;
            if (obj.TryGetValue("$id", out Sref))
            {
                hasSkeyword = true;
                if (Sref is long)
                {
                    id = (int)((long)Sref);
                }
                else
                {
                    id = (int)Sref;
                }
            }

            object Stype;
            var binded = false;

            var settings = context.Settings;

            JaysonTypeInfo info = null;
            if (obj.TryGetValue("$type", out Stype) && Stype != null)
            {
                hasSkeyword = true;

                info = JaysonTypeInfo.GetTypeInfo(toType);
                Type instanceType = null;

                if (!forceType || toType == typeof(object) || info.Interface)
                {
                    var typeName = Stype as string;

                    if (!String.IsNullOrEmpty(typeName))
                    {
                        binded = true;
                        instanceType = JaysonCommon.GetType(typeName, settings.Binder);
                    }
                    else if (context.GlobalTypes != null)
                    {
                        if (Stype is int)
                        {
                            instanceType = context.GlobalTypes.GetType((int)Stype);
                        }
                        else if (Stype is long)
                        {
                            instanceType = context.GlobalTypes.GetType((int)((long)Stype));
                        }
                    }
                }

                if (instanceType == null && !forceType)
                {
                    instanceType = toType;
                }

                if (instanceType != null)
                {
                    var typeOverride = settings.GetTypeOverride(instanceType);
                    if (typeOverride != null)
                    {
                        var bindToType = typeOverride.BindToType;
                        if (bindToType != null)
                        {
                            instanceType = bindToType;
                        }
                    }

                    if (instanceType != null && instanceType != toType)
                    {
                        toType = instanceType;
                        info = JaysonTypeInfo.GetTypeInfo(toType);
                    }

                    if (instanceType == typeof(object))
                    {
                        return new object();
                    }
                }

                if (settings.IgnoreAnonymousTypes && (info != null) && info.Anonymous)
                {
                    return null;
                }
            }

            if (id > 0 || Stype != null)
            {
                object Svalues;
                if (obj.TryGetValue("$value", out Svalues))
                {
                    var ret = ConvertObject(Svalues, toType, context);
                    if (id > 0)
                    {
                        context.ReferenceMap[id] = ret;
                    }

                    return ret;
                }

                if (obj.TryGetValue("$values", out Svalues) && (Svalues is IList<object>))
                {
                    var ret = ConvertList((IList<object>)Svalues, toType, context);
                    if (id > 0)
                    {
                        context.ReferenceMap[id] = ret;
                    }

                    return ret;
                }
            }

            if (obj.TryGetValue("$dt", out Stype) && Stype != null)
            {
                var dataType = Stype as string;
                if (dataType != null)
                {
                    if (dataType.Equals("Tbl", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!typeof(DataTable).IsAssignableFrom(toType))
                        {
                            toType = typeof(DataTable);
                        }
                    }
                    else if (dataType.Equals("Ds", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!typeof(DataSet).IsAssignableFrom(toType))
                        {
                            toType = typeof(DataSet);
                        }
                    }
                }
            }

            bool asReadOnly, asDictionary;
            toType = GetEvaluatedDictionaryType(toType, out asDictionary, out asReadOnly);

            if (!binded)
            {
                var instanceType = BindToType(settings, toType);
                if (instanceType != null && instanceType != toType)
                {
                    toType = instanceType;
                    info = JaysonTypeInfo.GetTypeInfo(toType);
                }
            }

            if (settings.IgnoreAnonymousTypes && (info != null) && info.Anonymous)
            {
                return null;
            }

            object result = null;

            var useDefaultCtor = true;
            if (settings.ObjectActivator != null)
            {
                result = settings.ObjectActivator(toType, obj, out useDefaultCtor);
            }

            if (useDefaultCtor)
            {
                if (toType == typeof(object))
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
                else
                {
                    if (info == null || info.Type != toType)
                    {
                        info = JaysonTypeInfo.GetTypeInfo(toType);
                    }

                    var serializationType = info.SerializationType;

                    switch (serializationType)
                    {
                        case JaysonTypeSerializationType.Type:
                            {
                                var asmKvp = obj.FirstOrDefault(kvp => kvp.Key.ToLowerInvariant() == "qualifiedname");
                                if (String.IsNullOrEmpty(asmKvp.Key))
                                {
                                    return null;
                                }

                                var typeName = asmKvp.Value as string;
                                if (String.IsNullOrEmpty(typeName))
                                {
                                    return null;
                                }

                                result = JaysonCommon.GetType(typeName, settings.Binder);
                                break;
                            }
                        case JaysonTypeSerializationType.ParameterInfo:
                            {
                                var asmKvp = obj.FirstOrDefault(kvp => kvp.Key.ToLowerInvariant() == "qualifiedname");
                                if (String.IsNullOrEmpty(asmKvp.Key))
                                {
                                    return null;
                                }

                                var typeName = asmKvp.Value as string;
                                if (String.IsNullOrEmpty(typeName))
                                {
                                    return null;
                                }

                                result = JaysonCommon.GetType(typeName, settings.Binder);
                                break;
                            }
                        case JaysonTypeSerializationType.ConstructorInfo:
                            {
                                string typeName;
                                Type type = null;

                                var asmKvp = obj.FirstOrDefault(kvp => kvp.Key.ToLowerInvariant() == "reflectedtype");
                                if (!String.IsNullOrEmpty(asmKvp.Key))
                                {
                                    typeName = asmKvp.Value as string;
                                    if (!String.IsNullOrEmpty(typeName))
                                    {
                                        type = JaysonCommon.GetType(typeName, settings.Binder);
                                    }
                                }

                                if (type == null)
                                {
                                    asmKvp = obj.FirstOrDefault(kvp => kvp.Key.ToLowerInvariant() == "qualifiedname");
                                    if (String.IsNullOrEmpty(asmKvp.Key))
                                    {
                                        return null;
                                    }

                                    typeName = asmKvp.Value as string;
                                    if (String.IsNullOrEmpty(typeName))
                                    {
                                        return null;
                                    }

                                    type = JaysonCommon.GetType(typeName, settings.Binder);
                                    if (type == null)
                                    {
                                        return null;
                                    }
                                }

                                Type[] parameterTypes = null;

                                asmKvp = obj.FirstOrDefault(kvp => kvp.Key.ToLowerInvariant() == "params");
                                if (!String.IsNullOrEmpty(asmKvp.Key))
                                {
                                    parameterTypes = ConvertObject(asmKvp.Value, typeof(Type[]), context) as Type[];
                                }

                                result = type.GetConstructor(BindingFlags.Instance | BindingFlags.Static |
                                    BindingFlags.Public | BindingFlags.NonPublic, null, parameterTypes, null);
                                break;
                            }
                        case JaysonTypeSerializationType.FieldInfo:
                        case JaysonTypeSerializationType.PropertyInfo:
                        case JaysonTypeSerializationType.MethodInfo:
                            {
                                string typeName;
                                Type type = null;

                                var asmKvp = obj.FirstOrDefault(kvp => kvp.Key.ToLowerInvariant() == "reflectedtype");
                                if (!String.IsNullOrEmpty(asmKvp.Key))
                                {
                                    typeName = asmKvp.Value as string;
                                    if (!String.IsNullOrEmpty(typeName))
                                    {
                                        type = JaysonCommon.GetType(typeName, settings.Binder);
                                    }
                                }

                                if (type == null)
                                {
                                    asmKvp = obj.FirstOrDefault(kvp => kvp.Key.ToLowerInvariant() == "qualifiedname");
                                    if (String.IsNullOrEmpty(asmKvp.Key))
                                    {
                                        return null;
                                    }

                                    typeName = asmKvp.Value as string;
                                    if (String.IsNullOrEmpty(typeName))
                                    {
                                        return null;
                                    }

                                    type = JaysonCommon.GetType(typeName, settings.Binder);
                                    if (type == null)
                                    {
                                        return null;
                                    }
                                }

                                asmKvp = obj.FirstOrDefault(kvp => kvp.Key.ToLowerInvariant() == "membername");
                                if (String.IsNullOrEmpty(asmKvp.Key))
                                {
                                    return null;
                                }

                                var memberName = asmKvp.Value as string;
                                if (String.IsNullOrEmpty(memberName))
                                {
                                    return null;
                                }

                                switch (serializationType)
                                {
                                    case JaysonTypeSerializationType.FieldInfo:
                                        result = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Static |
                                            BindingFlags.Public | BindingFlags.NonPublic);
                                        break;
                                    case JaysonTypeSerializationType.PropertyInfo:
                                        result = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Static |
                                            BindingFlags.Public | BindingFlags.NonPublic);
                                        break;
                                    case JaysonTypeSerializationType.MethodInfo:
                                        {
                                            Type[] parameterTypes = null;

                                            asmKvp = obj.FirstOrDefault(kvp => kvp.Key.ToLowerInvariant() == "params");
                                            if (!String.IsNullOrEmpty(asmKvp.Key))
                                            {
                                                parameterTypes = ConvertObject(asmKvp.Value, typeof(Type[]), context) as Type[];
                                            }

                                            if (parameterTypes == null)
                                            {
                                                result = type.GetMethod(memberName, BindingFlags.Instance | BindingFlags.Static |
                                                    BindingFlags.Public | BindingFlags.NonPublic);
                                            }
                                            else
                                            {
                                                result = type.GetMethod(memberName, BindingFlags.Instance | BindingFlags.Static |
                                                    BindingFlags.Public | BindingFlags.NonPublic, null, parameterTypes, null);
                                            }
                                            break;
                                        }
                                }
                                break;
                            }
                        case JaysonTypeSerializationType.ISerializable:
                            {
                                var ctorInfo = JaysonCtorInfo.GetISerializableCtorInfo(toType);
                                var serializationInfo = new SerializationInfo(toType, JaysonCommon.FormatterConverter);

                                string key;
                                object paramValue;

                                object ctxObj;
                                if (!obj.TryGetValue("$ctx", out ctxObj))
                                {
                                    foreach (var kvp in obj)
                                    {
                                        key = kvp.Key;
                                        if (!hasSkeyword || !(key == "$type" || key == "$id"))
                                        {
                                            paramValue = ConvertObject(kvp.Value, typeof(object), context);
                                            serializationInfo.AddValue(key, paramValue);
                                        }
                                    }
                                }
                                else
                                {
                                    var ctx = ctxObj as IList;
                                    if ((ctx != null) && (ctx.Count > 0))
                                    {
                                        object oKey;
                                        IDictionary<string, object> kvp;

                                        foreach (var kv in ctx)
                                        {
                                            kvp = kv as IDictionary<string, object>;
                                            if ((kvp != null) && kvp.TryGetValue("$k", out oKey))
                                            {
                                                key = oKey as string;
                                                if (!String.IsNullOrEmpty(key) &&
                                                    kvp.TryGetValue("$v", out paramValue))
                                                {
                                                    paramValue = ConvertObject(paramValue, typeof(object), context);
                                                    serializationInfo.AddValue(key, paramValue);
                                                }
                                            }
                                        }
                                    }
                                }

                                result = ctorInfo.New(new object[] { serializationInfo, new StreamingContext(StreamingContextStates.Other) });
                                break;
                            }
                        default:
                            {
                                var ctorInfo = JaysonCtorInfo.GetDefaultCtorInfo(toType);
                                if (!ctorInfo.HasParam)
                                {
                                    result = JaysonObjectConstructor.New(toType);
                                }
                                else
                                {
                                    object paramValue;
                                    ParameterInfo ctorParam;

                                    var ctorParams = new List<object>();
                                    var matcher = context.Settings.CtorParamMatcher ?? DefaultMatcher;

                                    for (var i = 0; i < ctorInfo.ParamLength; i++)
                                    {
                                        ctorParam = ctorInfo.CtorParams[i];

                                        paramValue = matcher(ctorParam.Name, obj);

                                        if (paramValue != null)
                                        {
                                            if (paramValue.GetType() != ctorParam.ParameterType)
                                            {
                                                paramValue = ConvertObject(paramValue, ctorParam.ParameterType, context);
                                            }
                                        }
#if !(NET4000 || NET3500 || NET3000 || NET2000)
                                        else if (ctorParam.HasDefaultValue)
                                        {
                                            paramValue = ctorParam.DefaultValue;
                                        }
#else
							            else if ((ctorParam.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault)
							            {
								            paramValue = ctorParam.DefaultValue;
							            }
#endif
                                        else
                                        {
                                            paramValue = ConvertObject(null, ctorParam.ParameterType, context);
                                        }

                                        ctorParams.Add(paramValue);
                                    }

                                    result = ctorInfo.New(ctorParams.ToArray());
                                }
                                break;
                            }
                    }
                }
            }

            if (id > 0)
            {
                context.ReferenceMap[id] = result;
            }

            SetDictionary(obj, ref result, context);

#if !(NET4000 || NET3500 || NET3000 || NET2000)
            if (asReadOnly)
            {
                return GetReadOnlyDictionaryActivator(toType)(new object[] { result });
            }
#endif

            return result;
        }

        private static Type BindToType(JaysonDeserializationSettings settings, Type type)
        {
            if (settings.Binder != null)
            {
                string typeName;
                string assemblyName;
                Type instanceType = null;

#if !(NET3500 || NET3000 || NET2000)
                settings.Binder.BindToName(type, out assemblyName, out typeName);
#else
				typeName = null;
				assemblyName = null;
#endif
                if (String.IsNullOrEmpty(typeName))
                {
                    instanceType = settings.Binder.BindToType(type.Assembly.FullName, type.FullName);
                }
                else
                {
                    instanceType = settings.Binder.BindToType(assemblyName, typeName);
                }

                if (instanceType != null)
                {
                    return instanceType;
                }
            }

            var typeOverride = settings.GetTypeOverride(type);
            if (typeOverride != null)
            {
                var bindToType = typeOverride.BindToType;
                if (bindToType != null)
                {
                    return bindToType;
                }
            }

            return type;
        }

        private static int[] GetArrayRankIndices(object obj, int rank)
        {
            var result = new int[rank];
            if (obj != null)
            {
                int count;
                var index = 0;

                IList list;
                IList<object> oList;
                ICollection collection;

                do
                {
                    list = obj as IList;
                    if (list != null)
                    {
                        count = list.Count;
                        if (count > 0)
                        {
                            obj = list[0];
                        }
                    }
                    else
                    {
                        oList = obj as IList<object>;
                        if (oList != null)
                        {
                            count = oList.Count;
                            if (count > 0)
                            {
                                obj = oList[0];
                            }
                        }
                        else
                        {
                            collection = obj as ICollection;
                            if (collection != null)
                            {
                                count = collection.Count;
                                obj = null;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    result[index++] = count;
                } while (index < rank && count > 0 && obj != null);
            }
            return result;
        }

        private static object ConvertList(IList<object> obj, Type toType, JaysonDeserializationContext context)
        {
            if (toType == typeof(byte[]) && obj.Count == 1)
            {
                var item = obj[0];
                if (item is string)
                {
                    return Convert.FromBase64String((string)item);
                }
            }

            var settings = context.Settings;

            toType = BindToType(settings, toType);

            if (toType == typeof(object))
            {
                switch (settings.ArrayType)
                {
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

            bool asList, asArray, asReadOnly;
            var listType = GetEvaluatedListType(toType, out asList, out asArray, out asReadOnly);

            object result;
            var info = JaysonTypeInfo.GetTypeInfo(listType);
            if (info.Array)
            {
                var rank = info.ArrayRank;
                if (rank == 1)
                {
                    result = Array.CreateInstance(info.ElementType, obj.Count);
                }
                else
                {
                    result = Array.CreateInstance(info.ElementType, GetArrayRankIndices(obj, rank));
                }
            }
            else if (settings.ObjectActivator == null)
            {
                result = JaysonObjectConstructor.New(listType);
            }
            else
            {
                bool useDefaultCtor;
                result = settings.ObjectActivator(listType, null, out useDefaultCtor);

                if (useDefaultCtor)
                {
                    result = JaysonObjectConstructor.New(listType);
                }
            }

            SetList(obj, result, context);
            if (result == null)
            {
                return result;
            }

            var resultType = result.GetType();
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

        private static object ConvertObject(object obj, Type toType, JaysonDeserializationContext context)
        {
            toType = BindToType(context.Settings, toType);
            if (toType == typeof(object))
            {
                if (obj != null)
                {
                    if (obj is IDictionary<string, object>)
                    {
                        obj = ConvertDictionary((IDictionary<string, object>)obj, toType, context);
                    }
                    else if (obj is IList<object>)
                    {
                        obj = ConvertList((IList<object>)obj, toType, context);
                    }
                }

                return obj;
            }

            if (obj == null)
            {
                if (!(context.Settings.UseDefaultValues || toType.IsClass))
                {
                    throw new JaysonCastException();
                }

                var info = JaysonTypeInfo.GetTypeInfo(toType);
                if (info.Class || info.Nullable)
                {
                    return null;
                }

                if (context.Settings.ObjectActivator == null)
                {
                    return JaysonObjectConstructor.New(toType);
                }

                bool useDefaultCtor;
                var result = context.Settings.ObjectActivator(toType, null, out useDefaultCtor);

                if (useDefaultCtor)
                {
                    return JaysonObjectConstructor.New(toType);
                }
                return result;
            }

            var objType = obj.GetType();

            if ((toType == objType || toType.IsAssignableFrom(objType)) && 
                !(obj is IDictionary<string, object>))
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
                return ConvertDictionary((IDictionary<string, object>)obj, toType, context);
            }

            if (obj is IList<object>)
            {
                return ConvertList((IList<object>)obj, toType, context);
            }

            var settings = context.Settings;

            var tInfo = JaysonTypeInfo.GetTypeInfo(toType);
            if (!tInfo.Class)
            {
                bool converted;

                var jtc = tInfo.JTypeCode;
                if (jtc == JaysonTypeCode.DateTime)
                {
                    return JaysonCommon.TryConvertDateTime(obj, settings.DateTimeFormat, settings.DateTimeZoneType);
                }

                if (jtc == JaysonTypeCode.DateTimeNullable)
                {
                    var dt = JaysonCommon.TryConvertDateTime(obj, settings.DateTimeFormat, settings.DateTimeZoneType);

                    if (dt == default(DateTime))
                    {
                        return null;
                    }
                    return (DateTime?)dt;
                }

                var result = JaysonCommon.ConvertToPrimitive(obj, toType, out converted);
                if (converted)
                {
                    if (settings.ConvertDecimalToDouble)
                    {
                        result = ConvertDecimalToDouble(result);
                    }
                    return result;
                }

                if (JaysonTypeInfo.IsNullable(toType))
                {
                    var argType = JaysonTypeInfo.GetGenericArguments(toType)[0];
                    result = ConvertObject(obj, argType, context);

                    var constructor = toType.GetConstructor(new Type[] { argType });
                    if (constructor != null)
                    {
                        result = constructor.Invoke(new object[] { result });
                    }

                    if (settings.ConvertDecimalToDouble)
                    {
                        result = ConvertDecimalToDouble(result);
                    }

                    return result;
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
                    var instanceType = settings.Binder.BindToType(assemblyName, typeName);
                    if (instanceType != null)
                    {
                        toType = instanceType;
                    }
                }
            }
#endif

            object value;
            if (settings.ObjectActivator == null)
            {
                value = JaysonObjectConstructor.New(toType);
            }
            else
            {
                bool useDefaultCtor;
                value = settings.ObjectActivator(toType, null, out useDefaultCtor);

                if (useDefaultCtor)
                {
                    value = JaysonObjectConstructor.New(toType);
                }

                if (settings.ConvertDecimalToDouble)
                {
                    value = ConvertDecimalToDouble(value);
                }
            }

            return value;
        }

        private static object ConvertDecimalToDouble(object value)
        {
            if (value is decimal)
            {
                value = (double)((decimal)value);
            }
            else if (value is decimal?)
            {
                var d = (decimal?)value;
                value = d.HasValue ? (double)((decimal?)d.Value) : (double?)null;
            }
            return value;
        }

        # region ConvertJsonObject

        public static object ConvertJsonObject(object obj, Type toType, JaysonDeserializationSettings settings = null)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return JaysonTypeInfo.GetDefault(toType);
            }

            using (var context = new JaysonDeserializationContext
            {
                Text = String.Empty,
                Length = 0,
                Position = 0,
                Settings = settings ?? JaysonDeserializationSettings.Default
            })
            {
                return ConvertObject(obj, toType, context);
            }
        }

        public static ToType ConvertJsonObject<ToType>(object obj, JaysonDeserializationSettings settings = null)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return (ToType)JaysonTypeInfo.GetDefault(typeof(ToType));
            }

            using (var context = new JaysonDeserializationContext
            {
                Text = String.Empty,
                Length = 0,
                Position = 0,
                Settings = settings ?? JaysonDeserializationSettings.Default
            })
            {
                return (ToType)ConvertObject(obj, typeof(ToType), context);
            }
        }

        # endregion ConvertJsonObject

        # endregion Convert

        # region ToX

        public static IDictionary<string, object> ToDictionary(string str, JaysonDeserializationSettings settings = null)
        {
            if (String.IsNullOrEmpty(str))
            {
                throw new JaysonException(JaysonError.EmptyString);
            }

            if (settings == null)
            {
                settings = new JaysonDeserializationSettings();
                JaysonDeserializationSettings.Default.AssignTo(settings);
                settings.DictionaryType = DictionaryDeserializationType.Dictionary;
            }

            using (var context = new JaysonDeserializationContext
                {
                    Text = str,
                    Length = str.Length,
                    Settings = settings ?? JaysonDeserializationSettings.Default
                })
            {
                var result = ParseDictionary(context);
                if (result != null)
                {
                    object typesObj;
                    if (result.TryGetValue("$types", out typesObj))
                    {
                        result.TryGetValue("$value", out typesObj);
                        result = typesObj as IDictionary<string, object>;
                    }
                }

                return result;
            }
        }

        public static T ToObject<T>(string str, JaysonDeserializationSettings settings = null)
        {
            if (String.IsNullOrEmpty(str))
            {
                if (((settings != null) && settings.UseDefaultValues) || typeof(T).IsClass)
                {
                    return default(T);
                }
                throw new JaysonCastException();
            }
            return (T)ToObject(str, typeof(T), settings);
        }

        public static object ToObject(string str, JaysonDeserializationSettings settings = null)
        {
            return ToObject(str, typeof(object), settings);
        }

        public static object ToObject(string str, Type toType, JaysonDeserializationSettings settings = null)
        {
            if (toType == null)
            {
                throw new ArgumentNullException("toType");
            }

            if (String.IsNullOrEmpty(str))
            {
                if (((settings != null) && settings.UseDefaultValues) || toType.IsClass)
                {
                    return JaysonTypeInfo.GetDefault(toType);
                }
                throw new JaysonCastException();
            }

            using (var context = new JaysonDeserializationContext
            {
                Text = str,
                Length = str.Length,
                Position = 0,
                Settings = settings ?? JaysonDeserializationSettings.Default
            })
            {
                var result = Parse(context);

                if (result == null)
                {
                    if (context.Settings.UseDefaultValues || toType.IsClass)
                    {
                        return JaysonTypeInfo.GetDefault(toType);
                    }
                    throw new JaysonCastException();
                }

                var dResult = result as IDictionary<string, object>;
                if (dResult != null)
                {
                    object typesObj;
                    if (dResult.TryGetValue("$types", out typesObj))
                    {
                        context.GlobalTypes = new JaysonDeserializationTypeList(typesObj as IDictionary<string, object>);
                        dResult.TryGetValue("$value", out result);
                    }
                }

                var instanceType = result.GetType();
                var instanceInfo = JaysonTypeInfo.GetTypeInfo(instanceType);

                if (!context.HasTypeInfo)
                {
                    var tc = instanceInfo.JTypeCode;
                    
                    if (((JaysonTypeCode.AllButNotObject & tc) == tc) &&
                        (toType == instanceType || toType == typeof(object) || toType.IsAssignableFrom(instanceType)))
                    {
                        return result;
                    }
                    return ConvertObject(result, toType, context);
                }

                var toInfo = JaysonTypeInfo.GetTypeInfo(toType);

                if (toInfo.JPrimitive)
                {
                    if (toInfo.Type == instanceInfo.Type)
                    {
                        return result;
                    }

                    if (context.HasTypeInfo)
                    {
                        var primeDict = result as IDictionary<string, object>;
                        if (primeDict != null)
                        {
                            return ConvertDictionary(primeDict, toType, context, true);
                        }
                    }

                    bool converted;
                    return JaysonCommon.ConvertToPrimitive(result, toType, out converted);
                }

                var asReadOnly = false;
                if (result is IList<object>)
                {
                    bool asList, asArray;
                    var listType = GetEvaluatedListType(toType, out asList, out asArray, out asReadOnly);

                    result = ConvertList((IList<object>)result, listType, context);
                    if (result == null)
                    {
                        return result;
                    }

                    var resultType = result.GetType();
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
                    result = ConvertDictionary((IDictionary<string, object>)result, dictionaryType, context, true);
                }
                else if (result is IDictionary)
                {
                    bool asDictionary;
                    var dictionaryType = GetEvaluatedDictionaryType(toType, out asDictionary, out asReadOnly);
                    result = ConvertDictionary((IDictionary<string, object>)result, dictionaryType, context);
                }
                else if (result is IList)
                {
                    result = ConvertList((IList<object>)result, toType, context);
                }
                else
                {
                    result = ConvertObject(result, toType, context);
                }

                if (result == null)
                {
                    return result;
                }

                var resultTypeD = result.GetType();
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

                throw new JaysonCastException();
            }
        }

#if !(NET3500 || NET3000 || NET2000)
        public static dynamic ToDynamic(string str)
        {
            return ToExpando(str);
        }

        public static ExpandoObject ToExpando(string str, JaysonDeserializationSettings settings = null)
        {
            if (settings == null)
            {
                settings = new JaysonDeserializationSettings();
                JaysonDeserializationSettings.Default.AssignTo(settings);
                settings.DictionaryType = DictionaryDeserializationType.Expando;
            }
            else if (settings.DictionaryType != DictionaryDeserializationType.Expando)
            {
                var temp = new JaysonDeserializationSettings();
                settings.AssignTo(temp);
                temp = settings;
                settings.DictionaryType = DictionaryDeserializationType.Expando;
            }

            var result = ToDictionary(str, settings);

            if (result != null)
            {
                object typesObj;
                if (result.TryGetValue("$types", out typesObj))
                {
                    result.TryGetValue("$value", out typesObj);
                    result = typesObj as IDictionary<string, object>;
                }
            }

            return (ExpandoObject)result;
        }
#endif

        # endregion ToX
    }

    # endregion JsonConverter
}
