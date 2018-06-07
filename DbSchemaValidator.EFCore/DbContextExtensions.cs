using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DbSchemaValidator.EFCore
{
    public static class DbContextExtensions
    {
        public static async Task<IReadOnlyCollection<InvalidMapping>> ValidateSchema(this DbContext context)
        {
            var invalidMappings = new List<InvalidMapping>();
            foreach (var entityType in context.Model.GetEntityTypes())
            {
                var tableName = entityType.Relational().TableName;
                var expectedColumnNames = entityType.GetProperties().Select(e => e.Relational().ColumnName).ToList();
                var missingColumns = new List<string>();
                try
                {
                    var actualColumnNames = await context.Database.GetDbConnection().GetColumnNames(tableName);
                    missingColumns = expectedColumnNames.Except(actualColumnNames).ToList();
                }
                catch (DbException)
                {
                    invalidMappings.Add(new InvalidMapping(tableName, missingColumns: null));
                }
                if (missingColumns.Any())
                {
                    invalidMappings.Add(new InvalidMapping(tableName, missingColumns));
                }
            }
            return invalidMappings;
        }
    }
}