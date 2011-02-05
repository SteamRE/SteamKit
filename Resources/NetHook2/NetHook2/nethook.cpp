
#include <windows.h>

#include "logger.h"
#include "crypto.h"

#include "steammessages_base.pb.h"

CLogger *g_pLogger = NULL;
CCrypto* g_pCrypto = NULL;

BOOL WINAPI DllMain( HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved )
{
	if ( fdwReason == DLL_PROCESS_ATTACH )
	{
		GOOGLE_PROTOBUF_VERIFY_VERSION;

		AllocConsole();

		LoadLibrary( "steamclient.dll" );

		g_pLogger = new CLogger();

		g_pCrypto = new CCrypto();

	}
	else if ( fdwReason == DLL_PROCESS_DETACH )
	{
		delete g_pLogger;

		delete g_pCrypto;
	}

	return TRUE;
}
