using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
#if NETFRAMEWORK
using DbSchemaValidator.EF6;
using System.Data.Entity;
#else
using DbSchemaValidator.EFCore;
using Microsoft.EntityFrameworkCore;
#endif
using Xunit;

namespace DbSchemaValidator.Tests
{
    public enum Provider
    {
        // ReSharper disable InconsistentNaming
        MySQL,
        Npgsql,
        SQLite,
        SqlServer,
        // ReSharper restore InconsistentNaming
    }
    
    public class Tests
    {
        private sealed class ProviderFactAttribute : FactAttribute
        {
            private static readonly Provider Provider;
            
            static ProviderFactAttribute()
            {
                var fullName = typeof(ValidContext).BaseType?.FullName ?? throw new Exception("ValidContext must inherit from Context");
                var providerName = fullName.Split('.').Reverse().Skip(1).Take(1).First();
                Provider = (Provider)Enum.Parse(typeof(Provider), providerName);
            }
            
            public ProviderFactAttribute(Provider provider)
            {
                if (provider != Provider)
                {
                    Skip = $"This test is not applicable to {Provider}";
                }
            }
        }
        
        private readonly Validator _defaultValidator;
        private readonly Validator _caseInsensitiveValidator;
        private readonly Validator _caseSensitiveValidator;

        static Tests()
        {
#if NETFRAMEWORK
            // Disable migrations
            Database.SetInitializer<ValidContext>(null);
            Database.SetInitializer<ContextWithPublicSchema>(null);
            Database.SetInitializer<ContextWithUnknownSchema>(null);
            Database.SetInitializer<ContextWithMisspelledCustomersTable>(null);
            Database.SetInitializer<ContextWithMisspelledOrderDateColumn>(null);
            Database.SetInitializer<ContextWithMixedCaseColumns>(null);
#endif
        }

        public Tests()
        {
            _defaultValidator = new Validator();
            _caseInsensitiveValidator = new Validator(StringComparer.InvariantCultureIgnoreCase);
            _caseSensitiveValidator = new Validator(StringComparer.InvariantCulture);
        }

        [Fact]
        public async Task ValidMapping()
        {
            using (var context = new ValidContext())
            {
                var invalidMappings = await _defaultValidator.ValidateSchemaAsync(context);
                invalidMappings.Should().BeEmpty();
                // ReSharper disable AccessToDisposedClosure
                Func<Task> customersTask = async () => { await context.Customers.ToListAsync(); };
                Func<Task> ordersTask = async () => { await context.Orders.ToListAsync(); };
                // ReSharper restore AccessToDisposedClosure
                customersTask.Should().NotThrow();
                ordersTask.Should().NotThrow();
            }
        }
        
        [ProviderFact(Provider.Npgsql)]
        public async Task ValidMappingWithPublicSchema()
        {
            using (var context = new ContextWithPublicSchema())
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
        public async Task UnknownSchema()
        {
            using (var context = new ContextWithUnknownSchema())
            {
                var invalidMappings = await _defaultValidator.ValidateSchemaAsync(context);
                invalidMappings.Should().HaveCount(2);
                invalidMappings.Should().OnlyContain(e => e.Schema == "unknown");
                invalidMappings.Select(e => e.TableName).Should().Contain("tCustomers", "tOrders");
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
                invalidMapping.MissingTableException.Should().NotBeNull();
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