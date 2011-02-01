#include <iostream>

#include "logger.h"
#include "utils.h"

CLogger* g_Logger;

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
	va_list args;
	va_start( args, szFmt );

	int buffSize = _vscprintf( szFmt, args ) + 1;

	if ( buffSize == 0 )
		return;

	char *szBuff = new char[ buffSize ];
	memset( szBuff, 0, buffSize );
	
	int len = vsprintf_s( szBuff, buffSize, szFmt, args );

	szBuff[ buffSize - 1 ] = 0;

	HANDLE hOutput = GetStdHandle( STD_OUTPUT_HANDLE );

	DWORD numWritten = 0;
	WriteFile( hOutput, szBuff, len, &numWritten, NULL );

	delete [] szBuff;
}


void CLogger::AppendFile( const char *szFileName, const char *szString, ... )
{
	va_list args;
	va_start( args, szString );

	int buffSize = _vscprintf( szString, args ) + 1;

	if ( buffSize == 0 )
		return;

	char *szBuff = new char[ buffSize ];
	memset( szBuff, 0, buffSize );

	int len = vsprintf_s( szBuff, buffSize, szString, args );

	szBuff[ buffSize - 1 ] = 0;

	this->LogFileData( szFileName, (uint8 *)szBuff, len, true );

	delete [] szBuff;
}

void CLogger::LogFileData( const char *szFileName, const uint8 *pData, uint32 cubData, bool bAppend )
{
	DWORD fileFlags = CREATE_ALWAYS;

	if ( bAppend )
		fileFlags = OPEN_ALWAYS;

	HANDLE hFile = CreateFileA( GetFileDir(szFileName), GENERIC_WRITE, FILE_SHARE_READ, NULL, fileFlags, FILE_ATTRIBUTE_NORMAL, NULL );

	if ( bAppend )
		SetFilePointer( hFile, 0, NULL, FILE_END );

	DWORD lNumBytes = 0;
	WriteFile( hFile, pData, cubData, &lNumBytes, NULL );

	CloseHandle( hFile );
}

void CLogger::CreateDir(const char *szDir)
{
	const char* szCreatePath = this->GetFileDir(szDir);

	DWORD dwAttribs = GetFileAttributes(szCreatePath);

	if ( dwAttribs == INVALID_FILE_ATTRIBUTES )
		CreateDirectory(szCreatePath, NULL);
}

const char *CLogger::GetFileDir( const char *szFile )
{
	static char szFilePath[ MAX_PATH ];
	memset( szFilePath, 0, sizeof( szFilePath ) );

	sprintf_s( szFilePath, sizeof( szFilePath ), "%s\\%s", m_szDir, szFile );

	return szFilePath;
}