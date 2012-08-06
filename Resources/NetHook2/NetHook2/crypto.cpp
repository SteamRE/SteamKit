

#include "crypto.h"

#include "logger.h"
#include "csimplescan.h"

#include <cassert>

#include <map>



bool (__cdecl *Encrypt_Orig)(const uint8*, uint32, const uint8*, uint32, uint8*, uint32*, const uint8*, uint32) = 0;
bool (__cdecl *Decrypt_Orig)(const uint8*, uint32, const uint8*, uint32, uint8*, uint32*, const uint8*, uint32) = 0;
bool (__cdecl *GetMessageFn)( int * ) = 0;



struct MsgInfo_t
{
	EMsg eMsg;
	const char *pchMsgName;
	int nFlags;
	EServerType k_EServerTarget;

	uint32 nTimesSent;
	uint64 uBytesSent;
	
	uint32 nTimesSentProfile;
	uint64 uBytesSentProfile;
};

typedef std::map<EMsg, MsgInfo_t *> MsgList;
typedef std::pair<EMsg, MsgInfo_t *> MsgPair;

MsgList eMsgList;



CCrypto::CCrypto()
	: Encrypt_Detour( NULL ), Decrypt_Detour( NULL )
{

	assert( sizeof( MsgInfo_t ) == 48 ); // god help the padding never change

	CSimpleScan steamClientScan( "steamclient.dll" );


	char *pEncrypt;
	bool bEncrypt = steamClientScan.FindFunction(
		"\x53\x8B\xDC\x83\xEC\x08\x83\xE4\xF8\x83\xC4\x04\x55\x8B\x6B\x04\x89\x6C\x24\x04\x8B\xEC\x6A\xFF\x68\x00\x00\x00\x00"
		"\x64\xA1\x00\x00\x00\x00\x50\x64\x89\x25\x00\x00\x00\x00\x51\x53\x81\xEC\x00\x00\x00\x00\xA1\x00\x00\x00\x00\x8B\x08"
		"\x53\x56\x57\x89\x65\xF0\x85\xC9\x75\x05\x89\x4D\xDC",
		"xxxxxxxxxxxxxxxxxxxxxxxxx????xx????xxxx????xxxx????x????xxxxxxxxxxxxxxx",
		(void **)&pEncrypt
	);

	Encrypt_Orig = (bool (__cdecl *)(const uint8*, uint32, const uint8*, uint32, uint8*, uint32*, const uint8*, uint32))( pEncrypt );

	g_pLogger->LogConsole( "CCrypto::SymmetricEncrypt = 0x%x\n", Encrypt_Orig );


	char *pDecrypt;
	bool bDecrypt = steamClientScan.FindFunction(
		"\x55\x8B\xEC\x6A\xFF\x68\x00\x00\x00\x00\x64\xA1\x00\x00\x00\x00\x50\x64\x89\x25\x00\x00\x00\x00\x81\xEC\x00\x00\x00"
		"\x00\xA1\x00\x00\x00\x00\x8B\x08\x53\x56\x57\x89\x65\xF0\x85\xC9\x75\x05\x89\x4D\xE4",
		"xxxxxx????xx????xxxx????xx????x????xxxxxxxxxxxxxxx",
		(void **)&pDecrypt
	);

	Decrypt_Orig = (bool (__cdecl *)(const uint8*, uint32, const uint8*, uint32, uint8*, uint32*, const uint8*, uint32))( pDecrypt );

	g_pLogger->LogConsole( "CCrypto::SymmetricDecrypt = 0x%x\n", Decrypt_Orig );


	char *pGhettoFunction;

	/*
	.text:3826F3D0 B8 01 00 00 00                          mov     eax, 1
	.text:3826F3D5 84 05 F0 6B 5A 38                       test    byte ptr dword_385A6BF0, al
	.text:3826F3DB 75 47                                   jnz     short loc_3826F424
	.text:3826F3DD 09 05 F0 6B 5A 38                       or      dword_385A6BF0, eax
	.text:3826F3E3 56                                      push    esi
	.text:3826F3E4 B9 A0 6B 5A 38                          mov     ecx, offset unk_385A6BA0


	.text:3826F3E9 E8 E2 B5 EE FF                          call    sub_3815A9D0
	.text:3826F3EE C6 05 EC 6B 5A 38 00                    mov     byte_385A6BEC, 0
	.text:3826F3F5 BE B8 03 58 38                          mov     esi, offset g_pStartMessageList
	.text:3826F3FA 8D 9B 00 00 00 00                       lea     ebx, [ebx+0]
	.text:3826F400
	.text:3826F400                         loc_3826F400:                           ; CODE XREF: sub_3826F3D0+44j
	.text:3826F400 56                                      push    esi
	.text:3826F401 B9 A0 6B 5A 38                          mov     ecx, offset unk_385A6BA0
	.text:3826F406 E8 F5 FB FF FF                          call    sub_3826F000
	.text:3826F40B 83 C6 30                                add     esi, 48
	.text:3826F40E 81 FE 78 4C 58 38                       cmp     esi, offset g_pEndMessageList
	*/

	steamClientScan.FindFunction(
		"\xB8\x01\x00\x00\x00\x84\x05\xF0\x6B\x5A\x38\x75\x47\x09\x05\xF0\x6B\x5A\x38\x56\xB9\xA0\x6B\x5A\x38",
		"xxxxxxx????x?xx????xx????",
		(void **)&pGhettoFunction
	);

	


	MsgInfo_t *pInfos = *(MsgInfo_t **)( pGhettoFunction + 38 );
	MsgInfo_t *pEndInfos = *(MsgInfo_t **)( pGhettoFunction + 64 );
	uint16 numMessages = ( ( int )pEndInfos - ( int )pInfos ) / sizeof( MsgInfo_t );

	g_pLogger->LogConsole( "pGhettoFunction = 0x%x\npInfos = 0x%x\nnumMessages = %d\n", pGhettoFunction, pInfos, numMessages );


	for ( uint16 x = 0 ; x < numMessages; x++ )
	{
		eMsgList.insert( MsgPair( pInfos->eMsg, pInfos ) );

		pInfos++;
	}

	if ( eMsgList.size() != 0 )
	{
		// should only delete our existing files if we have something new to dump
		g_pLogger->DeleteFile( "emsg_list.txt", false );
		g_pLogger->DeleteFile( "emsg_list_detailed.txt", false );

		for ( MsgList::iterator iter = eMsgList.begin() ; iter != eMsgList.end() ; iter++ )
		{
			MsgInfo_t *pInfo = iter->second;

			g_pLogger->LogFile( "emsg_list.txt", false, "\t%s = %d,\r\n", pInfo->pchMsgName, pInfo->eMsg );
			g_pLogger->LogFile( "emsg_list_detailed.txt", false, "\t%s = %d, // flags: %d, server type: %d\r\n", pInfo->pchMsgName, pInfo->eMsg, pInfo->nFlags, pInfo->k_EServerTarget );
		}

		g_pLogger->LogConsole( "Dumped emsg list! (%d messages)\n", eMsgList.size() );
	}
	else
	{
		g_pLogger->LogConsole( "Unable to dump emsg list: No messages! (Offset changed?)\n" );
	}


	static bool (__cdecl *encrypt)(const uint8*, uint32, const uint8*, uint32, uint8*, uint32*, const uint8*, uint32) = &CCrypto::SymmetricEncrypt;
	static bool (__cdecl *decrypt)(const uint8*, uint32, const uint8*, uint32, uint8*, uint32*, const uint8*, uint32) = &CCrypto::SymmetricDecrypt;

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



bool __cdecl CCrypto::SymmetricEncrypt( const uint8 *pubPlaintextData, uint32 cubPlaintextData, const uint8 *pIV, uint32 cubIV, uint8 *pubEncryptedData, uint32 *pcubEncryptedData, const uint8 *pubKey, uint32 cubKey )
{
	g_pLogger->LogNetMessage( k_eNetOutgoing, (uint8 *)pubPlaintextData, cubPlaintextData );

	return (*Encrypt_Orig)( pubPlaintextData, cubPlaintextData, pIV, cubIV, pubEncryptedData, pcubEncryptedData, pubKey, cubKey );
}

bool __cdecl CCrypto::SymmetricDecrypt( const uint8 *pubEncryptedData, uint32 cubEncryptedData, const uint8 *pIV, uint32 cubIV, uint8 *pubPlaintextData, uint32 *pcubPlaintextData, const uint8 *pubKey, uint32 cubKey )
{
	bool ret = (*Decrypt_Orig)(pubEncryptedData, cubEncryptedData, pIV, cubIV, pubPlaintextData, pcubPlaintextData, pubKey, cubKey);

	g_pLogger->LogNetMessage( k_eNetIncoming, pubPlaintextData, *pcubPlaintextData );

	return ret;
}


const char* CCrypto::GetMessage( EMsg eMsg, uint8 serverType )
{
	for ( MsgList::iterator iter = eMsgList.begin() ; iter != eMsgList.end() ; iter++ )
	{
		if ( iter->first == eMsg )
		{
			return iter->second->pchMsgName;
		}
	}

	return NULL;
}