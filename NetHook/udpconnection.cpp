
#include "udpconnection.h"

#include "logger.h"
#include "crypto.h"
#include "zip.h"

#include "utils.h"

#include "steam/udppkt.h"
#include "steam/csteamid.h"
#include "steam/clientmsgs.h"

#include "bitbuf.h"


const char CUDPConnection::m_szLogFile[] = "UdpLog.txt";


CUDPConnection::CUDPConnection() :
	m_recvMap( DefLessFunc( uint32 ) ),
	m_sendMap( DefLessFunc( uint32 ) )
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
	else if ( pHdr->m_EUDPPktType == k_EUDPPktTypeAccept )
	{
		return this->ReceiveAccept( pHdr, pData, cubData );
	}

	g_Logger->AppendFile( m_szLogFile, "Unhandled packet type: %s\r\n\r\n", PchNameFromEUDPPktType( (EUDPPktType)pHdr->m_EUDPPktType ) );

	return true;	
}


bool CUDPConnection::ReceiveDatagram( const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData )
{
	g_Logger->AppendFile( m_szLogFile, "\r\n" );

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
		"    m_Challenge2 = %d\r\n",

		pChal->m_Challenge1,
		pChal->m_Challenge2
		);

	g_Logger->AppendFile( m_szLogFile, "\r\n" );

	return true;
}

bool CUDPConnection::ReceiveData( const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData )
{
	g_Logger->AppendFile( m_szLogFile, "\r\n" );

	CNetPacket *pPacket = NULL;

	PacketMapIndex index = m_recvMap.Find( pHdr->m_nMsgStartSeq );


	if ( index != m_recvMap.InvalidIndex() )
	{
		pPacket = m_recvMap[ index ];
	}
	else
	{
		pPacket = new CNetPacket( pHdr->m_nMsgStartSeq, pHdr->m_nPktsInMsg, m_bUsingCrypto );

		m_recvMap.Insert( pHdr->m_nMsgStartSeq, pPacket );
	}

	pPacket->AddData( pHdr, pData, cubData );

	if ( pPacket->IsCompleted() )
		return this->ReceiveNetPacket( pHdr, pPacket );

	return true;
}

bool CUDPConnection::ReceiveAccept( const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData )
{
	g_Logger->AppendFile( m_szLogFile, "\r\n" );

	return true;
}

bool CUDPConnection::ReceiveNetPacket( const UDPPktHdr_t *pHdr, CNetPacket *pPacket )
{
	uint8 *pData = pPacket->GetData();
	uint32 cubData = pPacket->GetSize();

	m_recvMap.Remove( pHdr->m_nMsgStartSeq );

	// decrypt the data before we handle it
	if ( m_bUsingCrypto )
	{
		uint32 cubDecrypted = cubData * 2;
		uint8 *pDecrypted = new uint8[ cubDecrypted ];

		bool bCrypt = g_Crypto->SymmetricDecrypt( pData, cubData, pDecrypted, &cubDecrypted, g_Crypto->GetSessionKey(), 32 );

		if ( bCrypt )
		{
			cubData = cubDecrypted;

			delete [] pData;
			pData = pDecrypted;
		}
		else
			g_Logger->AppendFile( "EMsgLog.txt", "Failed crypto!!\r\n" );
	}

	// handle the net message
	EMsg *pEMsg = (EMsg *)pData;
	bool bRet = this->ReceiveNetMsg( *pEMsg, pData, cubData );

	delete [] pData;
	return bRet;
}

