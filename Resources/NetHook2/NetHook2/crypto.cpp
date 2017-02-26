#include "crypto.h"

#include "logger.h"
#include "csimplescan.h"

#include <cstddef>

#include <map>



SymmetricEncryptChosenIVFn Encrypt_Orig = 0;
SymmetricDecryptRecoverIVFn Decrypt_Orig = 0;
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

	uint64 uUnk1;
};

static_assert(sizeof(MsgInfo_t) == 56, "Wrong size of MsgInfo_t");
static_assert(offsetof(MsgInfo_t, eMsg) == 0, "Wrong offset of MsgInfo_t::eMsg");
static_assert(offsetof(MsgInfo_t, pchMsgName) == 4, "Wrong offset of MsgInfo_t::pchMsgName");
static_assert(offsetof(MsgInfo_t, nFlags) == 8, "Wrong offset of MsgInfo_t::nFlags");
static_assert(offsetof(MsgInfo_t, k_EServerTarget) == 12, "Wrong offset of MsgInfo_t::k_EServerTarget");
static_assert(offsetof(MsgInfo_t, nTimesSent) == 16, "Wrong offset of MsgInfo_t::nTimesSent");
static_assert(offsetof(MsgInfo_t, uBytesSent) == 24, "Wrong offset of MsgInfo_t::uBytesSent");
static_assert(offsetof(MsgInfo_t, nTimesSentProfile) == 32, "Wrong offset of MsgInfo_t::nTimesSentProfile");
static_assert(offsetof(MsgInfo_t, uBytesSentProfile) == 40, "Wrong offset of MsgInfo_t::uBytesSentProfile");
static_assert(offsetof(MsgInfo_t, uUnk1) == 48, "Wrong offset of MsgInfo_t::uUnk1");

typedef std::map<EMsg, MsgInfo_t *> MsgList;
typedef std::pair<EMsg, MsgInfo_t *> MsgPair;

MsgList eMsgList;



