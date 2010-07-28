
#include "udpconnection.h"

#include "logger.h"
#include "crypto.h"

#include "utils.h"

#include "steam/udppkt.h"
#include "steam/csteamid.h"
#include "steam/clientmsgs.h"


CUDPConnection::CUDPConnection( const char *szBasePath )
{
	memset( m_szLogPath, 0, sizeof( m_szLogPath ) );

	const char *szLastSlash = strrchr( szBasePath, '\\' );
	memcpy( m_szLogPath, szBasePath, szLastSlash - szBasePath );

	sprintf_s( m_szLogPath, sizeof( m_szLogPath ), "%s\\netlogs", m_szLogPath );

	CreateDirectoryA( m_szLogPath, NULL );

	sprintf_s( m_szLogPath, sizeof( m_szLogPath ), "%s\\UdpLog.txt", m_szLogPath );

	m_bUsingCrypto = false;
}

// incoming
bool CUDPConnection::ReceivePacket( const uint8 *pData, uint32 cubData, const sockaddr_in *sockAddr )
{
	if ( cubData < sizeof( UDPPktHdr_t ) )
	{
		g_Logger->LogConsole( "Got UDP packet smaller than sizeof( UDPPktHdr_t )!\n" );
		return true;
	}

	UDPPktHdr_t *pHdr = (UDPPktHdr_t *)pData;

	if ( pHdr->m_nMagic == -1 )
		return true; // we don't need OOB traffic

	if ( pHdr->m_nMagic != k_nMagic )
	{
		g_Logger->LogConsole( "Got UDP packet with incorrect magic!\n" );
		return true;
	}

	// datagram packets don't seem to be required. v0v
	// if ( pHdr->m_EUDPPktType == k_EUDPPktTypeDatagram )
	// 	return false;

	g_Logger->AppendFile( m_szLogPath, "-> Incoming %s", PchStringFromUDPPktHdr( pHdr ) );

	if ( pHdr->m_EUDPPktType == k_EUDPPktTypeChallenge )
	{
		struct ChallengeData_t
		{
			uint32 m_Challenge1;
			uint32 m_Challenge2;
		};

		ChallengeData_t *pChal = (ChallengeData_t *)( pData + sizeof( UDPPktHdr_t ) );

		g_Logger->AppendFile( m_szLogPath,

			"  Challenge Data:\r\n"
			"    m_Challenge1 = %d\r\n"
			"    m_Challenge2 = %d\r\n",

			pChal->m_Challenge1,
			pChal->m_Challenge2
		);
	}
	else if ( pHdr->m_EUDPPktType == k_EUDPPktTypeData )
	{
		if ( pHdr->m_nSrcConnectionID != 0 && !m_bUsingCrypto )
		{
			MsgHdr_t *pMsgHdr = (MsgHdr_t *)( pData + sizeof( UDPPktHdr_t ) );

			g_Logger->AppendFile( m_szLogPath, "%s", PchStringFromMsgHdr( pMsgHdr ) );

			// server is requesting encryption
			if ( pMsgHdr->m_EMsg == k_EMsgChannelEncryptRequest )
			{
				MsgChannelEncryptRequest_t *pMsgEncrypt = (MsgChannelEncryptRequest_t *)( pData + sizeof( UDPPktHdr_t ) + sizeof ( MsgHdr_t ) );

				g_Logger->AppendFile( m_szLogPath, "  MsgChannelEncryptRequest\r\n" );
				g_Logger->AppendFile( m_szLogPath, "    m_unProtocolVer = %u\r\n", pMsgEncrypt->m_unProtocolVer );
				g_Logger->AppendFile( m_szLogPath, "    m_EUniverse = %s (%u)\r\n", PchNameFromEUniverse( (EUniverse)pMsgEncrypt->m_EUniverse ), pMsgEncrypt->m_EUniverse );

			}

			if ( pMsgHdr->m_EMsg == k_EMsgChannelEncryptResult )
			{
				MsgChannelEncryptResult_t *pMsgEncrypt = (MsgChannelEncryptResult_t *)( pData + sizeof( UDPPktHdr_t ) + sizeof ( MsgHdr_t ) );

				g_Logger->AppendFile( m_szLogPath, "  MsgChannelEncryptResult\r\n" );
				g_Logger->AppendFile( m_szLogPath, "    m_EResult = %u\r\n", pMsgEncrypt->m_EResult );

				g_Logger->AppendFile( m_szLogPath, "  USING CRYPTO NOW!!\r\n" );

				if ( pMsgEncrypt->m_EResult == k_EResultOK )
					m_bUsingCrypto = true; // from this point on everything is using crypto
			}
		}
		else if ( m_bUsingCrypto )
		{
			const uint8 *pCryptedData = ( pData + sizeof( UDPPktHdr_t ) );
			uint32 cubDataSize = pHdr->m_cbMsgData * 2;
			uint8 *pPlaintextData = new uint8[ cubDataSize ]; // just to be sure!

			bool bCrypt = g_Crypto->SymmetricDecrypt( pCryptedData, pHdr->m_cbMsgData, pPlaintextData, &cubDataSize, g_Crypto->GetSessionKey(), 32 );

			ExtendedClientMsgHdr_t *pExtHdr = (ExtendedClientMsgHdr_t *)pPlaintextData;

			g_Logger->AppendFile( m_szLogPath, "%s", PchStringFromExtendedClientMsgHdr( pExtHdr ) );

			delete[] pPlaintextData;
		}
		else
		{
			int cubPktMsg = pHdr->m_cbPkt;
			int cubMsg = pHdr->m_cbMsgData;

			g_Logger->AppendFile( m_szLogPath, "  Data:\r\n    %s", PchStringFromData( pData + sizeof( UDPPktHdr_t ), cubPktMsg ) );
		}

	}

	g_Logger->AppendFile( m_szLogPath, "\r\n\r\n" );

	return true;	
}


