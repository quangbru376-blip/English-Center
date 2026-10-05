namespace EnglishCenter.Api.Infrastructure.Configurations;

using EnglishCenter.Api.Domain.Training;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable("Attendance", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
        });

        builder.HasKey(a => a.AttendanceId);

        builder.Property(a => a.AttendanceId)
            .HasColumnName("attendance_id")
            .HasColumnType("int")
            .UseIdentityColumn(1, 1);

        builder.Property(a => a.ScheduleId)
            .HasColumnName("schedule_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(a => a.ClassAssignmentId)
            .HasColumnName("class_assignment_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(a => a.Present)
            .HasColumnName("present")
            .HasColumnType("bit")
            .IsRequired();

        builder.Property(a => a.EnteredBy)
            .HasColumnName("entered_by")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(a => a.EnteredAt)
            .HasColumnName("entered_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired();

        builder.Property(a => a.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("rowversion")
            .IsRowVersion()
            .IsRequired();

        builder.HasOne(a => a.Schedule)
            .WithMany()
            .HasForeignKey(a => a.ScheduleId)
            .HasConstraintName("FK_Attendance_Schedule_schedule_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(a => a.ClassAssignment)
            .WithMany()
            .HasForeignKey(a => a.ClassAssignmentId)
            .HasConstraintName("FK_Attendance_StudentClassAssignment_class_assignment_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(a => a.EnteredByAccount)
            .WithMany()
            .HasForeignKey(a => a.EnteredBy)
            .HasConstraintName("FK_Attendance_Account_entered_by")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(a => new { a.ScheduleId, a.ClassAssignmentId })
            .HasDatabaseName("UQ_Attendance_ScheduleAssignment")
            .IsUnique();

        builder.HasIndex(a => new { a.ClassAssignmentId, a.AttendanceId })
            .HasDatabaseName("IX_Attendance_Assignment");

        builder.HasIndex(a => a.EnteredBy)
            .HasDatabaseName("IX_Attendance_EnteredBy");
    }
}
