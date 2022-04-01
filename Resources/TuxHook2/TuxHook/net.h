#ifndef NETHOOK_NET_H_
#define NETHOOK_NET_H_

#include "steam/steamtypes.h"
#include "steam/net.h"
#include "csimpledetour.h"

namespace NetHook
{

#ifdef __linux__
    typedef bool (*BBuildAndAsyncSendFrameFn)(void *, EWebSocketOpCode, const uint8 *, uint32);
    typedef bool (*BBuildAndAsyncSendFrame2Fn)(void *, const uint8 *, uint32, uint32);
    typedef void (*RecvPktFn)(void *, CNetPacket *);
#elif _WIN32
    typedef bool (__fastcall *BBuildAndAsyncSendFrameFn)(void *, void *, EWebSocketOpCode, const uint8 *, uint32);
    typedef void(__fastcall *RecvPktFn)(void *, void *, CNetPacket *);
#endif

class CNet
{

public:
    CNet() noexcept;
    ~CNet();

#ifdef __linux__
    // CWebSocketConnection::BBuildAndAsyncSendFrame(EWebSocketOpCode, uchar const*, int)
    static bool BBuildAndAsyncSendFrame(void* webSocketConnection, EWebSocketOpCode eWebSocketOpCode, const uint8* pubData, uint32 cubData);

    static bool BBuildAndAsyncSendFrameOl(void *webSocketConnection, const uint8 *pubData, uint32 cubData, uint32 uUnk);
    // CCMInterface::RecvPkt(CNetPacket *)
    static void RecvPkt(void *cmConnection, CNetPacket *pPacket);
#elif _WIN32
    static bool __fastcall BBuildAndAsyncSendFrame(void* webSocketConnection, void* unused, EWebSocketOpCode eWebSocketOpCode, const uint8* pubData, uint32 cubData);
    static void __fastcall RecvPkt(void* cmConnection, void* unused, CNetPacket* pPacket);
#endif

private:
    CSimpleDetour *m_RecvPktDetour;
    CSimpleDetour *m_BuildDetour;
#ifdef __linux__
    CSimpleDetour *m_BuildDetourOl;
#endif

};

}

extern NetHook::CNet *g_pNet;

#endif // !NETHOOK_NET_H_
