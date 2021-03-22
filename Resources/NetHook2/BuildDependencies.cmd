@echo off

for /f "usebackq tokens=*" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath`) do (
  set InstallDir=%%i
)

if exist "%InstallDir%\Common7\Tools\vsdevcmd.bat" (
  call "%InstallDir%\Common7\Tools\vsdevcmd.bat"
)

if not exist "native-dependencies\zlib-bins\ALL_BUILD.vcxproj" (
  cmake -S native-dependencies\zlib-1.2.11 -B native-dependencies\zlib-bins -A Win32
)

msbuild native-dependencies\zlib-bins\ALL_BUILD.vcxproj /p:Configuration=Release

if not exist "native-dependencies\protobuf-bins\libprotobuf.vcxproj" (
  cmake -S native-dependencies\protobuf-3.15.6\cmake -B native-dependencies\protobuf-bins -A Win32 -Dprotobuf_MSVC_STATIC_RUNTIME=OFF
)

msbuild native-dependencies\protobuf-bins\libprotobuf.vcxproj /p:Configuration=Debug
msbuild native-dependencies\protobuf-bins\libprotobuf.vcxproj /p:Configuration=Release

rem todo: compile protoc and generate steammessages_base.pb.{h|cpp}



