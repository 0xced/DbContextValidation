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
        private readonly Validator _defaultValidator;
        private readonly Validator _caseInsensitiveValidator;
        private readonly Validator _caseSensitiveValidator;
        private readonly ValidationProgress _progress;

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

        public Tests()
        {
            var fullName = typeof(ValidContext).BaseType?.FullName;
            var provider = fullName?.Split('.').Reverse().Skip(1).Take(1).First();
            if (provider == "Npgsql")
            {
                _defaultValidator = new Validator(selectStatement: NpgsqlSelectStatement);
                _caseInsensitiveValidator = new Validator(StringComparer.InvariantCultureIgnoreCase, NpgsqlSelectStatement);
                _caseSensitiveValidator = new Validator(StringComparer.InvariantCulture, NpgsqlSelectStatement);
            }
            else
            {
                _defaultValidator = new Validator();
                _caseInsensitiveValidator = new Validator(StringComparer.InvariantCultureIgnoreCase);
                _caseSensitiveValidator = new Validator(StringComparer.InvariantCulture);
            }
            
            _progress = new ValidationProgress();
        }

        private string NpgsqlSelectStatement(string schema, string tableName)
        {
            var table = string.IsNullOrEmpty(schema) ? tableName : schema + "." + tableName;
            return $"SELECT * FROM \"{table}\" WHERE 1=0";
        }

        [Fact]
        public async Task ValidMapping()
        {
            using (var context = new ValidContext())
            {
                var isValid = await _defaultValidator.ValidateSchemaAsync(context);
                Assert.True(isValid);
            }
        }
        
        [Fact]
        public async Task ValidMappingWithProgress()
        {
            using (var context = new ValidContext())
            {
                await _defaultValidator.ValidateSchemaAsync(context, progress: _progress);
                var fractions = _progress.Fractions;
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
                var validationTask = _defaultValidator.ValidateSchemaAsync(context, _progress, new CancellationToken(true));
                Assert.Empty(_progress.Fractions);
                Assert.Equal(TaskStatus.Canceled, validationTask.Status);
                await Assert.ThrowsAsync<OperationCanceledException>(() => validationTask);
            }
        }
        
        [Fact]
        public async Task MisspelledCustomersTable()
        {
            using (var context = new ContextWithMisspelledCustomersTable())
            {
                await _defaultValidator.ValidateSchemaAsync(context, _progress);
                var invalidMappings = _progress.InvalidMappings;
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
                await _defaultValidator.ValidateSchemaAsync(context, _progress);
                var invalidMappings = _progress.InvalidMappings;
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
                var isValid = await _caseInsensitiveValidator.ValidateSchemaAsync(context);
                Assert.True(isValid);
            }
        }
        
        [Fact]
        public async Task CaseSensitiveColumnNameComparison()
        {
            using (var context = new ContextWithMixedCaseColumns())
            {
                await _caseSensitiveValidator.ValidateSchemaAsync(context, _progress);
                var invalidMappings = _progress.InvalidMappings;
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