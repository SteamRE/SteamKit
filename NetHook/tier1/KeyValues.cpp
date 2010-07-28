//========= Copyright © 1996-2005, Valve Corporation, All rights reserved. ============//
//
// Purpose: 
//
// $NoKeywords: $
//
//=============================================================================//

#if defined( _WIN32 ) && !defined( _X360 )
#include <windows.h>		// for WideCharToMultiByte and MultiByteToWideChar
#elif defined(_LINUX)
#include <wchar.h> // wcslen()
#define _alloca alloca
#endif

#include <KeyValues.h>
#include "filesystem.h"
#include <vstdlib/IKeyValuesSystem.h>

#include <Color.h>
#include <stdlib.h>
#include "tier0/dbg.h"
#include "tier0/mem.h"
#include "utlvector.h"
#include "utlbuffer.h"

// memdbgon must be the last include file in a .cpp file!!!
#include <tier0/memdbgon.h>

static char * s_LastFileLoadingFrom = "unknown"; // just needed for error messages

#define KEYVALUES_TOKEN_SIZE	1024
static char s_pTokenBuf[KEYVALUES_TOKEN_SIZE];


#define INTERNALWRITE( pData, len ) InternalWrite( filesystem, f, pBuf, pData, len )


// a simple class to keep track of a stack of valid parsed symbols
const int MAX_ERROR_STACK = 64;
class CKeyValuesErrorStack
{
public:
	CKeyValuesErrorStack() : m_pFilename("NULL"), m_errorIndex(0), m_maxErrorIndex(0) {}

	void SetFilename( const char *pFilename )
	{
		m_pFilename = pFilename;
		m_maxErrorIndex = 0;
	}

	// entering a new keyvalues block, save state for errors
	// Not save symbols instead of pointers because the pointers can move!
	int Push( int symName )
	{
		if ( m_errorIndex < MAX_ERROR_STACK )
		{
			m_errorStack[m_errorIndex] = symName;
		}
		m_errorIndex++;
		m_maxErrorIndex = max( m_maxErrorIndex, (m_errorIndex-1) );
		return m_errorIndex-1;
	}

	// exiting block, error isn't in this block, remove.
	void Pop()
	{
		m_errorIndex--;
		Assert(m_errorIndex>=0);
	}

	// Allows you to keep the same stack level, but change the name as you parse peers
	void Reset( int stackLevel, int symName )
	{
		Assert( stackLevel >= 0 && stackLevel < m_errorIndex );
		m_errorStack[stackLevel] = symName;
	}

	// Hit an error, report it and the parsing stack for context
	void ReportError( const char *pError )
	{
		Warning( "KeyValues Error: %s in file %s\n", pError, m_pFilename );
		for ( int i = 0; i < m_maxErrorIndex; i++ )
		{
			if ( m_errorStack[i] != INVALID_KEY_SYMBOL )
			{
				if ( i < m_errorIndex )
				{
					Warning( "%s, ", KeyValuesSystem()->GetStringForSymbol(m_errorStack[i]) );
				}
				else
				{
					Warning( "(*%s*), ", KeyValuesSystem()->GetStringForSymbol(m_errorStack[i]) );
				}
			}
		}
		Warning( "\n" );
	}

private:
	int		m_errorStack[MAX_ERROR_STACK];
	const char *m_pFilename;
	int		m_errorIndex;
	int		m_maxErrorIndex;
} g_KeyValuesErrorStack;


// a simple helper that creates stack entries as it goes in & out of scope
class CKeyErrorContext
{
public:
	CKeyErrorContext( KeyValues *pKv )
	{
		Init( pKv->GetNameSymbol() );
	}

	~CKeyErrorContext()
	{
		g_KeyValuesErrorStack.Pop();
	}
	CKeyErrorContext( int symName )
	{
		Init( symName );
	}
	void Reset( int symName )
	{
		g_KeyValuesErrorStack.Reset( m_stackLevel, symName );
	}
private:
	void Init( int symName )
	{
		m_stackLevel = g_KeyValuesErrorStack.Push( symName );
	}

	int m_stackLevel;
};

// Uncomment this line to hit the ~CLeakTrack assert to see what's looking like it's leaking
// #define LEAKTRACK

#ifdef LEAKTRACK

class CLeakTrack
{
public:
	CLeakTrack()
	{
	}
	~CLeakTrack()
	{
		if ( keys.Count() != 0 )
		{
			Assert( 0 );
		}
	}

	struct kve
	{
		KeyValues *kv;
		char		name[ 256 ];
	};

	void AddKv( KeyValues *kv, char const *name )
	{
		kve k;
		Q_strncpy( k.name, name ? name : "NULL", sizeof( k.name ) );
		k.kv = kv;

		keys.AddToTail( k );
	}

	void RemoveKv( KeyValues *kv )
	{
		int c = keys.Count();
		for ( int i = 0; i < c; i++ )
		{
			if ( keys[i].kv == kv )
			{
				keys.Remove( i );
				break;
			}
		}
	}

	CUtlVector< kve > keys;
};

static CLeakTrack track;

#define TRACK_KV_ADD( ptr, name )	track.AddKv( ptr, name )
#define TRACK_KV_REMOVE( ptr )		track.RemoveKv( ptr )

#else

#define TRACK_KV_ADD( ptr, name ) 
#define TRACK_KV_REMOVE( ptr )	

#endif

//-----------------------------------------------------------------------------
// Purpose: Constructor
//-----------------------------------------------------------------------------
KeyValues::KeyValues( const char *setName )
{
	TRACK_KV_ADD( this, setName );

	Init();
	SetName ( setName );
}

//-----------------------------------------------------------------------------
// Purpose: Constructor
//-----------------------------------------------------------------------------
KeyValues::KeyValues( const char *setName, const char *firstKey, const char *firstValue )
{
	TRACK_KV_ADD( this, setName );

	Init();
	SetName( setName );
	SetString( firstKey, firstValue );
}

//-----------------------------------------------------------------------------
// Purpose: Constructor
//-----------------------------------------------------------------------------
KeyValues::KeyValues( const char *setName, const char *firstKey, const wchar_t *firstValue )
{
	TRACK_KV_ADD( this, setName );

	Init();
	SetName( setName );
	SetWString( firstKey, firstValue );
}

//-----------------------------------------------------------------------------
// Purpose: Constructor
//-----------------------------------------------------------------------------
KeyValues::KeyValues( const char *setName, const char *firstKey, int firstValue )
{
	TRACK_KV_ADD( this, setName );

	Init();
	SetName( setName );
	SetInt( firstKey, firstValue );
}

//-----------------------------------------------------------------------------
// Purpose: Constructor
//-----------------------------------------------------------------------------
KeyValues::KeyValues( const char *setName, const char *firstKey, const char *firstValue, const char *secondKey, const char *secondValue )
{
	TRACK_KV_ADD( this, setName );

	Init();
	SetName( setName );
	SetString( firstKey, firstValue );
	SetString( secondKey, secondValue );
}

//-----------------------------------------------------------------------------
// Purpose: Constructor
//-----------------------------------------------------------------------------
KeyValues::KeyValues( const char *setName, const char *firstKey, int firstValue, const char *secondKey, int secondValue )
{
	TRACK_KV_ADD( this, setName );

	Init();
	SetName( setName );
	SetInt( firstKey, firstValue );
	SetInt( secondKey, secondValue );
}

//-----------------------------------------------------------------------------
// Purpose: Initialize member variables
//-----------------------------------------------------------------------------
void KeyValues::Init()
{
	m_iKeyName = INVALID_KEY_SYMBOL;
	m_iDataType = TYPE_NONE;

	m_pSub = NULL;
	m_pPeer = NULL;
	m_pChain = NULL;

	m_sValue = NULL;
	m_wsValue = NULL;
	m_pValue = NULL;
	
	m_bHasEscapeSequences = false;

	// for future proof
	memset( unused, 0, sizeof(unused) );
}

//-----------------------------------------------------------------------------
// Purpose: Destructor
//-----------------------------------------------------------------------------
KeyValues::~KeyValues()
{
	TRACK_KV_REMOVE( this );

	RemoveEverything();
}

//-----------------------------------------------------------------------------
// Purpose: remove everything
//-----------------------------------------------------------------------------
void KeyValues::RemoveEverything()
{
	KeyValues *dat;
	KeyValues *datNext = NULL;
	for ( dat = m_pSub; dat != NULL; dat = datNext )
	{
		datNext = dat->m_pPeer;
		dat->m_pPeer = NULL;
		delete dat;
	}

	for ( dat = m_pPeer; dat && dat != this; dat = datNext )
	{
		datNext = dat->m_pPeer;
		dat->m_pPeer = NULL;
		delete dat;
	}

	delete [] m_sValue;
	m_sValue = NULL;
	delete [] m_wsValue;
	m_wsValue = NULL;
}

//-----------------------------------------------------------------------------
// Purpose: 
// Input  : *f - 
//-----------------------------------------------------------------------------

