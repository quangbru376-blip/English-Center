namespace EnglishCenter.Api.Infrastructure.Configurations;

using EnglishCenter.Api.Domain.Admissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class BandAssignmentConfiguration : IEntityTypeConfiguration<BandAssignment>
{
    public void Configure(EntityTypeBuilder<BandAssignment> builder)
    {
        builder.ToTable("BandAssignment");

        builder.HasKey(x => x.BandAssignmentId);

        builder.Property(x => x.BandAssignmentId)
            .HasColumnName("band_assignment_id")
            .HasColumnType("int")
            .IsRequired()
            .UseIdentityColumn(1, 1);

        builder.Property(x => x.RegistrationId)
            .HasColumnName("registration_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.BestAttemptId)
            .HasColumnName("best_attempt_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.BandId)
            .HasColumnName("band_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.AssignedAt)
            .HasColumnName("assigned_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(x => x.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("rowversion")
            .IsRequired()
            .IsRowVersion();

        builder.HasOne(x => x.Registration)
            .WithOne()
            .HasForeignKey<BandAssignment>(x => x.RegistrationId)
            .HasConstraintName("FK_BandAssignment_Registration_registration_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(x => x.BestAttempt)
            .WithMany()
            .HasForeignKey(x => new { x.BestAttemptId, x.RegistrationId })
            .HasPrincipalKey(a => new { a.AttemptId, a.RegistrationId })
            .HasConstraintName("FK_BandAssignment_Attempt_BestAttemptRegistration")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(x => x.Band)
            .WithMany()
            .HasForeignKey(x => x.BandId)
            .HasConstraintName("FK_BandAssignment_Band_band_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(x => x.RegistrationId)
            .IsUnique()
            .HasDatabaseName("UQ_BandAssignment_Registration");

        builder.HasIndex(x => x.BandId)
            .HasDatabaseName("IX_BandAssignment_Band");

        builder.HasIndex(x => new { x.BestAttemptId, x.RegistrationId })
            .HasDatabaseName("IX_BandAssignment_BestAttempt");
    }
}
