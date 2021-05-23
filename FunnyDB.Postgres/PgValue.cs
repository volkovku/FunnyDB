using System;
using NpgsqlTypes;

namespace FunnyDB.Postgres
{
    public sealed class PgValue
    {
        public PgValue(NpgsqlDbType dbType, Func<object> value)
        {
            DbType = dbType;
            Value = value;
        }

        public readonly NpgsqlDbType DbType;

        public readonly Func<object> Value;

        private bool Equals(PgValue other)
        {
            return DbType == other.DbType && Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is PgValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) DbType * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }
    }
}