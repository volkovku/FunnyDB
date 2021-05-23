using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using static FunnyDB.Dialect;

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

        [Test]
        public void SqlLinter_ShouldFoundErrorsInFolder()
        {
            var accountId = (777, "account_id");
            
            sql(() => $@"
                |SELECT event_time, balance
                |  FROM charges
                | WHERE account_id = {p(accountId)}
            | UNION ALL
                |SELECT event_time, balance
                |  FROM withdraws
                | WHERE account_id = {accountId}");    // <----------- ERROR RIGHT HERE
            
            var assemblyPath = GetType().Assembly.Location;
            var projectFolder = Enumerable.Range(0, 4).Aggregate(assemblyPath, (p, _) => Path.GetDirectoryName(p));
            SqlQueryLinter.ValidateFolder(projectFolder, out var errors).Should().BeFalse();
            errors.Count.Should().Be(1);
            errors.First().Path.Should().Be(Path.Combine(projectFolder, "SqlLinterTests.cs"));
            errors.First().Error.Line.Should().Be(49);
            errors.First().Error.Position.Should().Be(38);
        }
    }
    
}