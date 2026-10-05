using EnglishCenter.Api.Domain.Administration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishCenter.Api.Infrastructure.Configurations;

public sealed class ReplayOperationConfiguration : IEntityTypeConfiguration<ReplayOperation>
{
    public void Configure(EntityTypeBuilder<ReplayOperation> builder)
    {
        builder.ToTable("ReplayOperation", t =>
        {
            t.HasCheckConstraint(
                "CK_ReplayOperation_Mode",
                "[result_mode] COLLATE Latin1_General_100_BIN2 IN ('SAFE_REFERENCE', 'SECRET_RESULT_UNAVAILABLE') AND DATALENGTH([result_mode]) = LEN([result_mode])");

            t.HasCheckConstraint(
                "CK_ReplayOperation_ResultReference",
                "ISJSON([result_reference]) = 1");

            t.HasCheckConstraint(
                "CK_ReplayOperation_Expiry",
                "[expires_at] >= DATEADD(hour, 24, [completed_at])");
        });

        builder.HasKey(x => x.ReplayId);

        builder.Property(x => x.ReplayId)
            .HasColumnName("replay_id")
            .HasColumnType("int")
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(x => x.ActorAccountId)
            .HasColumnName("actor_account_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.ActionCode)
            .HasColumnName("action_code")
            .HasColumnType("varchar(80)")
            .HasMaxLength(80)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.RequestId)
            .HasColumnName("request_id")
            .HasColumnType("uniqueidentifier")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.InputDigest)
            .HasColumnName("input_digest")
            .HasColumnType("binary(32)")
            .HasMaxLength(32)
            .IsFixedLength()
            .IsRequired();

        builder.Property(x => x.OperationId)
            .HasColumnName("operation_id")
            .HasColumnType("uniqueidentifier")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.ResultMode)
            .HasColumnName("result_mode")
            .HasColumnType("varchar(32)")
            .HasMaxLength(32)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.ResultReference)
            .HasColumnName("result_reference")
            .HasColumnType("nvarchar(max)")
            .IsUnicode(true)
            .IsRequired();

        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired();

        builder.HasOne(x => x.ActorAccount)
            .WithMany()
            .HasForeignKey(x => x.ActorAccountId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_ReplayOperation_Account_actor_account_id");

        builder.HasIndex(x => new { x.ActorAccountId, x.ActionCode, x.RequestId })
            .HasDatabaseName("UQ_Replay_ActorActionRequest")
            .IsUnique();

        builder.HasIndex(x => x.ExpiresAt)
            .HasDatabaseName("IX_Replay_Expiry");

        builder.HasIndex(x => x.OperationId)
            .HasDatabaseName("IX_Replay_Operation");
    }
}
