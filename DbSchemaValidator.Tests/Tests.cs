using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if NETFRAMEWORK
using DbSchemaValidator.EF6;
#else
using DbSchemaValidator.EFCore;
#endif
using Xunit;

namespace DbSchemaValidator.Tests
{
    public class Tests
    {
        static Tests()
        {
#if NETFRAMEWORK
            // Disable migrations
            System.Data.Entity.Database.SetInitializer<ValidContext>(null);
            System.Data.Entity.Database.SetInitializer<ContextWithMisspelledCustomersTable>(null);
            System.Data.Entity.Database.SetInitializer<ContextWithMisspelledOrderDateColumn>(null);
            System.Data.Entity.Database.SetInitializer<ContextWithMixedCaseColumns>(null);
#endif
        }

        [Fact]
        public async Task ValidMapping()
        {
            using (var context = new ValidContext())
            {
                var invalidMappings = await context.ValidateSchema();
                Assert.Empty(invalidMappings);
            }
        }
        
        [Fact]
        public async Task ValidMappingWithProgress()
        {
            var validations = new List<DbSchemaValidation>();
            var progress = new Progress<DbSchemaValidation>(validations.Add);
            
            using (var context = new ValidContext())
            {
                await context.ValidateSchema(progress: progress);
                await Task.Yield();
                Assert.Equal(2, validations.Count);
                Assert.Contains(validations, e => e.TableName == "tCustomers" && e.Table == 1 && e.TableCount == 2 && e.InvalidMapping == null);
                Assert.Contains(validations, e => e.TableName == "tOrders"    && e.Table == 2 && e.TableCount == 2 && e.InvalidMapping == null);
            }
        }
        
        [Fact]
        public async Task MisspelledCustomersTable()
        {
            using (var context = new ContextWithMisspelledCustomersTable())
            {
                var invalidMappings = await context.ValidateSchema();
                Assert.Single(invalidMappings);
                var invalidMapping = invalidMappings.Single(); 
                Assert.Equal("Customers", invalidMapping.TableName);
                Assert.Null(invalidMapping.MissingColumns);
            }
        }
        
        [Fact]
        public async Task MisspelledOrderDateColumn()
        {
            using (var context = new ContextWithMisspelledOrderDateColumn())
            {
                var invalidMappings = await context.ValidateSchema();
                Assert.Single(invalidMappings);
                var invalidMapping = invalidMappings.Single(); 
                Assert.Equal("tOrders", invalidMapping.TableName);
                Assert.Single(invalidMapping.MissingColumns);
                Assert.Equal("OrderFate", invalidMapping.MissingColumns.Single());
            }
        }
        
        [Fact]
        public async Task CaseInsensitiveColumnNameComparison()
        {
            using (var context = new ContextWithMixedCaseColumns())
            {
                var invalidMappings = await context.ValidateSchema(StringComparer.InvariantCultureIgnoreCase);
                Assert.Empty(invalidMappings);
            }
        }
        
        [Fact]
        public async Task CaseSensitiveColumnNameComparison()
        {
            using (var context = new ContextWithMixedCaseColumns())
            {
                var invalidMappings = await context.ValidateSchema(StringComparer.InvariantCulture);
                Assert.Single(invalidMappings);
                var invalidMapping = invalidMappings.Single(); 
                Assert.Equal("tOrders", invalidMapping.TableName);
                Assert.Equal(2, invalidMapping.MissingColumns.Count);
                Assert.Contains("oRdErDaTe", invalidMapping.MissingColumns);
                Assert.Contains("cUsToMeRiD", invalidMapping.MissingColumns);
            }
        }
    }
}