#!/bin/bash
STEAM_PID=$(pidof steam | xargs -n1 | sort -g | head -1)
LIB_PL="${PWD}/libtuxhookldr.so"
INJECTOR="${PWD}/tuxjector"

if [ -n "${1}" ]
	then
		${INJECTOR} ${LIB_PL} ${1}
elif [ -n "${STEAM_PID}" ]
	then
		${INJECTOR} ${LIB_PL} ${STEAM_PID}
else
	echo "Steam PID not found!"
fi
