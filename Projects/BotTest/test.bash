#!/bin/bash
cd ../DotaBot/
xbuild DotaBot.sln
cd -
cd bin/Debug
mono BotTest.exe
cd -
