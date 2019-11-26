using System.Collections.Generic;

#if EFCORE
namespace DbContextValidation.EFCore
#else
namespace DbContextValidation.EF6
#endif
{
    /// <summary>
    /// Represents a table with missing column(s).
    /// </summary>
    public class MissingColumnsError : ValidationError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingColumnsError"/> class.
        /// </summary>
        /// <param name="table">The table with missing columns that is defined in the DbContext model.</param>
        /// <param name="columnNames">The names of the missing columns.</param>
        public MissingColumnsError(Table table, IReadOnlyCollection<string> columnNames) : base(table)
        {
            ColumnNames = columnNames;
        }

        /// <summary>
        /// The collection of column names which is defined in the DbContext model but not found in the actual database.
        /// This collection is never empty. 
        /// </summary>
        public IReadOnlyCollection<string> ColumnNames { get; }

        /// <returns>A sentence describing the table name and all its missing columns.</returns>
        public override string ToString()
        {
            return $"Table {Table} is missing {ColumnNames.Count} column{(ColumnNames.Count > 1 ? "s" : "")}: {string.Join(",", ColumnNames)}";
        }
    }
}