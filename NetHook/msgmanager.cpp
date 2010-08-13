
#include "msgmanager.h"

#include "logger.h"
#include "utils.h"


CMsgManager *g_pMsgManager = NULL;

const char CMsgManager::m_szLogFile[] = "EMsgLog Detailed.txt";

CMsgManager::CMsgManager() :
	m_Handlers( DefLessFunc( EMsg ) )
{
}


CMsgManager::~CMsgManager()
{
}



void CMsgManager::Register( EMsg eMsg, IMsgHandler *pHandler )
{
	if ( m_Handlers.Find( eMsg ) != m_Handlers.InvalidIndex() )
		return;

	m_Handlers.Insert( eMsg, pHandler );
}

void CMsgManager::Unregister( EMsg eMsg )
{
	if ( m_Handlers.Find( eMsg ) == m_Handlers.InvalidIndex() )
		return;

	m_Handlers.Remove( eMsg );
}


bool CMsgManager::HandleMsg( EMsg eMsg, ENetDirection eDirection, const uint8 *pData, uint32 cubData )
{
	MsgMapIndex indx = m_Handlers.Find( eMsg );
	IMsgHandler *pHandler = NULL;

	if ( indx != m_Handlers.InvalidIndex() )
		pHandler = m_Handlers[ indx ];

	// do our own logging
	g_Logger->AppendFile( m_szLogFile, "%s %s EMsg: %s ( %s)\r\n",
		NET_ARROW_STRING( eDirection ),
		NET_DIRECTION_STRING( eDirection ),
		PchNameFromEMsg( eMsg ),
		PchStringFromData( pData, 4 )
	);

	uint32 headerSize = 0;
	const uint8 *pDataPrinted = pData;
	uint32 cubDataPrinted = cubData;

	if ( pHandler )
	{
		headerSize = pHandler->GetHeaderSize();

		if ( headerSize == sizeof( MsgHdr_t ) )
			g_Logger->AppendFile( m_szLogFile, "%s\r\n", PchStringFromMsgHdr( (MsgHdr_t *)pData ) );
		else if ( headerSize == sizeof( ExtendedClientMsgHdr_t ) )
			g_Logger->AppendFile( m_szLogFile, "%s\r\n", PchStringFromExtendedClientMsgHdr( (ExtendedClientMsgHdr_t *)pData ) );
		else if ( headerSize == 0 )
			; // in case we're not sure what the header is
		else
			g_Logger->AppendFile( m_szLogFile, "  Unexpected header size %d!!\r\n", headerSize );

		pDataPrinted += headerSize;
		cubDataPrinted -= headerSize;

		headerSize = pHandler->GetMsgHeaderSize();

		g_Logger->AppendFile( m_szLogFile, "%s\r\n", pHandler->PrintHeader( eMsg, pDataPrinted, cubDataPrinted ) );

		pDataPrinted += headerSize;
		cubDataPrinted -= headerSize;
	}
	else
	{
		// assume extended
		headerSize = sizeof( ExtendedClientMsgHdr_t );

		g_Logger->AppendFile( m_szLogFile, "%s\r\n", PchStringFromExtendedClientMsgHdr( (ExtendedClientMsgHdr_t *)pData ) );

		pDataPrinted += headerSize;
		cubDataPrinted -= headerSize;
	}

	g_Logger->AppendFile( m_szLogFile, "    %s\r\n\r\n\r\n", PchStringFromData( pDataPrinted, cubDataPrinted ) );

	if ( pHandler )
		return pHandler->HandleMsg( eMsg, eDirection, pData, cubData );

	return true;
}