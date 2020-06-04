﻿using System.Collections.Generic;
using System.Data.Common;

#if EFCORE
namespace DbContextValidation.EFCore
#else
namespace DbContextValidation.EF6
#endif
{
    /// <summary>
    /// Represents a database table.
    /// </summary>
    public class Table
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        /// <param name="schema">The schema of the table.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columns">The columns of the table.</param>
        public Table(string schema, string tableName, IReadOnlyCollection<DbColumn> columns)
        {
            Schema = schema;
            TableName = tableName;
            Columns = columns;
        }

        /// <summary>
        /// The schema of the table. May be <code>null</code> or empty string as some providers (e.g. SQLite, MySQL) do not support schemata.
        /// </summary>
        public string Schema { get; }
        
        /// <summary>
        /// The name of the table.
        /// </summary>
        public string TableName { get; }
        
        /// <summary>
        /// The names of the columns.
        /// </summary>
        public IReadOnlyCollection<DbColumn> Columns { get; }

        /// <summary>
        /// Creates a string representation of the table.
        /// </summary>
        /// <returns>The string representation of the table, including the schema if not null or empty.</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(Schema) ? TableName : Schema + "." + TableName;
        }
    }
}