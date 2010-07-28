
#ifndef UDPCONNECTION_H_
#define UDPCONNECTION_H_
#ifdef _WIN32
#pragma once
#endif


#include "steam/steamtypes.h"
#include "steam/udppkt.h"

#include <winsock2.h>


class CUDPConnection
{

public:
	CUDPConnection( const char *szBasePath );

	bool ReceivePacket( const uint8 *pData, uint32 cubData, const sockaddr_in *sockAddr );
	bool SendPacket( const uint8 *pData, uint32 cubData, const sockaddr_in *sockAddr );

private:
	char m_szLogPath[ MAX_PATH ];
	bool m_bUsingCrypto;

};


extern CUDPConnection *g_udpConnection;



#endif // !UDPCONNECTION_H_