// outgoing
bool CUDPConnection::SendPacket( const uint8 *pData, uint32 cubData, const sockaddr_in *sockAddr )
{
	if ( cubData < sizeof( UDPPktHdr_t ) )
	{
		g_Logger->LogConsole( "Got UDP packet smaller than sizeof( UDPPktHdr_t )!\n" );
		return true;
	}

	UDPPktHdr_t *pHdr = (UDPPktHdr_t *)pData;

	if ( pHdr->m_nMagic == -1 )
		return true; // we don't need OOB traffic

	if ( pHdr->m_nMagic != k_nMagic )
	{
		g_Logger->LogConsole( "Got UDP packet with incorrect magic!\n" );
		return true;
	}

	// datagram packets don't seem to be required. v0v
	// if ( pHdr->m_EUDPPktType == k_EUDPPktTypeDatagram )
	// 	return false;

	g_Logger->AppendFile( m_szLogPath, "<- Outgoing %s", PchStringFromUDPPktHdr( pHdr ) );

	if ( pHdr->m_EUDPPktType == k_EUDPPktTypeConnect )
	{
		uint32 *nChallenge = (uint32 *)( pData + sizeof( UDPPktHdr_t ) );
		uint32 nChal =  *nChallenge ^ k_nChallengeMask;
		g_Logger->AppendFile( m_szLogPath,

			"  Challenge Data:\r\n"
			"    nChallenge = %d\r\n",

			nChal
		);
	}
	else if ( pHdr->m_EUDPPktType == k_EUDPPktTypeData )
	{
		if ( pHdr->m_nDstConnectionID != 0 && !m_bUsingCrypto )
		{
			MsgHdr_t *pMsgHdr = (MsgHdr_t *)( pData + sizeof( UDPPktHdr_t ) );

			g_Logger->AppendFile( m_szLogPath, "%s", PchStringFromMsgHdr( pMsgHdr ) );

			if ( pMsgHdr->m_EMsg == k_EMsgChannelEncryptResponse )
			{
				MsgChannelEncryptResponse_t *pMsgEncrypt = (MsgChannelEncryptResponse_t *)( pData + sizeof( UDPPktHdr_t ) + sizeof ( MsgHdr_t ) );

				g_Logger->AppendFile( m_szLogPath, "  MsgChannelEncryptResponse\r\n" );
				g_Logger->AppendFile( m_szLogPath, "    m_unProtocolVer = %u\r\n", pMsgEncrypt->m_unProtocolVer );
				g_Logger->AppendFile( m_szLogPath, "    m_cubEncryptedKey = %u\r\n", pMsgEncrypt->m_cubEncryptedKey );
			}
		
		}
		else if ( m_bUsingCrypto )
		{
			const uint8 *pCryptedData = ( pData + sizeof( UDPPktHdr_t ) );
			uint32 cubDataSize = pHdr->m_cbMsgData * 2;
			uint8 *pPlaintextData = new uint8[ cubDataSize ]; // just to be sure!

			bool bCrypt = g_Crypto->SymmetricDecrypt( pCryptedData, pHdr->m_cbMsgData, pPlaintextData, &cubDataSize, g_Crypto->GetSessionKey(), 32 );

			ExtendedClientMsgHdr_t *pExtHdr = (ExtendedClientMsgHdr_t *)pPlaintextData;

			g_Logger->AppendFile( m_szLogPath, "%s", PchStringFromExtendedClientMsgHdr( pExtHdr ) );

			delete[] pPlaintextData;
		}
		else
		{
			int cubPktMsg = pHdr->m_cbPkt;
			int cubMsg = pHdr->m_cbMsgData;

			g_Logger->AppendFile( m_szLogPath, "  Data:\r\n    %s", PchStringFromData( pData + sizeof( UDPPktHdr_t ), cubPktMsg ) );
		}
	}


	g_Logger->AppendFile( m_szLogPath, "\r\n\r\n" );

	return true;
}
