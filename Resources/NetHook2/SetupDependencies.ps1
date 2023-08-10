if (!$env:DevEnvDir) {
    $vsWhere = Join-Path -Path ${env:ProgramFiles(x86)} -ChildPath 'Microsoft Visual Studio\Installer\vswhere.exe'
    $vsInstallDir = (. $vsWhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath) |
        Select-Object -First 1

    $modulePath = Join-Path -Path $vsInstallDir -ChildPath 'Common7\Tools\Microsoft.VisualStudio.DevShell.dll'
    if (Test-Path -Path $modulePath -PathType Leaf) {
        Import-Module $modulePath
    } else {
        throw "Failed to location Visual Studio PowerShell module"
    }

    Enter-VsDevShell -VsInstallPath $vsInstallDir
}

if ($env:VCPKG_ROOT)
{
    $vcpkgTmp = Join-Path -Path $env:VCPKG_ROOT -ChildPath 'vcpkg.exe'
    if (Test-Path -Path $vcpkgTmp -PathType Leaf) {
        $vcpkg = $vcpkgTmp
    }
}

if (!$vcpkg) {
    $vcpkg = Get-Command -Name vcpkg -CommandType Application -ErrorAction SilentlyContinue |
            Select-Object -First 1 |
            Select-Object -ExpandProperty Source
    if ($vcpkg) {
        Write-Host "Found vcpkg at '$vcpkg' from PATH"
    } else {
        if ($env:CI) {
            Write-Host "Downloading vcpkg"
            $vcpkgDir = Join-Path -Path (Get-Location.Path) -ChildPath 'vcpkg'
            if (!(Test-Path -Path $vcpkgDir)) {
                git clone --depth 1 "https://github.com/Microsoft/vcpkg.git"
            } else {
                Set-Location vcpkg
                git pull
                Set-Location ..
            }
    
            .\vcpkg\bootstrap-vcpkg.bat
    
            $env:Path = "PATH=$vcpkgDir;$env:Path"
        }
        else {
            Write-Warning "vcpkg is required but not found. Please see https://vcpkg.io/en/getting-started to install it"
        }
    }
}
else {
    Write-Host "Found vcpkg at '$vcpkg' from VCPKG_ROOT"
}

# DO need to call this
. $vcpkg integrate install

# todo: compile or download protoc and generate steammessages_base.pb.{h|cpp}
