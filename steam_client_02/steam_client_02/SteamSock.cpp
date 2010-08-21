#include "SteamSock.h"

#include <stdio.h>

//SteamNetManager

bool SteamNetManager::Initialize()
{
	WSADATA WSAData;
	return WSAStartup(MAKEWORD(2, 2), &WSAData) == 0;
}

bool SteamNetManager::Shutdown()
{
	return WSACleanup() == 0;
}

int SteamNetManager::ShutdownWithMessage(const char *pchMessage, bool bPause)
{
	Shutdown();

	printf("%s\n", pchMessage);

	if (bPause)
		_getch();

	return 0;
}

//SteamAddr

SteamAddr::SteamAddr()
{
	m_bSet = false;
}

SteamAddr::SteamAddr(sockaddr_in sAddr)
{
	Set(sAddr);
}

SteamAddr::SteamAddr(const char *pchIP, unsigned short usPort)
{
	Set(pchIP, usPort);
}

void SteamAddr::Set(sockaddr_in sAddr)
{
	m_sAddr = sAddr;

	m_bSet = true;
}

void SteamAddr::Set(const char *pchIP, unsigned short usPort)
{
	m_sAddr.sin_family = AF_INET;
	m_sAddr.sin_addr.s_addr = inet_addr(pchIP);
	m_sAddr.sin_port = htons(usPort);

	m_bSet = true;
}

bool SteamAddr::IsSet()
{
	return m_bSet;
}

const char *SteamAddr::GetIP()
{
	if (!m_bSet)
		return NULL;

	return inet_ntoa(m_sAddr.sin_addr);
}

unsigned short SteamAddr::GetPort()
{
	if (!m_bSet)
		return 0;

	return ntohs(m_sAddr.sin_port);
}

const char *SteamAddr::ToString(bool bIncludePort)
{
	if (!m_bSet)
		return NULL;

	static char m_chAddr[32];
	memset(m_chAddr, 0, 32);

	strcat(m_chAddr, GetIP());

	if (bIncludePort)
	{
		char chPort[8];
		_itoa(GetPort(), chPort, 10);

		strcat(m_chAddr, ":");
		strcat(m_chAddr, chPort);
	}

	return m_chAddr;
}

sockaddr_in *SteamAddr::ToWinSock()
{
	if (!m_bSet)
		return NULL;

	return &m_sAddr;
}

bool SteamAddr::Matches(SteamAddr sAddrCompare)
{
	return strcmp(sAddrCompare.ToString(), this->ToString()) == 0;
}

//SteamSock

bool SteamSock::Shutdown(int nHow)
{
	return shutdown(m_pSocket, nHow) == 0;  
}

bool SteamSock::Bind(const char *pchIP, unsigned short usPort, int nType, int nProtocol)
{
	m_pSocket = socket(AF_INET, nType, nProtocol);

	m_sAddr.sin_family = AF_INET;
	m_sAddr.sin_addr.s_addr = inet_addr(pchIP);
	m_sAddr.sin_port = htons(usPort);

	if (bind(m_pSocket, (const sockaddr *)&m_sAddr, sizeof(sockaddr_in)) == 0)
		return (m_bBound = true);
	else
		return (m_bBound = false);
}

bool SteamSock::IsBound()
{
	return m_bBound;
}

sockaddr_in *SteamSock::GetBoundAddress()
{
	if (!m_bBound)
		return NULL;

	return &m_sAddr;
}

SOCKET SteamSock::ToWinSock()
{
	return m_pSocket;
}

//SteamSockUDP

SteamSockUDP::SteamSockUDP()
{
	m_bBound = false;
}

SteamSockUDP::SteamSockUDP(SteamAddr saBind)
{
	Bind(saBind.GetIP(), saBind.GetPort());
}

SteamSockUDP::SteamSockUDP(const char *pchIP, unsigned short usPort)
{
	Bind(pchIP, usPort);
}

bool SteamSockUDP::Bind(const char *pchIP, unsigned short usPort)
{
	m_nSrcConnectionID = 0x00000200;
	m_nDstConnectionID = 0x00000000;
	m_nSeqThis = 1;
	m_nSeqAcked = 0;

	return ((SteamSock *)this)->Bind(pchIP, usPort, SOCK_DGRAM, IPPROTO_UDP);
}

bool SteamSockUDP::RecvFrom(UDPPktHdr_t *pHdr, char *pchData, unsigned int cchData, unsigned int *pcchData, SteamAddr *psaSource)
{
	if (!m_bBound)
		return false;

	sockaddr_in from;
	int fromlen = sizeof(sockaddr_in);

	int recvFromSz = recvfrom(m_pSocket, pchData, cchData, 0, (sockaddr *)&from, &fromlen);

	if (recvFromSz <= 0)
		return false;

	memcpy(pHdr, pchData, sizeof(UDPPktHdr_t));
	
	*pcchData = recvFromSz - sizeof(UDPPktHdr_t);

	for (unsigned int i=0;i<*pcchData;i++)
		pchData[i] = pchData[i+sizeof(UDPPktHdr_t)];

	if (pHdr->m_nSrcConnectionID != m_nDstConnectionID)
		m_nDstConnectionID = pHdr->m_nSrcConnectionID;
	
	if (pHdr->m_nSeqThis > m_nSeqAcked)
		m_nSeqAcked = pHdr->m_nSeqThis;

	psaSource->Set(from);

	return true;
}

