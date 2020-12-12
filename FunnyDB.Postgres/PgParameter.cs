using System;
using System.Data;
using Npgsql;

namespace FunnyDB.Postgres
{
    public class PgParameter : SqlQueryParameter
    {
        private readonly PgValue _pgValue;

        public PgParameter(string name, PgValue value) : base(name)
        {
            _pgValue = value;
        }

        public PgParameter(int index, PgValue value) : base(index)
        {
            _pgValue = value;
        }

        public override object Value => _pgValue.Value;

        public override SqlQueryParameter ChangeIndex(int index)
        {
            return new PgParameter(index, _pgValue);
        }

        public override void AddToCommand(IDbCommand command)
        {
            if (!(command is NpgsqlCommand pgCommand))
            {
                throw new InvalidOperationException(
                    $"Can't add parameter to command with type '{command.GetType()}'. " +
                    $"'{typeof(NpgsqlCommand)}' expected.");
            }

            var p = pgCommand.CreateParameter();
            p.ParameterName = Name;
            p.NpgsqlDbType = _pgValue.DbType;
            p.Value = _pgValue.Value;

            pgCommand.Parameters.Add(p);
        }
    }
}