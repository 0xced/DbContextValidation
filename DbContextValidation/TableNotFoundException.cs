using System;
using System.Data.Common;
using System.Runtime.Serialization;

#if EFCORE
namespace DbContextValidation.EFCore
#else
namespace DbContextValidation.EF6
#endif
{
    /// <summary>
    /// Represents errors that occur while trying to get the list of column names for a given table.
    /// </summary>
    /// <seealso cref="MissingTableError.MissingTableException"/>
    [Serializable]
    public sealed class TableNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableNotFoundException"/> class.
        /// </summary>
        /// <param name="schema">The schema of the table.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="dbException">The DbException that was thrown while retrieving the table information.</param>
        /// <param name="selectStatement">The select statement that was issued to the database that generated the <see cref="DbException"/>.</param>
        public TableNotFoundException(string schema, string tableName, DbException dbException, string selectStatement) : base($"{schema}{(string.IsNullOrEmpty(schema) ? "" : ".")}{tableName} not found", dbException)
        {
            DbException = dbException;
            SelectStatement = selectStatement;
        }

        /// <inheritdoc />
        private TableNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            DbException = (DbException)info.GetValue(nameof(DbException), typeof(DbException));
            SelectStatement = info.GetString(nameof(SelectStatement));
        }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(DbException), DbException, typeof(DbException));
            info.AddValue(nameof(SelectStatement), SelectStatement);
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