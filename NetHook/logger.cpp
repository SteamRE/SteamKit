
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

	this->LogFileData( GetFileDir( szFileName ), (uint8 *)szBuff, len );
}

void CLogger::AppendFile( const char *szFileName, const char *szString, ... )
{
	static char szBuff[ 1024 * 100 ];
	memset( szBuff, 0, sizeof( szBuff ) );

	va_list args;
	va_start( args, szString );

	int len = vsprintf_s( szBuff, sizeof( szBuff ), szString, args );

	this->LogFileData( GetFileDir( szFileName ), (uint8 *)szBuff, len, true );
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


const char *CLogger::GetFileDir( const char *szFile )
{
	static char szFilePath[ MAX_PATH ];
	memset( szFilePath, 0, sizeof( szFilePath ) );

	sprintf_s( szFilePath, sizeof( szFilePath ), "%s\\%s", m_szDir, szFile );

	return szFilePath;
}