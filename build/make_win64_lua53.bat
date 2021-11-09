call windows.bat
set OUTPUT=build64

if exist %OUTPUT% (
  rmdir /Q/S %OUTPUT%
)

mkdir %OUTPUT% & pushd %OUTPUT%
cmake -G %VS_VERSION% ..
popd
cmake --build %OUTPUT% --config Release
md plugin_lua53\Plugins\x86_64
copy /Y %OUTPUT%\Release\xlua.dll plugin_lua53\Plugins\x86_64\xlua.dll
pause