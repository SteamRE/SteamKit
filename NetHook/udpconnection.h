
#ifndef UDPCONNECTION_H_
#define UDPCONNECTION_H_
#ifdef _WIN32
#pragma once
#endif


#include "netpacket.h"

#include "steam/steamtypes.h"
#include "steam/udppkt.h"

#include "utlmap.h"

#include <winsock2.h>



typedef CUtlMap< uint32, CNetPacket * > PacketMap;
typedef PacketMap::IndexType_t PacketMapIndex;


enum ENetDirection
{
	k_eNetIncoming,
	k_eNetOutgoing,
};


class CUDPConnection
{

public:
	CUDPConnection();


	bool ReceivePacket( const uint8 *pData, uint32 cubData, const sockaddr_in *sockAddr );
	bool SendPacket( const uint8 *pData, uint32 cubData, const sockaddr_in *sockAddr );

	bool HandlePacket( ENetDirection eDirection, const uint8 *pData, uint32 cubData, const sockaddr_in *sockAddr );

	bool HandleDatagram( ENetDirection eDirection, const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData );
	bool HandleChallenge( ENetDirection eDirection, const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData );
	bool HandleChallengeReq( ENetDirection eDirection, const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData );
	bool HandleAccept( ENetDirection eDirection, const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData );
	bool HandleConnect( ENetDirection eDirection, const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData );
	bool HandleData( ENetDirection eDirection, const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData );

	bool HandleNetPacket( ENetDirection eDirection, const UDPPktHdr_t *pHdr, CNetPacket *pPacket );
	bool HandleNetMsg( ENetDirection eDirection, EMsg eMsg, const uint8 *pData, uint32 cubData );

	bool MultiplexMsgMulti( ENetDirection eDirection, const uint8 *pData, uint32 cubData );


private:
	PacketMap m_recvMap;
	PacketMap m_sendMap;

	bool m_bUsingCrypto;
	static const char m_szLogFile[];

};


extern CUDPConnection *g_udpConnection;



#endif // !UDPCONNECTION_H_
