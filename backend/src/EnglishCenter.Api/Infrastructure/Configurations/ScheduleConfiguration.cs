using EnglishCenter.Api.Domain.Training;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishCenter.Api.Infrastructure.Configurations;

public sealed class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.ToTable("Schedule", "dbo", t =>
        {
            t.HasCheckConstraint(
                "CK_Schedule_Type",
                "[session_type] COLLATE Latin1_General_100_BIN2 IN ('LESSON', 'FINAL_EXAM') AND DATALENGTH([session_type]) = LEN([session_type])");

            t.HasCheckConstraint(
                "CK_Schedule_State",
                "[occurrence_state] COLLATE Latin1_General_100_BIN2 IN ('PLANNED', 'HELD', 'NOT_HELD') AND DATALENGTH([occurrence_state]) = LEN([occurrence_state])");

            t.HasCheckConstraint(
                "CK_Schedule_Time",
                "[start_time] < [end_time] AND DATEPART(second, [start_time]) = 0 AND DATEPART(second, [end_time]) = 0");

            t.HasCheckConstraint(
                "CK_Schedule_Room",
                "DATALENGTH([room]) BETWEEN 1 AND 30 AND DATALENGTH([room]) = LEN([room]) AND [room] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Z0-9_-]%'");

            t.HasCheckConstraint(
                "CK_Schedule_NoSelfReplacement",
                "[replaces_schedule_id] IS NULL OR [replaces_schedule_id] <> [schedule_id]");

            t.HasCheckConstraint(
                "CK_Schedule_CurrentState",
                "([occurrence_state] IN ('PLANNED', 'HELD') AND [is_current] = 1) OR ([occurrence_state] = 'NOT_HELD' AND [is_current] = 0)");

            t.HasCheckConstraint(
                "CK_Schedule_HeldBundle",
                "([occurrence_state] = 'HELD' AND [held_by] IS NOT NULL AND [held_at] IS NOT NULL) OR ([occurrence_state] <> 'HELD' AND [held_by] IS NULL AND [held_at] IS NULL)");

            t.HasCheckConstraint(
                "CK_Schedule_NotHeldBundle",
                "([occurrence_state] = 'NOT_HELD' AND [not_held_by] IS NOT NULL AND [not_held_at] IS NOT NULL AND [not_held_reason] IS NOT NULL AND LEN([not_held_reason]) > 0 AND [replacement_operation_id] IS NOT NULL) OR ([occurrence_state] <> 'NOT_HELD' AND [not_held_by] IS NULL AND [not_held_at] IS NULL AND [not_held_reason] IS NULL AND [replacement_operation_id] IS NULL)");
        });

        builder.HasKey(s => s.ScheduleId);

        builder.Property(s => s.ScheduleId)
            .HasColumnName("schedule_id")
            .HasColumnType("int")
            .UseIdentityColumn(1, 1);

        builder.Property(s => s.ClassId)
            .HasColumnName("class_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(s => s.SessionType)
            .HasColumnName("session_type")
            .HasColumnType("varchar(16)")
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(s => s.ObligationKey)
            .HasColumnName("obligation_key")
            .HasColumnType("uniqueidentifier")
            .IsRequired();

        builder.Property(s => s.ReplacesScheduleId)
            .HasColumnName("replaces_schedule_id")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(s => s.IsCurrent)
            .HasColumnName("is_current")
            .HasColumnType("bit")
            .HasDefaultValue(true)
            .HasSentinel(true)
            .IsRequired();

        builder.Property(s => s.SessionDate)
            .HasColumnName("session_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(s => s.StartTime)
            .HasColumnName("start_time")
            .HasColumnType("time(0)")
            .HasPrecision(0)
            .IsRequired();

        builder.Property(s => s.EndTime)
            .HasColumnName("end_time")
            .HasColumnType("time(0)")
            .HasPrecision(0)
            .IsRequired();

        builder.Property(s => s.Room)
            .HasColumnName("room")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(s => s.OccurrenceState)
            .HasColumnName("occurrence_state")
            .HasColumnType("varchar(16)")
            .HasMaxLength(16)
            .IsUnicode(false)
            .HasDefaultValue("PLANNED")
            .HasSentinel(string.Empty)
            .IsRequired();

        builder.Property(s => s.HeldBy)
            .HasColumnName("held_by")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(s => s.HeldAt)
            .HasColumnName("held_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired(false);

        builder.Property(s => s.NotHeldBy)
            .HasColumnName("not_held_by")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(s => s.NotHeldAt)
            .HasColumnName("not_held_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired(false);

        builder.Property(s => s.NotHeldReason)
            .HasColumnName("not_held_reason")
            .HasColumnType("nvarchar(2000)")
            .HasMaxLength(2000)
            .IsUnicode(true)
            .IsRequired(false);

        builder.Property(s => s.ReplacementOperationId)
            .HasColumnName("replacement_operation_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired(false);

        builder.Property(s => s.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("rowversion")
            .IsRowVersion()
            .IsRequired();

        builder.HasOne(s => s.Class)
            .WithMany()
            .HasForeignKey(s => s.ClassId)
            .HasConstraintName("FK_Schedule_Class_class_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(s => s.ReplacesSchedule)
            .WithOne()
            .HasForeignKey<Schedule>(s => s.ReplacesScheduleId)
            .HasConstraintName("FK_Schedule_Schedule_replaces_schedule_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(s => s.HeldByAccount)
            .WithMany()
            .HasForeignKey(s => s.HeldBy)
            .HasConstraintName("FK_Schedule_Account_held_by")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(s => s.NotHeldByAccount)
            .WithMany()
            .HasForeignKey(s => s.NotHeldBy)
            .HasConstraintName("FK_Schedule_Account_not_held_by")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasIndex(s => new { s.ClassId, s.ObligationKey })
            .IsUnique()
            .HasDatabaseName("UQ_Schedule_CurrentObligation")
            .HasFilter("[is_current] = 1");

        builder.HasIndex(s => s.ClassId)
            .IsUnique()
            .HasDatabaseName("UQ_Schedule_CurrentFinalExam")
            .HasFilter("[is_current] = 1 AND [session_type] = 'FINAL_EXAM'");

        builder.HasIndex(s => s.ReplacesScheduleId)
            .IsUnique()
            .HasDatabaseName("UQ_Schedule_Predecessor")
            .HasFilter("[replaces_schedule_id] IS NOT NULL");

        builder.HasIndex(s => new { s.ClassId, s.IsCurrent, s.SessionDate, s.StartTime, s.ScheduleId })
            .HasDatabaseName("IX_Schedule_ClassCurrentDate");

        builder.HasIndex(s => new { s.Room, s.SessionDate, s.StartTime, s.EndTime, s.ScheduleId })
            .HasDatabaseName("IX_Schedule_RoomDate");

        builder.HasIndex(s => s.HeldBy)
            .HasDatabaseName("IX_Schedule_HeldBy");

        builder.HasIndex(s => s.NotHeldBy)
            .HasDatabaseName("IX_Schedule_NotHeldBy");
    }
}
