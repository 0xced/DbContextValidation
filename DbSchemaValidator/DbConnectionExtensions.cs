using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;

#if EFCORE
namespace DbSchemaValidator.EFCore
#else
namespace DbSchemaValidator.EF6
#endif
{
    internal static class DbConnectionExtensions
    {
        internal static async Task<TableInfo> GetTableInfo(this DbConnection connection, SelectStatement selectStatement, string schema, string tableName)
        {
            var columnNames = new List<string>();
            bool? caseSensitive;
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
                        try
                        {
                            caseSensitive = reader.GetSchemaTable()?.CaseSensitive;
                        }
                        catch (Exception)
                        {
                            caseSensitive = null;
                        }
                    }
                }
            }
            catch (DbException exception)
            {
                throw new TableNotFoundException(exception, commandText);
            }
            finally
            {
                if (wasClosed)
                    connection.Close();
            }
            return new TableInfo(schema, tableName, columnNames, caseSensitive);
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static DbProviderFactory GetProviderFactory(this DbConnection connection)
        {
            var dbProviderFactoryProperty = connection.GetType().GetProperty("DbProviderFactory", BindingFlags.NonPublic | BindingFlags.Instance);
            return dbProviderFactoryProperty?.GetValue(connection) is DbProviderFactory dbProviderFactory ? dbProviderFactory : null;
        }
    }
}