CCrypto::CCrypto()
	: Encrypt_Detour( NULL ), Decrypt_Detour( NULL )
{
	CSimpleScan steamClientScan( "steamclient.dll" );


	SymmetricEncryptChosenIVFn pEncrypt = NULL;
	bool bEncrypt = steamClientScan.FindFunction(
		"\x55\x8B\xEC\x6A\x01\xFF\x75\x24",
		"xxxxxxxx",
		(void **)&pEncrypt
	);

	Encrypt_Orig = pEncrypt;

	g_pLogger->LogConsole( "CCrypto::SymmetricEncryptChosenIV = 0x%x\n", Encrypt_Orig );


	SymmetricDecryptRecoverIVFn pDecrypt = NULL;
	bool bDecrypt = steamClientScan.FindFunction(
		"\x55\x8B\xEC\x81\xEC\x04\x01\x00\x00\x83\x7D\x08\x00\x53",
		"xxxxxxxxxxxxxx",
		(void **)&pDecrypt
	);

	Decrypt_Orig = pDecrypt;

	g_pLogger->LogConsole( "CCrypto::SymmetricDecryptRecoverIV = 0x%x\n", Decrypt_Orig );


	char *pGetMessageList = NULL;
	bool bGetMessageList = steamClientScan.FindFunction(
		"\xA1\x00\x00\x00\x00\xA8\x01\x75\x00\x83\xC8\x01\xB9\x00\x00\x00\x00\x0056",
		"x????xxx?xxxx????x",
		(void **)&pGetMessageList
	);

	if (bGetMessageList)
	{
		const uint32 uMessageListStartPtrOffset = 38;
		const uint32 uMessageListEndPtrOffset = uMessageListStartPtrOffset + 26;

		MsgInfo_t *pInfos = *(MsgInfo_t **)( pGetMessageList + uMessageListStartPtrOffset );
		MsgInfo_t *pEndInfos = *(MsgInfo_t **)( pGetMessageList + uMessageListEndPtrOffset );
		uint16 numMessages = ( ( int )pEndInfos - ( int )pInfos ) / sizeof( MsgInfo_t );

		g_pLogger->LogConsole( "pGetMessageList = 0x%x\npInfos = 0x%x\nnumMessages = %d\n", pGetMessageList, pInfos, numMessages );


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

			HANDLE hListFile = g_pLogger->OpenFile( "emsg_list.txt", false );
			HANDLE hListDetailedFile = g_pLogger->OpenFile( "emsg_list_detailed.txt", false );

			for ( MsgList::iterator iter = eMsgList.begin() ; iter != eMsgList.end() ; iter++ )
			{
				MsgInfo_t *pInfo = iter->second;

				g_pLogger->LogOpenFile( hListFile, "\t%s = %d,\r\n", pInfo->pchMsgName, pInfo->eMsg );
				g_pLogger->LogOpenFile( hListDetailedFile, "\t%s = %d, // flags: %d, server type: %d\r\n", pInfo->pchMsgName, pInfo->eMsg, pInfo->nFlags, pInfo->k_EServerTarget );
			}

			g_pLogger->CloseFile( hListFile );
			g_pLogger->CloseFile( hListDetailedFile );

			g_pLogger->LogConsole( "Dumped emsg list! (%d messages)\n", eMsgList.size() );
		}
		else
		{
			g_pLogger->LogConsole( "Unable to dump emsg list: No messages! (Offset changed?)\n" );
		} 
	}
	else
	{
		g_pLogger->LogConsole( "Unable to find GetMessageList.\n" );
	}

	SymmetricEncryptChosenIVFn encrypt = CCrypto::SymmetricEncryptChosenIV;
	SymmetricDecryptRecoverIVFn decrypt = CCrypto::SymmetricDecryptRecoverIV;

	if ( bEncrypt )
	{
		Encrypt_Detour = new CSimpleDetour((void **) &Encrypt_Orig, (void*) encrypt);
		Encrypt_Detour->Attach();

		g_pLogger->LogConsole( "Detoured SymmetricEncryptChosenIV!\n" );
	}
	else
	{
		g_pLogger->LogConsole( "Unable to hook SymmetricEncryptChosenIV: Func scan failed.\n" );
	}

	if ( bDecrypt )
	{
		Decrypt_Detour = new CSimpleDetour((void **) &Decrypt_Orig, (void*) decrypt);
		Decrypt_Detour->Attach();

		g_pLogger->LogConsole( "Detoured SymmetricDecryptRecoverIV!\n" );
	}
	else
	{
		g_pLogger->LogConsole( "Unable to hook SymmetricDecryptRecoverIV: Func scan failed.\n" );
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



bool __cdecl CCrypto::SymmetricEncryptChosenIV( const uint8 *pubPlaintextData, uint32 cubPlaintextData, const uint8 *pIV, uint32 cubIV, uint8 *pubEncryptedData, uint32 *pcubEncryptedData, const uint8 *pubKey, uint32 cubKey )
{
	g_pLogger->LogNetMessage( k_eNetOutgoing, (uint8 *)pubPlaintextData, cubPlaintextData );

	return (*Encrypt_Orig)( pubPlaintextData, cubPlaintextData, pIV, cubIV, pubEncryptedData, pcubEncryptedData, pubKey, cubKey );
}

bool __cdecl CCrypto::SymmetricDecryptRecoverIV( const uint8 *pubEncryptedData, uint32 cubEncryptedData, uint8 *pubPlaintextData, uint32 *pcubPlaintextData, uint8 *pubRecoveredIV, uint32 cubRecoveredIV, const uint8 *pubKey, uint32 cubKey )
{
	bool ret = (*Decrypt_Orig)(pubEncryptedData, cubEncryptedData, pubPlaintextData, pcubPlaintextData, pubRecoveredIV, cubRecoveredIV, pubKey, cubKey);

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
