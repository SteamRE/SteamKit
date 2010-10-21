#include <winsock2.h>
#include <windows.h>

#include "clientapp.h"

#include "utils.h"
#include "logger.h"
#include "crypto.h"
#include "DataDumper.h"

#include "interface.h"

#define STEAMTYPES_H
#include "usercommon.h"
#include "ESteamError.h"
#include "isteamclient009.h"
#include "isteamgameserver010.h"
#include "steammessages_base.pb.h"

CDataDumper* g_Dumper;

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
	if ( fdwReason == DLL_PROCESS_ATTACH )
	{
		GOOGLE_PROTOBUF_VERIFY_VERSION;

		AllocConsole();
		LoadLibrary("steamclient.dll");	

		g_Logger = new CLogger(".\\");
		g_Dumper = new CDataDumper();
		g_Crypto = new CCrypto(g_Dumper);
	}
	else if ( fdwReason == DLL_PROCESS_DETACH )
	{
		delete g_Crypto;
		delete g_Dumper;
		delete g_Logger;
	}

	return TRUE;
}
