#include "crypto.h"

#include "logger.h"
#include "utils.h"

#include "csimplescan.h"
#include "csimpledetour.h"

#include "steam/steamtypes.h"

#include "tier0/dbg.h"

CCrypto* g_Crypto = NULL;

bool (__cdecl *Encrypt_Orig)(const uint8*, uint32, uint8*, uint32*, uint32);
bool (__cdecl *Decrypt_Orig)(const uint8*, uint32, uint8*, uint32*, const uint8*, uint32);

CCrypto::CCrypto(ICryptoCallback* callback) :
	m_Callback(callback)
{
	CSimpleScan steamClientScan( "steamclient.dll" );

	bool bRet = steamClientScan.FindFunction( 
		"\x55\x8B\xEC\x6A\xFF\x68\x01\x47\x32\x38\x64\xA1\x00\x00\x00\x00\x50\x64\x89\x25\x00\x00\x00\x00\x81\xEC\x04\x09\x00\x00",
		"xxxxxx????xxxxxxxxxxxxxxxxxxxx",
		(void **)& Encrypt_Orig
	);

	g_Logger->LogConsole( "CCrypto::SymmetricEncrypt = 0x%x \n", Encrypt_Orig );


	bRet = steamClientScan.FindFunction(
		"\x55\x8B\xEC\x6A\xFF\x68\x21\x74\x28\x38\x64\xA1\x00\x00\x00\x00\x50\x64\x89\x25\x00\x00\x00\x00\x81\xEC\xE8\x04\x00\x00",
		"xxxxxx????xxxxxxxxxxxxxxxxxxx",
		(void **)& Decrypt_Orig
	);

	g_Logger->LogConsole( "CCrypto::SymmetricDecrypt = 0x%x\n", Decrypt_Orig );

	static bool (__cdecl *encrypt)(const uint8*, uint32, uint8*, uint32*, uint32) = &CCrypto::SymmetricEncrypt;
	static bool (__cdecl *decrypt)(const uint8*, uint32, uint8*, uint32*, const uint8*, uint32) = &CCrypto::SymmetricDecrypt;

	Encrypt_Detour = new CSimpleDetour((void **) &Encrypt_Orig, *(void**) &encrypt);
	Decrypt_Detour = new CSimpleDetour((void **) &Decrypt_Orig, *(void**) &decrypt);

	Encrypt_Detour->Attach();
	Decrypt_Detour->Attach();
}

CCrypto::~CCrypto()
{
	Encrypt_Detour->Detach();
	Decrypt_Detour->Detach();

	delete Encrypt_Detour;
	delete Decrypt_Detour;
}

// This call got strangely optimized. Don't clobber ECX.
__declspec(naked) bool __cdecl CCrypto::SymmetricEncrypt( const uint8 *pubPlaintextData, uint32 cubPlaintextData, uint8 *pubEncryptedData, uint32 *pcubEncryptedData, uint32 cubKey )
{
	__asm {
		// prologue
		push ebp;
		mov ebp, esp;
		sub esp, __LOCAL_SIZE;

		// this needs to be saved (aes key)
		push ecx;
	}

	if ( g_Crypto )
		g_Crypto->m_Callback->DataEncrypted(pubPlaintextData, cubPlaintextData);

	__asm {
		// call it manually here, prevent
		// it from 'helpfully' using ECX

		// prep aes key in register
		pop ecx;

		// push args
		push cubKey;
		push pcubEncryptedData;
		push pubEncryptedData;
		push cubPlaintextData;
		push pubPlaintextData;

		// call!
		call Encrypt_Orig;

		// cleanup
		add esp, 0x14;

		// epilogue
		mov esp, ebp;
		pop ebp;
		ret;
   }
}

// This function is considerably less retarded
bool __cdecl CCrypto::SymmetricDecrypt( const uint8 *pubEncryptedData, uint32 cubEncryptedData, uint8 *pubPlaintextData, uint32 *pcubPlaintextData, const uint8 *pubKey, uint32 cubKey )
{
	bool ret = (*Decrypt_Orig)(pubEncryptedData, cubEncryptedData, pubPlaintextData, pcubPlaintextData, pubKey, cubKey);

	if ( g_Crypto )
		g_Crypto->m_Callback->DataDecrypted(pubPlaintextData, *pcubPlaintextData);

	return ret;
}
