-- sp.sql
-- Açıklama: Ödünç verme ve iade alma işlemleri için stored procedure'lar
-- Hedef: SQL Server

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1) Ödünç Verme Stored Procedure
-- sp_borrow_copy(member_id, copy_id) - Transaction içinde ödünç verme işlemi
IF OBJECT_ID('dbo.sp_borrow_copy', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_borrow_copy;
GO
CREATE PROCEDURE dbo.sp_borrow_copy
    @member_id INT,
    @copy_id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Transaction başlat
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Kopya var mı ve müsait mi kontrol et
        IF NOT EXISTS (SELECT 1 FROM dbo.Copies WHERE CopyId = @copy_id AND Status = 'Available')
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('Kopya bulunamadı veya müsait değil.', 16, 1);
            RETURN;
        END
        
        -- Üye var mı kontrol et
        IF NOT EXISTS (SELECT 1 FROM dbo.Members WHERE MemberId = @member_id AND Status = 'Active')
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('Üye bulunamadı veya aktif değil.', 16, 1);
            RETURN;
        END
        
        -- Üyenin aktif ödünç sayısını kontrol et (max 3)
        DECLARE @active_loan_count INT;
        SELECT @active_loan_count = COUNT(*) 
        FROM dbo.Loans 
        WHERE MemberId = @member_id AND ReturnedAt IS NULL;
        
        IF @active_loan_count >= 3
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('Üyenin aktif ödünç limiti dolmuş (maksimum 3).', 16, 1);
            RETURN;
        END
        
        -- Loan kaydı oluştur (14 günlük süre ile)
        DECLARE @now DATETIME2 = SYSUTCDATETIME();
        DECLARE @due_date DATETIME2 = DATEADD(DAY, 14, @now);
        
        INSERT INTO dbo.Loans (MemberId, CopyId, LoanedAt, DueAt, ReturnedAt)
        VALUES (@member_id, @copy_id, @now, @due_date, NULL);
        
        -- Kopya durumunu 'Loaned' olarak güncelle
        UPDATE dbo.Copies
        SET Status = 'Loaned'
        WHERE CopyId = @copy_id;
        
        -- Transaction commit
        COMMIT TRANSACTION;
        
        -- Başarı mesajı
        SELECT 'SUCCESS' AS Result, 
               @due_date AS DueDate,
               'Kitap başarıyla ödünç verildi.' AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        DECLARE @error_message NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @error_severity INT = ERROR_SEVERITY();
        DECLARE @error_state INT = ERROR_STATE();
        
        RAISERROR(@error_message, @error_severity, @error_state);
    END CATCH
END
GO

-- 2) İade Alma Stored Procedure
-- sp_return_copy(loan_id) - Transaction içinde iade alma işlemi
IF OBJECT_ID('dbo.sp_return_copy', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_return_copy;
GO
CREATE PROCEDURE dbo.sp_return_copy
    @loan_id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Transaction başlat
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Loan kaydını kontrol et
        DECLARE @copy_id INT;
        DECLARE @returned_at DATETIME2;
        
        SELECT @copy_id = CopyId, @returned_at = ReturnedAt
        FROM dbo.Loans
        WHERE LoanId = @loan_id;
        
        IF @copy_id IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('Ödünç kaydı bulunamadı.', 16, 1);
            RETURN;
        END
        
        IF @returned_at IS NOT NULL
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('Bu ödünç kaydı zaten iade edilmiş.', 16, 1);
            RETURN;
        END
        
        -- ReturnedAt'i güncelle (trigger tetiklenecek)
        UPDATE dbo.Loans
        SET ReturnedAt = SYSUTCDATETIME()
        WHERE LoanId = @loan_id;
        
        -- Kopya durumunu 'Available' olarak güncelle
        UPDATE dbo.Copies
        SET Status = 'Available'
        WHERE CopyId = @copy_id;
        
        -- Transaction commit
        COMMIT TRANSACTION;
        
        -- Başarı mesajı
        SELECT 'SUCCESS' AS Result, 
               'Kitap başarıyla iade edildi.' AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        DECLARE @error_message NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @error_severity INT = ERROR_SEVERITY();
        DECLARE @error_state INT = ERROR_STATE();
        
        RAISERROR(@error_message, @error_severity, @error_state);
    END CATCH
END
GO






