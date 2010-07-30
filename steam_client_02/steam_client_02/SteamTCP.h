#ifndef STEAMTCP_H
#define STEAMTCP_H

#include "SteamTypes.h"

enum ETCPPktType
{
    k_ETCPPktTypeRequestExternalIP = 1,
    k_ETCPPktTypeExternalIP = 2,
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

struct TCPPktTypeCredentials_t
{
    uint32 m_nUnknown1; //27 (Protocol version?)
    uint8 m_nUnknown2; //2 (Username count..?)

    uint16 m_nUsernameLength1;
    uint8 *m_pUsername1;

    uint16 m_nUsernameLength2;
    uint8 *m_pUsername2;
};

#pragma pack(pop)

#endif //STEAMTCP_H