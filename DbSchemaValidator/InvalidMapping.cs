using System.Collections.Generic;

#if EFCORE
namespace DbSchemaValidator.EFCore
#else
namespace DbSchemaValidator.EF6
#endif
{
    public class InvalidMapping
    {
        public InvalidMapping(string tableName, IReadOnlyCollection<string> missingColumns)
        {
            TableName = tableName;
            MissingColumns = missingColumns;
        }

        public string TableName { get; }
        public IReadOnlyCollection<string> MissingColumns { get; }

        public override string ToString()
        {
            return MissingColumns == null
                ? $"Table {TableName} is missing"
                : $"Table {TableName} is missing {MissingColumns.Count} column{(MissingColumns.Count > 1 ? "s" : "")}: {string.Join(",", MissingColumns)}";
        }
    }
}