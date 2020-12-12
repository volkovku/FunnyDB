using System;
using System.Data;

namespace FunnyDB
{
    public sealed class SqlQueryValue
    {
        public readonly DbType DbType;
        public readonly object Value;

        private SqlQueryValue(DbType dbType, object value)
        {
            DbType = dbType;
            Value = value;
        }

        public static implicit operator SqlQueryValue(byte value) => new SqlQueryValue(DbType.Byte, value);
        public static implicit operator SqlQueryValue(byte? value) => new SqlQueryValue(DbType.Byte, value);
        public static implicit operator SqlQueryValue(int value) => new SqlQueryValue(DbType.Int32, value);
        public static implicit operator SqlQueryValue(int? value) => new SqlQueryValue(DbType.Int32, value);
        public static implicit operator SqlQueryValue(long value) => new SqlQueryValue(DbType.Int64, value);
        public static implicit operator SqlQueryValue(long? value) => new SqlQueryValue(DbType.Int64, value);
        public static implicit operator SqlQueryValue(float value) => new SqlQueryValue(DbType.Single, value);
        public static implicit operator SqlQueryValue(float? value) => new SqlQueryValue(DbType.Single, value);
        public static implicit operator SqlQueryValue(double value) => new SqlQueryValue(DbType.Double, value);
        public static implicit operator SqlQueryValue(double? value) => new SqlQueryValue(DbType.Double, value);
        public static implicit operator SqlQueryValue(string value) => new SqlQueryValue(DbType.String, value);
        public static implicit operator SqlQueryValue(DateTime value) => new SqlQueryValue(DbType.DateTime, value);
        public static implicit operator SqlQueryValue(DateTime? value) => new SqlQueryValue(DbType.DateTime, value);
        public static implicit operator SqlQueryValue(bool value) => new SqlQueryValue(DbType.Boolean, value);
        public static implicit operator SqlQueryValue(bool? value) => new SqlQueryValue(DbType.Boolean, value);
        public static implicit operator SqlQueryValue(Guid value) => new SqlQueryValue(DbType.Guid, value);
        public static implicit operator SqlQueryValue(Guid? value) => new SqlQueryValue(DbType.Guid, value);
    }
}