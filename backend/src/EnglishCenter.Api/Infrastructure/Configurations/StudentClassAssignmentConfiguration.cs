namespace EnglishCenter.Api.Infrastructure.Configurations;

using EnglishCenter.Api.Domain.Training;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class StudentClassAssignmentConfiguration : IEntityTypeConfiguration<StudentClassAssignment>
{
    public void Configure(EntityTypeBuilder<StudentClassAssignment> builder)
    {
        builder.ToTable("StudentClassAssignment");

        builder.HasKey(x => x.ClassAssignmentId);

        builder.Property(x => x.ClassAssignmentId)
            .HasColumnName("class_assignment_id")
            .HasColumnType("int")
            .IsRequired()
            .UseIdentityColumn(1, 1);

        builder.Property(x => x.ClassId)
            .HasColumnName("class_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.RegistrationId)
            .HasColumnName("registration_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.AssignedBy)
            .HasColumnName("assigned_by")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.AssignedAt)
            .HasColumnName("assigned_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.HasOne(x => x.Class)
            .WithMany()
            .HasForeignKey(x => x.ClassId)
            .HasConstraintName("FK_Assignment_Class_class_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(x => x.Registration)
            .WithOne()
            .HasForeignKey<StudentClassAssignment>(x => x.RegistrationId)
            .HasConstraintName("FK_Assignment_Registration_registration_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(x => x.AssignedByAccount)
            .WithMany()
            .HasForeignKey(x => x.AssignedBy)
            .HasConstraintName("FK_Assignment_Account_assigned_by")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(x => x.RegistrationId)
            .IsUnique()
            .HasDatabaseName("UQ_Assignment_Registration");

        builder.HasIndex(x => new { x.ClassId, x.ClassAssignmentId })
            .HasDatabaseName("IX_Assignment_Class");

        builder.HasIndex(x => x.AssignedBy)
            .HasDatabaseName("IX_Assignment_AssignedBy");
    }
}
