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
        internal static async Task<Table> GetTableAsync(this DbConnection connection, string schema, string tableName, CancellationToken cancellationToken)
        {
            var columns = new List<DbColumn>();
            var wasClosed = connection.State == ConnectionState.Closed;
            if (wasClosed)
                await connection.OpenAsync(cancellationToken);

            var dbProviderFactory = connection.GetProviderFactory();
            var commandBuilder = dbProviderFactory.CreateCommandBuilder();
            var commandText = SelectStatement(schema, tableName, commandBuilder);
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = commandText;
                    command.CommandType = CommandType.Text;
                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        if (reader.CanGetColumnSchema())
                        {
                            columns.AddRange(reader.GetColumnSchema());
                        }
                        else
                        {
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                var columnName = reader.GetName(i);
                                var columnOrdinal = reader.GetOrdinal(columnName);
                                var dataType = reader.GetFieldType(columnOrdinal);
                                var dataTypeName = reader.GetDataTypeName(columnOrdinal);
                                columns.Add(new ModelColumn(columnName, columnOrdinal, dataType, dataTypeName));
                            }
                        }
                    }
                }
            }
            catch (DbException exception)
            {
                throw new TableNotFoundException(schema, tableName, exception, commandText);
            }
            finally
            {
                if (wasClosed)
#if NETSTANDARD2_1
                    await connection.CloseAsync();
#else
                    connection.Close();
#endif
            }
            return new Table(schema, tableName, columns);
        }

        /// <param name="schema">The schema of the table. May be <code>null</code> as some providers (e.g. SQLite, MySQL) do not support schemata.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="commandBuilder">The <see cref="DbCommandBuilder"/> of the provider, may be <code>null</code>.</param>
        /// <returns>A select statement used to retrieve all column names in a database.</returns>
        private static string SelectStatement(string schema, string tableName, DbCommandBuilder commandBuilder)
        {
            var hasSchema = !string.IsNullOrEmpty(schema);
            var quotedSchema = hasSchema ? commandBuilder?.QuoteIdentifier(schema) ?? schema : null;
            var quotedTableName = commandBuilder?.QuoteIdentifier(tableName) ?? tableName;
            var schemaSeparator = commandBuilder?.SchemaSeparator ?? ".";
            var tableDescription = hasSchema ? quotedSchema + schemaSeparator + quotedTableName : quotedTableName;
            return $"SELECT * FROM {tableDescription} WHERE 1=0";
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static DbProviderFactory GetProviderFactory(this DbConnection connection)
        {
#if EFCORE && (NETSTANDARD2_0 || NETCOREAPP2_0)
            // DbProviderFactories was introduced in netcoreapp2.1 but it is easy to get the DbProviderFactory through reflection
            var type = connection.GetType();
            var dbProviderFactoryProperty = type.GetProperty("DbProviderFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dbProviderFactoryProperty == null)
                throw new System.MissingFieldException(type.FullName, "DbProviderFactory");
            return (DbProviderFactory)dbProviderFactoryProperty.GetValue(connection);
#else
            return DbProviderFactories.GetFactory(connection);
#endif
        }
    }
}