using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using static FunnyDB.Dialect;

namespace FunnyDB.Test
{
    [TestFixture]
    public sealed class SqlQueryReuseStatementsTest
    {
        [Test]
        public void SqlQuery_ShouldAllowToReuseQueries()
        {
            var findAccount = FindAccount();

            var q1 = findAccount("account_1");
            q1.Parameters.ToArray()[0].Value().Should().Be("account_1");
            
            var q2 = findAccount("account_2");
            q2.Parameters.ToArray()[0].Value().Should().Be("account_2");
        }

        private static Func<string, SqlQuery> FindAccount()
        {
            return reusable<string>(accountId => sql(() => $"SELECT * FROM accounts WHERE {p(accountId)}"));
        }

        // ReSharper disable once InconsistentNaming
        private static Func<T1, SqlQuery> reusable<T1>(Func<Func<T1>, SqlQuery> builder)
        {
            T1 p1Holder = default;
            SqlQuery query = null;

            var f = new Func<T1, SqlQuery>(p1 =>
            {
                if (query == null)
                {
                    // ReSharper disable once AccessToModifiedClosure
                    query = builder(() => p1Holder);
                }

                p1Holder = p1; 
                return query;
            });

            return f;
        }
    }
}