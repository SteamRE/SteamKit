
#ifndef CRYPTO_H_
#define CRYPTO_H_
#ifdef _WIN32
#pragma once
#endif

#define _WINSOCKAPI_ // god damn winsock headers

#include "logger.h"
#include "csimpledetour.h"
#include "steam/steamtypes.h"
#include "steam/emsg.h"
#undef GetMessage

class ICryptoCallback
{
public:
	virtual void DataEncrypted(const uint8* pubPlaintextData, uint32 cubPlaintextData) = 0;
	virtual void DataDecrypted(const uint8* pubPlaintextData, uint32 cubPlaintextData) = 0;
};

class CCrypto
{
public:
	CCrypto(ICryptoCallback* callback);
	~CCrypto();

	const char* GetMessage( EMsg eMsg );

private:
	ICryptoCallback* m_Callback;

	CSimpleDetour* Encrypt_Detour;
	CSimpleDetour* Decrypt_Detour;

	static bool __cdecl SymmetricEncrypt( const uint8 *pubPlaintextData, uint32 cubPlaintextData, uint8 *pubEncryptedData, uint32 *pcubEncryptedData, uint32 cubKey );
	static bool __cdecl SymmetricDecrypt( const uint8 *pubEncryptedData, uint32 cubEncryptedData, uint8 *pubPlaintextData, uint32 *pcubPlaintextData, const uint8 *pubKey, uint32 cubKey );
};

extern CCrypto* g_Crypto;

#endif // !CRYPTO_H_
