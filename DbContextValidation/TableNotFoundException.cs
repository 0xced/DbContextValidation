using System;
using System.Data.Common;

#if EFCORE
namespace DbContextValidation.EFCore
#else
namespace DbContextValidation.EF6
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