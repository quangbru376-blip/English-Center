namespace EnglishCenter.Tests.Common;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Infrastructure.Finance;
using EnglishCenter.Api.Services.Finance;
using Xunit;

public sealed class M2FinanceContractTests
{
    [Theory]
    [InlineData("{\"overdueOnly\":\"true\"}")]
    [InlineData("{\"status\":\"OVERDUE\"}")]
    [InlineData("{\"pageSize\":101}")]
    [InlineData("{\"page\":0}")]
    [InlineData("{\"courseId\":null}")]
    [InlineData("{\"name\":null}")]
    [InlineData("{\"status\":\"PAID\",\"status\":\"UNPAID\"}")]
    [InlineData("{\"paidAt\":\"2026-01-01\"}")]
    [InlineData("{\"issueFromDate\":\"2026-02-30\"}")]
    [InlineData("{\"issueFromDate\":\"2026-10-11\",\"issueToDate\":\"2026-10-10\"}")]
    [InlineData("{\"email\":\"é\"}")]
    [InlineData("{\"invoiceNumber\":\"HD-2026-000000001\"}")]
    [InlineData("{\"sort\":\"paidAt\"}")]
    public void SearchRejectsUnsupportedOrAmbiguousInput(string json) => Assert.Throws<ApiErrorException>(() => FinanceSearchFilter.Parse(json));
    [Fact]
    public void SearchPreservesIndependentFiltersVnInclusiveBoundaryAndLiteralPrefix()
    {
        var f = FinanceSearchFilter.Parse("{\"invoiceNumber\":\" HD-%_ \",\"status\":\"PAID\",\"overdueOnly\":true,\"phone\":\"+84 912-345-678\",\"issueFromDate\":\"0001-01-01\",\"issueToDate\":\"9999-12-31\"}");
        Assert.Equal("HD-%_", f.InvoiceNumber); Assert.Equal("PAID", f.Status); Assert.True(f.OverdueOnly); Assert.Equal("0912345678", f.Phone);
        Assert.Equal(DateTime.MinValue, f.FromUtc); Assert.Equal(new DateTime(9999, 12, 31, 17, 0, 0), f.ToExclusiveUtc); Assert.Equal(20, f.PageSize);
    }
    [Fact]
    public void PaymentWireNormalizationBindsLiteralAbsenceAndPutPresence()
    {
        const string rv = "AAAAAAAAAAE="; var id = Guid.NewGuid();
        var b = System.Text.Json.JsonSerializer.Serialize(new { amount = "123456789012345678", method = "CASH", expectedVersions = new { registration = rv, invoice = rv }, expectedSet = new { registrationId = 1, paymentId = (int?)null } });
        var input = FinanceInput.Parse(51, 2, id, b, 65536); Assert.Contains("\"transactionReference\":null", System.Text.Encoding.UTF8.GetString(input.Intent.CopyBytes()));
        Assert.Throws<ApiErrorException>(() => FinanceInput.Parse(51, 2, id, b.Replace("\"CASH\"", "\"TRANSFER\""), 65536));
        var missing = System.Text.Json.JsonSerializer.Serialize(new { method = "CASH", reason = "why", expectedVersions = new { payment = rv, invoice = rv } });
        Assert.Throws<ApiErrorException>(() => FinanceInput.Parse(52, 3, id, missing, 65536));
    }
}
