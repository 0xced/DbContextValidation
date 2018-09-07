using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DbContextValidation.EFCore
{
    internal static class DbContextExtensions
    {
        internal static IReadOnlyCollection<Table> GetModelTables(this DbContext context)
        {
            var tables = new List<Table>();
            foreach (var entityType in context.Model.GetEntityTypes())
            {
                var schema = entityType.Relational().Schema;
                var tableName = entityType.Relational().TableName;
                var columnNames = entityType.GetProperties().Select(e => e.Relational().ColumnName);
                var table = new Table(schema, tableName, columnNames.ToList());
                tables.Add(table);
            }
            return tables;
        }

        internal static DbConnection GetDbConnection(this DbContext context)
        {
            return context.Database.GetDbConnection();
        }
    }
}