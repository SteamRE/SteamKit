<#
.SYNOPSIS
    Generate C# proto code from protobufs for SteamKit
.DESCRIPTION
    For each file in protos.csv, run protogen to create or update corresponding
    C# code and classes for use in SteamKit
.PARAMETER ProtoDir
    Protobuf folders to process. (Default: all)
.EXAMPLE
    PS C:\> .\generate-all.ps1 -ProtoDir steam,tf2
#>
param([string[]]$ProtoDir)

$ProtoGenExe = Join-Path $PSScriptRoot 'protogen.exe'
$ProtoProcessedBase = Join-Path $PSScriptRoot 'ProcessedProtoBufs'
$ProtoBase = Join-Path $PSScriptRoot '..\ProtoBufs' -Resolve
$SK2Base = Join-Path $PSScriptRoot '..\..\SteamKit2\SteamKit2\Base\Generated' -Resolve

$protos = Import-Csv -LiteralPath (Join-Path $PSScriptRoot 'protos.csv') |
    Where-Object { (!$ProtoDir) -or ($_.ProtoDir -in $ProtoDir)}

if ( Test-Path $ProtoProcessedBase )
{
    Remove-Item -Recurse -Force $ProtoProcessedBase
}

# one-off
$params = @(
    '--proto_path="{0}"' -f $PSScriptRoot
    '--csharp_out="{0}"' -f (Join-Path $ProtoProcessedBase 'GC')
    '+langver=7.0'
    '"gc.proto"'
)

& $ProtoGenExe $params > $null

$protos | ForEach-Object {
    $InputProtoDir = Join-Path $ProtoBase $_.ProtoDir
    $OutputProtoFile = Join-Path $ProtoProcessedBase $_.ClassFilePath
    $OutputProtoDir = Split-Path -Path $OutputProtoFile
    $OutputProtoCompiled = Join-Path $OutputProtoDir $($_.ProtoFileName.Substring(0, $_.ProtoFileName.Length - 6) + ".cs")

    $params = @(
        '--proto_path="{0}"' -f $InputProtoDir
        '--csharp_out="{0}"' -f $OutputProtoDir
        '--package="{0}"' -f $_.Namespace
        '+langver=7.0'
        $_.ProtoFileName
    )

    & $ProtoGenExe $params

    Copy-Item $OutputProtoCompiled -Destination (Join-Path $SK2Base $_.ClassFilePath)

    Write-Output ""
}

Remove-Item -Recurse -Force $ProtoProcessedBase