bool SteamSockUDP::SendTo(EUDPPktType eType, unsigned char vfFlags, const char *pchData, unsigned int cchData, SteamAddr saTarget)
{
	if (!m_bBound)
		return false;

	UDPPktHdr_t hdr;
	hdr.m_nMagic = k_nMagic;
	hdr.m_cbPkt = cchData;
	hdr.m_EUDPPktType = eType;
	hdr.m_nFlags = vfFlags;
	hdr.m_nSrcConnectionID = m_nSrcConnectionID;
	hdr.m_nDstConnectionID = m_nDstConnectionID;

	hdr.m_nSeqAcked = m_nSeqAcked;

	if (vfFlags != 0)
	{
		hdr.m_nSeqThis = ++m_nSeqThis;
		hdr.m_nPktsInMsg = 1;
		hdr.m_nMsgStartSeq = m_nSeqThis;
		hdr.m_cbMsgData = cchData;
	}
	else
	{
		hdr.m_nSeqThis = m_nSeqThis;
		hdr.m_nPktsInMsg = 0;
		hdr.m_nMsgStartSeq = 0;
		hdr.m_cbMsgData = 0;
	}

	//printf("Sending UDP packet (size=%d, type=%d, flags=%d, src=%08X, dst=%08X, seq=%d, ack=%d)\n", sizeof(UDPPktHdr_t)+cchData, eType, vfFlags, m_nSrcConnectionID, m_nDstConnectionID, m_nSeqThis, m_nSeqAcked);

	char chData[2048];

	memcpy(chData, &hdr, sizeof(UDPPktHdr_t));
	memcpy(chData+sizeof(UDPPktHdr_t), pchData, cchData);
	
	return sendto(m_pSocket, chData, sizeof(UDPPktHdr_t)+cchData, 0, (const sockaddr *)saTarget.ToWinSock(), sizeof(sockaddr_in)) > 0;
}

//SteamSockTCP

SteamSockTCP::SteamSockTCP()
{
	m_bBound = false;
	m_bConnected = false;
}

SteamSockTCP::SteamSockTCP(SteamAddr saBind, SteamAddr saConnect)
{
	Bind(saBind.GetIP(), saBind.GetPort());
	Connect(saConnect.GetIP(), saConnect.GetPort());
}

SteamSockTCP::SteamSockTCP(const char *pchIP, unsigned short usPort, const char *pchConnectIP, unsigned short usConnectPort)
{
	Bind(pchIP, usPort);
	Connect(pchConnectIP, usConnectPort);
}

bool SteamSockTCP::Bind(const char *pchIP, unsigned short usPort)
{
	return ((SteamSock *)this)->Bind(pchIP, usPort, SOCK_STREAM, IPPROTO_TCP);
}

bool SteamSockTCP::Connect(const char *pchIP, unsigned short usPort)
{
	m_sConnectAddr.sin_family = AF_INET;
	m_sConnectAddr.sin_addr.s_addr = inet_addr(pchIP);
	m_sConnectAddr.sin_port = htons(usPort);

	if (connect(m_pSocket, (const sockaddr *)&m_sConnectAddr, sizeof(sockaddr_in)) == 0)
		return (m_bConnected = true);
	else
		return (m_bConnected = false);
}

bool SteamSockTCP::IsConnected()
{
	return m_bConnected;
}

sockaddr_in *SteamSockTCP::GetConnectedAddress()
{
	if (!m_bConnected)
		return NULL;

	return &m_sConnectAddr;
}

bool SteamSockTCP::Recv(char *pchData, unsigned int cchData, unsigned int *pcchData)
{
	if (!m_bBound)
		return false;

	if (!m_bConnected)
		return false;

	int recvSz = recv(m_pSocket, pchData, cchData, 0);

	if (recvSz <= 0)
		return false;

	*pcchData = recvSz;

	return true;
}

bool SteamSockTCP::Send(const char *pchData, unsigned int cchData)
{
	if (!m_bBound)
		return false;

	if (!m_bConnected)
		return false;

	return send(m_pSocket, pchData, cchData, 0) > 0;
}

ETCPPktType SteamSockTCP::GetExpectedPacket()
{
	return m_ETCPPktTypeExpected;
}

void SteamSockTCP::SetExpectedPacket(ETCPPktType ETCPPktTypeExpected)
{
	m_ETCPPktTypeExpected = ETCPPktTypeExpected;
}