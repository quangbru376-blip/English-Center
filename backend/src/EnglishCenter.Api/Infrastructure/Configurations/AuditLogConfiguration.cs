using EnglishCenter.Api.Domain.Administration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishCenter.Api.Infrastructure.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLog", t =>
        {
            t.HasCheckConstraint(
                "CK_AuditLog_Result",
                "[result] COLLATE Latin1_General_100_BIN2 IN ('SUCCESS', 'FAILURE') AND DATALENGTH([result]) = LEN([result])");

            t.HasCheckConstraint(
                "CK_AuditLog_Category",
                "[audit_category] COLLATE Latin1_General_100_BIN2 IN ('MANDATORY', 'SECURITY') AND DATALENGTH([audit_category]) = LEN([audit_category])");

            t.HasCheckConstraint(
                "CK_AuditLog_Details",
                "[details] IS NULL OR ISJSON([details]) = 1");

            t.HasCheckConstraint(
                "CK_AuditLog_SecurityExpiry",
                "([audit_category] = 'SECURITY' AND [security_expires_at] IS NOT NULL AND [security_expires_at] >= DATEADD(day, 90, [occurred_at])) OR ([audit_category] = 'MANDATORY' AND [security_expires_at] IS NULL)");

            t.HasCheckConstraint(
                "CK_AuditLog_IPBundle",
                "([ip_address] IS NULL AND [ip_expires_at] IS NULL) OR ([ip_address] IS NOT NULL AND [ip_expires_at] IS NOT NULL AND [ip_expires_at] = DATEADD(day, 90, [occurred_at]))");
        });

        builder.HasKey(x => x.AuditId);

        builder.Property(x => x.AuditId)
            .HasColumnName("audit_id")
            .HasColumnType("int")
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(x => x.ActorAccountId)
            .HasColumnName("actor_account_id")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(x => x.Action)
            .HasColumnName("action")
            .HasColumnType("varchar(80)")
            .HasMaxLength(80)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.EntityType)
            .HasColumnName("entity_type")
            .HasColumnType("varchar(64)")
            .HasMaxLength(64)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.EntityId)
            .HasColumnName("entity_id")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(x => x.Result)
            .HasColumnName("result")
            .HasColumnType("varchar(16)")
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.Details)
            .HasColumnName("details")
            .HasColumnType("nvarchar(max)")
            .IsUnicode(true)
            .IsRequired(false);

        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired();

        builder.Property(x => x.IpAddress)
            .HasColumnName("ip_address")
            .HasColumnType("varchar(45)")
            .HasMaxLength(45)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(x => x.OperationId)
            .HasColumnName("operation_id")
            .HasColumnType("uniqueidentifier")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.AuditCategory)
            .HasColumnName("audit_category")
            .HasColumnType("varchar(16)")
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.SecurityExpiresAt)
            .HasColumnName("security_expires_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired(false);

        builder.Property(x => x.IpExpiresAt)
            .HasColumnName("ip_expires_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired(false);

        builder.HasOne(x => x.ActorAccount)
            .WithMany()
            .HasForeignKey(x => x.ActorAccountId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_AuditLog_Account_actor_account_id");

        builder.HasIndex(x => new { x.OccurredAt, x.AuditId })
            .HasDatabaseName("IX_Audit_Time")
            .IsDescending(true, true);

        builder.HasIndex(x => new { x.OperationId, x.AuditId })
            .HasDatabaseName("IX_Audit_Operation")
            .IsDescending(false, false);

        builder.HasIndex(x => new { x.ActorAccountId, x.OccurredAt, x.AuditId })
            .HasDatabaseName("IX_Audit_ActorTime")
            .IsDescending(false, true, true);

        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.OccurredAt, x.AuditId })
            .HasDatabaseName("IX_Audit_EntityTime")
            .IsDescending(false, false, true, true);
    }
}
