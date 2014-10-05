$NetHook2DependenciesTemporaryDirectory = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "nethook2-dependencies")
$ZLibSourceZipUrl = "http://zlib.net/zlib128.zip"
$ZLibSourceFile = [System.IO.Path]::Combine($NetHook2DependenciesTemporaryDirectory, "zlib.zip")
$ZLibSourceInnerFolderName = "zlib-1.2.8"
$ProtobufSourceZipUrl = "https://protobuf.googlecode.com/files/protobuf-2.5.0.zip"
$ProtobufSourceFile = [System.IO.Path]::Combine($NetHook2DependenciesTemporaryDirectory, "protobuf.zip")
$ProtobufSourceInnerFolderName = "protobuf-2.5.0"

if (Test-Path $NetHook2DependenciesTemporaryDirectory)
{
	Remove-Item -Recurse -Force -Path $NetHook2DependenciesTemporaryDirectory 
}

New-Item -Path $NetHook2DependenciesTemporaryDirectory -Type Directory

Write-Host Loading System.IO.Compression...
[System.Reflection.Assembly]::LoadWithPartialName("System.IO.Compression")
[System.Reflection.Assembly]::LoadWithPartialName("System.IO.Compression.FileSystem")

Write-Host Downloading ZLib headers...
Invoke-WebRequest $ZLibSourceZipUrl -OutFile $ZLibSourceFile

Write-Host Extracting ZLib...
$zip = [System.IO.Compression.ZipFile]::Open($ZLibSourceFile, [System.IO.Compression.ZipArchiveMode]::Read)
[System.IO.Compression.ZipFileExtensions]::ExtractToDirectory($zip, $NetHook2DependenciesTemporaryDirectory)
$zip.Dispose()

Write-Host Moving ZLib into place...
$zlibPath = [System.IO.Path]::Combine($NetHook2DependenciesTemporaryDirectory, $ZLibSourceInnerFolderName)
Copy-Item $zlibPath "NetHook2\zlib" -Force -Recurse

Write-Host Downloading Google Protobuf Headers...
Invoke-WebRequest $ProtobufSourceZipUrl -OutFile $ProtobufSourceFile
Write-Host Extracting Protobuf...
$zip = [System.IO.Compression.ZipFile]::Open($ProtobufSourceFile, [System.IO.Compression.ZipArchiveMode]::Read)
[System.IO.Compression.ZipFileExtensions]::ExtractToDirectory($zip, $NetHook2DependenciesTemporaryDirectory)
$zip.Dispose()

Write-Host Moving Protobuf into place...
$protoPath = [System.IO.Path]::Combine($NetHook2DependenciesTemporaryDirectory, $ProtobufSourceInnerFolderName, "src", "google")
Copy-Item $protoPath "NetHook2\google" -Force -Recurse

Write-Host Cleaning up...
Remove-Item -Recurse -Force -Path $NetHook2DependenciesTemporaryDirectory 
