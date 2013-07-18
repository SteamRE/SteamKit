#!/bin/sh -x

xbuild SteamKit2/SteamKit2.sln /target:SteamKit2 /target:Tests
BUILDS[0]=$?

xbuild Samples/Samples.sln /target:Sample1_Logon /target:Sample2_CallbackManager /target:Sample3_Extending /target:Sample4_DebugLog /target:Sample5_Friends /target:Sample6_SteamGuard
BUILDS[1]=$?

# These projects outcomes don't affect the build outcome
xbuild Resources/NetHookAnalyzer/NetHookAnalyzer.sln /target:NetHookAnalyzer
xbuild SteamKit2/SteamKit2.sln /target:SteamLanguageParser

# Test to see if string.Join(" ", builds) contains anything other than 0 and space
[[ ${BUILDS[@]} =~ ^[0\ ]*$ ]];
exit $?
