using EnglishCenter.Api.Domain.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishCenter.Api.Infrastructure.Configurations;

public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Course", "dbo", table =>
        {
            table.HasCheckConstraint(
                "CK_Course_Price",
                "[base_price] > 0");

            table.HasCheckConstraint(
                "CK_Course_Status",
                "[status] COLLATE Latin1_General_100_BIN2 IN ('ACTIVE','INACTIVE') AND DATALENGTH([status]) = LEN([status])");

            table.HasCheckConstraint(
                "CK_Course_Code",
                "DATALENGTH([course_code]) BETWEEN 1 AND 30 AND DATALENGTH([course_code]) = LEN([course_code]) AND [course_code] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Z0-9_-]%'");
        });

        builder.HasKey(c => c.CourseId);

        builder.Property(c => c.CourseId)
            .HasColumnName("course_id")
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn();

        builder.Property(c => c.CourseCode)
            .HasColumnName("course_code")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(c => c.CourseName)
            .HasColumnName("course_name")
            .HasColumnType("nvarchar(300)")
            .HasMaxLength(300)
            .IsUnicode(true)
            .IsRequired();

        builder.Property(c => c.BasePrice)
            .HasColumnName("base_price")
            .HasColumnType("decimal(18,0)")
            .HasPrecision(18, 0)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(16)")
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(c => c.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("rowversion")
            .IsRowVersion()
            .IsRequired();

        builder.HasIndex(c => c.CourseCode)
            .IsUnique()
            .HasDatabaseName("UQ_Course_Code");

        builder.HasIndex(c => new { c.Status, c.CourseId })
            .HasDatabaseName("IX_Course_Status_Id");
    }
}
