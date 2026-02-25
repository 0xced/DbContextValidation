using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

#if EFCORE
namespace DbContextValidation.EFCore
#else
namespace DbContextValidation.EF6
#endif
{
    internal static class DbConnectionExtensions
    {
        internal static async Task<Table> GetTableAsync(this DbConnection connection, string? schema, string tableName, CancellationToken cancellationToken)
        {
            var columnNames = new List<string>();
            var wasClosed = connection.State == ConnectionState.Closed;
            if (wasClosed)
                await connection.OpenAsync(cancellationToken);

            var dbProviderFactory = DbProviderFactories.GetFactory(connection);
            var commandBuilder = dbProviderFactory?.CreateCommandBuilder();
            var commandText = SelectStatement(schema, tableName, commandBuilder);
            try
            {
#if !NET45
                await
#endif
                using var command = connection.CreateCommand();
                command.CommandText = commandText;
                command.CommandType = CommandType.Text;
#if !NET45
                await
#endif
                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    columnNames.Add(reader.GetName(i));
                }
            }
            catch (DbException exception)
            {
                throw new TableNotFoundException(schema, tableName, exception, commandText);
            }
            finally
            {
                if (wasClosed)
#if NET45
                    connection.Close();
#else
                    await connection.CloseAsync();
#endif
            }
            return new Table(schema, tableName, columnNames);
        }

        /// <param name="schema">The schema of the table. May be <see langword="null"/> as some providers (e.g., SQLite, MySQL) do not support schemata.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="commandBuilder">The <see cref="DbCommandBuilder"/> of the provider, may be <see langword="null"/>.</param>
        /// <returns>A select statement used to retrieve all column names in a database.</returns>
        private static string SelectStatement(string? schema, string tableName, DbCommandBuilder? commandBuilder)
        {
            var quotedSchema = string.IsNullOrEmpty(schema) ? null : commandBuilder?.QuoteIdentifier(schema) ?? schema;
            var quotedTableName = commandBuilder?.QuoteIdentifier(tableName) ?? tableName;
            var schemaSeparator = commandBuilder?.SchemaSeparator ?? ".";
            var tableDescription = string.IsNullOrEmpty(quotedSchema) ? quotedTableName : quotedSchema + schemaSeparator + quotedTableName;
            return $"SELECT * FROM {tableDescription} WHERE 1=0";
        }
    }
}