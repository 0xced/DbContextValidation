using System;
using System.Data.Common;

#if EFCORE
namespace DbContextValidation.EFCore
#else
namespace DbContextValidation.EF6
#endif
{
    internal partial class ModelColumn : DbColumn
    {
        internal ModelColumn(string columnName, int columnOrdinal, Type dataType, string dataTypeName)
        {
            ColumnName = columnName;
            ColumnOrdinal = columnOrdinal;
            DataType = dataType;
            DataTypeName = dataTypeName;
        }
    }
}