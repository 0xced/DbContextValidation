using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

#if EFCORE
namespace DbContextValidation.EFCore
#else
namespace DbContextValidation.EF6
#endif
{
    internal static class DbConnectionExtensions
    {
        internal static async Task<Table> GetTable(this DbConnection connection, SelectStatement selectStatement, string schema, string tableName)
        {
            var columnNames = new List<string>();
            var wasClosed = connection.State == ConnectionState.Closed;
            if (wasClosed)
                await connection.OpenAsync();

            var dbProviderFactory = connection.GetProviderFactory();
            var commandBuilder = dbProviderFactory?.CreateCommandBuilder();
            var commandText = selectStatement(schema, tableName, commandBuilder);
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = commandText;
                    command.CommandType = CommandType.Text;
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            columnNames.Add(reader.GetName(i));
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
                    connection.Close();
            }
            return new Table(schema, tableName, columnNames);
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static DbProviderFactory GetProviderFactory(this DbConnection connection)
        {
#if EFCORE && (NETSTANDARD2_0 || NETCOREAPP2_0)
            // DbProviderFactories was introduced in netcoreapp2.1 but it is easy to get the DbProviderFactory through reflection 
            var dbProviderFactoryProperty = connection.GetType().GetProperty("DbProviderFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return dbProviderFactoryProperty?.GetValue(connection) is DbProviderFactory dbProviderFactory ? dbProviderFactory : null;
#else
            return DbProviderFactories.GetFactory(connection);
#endif
        }
    }
}