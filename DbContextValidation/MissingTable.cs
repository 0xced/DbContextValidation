#if EFCORE
namespace DbContextValidation.EFCore
#else
namespace DbContextValidation.EF6
#endif
{
    /// <summary>
    /// Represents a missing table, i.e. a table in the DbContext model which was not found in the actual database.
    /// </summary>
    public class MissingTable : ValidationError
    {
        internal MissingTable(string schema, string tableName, TableNotFoundException exception) : base(schema, tableName)
        {
            MissingTableException = exception;
        }
        
        /// <summary>
        /// Contains the exception that occured when the select statement to get the actual column names was issued to the database.
        /// Especially useful to diagnose why a table is missing.
        /// </summary>
        public TableNotFoundException MissingTableException { get; }
        
        /// <returns>A sentence describing the missing table.</returns>
        public override string ToString()
        {
            return $"Table {TableDescription} is missing";
        }
    }
}