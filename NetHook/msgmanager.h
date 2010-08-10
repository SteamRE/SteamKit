

#ifndef MSGMANAGER_H_
#define MSGMANAGER_H_
#ifdef _WIN32
#pragma once
#endif


#include "netpacket.h"
#include "udpconnection.h"

#include "steam/emsg.h"

#include "tier1/utlmap.h"


class IMsgHandler
{

public:
	virtual bool HandleMsg( ENetDirection eDirection, const uint8 *pData, uint32 cubData ) = 0;

};


typedef CUtlMap< EMsg, IMsgHandler * > MsgMap;
typedef MsgMap::IndexType_t MsgMapIndex;


class CMsgManager
{

public:
	CMsgManager();
	~CMsgManager();

	void Register( EMsg eMsg, IMsgHandler *pHandler );
	void Unregister( EMsg eMsg );


	bool HandleMsg( EMsg eMsg, ENetDirection eDirection, const uint8 *pData, uint32 cubData );


private:
	MsgMap m_Handlers;

};


extern CMsgManager *g_MsgManager;


#endif // !MSGMANAGER_H_
