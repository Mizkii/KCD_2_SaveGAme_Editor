@echo off
setlocal enabledelayedexpansion

echo ========================================================
echo  Kingdom Come: Deliverance 2 - SaveGame Editor Builder
echo ========================================================
echo.

:: Detect CMake
where cmake >nul 2>nul
if %ERRORLEVEL% equ 0 (
    set CMAKE_BIN=cmake
) else (
    if exist "C:\Program Files\Microsoft Visual Studio\18\Insiders\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe" (
        set CMAKE_BIN="C:\Program Files\Microsoft Visual Studio\18\Insiders\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe"
    ) else if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe" (
        set CMAKE_BIN="C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe"
    ) else if exist "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe" (
        set CMAKE_BIN="C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe"
    ) else (
        echo [-] Error: CMake could not be found in PATH or standard Visual Studio locations!
        echo Please ensure CMake is installed and added to PATH.
        exit /b 1
    )
)

echo [*] 1/3: Configuring and building C++ Engine (KCD2SaveCore.dll)...
if not exist "build" mkdir build
%CMAKE_BIN% -B build -S . -DCMAKE_BUILD_TYPE=Release -A x64
if %ERRORLEVEL% neq 0 (
    echo [-] CMake configuration failed!
    exit /b %ERRORLEVEL%
)

%CMAKE_BIN% --build build --config Release --target KCD2SaveCore
if %ERRORLEVEL% neq 0 (
    echo [-] C++ Core build failed!
    exit /b %ERRORLEVEL%
)

echo.
echo [*] 2/3: Building C# UI (.NET 10 WPF)...
dotnet build src/ui/KCD2SaveEditor.UI.csproj -c Release
if %ERRORLEVEL% neq 0 (
    echo [-] C# UI build failed!
    exit /b %ERRORLEVEL%
)

echo.
echo [*] 3/3: Bundling KCD2SaveCore.dll with UI...
if exist "build\bin\Release\KCD2SaveCore.dll" (
    if not exist "src\ui\bin\Release\net10.0-windows" mkdir "src\ui\bin\Release\net10.0-windows"
    copy /Y "build\bin\Release\KCD2SaveCore.dll" "src\ui\bin\Release\net10.0-windows\KCD2SaveCore.dll" >nul
)

echo.
echo ========================================================
echo  BUILD SUCCESSFUL!
echo  Run UI: src\ui\bin\Release\net10.0-windows\KCD2SaveEditor.UI.exe
echo ========================================================
