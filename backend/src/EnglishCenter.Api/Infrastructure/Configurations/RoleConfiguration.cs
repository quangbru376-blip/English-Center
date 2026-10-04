namespace EnglishCenter.Api.Infrastructure.Configurations;

using EnglishCenter.Api.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Role", t =>
        {
            t.HasCheckConstraint(
                "CK_Role_Code",
                "[role_code] COLLATE Latin1_General_100_BIN2 IN ('Student', 'Admissions', 'AcademicAffairs', 'Teacher', 'Accountant', 'Director', 'Admin') AND DATALENGTH([role_code]) = LEN([role_code])");
        });

        builder.HasKey(r => r.RoleId);

        builder.Property(r => r.RoleId)
            .HasColumnName("role_id")
            .HasColumnType("int")
            .ValueGeneratedOnAdd();

        builder.Property(r => r.RoleCode)
            .HasColumnName("role_code")
            .HasColumnType("varchar(32)")
            .HasMaxLength(32)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(r => r.RoleCode)
            .IsUnique()
            .HasDatabaseName("UQ_Role_Code");

        builder.Property(r => r.RoleName)
            .HasColumnName("role_name")
            .HasColumnType("nvarchar(200)")
            .HasMaxLength(200)
            .IsUnicode(true)
            .IsRequired();
    }
}
