using EnglishCenter.Api.Domain.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishCenter.Api.Infrastructure.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payment", t =>
        {
            t.HasCheckConstraint("CK_Payment_Amount", "[amount_paid] > 0");
            t.HasCheckConstraint(
                "CK_Payment_Method",
                "[method] COLLATE Latin1_General_100_BIN2 IN ('CASH', 'TRANSFER') AND DATALENGTH([method]) = LEN([method])");
            t.HasCheckConstraint(
                "CK_Payment_Reference",
                "[method] <> 'TRANSFER' OR ([transaction_reference] IS NOT NULL AND LEN([transaction_reference]) > 0)");
            t.HasCheckConstraint(
                "CK_Payment_CorrectionBundle",
                "([last_corrected_by] IS NULL AND [last_corrected_at] IS NULL AND [last_correction_reason] IS NULL) OR ([last_corrected_by] IS NOT NULL AND [last_corrected_at] IS NOT NULL AND [last_correction_reason] IS NOT NULL AND LEN([last_correction_reason]) > 0)");
        });

        builder.HasKey(x => x.PaymentId);

        builder.Property(x => x.PaymentId)
            .HasColumnName("payment_id")
            .HasColumnType("int")
            .UseIdentityColumn(1, 1);

        builder.Property(x => x.InvoiceId)
            .HasColumnName("invoice_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.AmountPaid)
            .HasColumnName("amount_paid")
            .HasColumnType("decimal(18,0)")
            .HasPrecision(18, 0)
            .IsRequired();

        builder.Property(x => x.Method)
            .HasColumnName("method")
            .HasColumnType("varchar(16)")
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.TransactionReference)
            .HasColumnName("transaction_reference")
            .HasColumnType("nvarchar(200)")
            .HasMaxLength(200)
            .IsUnicode(true)
            .IsRequired(false);

        builder.Property(x => x.ConfirmedBy)
            .HasColumnName("confirmed_by")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.PaidAt)
            .HasColumnName("paid_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired();

        builder.Property(x => x.LastCorrectedBy)
            .HasColumnName("last_corrected_by")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(x => x.LastCorrectedAt)
            .HasColumnName("last_corrected_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired(false);

        builder.Property(x => x.LastCorrectionReason)
            .HasColumnName("last_correction_reason")
            .HasColumnType("nvarchar(2000)")
            .HasMaxLength(2000)
            .IsUnicode(true)
            .IsRequired(false);

        builder.Property(x => x.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("rowversion")
            .IsRowVersion()
            .IsRequired();

        builder.HasIndex(x => x.InvoiceId)
            .IsUnique()
            .HasDatabaseName("UQ_Payment_Invoice");

        builder.HasIndex(x => new { x.PaidAt, x.PaymentId })
            .HasDatabaseName("IX_Payment_Paid");

        builder.HasIndex(x => x.ConfirmedBy)
            .HasDatabaseName("IX_Payment_ConfirmedBy");

        builder.HasIndex(x => x.LastCorrectedBy)
            .HasDatabaseName("IX_Payment_LastCorrectedBy");

        builder.HasOne(x => x.Invoice)
            .WithOne()
            .HasForeignKey<Payment>(x => x.InvoiceId)
            .HasConstraintName("FK_Payment_Invoice_invoice_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(x => x.ConfirmedByAccount)
            .WithMany()
            .HasForeignKey(x => x.ConfirmedBy)
            .HasConstraintName("FK_Payment_Account_confirmed_by")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(x => x.LastCorrectedByAccount)
            .WithMany()
            .HasForeignKey(x => x.LastCorrectedBy)
            .HasConstraintName("FK_Payment_Account_last_corrected_by")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);
    }
}
