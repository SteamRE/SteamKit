@echo off

for /f "usebackq tokens=*" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath`) do (
  set InstallDir=%%i
)

if exist "%InstallDir%\Common7\Tools\vsdevcmd.bat" (
  call "%InstallDir%\Common7\Tools\vsdevcmd.bat"
)

cmake -S native-dependencies\zlib-1.2.11 -B native-dependencies\zlib-bins -A Win32
msbuild native-dependencies\zlib-bins\ALL_BUILD.vcxproj /p:Configuration=Release

devenv /upgrade native-dependencies\protobuf-2.5.0\vsprojects\protobuf.sln
msbuild native-dependencies\protobuf-2.5.0\vsprojects\protobuf.sln


