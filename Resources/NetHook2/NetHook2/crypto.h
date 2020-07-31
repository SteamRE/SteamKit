

#ifndef NETHOOK_CRYPTO_H_
#define NETHOOK_CRYPTO_H_

#include "steam/emsg.h"
#include "steam/steamtypes.h"
#include "csimpledetour.h"
#include <map>

#undef GetMessage

typedef bool(__cdecl *SymmetricEncryptChosenIVFn)(const uint8*, uint32, const uint8*, uint32, uint8*, uint32*, const uint8*, uint32);
typedef const char* (__cdecl * PchMsgNameFromEMsgFn)(EMsg);

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

	const char* GetMessage( EMsg eMsg, uint8 serverType );

	CSimpleDetour* Encrypt_Detour;

	static bool __cdecl SymmetricEncryptChosenIV( const uint8 *pubPlaintextData, uint32 cubPlaintextData, const uint8* pIV, uint32 cubIV, uint8 *pubEncryptedData, uint32 *pcubEncryptedData, const uint8 *pubKey, uint32 cubKey );
};

extern CCrypto* g_pCrypto;

#endif // !NETHOOK_CRYPTO_H_
