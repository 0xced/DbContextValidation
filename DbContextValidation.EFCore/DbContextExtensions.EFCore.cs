using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DbContextValidation.EFCore
{
    /// <summary>
    /// Extends the <see cref="DbContext"/> class by providing a method for getting a simplified model of the tables.
    /// </summary>
    public static class DbContextExtensions
    {
        private static readonly ConcurrentDictionary<DbContext, IReadOnlyCollection<Table>> ModelTablesCache = new ConcurrentDictionary<DbContext, IReadOnlyCollection<Table>>();

        /// <param name="context">The context</param>
        /// <returns>A collection of the database tables defined in the given context.</returns>
        public static IReadOnlyCollection<Table> GetModelTables(this DbContext context)
        {
            return ModelTablesCache.GetOrAdd(context, ctx =>
            {
                var tables = new List<Table>();
                foreach (var entityType in ctx.Model.GetEntityTypes())
                {
                    var schema = entityType.Relational().Schema;
                    var tableName = entityType.Relational().TableName;
                    var columnNames = entityType.GetProperties().Select(e => e.Relational().ColumnName);
                    var table = new Table(schema, tableName, columnNames.ToList());
                    tables.Add(table);
                }
                return tables;
            });
        }

        internal static DbConnection GetDbConnection(this DbContext context)
        {
            return context.Database.GetDbConnection();
        }
    }
}