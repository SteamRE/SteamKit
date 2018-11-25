$ProtoGenDir = $PSScriptRoot
$ProtoGenExe = Join-Path $PSScriptRoot 'protogen.exe'
$ProtoBase = Join-Path $PSScriptRoot '..\ProtoBufs'
$SK2Base = Join-Path $PSScriptRoot '..\..\SteamKit2\SteamKit2\Base\Generated'

Push-Location

$protos = Import-Csv -LiteralPath (Join-Path $PSScriptRoot 'protos.csv')

$CommonParams = @(
    '-s:"{0}"' -f $ProtoGenDir,
    '-t:csharp'
    '-p:detectMissing'
)

# one-off
Set-Location -LiteralPath $PSScriptRoot
$params = $CommonParams + @(
    '-i:"gc.proto"'
    '-o:"{0}"' -f (Join-Path $SK2Base 'GC\MsgBaseGC.cs')
    '-ns:"SteamKit2.GC.Internal"'
)

& $ProtoGenExe $params > $null

$protos | % {
    Set-Location -LiteralPath (Join-Path $ProtoBase $_.ProtoDir)
    $params = $CommonParams + @(
        '-i:"{0}"' -f $_.ProtoFileName
        '-o:"{0}"' -f (Join-Path $SK2Base $_.ClassFilePath)
        '-ns:"{0}"' -f $_.Namespace
    )

    # Hack for friends.proto requiring other steam protos but being in separate dir
    if ($_.ProtoDir -eq 'webui')
    {
        $params += ('-s:"{0}"' -f (Join-Path $ProtoBase 'steam'))
    }

    & $ProtoGenExe $params > $null

    # Hack to work around protogen stderr messges not ending the line
    if ($LASTEXITCODE -ne 0)
    {
        Write-Host
    }

    $_
}

Pop-Location