namespace EnglishCenter.Api.Services.Finance;

using System.Globalization;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Finance;
using EnglishCenter.Api.Infrastructure.Finance;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Enrollment;
using EnglishCenter.Api.Services.Replay;

public sealed class FinanceService(SqlFinanceRepository repository, IReplayRepository markers, BusinessClock clock)
{
    public async Task<FinanceCommandOutcome> CommandAsync(VerifiedSessionToken token, FinanceInput input, CancellationToken ct)
    {
        await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId, input.Intent.Action, input.RequestId), ct);
        var link = await repository.ResolveAsync(input.Api, input.TargetId, ct);
        foreach (var gate in new[] { "EC:REG:" + link.RegistrationId, "EC:INVOICE:" + link.InvoiceId }.Order(StringComparer.Ordinal)) await markers.AcquireAsync(gate, ct);
        await repository.Shared.PrepareAccountsAsync(token, [link.StudentAccountId], ct);
        await EnrollmentAuthority.RequireAsync(repository.Shared, token, clock, "Accountant", ct);
        var state = await repository.LockAsync(link, input.Api, input.TargetId, ct);
        var replay = new ReplayService(markers, clock);
        var repeated = await replay.BeginAsync(token.AccountId, input.RequestId, input.Intent, async (stored, c) =>
        {
            await EnrollmentAuthority.RequireAsync(repository.Shared, token, clock, "Accountant", c);
            return stored.Target["invoiceId"] == link.InvoiceId && await repository.MatchesAsync(link.InvoiceId, stored.Target["paymentId"], c);
        }, ct);
        if (repeated is not null) return new(await Result(token, link.InvoiceId, repeated.OperationId, ct), true);
        var body = input.Body; var versions = body.GetProperty("expectedVersions");
        AcademicGuard.Version(versions.GetProperty("invoice"), state.Invoice.RowVersion);
        if (state.Registration.Status != "ADMITTED") throw AcademicGuard.Conflict();
        var operation = Guid.NewGuid(); var now = clock.GetUtcNow(); Payment payment;
        if (input.Api == 51)
        {
            AcademicGuard.Version(versions.GetProperty("registration"), state.Registration.RowVersion);
            if (body.GetProperty("expectedSet").GetProperty("registrationId").GetInt32() != link.RegistrationId || state.Invoice.Status != "UNPAID" || state.Payment is not null) throw AcademicGuard.Conflict();
            var amount = decimal.Parse(body.GetProperty("amount").GetString()!, CultureInfo.InvariantCulture);
            if (amount != state.Invoice.Amount) throw ApiErrorException.Validation("amount");
            payment = new Payment { InvoiceId = link.InvoiceId, AmountPaid = state.Invoice.Amount, Method = body.GetProperty("method").GetString()!,
                TransactionReference = body.GetProperty("transactionReference").GetString(), ConfirmedBy = token.AccountId, PaidAt = now.UtcDateTime };
            repository.Add(payment); state.Invoice.Status = "PAID";
            await repository.Shared.SaveAsync(ct);
            await repository.Shared.AuditAsync(AuditEntry.Mandatory(input.Intent.Action.Code, token.AccountId, "Invoice", link.InvoiceId, operation, now,
                new(new JsonObject { ["status"] = "UNPAID" }, new JsonObject { ["status"] = "PAID" }, new JsonObject { ["invoiceId"] = link.InvoiceId, ["paymentId"] = payment.PaymentId })), ct);
            await repository.Shared.AuditAsync(AuditEntry.Mandatory(input.Intent.Action.Code + ".EFFECT", token.AccountId, "Payment", payment.PaymentId, operation, now,
                new(null, Snapshot(payment), new JsonObject { ["invoiceId"] = link.InvoiceId, ["paymentId"] = payment.PaymentId })), ct);
        }
        else
        {
            payment = state.Payment ?? throw AcademicGuard.Conflict();
            AcademicGuard.Version(versions.GetProperty("payment"), payment.RowVersion);
            if (state.Invoice.Status != "PAID" || payment.AmountPaid != state.Invoice.Amount) throw AcademicGuard.Conflict();
            var method = body.GetProperty("method").GetString()!; var reference = body.GetProperty("transactionReference").GetString();
            if (method == payment.Method && reference == payment.TransactionReference) throw ApiErrorException.Validation();
            var before = Metadata(payment); var reason = body.GetProperty("reason").GetString()!;
            payment.Method = method; payment.TransactionReference = reference; payment.LastCorrectedBy = token.AccountId; payment.LastCorrectedAt = now.UtcDateTime; payment.LastCorrectionReason = reason;
            await repository.Shared.SaveAsync(ct);
            await repository.Shared.AuditAsync(AuditEntry.Mandatory(input.Intent.Action.Code, token.AccountId, "Payment", payment.PaymentId, operation, now,
                new(before, Metadata(payment), new JsonObject { ["invoiceId"] = link.InvoiceId, ["paymentId"] = payment.PaymentId }, reason)), ct);
        }
        await replay.CompleteAsync(token.AccountId, input.RequestId, input.Intent, operation, new Dictionary<string, int> { ["invoiceId"] = link.InvoiceId, ["paymentId"] = payment.PaymentId }, ct);
        await new CurrentSessionService(repository.Shared, clock).TouchApprovedInteractiveActivityAsync(token, ct);
        return new(await Result(token, link.InvoiceId, operation, ct), false);
    }
    private async Task<FinanceCommandResult> Result(VerifiedSessionToken token, int invoice, Guid operation, CancellationToken ct)
    {
        var view = await repository.DetailAsync(invoice, clock.GetToday(), ct);
        var authority = await EnrollmentAuthority.RequireAsync(repository.Shared, token, clock, "Accountant", ct);
        if (!clock.IsBefore(authority.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        return new(view, view.Payment ?? throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR), operation);
    }
    private static JsonObject Snapshot(Payment p) => new() { ["invoiceId"] = p.InvoiceId, ["amountPaid"] = p.AmountPaid,
        ["method"] = p.Method, ["transactionReference"] = p.TransactionReference, ["paidAt"] = AcademicGuard.Instant(p.PaidAt) };
    private static JsonObject Metadata(Payment p) => new() { ["method"] = p.Method, ["transactionReference"] = p.TransactionReference };
}
