using System;
using Microsoft.EntityFrameworkCore;

namespace DbContextValidation.Tests.MySQL.Pomelo
{
    public abstract class Context : DbContext
    {
        private readonly string _connectionString;

        protected Context(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString));
        }
    }
}