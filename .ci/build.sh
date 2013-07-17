#!/bin/sh -x

xbuild SteamKit2/SteamKit2.sln /target:SteamKit2 /target:Tests

exit $?
