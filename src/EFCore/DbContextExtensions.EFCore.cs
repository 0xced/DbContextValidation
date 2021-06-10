using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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
                var columnNames = entityType.GetProperties().Select(GetColumnName);
                var table = new Table(schema, tableName, columnNames.ToList());
                yield return table;
            }
        }

        internal static DbConnection GetDbConnection(this DbContext context)
        {
            return context.Database.GetDbConnection();
        }

        /*
         * For compatibility with both EF Core 2 and EF Core 3+, see https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-3.0/breaking-changes#provider-specific-metadata-api-changes
         * We can't use entityType.Relational() or property.Relational() in EF Core 3+ or else we get System.TypeLoadException : Could not load type 'Microsoft.EntityFrameworkCore.RelationalMetadataExtensions' from assembly 'Microsoft.EntityFrameworkCore.Relational, Version=3.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'.
         * We can't use entityType.GetSchema() or property.GetColumnName() since we support EF Core 2 and those methods are new in EF Core 3+
         * So we have to call them all through reflection.
         */
        private static readonly Assembly EfCoreRelationalAssembly;
        // EF Core 2
        private static readonly MethodInfo EntityTypeRelational;
        private static readonly MethodInfo PropertyRelational;
        // EF Core 3+
        private static readonly MethodInfo GetSchemaMethod;
        private static readonly MethodInfo GetTableNameMethod;
        private static readonly MethodInfo GetColumnNameMethod;

        static DbContextExtensions()
        {
            EfCoreRelationalAssembly = typeof(DbContextExtensions).Assembly.GetReferencedAssemblies().Where(e => e.Name == "Microsoft.EntityFrameworkCore.Relational").Select(Assembly.Load).SingleOrDefault();

            // EF Core 2
            var relationalMetadataExtensions = EfCoreRelationalAssembly?.GetType("Microsoft.EntityFrameworkCore.RelationalMetadataExtensions");
            EntityTypeRelational =  relationalMetadataExtensions?.GetMethod("Relational", new [] {typeof(IEntityType)});
            PropertyRelational =  relationalMetadataExtensions?.GetMethod("Relational", new [] {typeof(IProperty)});

            // EF Core 3+
            var relationalEntityTypeExtensions = EfCoreRelationalAssembly?.GetType("Microsoft.EntityFrameworkCore.RelationalEntityTypeExtensions");
            var relationalPropertyExtensions = EfCoreRelationalAssembly?.GetType("Microsoft.EntityFrameworkCore.RelationalPropertyExtensions");
            GetSchemaMethod = relationalEntityTypeExtensions?.GetMethod("GetSchema", new [] {typeof(IEntityType)});
            GetTableNameMethod = relationalEntityTypeExtensions?.GetMethod("GetTableName", new [] {typeof(IEntityType)});
            GetColumnNameMethod = relationalPropertyExtensions?.GetMethod("GetColumnName", new [] {typeof(IProperty)});
        }

        private static string GetSchema(this IEntityType entityType)
        {
            var parameters = new object[] {entityType};
            if (GetSchemaMethod != null)
            {
                // Equivalent to `entityType.GetSchema()` (EF Core 3+)
                return (string)GetSchemaMethod.Invoke(null, parameters);
            }
            if (EntityTypeRelational != null)
            {
                // Equivalent to `entityType.Relational().Schema` (EF Core 2)
                var relationalEntityTypeAnnotations = EntityTypeRelational.Invoke(null, parameters);
                var schema = relationalEntityTypeAnnotations.GetType().GetProperty("Schema") ?? throw new MissingMethodException("IRelationalEntityTypeAnnotations", "Schema");
                return (string)schema.GetValue(relationalEntityTypeAnnotations);
            }
            throw NotSupportedException();
        }

        private static string GetTableName(this IEntityType entityType)
        {
            var parameters = new object[] {entityType};
            if (GetTableNameMethod != null)
            {
                // Equivalent to `entityType.GetTableName()` (EF Core 3+)
                return (string)GetTableNameMethod.Invoke(null, parameters);
            }
            if (EntityTypeRelational != null)
            {
                // Equivalent to `entityType.Relational().TableName` (EF Core 2)
                var relationalEntityTypeAnnotations = EntityTypeRelational.Invoke(null, parameters);
                var tableName = relationalEntityTypeAnnotations.GetType().GetProperty("TableName") ?? throw new MissingMethodException("IRelationalEntityTypeAnnotations", "TableName");
                return (string)tableName.GetValue(relationalEntityTypeAnnotations);
            }
            throw NotSupportedException();
        }

        private static string GetColumnName(this IProperty property)
        {
            var parameters = new object[] {property};
            if (GetColumnNameMethod != null)
            {
                // Equivalent to `property.GetColumnName()` (EF Core 3+)
                return (string)GetColumnNameMethod.Invoke(null, parameters);
            }
            if (PropertyRelational != null)
            {
                // Equivalent to `property.Relational().ColumnName` (EF Core 2)
                var relationalPropertyAnnotations = PropertyRelational.Invoke(null, parameters);
                var columnName = relationalPropertyAnnotations.GetType().GetProperty("ColumnName") ?? throw new MissingMethodException("IRelationalPropertyAnnotations", "ColumnName");
                return (string)columnName.GetValue(relationalPropertyAnnotations);
            }
            throw NotSupportedException();
        }

        private static Exception NotSupportedException()
        {
            if (EfCoreRelationalAssembly == null)
                throw new InvalidOperationException($"The 'Microsoft.EntityFrameworkCore.Relational' assembly was not found as a referenced assembly by {typeof(DbContextExtensions).Assembly}.");
            return new NotSupportedException($"Found neither 'Microsoft.EntityFrameworkCore.RelationalMetadataExtensions' (expected in EF Core 2) nor 'Microsoft.EntityFrameworkCore.RelationalEntityTypeExtensions' (expected in EF Core 3+). Did Microsoft introduce a breaking change in {EfCoreRelationalAssembly} ?");
        }
    }
}