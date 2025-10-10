-- triggers.sql
-- Amaç: Audit log tablosu ve loans üzerinde tetikleyici ile BORROW/RETURN işlemlerini kaydetmek

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1) Audit Log Tablosu
IF OBJECT_ID('dbo.Audit_Log', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Audit_Log(
        AuditId INT IDENTITY(1,1) PRIMARY KEY,
        LoanId INT NOT NULL,
        Action NVARCHAR(20) NOT NULL,
        ActionTime DATETIME2 NOT NULL CONSTRAINT DF_Audit_Log_ActionTime DEFAULT SYSUTCDATETIME()
    );
END
GO

-- 2) Reservations Tablosu
IF OBJECT_ID('dbo.Reservations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reservations(
        ReservationId INT IDENTITY(1,1) PRIMARY KEY,
        MemberId INT NOT NULL,
        CopyId INT NOT NULL,
        ReservedAt DATETIME2 NOT NULL CONSTRAINT DF_Reservations_ReservedAt DEFAULT SYSUTCDATETIME(),
        Notified BIT NOT NULL CONSTRAINT DF_Reservations_Notified DEFAULT 0,
        CONSTRAINT FK_Reservations_Members FOREIGN KEY(MemberId) REFERENCES dbo.Members(MemberId) ON DELETE NO ACTION,
        CONSTRAINT FK_Reservations_Copies FOREIGN KEY(CopyId) REFERENCES dbo.Copies(CopyId) ON DELETE NO ACTION
    );
END
GO

-- 3) Loans Audit Tetikleyicisi
IF OBJECT_ID('dbo.tr_loans_audit', 'TR') IS NOT NULL
    DROP TRIGGER dbo.tr_loans_audit;
GO
CREATE TRIGGER dbo.tr_loans_audit
ON dbo.Loans
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    -- Türkçe Açıklama: Yeni ödünç (INSERT) durumunda BORROW, iade edilen (UPDATE ve ReturnedAt dolu) satırlar için RETURN kaydı oluştur.

    -- Insert edilen kayıtlar BORROW olarak loglansın
    INSERT INTO dbo.Audit_Log(LoanId, Action)
    SELECT i.LoanId, N'BORROW'
    FROM inserted i
    LEFT JOIN deleted d ON d.LoanId = i.LoanId
    WHERE d.LoanId IS NULL; -- yalnızca insert

    -- Update ile iade edilenler RETURN olarak loglansın
    INSERT INTO dbo.Audit_Log(LoanId, Action)
    SELECT i.LoanId, N'RETURN'
    FROM inserted i
    INNER JOIN deleted d ON d.LoanId = i.LoanId
    WHERE d.ReturnedAt IS NULL AND i.ReturnedAt IS NOT NULL;
END
GO



