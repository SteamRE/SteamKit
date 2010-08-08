
#ifndef CLIENTMSGS_H_
#define CLIENTMSGS_H_
#ifdef _WIN32
#pragma once
#endif

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

#pragma pack( pop )


#endif // !CLIENTMSGS_H_
