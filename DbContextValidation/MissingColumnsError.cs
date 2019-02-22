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
        internal MissingColumnsError(Table table, IReadOnlyCollection<string> columnNames) : base(table)
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