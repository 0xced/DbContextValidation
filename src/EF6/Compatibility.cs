// For .NET < 4.7.1 Compatibility

#if NET45
#pragma warning disable 1591
// ReSharper disable once CheckNamespace
namespace System.Data.Common
{
    public abstract class DbColumn
    {
        // ReSharper disable once InconsistentNaming
        public bool? AllowDBNull { get; protected set; }
        public string BaseCatalogName { get; protected set; }
        public string BaseColumnName { get; protected set; }
        public string BaseSchemaName { get; protected set; }
        public string BaseServerName { get; protected set; }
        public string BaseTableName { get; protected set; }
        public string ColumnName { get; protected set; }
        public int? ColumnOrdinal { get; protected set; }
        public int? ColumnSize { get; protected set; }
        public bool? IsAliased { get; protected set; }
        public bool? IsAutoIncrement { get; protected set; }
        public bool? IsExpression { get; protected set; }
        public bool? IsHidden { get; protected set; }
        public bool? IsIdentity { get; protected set; }
        public bool? IsKey { get; protected set; }
        public bool? IsLong { get; protected set; }
        public bool? IsReadOnly { get; protected set; }
        public bool? IsUnique { get; protected set; }
        public int? NumericPrecision { get; protected set; }
        public int? NumericScale { get; protected set; }
        public string UdtAssemblyQualifiedName { get; protected set; }
        public Type DataType { get; protected set; }
        public string DataTypeName { get; protected set; }
        public virtual object this[string property] => property switch
        {
            nameof(AllowDBNull) => AllowDBNull,
            nameof(BaseCatalogName) => BaseCatalogName,
            nameof(BaseColumnName) => BaseColumnName,
            nameof(BaseSchemaName) => BaseSchemaName,
            nameof(BaseServerName) => BaseServerName,
            nameof(BaseTableName) => BaseTableName,
            nameof(ColumnName) => ColumnName,
            nameof(ColumnOrdinal) => ColumnOrdinal,
            nameof(ColumnSize) => ColumnSize,
            nameof(DataType) => DataType,
            nameof(DataTypeName) => DataTypeName,
            nameof(IsAliased) => IsAliased,
            nameof(IsAutoIncrement) => IsAutoIncrement,
            nameof(IsExpression) => IsExpression,
            nameof(IsHidden) => IsHidden,
            nameof(IsIdentity) => IsIdentity,
            nameof(IsKey) => IsKey,
            nameof(IsLong) => IsLong,
            nameof(IsReadOnly) => IsReadOnly,
            nameof(IsUnique) => IsUnique,
            nameof(NumericPrecision) => NumericPrecision,
            nameof(NumericScale) => NumericScale,
            nameof(UdtAssemblyQualifiedName) => UdtAssemblyQualifiedName,
            _ => null
        };
    }

    public interface IDbColumnSchemaGenerator
    {
        Collections.ObjectModel.ReadOnlyCollection<DbColumn> GetColumnSchema();
    }

    public static class DbDataReaderExtensions
    {
        public static Collections.ObjectModel.ReadOnlyCollection<DbColumn> GetColumnSchema(this DbDataReader reader)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (reader is IDbColumnSchemaGenerator columnSchemaGenerator)
            {
                return columnSchemaGenerator.GetColumnSchema();
            }
            throw new NotSupportedException();
        }

        public static bool CanGetColumnSchema(this DbDataReader reader)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            return reader is IDbColumnSchemaGenerator;
        }
    }
}
#endif