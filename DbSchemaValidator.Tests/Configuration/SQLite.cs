using System;
using Docker.DotNet.Models;

namespace DbSchemaValidator.Tests
 {
     public static class Config
     {
         public static readonly Provider Provider = Provider.SQLite;
         public static readonly string Schema = null;
         
         public static readonly string ConnectionString = "Data Source=../../../../DbSchemaValidator.Tests/DbSchemaValidator.db";

         public static CreateContainerParameters ContainerParameters(string containerName, Func<string, string> sqlDirectory)
         {
             throw new NotSupportedException("SQLite doesn't require a Docker container.");
         }
     }
 }