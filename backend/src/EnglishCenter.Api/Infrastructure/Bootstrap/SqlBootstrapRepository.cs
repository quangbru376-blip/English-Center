namespace EnglishCenter.Api.Infrastructure.Bootstrap;

using System.Data;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure.Auditing;
using EnglishCenter.Api.Services.Bootstrap;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public sealed class SqlBootstrapRepository : IBootstrapRepository
{
    private readonly EnglishCenterDbContext _context;
    private readonly int _sqlSeconds;
    private readonly int _lockMs;
    public SqlBootstrapRepository(EnglishCenterDbContext context,int sqlSeconds,int lockMs)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (sqlSeconds<=0 || lockMs<=0 || lockMs>=(long)sqlSeconds*1000) throw new ArgumentException("Invalid bootstrap deadline profile.");
        _context=context;_sqlSeconds=sqlSeconds;_lockMs=lockMs;
        _context.Database.SetCommandTimeout(sqlSeconds);
    }

    public async Task<IReadOnlyList<Role>> LockAndReadRolesAsync(CancellationToken cancellationToken)
    {
        var tx=_context.Database.CurrentTransaction?.GetDbTransaction();
        if (tx is null || tx.IsolationLevel!=IsolationLevel.Serializable) throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
        await using var command=_context.Database.GetDbConnection().CreateCommand();command.Transaction=tx;command.CommandTimeout=_sqlSeconds;
        command.CommandText="IF ISNULL(HAS_PERMS_BY_NAME('dbo.Role','OBJECT','INSERT'),0)<>1 OR ISNULL(HAS_PERMS_BY_NAME('dbo.Account','OBJECT','INSERT'),0)<>1 THROW 51902,'Bootstrap operator required',1; DECLARE @result int; EXEC @result=sys.sp_getapplock @Resource='EC:ADMINSET',@LockMode='Exclusive',@LockOwner='Transaction',@DbPrincipal='public',@LockTimeout=@timeout; IF @result<0 THROW 51903,'Bootstrap gate unavailable',1;";
        command.Parameters.Add(new SqlParameter("@timeout",SqlDbType.Int){Value=_lockMs});
        try { await command.ExecuteNonQueryAsync(cancellationToken); }
        catch(SqlException e) when(e.Number==51902) { throw new ApiErrorException(ApiErrorCode.FORBIDDEN); }
        catch(SqlException e) when(e.Number==51903) { throw new ApiErrorException(ApiErrorCode.CONFLICT); }
        return await _context.Roles.FromSqlRaw("SELECT * FROM dbo.Role WITH(UPDLOCK,HOLDLOCK)").AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task InsertRolesAsync(IEnumerable<Role> roles,CancellationToken cancellationToken)
    {
        _context.Roles.AddRange(roles);await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<Account>> ReadAdminsAsync(CancellationToken cancellationToken) =>
        await _context.Accounts.FromSqlRaw("SELECT a.* FROM dbo.Account a WITH(UPDLOCK,HOLDLOCK) INNER JOIN dbo.Role r ON r.role_id=a.role_id WHERE r.role_code='Admin'").AsNoTracking().ToListAsync(cancellationToken);
    public Task<bool> EmailExistsAsync(string email,CancellationToken cancellationToken) => _context.Accounts.AnyAsync(a=>a.Email==email,cancellationToken);
    public async Task InsertAdminWithAuditAsync(Account admin,DateTimeOffset occurredAt,CancellationToken cancellationToken)
    {
        _context.Accounts.Add(admin);await _context.SaveChangesAsync(cancellationToken);
        var entry=AuditEntry.InitialAdmin(admin.AccountId,admin.RoleId,Guid.NewGuid(),occurredAt);
        await new SqlAuditAppender().AppendMandatoryAsync((SqlConnection)_context.Database.GetDbConnection(),
            (SqlTransaction)_context.Database.CurrentTransaction!.GetDbTransaction(),entry,_sqlSeconds,cancellationToken);
    }
}
