using System;
using System.Linq;
using FluentAssertions;
using FunnyDB.Postgres;
using Npgsql;
using NUnit.Framework;
using static FunnyDB.Dialect;

namespace FunnyDB.Test
{
    [TestFixture]
    public class SqlIntegrationTest
    {
        private static readonly DateTime Now = new DateTime(2020, 12, 07, 23, 17, 00);

        [Test]
        public void SqlQuery_WithoutParameters_ItShouldReadDataFromDatabase()
        {
            DbTest(connectionString =>
            {
                using var cn = new NpgsqlConnection(connectionString);

                var accounts = sql(() => @"
                    |SELECT * 
                    |  FROM accounts
                    | ORDER BY name;"
                ).Execute(cn).Map(Account.Produce).ToList();

                accounts.Count.Should().Be(5);

                accounts[0].Name.Should().Be("a");
                accounts[0].Balance.Should().Be(100);
                accounts[0].UpdatedAt.Should().Be(new DateTime(2020, 12, 06, 23, 17, 00));
                accounts[0].SmsNumber.Should().Be("+79267770001");

                accounts[1].Name.Should().Be("b");
                accounts[1].Balance.Should().Be(1000);
                accounts[1].UpdatedAt.Should().Be(new DateTime(2020, 12, 05, 23, 17, 00));
                accounts[1].SmsNumber.Should().BeNull();

                accounts[2].Name.Should().Be("c");
                accounts[2].Balance.Should().Be(0);
                accounts[2].UpdatedAt.Should().Be(new DateTime(2020, 12, 07, 11, 17, 00));
                accounts[2].SmsNumber.Should().BeNull();
            });
        }

        [Test]
        public void SqlQuery_WithParameters_ItShouldReadDataFromDatabase()
        {
            DbTest(connectionString =>
            {
                using var cn = new NpgsqlConnection(connectionString);

                var minBalance = 50;
                var maxBalance = 200;
                var accounts = sql(() => $@"
                    |SELECT * 
                    |  FROM accounts
                    | WHERE sms_number IS NOT NULL
                    |   AND balance BETWEEN {p(minBalance)} AND {p(maxBalance)} 
                    | ORDER BY name;"
                ).Execute(cn).Map(Account.Produce).ToList();

                accounts.Count.Should().Be(1);

                accounts[0].Name.Should().Be("a");
                accounts[0].Balance.Should().Be(100);
                accounts[0].UpdatedAt.Should().Be(new DateTime(2020, 12, 06, 23, 17, 00));
                accounts[0].SmsNumber.Should().Be("+79267770001");
            });
        }

        [Test]
        public void SqlQuery_ItShouldUpdateDataInDatabase()
        {
            DbTest(connectionString =>
            {
                using var cn = new NpgsqlConnection(connectionString);

                var accountName = "d";

                var account = sql(() => $@"
                    |SELECT * 
                    |  FROM accounts
                    | WHERE name = {p(accountName)};"
                ).Execute(cn).Map(Account.Produce).Single();

                var now = DateTime.UtcNow;
                var newBalance = account.Balance + Math.Abs(account.Balance) * 3;

                sql(() => $@"
                    |UPDATE accounts
                    |   SET balance = {p(newBalance)},
                    |       updated_at = {p(now)}
                    | WHERE id = {p(account.Id)}"
                ).ExecuteNonQuery(cn);

                var updatedAccount = sql(() => $@"
                    |SELECT * 
                    |  FROM accounts
                    | WHERE name = {p(accountName)};"
                ).Execute(cn).Map(Account.Produce).Single();

                updatedAccount.Id.Should().Be(account.Id);
                updatedAccount.Balance.Should().Be(newBalance);
                updatedAccount.UpdatedAt.Should().BeCloseTo(now, TimeSpan.FromMilliseconds(100));
            });
        }

