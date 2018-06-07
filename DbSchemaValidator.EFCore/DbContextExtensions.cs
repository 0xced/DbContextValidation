using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DbSchemaValidator.EFCore
{
    public static class DbContextExtensions
    {
        public static async Task<IReadOnlyCollection<InvalidMapping>> ValidateSchema(this DbContext context, IProgress<DbSchemaValidation> progress = null)
        {
            var invalidMappings = new List<InvalidMapping>();
            var entityTypes = context.Model.GetEntityTypes().ToList();
            var i = 0;
            foreach (var entityType in entityTypes)
            {
                var tableName = entityType.Relational().TableName;
                progress?.Report(new DbSchemaValidation(++i / (float)entityTypes.Count, tableName));
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