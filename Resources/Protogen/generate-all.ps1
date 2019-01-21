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

$CommonParams = @(
    '+langver=7.0'
    '+names=original'
)

# one-off
$params = $CommonParams + @(
    '--proto_path="{0}"' -f $PSScriptRoot
    '--csharp_out="{0}"' -f (Join-Path $ProtoProcessedBase 'GC')
    '"gc.proto"'
)

& $ProtoGenExe $params > $null

Move-Item -Force -Path (Join-Path $ProtoProcessedBase 'GC\gc.cs') -Destination (Join-Path $SK2Base 'GC\MsgBaseGC.cs')

$protos | ForEach-Object {
    $InputProtoDir = Join-Path $ProtoBase $_.ProtoDir
    $OutputProtoFile = Join-Path $ProtoProcessedBase $_.ClassFilePath
    $OutputProtoDir = Split-Path -Path $OutputProtoFile
    $OutputProtoCompiled = Join-Path $OutputProtoDir $($_.ProtoFileName.Substring(0, $_.ProtoFileName.Length - 6) + ".cs")

    $params = $CommonParams + @(
        '--proto_path="{0}"' -f $InputProtoDir
        '--csharp_out="{0}"' -f $OutputProtoDir
        '--package="{0}"' -f $_.Namespace
        $_.ProtoFileName
    )

    & $ProtoGenExe $params > $null

    Move-Item -Force -Path $OutputProtoCompiled -Destination (Join-Path $SK2Base $_.ClassFilePath)

    $_
}