void KeyValues::RecursiveSaveToFile( CUtlBuffer& buf, int indentLevel )
{
	RecursiveSaveToFile( NULL, FILESYSTEM_INVALID_HANDLE, &buf, indentLevel );
}

//-----------------------------------------------------------------------------
// Adds a chain... if we don't find stuff in this keyvalue, we'll look
// in the one we're chained to.
//-----------------------------------------------------------------------------

void KeyValues::ChainKeyValue( KeyValues* pChain )
{
	m_pChain = pChain;
}

//-----------------------------------------------------------------------------
// Purpose: Get the name of the current key section
//-----------------------------------------------------------------------------
const char *KeyValues::GetName( void ) const
{
	return KeyValuesSystem()->GetStringForSymbol(m_iKeyName);
}

//-----------------------------------------------------------------------------
// Purpose: Get the symbol name of the current key section
//-----------------------------------------------------------------------------
int KeyValues::GetNameSymbol() const
{
	return m_iKeyName;
}


//-----------------------------------------------------------------------------
// Purpose: Read a single token from buffer (0 terminated)
//-----------------------------------------------------------------------------
#pragma warning (disable:4706)
const char *KeyValues::ReadToken( CUtlBuffer &buf, bool &wasQuoted, bool &wasConditional )
{
	wasQuoted = false;
	wasConditional = false;

	if ( !buf.IsValid() )
		return NULL; 

	// eating white spaces and remarks loop
	while ( true )
	{
		buf.EatWhiteSpace();
		if ( !buf.IsValid() )
			return NULL;	// file ends after reading whitespaces

		// stop if it's not a comment; a new token starts here
		if ( !buf.EatCPPComment() )
			break;
	}

	const char *c = (const char*)buf.PeekGet( sizeof(char), 0 );
	if ( !c )
		return NULL;

	// read quoted strings specially
	if ( *c == '\"' )
	{
		wasQuoted = true;
		buf.GetDelimitedString( m_bHasEscapeSequences ? GetCStringCharConversion() : GetNoEscCharConversion(), 
			s_pTokenBuf, KEYVALUES_TOKEN_SIZE );
		return s_pTokenBuf;
	}

	if ( *c == '{' || *c == '}' )
	{
		// it's a control char, just add this one char and stop reading
		s_pTokenBuf[0] = *c;
		s_pTokenBuf[1] = 0;
		buf.SeekGet( CUtlBuffer::SEEK_CURRENT, 1 );
		return s_pTokenBuf;
	}

	// read in the token until we hit a whitespace or a control character
	bool bReportedError = false;
	bool bConditionalStart = false;
	int nCount = 0;
	while ( c = (const char*)buf.PeekGet( sizeof(char), 0 ) )
	{
		// end of file
		if ( *c == 0 )
			break;

		// break if any control character appears in non quoted tokens
		if ( *c == '"' || *c == '{' || *c == '}' )
			break;

		if ( *c == '[' )
			bConditionalStart = true;

		if ( *c == ']' && bConditionalStart )
		{
			wasConditional = true;
		}

		// break on whitespace
		if ( isspace(*c) )
			break;

		if (nCount < (KEYVALUES_TOKEN_SIZE-1) )
		{
			s_pTokenBuf[nCount++] = *c;	// add char to buffer
		}
		else if ( !bReportedError )
		{
			bReportedError = true;
			g_KeyValuesErrorStack.ReportError(" ReadToken overflow" );
		}

		buf.SeekGet( CUtlBuffer::SEEK_CURRENT, 1 );
	}
	s_pTokenBuf[ nCount ] = 0;
	return s_pTokenBuf;
}
#pragma warning (default:4706)

	

//-----------------------------------------------------------------------------
// Purpose: if parser should translate escape sequences ( /n, /t etc), set to true
//-----------------------------------------------------------------------------
void KeyValues::UsesEscapeSequences(bool state)
{
	m_bHasEscapeSequences = state;
}


//-----------------------------------------------------------------------------
// Purpose: Load keyValues from disk
//-----------------------------------------------------------------------------
bool KeyValues::LoadFromFile( IBaseFileSystem *filesystem, const char *resourceName, const char *pathID )
{
	Assert(filesystem);
	Assert( IsX360() || ( IsPC() && _heapchk() == _HEAPOK ) );

	FileHandle_t f = filesystem->Open(resourceName, "rb", pathID);
	if ( !f )
		return false;

	s_LastFileLoadingFrom = (char*)resourceName;

	// load file into a null-terminated buffer
	int fileSize = filesystem->Size( f );
	unsigned bufSize = ((IFileSystem *)filesystem)->GetOptimalReadSize( f, fileSize + 1 );

	char *buffer = (char*)((IFileSystem *)filesystem)->AllocOptimalReadBuffer( f, bufSize );
	Assert( buffer );
	
	// read into local buffer
	bool bRetOK = ( ((IFileSystem *)filesystem)->ReadEx( buffer, bufSize, fileSize, f ) != 0 );

	filesystem->Close( f );	// close file after reading

	if ( bRetOK )
	{
		buffer[fileSize] = 0; // null terminate file as EOF
		bRetOK = LoadFromBuffer( resourceName, buffer, filesystem );
	}

	((IFileSystem *)filesystem)->FreeOptimalReadBuffer( buffer );

	return bRetOK;
}

//-----------------------------------------------------------------------------
// Purpose: Save the keyvalues to disk
//			Creates the path to the file if it doesn't exist 
//-----------------------------------------------------------------------------
bool KeyValues::SaveToFile( IBaseFileSystem *filesystem, const char *resourceName, const char *pathID )
{
	// create a write file
	FileHandle_t f = filesystem->Open(resourceName, "wb", pathID);

	if ( f == FILESYSTEM_INVALID_HANDLE )
	{
		DevMsg(1, "KeyValues::SaveToFile: couldn't open file \"%s\" in path \"%s\".\n", 
			resourceName?resourceName:"NULL", pathID?pathID:"NULL" );
		return false;
	}

	RecursiveSaveToFile(filesystem, f, NULL, 0);
	filesystem->Close(f);

	return true;
}

//-----------------------------------------------------------------------------
// Purpose: Write out a set of indenting
//-----------------------------------------------------------------------------
void KeyValues::WriteIndents( IBaseFileSystem *filesystem, FileHandle_t f, CUtlBuffer *pBuf, int indentLevel )
{
	for ( int i = 0; i < indentLevel; i++ )
	{
		INTERNALWRITE( "\t", 1 );
	}
}

//-----------------------------------------------------------------------------
// Purpose: Write out a string where we convert the double quotes to backslash double quote
//-----------------------------------------------------------------------------
void KeyValues::WriteConvertedString( IBaseFileSystem *filesystem, FileHandle_t f, CUtlBuffer *pBuf, const char *pszString )
{
	// handle double quote chars within the string
	// the worst possible case is that the whole string is quotes
	int len = Q_strlen(pszString);
	char *convertedString = (char *) _alloca ((len + 1)  * sizeof(char) * 2);
	int j=0;
	for (int i=0; i <= len; i++)
	{
		if (pszString[i] == '\"')
		{
			convertedString[j] = '\\';
			j++;
		}
		else if ( m_bHasEscapeSequences && pszString[i] == '\\' )
		{
			convertedString[j] = '\\';
			j++;
		}
		convertedString[j] = pszString[i];
		j++;
	}		

	INTERNALWRITE(convertedString, strlen(convertedString));
}


void KeyValues::InternalWrite( IBaseFileSystem *filesystem, FileHandle_t f, CUtlBuffer *pBuf, const void *pData, int len )
{
	if ( filesystem )
	{
		filesystem->Write( pData, len, f );
	}

	if ( pBuf )
	{
		pBuf->Put( pData, len );
	}
} 


