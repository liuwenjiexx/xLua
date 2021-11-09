@echo off
call windows.bat
call "%VS_HOME%\VC\Auxiliary\Build\vcvars32.bat"

echo Swtich to x86 build env
cd %~dp0\luajit-2.1.0b3\src
call msvcbuild_mt.bat static
cd ..\..
set BUILD_PATH=build_lj32
if exist "%BUILD_PATH%" (
    rmdir /q/s "%BUILD_PATH%"
)
mkdir "%BUILD_PATH%" & pushd "%BUILD_PATH%"
cmake -DUSING_LUAJIT=ON -G %VS_VERSION% -A Win32 ..
IF %ERRORLEVEL% NEQ 0 cmake -DUSING_LUAJIT=ON -G "Visual Studio 15 2017" ..
popd
cmake --build "%BUILD_PATH%" --config Release
md plugin_luajit\Plugins\x86
copy /Y %BUILD_PATH%\Release\xlua.dll plugin_luajit\Plugins\x86\xlua.dll
pause