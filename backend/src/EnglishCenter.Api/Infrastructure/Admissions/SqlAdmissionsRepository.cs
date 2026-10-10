namespace EnglishCenter.Api.Infrastructure.Admissions;

using System.Data;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Finance;
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Enrollment;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public sealed class SqlAdmissionsRepository(SqlAuthDeadlineScope scope)
{
    public SqlEnrollmentRepository Shared { get; } = new(scope);
    public async Task<int> RequireAsync(int registration, CancellationToken ct) =>
        await scope.Context.Registrations.AsNoTracking().Where(x => x.RegistrationId == registration).Select(x => (int?)x.StudentId).SingleOrDefaultAsync(ct)
        ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
    public Task<bool> PaymentAsync(int registration, CancellationToken ct) =>
        (from i in scope.Context.Invoices from p in scope.Context.Payments where i.RegistrationId == registration && p.InvoiceId == i.InvoiceId select p.PaymentId).AnyAsync(ct);
    public void AddInvoice(Invoice invoice) => scope.Context.Invoices.Add(invoice);
    public async Task<string> AllocateAsync(short year, CancellationToken ct)
    {
        await using var cmd = scope.Context.Database.GetDbConnection().CreateCommand();
        cmd.Transaction = scope.Context.Database.CurrentTransaction!.GetDbTransaction(); cmd.CommandTimeout = scope.SqlSeconds;
        cmd.CommandType = CommandType.StoredProcedure; cmd.CommandText = "dbo.usp_AllocateInvoiceNumber";
        cmd.Parameters.Add(new SqlParameter("@issueYear", SqlDbType.SmallInt) { Value = year });
        System.Data.Common.DbDataReader reader;
        try { reader = await cmd.ExecuteReaderAsync(ct); }
        catch (SqlException e) when (e.Number == 51108) { throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT); }
        await using var ownedReader = reader;
        if (reader.FieldCount != 1 || reader.GetName(0) != "invoiceNumber" || !await reader.ReadAsync(ct)) throw new InvalidOperationException("Invalid allocator projection.");
        var number = reader.GetString(0);
        if (await reader.ReadAsync(ct) || await reader.NextResultAsync(ct)) throw new InvalidOperationException("Invalid allocator projection.");
        return number;
    }
    public async Task<RegistrationState> CurrentAsync(int registration, CancellationToken ct)
    {
        var student = await RequireAsync(registration, ct); var set = await Shared.ReadSetAsync(student, [registration], ct);
        return new(set, set.Registrations.Single());
    }
}
