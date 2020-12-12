FunnyDB - a simple and lightweight query builder and object mapper for .Net
===========================================================================

FunnyDB was inspired by [ScalikeJDBC](http://scalikejdbc.org/).

It was developed especially for programmers who likes plain SQL like me.

FunnyDB uses power of string interpolation and solves huge problem - 
gap between parameter definition and it value assigment.

Packages
--------

NuGet releases: https://www.nuget.org/packages/FunnyDB/

First example
-------------

In code above we query accounts  

```csharp
// Import necessary directives like: sql and p
using static FunnyDB.Dialect;

// Create connection
using var cn = new NpgsqlConnection(connectionString);

// Define parameters values they are passed via method paramater usually
var minBalance = 50;
var maxBalance = 200;

// Create query, execute it and map results
// To bind minBalance and maxBalance use p(...) directive
var accounts = sql(() => $@"
    |SELECT * 
    |  FROM accounts
    | WHERE sms_number IS NOT NULL
    |   AND balance BETWEEN {p(minBalance)} AND {p(maxBalance)} 
    | ORDER BY name;"
).Execute(cn).Map(Account.Produce).ToList();

class Account
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
```

Compose queries
---------------

You can compose you queries with '+' or '/' operators. 
Operator '+' concatenates two queries with line break between queries instead of '/' operator. 

```csharp
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

var query = charges + sql(() => " UNION ALL") + withdraws;

Console.WriteLine(query.Sql);
```

A result of this code execution is follow SQL code:

```sql
SELECT event_time, balance
  FROM charges
 WHERE account_id = @account_id
   AND event_time BETWEEN @p_1_ AND @p_2_
 UNION ALL
SELECT event_time, balance
  FROM withdraws
 WHERE account_id = @account_id
   AND event_time BETWEEN @p_3_ AND @p_4_
```

Transactions
------------

### Tx block 

Executes query / update in block-scoped transactions.

In the end of scope transaction will be committed.

If exception will happen in transaction scope rollback will performed. 

```csharp
// Sessions are depenend from database drivers implementation
using FunnyDB.Postgres;

Session.Tx(connectionString, session =>
{
    // --- Transcation scope start ---
    sql(() => $"INSERT INTO account (name, balance) VALUES ({p(name1)}, {p(balance1)}, null)".ExecuteNonQuery(session);
    sql(() => $"INSERT INTO account (name, balance) VALUES ({p(name2)}, {p(balance2)}, null)".ExecuteNonQuery(session);
    // --- Transaction scope end ---
}
```

### AutoCommit block

Executes query / update in auto-commit mode.

When using AutoCommit session, every operation will be executed in auto-commit mode.

```csharp
Session.AutoCommit(connectionString, session =>
{
    sql(() => $"INSERT INTO account (name, balance) VALUES ({p(name1)}, {p(balance1)}, null)".ExecuteNonQuery(session); // auto-commit
    sql(() => $"INSERT INTO account (name, balance) VALUES ({p(name2)}, {p(balance2)}, null)".ExecuteNonQuery(session); // auto-commit
}
```

Map domain model values to FunnyDB parameters
---------------------------------------------

FunnyDB uses parameter types restrictions to prevent unexpectedly not supported type assignments.

Follow code will fail on compilation:

```csharp
public class AccountId
{
    public AccountId(long value)
    {
        Value = value;
    }

    public readonly long Value;
}

var accountId = new AccountId(1);
var query = sql(() => $"SELECT balance FROM accounts WHERE id = {p(accountId)}");
```

To fix this issue we can use p(accountId.Value) of course, but it little bit verbose and annoying.

As alternative we can write mapping between domain and database model:

```csharp
public static class Dialect
{
    public static string p(AccountId accountId) => FunnyDB.Dialect.p(accountId.Value);
}
```

And use it in our query:
```csharp
// Import defined domain model dialect directives
using static Dialect;

// Now query compiles and works well
var accountId = new AccountId(1);
sql(() => $"SELECT balance FROM accounts WHERE id = {p(accountId)}");
```
