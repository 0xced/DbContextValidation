using System;
using System.Collections.Generic;
using Docker.DotNet.Models;

namespace DbSchemaValidator.Tests
 {
     public static class Config
     {
         public static readonly Provider Provider = Provider.SqlServer;
         public static readonly string Schema = "dbo";

         private static readonly string Host = "localhost";
         private static readonly string Port = "1433";
         private static readonly string Database = "tempdb";
         private static readonly string User = "sa";
         private static readonly string Password = "SqlServer-doc4er";
 
         public static readonly string ConnectionString = $"Server={Host};Database={Database};User Id={User};Password={Password}";
         
         public static CreateContainerParameters ContainerParameters(string containerName, Func<string, string> sqlDirectory)
         {
             return new CreateContainerParameters
             {
                 Name = containerName,
                 Image = "genschsa/mssql-server-linux:latest",
                 Env = new [] { "ACCEPT_EULA=Y", $"MSSQL_SA_PASSWORD=${Password}" },
                 HostConfig = new HostConfig
                 {
                     Binds = new [] { sqlDirectory("SQL.SqlServer") + ":/docker-entrypoint-initdb.d:ro" },
                     PortBindings = new Dictionary<string, IList<PortBinding>>
                     {
                         ["1433/tcp"] = new []{ new PortBinding { HostPort = Port } }
                     }
                 }
             };
         }
     }
 }