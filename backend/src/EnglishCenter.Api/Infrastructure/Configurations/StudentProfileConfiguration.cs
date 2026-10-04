namespace EnglishCenter.Api.Infrastructure.Configurations;

using EnglishCenter.Api.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
{
    public void Configure(EntityTypeBuilder<StudentProfile> builder)
    {
        builder.ToTable("StudentProfile");
        builder.HasKey(p => p.StudentId);
        builder.Property(p => p.StudentId).HasColumnName("student_id").HasColumnType("int").UseIdentityColumn();
        builder.Property(p => p.AccountId).HasColumnName("account_id").HasColumnType("int").IsRequired();
        builder.Property(p => p.DateOfBirth).HasColumnName("date_of_birth").HasColumnType("date").IsRequired();
        builder.Property(p => p.Address).HasColumnName("address").HasColumnType("nvarchar(510)").HasMaxLength(510).IsUnicode().IsRequired(false);
        builder.Property(p => p.RowVersion).HasColumnName("row_version").HasColumnType("rowversion").IsRequired().IsRowVersion();
        builder.HasOne(p => p.Account).WithOne(a => a.StudentProfile).HasForeignKey<StudentProfile>(p => p.AccountId).OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(p => p.AccountId).IsUnique().HasDatabaseName("UQ_StudentProfile_Account");
    }
}
