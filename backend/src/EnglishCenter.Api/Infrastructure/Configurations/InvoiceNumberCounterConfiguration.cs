using EnglishCenter.Api.Domain.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishCenter.Api.Infrastructure.Configurations;

public sealed class InvoiceNumberCounterConfiguration : IEntityTypeConfiguration<InvoiceNumberCounter>
{
    public void Configure(EntityTypeBuilder<InvoiceNumberCounter> builder)
    {
        builder.ToTable("InvoiceNumberCounter", t =>
        {
            t.HasCheckConstraint("CK_InvoiceNumberCounter_Year", "[issue_year] BETWEEN 1 AND 9999");
            t.HasCheckConstraint("CK_InvoiceNumberCounter_Value", "[last_value] BETWEEN 0 AND 99999999");
        });

        builder.HasKey(c => c.IssueYear)
            .HasName("PK_InvoiceNumberCounter");

        builder.Property(c => c.IssueYear)
            .HasColumnName("issue_year")
            .HasColumnType("smallint")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(c => c.LastValue)
            .HasColumnName("last_value")
            .HasColumnType("int")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(c => c.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("rowversion")
            .IsRowVersion()
            .IsRequired();
    }
}
