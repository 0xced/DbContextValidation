﻿using System;
using System.Data.Common;
using MySql.Data.MySqlClient;

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

        public static string DockerArguments(Func<string, string> sqlDirectory)
        {
            return string.Join(" ",
                $"-e MYSQL_ROOT_PASSWORD={Password}",
                $"-e MYSQL_DATABASE={Database}",
                "-e MYSQL_ROOT_HOST=%",
                $"--volume \"{sqlDirectory("SQL.MySQL")}:/docker-entrypoint-initdb.d:ro\"",
                $"--publish {Port}:3306/tcp",
                "--detach",
                "mysql/mysql-server:5.7");
        }

        public static DbConnection CreateDbConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
    }
}