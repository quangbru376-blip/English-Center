CREATE OR ALTER PROCEDURE [dbo].[usp_AllocateInvoiceNumber]
    @issueYear smallint
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        IF XACT_STATE() <> 1 OR @@TRANCOUNT = 0
        BEGIN
            THROW 51107, 'An active caller transaction is required.', 1;
        END;

        IF @issueYear IS NULL OR @issueYear < 1 OR @issueYear > 9999
        BEGIN
            THROW 51108, 'The issue year must be between 1 and 9999.', 1;
        END;

        DECLARE @lastValue int = NULL;
        DECLARE @allocatedValue int;

        SELECT @lastValue = [last_value]
        FROM [dbo].[InvoiceNumberCounter] WITH (UPDLOCK, HOLDLOCK)
        WHERE [issue_year] = @issueYear;

        IF @lastValue IS NOT NULL
        BEGIN
            IF @lastValue >= 99999999
            BEGIN
                THROW 51108, 'Invoice number counter has reached its maximum value.', 1;
            END;

            SET @allocatedValue = @lastValue + 1;

            UPDATE [dbo].[InvoiceNumberCounter]
            SET [last_value] = @allocatedValue
            WHERE [issue_year] = @issueYear;
        END
        ELSE
        BEGIN
            SET @allocatedValue = 1;

            INSERT INTO [dbo].[InvoiceNumberCounter] ([issue_year], [last_value])
            VALUES (@issueYear, @allocatedValue);
        END;

        DECLARE @invoiceNumber varchar(16);
        SET @invoiceNumber = CONCAT(
            'HD-',
            RIGHT('0000' + CONVERT(varchar(4), @issueYear), 4),
            '-',
            RIGHT('00000000' + CONVERT(varchar(8), @allocatedValue), 8)
        );

        SELECT @invoiceNumber AS [invoiceNumber];
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH;
END;
