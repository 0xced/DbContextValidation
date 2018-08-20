using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
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
    /// <summary>
    /// DbContextExtensions is a static class used to hold the <see cref="ValidateSchema"/> method.
    /// </summary>
    public static partial class DbContextExtensions
    {
        private static IEqualityComparer<string> ColumnNameEqualityComparer(IEqualityComparer<string> columnNameEqualityComparer, bool? caseSensitive)
        {
            if (columnNameEqualityComparer != null)
                return columnNameEqualityComparer;

            return caseSensitive is false ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture;
        }
        
        /// <summary>
        /// Validates all the table and column names of entities defined in the model associated with the context against the actual database schema.
        /// </summary>
        /// <param name="context">The context you want to validate against its actual database connection.</param>
        /// <param name="columnNameEqualityComparer">An equality comparer used to compare column names defined in the model against the actual column names. If <code>null</code>, tries to guess from the database provider if table names are case sensitive or not. If case sensitivity can not be guessed, then <code>StringComparer.InvariantCulture</code> is used.</param>
        /// <param name="progress">A progress reporting numbers between 0.0 and 1.0 representing the completed fraction of the validation process. If <code>null</code>, no progress is reported.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that you can use to abort the validation process.</param>
        /// <returns>A collection of <see cref="InvalidMapping"/>s, i.e. when an entity defined in the model does not have a matching table and column names in the database. If the context model exactly matches the database schema then an empty collection is returned.</returns>
        public static async Task<IReadOnlyCollection<InvalidMapping>> ValidateSchema(this DbContext context, IEqualityComparer<string> columnNameEqualityComparer = null, IProgress<float> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var invalidMappings = new List<InvalidMapping>();
            var dbModel = context.GetDbModel();
            var i = 0;
            foreach (var entry in dbModel)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var tableName = entry.Key;
                var expectedColumnNames = entry.Value;
                InvalidMapping invalidMapping = null;
                try
                {
                    var tableInfo = await context.GetDbConnection().GetTableInfo(tableName);
                    var equalityComparer = ColumnNameEqualityComparer(columnNameEqualityComparer, tableInfo.CaseSensitive);
                    var missingColumns = expectedColumnNames.Except(tableInfo.ColumnNames, equalityComparer).ToList();
                    if (missingColumns.Any())
                    {
                        invalidMapping = new InvalidMapping(tableName, missingColumns);
                    }
                }
                catch (DbException)
                {
                    invalidMapping = new InvalidMapping(tableName, missingColumns: null);
                }
                if (invalidMapping != null)
                {
                    invalidMappings.Add(invalidMapping);                    
                }
                progress?.Report(++i / (float)dbModel.Count);
            }
            return invalidMappings;
        }
    }
}