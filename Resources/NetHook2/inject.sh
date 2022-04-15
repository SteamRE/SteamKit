#!/bin/bash
STEAM_PID=$(ps -C steam -o pid= | head -1)
LIB_PL="${PWD}/libtuxhookldr.so"
INJECTOR="${PWD}/tuxjector"

if [ -n "${STEAM_PID}" ]
    then
		${INJECTOR} ${LIB_PL} ${STEAM_PID}
    else
        echo "Steam PID not found!"
fi
