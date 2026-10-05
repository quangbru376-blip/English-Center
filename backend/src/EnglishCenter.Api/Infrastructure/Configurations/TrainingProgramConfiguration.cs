using EnglishCenter.Api.Domain.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishCenter.Api.Infrastructure.Configurations;

public sealed class TrainingProgramConfiguration : IEntityTypeConfiguration<TrainingProgram>
{
    public void Configure(EntityTypeBuilder<TrainingProgram> builder)
    {
        builder.ToTable("TrainingProgram", "dbo", table =>
        {
            table.HasCheckConstraint(
                "CK_TrainingProgram_Status",
                "[status] COLLATE Latin1_General_100_BIN2 IN ('DRAFT','ACTIVE','INACTIVE') AND DATALENGTH([status]) = LEN([status])");
            table.HasCheckConstraint(
                "CK_TrainingProgram_ExpectedLessons",
                "[expected_lesson_count] > 0");
        });

        builder.HasKey(x => x.ProgramId);

        builder.Property(x => x.ProgramId)
            .HasColumnName("program_id")
            .HasColumnType("int")
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(x => x.CourseId)
            .HasColumnName("course_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.BandId)
            .HasColumnName("band_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.ProgramName)
            .HasColumnName("program_name")
            .HasColumnType("nvarchar(300)")
            .HasMaxLength(300)
            .IsUnicode(true)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(16)")
            .HasMaxLength(16)
            .IsUnicode(false)
            .HasDefaultValue("DRAFT")
            .HasSentinel(string.Empty)
            .IsRequired();

        builder.Property(x => x.ExpectedLessonCount)
            .HasColumnName("expected_lesson_count")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("rowversion")
            .IsRowVersion()
            .IsRequired();

        builder.HasOne(x => x.Course)
            .WithMany()
            .HasForeignKey(x => x.CourseId)
            .HasConstraintName("FK_TrainingProgram_Course_course_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(x => x.Band)
            .WithMany()
            .HasForeignKey(x => x.BandId)
            .HasConstraintName("FK_TrainingProgram_Band_band_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(x => new { x.CourseId, x.BandId })
            .HasDatabaseName("UQ_Program_CourseBand")
            .IsUnique();

        builder.HasIndex(x => x.BandId)
            .HasDatabaseName("IX_Program_Band");

        builder.HasIndex(x => new { x.Status, x.ProgramId })
            .HasDatabaseName("IX_Program_Status_Id");
    }
}
