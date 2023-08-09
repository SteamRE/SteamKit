@echo off

if not defined DevEnvDir (
    for /f "usebackq tokens=*" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath`) do (
        set InstallDir=%%i
    )

    if exist "%InstallDir%\Common7\Tools\vsdevcmd.bat" (
        call "%InstallDir%\Common7\Tools\vsdevcmd.bat"
    )
)

where.exe /q vcpkg
if %ERRORLEVEL%==1 (
    if defined CI (
        git clone --depth 1 "https://github.com/Microsoft/vcpkg.git"
        call ".\vcpkg\bootstrap-vcpkg.bat"
        vcpkg\vcpkg.exe integrate install
    ) else (
        echo "vcpkg is required but not found. Please see https://vcpkg.io/en/getting-started to install it
    )    
)

rem todo: compile or download protoc and generate steammessages_base.pb.{h|cpp}