//-----------------------------------------------------------------------------
// Purpose: Save keyvalues from disk, if subkey values are detected, calls
//			itself to save those
//-----------------------------------------------------------------------------
void KeyValues::RecursiveSaveToFile( IBaseFileSystem *filesystem, FileHandle_t f, CUtlBuffer *pBuf, int indentLevel )
{
	// write header
	WriteIndents( filesystem, f, pBuf, indentLevel );
	INTERNALWRITE("\"", 1);
	WriteConvertedString(filesystem, f, pBuf, GetName());	
	INTERNALWRITE("\"\n", 2);
	WriteIndents( filesystem, f, pBuf, indentLevel );
	INTERNALWRITE("{\n", 2);

	// loop through all our keys writing them to disk
	for ( KeyValues *dat = m_pSub; dat != NULL; dat = dat->m_pPeer )
	{
		if ( dat->m_pSub )
		{
			dat->RecursiveSaveToFile( filesystem, f, pBuf, indentLevel + 1 );
		}
		else
		{
			// only write non-empty keys

			switch (dat->m_iDataType)
			{
			case TYPE_STRING:
				{
					if (dat->m_sValue && *(dat->m_sValue))
					{
						WriteIndents(filesystem, f, pBuf, indentLevel + 1);
						INTERNALWRITE("\"", 1);
						WriteConvertedString(filesystem, f, pBuf, dat->GetName());	
						INTERNALWRITE("\"\t\t\"", 4);

						WriteConvertedString(filesystem, f, pBuf, dat->m_sValue);	

						INTERNALWRITE("\"\n", 2);
					}
					break;
				}
			case TYPE_WSTRING:
				{
#ifdef _WIN32
					if ( dat->m_wsValue )
					{
						static char buf[KEYVALUES_TOKEN_SIZE];
						// make sure we have enough space
						Assert(::WideCharToMultiByte(CP_UTF8, 0, dat->m_wsValue, -1, NULL, 0, NULL, NULL) < KEYVALUES_TOKEN_SIZE);
						int result = ::WideCharToMultiByte(CP_UTF8, 0, dat->m_wsValue, -1, buf, KEYVALUES_TOKEN_SIZE, NULL, NULL);
						if (result)
						{
							WriteIndents(filesystem, f, pBuf, indentLevel + 1);
							INTERNALWRITE("\"", 1);
							INTERNALWRITE(dat->GetName(), Q_strlen(dat->GetName()));
							INTERNALWRITE("\"\t\t\"", 4);

							WriteConvertedString(filesystem, f, pBuf, buf);

							INTERNALWRITE("\"\n", 2);
						}
					}
#endif
					break;
				}

			case TYPE_INT:
				{
					WriteIndents(filesystem, f, pBuf, indentLevel + 1);
					INTERNALWRITE("\"", 1);
					INTERNALWRITE(dat->GetName(), Q_strlen(dat->GetName()));
					INTERNALWRITE("\"\t\t\"", 4);

					char buf[32];
					Q_snprintf(buf, sizeof( buf ), "%d", dat->m_iValue);

					INTERNALWRITE(buf, Q_strlen(buf));
					INTERNALWRITE("\"\n", 2);
					break;
				}

			case TYPE_UINT64:
				{
					WriteIndents(filesystem, f, pBuf, indentLevel + 1);
					INTERNALWRITE("\"", 1);
					INTERNALWRITE(dat->GetName(), Q_strlen(dat->GetName()));
					INTERNALWRITE("\"\t\t\"", 4);

					char buf[32];
					// write "0x" + 16 char 0-padded hex encoded 64 bit value
					Q_snprintf( buf, sizeof( buf ), "0x%016I64X", *( (uint64 *)dat->m_sValue ) );

					INTERNALWRITE(buf, Q_strlen(buf));
					INTERNALWRITE("\"\n", 2);
					break;
				}

			case TYPE_FLOAT:
				{
					WriteIndents(filesystem, f, pBuf, indentLevel + 1);
					INTERNALWRITE("\"", 1);
					INTERNALWRITE(dat->GetName(), Q_strlen(dat->GetName()));
					INTERNALWRITE("\"\t\t\"", 4);

					char buf[48];
					Q_snprintf(buf, sizeof( buf ), "%f", dat->m_flValue);

					INTERNALWRITE(buf, Q_strlen(buf));
					INTERNALWRITE("\"\n", 2);
					break;
				}
			case TYPE_COLOR:
				DevMsg(1, "KeyValues::RecursiveSaveToFile: TODO, missing code for TYPE_COLOR.\n");
				break;

			default:
				break;
			}
		}
	}

	// write tail
	WriteIndents(filesystem, f, pBuf, indentLevel);
	INTERNALWRITE("}\n", 2);
}

