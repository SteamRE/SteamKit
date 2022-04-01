
#ifndef UDPPKT_H_
#define UDPPKT_H_
#ifdef _WIN32
#pragma once
#endif

#include "steamtypes.h"

#include "csteamid.h"
#include "emsg.h"

const uint32 k_uNetFlagNoIOCP				= 1;
const uint32 k_uNetFlagFindAvailPort		= 2;
const uint32 k_uNetFlagUseAuthentication	= 4;
const uint32 k_uNetFlagUseEncryption		= 8;
const uint32 k_uNetFlagRawStream			= 16;
const uint32 k_uNetFlagRawStreamSend		= 32;
const uint32 k_uNetFlagUnboundSocket		= 64;
const uint32 k_uNetFlagRawIORecv			= 128;

const uint32 k_uNetFlagsKeyCallbackRequired	= k_uNetFlagUseAuthentication | k_uNetFlagUseEncryption; // 12

enum EUDPPktType
{
	// This is the first packet type sent to Steam servers by the client.
	// The client iterates through approximately 20 servers in an attempt to find the "best" one.
	// Only the UDPPktType, local connection ID and outgoing sequence need to be given an appropriate value to request a challenge.
	k_EUDPPktTypeChallengeReq = 1,

	// Steam servers respond to k_EUDPPktTypeChallengeReq with this packet type value and 8 bytes of information.
	// The data is not encrypted.
	// The first 4 bytes are the 'base' challenge which is used in k_EUDPPktTypeConnect after going through some changes.
	// The next 4 bytes are unconfirmed, but they may be involved in the process of generating the actual challenge value used by the client.
	k_EUDPPktTypeChallenge = 2,

	// The client sends this packet type after choosing the "best" Steam server available.
	// A challenge is attached which is derived from the challenge in k_EUDPPktTypeChallenge.
	// The instruction "XOR EDI, A426DF2B" is executed, EDI being the challenge, however this is not always the correct value and can be offset by small amounts.
	// The Steam client uses the flag 4 when sending this, so assume it is necessary.
	// UDPPktHdr should be filled in as normal but without encryption on the data.
	k_EUDPPktTypeConnect = 3,

	// If the k_EUDPPktTypeConnect packet is received by the destination server and acknowledged as valid then it responds with this packet type.
	// No data is attached, however a destination connection ID is generated and should be stored for use in later traffic.
	// The Steam client uses the flag 4 when sending this, so assume it is necessary.
	k_EUDPPktTypeAccept = 4,

	// Unknown, most likely sent to signify process termination.
	k_EUDPPktTypeDisconnect = 5,

	// This packet type is used for the majority of VS01 traffic, incoming and outgoing.
	// The flag is usually 4, however this is not confirmed as necessary or constant.
	// The packet should include valid destination and source connection IDs.
	// Not all data sent through this type is encrypted and it's currently unclear what indicates when it is and when it isn't.
	k_EUDPPktTypeData = 6,

	// The datagram message type appears to be used for a packet resend.
	// Sometimes the sequence number isn't included, but message size is.
	// Sometimes the sequence number is included, but message size isn't.
	// Sequence number may be replaced by size in the case of the seq value being incremented between initial send time and retry time.
	k_EUDPPktTypeDatagram = 7,

	// Max enum value.
	k_EUDPPktTypeMax = 8,
};

#pragma pack( push, 1 )

struct UDPPktHdr_t
{
	uint32 m_nMagic; // "VS01" or "VT01"

	uint16 m_cbPkt;

	uint8 m_EUDPPktType; // EUDPPktType

	uint8 m_nFlags; // NetFlags

	uint32 m_nSrcConnectionID;
	uint32 m_nDstConnectionID;

	uint32 m_nSeqThis;
	uint32 m_nSeqAcked;

	uint32 m_nPktsInMsg;
	uint32 m_nMsgStartSeq;

	uint32 m_cbMsgData;
};

struct MsgHdr_t
{
	int32 m_EMsg; // EMsg

	JobID_t m_JobIDTarget;
	JobID_t m_JobIDSource;
};

struct ExtendedClientMsgHdr_t
{
	int32 m_EMsg; // EMsg

	uint8 m_nCubHdr;

	uint16 m_nHdrVersion;

	JobID_t m_JobIDTarget;
	JobID_t m_JobIDSource;

	uint8 m_nHdrCanary;

	CSteamID m_ulSteamID;

	int32 m_nSessionID;
};

#pragma pack( pop )


#endif // !UDPPKT_H_
