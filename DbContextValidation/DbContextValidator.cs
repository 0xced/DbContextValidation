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
    /// <summary>
    /// A delegate to construct a select statement used to retrieve all column names in a database.
    /// </summary>
    /// <param name="schema">The schema of the table. May be <code>null</code> as some providers (e.g. SQLite, MySQL) do not support schemata.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="commandBuilder">The <see cref="DbCommandBuilder"/> of the provider, may be <code>null</code>.</param>
    public delegate string SelectStatement(string schema, string tableName, DbCommandBuilder commandBuilder);

    /// <inheritdoc />
    public class DbContextValidator : IDbContextValidator
    {
        private readonly IEqualityComparer<string> _columnNameEqualityComparer;
        private readonly SelectStatement _selectStatement;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextValidator"/> class.
        /// </summary>
        /// <param name="columnNameEqualityComparer">An equality comparer used to compare column names defined in the model against the actual column names. </param>
        /// <param name="selectStatement">A <see cref="SelectStatement"/> delegate. If null, uses a default implementation that should work with most providers. If you specify a select statement delegate, make sure to select all columns with <code>*</code> and also make sure that no rows will be returned at all by including a <code>WHERE 1=0</code> clause in order to keep the validation efficient.</param>
        public DbContextValidator(IEqualityComparer<string> columnNameEqualityComparer, SelectStatement selectStatement = null)
        {
            _columnNameEqualityComparer = columnNameEqualityComparer ?? throw new ArgumentNullException(nameof(columnNameEqualityComparer));
            _selectStatement = selectStatement ?? DefaultSelectStatement;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<ValidationError>> ValidateContextAsync(DbContext context, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            var errors = new List<ValidationError>();
            var modelTables = context.GetModelTables();
            var i = 0;
            foreach (var modelTable in modelTables)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var schema = modelTable.Schema;
                var tableName = modelTable.TableName;
                var expectedColumnNames = modelTable.ColumnNames;
                try
                {
                    var databaseTable = await context.GetDbConnection().GetTableAsync(_selectStatement, schema, tableName);
                    var missingColumns = expectedColumnNames.Except(databaseTable.ColumnNames, _columnNameEqualityComparer).ToList();
                    if (missingColumns.Any())
                    {
                        errors.Add(new MissingColumnsError(schema, tableName, missingColumns));
                    }
                }
                catch (TableNotFoundException exception)
                {
                    errors.Add(new MissingTableError(schema, tableName, exception));
                }
                progress?.Report(++i / (float)modelTables.Count);
            }
            return errors;
        }
        
        private static string DefaultSelectStatement(string schema, string tableName, DbCommandBuilder commandBuilder)
        {
            var hasSchema = !string.IsNullOrEmpty(schema);
            var quotedSchema = hasSchema ? commandBuilder?.QuoteIdentifier(schema) ?? schema : null;
            var quotedTableName = commandBuilder?.QuoteIdentifier(tableName) ?? tableName;
            var schemaSeparator = commandBuilder?.SchemaSeparator ?? ".";
            var tableDescription = hasSchema ? quotedSchema + schemaSeparator + quotedTableName : quotedTableName;
            return $"SELECT * FROM {tableDescription} WHERE 1=0";
        }
    }
}