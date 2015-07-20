#!/bin/bash -x
set -e

xbuild /p:NoWarn=1584 SteamKit2/SteamKit2.sln /target:SteamKit2 /target:Tests
xbuild Samples/Samples.sln
