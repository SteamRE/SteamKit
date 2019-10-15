
#include "logger.h"

#include <cstdio>
#include <ctime>
#include <sstream>

#include "crypto.h"
#include "zip.h"
#include "binaryreader.h"

#include "steammessages_base.pb.h"


CLogger::CLogger() noexcept
{
	m_uiMsgNum = 0;
	char tempName[ MAX_PATH ];
	GetModuleFileName( nullptr, tempName, MAX_PATH );

	m_RootDir = tempName;
	m_RootDir = m_RootDir.substr( 0, m_RootDir.find_last_of( '\\' ) );
	m_RootDir += "\\nethook\\";

	// create root nethook log directory if it doesn't exist
	CreateDirectoryA( m_RootDir.c_str(), nullptr );

	time_t currentTime;
	time( &currentTime );

	std::ostringstream ss;
	ss << m_RootDir << currentTime << "\\";
	m_LogDir = ss.str();

	// create the session log directory
	CreateDirectoryA( m_LogDir.c_str(), nullptr );
}


void CLogger::LogConsole( const char *szFmt, ... )
{
	va_list args;
	va_start( args, szFmt );

	int buffSize = _vscprintf( szFmt, args ) + 1;

	if ( buffSize <= 1 )
		return;

	char *szBuff = new char[ buffSize ];
	memset( szBuff, 0, buffSize );

	const int len = vsprintf_s( szBuff, buffSize, szFmt, args );

	szBuff[ buffSize - 1 ] = 0;

	HANDLE hOutput = GetStdHandle( STD_OUTPUT_HANDLE );

	DWORD numWritten = 0;
	WriteFile( hOutput, szBuff, len, &numWritten, nullptr );

	delete [] szBuff;
}

void CLogger::DeleteFile( const char *szFileName, bool bSession )
{
	std::string outputFile = ( bSession ? m_LogDir : m_RootDir );
	outputFile += szFileName;

	DeleteFileA( outputFile.c_str() );
}

void CLogger::LogNetMessage( ENetDirection eDirection, const uint8 *pData, uint32 cubData )
{
	EMsg eMsg = (EMsg)*(uint16*)pData;
	eMsg = (EMsg)((int)eMsg & (~0x80000000));

	if ( eMsg == EMsg::k_EMsgMulti )
	{
		this->MultiplexMulti( eDirection, pData, cubData );
		return;
	}

	this->LogSessionData( eDirection, pData, cubData );
}

void CLogger::LogSessionData( ENetDirection eDirection, const uint8 *pData, uint32 cubData )
{
	std::string fullFile = m_LogDir;

	const char *outFile = GetFileNameBase( eDirection, (EMsg)*(uint16*)pData );
	fullFile += outFile;

	std::string fullFileTmp = fullFile + ".tmp";
	std::string fullFileFinal = fullFile + ".bin";
	HANDLE hFile = CreateFile( fullFileTmp.c_str(), GENERIC_WRITE, FILE_SHARE_READ, nullptr, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr );

	DWORD numBytes = 0;
	WriteFile( hFile, pData, cubData, &numBytes, nullptr );

	CloseHandle( hFile );

	MoveFile( fullFileTmp.c_str(), fullFileFinal.c_str() );

	this->LogConsole( "Wrote %d bytes to %s\n", cubData, outFile );
}

HANDLE CLogger::OpenFile( const char *szFileName, bool bSession )
{
	std::string outputFile = ( bSession ? m_LogDir : m_RootDir );
	outputFile += szFileName;

	return CreateFile( outputFile.c_str(), GENERIC_WRITE, FILE_SHARE_READ, nullptr, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr );
}

void CLogger::CloseFile( HANDLE hFile ) noexcept
{
	CloseHandle( hFile );
}

void CLogger::LogOpenFile( HANDLE hFile, const char *szFmt, ... )
{
	va_list args;
	va_start( args, szFmt );

	int buffSize = _vscprintf( szFmt, args ) + 1;

	if ( buffSize <= 1 )
		return;

	char *szBuff = new char[ buffSize ];
	memset( szBuff, 0, buffSize );

	const int len = vsprintf_s( szBuff, buffSize, szFmt, args );

	szBuff[ buffSize - 1 ] = 0;

	SetFilePointer( hFile, 0, nullptr, FILE_END );

	DWORD numBytes = 0;
	WriteFile( hFile, szBuff, len, &numBytes, nullptr );

	delete [] szBuff;
}

const char *CLogger::GetFileNameBase( ENetDirection eDirection, EMsg eMsg, uint8 serverType )
{
	static char szFileName[MAX_PATH];

	sprintf_s(
		szFileName, sizeof( szFileName ),
		"%03u_%s_%d_%s",
		++m_uiMsgNum,
		( eDirection == ENetDirection::k_eNetIncoming ? "in" : "out" ),
		static_cast<int>( eMsg ),
		g_pCrypto->GetMessage( eMsg, serverType )
	);

	return szFileName;
}

void CLogger::MultiplexMulti( ENetDirection eDirection, const uint8 *pData, uint32 cubData )
{
	struct ProtoHdr 
	{
		EMsg msg;
		int headerLength;
	};


	const ProtoHdr *pProtoHdr = (ProtoHdr*) pData;

	g_pLogger->LogConsole("Multi: msg %d length %d\n", (static_cast<int>(pProtoHdr->msg) & (~0x80000000)), pProtoHdr->headerLength );

	CMsgProtoBufHeader protoheader;
	protoheader.ParseFromArray( pData + 8, pProtoHdr->headerLength );

	g_pLogger->LogConsole("MultiProto\n");

	CMsgMulti multi;
	multi.ParseFromArray( pData + 8 + pProtoHdr->headerLength, cubData - 8 - pProtoHdr->headerLength );

	g_pLogger->LogConsole("MultiMsg: %d %d\n", multi.size_unzipped(), multi.message_body().length() );

	uint8 *pMsgData = nullptr;
	uint32 cubMsgData = 0;
	bool bDecomp = false;

	if ( multi.has_size_unzipped() && multi.size_unzipped() != 0 )
	{
		// decompress our data

		uint8 *pDecompressed = new uint8[ multi.size_unzipped() ];
		uint8 *pCompressed = (uint8 *)( multi.message_body().c_str() );
		const uint32 cubCompressed = multi.message_body().length();

		g_pLogger->LogConsole("decomp: %x comp: %x cubcomp: %d unzipped: %d\n", pDecompressed, pCompressed, cubCompressed, multi.size_unzipped());

		const bool bZip = CZip::Inflate( pCompressed, cubCompressed, pDecompressed, multi.size_unzipped() );

		if ( !bZip )
		{
			delete [] pDecompressed;

			g_pLogger->LogConsole("Unable to decompress buffer\n");
			return;
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

	CBinaryReader reader( pMsgData, cubMsgData );

	while ( reader.GetSizeLeft() > 0 )
	{
		const uint32 cubPayload = reader.Read<uint32>();
		const uint8 *pPayload = reader.ReadBytes( cubPayload );

		this->LogNetMessage( eDirection, pPayload, cubPayload );
	}

	if ( bDecomp )
		delete [] pMsgData;
}
