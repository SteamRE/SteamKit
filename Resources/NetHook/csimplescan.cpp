

#include "csimplescan.h"


CSimpleScan::CSimpleScan()
{
	m_bInterfaceSet = false;
}

CSimpleScan::CSimpleScan( const char *filename )
{
	SetDLL( filename );
}

bool CSimpleScan::SetDLL( const char *filename )
{
	m_Interface = Sys_GetFactory( filename );

	CSigScan::sigscan_dllfunc = m_Interface;

	if ( !CSigScan::GetDllMemInfo() )
		return m_bInterfaceSet = false;

	m_bInterfaceSet = ( m_Interface != NULL );

	return m_bInterfaceSet;
}

bool CSimpleScan::FindFunction( const char *sig, const char *mask, void **func )
{
	if ( !m_bInterfaceSet )
		return false;

	
	m_Signature.Init( ( unsigned char * )sig, ( char * )mask, strlen( mask ) );

	if ( !m_Signature.is_set )
		return false;

	*func = m_Signature.sig_addr;

	return true;
}