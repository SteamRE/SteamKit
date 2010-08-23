

#include "crypto.h"

#include "logger.h"
#include "utils.h"

#include "csimplescan.h"
#include "csimpledetour.h"

#include "steam/steamtypes.h"

#include "tier0/dbg.h"



SETUP_DETOUR_TRAMP( bool, __cdecl, CCrypto_SymmetricEncrypt, ( const uint8 *pubPlaintextData, uint32 cubPlaintextData, uint8 *pubEncryptedData, uint32 *pcubEncryptedData, uint32 cubKey ) );
SETUP_DETOUR_TRAMP( bool, __cdecl, CCrypto_SymmetricDecrypt, ( const uint8 *pubEncryptedData, uint32 cubEncryptedData, uint8 *pubPlaintextData, uint32 *pcubPlaintextData, const uint8 *pubKey, uint32 cubKey ) );


SETUP_DETOUR_FUNCTION_LATE( void, __cdecl, CCrypto_GenerateRandomBlock, ( uint8 *pubDest ) )
{
	g_Logger->LogConsole( "GenerateRandomBlock( 0x%08X )\n", pubDest );

	return g_Crypto->GenerateRandomBlock( pubDest );
}



CCrypto::CCrypto()
{
	m_bSessionGen = m_bCanReset = false;


	CSimpleScan steamClientScan( "steamclient" );


	bool bRet = steamClientScan.FindFunction( 
		"\x55\x8B\xEC\x6A\xFF\x68\x01\x47\x32\x38\x64\xA1\x00\x00\x00\x00\x50\x64\x89\x25\x00\x00\x00\x00\x81\xEC\x04\x09\x00\x00",
		"xxxxxx????xxxxxxxxxxxxxxxxxxxx",
		(void **)&CCrypto_SymmetricEncrypt_T
	);

	g_Logger->LogConsole( "CCrypto::SymmetricEncrypt = %d\n", bRet );


	bRet = steamClientScan.FindFunction(
		"\x55\x8B\xEC\x6A\xFF\x68\x21\x74\x28\x38\x64\xA1\x00\x00\x00\x00\x50\x64\x89\x25\x00\x00\x00\x00\x81\xEC\xE8\x04\x00\x00",
		"xxxxxx????xxxxxxxxxxxxxxxxxxx",
		(void **)&CCrypto_SymmetricDecrypt_T
	);

	g_Logger->LogConsole( "CCrypto::SymmetricDecrypt = %d\n", bRet );


	bRet = steamClientScan.FindFunction(
		"\x55\x8B\xEC\x6A\xFF\x68\xC8\x72\x28\x38\x64\xA1\x00\x00\x00\x00\x50\x64\x89\x25\x00\x00\x00\x00\x81\xEC"
		"\x04\x09\x00\x00\x53\x56\x57",
		"xxxxxx????xxxxxxxxxxxxxxxx"
		"xxxxxxx",
		(void **)&CCrypto_GenerateRandomBlock_T
	);

	g_Logger->LogConsole( "CCrypto::GenerateRandomBlock = %d\n", bRet );


	SETUP_DETOUR_LATE( CCrypto_GenerateRandomBlock );


	Detour_CCrypto_GenerateRandomBlock->Attach();

}

CCrypto::~CCrypto()
{
	Detour_CCrypto_GenerateRandomBlock->Detach();

	delete Detour_CCrypto_GenerateRandomBlock;
}



bool CCrypto::SymmetricEncrypt( const uint8 *pubPlaintextData, uint32 cubPlaintextData, uint8 *pubEncryptedData, uint32 *pcubEncryptedData, uint32 cubKey )
{
	Assert( CCrypto_SymmetricEncrypt_T );

	return CCrypto_SymmetricEncrypt_T( pubPlaintextData, cubPlaintextData, pubEncryptedData, pcubEncryptedData, cubKey);
}

bool CCrypto::SymmetricDecrypt( const uint8 *pubEncryptedData, uint32 cubEncryptedData, uint8 *pubPlaintextData, uint32 *pcubPlaintextData, const uint8 *pubKey, uint32 cubKey )
{
	Assert( CCrypto_SymmetricDecrypt_T );

	return CCrypto_SymmetricDecrypt_T( pubEncryptedData, cubEncryptedData, pubPlaintextData, pcubPlaintextData, pubKey, cubKey );
}

void CCrypto::GenerateRandomBlock( uint8 *pubDest )
{
	Assert( CCrypto_GenerateRandomBlock_T );

	CCrypto_GenerateRandomBlock_T( pubDest );

	if ( !m_bSessionGen || m_bCanReset )
	{
		memcpy_s( m_rghSessionKey, 32, pubDest, 32 );
		m_bSessionGen = true;
		m_bCanReset = false;

		g_Logger->LogConsole( "Gen'd session key: %s\n", PchStringFromData( m_rghSessionKey, 32 ) );
	}
}
