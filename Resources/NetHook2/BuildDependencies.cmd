@echo off

for /f "usebackq tokens=*" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath`) do (
  set InstallDir=%%i
)

if exist "%InstallDir%\Common7\Tools\vsdevcmd.bat" (
  call "%InstallDir%\Common7\Tools\vsdevcmd.bat"
)

cmake -S native-dependencies\zlib-1.2.11 -B native-dependencies\zlib-bins -A Win32
msbuild native-dependencies\zlib-bins\ALL_BUILD.vcxproj /p:Configuration=Release

if not exist "native-dependencies\protobuf-2.5.0\vsprojects\libprotobuf.vcxproj" (
  devenv /upgrade native-dependencies\protobuf-2.5.0\vsprojects\protobuf.sln
)

if not exist "native-dependencies\protobuf-2.5.0\vsprojects\Directory.Build.props" (
		copy /y protobuf-Directory.Build.props native-dependencies\protobuf-2.5.0\vsprojects\Directory.Build.props
)
msbuild native-dependencies\protobuf-2.5.0\vsprojects\protobuf.sln /p:Configuration=Debug
msbuild native-dependencies\protobuf-2.5.0\vsprojects\protobuf.sln /p:Configuration=Release


