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
        }
    }
}