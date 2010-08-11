
#ifndef CMSGHANDLERS_H_
#define CMSGHANDLERS_H_
#ifdef _WIN32
#pragma once
#endif


#include "steam/steamtypes.h"

#include "msgmanager.h"


#define DEFINE_MSGHANDLER( clientmsg, func ) \
	class clientmsg##Handler : public IMsgHandler \
	{ \
	public: \
		clientmsg##Handler() \
		{ \
			g_pMsgManager->Register( k_E##clientmsg, this ); \
		}\
		\
		virtual bool HandleMsg( ENetDirection eDirection, const uint8 *pData, uint32 cubData ) \
		{ \
			func \
		} \
	}; \
	clientmsg##Handler g_##clientmsg;


#endif // !CMSGHANDLERS_H_
