namespace EnglishCenter.Api.Services.Bootstrap;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Validation;
using EnglishCenter.Api.Domain.Identity;

public enum BootstrapOutcome { Created, AlreadyConfigured }

public interface IBootstrapRepository
{
    Task<IReadOnlyList<Role>> LockAndReadRolesAsync(CancellationToken cancellationToken);
    Task InsertRolesAsync(IEnumerable<Role> roles,CancellationToken cancellationToken);
    Task<IReadOnlyList<Account>> ReadAdminsAsync(CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email,CancellationToken cancellationToken);
    Task InsertAdminWithAuditAsync(Account admin,DateTimeOffset occurredAt,CancellationToken cancellationToken);
}

// Explicit operator use only. Repository requires an active caller-owned SERIALIZABLE transaction.
public sealed class BootstrapService(IBootstrapRepository repository,CredentialHasher hasher)
{
    private readonly IBootstrapRepository _repository=repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly CredentialHasher _hasher=hasher ?? throw new ArgumentNullException(nameof(hasher));

    public async Task<BootstrapOutcome> EnsureAdminAsync(string email,string fullName,string password,
        DateTimeOffset occurredAt,CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedEmail=ContactNormalizer.NormalizeEmail(email);
        var normalizedName=NonsecretText.NormalizeRequired(fullName,100);
        var roles=await _repository.LockAndReadRolesAsync(cancellationToken);
        if (roles.Select(r=>r.RoleCode).Distinct(StringComparer.Ordinal).Count()!=roles.Count ||
            roles.Any(r=>!FixedRoleCodes.All.Contains(r.RoleCode,StringComparer.Ordinal))) throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
        var admins=await _repository.ReadAdminsAsync(cancellationToken);
        if (admins.Count>0)
        {
            // Any existing Admin, including disabled/locked, blocks credential/account recreation.
            if (!FixedRoleCodes.MatchesExact(roles.Select(r=>r.RoleCode))) throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
            return BootstrapOutcome.AlreadyConfigured;
        }
        if (await _repository.EmailExistsAsync(normalizedEmail,cancellationToken)) throw new ApiErrorException(ApiErrorCode.CONFLICT);
        // Hash only when creating; repeats never process/reset a supplied credential.
        var hash=_hasher.Hash(password);
        var missing=FixedRoleCodes.All.Where(code=>roles.All(r=>r.RoleCode!=code)).Select(code=>new Role { RoleCode=code,RoleName=code }).ToArray();
        await _repository.InsertRolesAsync(missing,cancellationToken);
        roles=await _repository.LockAndReadRolesAsync(cancellationToken);
        if (!FixedRoleCodes.MatchesExact(roles.Select(r=>r.RoleCode))) throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
        var utc=occurredAt.UtcDateTime;utc=new DateTime(utc.Ticks-utc.Ticks%TimeSpan.TicksPerMillisecond,DateTimeKind.Utc);
        var admin=new Account { RoleId=roles.Single(r=>r.RoleCode=="Admin").RoleId,Email=normalizedEmail,FullName=normalizedName,PasswordHash=hash,Status="ACTIVE",SecurityVersion=1,MustChangePassword=false,CreatedAt=utc };
        await _repository.InsertAdminWithAuditAsync(admin,occurredAt,cancellationToken);
        return BootstrapOutcome.Created;
    }
}
