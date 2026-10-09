namespace EnglishCenter.Api.Infrastructure.Catalogue;

using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure.Auth;
using EnglishCenter.Api.Infrastructure.Auditing;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Catalogue;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public sealed class SqlCourseRepository(SqlAuthDeadlineScope scope) : ICourseRepository
{
    private readonly SqlCurrentSessionRepository current=new(scope);
    public Task<Account?> LockAccountAsync(int id,CancellationToken ct)=>current.LockAccountAsync(id,ct);
    public Task<AuthSession?> LockSessionAsync(byte[] sid,CancellationToken ct)=>current.LockSessionAsync(sid,ct);
    public Task<string?> RoleCodeAsync(int id,CancellationToken ct)=>current.RoleCodeAsync(id,ct);
    public Task SaveSessionAsync(CancellationToken ct)=>current.SaveSessionAsync(ct);
    public Task<Course?> LockCourseAsync(int id,CancellationToken ct)
    {scope.RequireActive();return scope.Context.Courses.FromSqlInterpolated($"SELECT * FROM dbo.Course WITH(UPDLOCK,HOLDLOCK) WHERE course_id={id}").SingleOrDefaultAsync(ct);}
    public Task<bool> CodeExistsAsync(string code,CancellationToken ct)
    {scope.RequireActive();return scope.Context.Courses.FromSqlInterpolated($"SELECT * FROM dbo.Course WITH(UPDLOCK,HOLDLOCK) WHERE course_code={code}").AsNoTracking().AnyAsync(ct);}
    public async Task InsertAsync(Course course,CancellationToken ct)
    {scope.RequireActive();scope.Context.Courses.Add(course);await scope.Context.SaveChangesAsync(ct);}
    public async Task SaveAsync(AuditEntry audit,CancellationToken ct)
    {
        scope.RequireActive();await scope.Context.SaveChangesAsync(ct);
        await new SqlAuditAppender().AppendMandatoryAsync((SqlConnection)scope.Context.Database.GetDbConnection(),
            (SqlTransaction)scope.Context.Database.CurrentTransaction!.GetDbTransaction(),audit,scope.SqlSeconds,ct);
    }
}
