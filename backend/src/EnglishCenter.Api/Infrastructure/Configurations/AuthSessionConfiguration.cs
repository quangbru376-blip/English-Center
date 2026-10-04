namespace EnglishCenter.Api.Infrastructure.Configurations;

using EnglishCenter.Api.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class AuthSessionConfiguration : IEntityTypeConfiguration<AuthSession>
{
    public void Configure(EntityTypeBuilder<AuthSession> builder)
    {
        builder.ToTable("AuthSession", table =>
        {
            table.HasCheckConstraint("CK_AuthSession_Scope", "[session_scope] COLLATE Latin1_General_100_BIN2 IN ('NORMAL', 'PASSWORD_CHANGE')");
            table.HasCheckConstraint("CK_AuthSession_Lifetime", "[issued_at] <= [last_activity_at] AND [last_activity_at] <= [expires_at] AND [expires_at] > [issued_at]");
            table.HasCheckConstraint("CK_AuthSession_RevokedAt", "[revoked_at] IS NULL OR [revoked_at] >= [issued_at]");
        });
        builder.HasKey(s => s.Sid);
        builder.Property(s => s.Sid).HasColumnName("sid").HasColumnType("binary(32)").HasMaxLength(32).IsFixedLength().IsRequired().ValueGeneratedNever();
        builder.Property(s => s.AccountId).HasColumnName("account_id").HasColumnType("int").IsRequired();
        builder.Property(s => s.IssuedAt).HasColumnName("issued_at").HasColumnType("datetime2(3)").HasPrecision(3).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(s => s.ExpiresAt).HasColumnName("expires_at").HasColumnType("datetime2(3)").HasPrecision(3).IsRequired();
        builder.Property(s => s.LastActivityAt).HasColumnName("last_activity_at").HasColumnType("datetime2(3)").HasPrecision(3).IsRequired();
        builder.Property(s => s.RevokedAt).HasColumnName("revoked_at").HasColumnType("datetime2(3)").HasPrecision(3).IsRequired(false);
        builder.Property(s => s.SecurityVersion).HasColumnName("security_version").HasColumnType("bigint").IsRequired();
        builder.Property(s => s.SessionScope).HasColumnName("session_scope").HasColumnType("varchar(24)").HasMaxLength(24).IsUnicode(false).IsRequired();
        builder.Property(s => s.RowVersion).HasColumnName("row_version").HasColumnType("rowversion").IsRequired().IsRowVersion();
        builder.HasOne(s => s.Account).WithMany().HasForeignKey(s => s.AccountId).OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(s => new { s.AccountId, s.RevokedAt }).HasDatabaseName("IX_AuthSession_Account_Revoked");
        builder.HasIndex(s => s.ExpiresAt).HasDatabaseName("IX_AuthSession_Expires");
    }
}
