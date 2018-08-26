using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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
                var invalidMappings = await _defaultValidator.ValidateSchemaAsync(context);
                invalidMappings.Should().BeEmpty();
            }
        }
        
        [Fact]
        public async Task ValidMappingWithProgress()
        {
            var fractions = new List<float>();
            var progress = new Progress<float>(fractions.Add);
            
            using (var context = new ValidContext())
            {
                await _defaultValidator.ValidateSchemaAsync(context, progress: progress);
                await Task.Yield();
                fractions.Should().HaveCount(2);
                fractions.Should().Contain(new []{0.5f, 1.0f});
            }
        }
        
        [Fact]
        public void ValidMappingWithCancellation()
        {
            using (var context = new ValidContext())
            {
                var validationTask = _defaultValidator.ValidateSchemaAsync(context, cancellationToken: new CancellationToken(true));
                validationTask.Status.Should().Be(TaskStatus.Canceled);
                Func<Task> validation = async () => { await validationTask; };
                validation.Should().Throw<OperationCanceledException>();
            }
        }
        
        [Fact]
        public async Task MisspelledCustomersTable()
        {
            using (var context = new ContextWithMisspelledCustomersTable())
            {
                var invalidMappings = await _defaultValidator.ValidateSchemaAsync(context);
                var invalidMapping = invalidMappings.Should().ContainSingle().Subject;
                invalidMapping.TableName.Should().Be("Customers");
                invalidMapping.MissingColumns.Should().BeNull();
                invalidMapping.SelectException.Should().NotBeNull();
            }
        }
        
        [Fact]
        public async Task MisspelledOrderDateColumn()
        {
            using (var context = new ContextWithMisspelledOrderDateColumn())
            {
                var invalidMappings = await _defaultValidator.ValidateSchemaAsync(context);
                var invalidMapping = invalidMappings.Should().ContainSingle().Subject;
                invalidMapping.TableName.Should().Be("tOrders");
                var missingColumn = invalidMapping.MissingColumns.Should().ContainSingle().Subject;
                missingColumn.Should().Be("OrderFate");
            }
        }
        
        [Fact]
        public async Task CaseInsensitiveColumnNameComparison()
        {
            using (var context = new ContextWithMixedCaseColumns())
            {
                var invalidMappings = await _caseInsensitiveValidator.ValidateSchemaAsync(context);
                invalidMappings.Should().BeEmpty();
            }
        }
        
        [Fact]
        public async Task CaseSensitiveColumnNameComparison()
        {
            using (var context = new ContextWithMixedCaseColumns())
            {
                var invalidMappings = await _caseSensitiveValidator.ValidateSchemaAsync(context);
                var invalidMapping = invalidMappings.Should().ContainSingle().Subject; 
                invalidMapping.TableName.Should().Be("tOrders");
                invalidMapping.MissingColumns.Should().HaveCount(2);
                invalidMapping.MissingColumns.Should().Contain("oRdErDaTe", "cUsToMeRiD");
            }
        }
    }
}