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
        internal static async Task<IReadOnlyCollection<string>> GetColumnNames(this DbConnection connection, string tableName)
        {
            var columnNames = new List<string>();
            var wasClosed = connection.State == ConnectionState.Closed;
            if (wasClosed)
                await connection.OpenAsync();

            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT * FROM {tableName} WHERE 1=0";
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
            finally
            {
                if (wasClosed)
                    connection.Close();
            }
            return columnNames;
        }
    }
}