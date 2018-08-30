namespace DbSchemaValidator.Tests
 {
     public static class Config
     {
         public static readonly Provider Provider = Provider.Npgsql;
         public static readonly string Schema = "public";
         
         public static readonly string Host = "localhost";
         public static readonly string Port = "5432";
         public static readonly string Database = "DbSchemaValidator";
         public static readonly string User = "postgres";
         public static readonly string Password = "docker";

         public static readonly string ConnectionString = $"Host={Host};Database={Database};UserName={User};Password={Password}";
     }
 }