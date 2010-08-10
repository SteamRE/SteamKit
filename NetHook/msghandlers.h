
#ifndef CMSGHANDLERS_H_
#define CMSGHANDLERS_H_
#ifdef _WIN32
#pragma once
#endif


#include "steam/steamtypes.h"

#include "msgmanager.h"
#include "logger.h"


#define DEFINE_MSGHANDLER( clientmsg, func ) \
	class clientmsg##Handler : public IMsgHandler \
	{ \
	public: \
		clientmsg##Handler() \
		{ \
			g_MsgManager->Register( k_E##clientmsg, this ); \
		}\
		\
		virtual bool HandleMsg( ENetDirection eDirection, const uint8 *pData, uint32 cubData ) \
		{ \
			func \
		} \
	}



DEFINE_MSGHANDLER( MsgChannelEncryptResponse,
	return true;
);


#endif // !CMSGHANDLERS_H_
