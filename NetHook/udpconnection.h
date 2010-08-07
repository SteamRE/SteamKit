
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


class CUDPConnection
{

public:
	CUDPConnection();


	bool ReceivePacket( const uint8 *pData, uint32 cubData, const sockaddr_in *sockAddr );

	bool ReceiveDatagram( const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData );
	bool ReceiveChallenge( const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData );
	bool ReceiveData( const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData );

	bool ReceiveNetPacket( const UDPPktHdr_t *pHdr, CNetPacket *pPacket );

	bool SendPacket( const uint8 *pData, uint32 cubData, const sockaddr_in *sockAddr );


	bool HandleNetMsg( EMsg eMsg, const uint8 *pData, uint32 cubData );


private:
	PacketMap m_packetMap;

	bool m_bUsingCrypto;
	static const char m_szLogFile[];

};


extern CUDPConnection *g_udpConnection;



#endif // !UDPCONNECTION_H_
