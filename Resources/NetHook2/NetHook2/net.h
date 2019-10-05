
#ifndef NETHOOK_NET_H_
#define NETHOOK_NET_H_
#ifdef _WIN32
#pragma once
#endif


#include "steam/steamtypes.h"
#include "steam/net.h"

#include "csimpledetour.h"


namespace NetHook
{

typedef bool (__fastcall *BBuildAndAsyncSendFrameFn)(void *, void *, EWebSocketOpCode, const uint8 *, uint32);
typedef void(__fastcall *RecvPktFn)(void *, void *, CNetPacket *);

class CNet
{

public:
	CNet() noexcept;
	~CNet();


public:
	// CWebSocketConnection::BBuildAndAsyncSendFrame(EWebSocketOpCode, uchar const*, int)
	static bool __fastcall BBuildAndAsyncSendFrame(void *webSocketConnection, void *unused, EWebSocketOpCode eWebSocketOpCode, const uint8 *pubData, uint32 cubData);

	// CCMInterface::RecvPkt(CNetPacket *)
	static void __fastcall RecvPkt(void *cmConnection, void *unused, CNetPacket *pPacket);


private:
	CSimpleDetour *m_RecvPktDetour;
	CSimpleDetour *m_BuildDetour;

};

extern CNet *g_pNet;

}

#endif // !NETHOOK_NET_H_
