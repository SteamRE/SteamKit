#include <cstddef>
#include <assert.h>
#include "crypto.h"
#include "logger.h"
#include "clientmodule.h"

namespace NetHook
{

SymmetricEncryptChosenIVFn Encrypt_Orig = nullptr;
PchMsgNameFromEMsgFn PchMsgNameFromEMsg = nullptr;

static_assert(sizeof(MsgInfo_t) == 20, "Wrong size of MsgInfo_t");
static_assert(offsetof(MsgInfo_t, eMsg) == 0, "Wrong offset of MsgInfo_t::eMsg");
static_assert(offsetof(MsgInfo_t, nFlags) == 4, "Wrong offset of MsgInfo_t::nFlags");
static_assert(offsetof(MsgInfo_t, k_EServerTarget) == 8, "Wrong offset of MsgInfo_t::k_EServerTarget");
static_assert(offsetof(MsgInfo_t, nUnk1) == 12, "Wrong offset of MsgInfo_t::uUnk1");
static_assert(offsetof(MsgInfo_t, pchMsgName) == 16, "Wrong offset of MsgInfo_t::pchMsgName");

typedef std::pair<EMsg, MsgInfo_t *> MsgPair;

CCrypto::CCrypto() noexcept
    : Encrypt_Detour( nullptr )
{
    SymmetricEncryptChosenIVFn pEncrypt = nullptr;

    const bool bEncrypt = g_pClientModule->FindSignature(
        "\x55\x8B\xEC\x6A\x01\xFF\x75\x24",
        "xxxxxxxx",
        (void**)&pEncrypt,
        nullptr
    );

    Encrypt_Orig = pEncrypt;

    g_pLogger->LogConsole( "CCrypto::SymmetricEncryptChosenIV = 0x%x\n", Encrypt_Orig );

    const bool bPchMsgNameFromEMsg = g_pClientModule->FindSignature(
        "\x55\x8B\xEC\x51\x56\xE8\x00\x00\x00\x00\x8B\x00\x00\x00\x00\x00\x8B\xF0",
        "xxxxxx????x?????xx",
        (void**)&PchMsgNameFromEMsg,
        nullptr
    );

    if (bPchMsgNameFromEMsg)
    {
        g_pLogger->LogConsole( "PchMsgNameFromEMsg = 0x%x\n", PchMsgNameFromEMsg);
    }
    else
    {
        g_pLogger->LogConsole( "Unable to find PchMsgNameFromEMsg.\n" );
    }

    SymmetricEncryptChosenIVFn encrypt = CCrypto::SymmetricEncryptChosenIV;

    if ( bEncrypt )
    {
        Encrypt_Detour = new CSimpleDetour((void **) &Encrypt_Orig, (void*) encrypt);
        Encrypt_Detour->Attach();

        g_pLogger->LogConsole( "Detoured SymmetricEncryptChosenIV!\n" );
    }
    else
    {
        g_pLogger->LogConsole( "Unable to hook SymmetricEncryptChosenIV: Func scan failed.\n" );
    }
}

CCrypto::~CCrypto()
{
    if ( Encrypt_Detour )
    {
        Encrypt_Detour->Detach();
        delete Encrypt_Detour;
    }
}

bool CCrypto::SymmetricEncryptChosenIV( const uint8 *pubPlaintextData, uint32 cubPlaintextData, const uint8 *pIV, uint32 cubIV, uint8 *pubEncryptedData, uint32 *pcubEncryptedData, const uint8 *pubKey, uint32 cubKey )
{
    g_pLogger->LogNetMessage( ENetDirection::k_eNetOutgoing, pubPlaintextData, cubPlaintextData );

    return (*Encrypt_Orig)( pubPlaintextData, cubPlaintextData, pIV, cubIV, pubEncryptedData, pcubEncryptedData, pubKey, cubKey );
}

const char* CCrypto::GetPchMessage( EMsg eMsg, uint8 serverType )
{
    if(PchMsgNameFromEMsg != nullptr)
    {
        return PchMsgNameFromEMsg(eMsg);
    }

    return nullptr;
}

}
