

#include "csimplescan.h"

#define CREATEINTERFACE_PROCNAME	"CreateInterface"

// load/unload components
//class CSysModule;

//-----------------------------------------------------------------------------
// Purpose: returns a pointer to a function, given a module
// Input  : pModuleName - module name
//			*pName - proc name
//-----------------------------------------------------------------------------
static void *Sys_GetProcAddress( const char *pModuleName, const char *pName )
{
	HMODULE hModule = GetModuleHandle( pModuleName );
	return GetProcAddress( hModule, pName );
}

//-----------------------------------------------------------------------------
// Purpose: returns the instance of the named module
// Input  : *pModuleName - name of the module
// Output : interface_instance_t - instance of that module
//-----------------------------------------------------------------------------
CreateInterfaceFn Sys_GetFactory( const char *pModuleName )
{
#ifdef _WIN32
	return static_cast<CreateInterfaceFn>( Sys_GetProcAddress( pModuleName, CREATEINTERFACE_PROCNAME ) );
#elif defined(_LINUX)
	// see Sys_GetFactory( CSysModule *pModule ) for an explanation
	return (CreateInterfaceFn)( Sys_GetProcAddress( pModuleName, CREATEINTERFACE_PROCNAME ) );
#endif
}


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