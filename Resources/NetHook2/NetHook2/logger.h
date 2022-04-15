#ifndef NETHOOK_LOGGER_H_
#define NETHOOK_LOGGER_H_

#include <string>

#include "steam/steamtypes.h"
#include "steam/emsg.h"

namespace NetHook
{

enum class ENetDirection
{
    k_eNetIncoming,
    k_eNetOutgoing,
};

class CLogger
{

public:
    CLogger() noexcept;
    ~CLogger();

    void InitSessionLogDir();

    void LogConsole( const char *szFmt, ... );
    void LogNetMessage( ENetDirection eDirection, const uint8 *pData, uint32 cubData );
    void LogSessionData( ENetDirection eDirection, const uint8 *pData, uint32 cubData );

private:
    const char *GetFileNameBase( ENetDirection eDirection, EMsg eMsg, uint8 serverType = 0xFF );
    void MultiplexMulti( ENetDirection eDirection, const uint8 *pData, uint32 cubData );

    std::string m_RootDir;
    std::string m_LogDir;

    uint32 m_uiMsgNum;

};

}

extern NetHook::CLogger *g_pLogger;

#endif // !NETHOOK_LOGGER_H_
