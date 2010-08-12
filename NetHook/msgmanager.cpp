
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
	// do our own logging
	g_Logger->AppendFile( m_szLogFile, "%s %s EMsg: %s ( %s)\r\n",
		NET_ARROW_STRING( eDirection ),
		NET_DIRECTION_STRING( eDirection ),
		PchNameFromEMsg( eMsg ),
		PchStringFromData( pData, 4 )
	);

	g_Logger->AppendFile( m_szLogFile, "    %s\r\n\r\n", PchStringFromData( pData, cubData ) );



	// check against registered handlers
	MsgMapIndex indx = m_Handlers.Find( eMsg );

	if ( indx == m_Handlers.InvalidIndex() )
		return true;

	Assert( m_Handlers[ indx ] );

	return m_Handlers[ indx ]->HandleMsg( eMsg, eDirection, pData, cubData );
}