namespace EnglishCenter.Api.Infrastructure.Configurations;

using EnglishCenter.Api.Domain.Admissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class RegistrationConfiguration : IEntityTypeConfiguration<Registration>
{
    public void Configure(EntityTypeBuilder<Registration> builder)
    {
        builder.ToTable("Registration", t =>
        {
            t.HasCheckConstraint(
                "CK_Registration_Status",
                "[status] COLLATE Latin1_General_100_BIN2 IN ('PENDING', 'TESTING', 'BAND_ASSIGNED', 'CONFIRMED', 'ADMITTED', 'REJECTED', 'CANCELLED') AND DATALENGTH([status]) = LEN([status])");

            t.HasCheckConstraint(
                "CK_Registration_ApprovedBundle",
                "([admission_approved_by] IS NULL AND [admission_approved_at] IS NULL) OR ([admission_approved_by] IS NOT NULL AND [admission_approved_at] IS NOT NULL)");

            t.HasCheckConstraint(
                "CK_Registration_RejectedBundle",
                "([status] = 'REJECTED' AND [rejected_by] IS NOT NULL AND [rejected_at] IS NOT NULL AND [rejection_reason] IS NOT NULL AND LEN([rejection_reason]) > 0 AND [rejected_from_status] IS NOT NULL AND [rejected_from_status] COLLATE Latin1_General_100_BIN2 = 'CONFIRMED' AND DATALENGTH([rejected_from_status]) = LEN([rejected_from_status]) AND [rejection_operation_id] IS NOT NULL) OR ([status] <> 'REJECTED' AND [rejected_by] IS NULL AND [rejected_at] IS NULL AND [rejection_reason] IS NULL AND [rejected_from_status] IS NULL AND [rejection_operation_id] IS NULL)");

            t.HasCheckConstraint(
                "CK_Registration_CancelledBundle",
                "([status] = 'CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancelled_from_status] IS NOT NULL AND [cancelled_from_status] COLLATE Latin1_General_100_BIN2 IN ('PENDING', 'TESTING', 'BAND_ASSIGNED', 'CONFIRMED', 'ADMITTED') AND DATALENGTH([cancelled_from_status]) = LEN([cancelled_from_status]) AND [cancellation_operation_id] IS NOT NULL) OR ([status] <> 'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancelled_from_status] IS NULL AND [cancellation_operation_id] IS NULL)");
        });

        builder.HasKey(r => r.RegistrationId);

        builder.Property(r => r.RegistrationId)
            .HasColumnName("registration_id")
            .HasColumnType("int")
            .UseIdentityColumn(1, 1);

        builder.Property(r => r.StudentId)
            .HasColumnName("student_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(r => r.CourseId)
            .HasColumnName("course_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(16)")
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired()
            .HasDefaultValue("PENDING")
            .HasSentinel(string.Empty);

        builder.Property(r => r.IsUnfinished)
            .HasColumnName("is_unfinished")
            .HasColumnType("bit")
            .IsRequired()
            .HasDefaultValue(true)
            .HasSentinel(true);

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(r => r.AdmissionConfirmedAt)
            .HasColumnName("admission_confirmed_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired(false);

        builder.Property(r => r.AdmissionApprovedBy)
            .HasColumnName("admission_approved_by")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(r => r.AdmissionApprovedAt)
            .HasColumnName("admission_approved_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired(false);

        builder.Property(r => r.RejectedBy)
            .HasColumnName("rejected_by")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(r => r.RejectedAt)
            .HasColumnName("rejected_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired(false);

        builder.Property(r => r.RejectionReason)
            .HasColumnName("rejection_reason")
            .HasColumnType("nvarchar(2000)")
            .HasMaxLength(2000)
            .IsUnicode(true)
            .IsRequired(false);

        builder.Property(r => r.RejectedFromStatus)
            .HasColumnName("rejected_from_status")
            .HasColumnType("varchar(16)")
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(r => r.RejectionOperationId)
            .HasColumnName("rejection_operation_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired(false);

        builder.Property(r => r.CancelledBy)
            .HasColumnName("cancelled_by")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(r => r.CancelledAt)
            .HasColumnName("cancelled_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired(false);

        builder.Property(r => r.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasColumnType("nvarchar(2000)")
            .HasMaxLength(2000)
            .IsUnicode(true)
            .IsRequired(false);

        builder.Property(r => r.CancelledFromStatus)
            .HasColumnName("cancelled_from_status")
            .HasColumnType("varchar(16)")
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(r => r.CancellationOperationId)
            .HasColumnName("cancellation_operation_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired(false);

        builder.Property(r => r.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("rowversion")
            .IsRowVersion()
            .IsRequired();

        builder.HasOne(r => r.Student)
            .WithMany()
            .HasForeignKey(r => r.StudentId)
            .HasConstraintName("FK_Registration_StudentProfile_student_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(r => r.Course)
            .WithMany()
            .HasForeignKey(r => r.CourseId)
            .HasConstraintName("FK_Registration_Course_course_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(r => r.AdmissionApprovedByAccount)
            .WithMany()
            .HasForeignKey(r => r.AdmissionApprovedBy)
            .HasConstraintName("FK_Registration_Account_admission_approved_by")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(r => r.RejectedByAccount)
            .WithMany()
            .HasForeignKey(r => r.RejectedBy)
            .HasConstraintName("FK_Registration_Account_rejected_by")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(r => r.CancelledByAccount)
            .WithMany()
            .HasForeignKey(r => r.CancelledBy)
            .HasConstraintName("FK_Registration_Account_cancelled_by")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasIndex(r => new { r.StudentId, r.CourseId }, "UQ_Registration_Unfinished")
            .IsUnique()
            .HasDatabaseName("UQ_Registration_Unfinished")
            .HasFilter("[is_unfinished] = 1");

        builder.HasIndex(r => new { r.StudentId, r.CreatedAt, r.RegistrationId }, "IX_Registration_StudentCreated")
            .HasDatabaseName("IX_Registration_StudentCreated");

        builder.HasIndex(r => new { r.CourseId, r.Status, r.CreatedAt, r.RegistrationId }, "IX_Registration_CourseStateCreated")
            .HasDatabaseName("IX_Registration_CourseStateCreated");

        builder.HasIndex(r => r.AdmissionApprovedBy, "IX_Registration_ApprovedBy")
            .HasDatabaseName("IX_Registration_ApprovedBy");

        builder.HasIndex(r => r.RejectedBy, "IX_Registration_RejectedBy")
            .HasDatabaseName("IX_Registration_RejectedBy");

        builder.HasIndex(r => r.CancelledBy, "IX_Registration_CancelledBy")
            .HasDatabaseName("IX_Registration_CancelledBy");
    }
}
