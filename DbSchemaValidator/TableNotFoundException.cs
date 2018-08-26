using System;
using System.Data.Common;

#if EFCORE
namespace DbSchemaValidator.EFCore
#else
namespace DbSchemaValidator.EF6
#endif
{
    internal class TableNotFoundException : Exception
    {
        public TableNotFoundException(DbException dbException) : base(null, dbException)
        {
            DbException = dbException;
        }
        
        public DbException DbException { get; }
    }
}