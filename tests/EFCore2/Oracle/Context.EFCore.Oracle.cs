﻿using System;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace DbContextValidation.Tests.Oracle
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
            optionsBuilder.UseOracle(_connectionString);
        }
    }
}