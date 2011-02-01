//========= Copyright © 1996-2005, Valve Corporation, All rights reserved. ============//
//
// Purpose: This header should never be used directly from leaf code!!!
// Instead, just add the file memoverride.cpp into your project and all this
// will automagically be used
//
// $NoKeywords: $
//=============================================================================//

#ifndef TIER0_MEMALLOC_H
#define TIER0_MEMALLOC_H

#ifdef _WIN32
#pragma once
#endif

// Define this in release to get memory tracking even in release builds
//#define USE_MEM_DEBUG 1

#if defined( _MEMTEST )
#define USE_MEM_DEBUG 1
#endif

// Undefine this if using a compiler lacking threadsafe RTTI (like vc6)
#define MEM_DEBUG_CLASSNAME 1

#if !defined(STEAM) && !defined(NO_MALLOC_OVERRIDE)

#include <stddef.h>
#include "tier0/mem.h"

struct _CrtMemState;

#define MEMALLOC_VERSION 1

typedef size_t (*MemAllocFailHandler_t)( size_t );

//-----------------------------------------------------------------------------
// NOTE! This should never be called directly from leaf code
// Just use new,delete,malloc,free etc. They will call into this eventually
//-----------------------------------------------------------------------------
abstract_class IMemAlloc
{
public:
	// Release versions
	virtual void *Alloc( size_t nSize ) = 0;
	virtual void *Realloc( void *pMem, size_t nSize ) = 0;
	virtual void Free( void *pMem ) = 0;
    virtual void *Expand_NoLongerSupported( void *pMem, size_t nSize ) = 0;

	// Debug versions
    virtual void *Alloc( size_t nSize, const char *pFileName, int nLine ) = 0;
    virtual void *Realloc( void *pMem, size_t nSize, const char *pFileName, int nLine ) = 0;
    virtual void  Free( void *pMem, const char *pFileName, int nLine ) = 0;
    virtual void *Expand_NoLongerSupported( void *pMem, size_t nSize, const char *pFileName, int nLine ) = 0;

	// Returns size of a particular allocation
	virtual size_t GetSize( void *pMem ) = 0;

    // Force file + line information for an allocation
    virtual void PushAllocDbgInfo( const char *pFileName, int nLine ) = 0;
    virtual void PopAllocDbgInfo() = 0;

	// FIXME: Remove when we have our own allocator
	// these methods of the Crt debug code is used in our codebase currently
	virtual long CrtSetBreakAlloc( long lNewBreakAlloc ) = 0;
	virtual	int CrtSetReportMode( int nReportType, int nReportMode ) = 0;
	virtual int CrtIsValidHeapPointer( const void *pMem ) = 0;
	virtual int CrtIsValidPointer( const void *pMem, unsigned int size, int access ) = 0;
	virtual int CrtCheckMemory( void ) = 0;
	virtual int CrtSetDbgFlag( int nNewFlag ) = 0;
	virtual void CrtMemCheckpoint( _CrtMemState *pState ) = 0;

	// FIXME: Make a better stats interface
	virtual void DumpStats() = 0;
	virtual void DumpStatsFileBase( char const *pchFileBase ) = 0;

	// FIXME: Remove when we have our own allocator
	virtual void* CrtSetReportFile( int nRptType, void* hFile ) = 0;
	virtual void* CrtSetReportHook( void* pfnNewHook ) = 0;
	virtual int CrtDbgReport( int nRptType, const char * szFile,
			int nLine, const char * szModule, const char * pMsg ) = 0;

	virtual int heapchk() = 0;

	virtual bool IsDebugHeap() = 0;

	virtual void GetActualDbgInfo( const char *&pFileName, int &nLine ) = 0;
	virtual void RegisterAllocation( const char *pFileName, int nLine, int nLogicalSize, int nActualSize, unsigned nTime ) = 0;
	virtual void RegisterDeallocation( const char *pFileName, int nLine, int nLogicalSize, int nActualSize, unsigned nTime ) = 0;

	virtual int GetVersion() = 0;

	virtual void CompactHeap() = 0;

	// Function called when malloc fails or memory limits hit to attempt to free up memory (can come in any thread)
	virtual MemAllocFailHandler_t SetAllocFailHandler( MemAllocFailHandler_t pfnMemAllocFailHandler ) = 0;

	virtual void DumpBlockStats( void * ) = 0;

#if defined( _MEMTEST )	
	virtual void SetStatsExtraInfo( const char *pMapName, const char *pComment ) = 0;
#endif

	// Returns 0 if no failure, otherwise the size_t of the last requested chunk
	virtual size_t MemoryAllocFailed() = 0;
};

//-----------------------------------------------------------------------------
// Singleton interface
//-----------------------------------------------------------------------------
MEM_INTERFACE IMemAlloc *g_pMemAlloc;

//-----------------------------------------------------------------------------

