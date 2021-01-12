#!/usr/bin/env sh
BASEDIR="$(dirname $0)"
cd "$BASEDIR"
cd ../SteamLanguageParser
dotnet run ../../
