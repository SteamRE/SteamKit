
#include "udpconnection.h"

#include "logger.h"
#include "crypto.h"

#include "utils.h"

#include "steam/udppkt.h"
#include "steam/csteamid.h"
#include "steam/clientmsgs.h"


const char CUDPConnection::m_szLogFile[] = "UdpLog.txt";


CUDPConnection::CUDPConnection() : m_packetMap( DefLessFunc( uint32 ) )
{
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

	g_Logger->AppendFile( m_szLogFile, "-> Incoming %s", PchStringFromUDPPktHdr( pHdr ) );


	size_t headerSize = sizeof( UDPPktHdr_t );

	pData += headerSize;
	cubData -= headerSize;

	if ( pHdr->m_EUDPPktType == k_EUDPPktTypeDatagram )
	{
		return this->ReceiveDatagram( pHdr, pData, cubData );
	}
	else if ( pHdr->m_EUDPPktType == k_EUDPPktTypeChallenge )
	{
		return this->ReceiveChallenge( pHdr, pData, cubData );
	}
	else if ( pHdr->m_EUDPPktType == k_EUDPPktTypeData )
	{
		return this->ReceiveData( pHdr, pData, cubData );
	}

	g_Logger->AppendFile( m_szLogFile, "Unhandled packet type: %s\r\n\r\n", PchNameFromEUDPPktType( (EUDPPktType)pHdr->m_EUDPPktType ) );

	return true;	
}


bool CUDPConnection::ReceiveDatagram( const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData )
{
	return true;
}

bool CUDPConnection::ReceiveChallenge( const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData )
{
	struct ChallengeData_t
	{
		uint32 m_Challenge1;
		uint32 m_Challenge2;
	};

	ChallengeData_t *pChal = (ChallengeData_t *)pData;

	g_Logger->AppendFile( m_szLogFile,

		"  Challenge Data:\r\n"
		"    m_Challenge1 = %d\r\n"
		"    m_Challenge2 = %d\r\n\r\n\r\n",

		pChal->m_Challenge1,
		pChal->m_Challenge2
		);

	return true;
}

bool CUDPConnection::ReceiveData( const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData )
{
	CNetPacket *pPacket = NULL;

	PacketMapIndex index = m_packetMap.Find( pHdr->m_nMsgStartSeq );


	if ( index != m_packetMap.InvalidIndex() )
	{
		pPacket = m_packetMap[ index ];
	}
	else
	{
		pPacket = new CNetPacket( pHdr->m_nMsgStartSeq, pHdr->m_nPktsInMsg, m_bUsingCrypto );

		m_packetMap.Insert( pHdr->m_nMsgStartSeq, pPacket );
	}

	pPacket->AddData( pHdr, pData, cubData );

	if ( pPacket->IsCompleted() )
		return this->ReceiveNetPacket( pHdr, pPacket );

	return true;
}

bool CUDPConnection::ReceiveNetPacket( const UDPPktHdr_t *pHdr, CNetPacket *pPacket )
{
	uint8 *pData = pPacket->GetData();
	uint32 cubData = pPacket->GetSize();

	m_packetMap.Remove( pHdr->m_nMsgStartSeq );

	// decrypt the data before we handle it
	if ( m_bUsingCrypto )
	{
		uint32 cubDecrypted = cubData * 2;
		uint8 *pDecrypted = new uint8[ cubDecrypted ];

		bool bCrypt = g_Crypto->SymmetricDecrypt( pData, cubData, pDecrypted, &cubDecrypted, g_Crypto->GetSessionKey(), 32 );

		if ( bCrypt )
			cubData = cubDecrypted;
		else
		{
			delete [] pDecrypted;
			delete [] pData;

			g_Logger->AppendFile( "EMsgLog.txt", "Failed crypto!!\r\n" );

			return false;
		}

		delete [] pData;
		pData = pDecrypted;
	}

	// handle the net message
	EMsg *pEMsg = (EMsg *)pData;
	bool bRet = this->HandleNetMsg( *pEMsg, pData, cubData );

	delete [] pData;
	return bRet;
}

bool CUDPConnection::HandleNetMsg( EMsg eMsg, const uint8 *pData, uint32 cubData )
{
	g_Logger->AppendFile( "EMsgLog.txt", "Incoming EMsg: %s\r\n", PchNameFromEMsg( eMsg ) );

	if ( eMsg == k_EMsgChannelEncryptResult )
	{
		MsgHdr_t *pMsgHdr = (MsgHdr_t *)pData;
		MsgChannelEncryptResult_t *pMsgBody = (MsgChannelEncryptResult_t *)( pData + sizeof( MsgHdr_t ) );

		if ( pMsgBody->m_EResult == k_EResultOK )
			m_bUsingCrypto = true;

	}

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
/*
	g_Logger->AppendFile( m_szLogFile, "<- Outgoing %s", PchStringFromUDPPktHdr( pHdr ) );

	if ( pHdr->m_EUDPPktType == k_EUDPPktTypeConnect )
	{
		uint32 *nChallenge = (uint32 *)( pData + sizeof( UDPPktHdr_t ) );
		uint32 nChal =  *nChallenge ^ k_nChallengeMask;
		g_Logger->AppendFile( m_szLogFile,

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

			g_Logger->AppendFile( m_szLogFile, "%s", PchStringFromMsgHdr( pMsgHdr ) );

			if ( pMsgHdr->m_EMsg == k_EMsgChannelEncryptResponse )
			{
				MsgChannelEncryptResponse_t *pMsgEncrypt = (MsgChannelEncryptResponse_t *)( pData + sizeof( UDPPktHdr_t ) + sizeof ( MsgHdr_t ) );

				g_Logger->AppendFile( m_szLogFile, "  MsgChannelEncryptResponse\r\n" );
				g_Logger->AppendFile( m_szLogFile, "    m_unProtocolVer = %u\r\n", pMsgEncrypt->m_unProtocolVer );
				g_Logger->AppendFile( m_szLogFile, "    m_cubEncryptedKey = %u\r\n", pMsgEncrypt->m_cubEncryptedKey );
			}
		
		}
		else if ( m_bUsingCrypto )
		{
			const uint8 *pCryptedData = ( pData + sizeof( UDPPktHdr_t ) );
			uint32 cubDataSize = pHdr->m_cbPkt * 2;
			uint8 *pPlaintextData = new uint8[ cubDataSize ]; // just to be sure!

			bool bCrypt = g_Crypto->SymmetricDecrypt( pCryptedData, pHdr->m_cbPkt, pPlaintextData, &cubDataSize, g_Crypto->GetSessionKey(), 32 );

			ExtendedClientMsgHdr_t *pExtHdr = (ExtendedClientMsgHdr_t *)pPlaintextData;

			g_Logger->AppendFile( m_szLogFile, "%s", PchStringFromExtendedClientMsgHdr( pExtHdr ) );

			delete[] pPlaintextData;
		}
		else
		{
			int cubPktMsg = pHdr->m_cbPkt;
			int cubMsg = pHdr->m_cbMsgData;

			g_Logger->AppendFile( m_szLogFile, "  Data:\r\n    %s", PchStringFromData( pData + sizeof( UDPPktHdr_t ), cubPktMsg ) );
		}
	}


	g_Logger->AppendFile( m_szLogFile, "\r\n\r\n" );
*/
	return true;
}
