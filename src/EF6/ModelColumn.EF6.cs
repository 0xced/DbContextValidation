using System.Data.Entity.Core.Metadata.Edm;

namespace DbContextValidation.EF6
{
    internal partial class ModelColumn
    {
        internal ModelColumn(EdmProperty column)
        {
            ColumnName = column.Name;
        }
    }
}