#!/usr/bin/env sh
BASEDIR="$(dirname $0)"
cd "$BASEDIR"
dotnet ../SteamLanguageParser/bin/Release/netcoreapp2.0/SteamLanguageParser.dll ../..

