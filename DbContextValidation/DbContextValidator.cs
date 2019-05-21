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
        private readonly IEqualityComparer<string> _columnNameEqualityComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextValidator"/> class.
        /// </summary>
        /// <param name="columnNameEqualityComparer">An equality comparer used to compare column names defined in the model against the actual column names. </param>
        public DbContextValidator(IEqualityComparer<string> columnNameEqualityComparer)
        {
            _columnNameEqualityComparer = columnNameEqualityComparer ?? throw new ArgumentNullException(nameof(columnNameEqualityComparer));
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
        /// <returns>The table for the given schema and name.</returns>
        /// <exception cref="TableNotFoundException">The table does not exist.</exception>
        protected virtual async Task<Table> GetTableAsync(DbConnection connection, string schema, string tableName)
        {
            return await connection.GetTableAsync(schema, tableName);
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
                var expectedColumnNames = modelTable.ColumnNames;
                try
                {
                    var databaseTable = await GetTableAsync(context.GetDbConnection(), schema, tableName);
                    var missingColumns = expectedColumnNames.Except(databaseTable.ColumnNames, _columnNameEqualityComparer).ToList();
                    if (missingColumns.Any())
                    {
                        errors.Add(new MissingColumnsError(modelTable, missingColumns));
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