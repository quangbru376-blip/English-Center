namespace EnglishCenter.Api.Infrastructure.Configurations;

using EnglishCenter.Api.Domain.Training;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("Class", t =>
        {
            t.HasCheckConstraint(
                "CK_Class_Status",
                "[status] COLLATE Latin1_General_100_BIN2 IN ('PLANNED', 'OPEN', 'ONGOING', 'COMPLETED', 'CANCELLED') AND DATALENGTH([status]) = LEN([status])");

            t.HasCheckConstraint(
                "CK_Class_MaxSize",
                "[max_size] BETWEEN 1 AND 15");

            t.HasCheckConstraint(
                "CK_Class_Code",
                "DATALENGTH([class_code]) BETWEEN 1 AND 30 AND DATALENGTH([class_code]) = LEN([class_code]) AND [class_code] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Z0-9_-]%'");

            t.HasCheckConstraint(
                "CK_Class_CancellationBundle",
                "([status] = 'CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancellation_reason] IS NOT NULL AND LEN([cancellation_reason]) > 0 AND [cancellation_operation_id] IS NOT NULL) OR ([status] <> 'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancellation_operation_id] IS NULL)");
        });

        builder.HasKey(c => c.ClassId);

        builder.Property(c => c.ClassId)
            .HasColumnName("class_id")
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1);

        builder.Property(c => c.ProgramId)
            .HasColumnName("program_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(c => c.TeacherId)
            .HasColumnName("teacher_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(c => c.ClassCode)
            .HasColumnName("class_code")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(16)")
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired()
            .HasDefaultValue("PLANNED")
            .HasSentinel(string.Empty);

        builder.Property(c => c.MaxSize)
            .HasColumnName("max_size")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(c => c.CancelledBy)
            .HasColumnName("cancelled_by")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(c => c.CancelledAt)
            .HasColumnName("cancelled_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired(false);

        builder.Property(c => c.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasColumnType("nvarchar(2000)")
            .HasMaxLength(2000)
            .IsUnicode(true)
            .IsRequired(false);

        builder.Property(c => c.CancellationOperationId)
            .HasColumnName("cancellation_operation_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired(false);

        builder.Property(c => c.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("rowversion")
            .IsRowVersion();

        builder.HasIndex(c => c.ClassCode)
            .HasDatabaseName("UQ_Class_Code")
            .IsUnique();

        builder.HasIndex(c => new { c.ProgramId, c.Status, c.ClassId })
            .HasDatabaseName("IX_Class_ProgramState");

        builder.HasIndex(c => new { c.TeacherId, c.Status, c.ClassId })
            .HasDatabaseName("IX_Class_TeacherState");

        builder.HasIndex(c => c.CancelledBy)
            .HasDatabaseName("IX_Class_CancelledBy");

        builder.HasOne(c => c.Program)
            .WithMany()
            .HasForeignKey(c => c.ProgramId)
            .HasConstraintName("FK_Class_TrainingProgram_program_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(c => c.Teacher)
            .WithMany()
            .HasForeignKey(c => c.TeacherId)
            .HasConstraintName("FK_Class_Account_teacher_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(c => c.CancelledByAccount)
            .WithMany()
            .HasForeignKey(c => c.CancelledBy)
            .HasConstraintName("FK_Class_Account_cancelled_by")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);
    }
}
