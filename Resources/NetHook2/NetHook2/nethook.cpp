
#include <windows.h>

#include "logger.h"
#include "crypto.h"

#include "steammessages_base.pb.h"

CLogger *g_pLogger = NULL;
CCrypto* g_pCrypto = NULL;
BOOL g_bOwnsConsole = FALSE;

BOOL IsRunDll32()
{
	char szMainModulePath[MAX_PATH];
	DWORD dwMainModulePathLength = GetModuleFileNameA(NULL, szMainModulePath, sizeof(szMainModulePath));
	const char * szEndsWithKey = "\\rundll32.exe";
	unsigned int szEndsWithKeyLength = strlen(szEndsWithKey);
	if (dwMainModulePathLength > szEndsWithKeyLength && _stricmp(szMainModulePath + dwMainModulePathLength - szEndsWithKeyLength, szEndsWithKey) == 0)
	{
		return TRUE;
	}

	return FALSE;
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

	}
	else if ( fdwReason == DLL_PROCESS_DETACH )
	{
		delete g_pLogger;

		delete g_pCrypto;

		if (g_bOwnsConsole)
		{
			FreeConsole();
		}
	}

	return TRUE;
}
