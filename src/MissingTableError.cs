#if EFCORE
namespace DbContextValidation.EFCore
#else
namespace DbContextValidation.EF6
#endif
{
    /// <summary>
    /// Represents a missing table, i.e. a table in the DbContext model which was not found in the actual database.
    /// </summary>
    public class MissingTableError : ValidationError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingTableError"/> class.
        /// </summary>
        /// <param name="table">The missing table that is defined in the DbContext model.</param>
        /// <param name="exception">The exception that occured when trying to get the table.</param>
        public MissingTableError(Table table, TableNotFoundException exception) : base(table)
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
            return $"Table {Table} is missing";
        }
    }
}