
#ifndef STEAM_NET_H_
#define STEAM_NET_H_
#ifdef _WIN32
#pragma once
#endif


#include "steamtypes.h"


typedef uint32 HCONNECTION;


enum class EWebSocketOpCode
{
	k_eWebSocketOpCode_Continuation	= 0x00,
	k_eWebSocketOpCode_Text			= 0x01,
	k_eWebSocketOpCode_Binary		= 0x02,

	k_eWebSocketOpCode_Close		= 0x08,
	k_eWebSocketOpCode_Ping			= 0x09,
	k_eWebSocketOpCode_Pong			= 0x0A,
};

inline const char *EWebSocketOpCodeToName(EWebSocketOpCode eWebSocketOpCode) noexcept
{
	const char * const rgchWebSocketOpCodeNames[] =
	{
		"Continuation",
		"Text",
		"Binary",
		"<Reserved>", // 0x03
		"<Reserved>", // 0x04
		"<Reserved>", // 0x05
		"<Reserved>", // 0x06
		"<Reserved>", // 0x07
		"Close",
		"Ping",
		"Pong",
		"<Reserved>", // 0x0B
		"<Reserved>", // 0x0C
		"<Reserved>", // 0x0D
		"<Reserved>", // 0x0E
		"<Reserved>", // 0x0F
	};

	const int iOpCodeValue = static_cast<int>(eWebSocketOpCode);

	if (iOpCodeValue < 0 || iOpCodeValue >= sizeof(rgchWebSocketOpCodeNames))
		return "<Invalid>";

	return rgchWebSocketOpCodeNames[iOpCodeValue];
}


class CNetPacket
{
public:
	HCONNECTION m_hConnection;

	uint8* m_pubData;
	uint32 m_cubData;

	int m_cRef;

	uint8* m_pubNetworkBuffer;

	CNetPacket* m_pNext;
};


#endif // !STEAM_NET_H_
