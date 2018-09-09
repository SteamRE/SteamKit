
#define WIN32_LEAN_AND_MEAN
#include <windows.h>


#include "net.h"

#include "logger.h"
#include "csimplescan.h"


namespace NetHook
{


BBuildAndAsyncSendFrameFn BBuildAndAsyncSendFrame_Orig = NULL;
RecvPktFn RecvPkt_Orig = NULL;

CNet::CNet()
	: m_RecvPktDetour(NULL),
	  m_BuildDetour(NULL)
{
	CSimpleScan steamClientScan("steamclient.dll");

	BBuildAndAsyncSendFrameFn pBuildFunc = NULL;
	bool bFoundBuildFunc = steamClientScan.FindFunction(
		"\x55\x8B\xEC\x83\xEC\x6C\x53\x8B\xD9\x89\x5D\xF4",
		"xxxxxxxxxxxx",
		(void **)&pBuildFunc
	);

	BBuildAndAsyncSendFrame_Orig = pBuildFunc;

	g_pLogger->LogConsole("CWebSocketConnection::BBuildAndAsyncSendFrame = 0x%x\n", BBuildAndAsyncSendFrame_Orig);

	RecvPktFn pRecvPktFunc = NULL;
	bool bFoundRecvPktFunc = steamClientScan.FindFunction(
		"\x55\x8B\xEC\x81\xEC\x88\x04\x00\x00\x53\x56\x57\x6A\x01\xFF",
		"xxxxx?xxxxxxxxx",
		(void **)&pRecvPktFunc
	);

	RecvPkt_Orig = pRecvPktFunc;

	g_pLogger->LogConsole("CCMInterface::RecvPkt = 0x%x\n", RecvPkt_Orig);


	RecvPktFn thisRecvPktFunc = CNet::RecvPkt;

	if (bFoundBuildFunc)
	{
		BBuildAndAsyncSendFrameFn thisBuildFunc = CNet::BBuildAndAsyncSendFrame;

		m_BuildDetour = new CSimpleDetour((void **)&BBuildAndAsyncSendFrame_Orig, (void *)thisBuildFunc);
		m_BuildDetour->Attach();

		g_pLogger->LogConsole("Detoured CWebSocketConnection::BBuildAndAsyncSendFrame!\n");
	}
	else
	{
		g_pLogger->LogConsole("Unable to hook CWebSocketConnection::BBuildAndAsyncSendFrame: func scan failed.\n");
	}

	if (bFoundRecvPktFunc)
	{
		RecvPktFn thisRecvPktFunc = CNet::RecvPkt;

		m_RecvPktDetour = new CSimpleDetour((void **)&RecvPkt_Orig, (void *)thisRecvPktFunc);
		m_RecvPktDetour->Attach();

		g_pLogger->LogConsole("Detoured CCMInterface::RecvPkt!\n");
	}
	else
	{
		g_pLogger->LogConsole("Unable to hook CCMInterface::RecvPkt: func scan failed.\n");
	}

}

CNet::~CNet()
{
	if (m_RecvPktDetour)
	{
		m_RecvPktDetour->Detach();
		delete m_RecvPktDetour;
	}

	if (m_BuildDetour)
	{
		m_BuildDetour->Detach();
		delete m_BuildDetour;
	}
}


bool CNet::BBuildAndAsyncSendFrame(void *webSocketConnection, void *unused, EWebSocketOpCode eWebSocketOpCode, const uint8 *pubData, uint32 cubData)
{
	if (eWebSocketOpCode == k_eWebSocketOpCode_Binary)
	{
		g_pLogger->LogNetMessage(k_eNetOutgoing, pubData, cubData);
	}
	else
	{
		g_pLogger->LogConsole("Sending websocket frame with opcode %d (%s), ignoring\n",
			eWebSocketOpCode, EWebSocketOpCodeToName(eWebSocketOpCode)
		);
	}

	return (*BBuildAndAsyncSendFrame_Orig)(webSocketConnection, unused, eWebSocketOpCode, pubData, cubData);
}

void CNet::RecvPkt(void *cmConnection, void *unused, CNetPacket *pPacket)
{
	g_pLogger->LogNetMessage(k_eNetIncoming, pPacket->m_pubData, pPacket->m_cubData);

	(*RecvPkt_Orig)(cmConnection, unused, pPacket);
}


}
