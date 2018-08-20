using System.Collections.Generic;

#if EFCORE
namespace DbSchemaValidator.EFCore
#else
namespace DbSchemaValidator.EF6
#endif
{
    /// <summary>
    /// Represents an invalid mapping, i.e. a mapping where the actual table and column names do not match the names defined in the DbContext model.
    /// </summary>
    public class InvalidMapping
    {
        internal InvalidMapping(string tableName, IReadOnlyCollection<string> missingColumns)
        {
            TableName = tableName;
            MissingColumns = missingColumns;
        }

        /// <summary>
        /// The name of the table.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// The collection of column names which is defined in the DbContext model but not found in the actual database. If <code>null</code>, the table itself is missing.
        /// This collection can never be empty. 
        /// </summary>
        public IReadOnlyCollection<string> MissingColumns { get; }

        /// <returns>A description of the invalid mapping including the table name and all its missing columns.</returns>
        public override string ToString()
        {
            return MissingColumns == null
                ? $"Table {TableName} is missing"
                : $"Table {TableName} is missing {MissingColumns.Count} column{(MissingColumns.Count > 1 ? "s" : "")}: {string.Join(",", MissingColumns)}";
        }
    }
}