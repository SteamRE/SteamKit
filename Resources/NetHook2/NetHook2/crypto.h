

#ifndef NETHOOK_CRYPTO_H_
#define NETHOOK_CRYPTO_H_

#include "steam/emsg.h"
#include "steam/steamtypes.h"
#include "csimpledetour.h"
#include <map>

#undef GetMessage

typedef bool(__cdecl *SymmetricEncryptChosenIVFn)(const uint8*, uint32, const uint8*, uint32, uint8*, uint32*, const uint8*, uint32);

struct MsgInfo_t
{
	EMsg eMsg;
	uint32 nUnk1;
	int nFlags;
	EServerType k_EServerTarget;
    const char *pchMsgName;
};

typedef std::map<EMsg, MsgInfo_t*> MsgList;

class CCrypto
{

public:
	CCrypto() noexcept;
	~CCrypto();

	const char* GetMessage( EMsg eMsg, uint8 serverType );

	CSimpleDetour* Encrypt_Detour;

	static bool __cdecl SymmetricEncryptChosenIV( const uint8 *pubPlaintextData, uint32 cubPlaintextData, const uint8* pIV, uint32 cubIV, uint8 *pubEncryptedData, uint32 *pcubEncryptedData, const uint8 *pubKey, uint32 cubKey );

	MsgList eMsgList;
};

extern CCrypto* g_pCrypto;

#endif // !NETHOOK_CRYPTO_H_
