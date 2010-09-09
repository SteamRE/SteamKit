#include "DataDumper.h"
#include "logger.h"
#include "utils.h"

#include "zip.h"
#include "bitbuf.h"

#include "steam/clientmsgs.h"

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
	//if ( eMsg == k_EMsgMulti )
	//	return this->MultiplexMsgMulti(eDirection, pData, cubData);

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
		PchNameFromEMsg(eMsg));

	return szFileName;
}

bool CDataDumper::MultiplexMsgMulti( ENetDirection eDirection, const uint8 *pData, uint32 cubData )
{
	MsgHdr_t *pMsgHdr = (MsgHdr_t *)pData;
	MsgMulti_t *pMsgBody = (MsgMulti_t *)( pData + sizeof( MsgHdr_t ) );

	size_t hdrSize = sizeof( MsgHdr_t ) + sizeof( MsgMulti_t );

	uint8 *pMsgData = NULL;
	uint32 cubMsgData = 0;
	bool bDecomp = false;

	if ( pMsgBody->m_cubUnzipped != 0 )
	{
		// decompress our data

		uint8 *pDecompressed = new uint8[ pMsgBody->m_cubUnzipped ];
		uint8 *pCompressed = (uint8 *)( pData + hdrSize );
		uint32 cubCompressed = cubData - hdrSize;

		bool bZip = CZip::Inflate( pCompressed, cubCompressed, pDecompressed, pMsgBody->m_cubUnzipped );

		if ( !bZip )
		{
			delete [] pDecompressed;

			g_Logger->AppendFile( "EMsgLog.txt", "Decompression failed!!\r\n" );

			return true;
		}

		pMsgData = pDecompressed;
		cubMsgData = pMsgBody->m_cubUnzipped;
		bDecomp = bZip;
	}
	else
	{
		pMsgData = (uint8 *)( pData + hdrSize );
		cubMsgData = cubData - hdrSize;
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