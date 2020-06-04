using Microsoft.EntityFrameworkCore.Metadata;

namespace DbContextValidation.EFCore
{
    internal partial class ModelColumn
    {
        internal ModelColumn(IProperty property)
        {
            ColumnName = property.GetColumnName();
        }
    }
}