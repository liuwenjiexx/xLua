@echo off
call windows.bat
set OUTPUT=build_lj64
if exist %OUTPUT% (
  rmdir /Q/S %OUTPUT%
)

call "%VS_HOME%\VC\Auxiliary\Build\vcvars64.bat"

echo Swtich to x64 build env
cd %~dp0\luajit-2.1.0b3\src
call msvcbuild_mt.bat static
cd ..\..

mkdir %OUTPUT% & pushd %OUTPUT%
cmake -DUSING_LUAJIT=ON -G "%VS_VERSION%" ..
IF %ERRORLEVEL% NEQ 0 cmake -DUSING_LUAJIT=ON -G "%VS_VERSION%" ..
popd
cmake --build %OUTPUT% --config Release
md plugin_luajit\Plugins\x86_64
copy /Y %OUTPUT%\Release\xlua.dll plugin_luajit\Plugins\x86_64\xlua.dll
pause