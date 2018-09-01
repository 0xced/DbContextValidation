using System;
using System.Collections.Generic;
using Docker.DotNet.Models;

namespace DbContextValidation.Tests
 {
     public static class Config
     {
         public static readonly Provider Provider = Provider.Npgsql;
         public static readonly string Schema = "public";

         private static readonly string Host = "localhost";
         private static readonly string Port = "5432";
         private static readonly string Database = "DbContextValidation";
         private static readonly string User = "postgres";
         private static readonly string Password = "docker";

         public static readonly string ConnectionString = $"Host={Host};Database={Database};UserName={User};Password={Password}";
         
         public static CreateContainerParameters ContainerParameters(string containerName, Func<string, string> sqlDirectory)
         {
             return new CreateContainerParameters
             {
                 Name = containerName,
                 Image = "postgres:10.5-alpine",
                 Env = new [] { $"POSTGRES_PASSWORD={Password}", $"POSTGRES_DB={Database}" },
                 HostConfig = new HostConfig
                 {
                     Binds = new [] { sqlDirectory("SQL.Common") + ":/docker-entrypoint-initdb.d:ro" },
                     PortBindings = new Dictionary<string, IList<PortBinding>>
                     {
                         ["5432/tcp"] = new []{ new PortBinding { HostPort = Port } }
                     }
                 }
             };
         }
     }
 }