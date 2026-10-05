namespace EnglishCenter.Api.Infrastructure.Configurations;

using EnglishCenter.Api.Domain.Training;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class ScoreConfiguration : IEntityTypeConfiguration<Score>
{
    public void Configure(EntityTypeBuilder<Score> builder)
    {
        builder.ToTable("Score", table =>
        {
            table.HasCheckConstraint(
                "CK_Score_Score",
                "[score_value] >= 0 AND [score_value] <= 100");
        });

        builder.HasKey(s => s.ScoreId);

        builder.Property(s => s.ScoreId)
            .HasColumnName("score_id")
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1);

        builder.Property(s => s.ClassAssignmentId)
            .HasColumnName("class_assignment_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(s => s.ComponentId)
            .HasColumnName("component_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(s => s.ScheduleId)
            .HasColumnName("schedule_id")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(s => s.ScoreValue)
            .HasColumnName("score_value")
            .HasColumnType("decimal(5,2)")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(s => s.EnteredBy)
            .HasColumnName("entered_by")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(s => s.EnteredAt)
            .HasColumnName("entered_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired();

        builder.Property(s => s.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("rowversion")
            .IsRowVersion()
            .IsRequired();

        builder.HasOne(s => s.ClassAssignment)
            .WithMany()
            .HasForeignKey(s => s.ClassAssignmentId)
            .HasConstraintName("FK_Score_StudentClassAssignment_class_assignment_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(s => s.Component)
            .WithMany()
            .HasForeignKey(s => s.ComponentId)
            .HasConstraintName("FK_Score_ScoreComponentConfig_component_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(s => s.EnteredByAccount)
            .WithMany()
            .HasForeignKey(s => s.EnteredBy)
            .HasConstraintName("FK_Score_Account_entered_by")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(s => s.Schedule)
            .WithMany()
            .HasForeignKey(s => s.ScheduleId)
            .HasConstraintName("FK_Score_Schedule_schedule_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasIndex(s => new { s.ClassAssignmentId, s.ComponentId })
            .HasDatabaseName("UQ_Score_AssignmentComponent")
            .IsUnique();

        builder.HasIndex(s => new { s.ComponentId, s.ScoreId })
            .HasDatabaseName("IX_Score_Component");

        builder.HasIndex(s => new { s.ScheduleId, s.ScoreId })
            .HasDatabaseName("IX_Score_Schedule")
            .HasFilter("[schedule_id] IS NOT NULL");

        builder.HasIndex(s => s.EnteredBy)
            .HasDatabaseName("IX_Score_EnteredBy");
    }
}
