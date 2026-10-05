namespace EnglishCenter.SqlTests;

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class AllocatorConcurrencyTests
{
    private static async Task<SqlConnection> Open()
    {
        var connection=new SqlConnection("Server=lpc:.\\SQLEXPRESS;Database=EnglishCenter_D07_20261005;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        Assert.Equal("EnglishCenter_D07_20261005",connection.Database);
        return connection;
    }

    private static async Task<string> Allocate(SqlConnection connection,SqlTransaction? transaction,short year)
    {
        using var command=connection.CreateCommand();command.Transaction=transaction;
        command.CommandText="EXEC dbo.usp_AllocateInvoiceNumber @issueYear=@year";
        command.Parameters.Add(new SqlParameter("@year",SqlDbType.SmallInt){Value=year});
        return Assert.IsType<string>(await command.ExecuteScalarAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task NoCallerTransaction_FailsClosed()
    {
        await using var connection=await Open();
        var error=await Assert.ThrowsAsync<SqlException>(()=>Allocate(connection,null,2026));Assert.Equal(51107,error.Number);
    }

    [Theory]
    [InlineData(1)] [InlineData(2)]
    public async Task MissingYearRangeAndCreatedRow_BlocksSecondConnectionUntilCallerRollback(int calls)
    {
        await using var first=await Open();await using var second=await Open();
        await using var firstTx=(SqlTransaction)await first.BeginTransactionAsync(TestContext.Current.CancellationToken);
        await using var secondTx=(SqlTransaction)await second.BeginTransactionAsync(TestContext.Current.CancellationToken);
        for(var i=1;i<=calls;i++) Assert.Equal("HD-9998-"+i.ToString("D8",System.Globalization.CultureInfo.InvariantCulture),await Allocate(first,firstTx,9998));
        using(var timeout=second.CreateCommand())
        {
            timeout.Transaction=secondTx;timeout.CommandText="SET LOCK_TIMEOUT 250;";
            await timeout.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        }
        var conflict=await Assert.ThrowsAsync<SqlException>(()=>Allocate(second,secondTx,9998));Assert.Equal(1222,conflict.Number);
        await secondTx.RollbackAsync(TestContext.Current.CancellationToken);
        await firstTx.RollbackAsync(TestContext.Current.CancellationToken);
        await using var freshTx=(SqlTransaction)await second.BeginTransactionAsync(TestContext.Current.CancellationToken);
        Assert.Equal("HD-9998-00000001",await Allocate(second,freshTx,9998));
        await freshTx.RollbackAsync(TestContext.Current.CancellationToken);
        using var count=second.CreateCommand();count.CommandText="SELECT COUNT(*) FROM dbo.InvoiceNumberCounter WHERE issue_year=9998";
        Assert.Equal(0,await count.ExecuteScalarAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(0,"00000001")] [InlineData(41,"00000042")] [InlineData(99999998,"99999999")]
    public async Task ExistingCounter_IncrementsExactlyAndReturnsMaximumWithoutOverflow(int previous,string next)
    {
        await using var f=await SqlFixture.CreateAsync();
        await f.ExecuteAsync($"INSERT dbo.InvoiceNumberCounter(issue_year,last_value) VALUES(2026,{previous});");
        var r=Assert.Single(await SqlRows.ReadAsync(f,"EXEC dbo.usp_AllocateInvoiceNumber @issueYear=2026"));
        Assert.Equal("HD-2026-"+next,r.Value(0,"invoiceNumber"));Assert.Equal(16,r.Schema[0].ColumnSize);
        Assert.Equal(1,await f.Context.Database.SqlQueryRaw<int>("SELECT @@TRANCOUNT AS Value").SingleAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(1,"0001")] [InlineData(9999,"9999")]
    public async Task YearBoundaries_ArePaddedWithoutTruncation(short year,string padded)
    {
        await using var connection=await Open();await using var tx=(SqlTransaction)await connection.BeginTransactionAsync(TestContext.Current.CancellationToken);
        Assert.Equal("HD-"+padded+"-00000001",await Allocate(connection,tx,year));
        await tx.RollbackAsync(TestContext.Current.CancellationToken);
    }
}
