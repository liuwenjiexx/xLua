
echo %LUA_VERSION%

cd %LUA_VERSION%

if exist %BUILD_OUTPUT% (
  rmdir /Q/S %BUILD_OUTPUT%   
)

mkdir %BUILD_OUTPUT% & pushd %BUILD_OUTPUT%

cmake -DLUAC_COMPATIBLE_FORMAT=ON -G "Visual Studio 16 2019" .. -DLUA_VERSION=%LUA_VERSION%
IF %ERRORLEVEL% NEQ 0 cmake -DLUAC_COMPATIBLE_FORMAT=ON -G "Visual Studio 15 2017 Win64" .. -DLUA_VERSION=%LUA_VERSION%
popd

cmake --build %BUILD_OUTPUT% --config Release
pause