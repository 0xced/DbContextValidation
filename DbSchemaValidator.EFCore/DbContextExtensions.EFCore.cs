using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DbSchemaValidator.EFCore
{
    internal static class DbContextExtensions
    {
        internal static IDictionary<(string schema, string tableName), IReadOnlyCollection<string>> GetDbModel(this DbContext context)
        {
            var model = new Dictionary<(string schema, string tableName), IReadOnlyCollection<string>>();
            foreach (var entityType in context.Model.GetEntityTypes())
            {
                var schema = entityType.Relational().Schema;
                var tableName = entityType.Relational().TableName;
                var columnNames = entityType.GetProperties().Select(e => e.Relational().ColumnName);
                model.Add((schema, tableName), columnNames.ToList());
            }
            return model;
        }

        internal static DbConnection GetDbConnection(this DbContext context)
        {
            return context.Database.GetDbConnection();
        }
    }
}