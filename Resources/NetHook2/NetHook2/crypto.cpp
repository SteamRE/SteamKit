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
static_assert(offsetof(MsgInfo_t, pchMsgName) == 4, "Wrong offset of MsgInfo_t::pchMsgName");
static_assert(offsetof(MsgInfo_t, nFlags) == 8, "Wrong offset of MsgInfo_t::nFlags");
static_assert(offsetof(MsgInfo_t, k_EServerTarget) == 12, "Wrong offset of MsgInfo_t::k_EServerTarget");
static_assert(offsetof(MsgInfo_t, nUnk1) == 16, "Wrong offset of MsgInfo_t::uUnk1");

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

	char *pGetMessageList = nullptr;
	const bool bGetMessageList = steamClientScan.FindFunction(
		"\xA1\x00\x00\x00\x00\xA8\x01\x75\x29\x68\x00\x00\x00\x00\x83\xC8\x01\xB9\x00\x00\x00\x00\x68\x00\x00\x00\x00\xA3\x00\x00\x00\x00\xE8\x00\x00\x00\x00\x68\x00\x00\x00\x00\xE8\x00\x00\x00\x00\x83\xC4\x04\xB8\x00\x00\x00\x00\xC3",
		"x????xxxxx????xxxx????x????x????x????x????x????xxxx????x",
		(void **)&pGetMessageList
	);

	if (bGetMessageList)
	{
		constexpr uint32 uMessageListStartPtrOffset = 23;
		constexpr uint32 uMessageListCountPtrOffset = 10;

		MsgInfo_t *pInfos = *(MsgInfo_t **)( pGetMessageList + uMessageListStartPtrOffset );
		const uint32 uNumMessages = *(uint32 *)( pGetMessageList + uMessageListCountPtrOffset );

		g_pLogger->LogConsole( "pGetMessageList = 0x%x\npInfos = 0x%x\nnumMessages = %d\n", pGetMessageList, pInfos, uNumMessages );
		
		HMODULE sc = GetModuleHandle("steamclient.dll");
		assert(sc != nullptr);
		MODULEINFO modInfo;
		GetModuleInformation(GetCurrentProcess(), sc, &modInfo, sizeof(modInfo));

		const uintptr_t uSteamAddrMin = (uintptr_t)modInfo.lpBaseOfDll;
		const uintptr_t uSteamAddrMax = (uintptr_t)modInfo.lpBaseOfDll + modInfo.SizeOfImage;

		for ( uint16 x = 0 ; x < uNumMessages; x++ )
		{
			const uintptr_t uMsgNameAddr = (uintptr_t)pInfos->pchMsgName;
			if (uMsgNameAddr < uSteamAddrMin || uMsgNameAddr >= uSteamAddrMax)
			{
				g_pLogger->LogConsole("Found bad emsg name. Check size and layout of MsgInfo_t. Aborting emsg lookup.\n");
				break;
			}
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
