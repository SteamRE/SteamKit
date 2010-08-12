
#include "msghandlers.h"

#include "logger.h"


struct MsgClientAnonLogOn_t
{

};

DEFINE_MSGHANDLER( MsgClientAnonLogOn,

	// todo
	ExtendedClientMsgHdr_t *extHdr = (ExtendedClientMsgHdr_t *)pData;
	return true;
);
