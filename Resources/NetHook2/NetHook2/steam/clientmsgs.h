
#ifndef CLIENTMSGS_H_
#define CLIENTMSGS_H_
#ifdef _WIN32
#pragma once
#endif


#include "steam/steamtypes.h"
#include "steam/csteamid.h"



#pragma pack( push, 1 )

struct MsgChannelEncryptRequest_t
{
	uint32 m_unProtocolVer;
	int32 m_EUniverse; // EUniverse
};

struct MsgChannelEncryptResponse_t
{
	uint32 m_unProtocolVer;
	uint32 m_cubEncryptedKey;
};

struct MsgChannelEncryptResult_t
{
	int32 m_EResult;
};

struct MsgMulti_t
{
	uint32 m_cubUnzipped;
};

struct MsgClientAnonLogOn_t
{
	uint32 m_unProtocolVer;

	uint32 m_unIPPrivateObfuscated;
	uint32 m_unIPPublic;

};

struct MsgClientLogOnResponse_t
{
	int m_EResult;
 
	int m_nOutOfGameHeartbeatRateSec;
	int m_nInGameHeartbeatRateSec;
 
	CSteamID m_ulClientSuppliedSteamId;
 
	uint32 m_unIPPublic;
 
	RTime32 m_RTime32ServerRealTime;
};


struct MsgClientLogOnWithCredentials_t
{
	uint32 m_unProtocolVer;

	uint32 m_unIPPrivateObfuscated;
	uint32 m_unIPPublic;

	uint64 m_ulClientSuppliedSteamId;

	uint32 m_unTicketLength;

	char m_rgchAccountName[ 64 ];
	char m_rgchPassword[ 20 ];

	uint32 m_qosLevel; // ENetQOSLevel
};

struct MsgClientRegisterAuthTicketWithCM_t
{
	uint32 m_unProtocolVer;
	uint32 m_unTicketLengthWithSignature; //B0 00 00 00
};



#pragma pack( pop )


#endif // !CLIENTMSGS_H_
