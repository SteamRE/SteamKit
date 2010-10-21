
#ifndef CLIENTAPP_H_
#define CLIENTAPP_H_
#ifdef _WIN32
#pragma once
#endif


#define STEAM_API_EXPORTS

#include "steam/steamtypes.h"

typedef int ( STEAM_CALL *SteamExeFrameFn )( int, int );
typedef int ( STEAM_CALL *SteamDllMainFn )( int , char ** );


#define CLIENTAPP( entry ) \
	SteamExeFrameFn g_SteamExeFrame = NULL; \
	int entry( int argc, char **argv ); \
	S_API int STEAM_CALL SteamDllMain( int argc, char **argv ) \
	{ \
		return entry( argc, argv ); \
	} \
	S_API int STEAM_CALL SteamDllMainEx( int argc, char **argv, SteamExeFrameFn frameFn ) \
	{ \
		g_SteamExeFrame = frameFn; \
		return entry( argc, argv ); \
	}


#endif // !CLIENTAPP_H_
