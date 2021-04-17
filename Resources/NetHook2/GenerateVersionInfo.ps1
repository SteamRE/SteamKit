$ErrorActionPreference = 'stop'

Function Test-GitAvailable {
    Try {
        If (Get-Command git) {
            return $true
        }
    }
    Catch {
    }
    return $false
}

$buildDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss K"

if (Test-GitAvailable) {
    $commitDate = [string](git show -s --format="%ci" HEAD)
    $sha = [string](git rev-parse --short HEAD)

    git diff --quiet
    $dirty = [bool]($LASTEXITCODE)
} else {
    $dirty = $false
}

$versionFile = 'version.cpp'

$sb = New-Object Text.StringBuilder
[void]$sb.AppendLine("#include `"version.h`"")
[void]$sb.AppendLine()
[void]$sb.AppendLine("const char *g_szBuildDate = `"$($buildDate)`";")
[void]$sb.AppendLine("const char *g_szBuiltFromCommitSha = `"$($sha)`";")
[void]$sb.AppendLine("const char *g_szBuiltFromCommitDate = `"$($commitDate)`";")
[void]$sb.AppendLine("const BOOL g_bBuiltFromDirty = $($dirty.ToString().ToLower());")

Set-Content -Path $versionFile -Value $sb.ToString()