bool CUDPConnection::ReceiveNetMsg( EMsg eMsg, const uint8 *pData, uint32 cubData )
{
	if ( eMsg != k_EMsgMulti )
	{
		g_Logger->AppendFile( "EMsgLog.txt", "Incoming EMsg: %s ( %s)\r\n", PchNameFromEMsg( eMsg ), PchStringFromData( pData, 4 ) );
		//g_Logger->AppendFile( "EMsgLog.txt", "  Data: %s\r\n\r\n", PchStringFromData( pData, cubData ) );
	}

	if ( eMsg == k_EMsgChannelEncryptResult )
	{
		MsgHdr_t *pMsgHdr = (MsgHdr_t *)pData;
		MsgChannelEncryptResult_t *pMsgBody = (MsgChannelEncryptResult_t *)( pData + sizeof( MsgHdr_t ) );

		if ( pMsgBody->m_EResult == k_EResultOK )
			m_bUsingCrypto = true;

	}


	if ( eMsg == k_EMsgMulti )
	{
		MsgHdr_t *pMsgHdr = (MsgHdr_t *)pData;
		MsgMulti_t *pMsgBody = (MsgMulti_t *)( pData + sizeof( MsgHdr_t ) );

		size_t hdrSize = sizeof( MsgHdr_t ) + sizeof( MsgMulti_t );

		uint8 *pMsgData = NULL;
		uint32 cubMsgData = 0;

		if ( pMsgBody->m_cubUnzipped != 0 )
		{
			// decompress our data

			uint8 *pDecompressed = new uint8[ pMsgBody->m_cubUnzipped ];
			uint8 *pCompressed = (uint8 *)( pData + hdrSize );
			uint32 cubCompressed = cubData - hdrSize;

			bool bZip = CZip::Inflate( pCompressed, cubCompressed, pDecompressed, pMsgBody->m_cubUnzipped );

			if ( !bZip )
			{
				delete [] pDecompressed;

				g_Logger->AppendFile( "EMsgLog.txt", "Decompression failed!!\r\n" );

				return true;
			}

			pMsgData = pDecompressed;
			cubMsgData = pMsgBody->m_cubUnzipped;
		}
		else
		{
			pMsgData = (uint8 *)( pData + hdrSize );
			cubMsgData = cubData - hdrSize;
		}

		bf_read reader( pMsgData, cubMsgData );

		//g_Logger->AppendFile( "EMsgLog.txt", "Data: %s\r\n", PchStringFromData( pMsgData, cubMsgData ) );

		
		while ( reader.GetNumBytesLeft() > 0 )
		{
			uint32 cubPayload = (uint32)reader.ReadLong();
			int off = reader.GetNumBitsRead() >> 3;

			uint8 *pPayload = (uint8 *)( pMsgData + off );
			EMsg *pEMsg = (EMsg *)pPayload;

			reader.SeekRelative( cubPayload << 3 );

			this->ReceiveNetMsg( *pEMsg, pPayload, cubPayload );
		}

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

	g_Logger->AppendFile( m_szLogFile, "<- Outgoing %s", PchStringFromUDPPktHdr( pHdr ) );

	size_t headerSize = sizeof( UDPPktHdr_t );

	pData += headerSize;
	cubData -= headerSize;

	if ( pHdr->m_EUDPPktType == k_EUDPPktTypeDatagram )
	{
		return this->SendDatagram( pHdr, pData, cubData );
	}
	else if ( pHdr->m_EUDPPktType == k_EUDPPktTypeChallengeReq )
	{
		return this->SendChallengeReq( pHdr, pData, cubData );
	}
	else if ( pHdr->m_EUDPPktType == k_EUDPPktTypeData )
	{
		return this->SendData( pHdr, pData, cubData );
	}
	else if ( pHdr->m_EUDPPktType == k_EUDPPktTypeConnect )
	{
		return this->SendConnect( pHdr, pData, cubData );
	}

	g_Logger->AppendFile( m_szLogFile, "Unhandled packet type: %s\r\n\r\n", PchNameFromEUDPPktType( (EUDPPktType)pHdr->m_EUDPPktType ) );

	return true;
}


bool CUDPConnection::SendChallengeReq( const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData )
{
	g_Logger->AppendFile( m_szLogFile, "\r\n" );

	return true;
}
bool CUDPConnection::SendDatagram( const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData )
{
	g_Logger->AppendFile( m_szLogFile, "\r\n" );

	return true;
}
bool CUDPConnection::SendData( const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData )
{

	g_Logger->AppendFile( m_szLogFile, "\r\n" );

	CNetPacket *pPacket = NULL;

	PacketMapIndex index = m_sendMap.Find( pHdr->m_nMsgStartSeq );


	if ( index != m_sendMap.InvalidIndex() )
	{
		pPacket = m_sendMap[ index ];
	}
	else
	{
		pPacket = new CNetPacket( pHdr->m_nMsgStartSeq, pHdr->m_nPktsInMsg, m_bUsingCrypto );

		m_sendMap.Insert( pHdr->m_nMsgStartSeq, pPacket );
	}

	pPacket->AddData( pHdr, pData, cubData );

	if ( pPacket->IsCompleted() )
		return this->SendNetPacket( pHdr, pPacket );


	return true;
}

bool CUDPConnection::SendConnect( const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData )
{
	g_Logger->AppendFile( m_szLogFile, "\r\n" );

	return true;
}

bool CUDPConnection::SendNetPacket( const UDPPktHdr_t *pHdr, CNetPacket *pPacket )
{
	uint8 *pData = pPacket->GetData();
	uint32 cubData = pPacket->GetSize();

	m_sendMap.Remove( pHdr->m_nMsgStartSeq );

	// decrypt the data before we handle it
	if ( m_bUsingCrypto )
	{
		uint32 cubDecrypted = cubData * 2;
		uint8 *pDecrypted = new uint8[ cubDecrypted ];

		bool bCrypt = g_Crypto->SymmetricDecrypt( pData, cubData, pDecrypted, &cubDecrypted, g_Crypto->GetSessionKey(), 32 );

		if ( bCrypt )
		{
			cubData = cubDecrypted;

			delete [] pData;
			pData = pDecrypted;
		}
		else
			g_Logger->AppendFile( "EMsgLog.txt", "Failed crypto!!\r\n" );
	}

	// handle the net message
	EMsg *pEMsg = (EMsg *)pData;
	bool bRet = this->SendNetMsg( *pEMsg, pData, cubData );

	delete [] pData;
	return bRet;
}

bool CUDPConnection::SendNetMsg( EMsg eMsg, const uint8 *pData, uint32 cubData )
{
	g_Logger->AppendFile( "EMsgLog.txt", "Outgoing EMsg: %s ( %s)\r\n", PchNameFromEMsg( eMsg ), PchStringFromData( pData, 4 ) );

	//g_Logger->AppendFile( "EMsgLog.txt", "  Data: %s\r\n", PchStringFromData( pData, cubData ) );

	return true;
}