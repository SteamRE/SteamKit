#include "net.h"
#include "logger.h"
#include "clientmodule.h"

extern NetHook::CLogger *g_pLogger;

namespace NetHook
{

BBuildAndAsyncSendFrameFn BBuildAndAsyncSendFrame_Orig = nullptr;
BBuildAndAsyncSendFrame2Fn BBuildAndAsyncSendFrameOl_Orig = nullptr;
RecvPktFn RecvPkt_Orig = nullptr;

CNet::CNet() noexcept
    : m_RecvPktDetour(nullptr),
      m_BuildDetour(nullptr),
      m_BuildDetourOl(nullptr)
{
    BBuildAndAsyncSendFrameFn pBuildFunc = nullptr;
    const bool bFoundBuildFunc = g_pClientModule->FindSignature(
        "\x55\x57\x56\x53\xE8\x00\x00\x00\x00\x81\xC3\x00\x00\x00\x00\x81\xEC\x00\x00\x00\x00\x8B\x84\x24\x00\x00\x00\x00\x8B\xBC\x24\x00\x00\x00\x00\x89\x44\x24\x10\x65\xA1\x00\x00\x00\x00\x89\x84\x24\x00\x00\x00\x00",
        "xxxxx????xx????xx????xxx????xxx????xxxxxx????xxx????",
        (void**)&pBuildFunc,
        nullptr
    );
    BBuildAndAsyncSendFrame_Orig = pBuildFunc;
    g_pLogger->LogConsole("CWebSocketConnection::BBuildAndAsyncSendFrame = 0x%x\n", BBuildAndAsyncSendFrame_Orig);

    BBuildAndAsyncSendFrame2Fn pBuildFuncOl = nullptr;
    const bool bFoundBuildOlFunc = g_pClientModule->FindSignature(
        "\x55\x57\x56\x53\xE8\x00\x00\x00\x00\x81\xC3\x00\x00\x00\x00\x81\xEC\x00\x00\x00\x00\x8B\x84\x24\x00\x00\x00\x00\x8B\xBC\x24\x00\x00\x00\x00\x89\x44\x24\x10\x65\xA1\x00\x00\x00\x00",
        "xxxxx????xx????xx????xxx????xxx????xxxxxx????",
        (void**)&pBuildFuncOl,
        (const char*)pBuildFunc
    );
    BBuildAndAsyncSendFrameOl_Orig = pBuildFuncOl;
    g_pLogger->LogConsole("CWebSocketConnection::BBuildAndAsyncSendFrameOl = 0x%x\n", BBuildAndAsyncSendFrameOl_Orig);

    RecvPktFn pRecvPktFunc = nullptr;
    const bool bFoundRecvPktFunc = g_pClientModule->FindSignature(
        "\x55\x57\x56\x53\xE8\x00\x00\x00\x00\x81\xC3\x00\x00\x00\x00\x81\xEC\x00\x00\x00\x00\x8B\x84\x24\x00\x00\x00\x00\x8B\xBC\x24\x00\x00\x00\x00\x89\x44\x24\x54\x65\xA1\x00\x00\x00\x00",
        "xxxxx????xx????xx????xxx????xxx????xxxxxx????",
        (void**)&pRecvPktFunc,
        nullptr
    );
    RecvPkt_Orig = pRecvPktFunc;
    g_pLogger->LogConsole("CCMInterface::RecvPkt = 0x%x\n", RecvPkt_Orig);


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

    if(bFoundBuildOlFunc)
    {
        BBuildAndAsyncSendFrame2Fn thisBuildFuncOl = CNet::BBuildAndAsyncSendFrameOl;

        m_BuildDetour = new CSimpleDetour((void **)&BBuildAndAsyncSendFrameOl_Orig, (void *)thisBuildFuncOl);
        m_BuildDetour->Attach();

        g_pLogger->LogConsole("Detoured CWebSocketConnection::BBuildAndAsyncSendFrameOl!\n");
    }
    else
    {
        g_pLogger->LogConsole("Unable to hook CWebSocketConnection::BBuildAndAsyncSendFrameOl: func scan failed.\n");
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

bool CNet::BBuildAndAsyncSendFrame(void *webSocketConnection, EWebSocketOpCode eWebSocketOpCode, const uint8 *pubData, uint32 cubData)
{
    if (eWebSocketOpCode == EWebSocketOpCode::k_eWebSocketOpCode_Binary)
    {
        g_pLogger->LogNetMessage(ENetDirection::k_eNetOutgoing, pubData, cubData);
    }
    else
    {
        g_pLogger->LogConsole("Sending websocket frame with opcode %d (%s), ignoring\n",
            eWebSocketOpCode, EWebSocketOpCodeToName(eWebSocketOpCode)
        );
    }

    return (*BBuildAndAsyncSendFrame_Orig)(webSocketConnection, eWebSocketOpCode, pubData, cubData);
}

bool CNet::BBuildAndAsyncSendFrameOl(void *webSocketConnection, const uint8 *pubData, uint32 cubData, uint32 uUnk)
{
    g_pLogger->LogNetMessage(ENetDirection::k_eNetOutgoing, pubData, cubData);

    return (*BBuildAndAsyncSendFrameOl_Orig)(webSocketConnection, pubData, cubData, uUnk);
}

void CNet::RecvPkt(void *cmConnection, CNetPacket *pPacket)
{
    g_pLogger->LogNetMessage(ENetDirection::k_eNetIncoming, pPacket->m_pubData, pPacket->m_cubData);

    (*RecvPkt_Orig)(cmConnection, pPacket);
}

}
