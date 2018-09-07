using System.Collections.Generic;

#if EFCORE
namespace DbContextValidation.EFCore
#else
namespace DbContextValidation.EF6
#endif
{
    internal class TableInfo
    {
        public TableInfo(string schema, string table, IReadOnlyCollection<string> columnNames)
        {
            Schema = schema;
            Table = table;
            ColumnNames = columnNames;
        }

        public string Schema { get; }
        public string Table { get; }
        public IReadOnlyCollection<string> ColumnNames { get; }

        public override string ToString()
        {
            var schemaPrefix = string.IsNullOrEmpty(Schema) ? "" : Schema + ".";
            return $"{schemaPrefix}{Table} {string.Join(",", ColumnNames)}";
        }
    }
}