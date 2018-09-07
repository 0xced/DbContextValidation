using System.Collections.Generic;

#if EFCORE
namespace DbContextValidation.EFCore
#else
namespace DbContextValidation.EF6
#endif
{
    internal class Table
    {
        public Table(string schema, string tableName, IReadOnlyCollection<string> columnNames)
        {
            Schema = schema;
            TableName = tableName;
            ColumnNames = columnNames;
        }

        public string Schema { get; }
        public string TableName { get; }
        public IReadOnlyCollection<string> ColumnNames { get; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Schema) ? TableName : Schema + "." + TableName;
        }
    }
}