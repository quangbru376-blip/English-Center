namespace EnglishCenter.Api.Infrastructure.Configurations;

using EnglishCenter.Api.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Account", table =>
        {
            table.HasCheckConstraint("CK_Account_Status", "[status] COLLATE Latin1_General_100_BIN2 IN ('ACTIVE', 'LOCKED', 'DISABLED')");
            table.HasCheckConstraint("CK_Account_SecurityVersion", "[security_version] > 0");
            table.HasCheckConstraint("CK_Account_FailedLoginCount", "[failed_login_count] >= 0");
            table.HasCheckConstraint("CK_Account_TempBundle",
                "([must_change_password] = 1 AND [temp_issued_at] IS NOT NULL AND [temp_expires_at] IS NOT NULL AND [temp_expires_at] = DATEADD(hour, 24, [temp_issued_at])) OR ([must_change_password] = 0 AND [temp_issued_at] IS NULL AND [temp_expires_at] IS NULL)");
            table.HasCheckConstraint("CK_Account_Phone",
                "[phone] IS NULL OR (DATALENGTH([phone]) = 10 AND LEFT([phone], 1) = '0' AND [phone] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^0-9]%')");
        });
        builder.HasKey(a => a.AccountId);
        builder.Property(a => a.AccountId).HasColumnName("account_id").HasColumnType("int").UseIdentityColumn();
        builder.Property(a => a.RoleId).HasColumnName("role_id").HasColumnType("int").IsRequired();
        builder.Property(a => a.Email).HasColumnName("email").HasColumnType("varchar(254)").HasMaxLength(254)
            .IsUnicode(false).UseCollation("Latin1_General_100_CI_AS_SC").IsRequired();
        builder.Property(a => a.Phone).HasColumnName("phone").HasColumnType("varchar(10)").HasMaxLength(10).IsUnicode(false).IsRequired(false);
        builder.Property(a => a.PasswordHash).HasColumnName("password_hash").HasColumnType("varchar(512)").HasMaxLength(512).IsUnicode(false).IsRequired();
        builder.Property(a => a.FullName).HasColumnName("full_name").HasColumnType("nvarchar(200)").HasMaxLength(200).IsUnicode().IsRequired();
        builder.Property(a => a.Status).HasColumnName("status").HasColumnType("varchar(16)").HasMaxLength(16)
            .IsUnicode(false).IsRequired().HasDefaultValue("ACTIVE").HasSentinel(string.Empty);
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2(3)").HasPrecision(3).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(a => a.SecurityVersion).HasColumnName("security_version").HasColumnType("bigint").IsRequired().HasDefaultValue(1L);
        builder.Property(a => a.FailedLoginCount).HasColumnName("failed_login_count").HasColumnType("int").IsRequired().HasDefaultValue(0);
        builder.Property(a => a.FailureWindowStartedAt).HasColumnName("failure_window_started_at").HasColumnType("datetime2(3)").HasPrecision(3).IsRequired(false);
        builder.Property(a => a.LoginThrottledUntil).HasColumnName("login_throttled_until").HasColumnType("datetime2(3)").HasPrecision(3).IsRequired(false);
        builder.Property(a => a.MustChangePassword).HasColumnName("must_change_password").HasColumnType("bit").IsRequired();
        builder.Property(a => a.TempIssuedAt).HasColumnName("temp_issued_at").HasColumnType("datetime2(3)").HasPrecision(3).IsRequired(false);
        builder.Property(a => a.TempExpiresAt).HasColumnName("temp_expires_at").HasColumnType("datetime2(3)").HasPrecision(3).IsRequired(false);
        builder.Property(a => a.RowVersion).HasColumnName("row_version").HasColumnType("rowversion").IsRequired().IsRowVersion();
        builder.HasOne(a => a.Role).WithMany().HasForeignKey(a => a.RoleId).OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(a => a.Email).IsUnique().HasDatabaseName("UQ_Account_Email");
        builder.HasIndex(a => a.Phone).IsUnique().HasFilter("[phone] IS NOT NULL").HasDatabaseName("UQ_Account_Phone");
        builder.HasIndex(a => new { a.RoleId, a.Status, a.AccountId }).HasDatabaseName("IX_Account_Role_Status_Id");
    }
}
