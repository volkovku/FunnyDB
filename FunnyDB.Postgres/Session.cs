using System;
using System.Data;
using Npgsql;

namespace FunnyDB.Postgres
{
    public class Session : ISession
    {
        public static void Tx(string connectionString, Action<ISession> body, IsolationLevel? level = null)
        {
            Tx<object>(connectionString, level: level, body: session =>
            {
                body(session);
                return null;
            });
        }

        public static T Tx<T>(string connectionString, Func<ISession, T> body, IsolationLevel? level = null)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = BeginTransaction(connection, level))
                {
                    try
                    {
                        var session = new Session(connection, transaction);
                        var result = body(session);
                        transaction.Commit();
                        return result;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch (Exception)
                        {
                            // ignore;
                        }

                        throw;
                    }
                }
            }
        }

        public static void AutoCommit(string connectionString, Action<ISession> body)
        {
            AutoCommit<object>(connectionString, session =>
            {
                body(session);
                return null;
            });
        }

        public static T AutoCommit<T>(string connectionString, Func<ISession, T> body)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var session = new Session(connection, null);
                var result = body(session);
                return result;
            }
        }

        private static NpgsqlTransaction BeginTransaction(NpgsqlConnection connection, IsolationLevel? level = null)
        {
            return level == null ? connection.BeginTransaction() : connection.BeginTransaction(level.Value);
        }

        private Session(IDbConnection connection, IDbTransaction transaction)
        {
            Connection = connection;
            Transaction = transaction;
        }

        public IDbConnection Connection { get; }

        public IDbTransaction Transaction { get; }

        public void Commit()
        {
            Transaction.Commit();
        }

        public void Rollback()
        {
            Transaction.Rollback();
        }
    }
}