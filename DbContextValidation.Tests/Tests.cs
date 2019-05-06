using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
#if NETFRAMEWORK
using DbContextValidation.EF6;
using System.Data.Entity;
#else
using DbContextValidation.EFCore;
using Microsoft.EntityFrameworkCore;
#endif
using Xunit;
using Xunit.Fixture.DockerDb;

#if PROVIDER_FIREBIRD
namespace DbContextValidation.Tests.Firebird
#elif PROVIDER_MYSQL
namespace DbContextValidation.Tests.MySQL
#elif PROVIDER_MYSQL_POMELO
namespace DbContextValidation.Tests.MySQL.Pomelo
#elif PROVIDER_NPGSQL
namespace DbContextValidation.Tests.Npgsql
#elif PROVIDER_ORACLE
namespace DbContextValidation.Tests.Oracle
#elif PROVIDER_SQLITE
namespace DbContextValidation.Tests.SQLite
#elif PROVIDER_SQLSERVER
namespace DbContextValidation.Tests.SqlServer
#else
#error Make sure to define a PROVIDER_* constant in the tests project
namespace DbContextValidation.Tests
#endif
{
    [SuppressMessage("ReSharper", "VSTHRD200", Justification = "Naming all tests methods with the Async suffix feels weird")]
    public class ValidatorTests : IClassFixture<DockerDatabaseFixture>
    {
        private class AccumulatorProgress<T> : IProgress<T>
        {
            private readonly List<T> _items = new List<T>();

            public IReadOnlyList<T> Items => _items;

            public void Report(T value)
            {
                _items.Add(value);
            }
        }
        
        private readonly DbContextValidator _defaultValidator;
        private readonly string _connectionString;

        public ValidatorTests(DockerDatabaseFixture dockerDatabaseFixture)
        {
            _defaultValidator = new DbContextValidator(StringComparer.InvariantCulture);
            _connectionString = dockerDatabaseFixture.ConnectionString;
        }

        [Fact]
        public async Task Validator_ValidContext_ReturnNoErrors()
        {
            using (var context = new ValidContext(_connectionString))
            {
                var errors = await _defaultValidator.ValidateContextAsync(context);
                errors.Should().BeEmpty();
                // ReSharper disable AccessToDisposedClosure
                Func<Task> customersTask = async () => { await context.Customers.ToListAsync(); };
                Func<Task> ordersTask = async () => { await context.Orders.ToListAsync(); };
                // ReSharper restore AccessToDisposedClosure
                await customersTask.Should().NotThrowAsync();
                await ordersTask.Should().NotThrowAsync();
            }
        }
        
        [Fact]
        public async Task Validator_ValidContextWithExplicitSchema_ReturnNoErrors()
        {
            using (var context = new ValidContextWithExplicitSchema(_connectionString, Configuration.Schema))
            {
                var errors = await _defaultValidator.ValidateContextAsync(context);
                errors.Should().BeEmpty();
            }
        }
        
        [Fact]
        public async Task Validator_ValidContext_ReportsProgress()
        {
            var progress = new AccumulatorProgress<Table>();
            using (var context = new ValidContext(_connectionString))
            {
                await _defaultValidator.ValidateContextAsync(context, progress);
                progress.Items.Select(e => e.TableName).Should().BeEquivalentTo("tCustomers", "tOrders");
            }
        }
        
        [Fact]
        public void Validator_ValidContext_SupportsCancellation()
        {
            using (var context = new ValidContext(_connectionString))
            {
                var validationTask = _defaultValidator.ValidateContextAsync(context, cancellationToken: new CancellationToken(true));
                validationTask.Status.Should().Be(TaskStatus.Canceled);
            }
        }

        [Fact]
        public async Task Validator_UnknownSchema_ReturnsMissingTableErrors()
        {
            using (var context = new ContextWithUnknownSchema(_connectionString))
            {
                var errors = await _defaultValidator.ValidateContextAsync(context);
                errors.Should().OnlyContain(e => e.Table.Schema == "unknown");
                errors.Should().OnlyContain(e => e.GetType() == typeof(MissingTableError));
                errors.Select(e => e.Table.TableName).Should().BeEquivalentTo("tCustomers", "tOrders");
            }
        }
        
        [Fact]
        public async Task Validator_MisspelledCustomersTable_ReturnsMissingTableError()
        {
            using (var context = new ContextWithMisspelledCustomersTable(_connectionString))
            {
                var errors = await _defaultValidator.ValidateContextAsync(context);
                var error = errors.Should().ContainSingle().Subject;
                error.Table.TableName.Should().Be("Customers");
                error.Should().BeOfType<MissingTableError>();
            }
        }
        
        [Fact]
        public async Task Validator_MisspelledOrderDateColumn_ReturnsMissingColumnsError()
        {
            using (var context = new ContextWithMisspelledOrderDateColumn(_connectionString))
            {
                var errors = await _defaultValidator.ValidateContextAsync(context);
                var error = errors.Should().ContainSingle().Subject;
                error.Table.TableName.Should().Be("tOrders");
                error.Should().BeOfType<MissingColumnsError>()
                    .Which.ColumnNames.Should().ContainSingle()
                    .Which.Should().Be("OrderFate");
            }
        }
        
        [Fact]
        public async Task Validator_CaseInsensitiveColumnNameComparison_ReturnNoErrors()
        {
            using (var context = new ContextWithMixedCaseColumns(_connectionString))
            {
                var caseInsensitiveValidator = new DbContextValidator(StringComparer.InvariantCultureIgnoreCase);
                var errors = await caseInsensitiveValidator.ValidateContextAsync(context);
                errors.Should().BeEmpty();
            }
        }
        
        [Fact]
        public async Task Validator_CaseSensitiveColumnNameComparison_ReturnsMissingColumnsError()
        {
            using (var context = new ContextWithMixedCaseColumns(_connectionString))
            {
                var errors = await _defaultValidator.ValidateContextAsync(context);
                var error = errors.Should().ContainSingle().Subject; 
                error.Table.TableName.Should().Be("tOrders");
                error.Should().BeOfType<MissingColumnsError>()
                    .Which.ColumnNames.Should().BeEquivalentTo("oRdErDaTe", "cUsToMeRiD");
            }
        }
    }
}