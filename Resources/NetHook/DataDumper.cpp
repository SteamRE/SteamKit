#include "DataDumper.h"
#include "logger.h"
#include "utils.h"

#include "zip.h"
#include "bitbuf.h"

#include "steam/clientmsgs.h"
#include "steammessages_base.pb.h"

CDataDumper::CDataDumper() :
	m_uiMsgNum(0)
{
	time_t tCurrentTime;
	time(&tCurrentTime);

	sprintf_s(m_szSessionDir, sizeof( m_szSessionDir ), "%d\\", tCurrentTime);

	g_Logger->CreateDir(m_szSessionDir);
}

void CDataDumper::DataEncrypted(const uint8* pubPlaintextData, uint32 cubPlaintextData)
{
	this->HandleNetMsg(k_eNetOutgoing, (EMsg) *(short *) pubPlaintextData, pubPlaintextData, cubPlaintextData);
}

void CDataDumper::DataDecrypted(const uint8* pubPlaintextData, uint32 cubPlaintextData)
{
	this->HandleNetMsg(k_eNetIncoming, (EMsg) *(short *) pubPlaintextData, pubPlaintextData, cubPlaintextData);
}

bool CDataDumper::HandleNetMsg( ENetDirection eDirection, EMsg eMsg, const uint8 *pData, uint32 cubData )
{
	eMsg = (EMsg)((int)eMsg & (~0x80000000));

	if ( eMsg == k_EMsgMulti )
		return this->MultiplexMsgMulti(eDirection, pData, cubData);

	const char* szFile = this->GetFileName(eDirection, eMsg);
	g_Logger->LogFileData(szFile, pData, cubData);

	g_Logger->LogConsole("Wrote %d bytes to %s\n", cubData, szFile);

	return true;
}

const char* CDataDumper::GetFileName(ENetDirection eDirection, EMsg eMsg)
{
	static char szFileName[MAX_PATH];
	
	sprintf_s(szFileName, sizeof( szFileName ), "%s%d_%s_%d_%s.bin", m_szSessionDir,
		++m_uiMsgNum, (eDirection == k_eNetIncoming ? "in" : "out"), eMsg,
		g_Crypto->GetMessage(eMsg));

	return szFileName;
}

const char* CDataDumper::GetFileName( const char* file )
{
	static char szFileName[MAX_PATH];
	
	sprintf_s(szFileName, sizeof( szFileName ), "%s%s", m_szSessionDir,
		file);

	return szFileName;
}

bool CDataDumper::MultiplexMsgMulti( ENetDirection eDirection, const uint8 *pData, uint32 cubData )
{
	struct ProtoHdr 
	{
		EMsg msg;
		int headerLength;
	};

	
	ProtoHdr *pProtoHdr = (ProtoHdr*) pData;

	g_Logger->LogConsole("Multi: msg %d length %d\n", (pProtoHdr->msg & (~0x80000000)), pProtoHdr->headerLength );

	CMsgProtoBufHeader protoheader;
	protoheader.ParseFromArray( pData + 8, pProtoHdr->headerLength );

	g_Logger->LogConsole("MultiProto\n");

	CMsgMulti multi;
	multi.ParseFromArray( pData + 8 + pProtoHdr->headerLength, cubData - 8 - pProtoHdr->headerLength );

	g_Logger->LogConsole("MultiMsg: %d %d\n", multi.size_unzipped(), multi.message_body().length() );

	uint8 *pMsgData = NULL;
	uint32 cubMsgData = 0;
	bool bDecomp = false;

	if ( multi.has_size_unzipped() && multi.size_unzipped() != 0 )
	{
		// decompress our data

		uint8 *pDecompressed = new uint8[ multi.size_unzipped() ];
		uint8 *pCompressed = (uint8 *)( multi.message_body().c_str() );
		uint32 cubCompressed = multi.message_body().length();

		g_Logger->LogConsole("decomp: %x comp: %x cubcomp: %d unzipped: %d\n", pDecompressed, pCompressed, cubCompressed, multi.size_unzipped());

		bool bZip = CZip::Inflate( pCompressed, cubCompressed, pDecompressed, multi.size_unzipped() );

		if ( !bZip )
		{
			delete [] pDecompressed;

			g_Logger->LogConsole("Unable to decompress buffer\n");

			return true;
		}

		pMsgData = pDecompressed;
		cubMsgData = multi.size_unzipped();
		bDecomp = bZip;
	}
	else
	{
		pMsgData = (uint8 *)( multi.message_body().c_str() );
		cubMsgData = multi.message_body().length();
	}

	bf_read reader( pMsgData, cubMsgData );

	while ( reader.GetNumBytesLeft() > 0 )
	{
		uint32 cubPayload = (uint32)reader.ReadLong();
		int off = reader.GetNumBitsRead() >> 3;

		uint8 *pPayload = (uint8 *)( pMsgData + off );
		EMsg *pEMsg = (EMsg *)pPayload;

		reader.SeekRelative( cubPayload << 3 );

		this->HandleNetMsg( eDirection, *pEMsg, pPayload, cubPayload );
	}

	if ( bDecomp )
		delete [] pMsgData;

	return true;
}