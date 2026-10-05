namespace EnglishCenter.Api.Infrastructure.Configurations;

using EnglishCenter.Api.Domain.Admissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class EntryTestAttemptConfiguration : IEntityTypeConfiguration<EntryTestAttempt>
{
    public void Configure(EntityTypeBuilder<EntryTestAttempt> builder)
    {
        builder.ToTable("EntryTestAttempt", "dbo", t =>
        {
            t.HasCheckConstraint(
                "CK_EntryTestAttempt_Number",
                "[attempt_number] IN (1, 2)");

            t.HasCheckConstraint(
                "CK_EntryTestAttempt_Score",
                "[score] IS NULL OR ([score] >= 0 AND [score] <= 100)");

            t.HasCheckConstraint(
                "CK_EntryTestAttempt_ResultBundle",
                "([score] IS NULL AND [graded_by] IS NULL AND [graded_at] IS NULL AND [completed_booking_id] IS NULL) OR ([score] IS NOT NULL AND [graded_by] IS NOT NULL AND [graded_at] IS NOT NULL AND [completed_booking_id] IS NOT NULL)");
        });

        builder.HasKey(a => a.AttemptId);

        builder.Property(a => a.AttemptId)
            .HasColumnName("attempt_id")
            .HasColumnType("int")
            .UseIdentityColumn(1, 1);

        builder.Property(a => a.RegistrationId)
            .HasColumnName("registration_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(a => a.AttemptNumber)
            .HasColumnName("attempt_number")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(a => a.Score)
            .HasColumnName("score")
            .HasColumnType("decimal(5,2)")
            .HasPrecision(5, 2)
            .IsRequired(false);

        builder.Property(a => a.GradedBy)
            .HasColumnName("graded_by")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(a => a.GradedAt)
            .HasColumnName("graded_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired(false);

        builder.Property(a => a.CompletedBookingId)
            .HasColumnName("completed_booking_id")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(a => a.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("rowversion")
            .IsRowVersion()
            .IsRequired();

        builder.HasAlternateKey(a => new { a.AttemptId, a.RegistrationId })
            .HasName("AK_Attempt_IdRegistration");

        builder.HasIndex(a => new { a.RegistrationId, a.AttemptNumber })
            .IsUnique()
            .HasDatabaseName("UQ_Attempt_RegistrationNumber");

        builder.HasIndex(a => a.CompletedBookingId)
            .IsUnique()
            .HasDatabaseName("UQ_Attempt_CompletedBooking")
            .HasFilter("[completed_booking_id] IS NOT NULL");

        builder.HasIndex(a => a.GradedBy)
            .HasDatabaseName("IX_Attempt_GradedBy");

        builder.HasOne(a => a.Registration)
            .WithMany()
            .HasForeignKey(a => a.RegistrationId)
            .IsRequired()
            .HasConstraintName("FK_Attempt_Registration_registration_id")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(a => a.GradedByAccount)
            .WithMany()
            .HasForeignKey(a => a.GradedBy)
            .IsRequired(false)
            .HasConstraintName("FK_Attempt_Account_graded_by")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(a => a.CompletedBooking)
            .WithMany()
            .HasForeignKey(a => new { a.CompletedBookingId, a.AttemptId })
            .HasPrincipalKey(b => new { b.BookingId, b.AttemptId })
            .IsRequired(false)
            .HasConstraintName("FK_Attempt_Booking_CompletedBookingAttempt")
            .OnDelete(DeleteBehavior.NoAction);

    }
}