inline void *MemAlloc_AllocAligned( size_t size, size_t align )
{
	unsigned char *pAlloc, *pResult;

	if (!IsPowerOfTwo(align))
		return NULL;

	align = (align > sizeof(void *) ? align : sizeof(void *)) - 1;

	if ( (pAlloc = (unsigned char*)g_pMemAlloc->Alloc( sizeof(void *) + align + size ) ) == (unsigned char*)NULL)
		return NULL;

	pResult = (unsigned char*)( (size_t)(pAlloc + sizeof(void *) + align ) & ~align );
	((unsigned char**)(pResult))[-1] = pAlloc;

	return (void *)pResult;
}

inline void *MemAlloc_AllocAligned( size_t size, size_t align, const char *pszFile, int nLine )
{
	unsigned char *pAlloc, *pResult;

	if (!IsPowerOfTwo(align))
		return NULL;

	align = (align > sizeof(void *) ? align : sizeof(void *)) - 1;

	if ( (pAlloc = (unsigned char*)g_pMemAlloc->Alloc( sizeof(void *) + align + size, pszFile, nLine ) ) == (unsigned char*)NULL)
		return NULL;

	pResult = (unsigned char*)( (size_t)(pAlloc + sizeof(void *) + align ) & ~align );
	((unsigned char**)(pResult))[-1] = pAlloc;

	return (void *)pResult;
}

inline void *MemAlloc_ReallocAligned( void *ptr, size_t size, size_t align )
{
	if ( !IsPowerOfTwo( align ) )
		return NULL;

	// Don't change alignment between allocation + reallocation.
	if ( ( (size_t)ptr & ( align - 1 ) ) != 0 )
		return NULL;

	if ( !ptr )
		return MemAlloc_AllocAligned( size, align );

	void *pAlloc, *pResult;

	// Figure out the actual allocation point
	pAlloc = ptr;
	pAlloc = (void *)(((size_t)pAlloc & ~( sizeof(void *) - 1 ) ) - sizeof(void *));
	pAlloc = *( (void **)pAlloc );

	// See if we have enough space
	size_t nOffset = (size_t)ptr - (size_t)pAlloc;
	size_t nOldSize = g_pMemAlloc->GetSize( pAlloc );
	if ( nOldSize >= size + nOffset )
		return ptr;

	pResult = MemAlloc_AllocAligned( size, align );
	memcpy( pResult, ptr, nOldSize - nOffset );
	g_pMemAlloc->Free( pAlloc );
	return pResult;
}

inline void MemAlloc_FreeAligned( void *pMemBlock )
{
	void *pAlloc;

	if ( pMemBlock == NULL )
		return;

	pAlloc = pMemBlock;

	// pAlloc points to the pointer to starting of the memory block
	pAlloc = (void *)(((size_t)pAlloc & ~( sizeof(void *) - 1 ) ) - sizeof(void *));

	// pAlloc is the pointer to the start of memory block
	pAlloc = *( (void **)pAlloc );
	g_pMemAlloc->Free( pAlloc );
}

inline size_t MemAlloc_GetSizeAligned( void *pMemBlock )
{
	void *pAlloc;

	if ( pMemBlock == NULL )
		return 0;

	pAlloc = pMemBlock;

	// pAlloc points to the pointer to starting of the memory block
	pAlloc = (void *)(((size_t)pAlloc & ~( sizeof(void *) - 1 ) ) - sizeof(void *));

	// pAlloc is the pointer to the start of memory block
	pAlloc = *((void **)pAlloc );
	return g_pMemAlloc->GetSize( pAlloc ) - ( (byte *)pMemBlock - (byte *)pAlloc );
}

//-----------------------------------------------------------------------------

#if (defined(_DEBUG) || defined(USE_MEM_DEBUG))
#define MEM_ALLOC_CREDIT_(tag)	CMemAllocAttributeAlloction memAllocAttributeAlloction( tag, __LINE__ )
#define MemAlloc_PushAllocDbgInfo( pszFile, line ) g_pMemAlloc->PushAllocDbgInfo( pszFile, line )
#define MemAlloc_PopAllocDbgInfo() g_pMemAlloc->PopAllocDbgInfo()
#define MemAlloc_RegisterAllocation( pFileName, nLine, nLogicalSize, nActualSize, nTime ) g_pMemAlloc->RegisterAllocation( pFileName, nLine, nLogicalSize, nActualSize, nTime )
#define MemAlloc_RegisterDeallocation( pFileName, nLine, nLogicalSize, nActualSize, nTime ) g_pMemAlloc->RegisterDeallocation( pFileName, nLine, nLogicalSize, nActualSize, nTime )
#else
#define MEM_ALLOC_CREDIT_(tag)	((void)0)
#define MemAlloc_PushAllocDbgInfo( pszFile, line ) ((void)0)
#define MemAlloc_PopAllocDbgInfo() ((void)0)
#define MemAlloc_RegisterAllocation( pFileName, nLine, nLogicalSize, nActualSize, nTime ) ((void)0)
#define MemAlloc_RegisterDeallocation( pFileName, nLine, nLogicalSize, nActualSize, nTime ) ((void)0)
#endif

//-----------------------------------------------------------------------------

