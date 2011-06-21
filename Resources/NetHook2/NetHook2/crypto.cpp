

#include "crypto.h"

#include "logger.h"
#include "csimplescan.h"

#include <map>



bool (__cdecl *Encrypt_Orig)(const uint8*, uint32, uint8*, uint32*, const uint8*, uint32) = 0;
bool (__cdecl *Decrypt_Orig)(const uint8*, uint32, uint8*, uint32*, const uint8*, uint32) = 0;
bool (__cdecl *GetMessageFn)( int * ) = 0;



#pragma pack( push, 1 )
struct MsgInfo_t
{
	EMsg emsg;
	const char *name;
	uint32 flags;
	uint32 serverType;

	uint64 unk;
	uint32 unk2;
	uint32 unk3;

	uint64 unk4;
	uint32 unk5;
	uint32 unk6;
};
#pragma pack( pop )

typedef std::map<EMsg, MsgInfo_t *> MsgList;
typedef std::pair<EMsg, MsgInfo_t *> MsgPair;

MsgList eMsgList;



CCrypto::CCrypto()
	: Encrypt_Detour( NULL ), Decrypt_Detour( NULL )
{
	CSimpleScan steamClientScan( "steamclient.dll" );

	bool bEncrypt = steamClientScan.FindFunction(
		"\x55\x8B\xEC\x6A\xFF\x68\x01\x47\x32\x38\x64\xA1\x00\x00\x00\x00\x50\x64\x89\x25\x00\x00\x00\x00\x81\xEC\xD4\x08\x00\x00",
		"xxxxxx????xxxxxxxxxxxxxxxxxxxx",
		(void **)&Encrypt_Orig
	);

	g_pLogger->LogConsole( "CCrypto::SymmetricEncrypt = 0x%x \n", Encrypt_Orig );


	bool bDecrypt = steamClientScan.FindFunction(
		"\x55\x8B\xEC\x6A\xFF\x68\x21\x74\x28\x38\x64\xA1\x00\x00\x00\x00\x50\x64\x89\x25\x00\x00\x00\x00\x81\xEC\xDC\x04\x00\x00",
		"xxxxxx????xxxxxxxxxxxxxxxxxxx",
		(void **)&Decrypt_Orig
	);

	g_pLogger->LogConsole( "CCrypto::SymmetricDecrypt = 0x%x\n", Decrypt_Orig );


	MsgInfo_t *pInfos = (MsgInfo_t *)0x38550B48; // hard coded for now, we'll scan later

	g_pLogger->DeleteFile( "emsg_list.txt", false );
	g_pLogger->DeleteFile( "emsg_list_detailed.txt", false );

	while ( true )
	{
		if ( pInfos->unk != 0 ) // seems all the entries have flags set to 2
			break;

		eMsgList.insert( MsgPair( pInfos->emsg, pInfos ) );

		g_pLogger->LogFile( "emsg_list.txt", false, "\t%s = %d,\r\n", pInfos->name, pInfos->emsg );
		g_pLogger->LogFile( "emsg_list_detailed.txt", false, "\t%s = %d, // flags: %d, server type: %d\r\n", pInfos->name, pInfos->emsg, pInfos->flags, pInfos->serverType );

		pInfos++;
	}

	g_pLogger->LogConsole( "Dumped emsg list!\n" );


	static bool (__cdecl *encrypt)(const uint8*, uint32, uint8*, uint32*, const uint8*, uint32) = &CCrypto::SymmetricEncrypt;
	static bool (__cdecl *decrypt)(const uint8*, uint32, uint8*, uint32*, const uint8*, uint32) = &CCrypto::SymmetricDecrypt;

	if ( bEncrypt )
	{
		Encrypt_Detour = new CSimpleDetour((void **) &Encrypt_Orig, *(void**) &encrypt);
		Encrypt_Detour->Attach();

		g_pLogger->LogConsole( "Detoured SymmetricEncrypt!\n" );
	}
	else
	{
		g_pLogger->LogConsole( "Unable to hook SymmetricEncrypt: Func scan failed.\n" );
	}

	if ( bDecrypt )
	{
		Decrypt_Detour = new CSimpleDetour((void **) &Decrypt_Orig, *(void**) &decrypt);
		Decrypt_Detour->Attach();

		g_pLogger->LogConsole( "Detoured SymmetricDecrypt!\n" );
	}
	else
	{
		g_pLogger->LogConsole( "Unable to hook SymmetricDecrypt: Func scan failed.\n" );
	}
}

CCrypto::~CCrypto()
{

	eMsgList.clear();

	if ( Encrypt_Detour )
	{
		Encrypt_Detour->Detach();
		delete Encrypt_Detour;
	}

	if ( Decrypt_Detour )
	{
		Decrypt_Detour->Detach();
		delete Decrypt_Detour;
	}
}



bool __cdecl CCrypto::SymmetricEncrypt( const uint8 *pubPlaintextData, uint32 cubPlaintextData, uint8 *pubEncryptedData, uint32 *pcubEncryptedData, const uint8 *pubKey, uint32 cubKey )
{
	g_pLogger->LogNetMessage( k_eNetOutgoing, (uint8 *)pubPlaintextData, cubPlaintextData );

	return (*Encrypt_Orig)( pubPlaintextData, cubPlaintextData, pubEncryptedData, pcubEncryptedData, pubKey, cubKey );
}

bool __cdecl CCrypto::SymmetricDecrypt( const uint8 *pubEncryptedData, uint32 cubEncryptedData, uint8 *pubPlaintextData, uint32 *pcubPlaintextData, const uint8 *pubKey, uint32 cubKey )
{
	bool ret = (*Decrypt_Orig)(pubEncryptedData, cubEncryptedData, pubPlaintextData, pcubPlaintextData, pubKey, cubKey);

	g_pLogger->LogNetMessage( k_eNetIncoming, pubPlaintextData, *pcubPlaintextData );

	return ret;
}


const char* CCrypto::GetMessage( EMsg eMsg, uint8 serverType )
{
	for ( MsgList::iterator iter = eMsgList.begin() ; iter != eMsgList.end() ; iter++ )
	{
		if ( iter->first == eMsg )
		{
			return iter->second->name;
		}
	}

	return NULL;
}