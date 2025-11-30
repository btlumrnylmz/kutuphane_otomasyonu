@echo off
echo ============================================
echo Stored Procedure Kurulum Scripti
echo ============================================
echo.

REM Veritabanı bilgilerini buraya girin
set SERVER=DESKTOP-D00J96T\SQLEXPRESS
set DATABASE=KutuphaneOtomasyonu
set SQL_FILE=%~dp0sp.sql

echo Veritabanı: %DATABASE%
echo Server: %SERVER%
echo SQL Dosyası: %SQL_FILE%
echo.

REM sqlcmd ile çalıştır
echo Stored procedure'lar oluşturuluyor...
sqlcmd -S %SERVER% -d %DATABASE% -E -i "%SQL_FILE%"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ============================================
    echo Basariyla tamamlandi!
    echo ============================================
) else (
    echo.
    echo ============================================
    echo HATA: Islem basarisiz oldu!
    echo ============================================
)

pause