        [Test]
        public void SqlQuery_ItShouldRollbackTransactionOnError()
        {
            DbTest(connectionString =>
            {
                var accountName = "d";
                Account account;
                using var cn = new NpgsqlConnection(connectionString);
                {
                    account = sql(() => $@"
                        |SELECT * 
                        |  FROM accounts
                        | WHERE name = {p(accountName)};"
                    ).Execute(cn).Map(Account.Produce).Single();
                }

                try
                {
                    Session.Tx(connectionString, session =>
                    {
                        var now = DateTime.UtcNow;
                        var newBalance = account.Balance + Math.Abs(account.Balance) * 3;

                        sql(() => $@"
                            |UPDATE accounts
                            |   SET balance = {p(newBalance)},
                            |       updated_at = {p(now)}
                            | WHERE id = {p(account.Id)}"
                        ).ExecuteNonQuery(session);

                        throw new InvalidOperationException("Something bad happen");
                    });
                }
                catch (Exception)
                {
                    // ignore
                }

                var updatedAccount = sql(() => $@"
                    |SELECT * 
                    |  FROM accounts
                    | WHERE name = {p(accountName)};"
                ).Execute(cn).Map(Account.Produce).Single();

                updatedAccount.Id.Should().Be(account.Id);
                updatedAccount.Balance.Should().Be(account.Balance);
                updatedAccount.UpdatedAt.Should().Be(account.UpdatedAt);
            });
        }

        [Test]
        public void SqlQuery_ItShouldPerformAutoCommitOnEachSentence()
        {
            DbTest(connectionString =>
            {
                var accountName = "d";
                Account account;
                using var cn = new NpgsqlConnection(connectionString);
                {
                    account = sql(() => $@"
                        |SELECT * 
                        |  FROM accounts
                        | WHERE name = {p(accountName)};"
                    ).Execute(cn).Map(Account.Produce).Single();
                }

                try
                {
                    Session.AutoCommit(connectionString, session =>
                    {
                        for (var i = 0; i < 5; i++)
                        {
                            if (i == 4)
                            {
                                throw new InvalidOperationException("Something bad happen");
                            }

                            sql(() => $@"
                                UPDATE accounts SET balance = balance + 100 WHERE id = {p(account.Id)}"
                            ).ExecuteNonQuery(session);
                        }
                    });
                }
                catch (Exception)
                {
                    // ignore
                }

                var updatedAccount = sql(() => $@"
                    |SELECT * 
                    |  FROM accounts
                    | WHERE name = {p(accountName)};"
                ).Execute(cn).Map(Account.Produce).Single(); 

                updatedAccount.Id.Should().Be(account.Id);
                updatedAccount.Balance.Should().Be(account.Balance + 400);
                updatedAccount.UpdatedAt.Should().Be(account.UpdatedAt);
            });
        }

        private static void DbTest(Action<string> f)
        {
            if (!GetConnectionString(out var connectionString))
            {
                Assert.Ignore("Connection string is not set.");
                return;
            }

            PolluteDb(connectionString);
            f(connectionString);
        }

        private static void PolluteDb(string connectionString)
        {
            Session.Tx(connectionString, session =>
            {
                sql(() => @"
                    |CREATE TABLE IF NOT EXISTS accounts (
                    |  id SERIAL PRIMARY KEY,
                    |  name TEXT NOT NULL,
                    |  updated_at TIMESTAMP NOT NULL,
                    |  balance BIGINT NOT NULL,
                    |  sms_number TEXT NULL
                    |)"
                ).ExecuteNonQuery(session);

                sql(() => "TRUNCATE TABLE accounts;").ExecuteNonQuery(session);

                sql(() => $@"
                    |INSERT INTO accounts (name, updated_at, balance, sms_number)
                    |VALUES ('a', {p(Now)} - interval '24 hours', 100, '+79267770001'),
                    |       ('b', {p(Now)} - interval '48 hours', 1000, NULL),
                    |       ('c', {p(Now)} - interval '12 hours', 0, NULL),
                    |       ('d', {p(Now)} - interval '72 hours', -100, NULL),
                    |       ('f', {p(Now)} - interval '10 hours', 0, '+79059990099');"
                ).ExecuteNonQuery(session);
            });
        }

        private static bool GetConnectionString(out string connectionString)
        {
            connectionString = Environment.GetEnvironmentVariable("FunnyDb_Test_ConnectionString");
            return !string.IsNullOrWhiteSpace(connectionString);
        }

        private class Account
        {
            public static Account Produce(ISqlResultSet rs) => new Account(
                rs.Long("id"),
                rs.String("name"),
                rs.DateTime("updated_at"),
                rs.Long("balance"),
                rs.String("sms_number", null)
            );

            private Account(long id, string name, DateTime updatedAt, long balance, string smsNumber)
            {
                Id = id;
                Name = name;
                UpdatedAt = updatedAt;
                Balance = balance;
                SmsNumber = smsNumber;
            }

            public readonly long Id;
            public readonly string Name;
            public readonly DateTime UpdatedAt;
            public readonly long Balance;
            public readonly string SmsNumber;
        }
    }
}