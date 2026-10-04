namespace EnglishCenter.Api.Infrastructure;

using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

public class EnglishCenterDbContext : DbContext
{
    public EnglishCenterDbContext(DbContextOptions<EnglishCenterDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("dbo");
        modelBuilder.UseCollation("Latin1_General_100_CI_AS_SC");

        modelBuilder.ApplyConfiguration(new RoleConfiguration());

        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new StudentProfileConfiguration());
        modelBuilder.ApplyConfiguration(new AuthSessionConfiguration());
    }
}
