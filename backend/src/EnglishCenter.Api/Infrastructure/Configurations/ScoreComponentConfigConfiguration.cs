using EnglishCenter.Api.Domain.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishCenter.Api.Infrastructure.Configurations;

public sealed class ScoreComponentConfigConfiguration : IEntityTypeConfiguration<ScoreComponentConfig>
{
    public void Configure(EntityTypeBuilder<ScoreComponentConfig> builder)
    {
        builder.ToTable("ScoreComponentConfig", "dbo", table =>
        {
            table.UseSqlOutputClause(false);

            table.HasCheckConstraint(
                "CK_ScoreComponentConfig_Kind",
                "[kind] COLLATE Latin1_General_100_BIN2 IN ('GENERAL','LESSON','FINAL_EXAM') AND DATALENGTH([kind]) = LEN([kind])");

            table.HasCheckConstraint(
                "CK_ScoreComponentConfig_Weight",
                "[weight] > 0 AND [weight] <= 100");

            table.HasCheckConstraint(
                "CK_ScoreComponentConfig_Name",
                "LEN([component_name]) > 0");
        });

        builder.HasKey(x => x.ComponentId);

        builder.Property(x => x.ComponentId)
            .HasColumnName("component_id")
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1);

        builder.Property(x => x.ProgramId)
            .HasColumnName("program_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.ComponentName)
            .HasColumnName("component_name")
            .HasColumnType("nvarchar(200)")
            .HasMaxLength(200)
            .IsUnicode(true)
            .UseCollation("Latin1_General_100_CI_AS_SC")
            .IsRequired();

        builder.Property(x => x.Weight)
            .HasColumnName("weight")
            .HasColumnType("decimal(5,2)")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasColumnType("varchar(16)")
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("rowversion")
            .IsRowVersion()
            .IsRequired();

        builder.HasOne(x => x.Program)
            .WithMany()
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_ScoreComponentConfig_TrainingProgram_program_id")
            .IsRequired();

        builder.HasIndex(x => new { x.ProgramId, x.ComponentName })
            .HasDatabaseName("UQ_Component_ProgramName")
            .IsUnique();

        builder.HasIndex(x => x.ProgramId)
            .HasDatabaseName("UQ_Component_FinalExam")
            .IsUnique()
            .HasFilter("[kind] = 'FINAL_EXAM'");
    }
}
