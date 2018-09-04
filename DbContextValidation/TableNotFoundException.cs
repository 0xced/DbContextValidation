using System;
using System.Data.Common;

#if EFCORE
namespace DbContextValidation.EFCore
#else
namespace DbContextValidation.EF6
#endif
{
    /// <summary>
    /// Represents errors that occur while trying to get the list of column names for a given table. <seealso cref="InvalidMapping.MissingTableException"/>
    /// </summary>
    public class TableNotFoundException : Exception
    {
        internal TableNotFoundException(string schema, string tableName, DbException dbException, string selectStatement) : base($"{schema}{(string.IsNullOrEmpty(schema) ? "" : ".")}{tableName} not found", dbException)
        {
            DbException = dbException;
            SelectStatement = selectStatement;
        }
        
        /// <summary>
        /// The DbException that was thrown while executing the <see cref="SelectStatement"/>.
        /// </summary>
        public DbException DbException { get; }
        
        /// <summary>
        /// The select statement that was issued to the database that generated the <see cref="DbException"/>.
        /// </summary>
        public string SelectStatement { get; }
    }
}