namespace EnglishCenter.Api.Infrastructure.Configurations;

using EnglishCenter.Api.Domain.Admissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class EntryTestSessionConfiguration : IEntityTypeConfiguration<EntryTestSession>
{
    public void Configure(EntityTypeBuilder<EntryTestSession> builder)
    {
        builder.ToTable("EntryTestSession", t =>
        {
            t.HasCheckConstraint(
                "CK_EntryTestSession_Status",
                "[status] COLLATE Latin1_General_100_BIN2 IN ('OPEN', 'CLOSED', 'CANCELLED') AND DATALENGTH([status]) = LEN([status])");

            t.HasCheckConstraint(
                "CK_EntryTestSession_Capacity",
                "[capacity] BETWEEN 1 AND 20");

            t.HasCheckConstraint(
                "CK_EntryTestSession_Time",
                "[start_at] < [end_at] AND DATEPART(second, [start_at]) = 0 AND DATEPART(millisecond, [start_at]) = 0 AND DATEPART(second, [end_at]) = 0 AND DATEPART(millisecond, [end_at]) = 0 AND CAST(DATEADD(minute, 420, [start_at]) AS date) = CAST(DATEADD(minute, 420, [end_at]) AS date)");

            t.HasCheckConstraint(
                "CK_EntryTestSession_Room",
                "DATALENGTH([room]) BETWEEN 1 AND 30 AND DATALENGTH([room]) = LEN([room]) AND [room] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Z0-9_-]%'");

            t.HasCheckConstraint(
                "CK_EntryTestSession_CancelledBundle",
                "([status] = 'CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancellation_reason] IS NOT NULL AND LEN([cancellation_reason]) > 0 AND [cancellation_operation_id] IS NOT NULL) OR ([status] <> 'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancellation_operation_id] IS NULL)");
        });

        builder.HasKey(s => s.SessionId);

        builder.Property(s => s.SessionId)
            .HasColumnName("session_id")
            .HasColumnType("int")
            .UseIdentityColumn(1, 1);

        builder.Property(s => s.StartAt)
            .HasColumnName("start_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired();

        builder.Property(s => s.EndAt)
            .HasColumnName("end_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired();

        builder.Property(s => s.Room)
            .HasColumnName("room")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(s => s.Capacity)
            .HasColumnName("capacity")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(s => s.GraderId)
            .HasColumnName("grader_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(16)")
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired()
            .HasDefaultValue("CLOSED")
            .HasSentinel(string.Empty);

        builder.Property(s => s.CancelledBy)
            .HasColumnName("cancelled_by")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(s => s.CancelledAt)
            .HasColumnName("cancelled_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired(false);

        builder.Property(s => s.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasColumnType("nvarchar(2000)")
            .HasMaxLength(2000)
            .IsUnicode(true)
            .IsRequired(false);

        builder.Property(s => s.CancellationOperationId)
            .HasColumnName("cancellation_operation_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired(false);

        builder.Property(s => s.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("rowversion")
            .IsRowVersion()
            .IsRequired();

        builder.HasOne(s => s.Grader)
            .WithMany()
            .HasForeignKey(s => s.GraderId)
            .HasConstraintName("FK_EntryTestSession_Account_grader_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(s => s.CancelledByAccount)
            .WithMany()
            .HasForeignKey(s => s.CancelledBy)
            .HasConstraintName("FK_EntryTestSession_Account_cancelled_by")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasIndex(s => new { s.GraderId, s.StartAt, s.EndAt, s.SessionId }, "IX_Session_GraderStart")
            .HasDatabaseName("IX_Session_GraderStart");

        builder.HasIndex(s => new { s.Room, s.StartAt, s.EndAt, s.SessionId }, "IX_Session_RoomStart")
            .HasDatabaseName("IX_Session_RoomStart");

        builder.HasIndex(s => s.CancelledBy, "IX_Session_CancelledBy")
            .HasDatabaseName("IX_Session_CancelledBy");
    }
}
