#define WIN32_LEAN_AND_MEAN
#include <windows.h>

#include "logger.h"
#include "crypto.h"
#include "net.h"

#include "nh2_string.h"

#include "steammessages_base.pb.h"

CLogger *g_pLogger = NULL;
CCrypto* g_pCrypto = NULL;
NetHook::CNet *g_pNet = NULL;

BOOL g_bOwnsConsole = FALSE;

BOOL IsRunDll32()
{
	char szMainModulePath[MAX_PATH];
	DWORD dwMainModulePathLength = GetModuleFileNameA(NULL, szMainModulePath, sizeof(szMainModulePath));

	return stringCaseInsensitiveEndsWith(szMainModulePath, "\\rundll32.exe");
}

BOOL WINAPI DllMain( HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved )
{
	if (IsRunDll32())
	{
		return TRUE;
	}

	if ( fdwReason == DLL_PROCESS_ATTACH )
	{
		GOOGLE_PROTOBUF_VERIFY_VERSION;

		g_bOwnsConsole = AllocConsole();

		LoadLibrary( "steamclient.dll" );

		g_pLogger = new CLogger();

		g_pCrypto = new CCrypto();
		g_pNet = new NetHook::CNet();

	}
	else if ( fdwReason == DLL_PROCESS_DETACH )
	{
		delete g_pNet;
		delete g_pCrypto;

		delete g_pLogger;

		if (g_bOwnsConsole)
		{
			FreeConsole();
		}
	}

	return TRUE;
}
