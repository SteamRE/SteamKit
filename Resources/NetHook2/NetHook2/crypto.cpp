#include "crypto.h"

#include "logger.h"
#include "csimplescan.h"

#include <cstddef>

#include <map>



SymmetricEncryptChosenIVFn Encrypt_Orig = 0;
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
	: Encrypt_Detour( NULL )
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

	char *pGetMessageList = NULL;
	bool bGetMessageList = steamClientScan.FindFunction(
		"\x64\xA1\x2C\x00\x00\x00\x8B\x0D\x2A\x2A\x2A\x2A\x8B\x0C\x88\xA1\x2A\x2A\x2A\x2A\x3B\x81\x04\x00\x00\x00\x7F\x2A\xB8\x2A\x2A\x2A\x2A\xC3\x68\x2A\x2A\x2A\x2A\xE8\x2A\x2A\x2A\x2A\x83\xC4\x04\x83\x3D\x2A\x2A\x2A\x2A\xFF\x75\x2A\x68\xD7\x01\x00\x00",
		"xxxxxxxx????xxxx????xxxxxxx?x????xx????x????xxxxx????xx?xxxxx",
		(void **)&pGetMessageList
	);

	if (bGetMessageList)
	{
		const uint32 uMessageListStartPtrOffset = 62;
		const uint32 uMessageListCountPtrOffset = 57;

		MsgInfo_t *pInfos = *(MsgInfo_t **)( pGetMessageList + uMessageListStartPtrOffset );
		const uint32 uNumMessages = *(uint32 *)( pGetMessageList + uMessageListCountPtrOffset );

		g_pLogger->LogConsole( "pGetMessageList = 0x%x\npInfos = 0x%x\nnumMessages = %d\n", pGetMessageList, pInfos, uNumMessages );


		for ( uint16 x = 0 ; x < uNumMessages; x++ )
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
}

CCrypto::~CCrypto()
{

	eMsgList.clear();

	if ( Encrypt_Detour )
	{
		Encrypt_Detour->Detach();
		delete Encrypt_Detour;
	}
}



bool __cdecl CCrypto::SymmetricEncryptChosenIV( const uint8 *pubPlaintextData, uint32 cubPlaintextData, const uint8 *pIV, uint32 cubIV, uint8 *pubEncryptedData, uint32 *pcubEncryptedData, const uint8 *pubKey, uint32 cubKey )
{
	g_pLogger->LogNetMessage( k_eNetOutgoing, pubPlaintextData, cubPlaintextData );

	return (*Encrypt_Orig)( pubPlaintextData, cubPlaintextData, pIV, cubIV, pubEncryptedData, pcubEncryptedData, pubKey, cubKey );
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
