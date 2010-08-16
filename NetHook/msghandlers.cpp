
#include "msghandlers.h"

#include "logger.h"
#include "utils.h"


bool DefaultHandler( EMsg eMsg, ENetDirection eDirection, const uint8 *pData, uint32 cubData )
{
	return true;
}

const char *PrintChannelEncrypt( EMsg eMsg, const uint8 *pData, uint32 cubData )
{
	static char szHeader[ 1024 ];
	memset( szHeader, 0, sizeof( szHeader ) );

	switch ( eMsg )
	{
	case k_EMsgChannelEncryptRequest:
		{
			MsgChannelEncryptRequest_t *encRequest = (MsgChannelEncryptRequest_t *)pData;
			sprintf_s( szHeader, sizeof( szHeader ),
				"   MsgChannelEncryptRequest_t\r\n"
				"    m_unProtocolVer: %d\r\n"
				"    m_EUniverse: %s (%d)\r\n",
				encRequest->m_unProtocolVer,
				PchNameFromEUniverse( (EUniverse)encRequest->m_EUniverse ),
				encRequest->m_EUniverse
			);
		}
		break;

	case k_EMsgChannelEncryptResponse:
		{
			MsgChannelEncryptResponse_t *encResponse = (MsgChannelEncryptResponse_t *)pData;
			sprintf_s( szHeader, sizeof( szHeader ),
				"   MsgChannelEncryptResponse_t\r\n"
				"    m_unProtocolVer: %d\r\n"
				"    m_cubEncryptedKey: %d\r\n",
				encResponse->m_unProtocolVer,
				encResponse->m_cubEncryptedKey
			);
		}
		break;

	case k_EMsgChannelEncryptResult:
		{
			MsgChannelEncryptResult_t *encResult = (MsgChannelEncryptResult_t *)pData;
			sprintf_s( szHeader, sizeof( szHeader ),
				"   MsgChannelEncryptResult_t\r\n"
				"    m_EResult: %s (%d)\r\n",
				PchNameFromEResult( (EResult)encResult->m_EResult ),
				encResult->m_EResult
			);
		}
		break;

	}

	return szHeader;
}

DEFINE_MSGHANDLER( MsgChannelEncryptRequest, MsgHdr_t, PrintChannelEncrypt, DefaultHandler ); 
DEFINE_MSGHANDLER( MsgChannelEncryptResponse, MsgHdr_t, PrintChannelEncrypt, DefaultHandler );
DEFINE_MSGHANDLER( MsgChannelEncryptResult, MsgHdr_t, PrintChannelEncrypt, DefaultHandler );

const char *PrintAnonLogOn( EMsg eMsg, const uint8 *pData, uint32 cubData )
{
	static char szHeader[ 1024 ];
	memset( szHeader, 0, sizeof( szHeader ) );

	return szHeader;
}

//DEFINE_MSGHANDLER( MsgClientAnonLogOn, ExtendedClientMsgHdr_t, PrintAnonLogOn, DefaultHandler );
