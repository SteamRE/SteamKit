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
$ProtoBase = Join-Path $PSScriptRoot '..\Protobufs'
$SK2Base = Join-Path $PSScriptRoot '..\..\SteamKit2\SteamKit2\Base\Generated'
$ModelParams = @('--model', 'SteamKit2.Internal.SteamKitProtobufModel')

& dotnet build --configuration Debug $ProtoGenSrc

Push-Location

$failed = @()

$protos = Import-Csv -LiteralPath (Join-Path $PSScriptRoot 'protos.csv') |
    Where-Object { (!$ProtoDir) -or ($_.ProtoDir -in $ProtoDir)}

# one-off
Set-Location -LiteralPath $PSScriptRoot
$params = $CommonParams + $ModelParams + @(
    '--proto', "gc.proto",
    '--output', (Join-Path $SK2Base 'GC\MsgBaseGC.cs'),
    '--namespace', "SteamKit2.GC.Internal",
    '--model-all-messages'
    )

& dotnet $ProtoGenDll $params > $null
if ($LASTEXITCODE -ne 0) { $failed += 'gc.proto' }

# protobuf dumper descriptor
Set-Location -LiteralPath (Join-Path $ProtoBase 'steam\google\protobuf')
$params = $CommonParams + @(
    '--proto', "descriptor.proto",
    '--output', (Join-Path $PSScriptRoot '..\ProtobufDumper\ProtobufDumper\Descriptor.cs'),
    '--namespace', "google.protobuf"
    )

#& dotnet $ProtoGenDll $params > $null

$protos | % {
    Set-Location -LiteralPath (Join-Path $ProtoBase $_.ProtoDir)
    $params = $CommonParams + $ModelParams + @(
        '--proto', $_.ProtoFileName
        '--output', (Join-Path $SK2Base $_.ClassFilePath),
        '--namespace', $_.Namespace
    )

    # 'all' registers every top-level message on the model, 'services' only the service request and response types
    if ($_.ModelRoots -eq 'all') {
        $params += '--model-all-messages'
    }

    & dotnet $ProtoGenDll -- $params > $null
    if ($LASTEXITCODE -ne 0) { $failed += "$($_.ProtoDir)/$($_.ProtoFileName)" }

    $_
}

Pop-Location

# A proto that fails to generate keeps its previous generated code
if ($failed.Count -gt 0) {
    Write-Error "Failed to generate: $($failed -join ', ')"
    exit 1
}
