using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace FunnyDB.Test
{
    [TestFixture]
    public class SqlLinterTests
    {
        [Test]
        public void SqlLinter_ShouldFoundErrors()
        {
            var codeWithError = @"
[Test]
public void SqlQuery_ShouldUseNamedParameters()
{
    var accountId = (777, ""account_id"");

    var query = sql(() => $@""
        |SELECT event_time, balance
        |  FROM charges
        | WHERE account_id = {p(accountId)}
        | UNION ALL
        |SELECT event_time, balance
        |  FROM withdraws
        | WHERE account_id = {accountId}"");
";

            SqlQueryLinter.Validate(codeWithError, out var errors).Should().BeFalse();
            errors.Count.Should().Be(1);
            errors.First().Line.Should().Be(14);
            errors.First().Position.Should().Be(30);
        } 
    }
}