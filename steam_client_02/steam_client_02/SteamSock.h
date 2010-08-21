#ifndef STEAMSOCK_H
#define STEAMSOCK_H

#include <WinSock2.h>
#include <WS2tcpip.h>

#include <stdio.h>
#include <conio.h>

#include "steam/SteamTypes.h"

#include "steam/udppkt.h"
//#include "SteamUDP.h"
#include "SteamTCP.h"



class SteamNetManager
{
public:
	bool Initialize();
	bool Shutdown();

	int ShutdownWithMessage(const char *pchMessage, bool bPause = true);
};

class SteamAddr
{
public:
	SteamAddr();
	SteamAddr(sockaddr_in sAddr);
	SteamAddr(const char *pchIP, unsigned short usPort);

	void Set(sockaddr_in sAddr);
	void Set(const char *pchIP, unsigned short usPort);

	bool IsSet();

	const char *GetIP();
	unsigned short GetPort();

	const char *ToString(bool bIncludePort = true);
	sockaddr_in *ToWinSock();

	bool Matches(SteamAddr sAddrCompare);
private:
	sockaddr_in m_sAddr;

	bool m_bSet;
};

class SteamSock
{
public:
	bool Shutdown(int nHow = SD_BOTH);

	bool Bind(const char *pchIP, unsigned short usPort, int nType, int nProtocol);
	bool IsBound();
	sockaddr_in *GetBoundAddress();

	SOCKET ToWinSock();
protected:
	SOCKET m_pSocket;
	sockaddr_in m_sAddr;

	bool m_bBound;
};

class SteamSockUDP : public SteamSock
{
public:
	SteamSockUDP();
	SteamSockUDP(SteamAddr saBind);
	SteamSockUDP(const char *pchIP, unsigned short usPort);

	bool Bind(const char *pchIP, unsigned short usPort);

	bool RecvFrom(UDPPktHdr_t *pHdr, char *pchData, unsigned int cchData, unsigned int *pcchData, SteamAddr *psaSource);
	bool SendTo(EUDPPktType eType, unsigned char vfFlags, const char *pchData, unsigned int cchData, SteamAddr saTarget);
private:
	unsigned int m_nSrcConnectionID;
	unsigned int m_nDstConnectionID;
	unsigned int m_nSeqThis;
	unsigned int m_nSeqAcked;
};

class SteamSockTCP : public SteamSock
{
public:
	SteamSockTCP();
	SteamSockTCP(SteamAddr saBind, SteamAddr saConnect);
	SteamSockTCP(const char *pchIP, unsigned short usPort, const char *pchConnectIP, unsigned short usConnectPort);

	bool Bind(const char *pchIP, unsigned short usPort);

	bool Connect(const char *pchIP, unsigned short usPort);
	bool IsConnected();
	sockaddr_in *GetConnectedAddress();

	bool Recv(char *pchData, unsigned int cchData, unsigned int *pcchData);
	bool Send(const char *pchData, unsigned int cchData);

	ETCPPktType GetExpectedPacket();
	void SetExpectedPacket(ETCPPktType ETCPPktTypeExpected);
private:
	sockaddr_in m_sConnectAddr;

	bool m_bConnected;

	ETCPPktType m_ETCPPktTypeExpected;
};

#endif //STEAM_SOCK_H