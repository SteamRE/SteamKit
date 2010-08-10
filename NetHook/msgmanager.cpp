
#include "msgmanager.h"


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

	if ( indx == m_Handlers.InvalidIndex() )
		return true;

	Assert( m_Handlers[ indx ] );

	return m_Handlers[ indx ]->HandleMsg( eDirection, pData, cubData );
}