#ifndef NETHOOK_CRYPTO_H_
#define NETHOOK_CRYPTO_H_

#include "steam/emsg.h"
#include "steam/steamtypes.h"
#include "csimpledetour.h"

namespace NetHook
{

#ifdef __linux__
    typedef bool(*SymmetricEncryptChosenIVFn)(const uint8*, uint32, const uint8*, uint32, uint8*, uint32*, const uint8*, uint32);
    typedef const char* (* PchMsgNameFromEMsgFn)(EMsg);
#elif _WIN32
    typedef bool(__cdecl *SymmetricEncryptChosenIVFn)(const uint8*, uint32, const uint8*, uint32, uint8*, uint32*, const uint8*, uint32);
    typedef const char* (__cdecl * PchMsgNameFromEMsgFn)(EMsg);
#endif

struct MsgInfo_t
{
    EMsg eMsg;
    int nFlags;
    EServerType k_EServerTarget;
    uint32 nUnk1;
    const char* pchMsgName;
};

class CCrypto
{
public:
    CCrypto() noexcept;
    ~CCrypto();

    const char* GetPchMessage( EMsg eMsg, uint8 serverType );
    static bool SymmetricEncryptChosenIV( const uint8 *pubPlaintextData, uint32 cubPlaintextData, const uint8* pIV, uint32 cubIV, uint8 *pubEncryptedData, uint32 *pcubEncryptedData, const uint8 *pubKey, uint32 cubKey );

private:
    CSimpleDetour *Encrypt_Detour;

};

}

extern NetHook::CCrypto *g_pCrypto;


#endif // !NETHOOK_CRYPTO_H_
