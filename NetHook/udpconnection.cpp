
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


bool CUDPConnection::ReceivePacket( const uint8 *pData, uint32 cubData, const sockaddr_in *sockAddr )
{
	return this->HandlePacket( k_eNetIncoming, pData, cubData, sockAddr );
}

bool CUDPConnection::SendPacket( const uint8 *pData, uint32 cubData, const sockaddr_in *sockAddr )
{
	return this->HandlePacket( k_eNetOutgoing, pData, cubData, sockAddr );
}


bool CUDPConnection::HandlePacket( ENetDirection eDirection, const uint8 *pData, uint32 cubData, const sockaddr_in *sockAddr )
{
	if ( cubData < sizeof( UDPPktHdr_t ) )
	{
		g_Logger->LogConsole( "Got UDP packet smaller than sizeof( UDPPktHdr_t )!\n" );
		return true;
	}

	UDPPktHdr_t *pHdr = (UDPPktHdr_t *)pData;

	if ( pHdr->m_nMagic == -1 )
		return true; // we don't need OOB traffic

	Assert( pHdr->m_nMagic == k_nMagic );

	g_Logger->AppendFile( m_szLogFile, "%s %s %s", NET_ARROW_STRING( eDirection ),NET_DIRECTION_STRING( eDirection ), PchStringFromUDPPktHdr( pHdr ) );

	size_t headerSize = sizeof( UDPPktHdr_t );

	pData += headerSize;
	cubData -= headerSize;

	bool bRet = true;

	if ( pHdr->m_EUDPPktType == k_EUDPPktTypeDatagram )
	{
		bRet = this->HandleDatagram( k_eNetOutgoing, pHdr, pData, cubData );
	}
	else if ( pHdr->m_EUDPPktType == k_EUDPPktTypeChallengeReq )
	{
		bRet = this->HandleChallengeReq( k_eNetOutgoing, pHdr, pData, cubData );
	}
	else if ( pHdr->m_EUDPPktType == k_EUDPPktTypeData )
	{
		bRet = this->HandleData( k_eNetOutgoing, pHdr, pData, cubData );
	}
	else if ( pHdr->m_EUDPPktType == k_EUDPPktTypeConnect )
	{
		bRet = this->HandleConnect( k_eNetOutgoing, pHdr, pData, cubData );
	}
	else
	{
		g_Logger->AppendFile( m_szLogFile, "Unhandled %s packet type: %s\r\n\r\n", NET_DIRECTION_STRING( eDirection ), PchNameFromEUDPPktType( (EUDPPktType)pHdr->m_EUDPPktType ) );
	}

	g_Logger->AppendFile( m_szLogFile, "\r\n" );

	return bRet;
}


bool CUDPConnection::HandleDatagram( ENetDirection eDirection, const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData )
{
	return true;
}

bool CUDPConnection::HandleChallenge( ENetDirection eDirection, const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData )
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
bool CUDPConnection::HandleChallengeReq( ENetDirection eDirection, const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData )
{
	return true;
}
bool CUDPConnection::HandleAccept( ENetDirection eDirection, const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData )
{

	return true;
}
bool CUDPConnection::HandleConnect( ENetDirection eDirection, const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData )
{

	return true;
}
bool CUDPConnection::HandleData( ENetDirection eDirection, const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData )
{
	CNetPacket *pPacket = NULL;

	PacketMap *pMap;

	if ( eDirection == k_eNetIncoming )
		pMap = &m_recvMap;
	else
		pMap = &m_sendMap;

	PacketMapIndex index = pMap->Find( pHdr->m_nMsgStartSeq );

	if ( index != pMap->InvalidIndex() )
		pPacket = pMap->Element( index );
	else
	{
		pPacket = new CNetPacket( pHdr->m_nMsgStartSeq, pHdr->m_nPktsInMsg, m_bUsingCrypto );

		pMap->Insert( pHdr->m_nMsgStartSeq, pPacket );
	}

	pPacket->AddData( pHdr, pData, cubData );

	if ( pPacket->IsCompleted() )
		return this->HandleNetPacket( eDirection, pHdr, pPacket );

	return true;
}

bool CUDPConnection::HandleNetPacket( ENetDirection eDirection, const UDPPktHdr_t *pHdr, CNetPacket *pPacket )
{
	uint8 *pData = pPacket->GetData();
	uint32 cubData = pPacket->GetSize();

	if ( eDirection == k_eNetIncoming )
		m_recvMap.Remove( pHdr->m_nMsgStartSeq );
	else
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
	bool bRet = this->HandleNetMsg( eDirection, *pEMsg, pData, cubData );

	delete [] pData;
	return bRet;
}

bool CUDPConnection::HandleNetMsg( ENetDirection eDirection, EMsg eMsg, const uint8 *pData, uint32 cubData )
{
	if ( eMsg == k_EMsgChannelEncryptResult )
	{
		MsgHdr_t *pMsgHdr = (MsgHdr_t *)pData;
		MsgChannelEncryptResult_t *pMsgBody = (MsgChannelEncryptResult_t *)( pData + sizeof( MsgHdr_t ) );

		if ( pMsgBody->m_EResult == k_EResultOK )
			m_bUsingCrypto = true;

	}

	if ( eMsg == k_EMsgMulti )
		return this->MultiplexMsgMulti( eDirection, pData, cubData );

	g_Logger->AppendFile( "EMsgLog.txt", "%s %s EMsg: %s ( %s)\r\n", NET_ARROW_STRING( eDirection ), NET_DIRECTION_STRING( eDirection ), PchNameFromEMsg( eMsg ), PchStringFromData( pData, 4 ) );

	return true;
}

bool CUDPConnection::MultiplexMsgMulti( ENetDirection eDirection, const uint8 * pData, uint32 cubData )
{

	MsgHdr_t *pMsgHdr = (MsgHdr_t *)pData;
	MsgMulti_t *pMsgBody = (MsgMulti_t *)( pData + sizeof( MsgHdr_t ) );

	size_t hdrSize = sizeof( MsgHdr_t ) + sizeof( MsgMulti_t );

	uint8 *pMsgData = NULL;
	uint32 cubMsgData = 0;
	bool bDecomp = false;

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
		bDecomp = bZip;
	}
	else
	{
		pMsgData = (uint8 *)( pData + hdrSize );
		cubMsgData = cubData - hdrSize;
	}

	bf_read reader( pMsgData, cubMsgData );

	while ( reader.GetNumBytesLeft() > 0 )
	{
		uint32 cubPayload = (uint32)reader.ReadLong();
		int off = reader.GetNumBitsRead() >> 3;

		uint8 *pPayload = (uint8 *)( pMsgData + off );
		EMsg *pEMsg = (EMsg *)pPayload;

		reader.SeekRelative( cubPayload << 3 );

		this->HandleNetMsg( eDirection, *pEMsg, pPayload, cubPayload );
	}

	if ( bDecomp )
		delete [] pMsgData;

	return true;
}
