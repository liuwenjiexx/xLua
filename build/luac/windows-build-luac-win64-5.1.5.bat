set BUILD_OUTPUT=build64
set LUA_VERSION=5.1.5

call windows_build_luac_win64.bat

@REM cd %LUA_VERSION%

@REM if exist %BUILD_OUTPUT% (
@REM   rmdir /Q/S %BUILD_OUTPUT%   
@REM )

@REM mkdir %BUILD_OUTPUT% & pushd %BUILD_OUTPUT%

@REM cmake -DLUAC_COMPATIBLE_FORMAT=ON -G "Visual Studio 16 2019" .. -DLUA_VERSION=%LUA_VERSION%
@REM pause
@REM IF %ERRORLEVEL% NEQ 0 cmake -DLUAC_COMPATIBLE_FORMAT=ON -G "Visual Studio 15 2017 Win64" .. -DLUA_VERSION=%LUA_VERSION%
@REM popd

@REM cmake --build %BUILD_OUTPUT% --config Release

@REM pause