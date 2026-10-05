using EnglishCenter.Api.Domain.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishCenter.Api.Infrastructure.Configurations;

public sealed class BandConfiguration : IEntityTypeConfiguration<Band>
{
    public void Configure(EntityTypeBuilder<Band> builder)
    {
        builder.ToTable("Band", "dbo", table =>
        {
            table.UseSqlOutputClause(false);

            table.HasCheckConstraint(
                "CK_Band_MinScore",
                "[min_score] >= 0 AND [min_score] <= 100");

            table.HasCheckConstraint(
                "CK_Band_Code",
                "DATALENGTH([band_code]) BETWEEN 1 AND 30 AND [band_code] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^ -~]%' AND LEN([band_code]) > 0");
        });

        builder.HasKey(b => b.BandId);

        builder.Property(b => b.BandId)
            .HasColumnName("band_id")
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1);

        builder.Property(b => b.BandCode)
            .HasColumnName("band_code")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(b => b.MinScore)
            .HasColumnName("min_score")
            .HasColumnType("decimal(5,2)")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(b => b.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("rowversion")
            .IsRowVersion()
            .IsRequired();

        builder.HasIndex(b => b.BandCode)
            .HasDatabaseName("UQ_Band_Code")
            .IsUnique();

        builder.HasIndex(b => b.MinScore)
            .HasDatabaseName("UQ_Band_MinScore")
            .IsUnique();
    }
}
