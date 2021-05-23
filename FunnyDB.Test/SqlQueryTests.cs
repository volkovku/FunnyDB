using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using static FunnyDB.Dialect;

namespace FunnyDB.Test
{
    [TestFixture]
    public class SqlQueryTests
    {
        [Test]
        public void SqlQuery_ShouldGenerateParameters()
        {
            var toDate = DateTime.Now.Date;
            var fromDate = toDate - TimeSpan.FromDays(30);
            var balance = 100;

            var query = sql(() => $@"
                |SELECT * 
                |  FROM accounts
                | WHERE updated_at BETWEEN {p(fromDate)} AND {p(toDate)}
                |   AND balance >= {p(balance)}");

            query.Sql.Should().Be(
                @"SELECT * 
  FROM accounts
 WHERE updated_at BETWEEN @p_0_ AND @p_1_
   AND balance >= @p_2_");

            var parameters = query.Parameters.ToList();
            parameters.Count.Should().Be(3);

            parameters[0].Name.Should().Be("p_0_");
            parameters[0].Value().Should().Be(fromDate);
            parameters[1].Name.Should().Be("p_1_");
            parameters[1].Value().Should().Be(toDate);
            parameters[2].Name.Should().Be("p_2_");
            parameters[2].Value().Should().Be(balance);
        }

        [Test]
        public void SqlQuery_ShouldUseNamedParameters()
        {
            var accountId = (777, "account_id");

            var query = sql(() => $@"
                |SELECT event_time, balance
                |  FROM charges
                | WHERE account_id = {p(accountId)}
                | UNION ALL
                |SELECT event_time, balance
                |  FROM withdraws
                | WHERE account_id = {p(accountId)}");

            query.Sql.Should().Be(
                @"SELECT event_time, balance
  FROM charges
 WHERE account_id = @account_id
 UNION ALL
SELECT event_time, balance
  FROM withdraws
 WHERE account_id = @account_id");

            var parameters = query.Parameters.ToList();
            parameters.Count.Should().Be(1);

            parameters[0].Name.Should().Be("account_id");
            parameters[0].Value().Should().Be(accountId.Item1);
        }

        [Test]
        public void SqlQuery_ItShouldConcatQueries()
        {
            var accountId = (777, "account_id");
            var toDate = DateTime.Now.Date;
            var fromDate = toDate - TimeSpan.FromDays(30);

            var charges = sql(() => $@"
                |SELECT event_time, balance
                |  FROM charges
                | WHERE account_id = {p(accountId)}
                |   AND event_time BETWEEN {p(fromDate)} AND {p(toDate)}");

            var withdraws = sql(() => $@"
                |SELECT event_time, balance
                |  FROM withdraws
                | WHERE account_id = {p(accountId)}
                |   AND event_time BETWEEN {p(fromDate)} AND {p(toDate)}");

            var query = charges + sql(() => "| UNION ALL") + withdraws;
            
            query.Sql.Should().Be(
                @"SELECT event_time, balance
  FROM charges
 WHERE account_id = @account_id
   AND event_time BETWEEN @p_1_ AND @p_2_
 UNION ALL
SELECT event_time, balance
  FROM withdraws
 WHERE account_id = @account_id
   AND event_time BETWEEN @p_3_ AND @p_4_");
 
            var parameters = query.Parameters.OrderBy(_ => _.Name).ToList();
            parameters.Count.Should().Be(5);

            parameters[0].Name.Should().Be("account_id");
            parameters[0].Value().Should().Be(accountId.Item1);
            parameters[1].Name.Should().Be("p_1_");
            parameters[1].Value().Should().Be(fromDate);
            parameters[2].Name.Should().Be("p_2_");
            parameters[2].Value().Should().Be(toDate);
            parameters[3].Name.Should().Be("p_3_");
            parameters[3].Value().Should().Be(fromDate);
            parameters[4].Name.Should().Be("p_4_");
            parameters[4].Value().Should().Be(toDate);
        }
    }
}