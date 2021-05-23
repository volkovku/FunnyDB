using System;
using System.Data;

namespace FunnyDB
{
    internal sealed class DbSqlQueryParameter : SqlQueryParameter
    {
        public DbSqlQueryParameter(int index, DbType dbType, Func<object> value) : base(index)
        {
            DbType = dbType;
            Value = value;
        }

        public DbSqlQueryParameter(string name, DbType dbType, Func<object> value) : base(name)
        {
            DbType = dbType;
            Value = value;
        }

        public readonly DbType DbType;

        public override Func<object> Value { get; }

        public override SqlQueryParameter ChangeIndex(int index)
        {
            return new DbSqlQueryParameter(index, DbType, Value);
        }

        public override void AddToCommand(IDbCommand command)
        {
            var p = command.CreateParameter();
            p.ParameterName = Name;
            p.DbType = DbType;
            p.Value = Value();
            command.Parameters.Add(p);
        }
    }
}