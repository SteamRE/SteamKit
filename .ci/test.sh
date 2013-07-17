#!/bin/sh -x

# install xunit runners
mono --runtime=v4.0 SteamKit2/.nuget/NuGet.exe install xunit.runners -Version 1.9.1 -o SteamKit2/packages

# run tests
mono --runtime=v4.0 SteamKit2/packages/xunit.runners.1.9.1/tools/xunit.console.exe SteamKit2/Tests/bin/Debug/Tests.dll

exit $?
