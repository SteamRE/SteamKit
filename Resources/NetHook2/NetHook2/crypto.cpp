#include "crypto.h"

#include "logger.h"
#include "csimplescan.h"

#include <cstddef>

#include <Psapi.h>
#include <assert.h>



SymmetricEncryptChosenIVFn Encrypt_Orig = nullptr;
bool (__cdecl *GetMessageFn)( int * ) = nullptr;


static_assert(sizeof(MsgInfo_t) == 20, "Wrong size of MsgInfo_t");
static_assert(offsetof(MsgInfo_t, eMsg) == 0, "Wrong offset of MsgInfo_t::eMsg");
static_assert(offsetof(MsgInfo_t, pchMsgName) == 16, "Wrong offset of MsgInfo_t::pchMsgName");
static_assert(offsetof(MsgInfo_t, nFlags) == 8, "Wrong offset of MsgInfo_t::nFlags");
static_assert(offsetof(MsgInfo_t, k_EServerTarget) == 12, "Wrong offset of MsgInfo_t::k_EServerTarget");
static_assert(offsetof(MsgInfo_t, nUnk1) == 4, "Wrong offset of MsgInfo_t::uUnk1");

typedef std::pair<EMsg, MsgInfo_t *> MsgPair;


CCrypto::CCrypto() noexcept
	: Encrypt_Detour( nullptr )
{
	CSimpleScan steamClientScan( "steamclient.dll" );


	SymmetricEncryptChosenIVFn pEncrypt = nullptr;
	const bool bEncrypt = steamClientScan.FindFunction(
		"\x55\x8B\xEC\x6A\x01\xFF\x75\x24",
		"xxxxxxxx",
		(void **)&pEncrypt
	);

	Encrypt_Orig = pEncrypt;

	g_pLogger->LogConsole( "CCrypto::SymmetricEncryptChosenIV = 0x%x\n", Encrypt_Orig );

	char *pMessageList = nullptr;
	const bool bGetMessageList = steamClientScan.FindFunction(
		"\xE8\x00\x00\x00\x00\x8D\x8B\x00\x00\x00\x00\xE8\x00\x00\x00\x00\x8D\x8B\x00\x00\x00\x00\xE8\x00\x00\x00\x00\x8B\x35\x00\x00\x00\x00\xBF\x00\x00\x00\x00\x66\xC7\x83\x00\x00\x00\x00\x00\x00\x8D\x64\x24\x00",
		"x????xx????x????xx????x????xx????x????xxx??????xxx?",
		(void **)&pMessageList
	);

	if (bGetMessageList)
	{
        constexpr uint32 uMessageListStartPtrOffset = 34;
		constexpr uint32 uMessageListCountPtrOffset = 277;

		MsgInfo_t *pInfos = *(MsgInfo_t **)( pMessageList + uMessageListStartPtrOffset );
		MsgInfo_t *pInfoLast = *(MsgInfo_t **)( pMessageList + uMessageListCountPtrOffset );

        const uint32 uNumMessages = ((uint32)pInfoLast - (uint32)pInfos) / sizeof(MsgInfo_t);

		g_pLogger->LogConsole( "pGetMessageList = 0x%x\npInfos = 0x%x\nnumMessages = %d\n", pMessageList, pInfos, uNumMessages );
		
		for ( uint16 x = 0 ; x < uNumMessages; x++ )
		{
			eMsgList.insert( MsgPair( pInfos->eMsg, pInfos ) );

			pInfos++;
		}

		if (!eMsgList.empty())
		{
			// should only delete our existing files if we have something new to dump
			g_pLogger->DeleteFile( "emsg_list.txt", false );
			g_pLogger->DeleteFile( "emsg_list_detailed.txt", false );

			const HANDLE hListFile = g_pLogger->OpenFile( "emsg_list.txt", false );
			const HANDLE hListDetailedFile = g_pLogger->OpenFile( "emsg_list_detailed.txt", false );

			for ( MsgList::iterator iter = eMsgList.begin() ; iter != eMsgList.end() ; iter++ )
			{
				const MsgInfo_t * pInfo = iter->second;

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
	g_pLogger->LogNetMessage( ENetDirection::k_eNetOutgoing, pubPlaintextData, cubPlaintextData );

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

	return nullptr;
}
