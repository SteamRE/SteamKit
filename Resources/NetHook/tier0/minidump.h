//========= Copyright © 1996-2005, Valve Corporation, All rights reserved. ============//
//
// Purpose: 
//
//=============================================================================//

#ifndef MINIDUMP_H
#define MINIDUMP_H
#ifdef _WIN32
#pragma once
#endif

#include "tier0/platform.h"

// writes out a minidump of the current stack trace with a unique filename
PLATFORM_INTERFACE void WriteMiniDump();

typedef void (*FnWMain)( int , tchar *[] );

#if defined(_WIN32) && !defined(_X360)

// calls the passed in function pointer and catches any exceptions/crashes thrown by it, and writes a minidump
// use from wmain() to protect the whole program

PLATFORM_INTERFACE void CatchAndWriteMiniDump( FnWMain pfn, int argc, tchar *argv[] );

#include <dbghelp.h>

// Replaces the current function pointer with the one passed in.
// Returns the previously-set function.
// The function is called internally by WriteMiniDump() and CatchAndWriteMiniDump()
// The default is the built-in function that uses DbgHlp.dll's MiniDumpWriteDump function
typedef void (*FnMiniDump)( unsigned int uStructuredExceptionCode, _EXCEPTION_POINTERS * pExceptionInfo );
PLATFORM_INTERFACE FnMiniDump SetMiniDumpFunction( FnMiniDump pfn );

// Use this to write a minidump explicitly.
// Some of the tools choose to catch the minidump themselves instead of using CatchAndWriteMinidump
// so they can show their own dialog.
//
// ptchMinidumpFileNameBuffer if not-NULL should be a writable tchar buffer of length at
// least _MAX_PATH and on return will contain the name of the minidump file written.
// If ptchMinidumpFileNameBuffer is NULL the name of the minidump file written will not
// be available after the function returns.
//
PLATFORM_INTERFACE bool WriteMiniDumpUsingExceptionInfo( 
	unsigned int uStructuredExceptionCode,
	_EXCEPTION_POINTERS * pExceptionInfo, 
	MINIDUMP_TYPE minidumpType,
	tchar *ptchMinidumpFileNameBuffer = NULL
	);
#endif

#endif // MINIDUMP_H
