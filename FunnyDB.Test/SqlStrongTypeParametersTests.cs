using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using static FunnyDB.Dialect;

namespace Domain
{
    public class AccountId
    {
        public AccountId(long value)
        {
            Value = value;
        }

        public readonly long Value;
    }

    public static class Dialect
    {
        // ReSharper disable once InconsistentNaming
        public static string p(AccountId accountId) => FunnyDB.Dialect.p(accountId.Value);
    }
}

namespace FunnyDB.Test
{
    using Domain;
    using static Domain.Dialect;

    [TestFixture]
    public class SqlStrongTypeParametersTests
    {
        [Test]
        public void SqlQuery_ShouldBeExtendableWithDomainModelTypes()
        {
            var accountId = new AccountId(1);
            var query = sql(() => $"SELECT balance FROM accounts WHERE id = {p(accountId)}");
            query.Sql.Should().Be("SELECT balance FROM accounts WHERE id = @p_0_");
            query.Parameters.Count.Should().Be(1);
            query.Parameters.First().Value().Should().Be(accountId.Value);
        }
    }
}