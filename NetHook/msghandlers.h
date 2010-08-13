
#ifndef CMSGHANDLERS_H_
#define CMSGHANDLERS_H_
#ifdef _WIN32
#pragma once
#endif


#include "steam/steamtypes.h"
#include "steam/clientmsgs.h"

#include "msgmanager.h"


// preprocessor madness
#define DEFINE_MSGHANDLER( clientmsg, hdr, hdrfunc, handlefunc ) \
	class clientmsg##Handler : public IMsgHandler \
	{ \
	public: \
		clientmsg##Handler() \
		{ \
			this->Register(); \
		}\
		\
		virtual bool HandleMsg( EMsg eMsg, ENetDirection eDirection, const uint8 *pData, uint32 cubData ) \
		{ \
			hdr *pMsgHdr = (hdr *)pData; \
			clientmsg##_t *pClientHdr = (clientmsg##_t *)( pData + sizeof( hdr ) ); \
			return handlefunc( eMsg, eDirection, pData, cubData ); \
		} \
		\
		virtual uint32 GetHeaderSize() { return sizeof( hdr ); } \
		\
		virtual uint32 GetMsgHeaderSize() { return sizeof( clientmsg##_t ); } \
		\
		virtual const char *PrintHeader( EMsg eMsg, const uint8 *pData, uint32 cubData ) \
		{ \
			return hdrfunc( eMsg, pData, cubData );  \
		} \
		\
	private: \
		void Register() \
		{ \
			if ( !g_pMsgManager ) \
				g_pMsgManager = new CMsgManager(); \
			\
			g_pMsgManager->Register( k_E##clientmsg, this ); \
		} \
	}; \
	clientmsg##Handler g_##clientmsg;


#endif // !CMSGHANDLERS_H_
