using EnglishCenter.Api.Domain.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishCenter.Api.Infrastructure.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoice", t =>
        {
            t.HasCheckConstraint("CK_Invoice_Amount", "[amount] > 0");
            t.HasCheckConstraint(
                "CK_Invoice_Status",
                "[status] COLLATE Latin1_General_100_BIN2 IN ('UNPAID', 'PAID', 'CANCELLED') AND DATALENGTH([status]) = LEN([status])");
            t.HasCheckConstraint(
                "CK_Invoice_NumberPattern",
                "DATALENGTH([invoice_number]) = 16 AND DATALENGTH([invoice_number]) = LEN([invoice_number]) AND [invoice_number] COLLATE Latin1_General_100_BIN2 LIKE 'HD-[0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");
            t.HasCheckConstraint(
                "CK_Invoice_CancelledBundle",
                "([status] = 'CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancelled_from_status] IS NOT NULL AND [cancellation_operation_id] IS NOT NULL AND [cancelled_from_status] COLLATE Latin1_General_100_BIN2 = 'UNPAID' AND DATALENGTH([cancelled_from_status]) = LEN([cancelled_from_status])) OR ([status] <> 'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancelled_from_status] IS NULL AND [cancellation_operation_id] IS NULL)");
        });

        builder.HasKey(x => x.InvoiceId);

        builder.Property(x => x.InvoiceId)
            .HasColumnName("invoice_id")
            .HasColumnType("int")
            .UseIdentityColumn(1, 1);

        builder.Property(x => x.InvoiceNumber)
            .HasColumnName("invoice_number")
            .HasColumnType("varchar(16)")
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.RegistrationId)
            .HasColumnName("registration_id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,0)")
            .HasPrecision(18, 0)
            .IsRequired();

        builder.Property(x => x.IssuedAt)
            .HasColumnName("issued_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired();

        builder.Property(x => x.DueDate)
            .HasColumnName("due_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(16)")
            .HasMaxLength(16)
            .IsUnicode(false)
            .HasDefaultValue("UNPAID")
            .HasSentinel(string.Empty)
            .IsRequired();

        builder.Property(x => x.CancelledBy)
            .HasColumnName("cancelled_by")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(x => x.CancelledAt)
            .HasColumnName("cancelled_at")
            .HasColumnType("datetime2(3)")
            .HasPrecision(3)
            .IsRequired(false);

        builder.Property(x => x.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasColumnType("nvarchar(2000)")
            .HasMaxLength(2000)
            .IsUnicode(true)
            .IsRequired(false);

        builder.Property(x => x.CancelledFromStatus)
            .HasColumnName("cancelled_from_status")
            .HasColumnType("varchar(16)")
            .HasMaxLength(16)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(x => x.CancellationOperationId)
            .HasColumnName("cancellation_operation_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired(false);

        builder.Property(x => x.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("rowversion")
            .IsRowVersion()
            .IsRequired();

        builder.HasIndex(x => x.InvoiceNumber)
            .IsUnique()
            .HasDatabaseName("UQ_Invoice_Number");

        builder.HasIndex(x => x.RegistrationId)
            .IsUnique()
            .HasDatabaseName("UQ_Invoice_Registration");

        builder.HasIndex(x => new { x.Status, x.DueDate, x.InvoiceId })
            .HasDatabaseName("IX_Invoice_StateDue");

        builder.HasIndex(x => new { x.IssuedAt, x.InvoiceId })
            .HasDatabaseName("IX_Invoice_Issued");

        builder.HasIndex(x => x.CancelledBy)
            .HasDatabaseName("IX_Invoice_CancelledBy");

        builder.HasOne(x => x.Registration)
            .WithOne()
            .HasForeignKey<Invoice>(x => x.RegistrationId)
            .HasConstraintName("FK_Invoice_Registration_registration_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(x => x.CancelledByAccount)
            .WithMany()
            .HasForeignKey(x => x.CancelledBy)
            .HasConstraintName("FK_Invoice_Account_cancelled_by")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);
    }
}
