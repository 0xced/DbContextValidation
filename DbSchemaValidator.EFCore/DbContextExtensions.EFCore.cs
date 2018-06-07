using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DbSchemaValidator.EFCore
{
    public static partial class DbContextExtensions
    {
        private static IDictionary<string, IReadOnlyCollection<string>> GetDbModel(this DbContext context)
        {
            var model = new Dictionary<string, IReadOnlyCollection<string>>();
            foreach (var entityType in context.Model.GetEntityTypes())
            {
                var tableName = entityType.Relational().TableName;
                var columnNames = entityType.GetProperties().Select(e => e.Relational().ColumnName);
                model.Add(tableName, columnNames.ToList());
            }
            return model;
        }

        private static DbConnection GetDbConnection(this DbContext context)
        {
            return context.Database.GetDbConnection();
        }
    }
}