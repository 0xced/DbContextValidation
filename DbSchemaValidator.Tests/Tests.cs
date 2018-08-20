using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
                var invalidMappings = await context.ValidateSchemaAsync();
                Assert.Empty(invalidMappings);
            }
        }
        
        [Fact]
        public async Task ValidMappingWithProgress()
        {
            var fractions = new List<float>();
            var progress = new Progress<float>(fractions.Add);
            
            using (var context = new ValidContext())
            {
                await context.ValidateSchemaAsync(progress: progress);
                await Task.Yield();
                Assert.Equal(2, fractions.Count);
                Assert.Contains(fractions, e => e >= 0.5 && e <= 0.5);
                Assert.Contains(fractions, e => e >= 1.0 && e <= 1.0);
            }
        }
        
        [Fact]
        public async Task ValidMappingWithCancellation()
        {
            using (var context = new ValidContext())
            {
                var validationTask = context.ValidateSchemaAsync(cancellationToken: new CancellationToken(true));
                Assert.Equal(TaskStatus.Canceled, validationTask.Status);
                await Assert.ThrowsAsync<OperationCanceledException>(() => validationTask);
            }
        }
        
        [Fact]
        public async Task MisspelledCustomersTable()
        {
            using (var context = new ContextWithMisspelledCustomersTable())
            {
                var invalidMappings = await context.ValidateSchemaAsync();
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
                var invalidMappings = await context.ValidateSchemaAsync();
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
                var invalidMappings = await context.ValidateSchemaAsync(StringComparer.InvariantCultureIgnoreCase);
                Assert.Empty(invalidMappings);
            }
        }
        
        [Fact]
        public async Task CaseSensitiveColumnNameComparison()
        {
            using (var context = new ContextWithMixedCaseColumns())
            {
                var invalidMappings = await context.ValidateSchemaAsync(StringComparer.InvariantCulture);
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