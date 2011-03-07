

#ifndef NETHOOK_CRYPTO_H_
#define NETHOOK_CRYPTO_H_

#include "steam/emsg.h"
#include "steam/steamtypes.h"
#include "csimpledetour.h"

#undef GetMessage

class CCrypto
{

public:
	CCrypto();
	~CCrypto();

	const char* GetMessage( EMsg eMsg, uint8 serverType );

	CSimpleDetour* Encrypt_Detour;
	CSimpleDetour* Decrypt_Detour;

	static bool __cdecl SymmetricEncrypt( const uint8 *pubPlaintextData, uint32 cubPlaintextData, uint8 *pubEncryptedData, uint32 *pcubEncryptedData, uint32 cubKey );
	static bool __cdecl SymmetricDecrypt( const uint8 *pubEncryptedData, uint32 cubEncryptedData, uint8 *pubPlaintextData, uint32 *pcubPlaintextData, const uint8 *pubKey, uint32 cubKey );

};

extern CCrypto* g_pCrypto;

#endif // !NETHOOK_CRYPTO_H_
