#ifndef STEAMTCP_H
#define STEAMTCP_H

#include "steam/SteamTypes.h"

enum ETCPPktType
{
	k_ETCPPktTypeRequestExternalIP = 1,
	k_ETCPPktTypeExternalIP = 2,
	k_ETCPPktTypeCredentials = 3,
	k_ETCPPktTypePostCredentials = 4,
	k_ETCPPktTypeVerifyAuth = 5,
};

#pragma pack(push, 1)

struct TCPPktTypeRequestExternalIP_t
{
	uint32 m_nUnknown1; //0
	uint8 m_cUnknown2; //4
	uint32 m_nInternalIP;
	uint32 m_nUnknown3; //1
};

struct TCPPktTypeExternalIP_t
{
	uint8 m_nUnknown1; //0
	uint32 m_nExternalIP;
};

#pragma pack(pop)

#endif //STEAMTCP_H