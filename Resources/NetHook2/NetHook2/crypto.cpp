

#include "crypto.h"

#include "logger.h"
#include "csimplescan.h"

#include <vector>

bool (__cdecl *Encrypt_Orig)(const uint8*, uint32, uint8*, uint32*, uint32) = 0;
bool (__cdecl *Decrypt_Orig)(const uint8*, uint32, uint8*, uint32*, const uint8*, uint32) = 0;
bool (__cdecl *GetMessageFn)( int * ) = 0;

struct EMsg_s
{
	EMsg eMsg;
	uint8 serverType;
};

#if 0
bool HasEMsg( std::vector<EMsg_s> eMsgList, EMsg eMsg )
{
	for ( int x = 0; x < eMsgList.size(); ++x )
	{
		if ( eMsgList[ x ].eMsg == eMsg )
			return true;
	}
	return false;
}
#endif

CCrypto::CCrypto()
	: Encrypt_Detour( NULL ), Decrypt_Detour( NULL )
{
	CSimpleScan steamClientScan( "steamclient.dll" );

	bool bEncrypt = steamClientScan.FindFunction( 
		"\x55\x8B\xEC\x6A\xFF\x68\x01\x47\x32\x38\x64\xA1\x00\x00\x00\x00\x50\x64\x89\x25\x00\x00\x00\x00\x81\xEC\x04\x09\x00\x00",
		"xxxxxx????xxxxxxxxxxxxxxxxxxxx",
		(void **)&Encrypt_Orig
	);

	g_pLogger->LogConsole( "CCrypto::SymmetricEncrypt = 0x%x \n", Encrypt_Orig );


	bool bDecrypt = steamClientScan.FindFunction(
		"\x55\x8B\xEC\x6A\xFF\x68\x21\x74\x28\x38\x64\xA1\x00\x00\x00\x00\x50\x64\x89\x25\x00\x00\x00\x00\x81\xEC\xE8\x04\x00\x00",
		"xxxxxx????xxxxxxxxxxxxxxxxxxx",
		(void **)&Decrypt_Orig
	);

	g_pLogger->LogConsole( "CCrypto::SymmetricDecrypt = 0x%x\n", Decrypt_Orig );

	bool bGetMessage = steamClientScan.FindFunction(
		"\x51\xA1\x00\x95\x32\x38\x8B\x08\x85\xC9\x75\x05\x89\x0C\x24\xEB\x28\xE8",
		"xx????xxxxxxxxxxxx",
		(void **)&GetMessageFn
	);

	g_pLogger->LogConsole( "CMessageList::GetMessage = 0x%x\n", GetMessageFn );


	#define MAX_EMSGS 7699 // rough guess, don't really have a way of getting an exact number

	if ( bGetMessage )
	{

		std::vector<EMsg_s> eMsgList;
		std::vector<EMsg_s>::iterator eMsgIter;

		for ( int x = 0; x < MAX_EMSGS; ++x )
		{
			EMsg eMsg = (EMsg)x;
			const char *szMsg = this->GetMessage( eMsg, 0xFF );

			if ( szMsg == NULL )
				continue;

#if 0
			if ( !HasEMsg( eMsgList, eMsg ) )
#endif
			{
				EMsg_s eMsgInfo = { eMsg, 0xFF };
				eMsgList.push_back( eMsgInfo );
			}

		}

		g_pLogger->DeleteFile( "emsg_list.txt", false );

		for ( int x = 0; x < eMsgList.size(); ++x )
		{
			EMsg_s info = eMsgList[ x ];

			const char *szMsg = this->GetMessage( info.eMsg, info.serverType );

			g_pLogger->LogFile( "emsg_list.txt", false, "\t%s = %d,\r\n", szMsg, info.eMsg );
		}

		g_pLogger->LogConsole( "Dumped EMsg list!\n" );

	}
	else
	{
		g_pLogger->LogConsole( "Unable to dump EMsg list: Func scan failed.\n" );
	}

	

	/*
	static bool (__cdecl *encrypt)(const uint8*, uint32, uint8*, uint32*, uint32) = &CCrypto::SymmetricEncrypt;
	static bool (__cdecl *decrypt)(const uint8*, uint32, uint8*, uint32*, const uint8*, uint32) = &CCrypto::SymmetricDecrypt;
	*/


	if ( bEncrypt )
	{
		Encrypt_Detour = new CSimpleDetour((void **) &Encrypt_Orig, *(void**) &CCrypto::SymmetricEncrypt);
		Encrypt_Detour->Attach();

		g_pLogger->LogConsole( "Detoured SymmetricEncrypt!\n" );
	}
	else
	{
		g_pLogger->LogConsole( "Unable to hook SymmetricEncrypt: Func scan failed.\n" );
	}

	if ( bDecrypt )
	{
		Decrypt_Detour = new CSimpleDetour((void **) &Decrypt_Orig, *(void**) &CCrypto::SymmetricDecrypt);
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

	g_pLogger->LogNetMessage( k_eNetOutgoing, (uint8 *)pubPlaintextData, cubPlaintextData );

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

	g_pLogger->LogNetMessage( k_eNetIncoming, pubPlaintextData, *pcubPlaintextData );

	return ret;
}


const char* CCrypto::GetMessage( EMsg eMsg, uint8 serverType )
{
	static char *szMsg = new char[ 200 ];

	int ieMsg = ( int )eMsg;
	uint32 iServerType = serverType;

	bool bRet = false;

	__asm
	{
		pushad

		lea esi, [ szMsg ]
		mov ecx, 200
		mov edi, ieMsg

		push iServerType

		call GetMessageFn
		mov bRet, al

		popad
	}

	if ( bRet )
		return szMsg;

	return NULL;
}