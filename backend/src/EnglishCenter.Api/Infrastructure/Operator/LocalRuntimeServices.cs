using System.Net;
using EnglishCenter.Api.Api.Administration;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Administration;
using EnglishCenter.Api.Infrastructure.Auditing;
using EnglishCenter.Api.Infrastructure.Auth;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Api.Academics;
using EnglishCenter.Api.Infrastructure.Academics;
using EnglishCenter.Api.Api.Catalogue;
using EnglishCenter.Api.Infrastructure.Catalogue;
using EnglishCenter.Api.Infrastructure.Readiness;
using EnglishCenter.Api.Api.Enrollment;
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Api.Admissions;
using EnglishCenter.Api.Infrastructure.Admissions;
using EnglishCenter.Api.Api.StudentInvoices;
using EnglishCenter.Api.Infrastructure.StudentInvoices;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Api.Infrastructure.Operator;

internal sealed class LocalRuntimeServices : ITrustedLoginIpResolver, IDisposable
{
    private readonly BusinessClock clock = new(TimeProvider.System);
    private readonly LoginIpLimiter limiter = new();
    private readonly SecurityAuditRecorder audit;
    private readonly SqlStudentSignupExecutor signup;
    private readonly SqlLoginExecutor login;
    private readonly SqlCurrentSessionReader current;
    private readonly SqlCurrentSessionLogout logout;
    private readonly SqlPasswordChangeExecutor password;
    private readonly SqlAdminAccountExecutor admin;
    private readonly SqlStaffAccountExecutor staff;
    private readonly SqlCourseExecutor courses;
    private readonly SqlProgramExecutor programs;
    private readonly SqlEntrySessionExecutor entrySessions;
    private readonly SqlOwnProfileExecutor profiles;
    private readonly SqlStudentEnrollmentExecutor studentEnrollment;
    private readonly SqlStudentEnrollmentReader studentReads;
    private readonly SqlTeacherEntryExecutor teacherEntry;
    private readonly SqlTeacherEntryReader teacherReads;
    private readonly SqlAdmissionsExecutor admissions;
    private readonly SqlStudentInvoiceReader studentInvoices;
    public LiveCsrfIdentityResolver Identity { get; }
    public LocalRuntimeServices(RuntimeProfile profile, LocalRuntimeKeyStore keys, string corpusDirectory)
    {
        var hasher = new CredentialHasher(new PasswordPolicy(OfflinePasswordBlocklist.Load(corpusDirectory)));
        var limits = profile.Execution!;
        audit = new(new SqlSecurityAuditStore(() => new SqlConnection(LocalRuntimeReferences.SqlConnection), limits.SqlSeconds), new AuditIncidentCounter());
        var issuer = new JwtSessionIssuer(keys.Jwt.Signing, keys.Jwt.Issuer, keys.Jwt.Audience);
        var validator = new JwtSessionValidator(keys.Jwt.ValidationKeys.Select(value => value.Key),
            keys.Jwt.ValidationKeys.Select(value => value.Algorithm), keys.Jwt.Issuer, keys.Jwt.Audience, clock, 4096);
        signup = new(Context, hasher, clock, limits.SqlSeconds, limits.LockMs);
        login = new(Context, hasher, clock, issuer, audit, limits.SqlSeconds, limits.LockMs);
        current = new(Context, clock, limits.SqlSeconds, limits.LockMs);
        logout = new(Context, clock, audit, limits.SqlSeconds, limits.LockMs);
        password = new(Context, hasher, clock, limits.SqlSeconds, limits.LockMs);
        admin = new(Context, clock, limits.SqlSeconds, limits.LockMs);
        staff = new(Context, clock, hasher, new TemporaryPasswordGenerator(new PasswordPolicy(OfflinePasswordBlocklist.Load(corpusDirectory))), limits.SqlSeconds, limits.LockMs);
        courses = new(Context, clock, limits.SqlSeconds, limits.LockMs);
        programs = new(Context, clock, limits.SqlSeconds, limits.LockMs);
        entrySessions = new(Context, clock, limits.SqlSeconds, limits.LockMs);
        profiles = new(Context, clock, limits.SqlSeconds, limits.LockMs);
        studentEnrollment = new(Context, clock, limits.SqlSeconds, limits.LockMs);
        studentReads = new(Context, clock, limits.SqlSeconds, limits.LockMs);
        teacherEntry = new(Context, clock, limits.SqlSeconds, limits.LockMs);
        teacherReads = new(Context, clock, limits.SqlSeconds, limits.LockMs);
        admissions = new(Context, clock, limits.SqlSeconds, limits.LockMs);
        studentInvoices = new(Context, clock, limits.SqlSeconds, limits.LockMs);
        Identity = new(validator, current, audit, clock);
    }
    private static EnglishCenterDbContext Context() => new(new DbContextOptionsBuilder<EnglishCenterDbContext>()
        .UseSqlServer(LocalRuntimeReferences.SqlConnection, options => options.UseCompatibilityLevel(160)).Options);
    public static void RegisterCsrf(IServiceCollection services)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        var keys = DataProtectionProvider.Create(new DirectoryInfo(Path.Combine(LocalRuntimeReferences.RuntimeRoot, "csrf")), options =>
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
            options.SetApplicationName("EnglishCenter.M0.Local").ProtectKeysWithDpapi(protectToLocalMachine: false).DisableAutomaticKeyGeneration();
        });
        services.AddCsrfBoundary(keys);
    }
    public void Map(IEndpointRouteBuilder routes, int bodyBytes)
    {
        routes.MapCsrfContext(); routes.MapStudentSignup(signup, bodyBytes);
        routes.MapLogin(login, limiter, this, audit, clock, bodyBytes);
        routes.MapCurrentSession(current, clock); routes.MapCurrentSessionLogout(logout);
        routes.MapPasswordChange(password, bodyBytes); routes.MapAdminAccountPilot(admin, bodyBytes);
        routes.MapStaffAccounts(staff, bodyBytes); routes.MapCourseCatalogue(courses, bodyBytes);
        routes.MapTrainingPrograms(programs,bodyBytes);
        routes.MapEntrySessions(entrySessions,bodyBytes,admissions);
        routes.MapStudentEnrollment(profiles, studentReads, studentEnrollment, bodyBytes);
        routes.MapTeacherEntry(teacherReads, teacherEntry, bodyBytes);
        routes.MapAdmissions(admissions, bodyBytes);
        routes.MapStudentInvoices(studentInvoices);
    }
    public IPAddress? Resolve(HttpContext context) => context.Connection.RemoteIpAddress is { } address && IPAddress.IsLoopback(address) ? address : null;
    public void Dispose() => limiter.Dispose();
}
