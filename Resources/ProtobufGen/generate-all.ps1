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

$ProtoGenSrc = Join-Path $PSScriptRoot 'ProtobufGen'
$ProtoGenDll = Join-Path $ProtoGenSrc '\bin\Debug\ProtobufGen.dll'
$ProtoBase = Join-Path $PSScriptRoot '..\ProtoBufs'
$SK2Base = Join-Path $PSScriptRoot '..\..\SteamKit2\SteamKit2\Base\Generated'

& dotnet build --configuration Debug $ProtoGenSrc

Push-Location

$protos = Import-Csv -LiteralPath (Join-Path $PSScriptRoot 'protos.csv') |
    Where-Object { (!$ProtoDir) -or ($_.ProtoDir -in $ProtoDir)}

# one-off
Set-Location -LiteralPath $PSScriptRoot
$params = $CommonParams + @(
    '--proto', "gc.proto",
    '--output', (Join-Path $SK2Base 'GC\MsgBaseGC.cs'),
    '--namespace', "SteamKit2.GC.Internal"
    )

& dotnet $ProtoGenDll $params > $null


$protos | % {
    Set-Location -LiteralPath (Join-Path $ProtoBase $_.ProtoDir)
    $params = $CommonParams + @(
        '--proto', $_.ProtoFileName
        '--output', (Join-Path $SK2Base $_.ClassFilePath),
        '--namespace', $_.Namespace
    )

    & dotnet $ProtoGenDll -- $params > $null

    $_
}

Pop-Location
