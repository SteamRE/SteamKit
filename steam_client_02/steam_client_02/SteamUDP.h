#ifndef STEAMUDP_H
#define STEAMUDP_H

#include "steam/SteamTypes.h"

const uint32 k_nMagic = 0x31305356; // "VS01"

const int32 k_nChallengeMask = -1540956373;

enum EUDPPktType
{
	k_EUDPPktTypeChallengeReq = 1,
	k_EUDPPktTypeChallenge = 2,
	k_EUDPPktTypeConnect = 3,
	k_EUDPPktTypeAccept = 4,
	k_EUDPPktTypeDisconnect = 5,
	k_EUDPPktTypeData = 6,
	k_EUDPPktTypeDatagram = 7,
	k_EUDPPktTypeMax = 8,
};

#pragma pack(push, 1)

struct UDPPktTypeChallenge_t
{
	uint32 m_nChallenge;
	uint32 m_nUnknown;
};

struct UDPPktTypeConnect_t
{
	uint32 m_nObfuscatedChallenge;
};

struct UDPPktHdr_t
{
	uint32 m_nMagic;
 
	uint16 m_cbPkt;
 
	uint8 m_EUDPPktType;
 
	uint8 m_nFlags;
 
	uint32 m_nSrcConnectionID;
	uint32 m_nDstConnectionID;
 
	uint32 m_nSeqThis;
	uint32 m_nSeqAcked;
 
	uint32 m_nPktsInMsg;
	uint32 m_nMsgStartSeq;
 
	uint32 m_cbMsgData;
};

#pragma pack(pop)

#endif //STEAMUDP_H