

#include <winsock2.h>
#include <windows.h>

#include "clientapp.h"

#include "logger.h"
#include "udpconnection.h"
#include "wshooks.h"
#include "crypto.h"
#include "msgmanager.h"


// define our clientapp entry point
CLIENTAPP( main );


CLogger *g_Logger = NULL;
CUDPConnection *g_udpConnection = NULL;

CMsgManager *g_MsgManager = NULL;



int main( int argc, char **argv )
{

	AllocConsole();

	g_Logger = new CLogger( argv[ 0 ] );

	g_MsgManager = new CMsgManager();
	g_udpConnection = new CUDPConnection();

	Detour_WSAStartup->Attach();


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

	
	// udp
	Detour_recvfrom->Detach(); 
	Detour_WSASendTo->Detach();

	// tcp
	Detour_WSARecv->Detach();
	Detour_WSASend->Detach();

	Detour_WSAStartup->Detach();


	delete g_Crypto; // cleanup crypto hooks
	delete g_udpConnection;

	delete g_Logger;

	return ret;
}