class CMemAllocAttributeAlloction
{
public:
	CMemAllocAttributeAlloction( const char *pszFile, int line ) 
	{
		MemAlloc_PushAllocDbgInfo( pszFile, line );
	}
	
	~CMemAllocAttributeAlloction()
	{
		MemAlloc_PopAllocDbgInfo();
	}
};

#define MEM_ALLOC_CREDIT()	MEM_ALLOC_CREDIT_(__FILE__)

//-----------------------------------------------------------------------------

#if defined(_WIN32) && ( defined(_DEBUG) || defined(USE_MEM_DEBUG) )

	#pragma warning(disable:4290)
	#pragma warning(push)
	#include <typeinfo.h>

	// MEM_DEBUG_CLASSNAME is opt-in.
	// Note: typeid().name() is not threadsafe, so if the project needs to access it in multiple threads
	// simultaneously, it'll need a mutex.
	#if defined(_CPPRTTI) && defined(MEM_DEBUG_CLASSNAME)
		#define MEM_ALLOC_CREDIT_CLASS()	MEM_ALLOC_CREDIT_( typeid(*this).name() )
		#define MEM_ALLOC_CLASSNAME(type) (typeid((type*)(0)).name())
	#else
		#define MEM_ALLOC_CREDIT_CLASS()	MEM_ALLOC_CREDIT_( __FILE__ )
		#define MEM_ALLOC_CLASSNAME(type) (__FILE__)
	#endif

	// MEM_ALLOC_CREDIT_FUNCTION is used when no this pointer is available ( inside 'new' overloads, for example )
	#ifdef _MSC_VER
		#define MEM_ALLOC_CREDIT_FUNCTION()		MEM_ALLOC_CREDIT_( __FUNCTION__ )
	#else
		#define MEM_ALLOC_CREDIT_FUNCTION() (__FILE__)
	#endif

	#pragma warning(pop)
#else
	#define MEM_ALLOC_CREDIT_CLASS()
	#define MEM_ALLOC_CLASSNAME(type) NULL
	#define MEM_ALLOC_CREDIT_FUNCTION() 
#endif

//-----------------------------------------------------------------------------

#if (defined(_DEBUG) || defined(USE_MEM_DEBUG))
struct MemAllocFileLine_t
{
	const char *pszFile;
	int line;
};

#define MEMALLOC_DEFINE_EXTERNAL_TRACKING( tag ) \
	static CUtlMap<void *, MemAllocFileLine_t, int> g_##tag##Allocs( DefLessFunc( void *) ); \
	static const char *g_psz##tag##Alloc = strcpy( (char *)g_pMemAlloc->Alloc( strlen( #tag "Alloc" ) + 1, "intentional leak", 0 ), #tag "Alloc" );

#define MemAlloc_RegisterExternalAllocation( tag, p, size ) \
	if ( !p ) \
		; \
	else \
	{ \
		MemAllocFileLine_t fileLine = { g_psz##tag##Alloc, 0 }; \
		g_pMemAlloc->GetActualDbgInfo( fileLine.pszFile, fileLine.line ); \
		if ( fileLine.pszFile != g_psz##tag##Alloc ) \
		{ \
			g_##tag##Allocs.Insert( p, fileLine ); \
		} \
		\
		MemAlloc_RegisterAllocation( fileLine.pszFile, fileLine.line, size, size, 0); \
	}

#define MemAlloc_RegisterExternalDeallocation( tag, p, size ) \
	if ( !p ) \
		; \
	else \
	{ \
		MemAllocFileLine_t fileLine = { g_psz##tag##Alloc, 0 }; \
		CUtlMap<void *, MemAllocFileLine_t, int>::IndexType_t iRecordedFileLine = g_##tag##Allocs.Find( p ); \
		if ( iRecordedFileLine !=  g_##tag##Allocs.InvalidIndex() ) \
		{ \
			fileLine = g_##tag##Allocs[iRecordedFileLine]; \
			g_##tag##Allocs.RemoveAt( iRecordedFileLine ); \
		} \
		\
		MemAlloc_RegisterDeallocation( fileLine.pszFile, fileLine.line, size, size, 0); \
	}

#else

#define MEMALLOC_DEFINE_EXTERNAL_TRACKING( tag )
#define MemAlloc_RegisterExternalAllocation( tag, p, size ) ((void)0)
#define MemAlloc_RegisterExternalDeallocation( tag, p, size ) ((void)0)

#endif

//-----------------------------------------------------------------------------

#endif // !STEAM && !NO_MALLOC_OVERRIDE

//-----------------------------------------------------------------------------

#if !defined(STEAM) && defined(NO_MALLOC_OVERRIDE)

#define MEM_ALLOC_CREDIT_(tag)	((void)0)
#define MEM_ALLOC_CREDIT()	MEM_ALLOC_CREDIT_(__FILE__)
#define MEM_ALLOC_CREDIT_CLASS()
#define MEM_ALLOC_CLASSNAME(type) NULL

#endif !STEAM && NO_MALLOC_OVERRIDE

//-----------------------------------------------------------------------------

#endif /* TIER0_MEMALLOC_H */
