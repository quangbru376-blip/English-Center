using EnglishCenter.Api.Domain.Training;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishCenter.Api.Infrastructure.Configurations;

public sealed class GradeReportConfiguration : IEntityTypeConfiguration<GradeReport>
{
    public void Configure(EntityTypeBuilder<GradeReport> builder)
    {
        builder.ToTable("GradeReport", "dbo", table =>
        {
            table.HasCheckConstraint(
                "CK_GradeReport_Status",
                "[status] COLLATE Latin1_General_100_BIN2 IN ('DRAFT', 'SUBMITTED', 'RETURNED', 'FINALIZED') AND DATALENGTH([status]) = LEN([status])");

            table.HasCheckConstraint(
                "CK_GradeReport_FinalResult",
                "([status] = 'FINALIZED' AND [final_score] IS NOT NULL AND [final_score] >= 0 AND [final_score] <= 100 AND [finalized_at] IS NOT NULL AND [reviewed_by] IS NOT NULL AND [reviewed_at] IS NOT NULL AND [review_operation_id] IS NOT NULL AND [finalized_at] = [reviewed_at]) OR ([status] <> 'FINALIZED' AND [final_score] IS NULL AND [finalized_at] IS NULL)");

            table.HasCheckConstraint(
                "CK_GradeReport_SubmitBundle",
                "([submitted_by] IS NOT NULL AND [submitted_at] IS NOT NULL AND [submitted_operation_id] IS NOT NULL) OR ([status] = 'DRAFT' AND [submitted_by] IS NULL AND [submitted_at] IS NULL AND [submitted_operation_id] IS NULL)");

            table.HasCheckConstraint(
                "CK_GradeReport_ReviewBundle",
                "([reviewed_by] IS NULL AND [reviewed_at] IS NULL AND [review_operation_id] IS NULL) OR ([reviewed_by] IS NOT NULL AND [reviewed_at] IS NOT NULL AND [review_operation_id] IS NOT NULL)");

            table.HasCheckConstraint(
                "CK_GradeReport_ReturnedReason",
                "[status] <> 'RETURNED' OR ([returned_reason] IS NOT NULL AND LEN([returned_reason]) > 0 AND [reviewed_by] IS NOT NULL AND [reviewed_at] IS NOT NULL AND [review_operation_id] IS NOT NULL)");
        });

        builder.HasKey(r => r.ReportId);

        builder.Property(r => r.ReportId)
            .HasColumnName("report_id")
            .HasColumnType("int")
            .UseIdentityColumn(1, 1);

        builder.Property(r => r.ClassAssignmentId)
            .HasColumnName("class_assignment_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(r => r.FinalScore)
            .HasColumnName("final_score")
            .HasPrecision(5, 2)
            .HasColumnType("decimal(5,2)")
            .IsRequired(false);

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(16)")
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired()
            .HasDefaultValue("DRAFT")
            .HasSentinel(string.Empty);

        builder.Property(r => r.TeacherComment)
            .HasColumnName("teacher_comment")
            .HasColumnType("nvarchar(2000)")
            .HasMaxLength(2000)
            .IsUnicode(true)
            .IsRequired(false);

        builder.Property(r => r.SubmittedBy)
            .HasColumnName("submitted_by")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(r => r.SubmittedAt)
            .HasColumnName("submitted_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired(false);

        builder.Property(r => r.SubmittedOperationId)
            .HasColumnName("submitted_operation_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired(false);

        builder.Property(r => r.ReviewedBy)
            .HasColumnName("reviewed_by")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(r => r.ReviewedAt)
            .HasColumnName("reviewed_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired(false);

        builder.Property(r => r.ReviewOperationId)
            .HasColumnName("review_operation_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired(false);

        builder.Property(r => r.ReturnedReason)
            .HasColumnName("returned_reason")
            .HasColumnType("nvarchar(2000)")
            .HasMaxLength(2000)
            .IsUnicode(true)
            .IsRequired(false);

        builder.Property(r => r.FinalizedAt)
            .HasColumnName("finalized_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired(false);

        builder.Property(r => r.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("rowversion")
            .IsRowVersion()
            .IsRequired();

        builder.HasOne(r => r.ClassAssignment)
            .WithOne()
            .HasForeignKey<GradeReport>(r => r.ClassAssignmentId)
            .HasPrincipalKey<StudentClassAssignment>(a => a.ClassAssignmentId)
            .HasConstraintName("FK_GradeReport_StudentClassAssignment_class_assignment_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(r => r.SubmittedByAccount)
            .WithMany()
            .HasForeignKey(r => r.SubmittedBy)
            .HasPrincipalKey(a => a.AccountId)
            .HasConstraintName("FK_GradeReport_Account_submitted_by")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(r => r.ReviewedByAccount)
            .WithMany()
            .HasForeignKey(r => r.ReviewedBy)
            .HasPrincipalKey(a => a.AccountId)
            .HasConstraintName("FK_GradeReport_Account_reviewed_by")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasIndex(r => r.ClassAssignmentId)
            .IsUnique()
            .HasDatabaseName("UQ_GradeReport_Assignment");

        builder.HasIndex(r => new { r.Status, r.FinalizedAt, r.ReportId })
            .HasDatabaseName("IX_GradeReport_StateFinalized");

        builder.HasIndex(r => r.SubmittedBy)
            .HasDatabaseName("IX_GradeReport_SubmittedBy");

        builder.HasIndex(r => r.ReviewedBy)
            .HasDatabaseName("IX_GradeReport_ReviewedBy");
    }
}
