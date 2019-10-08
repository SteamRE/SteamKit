

#ifndef NETHOOK_LOGGER_H_
#define NETHOOK_LOGGER_H_


#include <windows.h>
#include <string>

#include "steam/steamtypes.h"
#include "steam/emsg.h"

#ifdef DeleteFile
#undef DeleteFile
#endif

enum class ENetDirection
{
	k_eNetIncoming,
	k_eNetOutgoing,
};

class CLogger
{

public:
	CLogger() noexcept;

	void LogConsole( const char *szFmt, ... );
	void LogNetMessage( ENetDirection eDirection, const uint8 *pData, uint32 cubData );
	void LogSessionData( ENetDirection eDirection, const uint8 *pData, uint32 cubData );
	void LogOpenFile( HANDLE hFile, const char *szFmt, ... );

	HANDLE OpenFile( const char *szFileName, bool bSession );
	void CloseFile( HANDLE hFile) noexcept;
	void DeleteFile( const char *szFileName, bool bSession );

private:
	const char *GetFileNameBase( ENetDirection eDirection, EMsg eMsg, uint8 serverType = 0xFF );
	void MultiplexMulti( ENetDirection eDirection, const uint8 *pData, uint32 cubData );

private:
	std::string m_RootDir;
	std::string m_LogDir;

	uint32 m_uiMsgNum;

};

extern CLogger *g_pLogger;


#endif // !NETHOOK_LOGGER_H_
