using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if EFCORE
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif

#if EFCORE
namespace DbContextValidation.EFCore
#else
namespace DbContextValidation.EF6
#endif
{
    /// <inheritdoc />
    public class DbContextValidator : IDbContextValidator
    {
        private readonly IEqualityComparer<DbColumn> _columnEqualityComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextValidator"/> class.
        /// </summary>
        /// <param name="columnEqualityComparer">An equality comparer used to compare columns defined in the model against the actual columns.</param>
        public DbContextValidator(IEqualityComparer<DbColumn> columnEqualityComparer)
        {
            _columnEqualityComparer = columnEqualityComparer ?? throw new ArgumentNullException(nameof(columnEqualityComparer));
        }

        /// <param name="context">The context</param>
        /// <returns>An enumerable collection of the database tables defined in the given context.</returns>
        protected virtual IEnumerable<Table> GetModelTables(DbContext context)
        {
            return context.GetModelTables();
        }

        /// <param name="connection">The database connection.</param>
        /// <param name="schema">The schema of the table. May be <code>null</code> as some providers (e.g. SQLite, MySQL) do not support schemata.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>The table for the given schema and name.</returns>
        /// <exception cref="TableNotFoundException">The table does not exist.</exception>
        protected virtual async Task<Table> GetTableAsync(DbConnection connection, string schema, string tableName, CancellationToken cancellationToken)
        {
            return await connection.GetTableAsync(schema, tableName, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<ValidationError>> ValidateContextAsync(DbContext context, IProgress<Table> progress = null, CancellationToken cancellationToken = default)
        {
            var errors = new List<ValidationError>();
            var modelTables = GetModelTables(context);
            foreach (var modelTable in modelTables)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var schema = modelTable.Schema;
                var tableName = modelTable.TableName;
                var expectedColumns = modelTable.Columns;
                try
                {
                    var databaseTable = await GetTableAsync(context.GetDbConnection(), schema, tableName, cancellationToken);
                    var missingColumns = expectedColumns.Except(databaseTable.Columns, _columnEqualityComparer).ToList();
                    if (missingColumns.Any())
                    {
                        errors.Add(new MissingColumnsError(modelTable, missingColumns.Select(e => e.ColumnName).ToList()));
                    }
                }
                catch (TableNotFoundException exception)
                {
                    errors.Add(new MissingTableError(modelTable, exception));
                }
                progress?.Report(modelTable);
            }
            return errors;
        }
    }
}