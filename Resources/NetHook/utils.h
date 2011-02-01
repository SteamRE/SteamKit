
#ifndef UTILS_H_
#define UTILS_H_
#ifdef _WIN32
#pragma once
#endif


#include "steam/steamtypes.h"
#include "steam/csteamid.h"
#include "steam/udppkt.h"

#include <winsock2.h>



const char *PchStringFromUDPPktHdr( const UDPPktHdr_t *pHdr );
const char *PchStringFromMsgHdr( const MsgHdr_t *pMsgHdr );
const char *PchStringFromExtendedClientMsgHdr( const ExtendedClientMsgHdr_t *pMsgHdr );


const char *PchStringFromData( const uint8 *pData, uint32 cubData );

const char *PchNameFromEMsg( EMsg eMsg );
const char *PchNameFromNetFlags( uint32 netFlags );
const char *PchNameFromEUDPPktType( EUDPPktType eUdpPktType );

const char *PchNameFromEUniverse( EUniverse eUniverse );
const char *PchNameFromEResult( EResult eResult );
const char *PchNameFromEAccountType( EAccountType eAccountType );

const char *PchStringFromSockAddr( const sockaddr_in *sockAddr );


#endif // !UTILS_H_

