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
        /// <param name="context">The context</param>
        /// <returns>An enumerable collection of the database tables defined in the given context.</returns>
        public static IEnumerable<Table> GetModelTables(this DbContext context)
        {
            foreach (var entityType in context.Model.GetEntityTypes())
            {
                var schema = entityType.GetSchema();
                var tableName = entityType.GetTableName();
                var columnNames = entityType.GetProperties().Select(e => e.GetColumnName());
                var table = new Table(schema, tableName, columnNames.ToList());
                yield return table;
            }
        }

        internal static DbConnection GetDbConnection(this DbContext context)
        {
            return context.Database.GetDbConnection();
        }
    }
}