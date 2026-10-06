namespace EnglishCenter.Api.Services.Identity;

using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Common.Validation;
using EnglishCenter.Api.Domain.Identity;

public interface IStudentSignupRepository
{
    Task<int> StudentRoleAsync(CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
    Task<bool> PhoneExistsAsync(string phone, CancellationToken cancellationToken);
    Task InsertWithAuditAsync(Account account, StudentProfile profile, DateTimeOffset at, CancellationToken cancellationToken);
}
public interface IStudentSignupExecutor
{
    // Returns only after the operation's transaction successfully commits.
    Task<RegisterStudentResult> ExecuteAsync(StudentSignupInput input, CancellationToken cancellationToken);
}
public sealed class StudentSignupService(IStudentSignupRepository repository, CredentialHasher hasher, BusinessClock clock)
{
    public async Task<RegisterStudentResult> RegisterAsync(StudentSignupInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input); cancellationToken.ThrowIfCancellationRequested();
        ValidateBirth(input.DateOfBirth); var hash = hasher.Hash(input.Password);
        var roleId = await repository.StudentRoleAsync(cancellationToken);
        if (await repository.EmailExistsAsync(input.Email, cancellationToken)) throw new ApiErrorException(ApiErrorCode.CONFLICT, "email");
        if (await repository.PhoneExistsAsync(input.Phone, cancellationToken)) throw new ApiErrorException(ApiErrorCode.CONFLICT, "phone");
        ValidateBirth(input.DateOfBirth); var now = clock.GetUtcNow();
        var account = new Account { RoleId = roleId, Email = input.Email, Phone = input.Phone, FullName = input.FullName, PasswordHash = hash, Status = "ACTIVE", SecurityVersion = 1, CreatedAt = now.UtcDateTime, MustChangePassword = false };
        var profile = new StudentProfile { Account = account, DateOfBirth = input.DateOfBirth, Address = input.Address };
        await repository.InsertWithAuditAsync(account, profile, now, cancellationToken);
        return RegisterStudentResult.Created();
    }
    private void ValidateBirth(DateOnly birth)
    {
        try { new StudentBirthDateValidator(clock).Validate(birth); }
        catch (ArgumentException) { throw ApiErrorException.Validation("dateOfBirth"); }
    }
}
