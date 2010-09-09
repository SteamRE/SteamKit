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

// define our clientapp entry point
CLIENTAPP( main );

int main( int argc, char **argv )
{
	g_Logger = new CLogger(argv[0]);

	AllocConsole();
	LoadLibrary("steamclient.dll");		

	CDataDumper test;
	g_Crypto = new CCrypto(&test);

	// load the real client app
	HMODULE steamUI = LoadLibrary( "SteamUI.dll" );
	if ( !steamUI )
	{
		MessageBox( HWND_DESKTOP, "Unable to load SteamUI.", "Oops!", MB_OK );
		return -1;
	}

	SteamDllMainFn realSteamMain = ( SteamDllMainFn )GetProcAddress( steamUI, "SteamDllMain" );
	if ( !realSteamMain )
	{
		MessageBox( HWND_DESKTOP, "Unable to find Steam entrypoint.", "Oops!", MB_OK );
		return -1;
	}

	int ret = realSteamMain( argc, argv );

	FreeLibrary( steamUI );

	delete g_Crypto;
	delete g_Logger;

	return ret;
}
