using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Testcontainers.Xunit;
using Xunit.Abstractions;

namespace DbContextValidation.Tests
{
    // ReSharper disable once UnusedType.Global
    public abstract class DbFixture<TBuilderEntity, TContainerEntity> : DbContainerFixture<TBuilderEntity, TContainerEntity>
        where TBuilderEntity : IContainerBuilder<TBuilderEntity, TContainerEntity, IContainerConfiguration>, new()
        where TContainerEntity : IContainer, IDatabaseContainer
    {
        protected DbFixture(IMessageSink messageSink) : base(messageSink)
        {
        }

        protected abstract TBuilderEntity CreateBuilder();

        protected abstract string SqlDirectoryName { get; }

        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedMemberInSuper.Global
        public abstract string Schema { get; }

        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            var connection = CreateConnection();
            await connection.OpenAsync();

            foreach (var sql in ReadSqlStatements(SqlDirectory(SqlDirectoryName)))
            {
                var command = connection.CreateCommand();
                command.CommandText = sql;
                await command.ExecuteNonQueryAsync();
            }
        }

        protected override TBuilderEntity Configure()
        {
            return CreateBuilder().WithWaitStrategy(Wait.ForUnixContainer().UntilDatabaseIsAvailable(DbProviderFactory));
        }

        private static IEnumerable<string> ReadSqlStatements(DirectoryInfo directory)
        {
            return directory.GetFiles("*.sql").OrderBy(file => file.Name).Select(file => File.ReadAllText(file.FullName));
        }

        private static DirectoryInfo SqlDirectory(string directoryName)
        {
            var testsDirectory = TestsDirectory();
            var sqlDirectory = new DirectoryInfo(Path.Combine(testsDirectory.FullName, directoryName));
            return sqlDirectory.Exists
                ? sqlDirectory
                : throw new FileNotFoundException($"SQL directory not found ({sqlDirectory.FullName})", sqlDirectory.FullName);
        }

        private static DirectoryInfo TestsDirectory()
        {
            var testsDirectory = Environment.GetEnvironmentVariable("TESTS_DIRECTORY");
            if (testsDirectory != null)
            {
                var testsDirectoryInfo = new DirectoryInfo(testsDirectory);
                return testsDirectoryInfo.Exists
                    ? testsDirectoryInfo
                    : throw new FileNotFoundException($"Tests directory specified in the TESTS_DIRECTORY environment variable not found ({testsDirectoryInfo.FullName})", testsDirectoryInfo.FullName);
            }

            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = new FileInfo(assemblyLocation).Directory;
            for (var directory = assemblyDirectory; directory != null; directory = directory.Parent)
            {
                if (directory.Name == "tests")
                    return directory;
            }
            throw new FileNotFoundException($"Tests directory not found by going up '{assemblyLocation}' and searching for a directory named 'tests'.");
        }
    }
}