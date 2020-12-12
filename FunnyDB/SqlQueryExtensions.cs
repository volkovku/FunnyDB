using System;
using System.Data;

namespace FunnyDB
{
    public static class SqlQueryExtensions
    {
        public static SqlQueryResult Execute(this SqlQuery sql, ISession session)
        {
            var connection = session.Connection;
            var transaction = session.Transaction;
            return sql.Execute(connection, transaction, autoOpen: false);
        }

        public static SqlQueryResult Execute(
            this SqlQuery sql,
            IDbConnection connection,
            IDbTransaction transaction = null,
            bool autoOpen = true)
        {
            var reader = Execute(sql, connection, transaction, autoOpen, cmd => cmd.ExecuteReader());
            return new SqlQueryResult(reader);
        }

        public static T ExecuteScalar<T>(this SqlQuery sql, ISession session)
        {
            var connection = session.Connection;
            var transaction = session.Transaction;
            return sql.ExecuteScalar<T>(connection, transaction, autoOpen: false);
        }

        public static T ExecuteScalar<T>(
            this SqlQuery sql,
            IDbConnection connection,
            IDbTransaction transaction = null,
            bool autoOpen = true)
        {
            return (T) Execute(sql, connection, transaction, autoOpen, cmd => cmd.ExecuteScalar());
        }

        public static int ExecuteNonQuery(this SqlQuery sql, ISession session)
        {
            var connection = session.Connection;
            var transaction = session.Transaction;
            return sql.ExecuteNonQuery(connection, transaction, autoOpen: false);
        }

        
        public static int ExecuteNonQuery(
            this SqlQuery sql,
            IDbConnection connection,
            IDbTransaction transaction = null,
            bool autoOpen = true)
        {
            return Execute(sql, connection, transaction, autoOpen, cmd => cmd.ExecuteNonQuery());
        }

        private static T Execute<T>(
            SqlQuery sql,
            IDbConnection connection,
            IDbTransaction transaction,
            bool autoOpen,
            Func<IDbCommand, T> f)
        {
            if (autoOpen && connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            var cmd = connection.CreateCommand();
            cmd.CommandText = sql.Sql;
            foreach (var parameter in sql.Parameters)
            {
                parameter.AddToCommand(cmd);
            }

            if (transaction != null)
            {
                cmd.Transaction = transaction;
            }

            return f(cmd);
        }
    }
}