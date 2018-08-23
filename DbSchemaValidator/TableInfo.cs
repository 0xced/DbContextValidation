﻿using System.Collections.Generic;

#if EFCORE
namespace DbSchemaValidator.EFCore
#else
namespace DbSchemaValidator.EF6
#endif
{
    internal class TableInfo
    {
        public TableInfo(string schema, string table, IReadOnlyCollection<string> columnNames, bool? caseSensitive)
        {
            Schema = schema;
            Table = table;
            ColumnNames = columnNames;
            CaseSensitive = caseSensitive;
        }

        public string Schema { get; }
        public string Table { get; }
        public IReadOnlyCollection<string> ColumnNames { get; }
        public bool? CaseSensitive { get; }
    }
}