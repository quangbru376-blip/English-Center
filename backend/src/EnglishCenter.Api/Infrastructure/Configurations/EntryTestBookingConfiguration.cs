namespace EnglishCenter.Api.Infrastructure.Configurations;

using EnglishCenter.Api.Domain.Admissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class EntryTestBookingConfiguration : IEntityTypeConfiguration<EntryTestBooking>
{
    public void Configure(EntityTypeBuilder<EntryTestBooking> builder)
    {
        builder.ToTable("EntryTestBooking", "dbo", t =>
        {
            t.HasCheckConstraint(
                "CK_EntryTestBooking_Status",
                "[status] COLLATE Latin1_General_100_BIN2 IN ('SCHEDULED', 'COMPLETED', 'ABSENT', 'CANCELLED') AND DATALENGTH([status]) = LEN([status])");

            t.HasCheckConstraint(
                "CK_EntryTestBooking_AbsenceBundle",
                "([status] = 'ABSENT' AND [absent_by] IS NOT NULL AND [absent_at] IS NOT NULL AND [absent_operation_id] IS NOT NULL) OR ([status] <> 'ABSENT' AND [absent_by] IS NULL AND [absent_at] IS NULL AND [absent_operation_id] IS NULL)");

            t.HasCheckConstraint(
                "CK_EntryTestBooking_CancellationBundle",
                "([status] = 'CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancelled_from_status] IS NOT NULL AND [cancelled_from_status] COLLATE Latin1_General_100_BIN2 = 'SCHEDULED' AND DATALENGTH([cancelled_from_status]) = LEN([cancelled_from_status]) AND [cancellation_operation_id] IS NOT NULL) OR ([status] <> 'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancelled_from_status] IS NULL AND [cancellation_reason] IS NULL AND [cancellation_operation_id] IS NULL)");
        });

        builder.HasKey(b => b.BookingId);

        builder.Property(b => b.BookingId)
            .HasColumnName("booking_id")
            .HasColumnType("int")
            .UseIdentityColumn(1, 1);

        builder.Property(b => b.AttemptId)
            .HasColumnName("attempt_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(b => b.SessionId)
            .HasColumnName("session_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(b => b.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(16)")
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired()
            .HasDefaultValue("SCHEDULED")
            .HasSentinel(string.Empty);

        builder.Property(b => b.BookedAt)
            .HasColumnName("booked_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(b => b.AbsentBy)
            .HasColumnName("absent_by")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(b => b.AbsentAt)
            .HasColumnName("absent_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired(false);

        builder.Property(b => b.AbsentOperationId)
            .HasColumnName("absent_operation_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired(false);

        builder.Property(b => b.CancelledBy)
            .HasColumnName("cancelled_by")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(b => b.CancelledAt)
            .HasColumnName("cancelled_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired(false);

        builder.Property(b => b.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasColumnType("nvarchar(2000)")
            .HasMaxLength(2000)
            .IsUnicode(true)
            .IsRequired(false);

        builder.Property(b => b.CancelledFromStatus)
            .HasColumnName("cancelled_from_status")
            .HasColumnType("varchar(16)")
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(b => b.CancellationOperationId)
            .HasColumnName("cancellation_operation_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired(false);

        builder.Property(b => b.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("rowversion")
            .IsRowVersion()
            .IsRequired();

        builder.HasAlternateKey(b => new { b.BookingId, b.AttemptId })
            .HasName("AK_Booking_IdAttempt");

        builder.HasIndex(b => b.AttemptId, "UQ_Booking_Current")
            .IsUnique()
            .HasDatabaseName("UQ_Booking_Current")
            .HasFilter("[status] = 'SCHEDULED'");

        builder.HasIndex(b => b.AttemptId, "UQ_Booking_Completed")
            .IsUnique()
            .HasDatabaseName("UQ_Booking_Completed")
            .HasFilter("[status] = 'COMPLETED'");

        builder.HasIndex(b => new { b.AttemptId, b.BookedAt, b.BookingId })
            .HasDatabaseName("IX_Booking_AttemptHistory")
            .IsDescending(false, true, true);

        builder.HasIndex(b => new { b.SessionId, b.Status, b.BookingId })
            .HasDatabaseName("IX_Booking_SessionState");

        builder.HasIndex(b => b.AbsentBy)
            .HasDatabaseName("IX_Booking_AbsentBy");

        builder.HasIndex(b => b.CancelledBy)
            .HasDatabaseName("IX_Booking_CancelledBy");

        builder.HasOne(b => b.Attempt)
            .WithMany()
            .HasForeignKey(b => b.AttemptId)
            .IsRequired()
            .HasConstraintName("FK_Booking_Attempt_attempt_id")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(b => b.Session)
            .WithMany()
            .HasForeignKey(b => b.SessionId)
            .IsRequired()
            .HasConstraintName("FK_Booking_Session_session_id")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(b => b.AbsentByAccount)
            .WithMany()
            .HasForeignKey(b => b.AbsentBy)
            .IsRequired(false)
            .HasConstraintName("FK_Booking_Account_absent_by")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(b => b.CancelledByAccount)
            .WithMany()
            .HasForeignKey(b => b.CancelledBy)
            .IsRequired(false)
            .HasConstraintName("FK_Booking_Account_cancelled_by")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
