using System;
using System.Data.Common;

#if EFCORE
namespace DbSchemaValidator.EFCore
#else
namespace DbSchemaValidator.EF6
#endif
{
    public class TableNotFoundException : Exception
    {
        public TableNotFoundException(DbException dbException, string selectStatement) : base(null, dbException)
        {
            DbException = dbException;
            SelectStatement = selectStatement;
        }
        
        public DbException DbException { get; }
        
        public string SelectStatement { get; }
    }
}