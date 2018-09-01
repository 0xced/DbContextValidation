using System;
using System.Collections.Generic;
using Docker.DotNet.Models;

namespace DbContextValidation.Tests
 {
     public static class Config
     {
         public static readonly Provider Provider = Provider.MySQL;
         public static readonly string Schema = null;

         private static readonly string Host = "localhost";
         private static readonly string Port = "3306";
         private static readonly string Database = "DbContextValidation";
         private static readonly string User = "root";
         private static readonly string Password = "docker";

         public static readonly string ConnectionString = $"Host={Host};Database={Database};UserName={User};Password={Password}";
         
         public static CreateContainerParameters ContainerParameters(string containerName, Func<string, string> sqlDirectory)
         {
             return new CreateContainerParameters
             {
                 Name = containerName,
                 Image = "mysql/mysql-server:5.7",
                 Env = new [] { $"MYSQL_ROOT_PASSWORD={Password}", $"MYSQL_DATABASE={Database}", "MYSQL_ROOT_HOST=%" },
                 HostConfig = new HostConfig
                 {
                     Binds = new [] { sqlDirectory("SQL.MySQL") + ":/docker-entrypoint-initdb.d:ro" },
                     PortBindings = new Dictionary<string, IList<PortBinding>>
                     {
                         ["3306/tcp"] = new []{ new PortBinding { HostPort = Port } }
                     }
                 }
             };
         }
     }
 }