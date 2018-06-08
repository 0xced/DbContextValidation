using System;
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
        private static readonly Progress<DbSchemaValidation> Progress = new Progress<DbSchemaValidation>(e => Console.WriteLine($"{e.TableName} ({e.FractionCompleted:P2})"));
        
        static Tests()
        {
#if NETFRAMEWORK
            // Disable migrations
            System.Data.Entity.Database.SetInitializer<ValidContext>(null);
            System.Data.Entity.Database.SetInitializer<MisspelledTableContext>(null);
            System.Data.Entity.Database.SetInitializer<MisspelledColumnContext>(null);
#endif
        }

        [Fact]
        public async Task ValidMapping()
        {
            using (var context = new ValidContext())
            {
                var invalidMappings = await context.ValidateSchema(progress: Progress);
                Assert.Empty(invalidMappings);
            }
        }
        
        [Fact]
        public async Task MisspelledTable()
        {
            using (var context = new MisspelledTableContext())
            {
                var invalidMappings = await context.ValidateSchema(progress: Progress);
                Assert.Single(invalidMappings);
                var invalidMapping = invalidMappings.Single(); 
                Assert.Equal("Customers", invalidMapping.TableName);
                Assert.Null(invalidMapping.MissingColumns);
            }
        }
        
        [Fact]
        public async Task MisspelledColumn()
        {
            using (var context = new MisspelledColumnContext())
            {
                var invalidMappings = await context.ValidateSchema(progress: Progress);
                Assert.Single(invalidMappings);
                var invalidMapping = invalidMappings.Single(); 
                Assert.Equal("tOrders", invalidMapping.TableName);
                Assert.Single(invalidMapping.MissingColumns);
                Assert.Equal("OrderFate", invalidMapping.MissingColumns.Single());
            }
        }
        
        [Fact]
        public async Task CaseInsentiveColumn()
        {
            using (var context = new CaseInsensitiveColumnsContext())
            {
                var invalidMappings = await context.ValidateSchema(StringComparer.InvariantCultureIgnoreCase, Progress);
                Assert.Empty(invalidMappings);
            }
        }
        
        [Fact]
        public async Task CaseSentiveColumn()
        {
            using (var context = new CaseInsensitiveColumnsContext())
            {
                var invalidMappings = await context.ValidateSchema(progress: Progress);
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