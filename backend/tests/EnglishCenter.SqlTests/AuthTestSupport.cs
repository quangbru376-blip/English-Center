namespace EnglishCenter.SqlTests;

using System.Security.Cryptography;
using System.Text.Json;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Services.Identity;
using EnglishCenter.Api.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.IdentityModel.Tokens;

internal static class AuthTestSupport
{
    internal sealed class Clock(DateTimeOffset initial) : TimeProvider
    {
        internal DateTimeOffset Now=initial;
        public override DateTimeOffset GetUtcNow()=>Now;
    }
    internal static CredentialHasher Hasher()=>new(Policy());
    internal static PasswordPolicy Policy()=>new(OfflinePasswordBlocklist.Load(Path.Combine(AppContext.BaseDirectory,"Resources","PasswordBlocklist")));
    internal static string Password()=>new TemporaryPasswordGenerator(Policy()).Generate();
    internal static JwtSessionIssuer Signer()=>new(new SigningCredentials(new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32)),SecurityAlgorithms.HmacSha256),"fixture-issuer","fixture-audience");
    internal static StudentSignupInput Signup(string password,string email="signup+tag@example.invalid",string phone="+84 912 345 678",string birth="2000-01-01")=>StudentSignupInput.Parse(JsonSerializer.Serialize(new{fullName="Synthetic student",dateOfBirth=birth,email,phone,password,address=(string?)null}));
    internal static LoginInput Login(string password,string email="fixture@example.invalid")=>LoginInput.Parse(JsonSerializer.Serialize(new{email,password}));
    internal static async Task StudentRole(SqlFixture f)
    {
        f.Context.Roles.Add(new Role { RoleCode="Student",RoleName="Student" });await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
    internal static async Task<string> Credential(SqlFixture f)
    {
        var raw=Password();f.Teacher.PasswordHash=Hasher().Hash(raw);await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);return raw;
    }
    internal static async Task EmptyAfterRollback(SqlFixture f)
    {
        await f.Context.Database.CurrentTransaction!.RollbackAsync(TestContext.Current.CancellationToken);f.Context.ChangeTracker.Clear();
        Assert.Equal(0,await f.Context.Accounts.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0,await f.Context.StudentProfiles.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0,await f.Context.AuthSessions.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0,await f.Context.AuditLogs.CountAsync(TestContext.Current.CancellationToken));
    }
}
