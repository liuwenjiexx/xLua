call windows.bat
set BUILD_PATH=build64_54

if exist "%BUILD_PATH%" (
    rmdir /q/s "%BUILD_PATH%"
)
mkdir %BUILD_PATH% & pushd %BUILD_PATH%

cmake -DLUA_VERSION=5.4.1 -G %VS_VERSION% ..
popd
cmake --build "%BUILD_PATH%" --config Release
md plugin_lua54\Plugins\x86_64
copy /Y %BUILD_PATH%\Release\xlua.dll plugin_lua54\Plugins\x86_64\xlua.dll

set BUILD_PATH=build32_54

if exist "%BUILD_PATH%" (
    rmdir /q/s "%BUILD_PATH%"
)
mkdir %BUILD_PATH% & pushd %BUILD_PATH%
cmake -DLUA_VERSION=5.4.1 -G %VS_VERSION% -A Win32 ..
popd
cmake --build %BUILD_PATH% --config Release
md plugin_lua54\Plugins\x86
copy /Y %BUILD_PATH%\Release\xlua.dll plugin_lua54\Plugins\x86\xlua.dll

pause