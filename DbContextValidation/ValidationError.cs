#if EFCORE
namespace DbContextValidation.EFCore
#else
namespace DbContextValidation.EF6
#endif
{
    /// <summary>
    /// Represents a validation error, i.e. when a table defined in the DbContext model does not match the corresponding table in the actual database.
    /// Concrete errors are instances of either the <see cref="MissingTable"/> or the <see cref="MissingColumns"/> class.
    /// </summary>
    public abstract class ValidationError
    {
        internal ValidationError(string schema, string tableName)
        {
            Schema = schema;
            TableName = tableName;
        }

        /// <summary>
        /// The schema of the table. May be <code>null</code> as some providers (e.g. SQLite, MySQL) do not support schemata.
        /// </summary>
        public string Schema { get; }
        
        /// <summary>
        /// The name of the table.
        /// </summary>
        public string TableName { get; }

        internal string TableDescription => string.IsNullOrEmpty(Schema) ? TableName : $"{Schema}.{TableName}";
    }
}