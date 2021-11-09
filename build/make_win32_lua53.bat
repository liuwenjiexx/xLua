call windows.bat
set BUILD_PATH=build32
if exist "%BUILD_PATH%" (
    rmdir /q/s "%BUILD_PATH%"
)
mkdir "%BUILD_PATH%" & pushd "%BUILD_PATH%"
cmake -G %VS_VERSION% -A Win32 ..
popd
cmake --build "%BUILD_PATH%" --config Release
md plugin_lua53\Plugins\x86
copy /Y %BUILD_PATH%\Release\xlua.dll plugin_lua53\Plugins\x86\xlua.dll
pause