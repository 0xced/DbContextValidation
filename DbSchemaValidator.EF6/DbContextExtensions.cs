using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace DbSchemaValidator.EF6
{
    public static class DbContextExtensions
    {
        public static async Task<IReadOnlyCollection<InvalidMapping>> ValidateSchema(this DbContext context, IProgress<DbSchemaValidation> progress = null)
        {
            var invalidMappings = new List<InvalidMapping>();
            var workspace = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;
            var entitySets = workspace.GetItems<EntityContainer>(DataSpace.CSpace).Single().EntitySets;
            var entitySetMappings = workspace.GetItems<EntityContainerMapping>(DataSpace.CSSpace).Single().EntitySetMappings.ToList();
            var entityTypes = workspace.GetItems<EntityType>(DataSpace.CSpace);
            var i = 0;
            foreach (var entityType in entityTypes)
            {
                var entitySet = entitySets.Single(s => s.ElementType.Name == entityType.Name);
                var entitySetMapping = entitySetMappings.Single(s => s.EntitySet == entitySet);
                var fragmentMapping = entitySetMapping.EntityTypeMappings.Single().Fragments.Single();
                var tableName = fragmentMapping.StoreEntitySet.MetadataProperties["Table"].Value?.ToString();
                progress?.Report(new DbSchemaValidation(++i / (float)entityTypes.Count, tableName));
                var expectedColumnNames = fragmentMapping.PropertyMappings.OfType<ScalarPropertyMapping>().Select(e => e.Column.Name);
                var missingColumns = new List<string>();
                try
                {
                    var actualColumnNames = await context.Database.Connection.GetColumnNames(tableName);
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