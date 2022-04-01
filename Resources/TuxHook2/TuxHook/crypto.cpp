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
        "\x55\x57\x56\x83\xEC\x08\x8B\x4C\x24\x20\x8B\x44\x24\x18\x8B\x6C\x24\x2C\x8B\x54\x24\x1C\x89\xCE\x8B\x4C\x24\x24\x89\x04\x24\x8B\x44\x24\x34",
        "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
        (void **)&pEncrypt,
        nullptr
    );

    Encrypt_Orig = pEncrypt;

    g_pLogger->LogConsole( "CCrypto::SymmetricEncryptChosenIV = 0x%x\n", Encrypt_Orig );

    const bool bPchMsgNameFromEMsg = g_pClientModule->FindSignature(
        "\x57\x56\x53\xE8\x00\x00\x00\x00\x81\xC3\x00\x00\x00\x00\x83\xEC\x20\x80\xBB\x00\x00\x00\x00\x00\x8D\xB3\x00\x00\x00\x00\x74\x60\x8B\x83\x00\x00\x00\x00",
        "xxxx????xx????xxxxx?????xx????xxxx????",
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
