﻿using System;
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
namespace DbContextValidation.EFCore
#else
namespace DbContextValidation.EF6
#endif
{
    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="schema">TODO</param>
    /// <param name="tableName">TODO</param>
    /// <param name="commandBuilder">TODO</param>
    public delegate string SelectStatement(string schema, string tableName, DbCommandBuilder commandBuilder);
    
    /// <summary>
    /// 
    /// </summary>
    public class DbContextValidator : IDbContextValidator
    {

        private readonly IEqualityComparer<string> _columnNameEqualityComparer;
        private readonly SelectStatement _selectStatement;

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="columnNameEqualityComparer">An equality comparer used to compare column names defined in the model against the actual column names. If <code>null</code>, tries to guess from the database provider if table names are case sensitive or not. If case sensitivity can not be guessed, then <code>StringComparer.InvariantCulture</code> is used.</param>
        /// <param name="selectStatement">TODO</param>
        public DbContextValidator(IEqualityComparer<string> columnNameEqualityComparer = null, SelectStatement selectStatement = null)
        {
            _columnNameEqualityComparer = columnNameEqualityComparer;
            _selectStatement = selectStatement ?? DefaultSelectStatement;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<InvalidMapping>> ValidateSchemaAsync(DbContext context, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            var invalidMappings = new List<InvalidMapping>();
            var dbModel = context.GetDbModel();
            var i = 0;
            foreach (var entry in dbModel)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var schema = entry.Key.schema;
                var tableName = entry.Key.tableName;
                var expectedColumnNames = entry.Value;
                InvalidMapping invalidMapping = null;
                try
                {
                    var tableInfo = await context.GetDbConnection().GetTableInfo(_selectStatement, schema, tableName);
                    var equalityComparer = ColumnNameEqualityComparer(_columnNameEqualityComparer, tableInfo.CaseSensitive);
                    var missingColumns = expectedColumnNames.Except(tableInfo.ColumnNames, equalityComparer).ToList();
                    if (missingColumns.Any())
                    {
                        invalidMapping = new InvalidMapping(schema, tableName, missingColumns, exception: null);
                    }
                }
                catch (TableNotFoundException exception)
                {
                    invalidMapping = new InvalidMapping(schema, tableName, missingColumns: null, exception);
                }
                if (invalidMapping != null)
                {
                    invalidMappings.Add(invalidMapping);                    
                }
                progress?.Report(++i / (float)dbModel.Count);
            }
            return invalidMappings;
        }
        
        private static string DefaultSelectStatement(string schema, string tableName, DbCommandBuilder commandBuilder)
        {
            var hasSchema = !string.IsNullOrEmpty(schema);
            var quotedSchema = hasSchema ? commandBuilder?.QuoteIdentifier(schema) ?? schema : null;
            var quotedTableName = commandBuilder?.QuoteIdentifier(tableName) ?? tableName;
            var schemaSeparator = commandBuilder?.SchemaSeparator ?? ".";
            var tableDescription = hasSchema ? quotedSchema + schemaSeparator + quotedTableName : quotedTableName;
            return $"SELECT * FROM {tableDescription} WHERE 1=0";
        }
        
        private static IEqualityComparer<string> ColumnNameEqualityComparer(IEqualityComparer<string> columnNameEqualityComparer, bool? caseSensitive)
        {
            if (columnNameEqualityComparer != null)
                return columnNameEqualityComparer;

            return caseSensitive is false ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture;
        }
    }
}