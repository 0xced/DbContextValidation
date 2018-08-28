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
        internal InvalidMapping(string schema, string tableName, IReadOnlyCollection<string> missingColumns, TableNotFoundException exception)
        {
            Schema = schema;
            TableName = tableName;
            MissingColumns = missingColumns;
            MissingTableException = exception;
        }

        /// <summary>
        /// The schema of the table.  
        /// </summary>
        public string Schema { get; }
        
        /// <summary>
        /// The name of the table.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// The collection of column names which is defined in the DbContext model but not found in the actual database.
        /// This collection is never empty. If <code>null</code>, the table itself is missing and <see cref="MissingTableException"/> is not <code>null</code>. 
        /// </summary>
        public IReadOnlyCollection<string> MissingColumns { get; }
        
        /// <summary>
        /// If <see cref="MissingColumns"/> is <code>null</code>, contains the exception that occured when the select statement to get the actual column names was issued to the database.
        /// May be useful to understand why a table is missing.
        /// </summary>
        public TableNotFoundException MissingTableException { get; }
        
        /// <returns>A description of the invalid mapping including the table name and all its missing columns.</returns>
        public override string ToString()
        {
            return MissingColumns == null
                ? $"Table {TableDescription} is missing"
                : $"Table {TableDescription} is missing {MissingColumns.Count} column{(MissingColumns.Count > 1 ? "s" : "")}: {string.Join(",", MissingColumns)}";
        }

        private string TableDescription => string.IsNullOrEmpty(Schema) ? TableName : $"{Schema}.{TableName}";
    }
}