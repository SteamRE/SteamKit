#!/bin/bash -x

function ExitIfNonZero {
	if [ $1 -ne 0 ]; then
		exit $1
	fi
}

xbuild /p:NoWarn=1584 SteamKit2/SteamKit2.sln /target:SteamKit2 /target:Tests
ExitIfNonZero $?

xbuild Samples/Samples.sln
ExitIfNonZero $?
