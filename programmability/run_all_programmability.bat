@echo off
echo ============================================
echo TUM PROGRAMMABILITY OBJELERINI OLUSTURMA
echo ============================================
echo.

REM Veritabanı bilgilerini buraya girin
set SERVER=DESKTOP-D00J96T\SQLEXPRESS
set DATABASE=KutuphaneOtomasyonu
set SCRIPT_DIR=%~dp0

echo Veritabanı: %DATABASE%
echo Server: %SERVER%
echo.

REM Dosyaları sırayla çalıştır (önemli: sıralama kritik)
echo [1/4] Trigger ve tablolar oluşturuluyor...
sqlcmd -S %SERVER% -d %DATABASE% -E -i "%SCRIPT_DIR%triggers.sql"
if %ERRORLEVEL% NEQ 0 goto :error

echo [2/4] View'lar oluşturuluyor...
sqlcmd -S %SERVER% -d %DATABASE% -E -i "%SCRIPT_DIR%views.sql"
if %ERRORLEVEL% NEQ 0 goto :error

echo [3/4] Function oluşturuluyor...
sqlcmd -S %SERVER% -d %DATABASE% -E -i "%SCRIPT_DIR%functions.sql"
if %ERRORLEVEL% NEQ 0 goto :error

echo [4/5] Stored Procedure'lar oluşturuluyor...
sqlcmd -S %SERVER% -d %DATABASE% -E -i "%SCRIPT_DIR%sp.sql"
if %ERRORLEVEL% NEQ 0 goto :error

echo [5/5] Performans indeksleri oluşturuluyor...
sqlcmd -S %SERVER% -d %DATABASE% -E -i "%SCRIPT_DIR%indexes.sql"
if %ERRORLEVEL% NEQ 0 goto :error

echo.
echo ============================================
echo BASARIYLA TAMAMLANDI!
echo ============================================
echo Oluşturulan objeler:
echo   - 2 VIEW (vw_active_loans, vw_top_books_last30)
echo   - 1 FUNCTION (fn_top_books_between)
echo   - 1 TRIGGER (tr_loans_audit)
echo   - 2 STORED PROCEDURE (sp_borrow_copy, sp_return_copy)
echo   - Performans indeksleri (20+ indeks)
echo ============================================
pause
exit /b 0

:error
echo.
echo ============================================
echo HATA: Islem basarisiz oldu!
echo ============================================
pause
exit /b 1

