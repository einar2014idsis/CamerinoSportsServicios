@echo off
setlocal
title Camerino Sports - Servidor Motorola por WiFi

set "ADB=%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe"
set "DEVICE="
set "USB_DEVICE="
set "PHONE_IP="

if not exist "%ADB%" (
    echo ERROR: No se encontro ADB.
    pause
    exit /b 1
)

echo Verificando la conexion WiFi del Motorola...
"%ADB%" start-server >nul 2>&1

for /f "tokens=1,2" %%A in ('"%ADB%" devices ^| findstr ":5555"') do (
    if "%%B"=="device" set "DEVICE=%%A"
)

if not defined DEVICE (
    echo La conexion WiFi necesita prepararse una vez por USB...

    for /f "tokens=1,2" %%A in ('"%ADB%" devices ^| findstr /v "List emulator :5555"') do (
        if "%%B"=="device" set "USB_DEVICE=%%A"
    )

    if not defined USB_DEVICE (
        echo ERROR: Conecta el Motorola por USB y acepta Depuracion USB.
        pause
        exit /b 1
    )

    for /f "tokens=9" %%I in ('"%ADB%" -s "%USB_DEVICE%" shell ip route ^| findstr "wlan0"') do (
        if not defined PHONE_IP set "PHONE_IP=%%I"
    )

    if not defined PHONE_IP (
        echo ERROR: El Motorola y la laptop deben estar conectados al mismo WiFi.
        pause
        exit /b 1
    )

    "%ADB%" -s "%USB_DEVICE%" tcpip 5555
    timeout /t 3 /nobreak >nul
    "%ADB%" connect %PHONE_IP%:5555
    set "DEVICE=%PHONE_IP%:5555"
)

echo Preparando el servidor por WiFi...
"%ADB%" -s "%DEVICE%" reverse --remove tcp:5110 >nul 2>&1
"%ADB%" -s "%DEVICE%" reverse tcp:5110 tcp:5110
if errorlevel 1 (
    echo ERROR: No se pudo crear la conexion WiFi con el puerto 5110.
    echo Conecta el cable una vez y vuelve a ejecutar este archivo.
    pause
    exit /b 1
)

echo Motorola conectado por WiFi: %DEVICE%
echo Ya puedes desconectar el cable USB.
echo No cierres esta ventana mientras uses la aplicacion.
echo.

echo Iniciando Camerino Sports Servicios...
cd /d "%~dp0CamerinoSportsServicios"
dotnet run --launch-profile http

echo.
echo El servidor se ha detenido. La aplicacion no podra conectarse.
pause
