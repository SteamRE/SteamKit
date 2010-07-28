
#ifndef LOGGER_H_
#define LOGGER_H_
#ifdef _WIN32
#pragma once
#endif


#include "steam/steamtypes.h"

#include <windows.h>
#include <map>


enum ENetType
{
	eNetType_Invalid = 0,

	eNetType_TCP,
	eNetType_UDP,

	eNetType_Max,
};

enum ENetDirection
{
	eNetDirection_Invalid = 0,

	eNetDirection_Send,
	eNetDirection_Recv,

	eNetDriection_Max,
};


typedef std::map< ENetType, uint32 > CountMap;
typedef std::pair< ENetType, uint32 > CountPair;


class CLogger
{

public:
	CLogger( const char *szBaseDir );

	void LogPacket( ENetType eNetType, ENetDirection eNetDirection, const uint8 *pData, uint32 cubData, uint32 ip = 0, uint16 port = 0 );


	void LogConsole( const char *szString, ... );

	void LogFile( const char *szFileName, const char *szString, ... );
	void LogFileData( const char *szFileName, const uint8 *pData, uint32 cubData, bool bAppend = false );

	void AppendFile( const char *szFileName, const char *szString, ... );

private:
	char m_szDir[ MAX_PATH ];

	CountMap m_typeCounts;

};


extern CLogger *g_Logger;


#endif
