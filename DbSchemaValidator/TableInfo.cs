using System.Collections.Generic;

#if EFCORE
namespace DbSchemaValidator.EFCore
#else
namespace DbSchemaValidator.EF6
#endif
{
    public class TableInfo
    {
        public TableInfo(IReadOnlyCollection<string> columnNames, bool? caseSensitive)
        {
            ColumnNames = columnNames;
            CaseSensitive = caseSensitive;
        }

        public IReadOnlyCollection<string> ColumnNames { get; }
        public bool? CaseSensitive { get; }
    }
}