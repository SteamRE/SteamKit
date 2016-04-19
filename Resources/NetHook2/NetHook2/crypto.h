

#ifndef NETHOOK_CRYPTO_H_
#define NETHOOK_CRYPTO_H_

#include "steam/emsg.h"
#include "steam/steamtypes.h"
#include "csimpledetour.h"

#undef GetMessage

typedef bool(__cdecl *SymmetricEncryptWithIVFn)(const uint8*, uint32, const uint8*, uint32, uint8*, uint32*, const uint8*, uint32);
typedef bool(__cdecl *SymmetricDecryptRecoverIVFn)(const uint8*, uint32, uint8*, uint32*, uint8*, uint32, const uint8*, uint32);

class CCrypto
{

public:
	CCrypto();
	~CCrypto();

	const char* GetMessage( EMsg eMsg, uint8 serverType );

	CSimpleDetour* Encrypt_Detour;
	CSimpleDetour* Decrypt_Detour;

	static bool __cdecl SymmetricEncryptWithIV( const uint8 *pubPlaintextData, uint32 cubPlaintextData, const uint8* pIV, uint32 cubIV, uint8 *pubEncryptedData, uint32 *pcubEncryptedData, const uint8 *pubKey, uint32 cubKey );
	static bool __cdecl SymmetricDecryptRecoverIV( const uint8 *pubEncryptedData, uint32 cubEncryptedData, uint8 *pubPlaintextData, uint32 *pcubPlaintextData, uint8 *pubRecoveredIV, uint32 cubRecoveredIV, const uint8 *pubKey, uint32 cubKey );

};

extern CCrypto* g_pCrypto;

#endif // !NETHOOK_CRYPTO_H_
