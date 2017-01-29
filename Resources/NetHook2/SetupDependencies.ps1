$NetHook2DependenciesTemporaryDirectory = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "nethook2-dependencies")
$ZLibSourceZipUrl = "http://zlib.net/zlib1211.zip"
$ZLibSourceFile = [System.IO.Path]::Combine($NetHook2DependenciesTemporaryDirectory, "zlib.zip")
$ZLibSourceInnerFolderName = "zlib-1.2.11"
$ProtobufSourceZipUrl = "https://github.com/google/protobuf/releases/download/v2.5.0/protobuf-2.5.0.zip"
$ProtobufSourceFile = [System.IO.Path]::Combine($NetHook2DependenciesTemporaryDirectory, "protobuf.zip")
$ProtobufSourceInnerFolderName = "protobuf-2.5.0"

Set-Location $PSScriptRoot

if (-Not (Test-Path $NetHook2DependenciesTemporaryDirectory))
{
    New-Item -Path $NetHook2DependenciesTemporaryDirectory -Type Directory | Out-Null
}

Write-Host Loading System.IO.Compression...
[Reflection.Assembly]::LoadWithPartialName("System.IO.Compression")
[Reflection.Assembly]::LoadWithPartialName("System.IO.Compression.FileSystem")

$ZLibFolderPath = [IO.Path]::Combine($NetHook2DependenciesTemporaryDirectory, $ZLibSourceInnerFolderName)
if (-Not (Test-Path $ZLibFolderPath))
{
    if (-Not (Test-Path $ZLibSourceFile))
    {
        Write-Host Downloading ZLib headers...
        Invoke-WebRequest $ZLibSourceZipUrl -OutFile $ZLibSourceFile
    }

    Write-Host Extracting ZLib...
    $zip = [IO.Compression.ZipFile]::Open($ZLibSourceFile, [System.IO.Compression.ZipArchiveMode]::Read)
    [IO.Compression.ZipFileExtensions]::ExtractToDirectory($zip, $NetHook2DependenciesTemporaryDirectory)
    $zip.Dispose()
}

Write-Host Copying ZLib into place...
Copy-Item $ZLibFolderPath "NetHook2\zlib" -Force -Recurse

$ProtobufFolderPath = [IO.Path]::Combine($NetHook2DependenciesTemporaryDirectory, $ProtobufSourceInnerFolderName)
if (-Not (Test-Path $ProtobufFolderPath))
{
    if (-Not (Test-Path $ProtobufSourceFile))
    {
        Write-Host Downloading Google Protobuf Headers...
        Invoke-WebRequest $ProtobufSourceZipUrl -OutFile $ProtobufSourceFile
    }

    Write-Host Extracting Protobuf...
    $zip = [IO.Compression.ZipFile]::Open($ProtobufSourceFile, [System.IO.Compression.ZipArchiveMode]::Read)
    [IO.Compression.ZipFileExtensions]::ExtractToDirectory($zip, $NetHook2DependenciesTemporaryDirectory)
    $zip.Dispose()
}

Write-Host Copying Protobuf into place...
$ProtobufFolderPath = [IO.Path]::Combine($NetHook2DependenciesTemporaryDirectory, $ProtobufSourceInnerFolderName, "src", "google")
Copy-Item $ProtobufFolderPath "NetHook2\google" -Force -Recurse
