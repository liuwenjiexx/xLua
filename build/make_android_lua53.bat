set ANDROID_ABI=armeabi-v7a
set OUTPUT=build_android_%ANDROID_ABI%
set LUA_VERSION=5.3.5
set LUA_VERSION_Name=53

call windows_build_android.bat armeabi-v7a,%LUA_VERSION%
call windows_build_android.bat arm64-v8a,%LUA_VERSION%
call windows_build_android.bat x86,%LUA_VERSION%

pause