@echo off

set ANDROID_ABI=%1
set LUA_VERSION=%2
set ANDROID_NDK=X:\bin\Android\SDK\ndk\android-ndk-r10e
set cmake_version=3.6.4111459
set BUILD_PATH=build/lua-%LUA_VERSION%/android/%ANDROID_ABI%
set OUTPUT_PATH=.\output/lua-%LUA_VERSION%\android\%ANDROID_ABI%
set CMAKE=%ANDROID_SDK%\cmake\%cmake_version%\bin\cmake.exe
set NINJA=%ANDROID_SDK%\cmake\%cmake_version%\bin\ninja.exe

echo --- BUILD %LUA_VERSION% %ANDROID_ABI% Start ---
echo NDK: %ANDROID_NDK%
echo CMake: %cmake_version%
echo ABI: %ANDROID_ABI%
echo Output: %OUTPUT_PATH%
 
if exist "%BUILD_PATH%" (
  rmdir /Q/S "%BUILD_PATH%"
)
mkdir "%BUILD_PATH%"

%CMAKE% -H.\ -B.\%BUILD_PATH% "-GAndroid Gradle - Ninja" -DANDROID_ABI=%ANDROID_ABI% -DANDROID_NDK=%ANDROID_NDK% -DCMAKE_BUILD_TYPE=Relase -DCMAKE_MAKE_PROGRAM=%NINJA% -DCMAKE_TOOLCHAIN_FILE=./cmake/android.windows.toolchain.cmake "-DCMAKE_CXX_FLAGS=-std=c++11 -fexceptions"
%NINJA% -C .\%BUILD_PATH%

if not exist "%OUTPUT_PATH%" (mkdir "%OUTPUT_PATH%")

move .\%BUILD_PATH%\libxlua.so %OUTPUT_PATH%\libxlua.so
echo --- BUILD %LUA_VERSION% %ANDROID_ABI% End ---
