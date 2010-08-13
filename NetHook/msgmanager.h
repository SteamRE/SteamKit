

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
	virtual bool HandleMsg( EMsg eMsg, ENetDirection eDirection, const uint8 *pData, uint32 cubData ) = 0;
	virtual uint32 GetHeaderSize() = 0;
	virtual uint32 GetMsgHeaderSize() = 0;
	virtual const char *PrintHeader( EMsg eMsg, const uint8 *pData, uint32 cubData ) = 0;
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

	static const char m_szLogFile[];

};


extern CMsgManager *g_pMsgManager;


#endif // !MSGMANAGER_H_
