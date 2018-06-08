using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
#if EFCORE
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif

#if EFCORE
namespace DbSchemaValidator.EFCore
#else
namespace DbSchemaValidator.EF6
#endif
{
    public static partial class DbContextExtensions
    {
        public static async Task<IReadOnlyCollection<InvalidMapping>> ValidateSchema(this DbContext context, IEqualityComparer<string> columnNameEqualityComparer = null, IProgress<DbSchemaValidation> progress = null)
        {
            var invalidMappings = new List<InvalidMapping>();
            var dbModel = context.GetDbModel();
            var i = 0;
            foreach (var entry in dbModel)
            {
                var tableName = entry.Key;
                var expectedColumnNames = entry.Value; 
                progress?.Report(new DbSchemaValidation(++i / (float)dbModel.Count, tableName));
                try
                {
                    var actualColumnNames = await context.GetDbConnection().GetColumnNames(tableName);
                    var missingColumns = expectedColumnNames.Except(actualColumnNames, columnNameEqualityComparer).ToList();
                    if (missingColumns.Any())
                    {
                        invalidMappings.Add(new InvalidMapping(tableName, missingColumns));
                    }
                }
                catch (DbException)
                {
                    invalidMappings.Add(new InvalidMapping(tableName, missingColumns: null));
                }
            }
            return invalidMappings;
        }
    }
}