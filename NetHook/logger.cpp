
#include "logger.h"

#include "utils.h"

#include <iostream>



CLogger::CLogger( const char *szBaseDir )
{
	memset( m_szDir, 0, sizeof( m_szDir ) );

	const char *szLastSlash = strrchr( szBaseDir, '\\' );

	memcpy( m_szDir, szBaseDir, szLastSlash - szBaseDir );

	sprintf_s( m_szDir, MAX_PATH, "%s\\netlogs\\", m_szDir ); // build the netlogs dir

	CreateDirectoryA( m_szDir, NULL );
}

void CLogger::LogPacket( ENetType eNetType, ENetDirection eNetDirection, const uint8 *pData, uint32 cubData, uint32 ip, uint16 port )
{
	if ( cubData == 0 )
	{
		//this->LogConsole( "[ %s ] 0 bytes. Direction: %s\n", PchNameFromENetType( eNetType ), PchNameFromENetDirection( eNetDirection ) );
		return;
	}

	int count = m_typeCounts[ eNetType ];

	static char szBinPath[ MAX_PATH ];
	static char szMetaPath[ MAX_PATH ];

	memset( szBinPath, 0, MAX_PATH );
	memset( szMetaPath, 0, MAX_PATH );

	sprintf_s( szBinPath, MAX_PATH, "%s\\%s\\", m_szDir, PchNameFromENetType( eNetType ) );
	sprintf_s( szMetaPath, MAX_PATH, "%s\\%s\\", m_szDir, PchNameFromENetType( eNetType ) );

	CreateDirectoryA( szBinPath, NULL );

	sprintf_s( szBinPath, MAX_PATH, "%s\\%d_%s.dump", szBinPath, count, PchNameFromENetDirection( eNetDirection ) );
	sprintf_s( szMetaPath, MAX_PATH, "%s\\%d_%s.txt", szMetaPath, count, PchNameFromENetDirection( eNetDirection ) );

	m_typeCounts[ eNetType ]++;

	const char *szIp = NULL;

	if ( ip != 0 )
		szIp = inet_ntoa( *( ( in_addr *)&ip ) );

	this->LogFileData( szBinPath, pData, cubData );
	this->LogFile( szMetaPath, "%s Packet #%d\r\n%d bytes.\r\n%s: %s:%d\r\n",
		PchNameFromENetType( eNetType ),
		count,
		cubData,
		( eNetDirection == eNetDirection_Recv ? "From" : "To" ),
		( szIp == NULL ? "unknown" : szIp ),
		port
	);


	this->LogConsole( "[ %s ] %s %d bytes.\n", PchNameFromENetType( eNetType ), PchNameFromENetDirection( eNetDirection ), cubData );
	if ( ip != 0 )
	{
		this->LogConsole( "  %s %s:%d\n",
			( eNetDirection == eNetDirection_Recv ? "From" : "To" ),
			( szIp == NULL ? "unknown" : szIp ),
			port
		);
	}

	this->LogConsole( "\n" );
}

void CLogger::LogConsole( const char *szFmt, ... )
{
	static char szBuff[ 1024 * 80 ];
	memset( szBuff, 0, sizeof( szBuff ) );

	va_list args;
	va_start( args, szFmt );
	
	int len = vsprintf_s( szBuff, sizeof( szBuff ), szFmt, args );

	HANDLE hOutput = GetStdHandle( STD_OUTPUT_HANDLE );

	DWORD numWritten = 0;
	WriteFile( hOutput, szBuff, len, &numWritten, NULL );
}

void CLogger::LogFile( const char *szFileName, const char *szString, ... )
{
	static char szBuff[ 1024 * 80 ];
	memset( szBuff, 0, sizeof( szBuff ) );

	va_list args;
	va_start( args, szString );

	int len = vsprintf_s( szBuff, sizeof( szBuff ), szString, args );

	this->LogFileData( szFileName, (uint8 *)szBuff, len );
}

void CLogger::LogFileData( const char *szFileName, const uint8 *pData, uint32 cubData, bool bAppend )
{
	DWORD fileFlags = CREATE_ALWAYS;

	if ( bAppend )
		fileFlags = OPEN_ALWAYS;

	HANDLE hFile = CreateFileA( szFileName, GENERIC_WRITE, FILE_SHARE_READ, NULL, fileFlags, FILE_ATTRIBUTE_NORMAL, NULL );

	if ( bAppend )
		SetFilePointer( hFile, 0, NULL, FILE_END );

	DWORD lNumBytes = 0;
	WriteFile( hFile, pData, cubData, &lNumBytes, NULL );

	CloseHandle( hFile );
}
void CLogger::AppendFile( const char *szFileName, const char *szString, ... )
{
	static char szBuff[ 1024 * 100 ];
	memset( szBuff, 0, sizeof( szBuff ) );

	va_list args;
	va_start( args, szString );

	int len = vsprintf_s( szBuff, sizeof( szBuff ), szString, args );

	this->LogFileData( szFileName, (uint8 *)szBuff, len, true );
}

