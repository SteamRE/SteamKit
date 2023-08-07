@echo off

for /f "usebackq tokens=*" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath`) do (
  set InstallDir=%%i
)

if exist "%InstallDir%\Common7\Tools\vsdevcmd.bat" (
  call "%InstallDir%\Common7\Tools\vsdevcmd.bat"
)

where /q vcpkg
if ERRORLEVEL 1 (
    if not exist vcpkg/ (
        git clone https://github.com/Microsoft/vcpkg.git
    ) else (
        cd vcpkg
        git pull
        cd ..
    )

    .\vcpkg\bootstrap-vcpkg.bat
    SET PATH=%PATH%;%CD%\vcpkg
)

vcpkg integrate project

rem todo: compile or download protoc and generate steammessages_base.pb.{h|cpp}