//-----------------------------------------------------------------------------
// Purpose: looks up a key by symbol name
//-----------------------------------------------------------------------------
KeyValues *KeyValues::FindKey(int keySymbol) const
{
	for (KeyValues *dat = m_pSub; dat != NULL; dat = dat->m_pPeer)
	{
		if (dat->m_iKeyName == keySymbol)
			return dat;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
// Purpose: Find a keyValue, create it if it is not found.
//			Set bCreate to true to create the key if it doesn't already exist 
//			(which ensures a valid pointer will be returned)
//-----------------------------------------------------------------------------
KeyValues *KeyValues::FindKey(const char *keyName, bool bCreate)
{
	// return the current key if a NULL subkey is asked for
	if (!keyName || !keyName[0])
		return this;

	// look for '/' characters deliminating sub fields
	char szBuf[256];
	const char *subStr = strchr(keyName, '/');
	const char *searchStr = keyName;

	// pull out the substring if it exists
	if (subStr)
	{
		int size = subStr - keyName;
		Q_memcpy( szBuf, keyName, size );
		szBuf[size] = 0;
		searchStr = szBuf;
	}

	// lookup the symbol for the search string
	HKeySymbol iSearchStr = KeyValuesSystem()->GetSymbolForString( searchStr, bCreate );
	if ( iSearchStr == INVALID_KEY_SYMBOL )
	{
		// not found, couldn't possibly be in key value list
		return NULL;
	}

	KeyValues *lastItem = NULL;
	KeyValues *dat;
	// find the searchStr in the current peer list
	for (dat = m_pSub; dat != NULL; dat = dat->m_pPeer)
	{
		lastItem = dat;	// record the last item looked at (for if we need to append to the end of the list)

		// symbol compare
		if (dat->m_iKeyName == iSearchStr)
		{
			break;
		}
	}

	if ( !dat && m_pChain )
	{
		dat = m_pChain->FindKey(keyName, false);
	}

	// make sure a key was found
	if (!dat)
	{
		if (bCreate)
		{
			// we need to create a new key
			dat = new KeyValues( searchStr );
//			Assert(dat != NULL);

			// insert new key at end of list
			if (lastItem)
			{
				lastItem->m_pPeer = dat;
			}
			else
			{
				m_pSub = dat;
			}
			dat->m_pPeer = NULL;

			// a key graduates to be a submsg as soon as it's m_pSub is set
			// this should be the only place m_pSub is set
			m_iDataType = TYPE_NONE;
		}
		else
		{
			return NULL;
		}
	}
	
	// if we've still got a subStr we need to keep looking deeper in the tree
	if ( subStr )
	{
		// recursively chain down through the paths in the string
		return dat->FindKey(subStr + 1, bCreate);
	}

	return dat;
}

//-----------------------------------------------------------------------------
// Purpose: Create a new key, with an autogenerated name.  
//			Name is guaranteed to be an integer, of value 1 higher than the highest 
//			other integer key name
//-----------------------------------------------------------------------------
KeyValues *KeyValues::CreateNewKey()
{
	int newID = 1;

	// search for any key with higher values
	for (KeyValues *dat = m_pSub; dat != NULL; dat = dat->m_pPeer)
	{
		// case-insensitive string compare
		int val = atoi(dat->GetName());
		if (newID <= val)
		{
			newID = val + 1;
		}
	}

	char buf[12];
	Q_snprintf( buf, sizeof(buf), "%d", newID );

	return CreateKey( buf );
}


//-----------------------------------------------------------------------------
// Create a key
//-----------------------------------------------------------------------------
KeyValues* KeyValues::CreateKey( const char *keyName )
{
	// key wasn't found so just create a new one
	KeyValues* dat = new KeyValues( keyName );

	dat->UsesEscapeSequences( m_bHasEscapeSequences != 0 ); // use same format as parent does
	
	// add into subkey list
	AddSubKey( dat );

	return dat;
}


//-----------------------------------------------------------------------------
// Adds a subkey. Make sure the subkey isn't a child of some other keyvalues
//-----------------------------------------------------------------------------
void KeyValues::AddSubKey( KeyValues *pSubkey )
{
	// Make sure the subkey isn't a child of some other keyvalues
	Assert( pSubkey->m_pPeer == NULL );

	// add into subkey list
	if ( m_pSub == NULL )
	{
		m_pSub = pSubkey;
	}
	else
	{
		KeyValues *pTempDat = m_pSub;
		while ( pTempDat->GetNextKey() != NULL )
		{
			pTempDat = pTempDat->GetNextKey();
		}

		pTempDat->SetNextKey( pSubkey );
	}
}


	
//-----------------------------------------------------------------------------
// Purpose: Remove a subkey from the list
//-----------------------------------------------------------------------------
void KeyValues::RemoveSubKey(KeyValues *subKey)
{
	if (!subKey)
		return;

	// check the list pointer
	if (m_pSub == subKey)
	{
		m_pSub = subKey->m_pPeer;
	}
	else
	{
		// look through the list
		KeyValues *kv = m_pSub;
		while (kv->m_pPeer)
		{
			if (kv->m_pPeer == subKey)
			{
				kv->m_pPeer = subKey->m_pPeer;
				break;
			}
			
			kv = kv->m_pPeer;
		}
	}

	subKey->m_pPeer = NULL;
}



//-----------------------------------------------------------------------------
// Purpose: Return the first subkey in the list
//-----------------------------------------------------------------------------
KeyValues *KeyValues::GetFirstSubKey()
{
	return m_pSub;
}

//-----------------------------------------------------------------------------
// Purpose: Return the next subkey
//-----------------------------------------------------------------------------
KeyValues *KeyValues::GetNextKey()
{
	return m_pPeer;
}

//-----------------------------------------------------------------------------
// Purpose: Sets this key's peer to the KeyValues passed in
//-----------------------------------------------------------------------------
void KeyValues::SetNextKey( KeyValues *pDat )
{
	m_pPeer = pDat;
}


KeyValues* KeyValues::GetFirstTrueSubKey()
{
	KeyValues *pRet = m_pSub;
	while ( pRet && pRet->m_iDataType != TYPE_NONE )
		pRet = pRet->m_pPeer;

	return pRet;
}

KeyValues* KeyValues::GetNextTrueSubKey()
{
	KeyValues *pRet = m_pPeer;
	while ( pRet && pRet->m_iDataType != TYPE_NONE )
		pRet = pRet->m_pPeer;

	return pRet;
}

KeyValues* KeyValues::GetFirstValue()
{
	KeyValues *pRet = m_pSub;
	while ( pRet && pRet->m_iDataType == TYPE_NONE )
		pRet = pRet->m_pPeer;

	return pRet;
}

KeyValues* KeyValues::GetNextValue()
{
	KeyValues *pRet = m_pPeer;
	while ( pRet && pRet->m_iDataType == TYPE_NONE )
		pRet = pRet->m_pPeer;

	return pRet;
}


//-----------------------------------------------------------------------------
// Purpose: Get the integer value of a keyName. Default value is returned
//			if the keyName can't be found.
//-----------------------------------------------------------------------------
int KeyValues::GetInt( const char *keyName, int defaultValue )
{
	KeyValues *dat = FindKey( keyName, false );
	if ( dat )
	{
		switch ( dat->m_iDataType )
		{
		case TYPE_STRING:
			return atoi(dat->m_sValue);
		case TYPE_WSTRING:
#ifdef _WIN32
			return _wtoi(dat->m_wsValue);
#else
			DevMsg( "TODO: implement _wtoi\n");
			return 0;
#endif
		case TYPE_FLOAT:
			return (int)dat->m_flValue;
		case TYPE_UINT64:
			// can't convert, since it would lose data
			Assert(0);
			return 0;
		case TYPE_INT:
		case TYPE_PTR:
		default:
			return dat->m_iValue;
		};
	}
	return defaultValue;
}

//-----------------------------------------------------------------------------
// Purpose: Get the integer value of a keyName. Default value is returned
//			if the keyName can't be found.
//-----------------------------------------------------------------------------
uint64 KeyValues::GetUint64( const char *keyName, uint64 defaultValue )
{
	KeyValues *dat = FindKey( keyName, false );
	if ( dat )
	{
		switch ( dat->m_iDataType )
		{
		case TYPE_STRING:
			return atoi(dat->m_sValue);
		case TYPE_WSTRING:
#ifdef _WIN32
			return _wtoi(dat->m_wsValue);
#else
			AssertFatal( 0 );
			return 0;
#endif
		case TYPE_FLOAT:
			return (int)dat->m_flValue;
		case TYPE_UINT64:
			return *((uint64 *)dat->m_sValue);
		case TYPE_INT:
		case TYPE_PTR:
		default:
			return dat->m_iValue;
		};
	}
	return defaultValue;
}

//-----------------------------------------------------------------------------
// Purpose: Get the pointer value of a keyName. Default value is returned
//			if the keyName can't be found.
//-----------------------------------------------------------------------------
void *KeyValues::GetPtr( const char *keyName, void *defaultValue )
{
	KeyValues *dat = FindKey( keyName, false );
	if ( dat )
	{
		switch ( dat->m_iDataType )
		{
		case TYPE_PTR:
			return dat->m_pValue;

		case TYPE_WSTRING:
		case TYPE_STRING:
		case TYPE_FLOAT:
		case TYPE_INT:
		case TYPE_UINT64:
		default:
			return NULL;
		};
	}
	return defaultValue;
}

//-----------------------------------------------------------------------------
// Purpose: Get the float value of a keyName. Default value is returned
//			if the keyName can't be found.
//-----------------------------------------------------------------------------
float KeyValues::GetFloat( const char *keyName, float defaultValue )
{
	KeyValues *dat = FindKey( keyName, false );
	if ( dat )
	{
		switch ( dat->m_iDataType )
		{
		case TYPE_STRING:
			return (float)atof(dat->m_sValue);
		case TYPE_WSTRING:
#ifdef _WIN32
			return (float) _wtof(dat->m_wsValue);		// no wtof
#else
			Assert(0);
			return 0.;
#endif
			case TYPE_FLOAT:
			return dat->m_flValue;
		case TYPE_INT:
			return (float)dat->m_iValue;
		case TYPE_UINT64:
			return (float)(*((uint64 *)dat->m_sValue));
		case TYPE_PTR:
		default:
			return 0.0f;
		};
	}
	return defaultValue;
}

//-----------------------------------------------------------------------------
// Purpose: Get the string pointer of a keyName. Default value is returned
//			if the keyName can't be found.
//-----------------------------------------------------------------------------
const char *KeyValues::GetString( const char *keyName, const char *defaultValue )
{
	KeyValues *dat = FindKey( keyName, false );
	if ( dat )
	{
		// convert the data to string form then return it
		char buf[64];
		switch ( dat->m_iDataType )
		{
		case TYPE_FLOAT:
			Q_snprintf( buf, sizeof( buf ), "%f", dat->m_flValue );
			SetString( keyName, buf );
			break;
		case TYPE_INT:
		case TYPE_PTR:
			Q_snprintf( buf, sizeof( buf ), "%d", dat->m_iValue );
			SetString( keyName, buf );
			break;
		case TYPE_UINT64:
			Q_snprintf( buf, sizeof( buf ), "%I64i", *((uint64 *)(dat->m_sValue)) );
			SetString( keyName, buf );
			break;

		case TYPE_WSTRING:
		{
#ifdef _WIN32
			// convert the string to char *, set it for future use, and return it
			char wideBuf[512];
			int result = ::WideCharToMultiByte(CP_UTF8, 0, dat->m_wsValue, -1, wideBuf, 512, NULL, NULL);
			if ( result )
			{
				// note: this will copy wideBuf
				SetString( keyName, wideBuf );
			}
			else
			{
				return defaultValue;
			}
#endif
			break;
		}
		case TYPE_STRING:
			break;
		default:
			return defaultValue;
		};
		
		return dat->m_sValue;
	}
	return defaultValue;
}

const wchar_t *KeyValues::GetWString( const char *keyName, const wchar_t *defaultValue)
{
	KeyValues *dat = FindKey( keyName, false );
#ifdef _WIN32
	if ( dat )
	{
		wchar_t wbuf[64];
		switch ( dat->m_iDataType )
		{
		case TYPE_FLOAT:
			swprintf(wbuf, L"%f", dat->m_flValue);
			SetWString( keyName, wbuf);
			break;
		case TYPE_INT:
		case TYPE_PTR:
			swprintf( wbuf, L"%d", dat->m_iValue );
			SetWString( keyName, wbuf );
			break;
		case TYPE_UINT64:
			{
				swprintf( wbuf, L"%I64i", *((uint64 *)(dat->m_sValue)) );
				SetWString( keyName, wbuf );
			}
			break;

		case TYPE_WSTRING:
			break;
		case TYPE_STRING:
		{
			static wchar_t wbuftemp[512]; // convert to wide	
			int result = ::MultiByteToWideChar(CP_UTF8, 0, dat->m_sValue, -1, wbuftemp, 512);
			if ( result )
			{
				SetWString( keyName, wbuftemp);
			}
			else
			{
				return defaultValue;
			}
			break;
		}
		default:
			return defaultValue;
		};
		
		return (const wchar_t* )dat->m_wsValue;
	}
#else
	DevMsg("TODO: implement wide char functions\n");
#endif
	return defaultValue;
}

//-----------------------------------------------------------------------------
// Purpose: Gets a color
//-----------------------------------------------------------------------------
Color KeyValues::GetColor( const char *keyName )
{
	Color color(0, 0, 0, 0);
	KeyValues *dat = FindKey( keyName, false );
	if ( dat )
	{
		if ( dat->m_iDataType == TYPE_COLOR )
		{
			color[0] = dat->m_Color[0];
			color[1] = dat->m_Color[1];
			color[2] = dat->m_Color[2];
			color[3] = dat->m_Color[3];
		}
		else if ( dat->m_iDataType == TYPE_FLOAT )
		{
			color[0] = dat->m_flValue;
		}
		else if ( dat->m_iDataType == TYPE_INT )
		{
			color[0] = dat->m_iValue;
		}
		else if ( dat->m_iDataType == TYPE_STRING )
		{
			// parse the colors out of the string
			float a, b, c, d;
			sscanf(dat->m_sValue, "%f %f %f %f", &a, &b, &c, &d);
			color[0] = (unsigned char)a;
			color[1] = (unsigned char)b;
			color[2] = (unsigned char)c;
			color[3] = (unsigned char)d;
		}
	}
	return color;
}

//-----------------------------------------------------------------------------
// Purpose: Sets a color
//-----------------------------------------------------------------------------
void KeyValues::SetColor( const char *keyName, Color value)
{
	KeyValues *dat = FindKey( keyName, true );

	if ( dat )
	{
		dat->m_iDataType = TYPE_COLOR;
		dat->m_Color[0] = value[0];
		dat->m_Color[1] = value[1];
		dat->m_Color[2] = value[2];
		dat->m_Color[3] = value[3];
	}
}

void KeyValues::SetStringValue( char const *strValue )
{
	// delete the old value
	delete [] m_sValue;
	// make sure we're not storing the WSTRING  - as we're converting over to STRING
	delete [] m_wsValue;
	m_wsValue = NULL;

	if (!strValue)
	{
		// ensure a valid value
		strValue = "";
	}

	// allocate memory for the new value and copy it in
	int len = Q_strlen( strValue );
	m_sValue = new char[len + 1];
	Q_memcpy( m_sValue, strValue, len+1 );

	m_iDataType = TYPE_STRING;
}

//-----------------------------------------------------------------------------
// Purpose: Set the string value of a keyName. 
//-----------------------------------------------------------------------------
void KeyValues::SetString( const char *keyName, const char *value )
{
	KeyValues *dat = FindKey( keyName, true );

	if ( dat )
	{
		// delete the old value
		delete [] dat->m_sValue;
		// make sure we're not storing the WSTRING  - as we're converting over to STRING
		delete [] dat->m_wsValue;
		dat->m_wsValue = NULL;

		if (!value)
		{
			// ensure a valid value
			value = "";
		}

		// allocate memory for the new value and copy it in
		int len = Q_strlen( value );
		dat->m_sValue = new char[len + 1];
		Q_memcpy( dat->m_sValue, value, len+1 );

		dat->m_iDataType = TYPE_STRING;
	}
}

//-----------------------------------------------------------------------------
// Purpose: Set the string value of a keyName. 
//-----------------------------------------------------------------------------
void KeyValues::SetWString( const char *keyName, const wchar_t *value )
{
	KeyValues *dat = FindKey( keyName, true );
	if ( dat )
	{
		// delete the old value
		delete [] dat->m_wsValue;
		// make sure we're not storing the STRING  - as we're converting over to WSTRING
		delete [] dat->m_sValue;
		dat->m_sValue = NULL;

		if (!value)
		{
			// ensure a valid value
			value = L"";
		}

		// allocate memory for the new value and copy it in
		int len = wcslen( value );
		dat->m_wsValue = new wchar_t[len + 1];
		Q_memcpy( dat->m_wsValue, value, (len+1) * sizeof(wchar_t) );

		dat->m_iDataType = TYPE_WSTRING;
	}
}

//-----------------------------------------------------------------------------
// Purpose: Set the integer value of a keyName. 
//-----------------------------------------------------------------------------
void KeyValues::SetInt( const char *keyName, int value )
{
	KeyValues *dat = FindKey( keyName, true );

	if ( dat )
	{
		dat->m_iValue = value;
		dat->m_iDataType = TYPE_INT;
	}
}

//-----------------------------------------------------------------------------
// Purpose: Set the integer value of a keyName. 
//-----------------------------------------------------------------------------
void KeyValues::SetUint64( const char *keyName, uint64 value )
{
	KeyValues *dat = FindKey( keyName, true );

	if ( dat )
	{
		// delete the old value
		delete [] dat->m_sValue;
		// make sure we're not storing the WSTRING  - as we're converting over to STRING
		delete [] dat->m_wsValue;
		dat->m_wsValue = NULL;

		dat->m_sValue = new char[sizeof(uint64)];
		*((uint64 *)dat->m_sValue) = value;
		dat->m_iDataType = TYPE_UINT64;
	}
}

//-----------------------------------------------------------------------------
// Purpose: Set the float value of a keyName. 
//-----------------------------------------------------------------------------
void KeyValues::SetFloat( const char *keyName, float value )
{
	KeyValues *dat = FindKey( keyName, true );

	if ( dat )
	{
		dat->m_flValue = value;
		dat->m_iDataType = TYPE_FLOAT;
	}
}

void KeyValues::SetName( const char * setName )
{
	m_iKeyName = KeyValuesSystem()->GetSymbolForString( setName );
}

//-----------------------------------------------------------------------------
// Purpose: Set the pointer value of a keyName. 
//-----------------------------------------------------------------------------
void KeyValues::SetPtr( const char *keyName, void *value )
{
	KeyValues *dat = FindKey( keyName, true );

	if ( dat )
	{
		dat->m_pValue = value;
		dat->m_iDataType = TYPE_PTR;
	}
}

void KeyValues::RecursiveCopyKeyValues( KeyValues& src )
{
	// garymcthack - need to check this code for possible buffer overruns.
	
	m_iKeyName = src.GetNameSymbol();

	if( !src.m_pSub )
	{
		m_iDataType = src.m_iDataType;
		char buf[256];
		switch( src.m_iDataType )
		{
		case TYPE_NONE:
			break;
		case TYPE_STRING:
			if( src.m_sValue )
			{
				int len = Q_strlen(src.m_sValue) + 1;
				m_sValue = new char[len];
				Q_strncpy( m_sValue, src.m_sValue, len );
			}
			break;
		case TYPE_INT:
			{
				m_iValue = src.m_iValue;
				Q_snprintf( buf,sizeof(buf), "%d", m_iValue );
				int len = Q_strlen(buf) + 1;
				m_sValue = new char[len];
				Q_strncpy( m_sValue, buf, len  );
			}
			break;
		case TYPE_FLOAT:
			{
				m_flValue = src.m_flValue;
				Q_snprintf( buf,sizeof(buf), "%f", m_flValue );
				int len = Q_strlen(buf) + 1;
				m_sValue = new char[len];
				Q_strncpy( m_sValue, buf, len );
			}
			break;
		case TYPE_PTR:
			{
				m_pValue = src.m_pValue;
			}
			break;
		case TYPE_UINT64:
			{
				m_sValue = new char[sizeof(uint64)];
				Q_memcpy( m_sValue, src.m_sValue, sizeof(uint64) );
			}
			break;
		case TYPE_COLOR:
			{
				m_Color[0] = src.m_Color[0];
				m_Color[1] = src.m_Color[1];
				m_Color[2] = src.m_Color[2];
				m_Color[3] = src.m_Color[3];
			}
			break;
			
		default:
			{
				// do nothing . .what the heck is this?
				Assert( 0 );
			}
			break;
		}

	}
#if 0
	KeyValues *pDst = this;
	for ( KeyValues *pSrc = src.m_pSub; pSrc; pSrc = pSrc->m_pPeer )
	{
		if ( pSrc->m_pSub )
		{
			pDst->m_pSub = new KeyValues( pSrc->m_pSub->getName() );
			pDst->m_pSub->RecursiveCopyKeyValues( *pSrc->m_pSub );
		}
		else
		{
			// copy non-empty keys
			if ( pSrc->m_sValue && *(pSrc->m_sValue) )
			{
				pDst->m_pPeer = new KeyValues( 
			}
		}
	}
#endif

	// Handle the immediate child
	if( src.m_pSub )
	{
		m_pSub = new KeyValues( NULL );
		m_pSub->RecursiveCopyKeyValues( *src.m_pSub );
	}

	// Handle the immediate peer
	if( src.m_pPeer )
	{
		m_pPeer = new KeyValues( NULL );
		m_pPeer->RecursiveCopyKeyValues( *src.m_pPeer );
	}
}

KeyValues& KeyValues::operator=( KeyValues& src )
{
	RemoveEverything();
	Init();	// reset all values
	RecursiveCopyKeyValues( src );
	return *this;
}


//-----------------------------------------------------------------------------
// Make a new copy of all subkeys, add them all to the passed-in keyvalues
//-----------------------------------------------------------------------------
void KeyValues::CopySubkeys( KeyValues *pParent ) const
{
	// recursively copy subkeys
	// Also maintain ordering....
	KeyValues *pPrev = NULL;
	for ( KeyValues *sub = m_pSub; sub != NULL; sub = sub->m_pPeer )
	{
		// take a copy of the subkey
		KeyValues *dat = sub->MakeCopy();
		 
		// add into subkey list
		if (pPrev)
		{
			pPrev->m_pPeer = dat;
		}
		else
		{
			pParent->m_pSub = dat;
		}
		dat->m_pPeer = NULL;
		pPrev = dat;
	}
}


//-----------------------------------------------------------------------------
// Purpose: Makes a copy of the whole key-value pair set
//-----------------------------------------------------------------------------
KeyValues *KeyValues::MakeCopy( void ) const
{
	KeyValues *newKeyValue = new KeyValues(GetName());

	// copy data
	newKeyValue->m_iDataType = m_iDataType;
	switch ( m_iDataType )
	{
	case TYPE_STRING:
		{
			if ( m_sValue )
			{
				int len = Q_strlen( m_sValue );
				Assert( !newKeyValue->m_sValue );
				newKeyValue->m_sValue = new char[len + 1];
				Q_memcpy( newKeyValue->m_sValue, m_sValue, len+1 );
			}
		}
		break;
	case TYPE_WSTRING:
		{
			if ( m_wsValue )
			{
				int len = wcslen( m_wsValue );
				newKeyValue->m_wsValue = new wchar_t[len+1];
				Q_memcpy( newKeyValue->m_wsValue, m_wsValue, (len+1)*sizeof(wchar_t));
			}
		}
		break;

	case TYPE_INT:
		newKeyValue->m_iValue = m_iValue;
		break;

	case TYPE_FLOAT:
		newKeyValue->m_flValue = m_flValue;
		break;

	case TYPE_PTR:
		newKeyValue->m_pValue = m_pValue;
		break;
		
	case TYPE_COLOR:
		newKeyValue->m_Color[0] = m_Color[0];
		newKeyValue->m_Color[1] = m_Color[1];
		newKeyValue->m_Color[2] = m_Color[2];
		newKeyValue->m_Color[3] = m_Color[3];
		break;

	case TYPE_UINT64:
		newKeyValue->m_sValue = new char[sizeof(uint64)];
		Q_memcpy( newKeyValue->m_sValue, m_sValue, sizeof(uint64) );
		break;
	};

	// recursively copy subkeys
	CopySubkeys( newKeyValue );
	return newKeyValue;
}


//-----------------------------------------------------------------------------
// Purpose: Check if a keyName has no value assigned to it.
//-----------------------------------------------------------------------------
bool KeyValues::IsEmpty(const char *keyName)
{
	KeyValues *dat = FindKey(keyName, false);
	if (!dat)
		return true;

	if (dat->m_iDataType == TYPE_NONE && dat->m_pSub == NULL)
		return true;

	return false;
}

//-----------------------------------------------------------------------------
// Purpose: Clear out all subkeys, and the current value
//-----------------------------------------------------------------------------
void KeyValues::Clear( void )
{
	delete m_pSub;
	m_pSub = NULL;
	m_iDataType = TYPE_NONE;
}

//-----------------------------------------------------------------------------
// Purpose: Get the data type of the value stored in a keyName
//-----------------------------------------------------------------------------
KeyValues::types_t KeyValues::GetDataType(const char *keyName)
{
	KeyValues *dat = FindKey(keyName, false);
	if (dat)
		return (types_t)dat->m_iDataType;

	return TYPE_NONE;
}

//-----------------------------------------------------------------------------
// Purpose: Deletion, ensures object gets deleted from correct heap
//-----------------------------------------------------------------------------
void KeyValues::deleteThis()
{
	delete this;
}

//-----------------------------------------------------------------------------
// Purpose: 
// Input  : includedKeys - 
//-----------------------------------------------------------------------------
void KeyValues::AppendIncludedKeys( CUtlVector< KeyValues * >& includedKeys )
{
	// Append any included keys, too...
	int includeCount = includedKeys.Count();
	int i;
	for ( i = 0; i < includeCount; i++ )
	{
		KeyValues *kv = includedKeys[ i ];
		Assert( kv );

		KeyValues *insertSpot = this;
		while ( insertSpot->GetNextKey() )
		{
			insertSpot = insertSpot->GetNextKey();
		}

		insertSpot->SetNextKey( kv );
	}
}

void KeyValues::ParseIncludedKeys( char const *resourceName, const char *filetoinclude, 
		IBaseFileSystem* pFileSystem, const char *pPathID, CUtlVector< KeyValues * >& includedKeys )
{
	Assert( resourceName );
	Assert( filetoinclude );
	Assert( pFileSystem );
	
	// Load it...
	if ( !pFileSystem )
	{
		return;
	}

	// Get relative subdirectory
	char fullpath[ 512 ];
	Q_strncpy( fullpath, resourceName, sizeof( fullpath ) );

	// Strip off characters back to start or first /
	bool done = false;
	int len = Q_strlen( fullpath );
	while ( !done )
	{
		if ( len <= 0 )
		{
			break;
		}
		
		if ( fullpath[ len - 1 ] == '\\' || 
			 fullpath[ len - 1 ] == '/' )
		{
			break;
		}

		// zero it
		fullpath[ len - 1 ] = 0;
		--len;
	}

	// Append included file
	Q_strncat( fullpath, filetoinclude, sizeof( fullpath ), COPY_ALL_CHARACTERS );

	KeyValues *newKV = new KeyValues( fullpath );

	// CUtlSymbol save = s_CurrentFileSymbol;	// did that had any use ???

	newKV->UsesEscapeSequences( m_bHasEscapeSequences != 0 );	// use same format as parent

	if ( newKV->LoadFromFile( pFileSystem, fullpath, pPathID ) )
	{
		includedKeys.AddToTail( newKV );
	}
	else
	{
		DevMsg( "KeyValues::ParseIncludedKeys: Couldn't load included keyvalue file %s\n", fullpath );
		newKV->deleteThis();
	}

	// s_CurrentFileSymbol = save;
}

//-----------------------------------------------------------------------------
// Purpose: 
// Input  : baseKeys - 
//-----------------------------------------------------------------------------
void KeyValues::MergeBaseKeys( CUtlVector< KeyValues * >& baseKeys )
{
	int includeCount = baseKeys.Count();
	int i;
	for ( i = 0; i < includeCount; i++ )
	{
		KeyValues *kv = baseKeys[ i ];
		Assert( kv );

		RecursiveMergeKeyValues( kv );
	}
}

//-----------------------------------------------------------------------------
// Purpose: 
// Input  : baseKV - keyvalues we're basing ourselves on
//-----------------------------------------------------------------------------
void KeyValues::RecursiveMergeKeyValues( KeyValues *baseKV )
{
	// Merge ourselves
	// we always want to keep our value, so nothing to do here

	// Now merge our children
	for ( KeyValues *baseChild = baseKV->m_pSub; baseChild != NULL; baseChild = baseChild->m_pPeer )
	{
		// for each child in base, see if we have a matching kv

		bool bFoundMatch = false;

		// If we have a child by the same name, merge those keys
		for ( KeyValues *newChild = m_pSub; newChild != NULL; newChild = newChild->m_pPeer )
		{
			if ( !Q_strcmp( baseChild->GetName(), newChild->GetName() ) )
			{
				newChild->RecursiveMergeKeyValues( baseChild );
				bFoundMatch = true;
				break;
			}	
		}

		// If not merged, append this key
		if ( !bFoundMatch )
		{
			KeyValues *dat = baseChild->MakeCopy();
			Assert( dat );
			AddSubKey( dat );
		}
	}
}

//-----------------------------------------------------------------------------
// Returns whether a keyvalues conditional evaluates to true or false
// Needs more flexibility with conditionals, checking convars would be nice.
//-----------------------------------------------------------------------------
bool EvaluateConditional( const char *str )
{
	bool bResult = false;
	bool bXboxUI = IsX360();

	if ( bXboxUI )
	{
		bResult = !Q_stricmp( "[$X360]", str );
	}
	else
	{
		bResult = !Q_stricmp( "[$WIN32]", str );
	}

	return bResult;
}


//-----------------------------------------------------------------------------
// Read from a buffer...
//-----------------------------------------------------------------------------
bool KeyValues::LoadFromBuffer( char const *resourceName, CUtlBuffer &buf, IBaseFileSystem* pFileSystem, const char *pPathID )
{
	KeyValues *pPreviousKey = NULL;
	KeyValues *pCurrentKey = this;
	CUtlVector< KeyValues * > includedKeys;
	CUtlVector< KeyValues * > baseKeys;
	bool wasQuoted;
	bool wasConditional;
	g_KeyValuesErrorStack.SetFilename( resourceName );	
	do 
	{
		bool bAccepted = true;

		// the first thing must be a key
		const char *s = ReadToken( buf, wasQuoted, wasConditional );
		if ( !buf.IsValid() || !s || *s == 0 )
			break;

		if ( !Q_stricmp( s, "#include" ) )	// special include macro (not a key name)
		{
			s = ReadToken( buf, wasQuoted, wasConditional );
			// Name of subfile to load is now in s

			if ( !s || *s == 0 )
			{
				g_KeyValuesErrorStack.ReportError("#include is NULL " );
			}
			else
			{
				ParseIncludedKeys( resourceName, s, pFileSystem, pPathID, includedKeys );
			}

			continue;
		}
		else if ( !Q_stricmp( s, "#base" ) )
		{
			s = ReadToken( buf, wasQuoted, wasConditional );
			// Name of subfile to load is now in s

			if ( !s || *s == 0 )
			{
				g_KeyValuesErrorStack.ReportError("#base is NULL " );
			}
			else
			{
				ParseIncludedKeys( resourceName, s, pFileSystem, pPathID, baseKeys );
			}

			continue;
		}

		if ( !pCurrentKey )
		{
			pCurrentKey = new KeyValues( s );
			Assert( pCurrentKey );

			pCurrentKey->UsesEscapeSequences( m_bHasEscapeSequences != 0 ); // same format has parent use

			if ( pPreviousKey )
			{
				pPreviousKey->SetNextKey( pCurrentKey );
			}
		}
		else
		{
			pCurrentKey->SetName( s );
		}

		// get the '{'
		s = ReadToken( buf, wasQuoted, wasConditional );

		if ( wasConditional )
		{
			bAccepted = EvaluateConditional( s );

			// Now get the '{'
			s = ReadToken( buf, wasQuoted, wasConditional );
		}

		if ( s && *s == '{' && !wasQuoted )
		{
			// header is valid so load the file
			pCurrentKey->RecursiveLoadFromBuffer( resourceName, buf );
		}
		else
		{
			g_KeyValuesErrorStack.ReportError("LoadFromBuffer: missing {" );
		}

		if ( !bAccepted )
		{
			if ( pPreviousKey )
			{
				pPreviousKey->SetNextKey( NULL );
			}
			pCurrentKey->Clear();
		}
		else
		{
			pPreviousKey = pCurrentKey;
			pCurrentKey = NULL;
		}
	} while ( buf.IsValid() );

	AppendIncludedKeys( includedKeys );
	{
		// delete included keys!
		int i;
		for ( i = includedKeys.Count() - 1; i > 0; i-- )
		{
			KeyValues *kv = includedKeys[ i ];
			kv->deleteThis();
		}
	}

	MergeBaseKeys( baseKeys );
	{
		// delete base keys!
		int i;
		for ( i = baseKeys.Count() - 1; i >= 0; i-- )
		{
			KeyValues *kv = baseKeys[ i ];
			kv->deleteThis();
		}
	}

	g_KeyValuesErrorStack.SetFilename( "" );	

	return true;
}


//-----------------------------------------------------------------------------
// Read from a buffer...
//-----------------------------------------------------------------------------
bool KeyValues::LoadFromBuffer( char const *resourceName, const char *pBuffer, IBaseFileSystem* pFileSystem, const char *pPathID )
{
	if ( !pBuffer )
		return true;

	int nLen = Q_strlen( pBuffer );
	CUtlBuffer buf( pBuffer, nLen, CUtlBuffer::READ_ONLY | CUtlBuffer::TEXT_BUFFER );
	return LoadFromBuffer( resourceName, buf, pFileSystem, pPathID );
}

//-----------------------------------------------------------------------------
// Purpose: 
//-----------------------------------------------------------------------------
void KeyValues::RecursiveLoadFromBuffer( char const *resourceName, CUtlBuffer &buf )
{
	CKeyErrorContext errorReport(this);
	bool wasQuoted;
	bool wasConditional;
	// keep this out of the stack until a key is parsed
	CKeyErrorContext errorKey( INVALID_KEY_SYMBOL );
	while ( 1 )
	{
		bool bAccepted = true;

		// get the key name
		const char * name = ReadToken( buf, wasQuoted, wasConditional );

		if ( !name )	// EOF stop reading
		{
			g_KeyValuesErrorStack.ReportError("RecursiveLoadFromBuffer:  got EOF instead of keyname" );
			break;
		}

		if ( !*name ) // empty token, maybe "" or EOF
		{
			g_KeyValuesErrorStack.ReportError("RecursiveLoadFromBuffer:  got empty keyname" );
			break;
		}

		if ( *name == '}' && !wasQuoted )	// top level closed, stop reading
			break;

		// Always create the key; note that this could potentially
		// cause some duplication, but that's what we want sometimes
		KeyValues *dat = CreateKey( name );

		errorKey.Reset( dat->GetNameSymbol() );

		// get the value
		const char * value = ReadToken( buf, wasQuoted, wasConditional );

		if ( wasConditional && value )
		{
			bAccepted = EvaluateConditional( value );

			// get the real value
			value = ReadToken( buf, wasQuoted, wasConditional );
		}

		if ( !value )
		{
			g_KeyValuesErrorStack.ReportError("RecursiveLoadFromBuffer:  got NULL key" );
			break;
		}
		
		if ( *value == '}' && !wasQuoted )
		{
			g_KeyValuesErrorStack.ReportError("RecursiveLoadFromBuffer:  got } in key" );
			break;
		}

		if ( *value == '{' && !wasQuoted )
		{
			// this isn't a key, it's a section
			errorKey.Reset( INVALID_KEY_SYMBOL );
			// sub value list
			dat->RecursiveLoadFromBuffer( resourceName, buf );
		}
		else 
		{
			if ( wasConditional )
			{
				g_KeyValuesErrorStack.ReportError("RecursiveLoadFromBuffer:  got conditional between key and value" );
				break;
			}
			
			if (dat->m_sValue)
			{
				delete[] dat->m_sValue;
				dat->m_sValue = NULL;
			}

			int len = Q_strlen( value );

			// Here, let's determine if we got a float or an int....
			char* pIEnd;	// pos where int scan ended
			char* pFEnd;	// pos where float scan ended
			const char* pSEnd = value + len ; // pos where token ends

			int ival = strtol( value, &pIEnd, 10 );
			float fval = (float)strtod( value, &pFEnd );

			if ( *value == 0 )
			{
				dat->m_iDataType = TYPE_STRING;	
			}
			else if ( ( 18 == len ) && ( value[0] == '0' ) && ( value[1] == 'x' ) )
			{
				// an 18-byte value prefixed with "0x" (followed by 16 hex digits) is an int64 value
				int64 retVal = 0;
				for( int i=2; i < 2 + 16; i++ )
				{
					char digit = value[i];
					if ( digit >= 'a' ) 
						digit -= 'a' - ( '9' + 1 );
					else
						if ( digit >= 'A' )
							digit -= 'A' - ( '9' + 1 );
					retVal = ( retVal * 16 ) + ( digit - '0' );
				}
				dat->m_sValue = new char[sizeof(uint64)];
				*((uint64 *)dat->m_sValue) = retVal;
				dat->m_iDataType = TYPE_UINT64;
			}
			else if ( (pFEnd > pIEnd) && (pFEnd == pSEnd) )
			{
				dat->m_flValue = fval; 
				dat->m_iDataType = TYPE_FLOAT;
			}
			else if (pIEnd == pSEnd)
			{
				dat->m_iValue = ival; 
				dat->m_iDataType = TYPE_INT;
			}
			else
			{
				dat->m_iDataType = TYPE_STRING;
			}

			if (dat->m_iDataType == TYPE_STRING)
			{
				// copy in the string information
				dat->m_sValue = new char[len+1];
				Q_memcpy( dat->m_sValue, value, len+1 );
			}

			// Look ahead one token for a conditional tag
			int prevPos = buf.TellGet();
			const char *peek = ReadToken( buf, wasQuoted, wasConditional );
			if ( wasConditional )
			{
				bAccepted = EvaluateConditional( peek );
			}
			else
			{
				buf.SeekGet( CUtlBuffer::SEEK_HEAD, prevPos );
			}
		}

		if ( !bAccepted )
		{
			this->RemoveSubKey( dat );
			dat->deleteThis();
			dat = NULL;
		}
	}
}



// writes KeyValue as binary data to buffer
bool KeyValues::WriteAsBinary( CUtlBuffer &buffer )
{
	if ( buffer.IsText() ) // must be a binary buffer
		return false;

	if ( !buffer.IsValid() ) // must be valid, no overflows etc
		return false;

	// Write subkeys:
	
	// loop through all our peers
	for ( KeyValues *dat = this; dat != NULL; dat = dat->m_pPeer )
	{
		// write type
		buffer.PutUnsignedChar( dat->m_iDataType );

		// write name
		buffer.PutString( dat->GetName() );

		// write type
		switch (dat->m_iDataType)
		{
		case TYPE_NONE:
			{
				dat->m_pSub->WriteAsBinary( buffer );
				break;
			}
		case TYPE_STRING:
			{
				if (dat->m_sValue && *(dat->m_sValue))
				{
					buffer.PutString( dat->m_sValue );
				}
				else
				{
					buffer.PutString( "" );
				}
				break;
			}
		case TYPE_WSTRING:
			{
				Assert( !"TYPE_WSTRING" );
				break;
			}

		case TYPE_INT:
			{
				buffer.PutInt( dat->m_iValue );				
				break;
			}

		case TYPE_UINT64:
			{
				buffer.PutDouble( *((double *)dat->m_sValue) );
				break;
			}

		case TYPE_FLOAT:
			{
				buffer.PutFloat( dat->m_flValue );
				break;
			}
		case TYPE_COLOR:
			{
				buffer.PutUnsignedChar( dat->m_Color[0] );
				buffer.PutUnsignedChar( dat->m_Color[1] );
				buffer.PutUnsignedChar( dat->m_Color[2] );
				buffer.PutUnsignedChar( dat->m_Color[3] );
				break;
			}
		case TYPE_PTR:
			{
				buffer.PutUnsignedInt( (int)dat->m_pValue );
			}

		default:
			break;
		}
	}

	// write tail, marks end of peers
	buffer.PutUnsignedChar( TYPE_NUMTYPES ); 

	return buffer.IsValid();
}

// read KeyValues from binary buffer, returns true if parsing was successful
bool KeyValues::ReadAsBinary( CUtlBuffer &buffer )
{
	if ( buffer.IsText() ) // must be a binary buffer
		return false;

	if ( !buffer.IsValid() ) // must be valid, no overflows etc
		return false;

	RemoveEverything(); // remove current content
	Init();	// reset
	
	char		token[KEYVALUES_TOKEN_SIZE];
	KeyValues	*dat = this;
	types_t		type = (types_t)buffer.GetUnsignedChar();
	
	// loop through all our peers
	while ( true )
	{
		if ( type == TYPE_NUMTYPES )
			break; // no more peers

		dat->m_iDataType = type;

		buffer.GetString( token, KEYVALUES_TOKEN_SIZE-1 );
		token[KEYVALUES_TOKEN_SIZE-1] = 0;

		dat->SetName( token );
		
		switch ( type )
		{
		case TYPE_NONE:
			{
				dat->m_pSub = new KeyValues("");
				dat->m_pSub->ReadAsBinary( buffer );
				break;
			}
		case TYPE_STRING:
			{
				buffer.GetString( token, KEYVALUES_TOKEN_SIZE-1 );
				token[KEYVALUES_TOKEN_SIZE-1] = 0;

				int len = Q_strlen( token );
				dat->m_sValue = new char[len + 1];
				Q_memcpy( dat->m_sValue, token, len+1 );
								
				break;
			}
		case TYPE_WSTRING:
			{
				Assert( !"TYPE_WSTRING" );
				break;
			}

		case TYPE_INT:
			{
				dat->m_iValue = buffer.GetInt();
				break;
			}

		case TYPE_UINT64:
			{
				dat->m_sValue = new char[sizeof(uint64)];
				*((double *)dat->m_sValue) = buffer.GetDouble();
			}

		case TYPE_FLOAT:
			{
				dat->m_flValue = buffer.GetFloat();
				break;
			}
		case TYPE_COLOR:
			{
				dat->m_Color[0] = buffer.GetUnsignedChar();
				dat->m_Color[1] = buffer.GetUnsignedChar();
				dat->m_Color[2] = buffer.GetUnsignedChar();
				dat->m_Color[3] = buffer.GetUnsignedChar();
				break;
			}
		case TYPE_PTR:
			{
				dat->m_pValue = (void*)buffer.GetUnsignedInt();
			}

		default:
			break;
		}

		if ( !buffer.IsValid() ) // error occured
			return false;

		type = (types_t)buffer.GetUnsignedChar();

		if ( type == TYPE_NUMTYPES )
			break;

		// new peer follows
		dat->m_pPeer = new KeyValues("");
		dat = dat->m_pPeer;
	}

	return buffer.IsValid();
}

#include "tier0/memdbgoff.h"

//-----------------------------------------------------------------------------
// Purpose: memory allocator
//-----------------------------------------------------------------------------
void *KeyValues::operator new( unsigned int iAllocSize )
{
	MEM_ALLOC_CREDIT();
	return KeyValuesSystem()->AllocKeyValuesMemory(iAllocSize);
}

void *KeyValues::operator new( unsigned int iAllocSize, int nBlockUse, const char *pFileName, int nLine )
{
	MemAlloc_PushAllocDbgInfo( pFileName, nLine );
	void *p = KeyValuesSystem()->AllocKeyValuesMemory(iAllocSize);
	MemAlloc_PopAllocDbgInfo();
	return p;
}

//-----------------------------------------------------------------------------
// Purpose: deallocator
//-----------------------------------------------------------------------------
void KeyValues::operator delete( void *pMem )
{
	KeyValuesSystem()->FreeKeyValuesMemory(pMem);
}

void KeyValues::operator delete( void *pMem, int nBlockUse, const char *pFileName, int nLine )
{
	KeyValuesSystem()->FreeKeyValuesMemory(pMem);
}

void KeyValues::UnpackIntoStructure( KeyValuesUnpackStructure const *pUnpackTable, void *pDest )
{
	uint8 *dest=(uint8 *) pDest;
	while( pUnpackTable->m_pKeyName )
	{
		uint8 *dest_field=dest+pUnpackTable->m_nFieldOffset;
		KeyValues *find_it=FindKey( pUnpackTable->m_pKeyName );
		switch( pUnpackTable->m_eDataType )
		{
			case UNPACK_TYPE_FLOAT:
			{
				float default_value=(pUnpackTable->m_pKeyDefault)?atof(pUnpackTable->m_pKeyDefault):0.0;
				*( ( float *) dest_field)=GetFloat( pUnpackTable->m_pKeyName, default_value );
				break;
			}
			break;

			case UNPACK_TYPE_VECTOR:
			{
				Vector *dest_v=(Vector *) dest_field;
				char const *src_string=
					GetString( pUnpackTable->m_pKeyName, pUnpackTable->m_pKeyDefault );
				if ( (!src_string) ||
					 ( sscanf(src_string,"%f %f %f",
							  &(dest_v->x), &(dest_v->y), &(dest_v->z)) != 3))
					dest_v->Init( 0, 0, 0 );
			}
			break;

			case UNPACK_TYPE_FOUR_FLOATS:
			{
				float *dest_f=(float *) dest_field;
				char const *src_string=
					GetString( pUnpackTable->m_pKeyName, pUnpackTable->m_pKeyDefault );
				if ( (!src_string) ||
					 ( sscanf(src_string,"%f %f %f %f",
							  dest_f,dest_f+1,dest_f+2,dest_f+3)) != 4)
					memset( dest_f, 0, 4*sizeof(float) );
			}
			break;

			case UNPACK_TYPE_TWO_FLOATS:
			{
				float *dest_f=(float *) dest_field;
				char const *src_string=
					GetString( pUnpackTable->m_pKeyName, pUnpackTable->m_pKeyDefault );
				if ( (!src_string) ||
					 ( sscanf(src_string,"%f %f",
							  dest_f,dest_f+1)) != 2)
					memset( dest_f, 0, 2*sizeof(float) );
			}
			break;

			case UNPACK_TYPE_STRING:
			{
				char *dest_s=(char *) dest_field;
				strncpy( dest_s, GetString( pUnpackTable->m_pKeyName,
											pUnpackTable->m_pKeyDefault ),
						 pUnpackTable->m_nFieldSize );

			}
			break;

			case UNPACK_TYPE_INT:
			{
				int *dest_i=(int *) dest_field;
				int default_int=0;
				if ( pUnpackTable->m_pKeyDefault)
					default_int = atoi( pUnpackTable->m_pKeyDefault );
				*(dest_i)=GetInt( pUnpackTable->m_pKeyName, default_int );
			}
			break;

			case UNPACK_TYPE_VECTOR_COLOR:
			{
				Vector *dest_v=(Vector *) dest_field;
				if (find_it)
				{
					Color c=GetColor( pUnpackTable->m_pKeyName );
					dest_v->x = c.r();
					dest_v->y = c.g();
					dest_v->z = c.b();
				}
				else
				{
					if ( pUnpackTable->m_pKeyDefault )
						sscanf(pUnpackTable->m_pKeyDefault,"%f %f %f",
							   &(dest_v->x), &(dest_v->y), &(dest_v->z));
					else
						dest_v->Init( 0, 0, 0 );
				}
				*(dest_v) *= (1.0/255);
			}
		}
		pUnpackTable++;
	}
}

//-----------------------------------------------------------------------------
// Helper function for processing a keyvalue tree for console resolution support.
// Alters key/values for easier console video resolution support. 
// If running SD (640x480), the presence of "???_lodef" creates or slams "???".
// If running HD (1280x720), the presence of "???_hidef" creates or slams "???".
//-----------------------------------------------------------------------------
bool KeyValues::ProcessResolutionKeys( const char *pResString )
{	
	if ( !pResString )
	{
		// not for pc, console only
		return false;
	}

	KeyValues *pSubKey = GetFirstSubKey();
	if ( !pSubKey )
	{
		// not a block
		return false;
	}

	for ( ; pSubKey != NULL; pSubKey = pSubKey->GetNextKey() )
	{
		// recursively descend each sub block
		pSubKey->ProcessResolutionKeys( pResString );

		// check to see if our substring is present
		if ( Q_stristr( pSubKey->GetName(), pResString ) != NULL )
		{
			char normalKeyName[128];
			V_strncpy( normalKeyName, pSubKey->GetName(), sizeof( normalKeyName ) );

			// substring must match exactly, otherwise keys like "_lodef" and "_lodef_wide" would clash.
			char *pString = Q_stristr( normalKeyName, pResString );
			if ( pString && !Q_stricmp( pString, pResString ) )
			{
				*pString = '\0';

				// find and delete the original key (if any)
				KeyValues *pKey = FindKey( normalKeyName );
				if ( pKey )
				{		
					// remove the key
					RemoveSubKey( pKey );
				}

				// rename the marked key
				pSubKey->SetName( normalKeyName );
			}
		}
	}

	return true;
}
