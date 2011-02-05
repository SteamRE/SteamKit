

#ifndef NETHOOK_LOGGER_H_
#define NETHOOK_LOGGER_H_


#include <windows.h>
#include <string>

#include "steam/steamtypes.h"
#include "steam/emsg.h"


enum ENetDirection
{
	k_eNetIncoming,
	k_eNetOutgoing,
};

class CLogger
{

public:
	CLogger();

	void LogConsole( const char *szFmt, ... );

	void LogNetMessage( ENetDirection eDirection, uint8 *pData, uint32 cubData );

	void LogSessionData( ENetDirection eDirection, uint8 *pData, uint32 cubData );
	void LogFile( const char *szFileName, bool bSession, const char *szFmt, ... );

private:
	const char *GetFileName( ENetDirection eDirection, EMsg eMsg );
	void MultiplexMulti( ENetDirection eDirection, uint8 *pData, uint32 cubData );

private:
	std::string m_RootDir;
	std::string m_LogDir;

	uint32 m_uiMsgNum;

};

extern CLogger *g_pLogger;


#endif // !NETHOOK_LOGGER_H_
