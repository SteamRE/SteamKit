#!/bin/bash
STEAM_PID=$(ps -C steam -o pid= | head -1)
LIB_PL="${PWD}/libtuxhookldr.so"

if [ -n "${STEAM_PID}" ]
    then
        echo "print (void*)__libc_dlopen_mode(\"${LIB_PL}\", 2)" | gdb -p $STEAM_PID
    else
        echo "Steam PID not found!"
fi
