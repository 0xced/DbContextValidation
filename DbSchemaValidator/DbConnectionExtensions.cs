using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

#if EFCORE
namespace DbSchemaValidator.EFCore
#else
namespace DbSchemaValidator.EF6
#endif
{
    internal static class DbConnectionExtensions
    {
        internal static async Task<TableInfo> GetTableInfo(this DbConnection connection, string selectStatement, string schema, string tableName)
        {
            var columnNames = new List<string>();
            bool? caseSensitive;
            var wasClosed = connection.State == ConnectionState.Closed;
            if (wasClosed)
                await connection.OpenAsync();

            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = selectStatement;
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
                throw new TableNotFoundException(exception);
            }
            finally
            {
                if (wasClosed)
                    connection.Close();
            }
            return new TableInfo(schema, tableName, columnNames, caseSensitive);
        }
    }
}