
#ifndef LOGGER_H_
#define LOGGER_H_
#ifdef _WIN32
#pragma once
#endif


#include "steam/steamtypes.h"

#define _WINSOCKAPI_
#include <windows.h>



class CLogger
{

public:
	CLogger( const char *szBaseDir );


	void LogConsole( const char *szString, ... );

	void AppendFile( const char *szFileName, const char *szString, ... );

	void LogFileData( const char *szFileName, const uint8 *pData, uint32 cubData, bool bAppend = false );

	void CreateDir( const char* szDir );


private:
	const char *GetFileDir( const char *szFile );

private:
	char m_szDir[ MAX_PATH ];

};

extern CLogger* g_Logger;

#endif // !LOGGER_H_
