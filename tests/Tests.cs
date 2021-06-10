using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
#if EFCORE
using DbContextValidation.EFCore;
using Microsoft.EntityFrameworkCore;
#else
using DbContextValidation.EF6;
using System.Data.Entity;
#endif
using Xunit;

#if !PROVIDER_SQLITE
using DbFixture = DockerRunner.Xunit.DockerDatabaseContainerFixture<DbContextValidation.Tests.Configuration>;
#endif

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
    public class ValidatorTests : IClassFixture<DbFixture>
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

        public ValidatorTests(DbFixture dbFixture)
        {
            _defaultValidator = new DbContextValidator(StringComparer.InvariantCulture);
            _connectionString = dbFixture.ConnectionString;
        }

        [Fact]
        public async Task Validator_ValidContext_ReturnNoErrors()
        {
            // Arrange
            using var context = new ValidContext(_connectionString);

            // Act
            var errors = await _defaultValidator.ValidateContextAsync(context);

            // Assert
            errors.Should().BeEmpty();
            // ReSharper disable AccessToDisposedClosure
            Func<Task> customersTask = async () => { await context.Customers.ToListAsync(); };
            Func<Task> ordersTask = async () => { await context.Orders.ToListAsync(); };
            // ReSharper restore AccessToDisposedClosure
            await customersTask.Should().NotThrowAsync();
            await ordersTask.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Validator_ValidContextWithExplicitSchema_ReturnNoErrors()
        {
            // Arrange
            using var context = new ValidContextWithExplicitSchema(_connectionString, Configuration.Schema);

            // Act
            var errors = await _defaultValidator.ValidateContextAsync(context);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public async Task Validator_ValidContext_ReportsProgress()
        {
            // Arrange
            var progress = new AccumulatorProgress<Table>();
            using var context = new ValidContext(_connectionString);

            // Act
            await _defaultValidator.ValidateContextAsync(context, progress);

            // Assert
            progress.Items.Select(e => e.TableName).Should().BeEquivalentTo("tCustomers", "tOrders");
        }

        [Fact]
        public void Validator_ValidContext_SupportsCancellation()
        {
            // Arrange
            using var context = new ValidContext(_connectionString);

            // Act
            var validationTask = _defaultValidator.ValidateContextAsync(context, cancellationToken: new CancellationToken(true));

            // Assert
            validationTask.Status.Should().Be(TaskStatus.Canceled);
        }

        [Fact]
        public async Task Validator_UnknownSchema_ReturnsMissingTableErrors()
        {
            // Arrange
            using var context = new ContextWithUnknownSchema(_connectionString);

            // Act
            var errors = await _defaultValidator.ValidateContextAsync(context);

            // Assert
            errors.Should().OnlyContain(e => e.Table.Schema == "unknown");
            errors.Should().OnlyContain(e => e.GetType() == typeof(MissingTableError));
            errors.Select(e => e.Table.TableName).Should().BeEquivalentTo("tCustomers", "tOrders");
        }

        [Fact]
        public async Task Validator_MisspelledCustomersTable_ReturnsMissingTableError()
        {
            // Arrange
            using var context = new ContextWithMisspelledCustomersTable(_connectionString);

            // Act
            var errors = await _defaultValidator.ValidateContextAsync(context);

            // Assert
            var error = errors.Should().ContainSingle().Subject;
            error.Table.TableName.Should().Be("Customers");
            error.Should().BeOfType<MissingTableError>();
        }

        [Fact]
        public async Task Validator_MisspelledOrderDateColumn_ReturnsMissingColumnsError()
        {
            // Arrange
            using var context = new ContextWithMisspelledOrderDateColumn(_connectionString);

            // Act
            var errors = await _defaultValidator.ValidateContextAsync(context);

            // Assert
            var error = errors.Should().ContainSingle().Subject;
            error.Table.TableName.Should().Be("tOrders");
            error.Should().BeOfType<MissingColumnsError>()
                .Which.ColumnNames.Should().ContainSingle()
                .Which.Should().Be("OrderFate");
        }

        [Fact]
        public async Task Validator_CaseInsensitiveColumnNameComparison_ReturnNoErrors()
        {
            // Arrange
            using var context = new ContextWithMixedCaseColumns(_connectionString);
            var caseInsensitiveValidator = new DbContextValidator(StringComparer.InvariantCultureIgnoreCase);

            // Act
            var errors = await caseInsensitiveValidator.ValidateContextAsync(context);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public async Task Validator_CaseSensitiveColumnNameComparison_ReturnsMissingColumnsError()
        {
            // Arrange
            using var context = new ContextWithMixedCaseColumns(_connectionString);

            // Act
            var errors = await _defaultValidator.ValidateContextAsync(context);

            // Assert
            var error = errors.Should().ContainSingle().Subject;
            error.Table.TableName.Should().Be("tOrders");
            error.Should().BeOfType<MissingColumnsError>()
                .Which.ColumnNames.Should().BeEquivalentTo("oRdErDaTe", "cUsToMeRiD");
        }
    }
}