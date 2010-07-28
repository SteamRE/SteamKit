//========== Copyright © 2005, Valve Corporation, All rights reserved. ========
//
// Purpose: A collection of utility classes to simplify thread handling, and
//			as much as possible contain portability problems. Here avoiding 
//			including windows.h.
//
//=============================================================================

#ifndef THREADTOOLS_H
#define THREADTOOLS_H

#include <limits.h>

#include "tier0/platform.h"
#include "tier0/dbg.h"
#include "tier0/vcrmode.h"

#ifdef _LINUX
#include <pthread.h>
#include <errno.h>
#endif

#if defined( _WIN32 )
#pragma once
#pragma warning(push)
#pragma warning(disable:4251)
#endif

// #define THREAD_PROFILER 1

#ifndef STATIC_TIER0

#ifdef TIER0_DLL_EXPORT
#define TT_INTERFACE	DLL_EXPORT
#define TT_OVERLOAD	DLL_GLOBAL_EXPORT
#define TT_CLASS		DLL_CLASS_EXPORT
#else
#define TT_INTERFACE	DLL_IMPORT
#define TT_OVERLOAD	DLL_GLOBAL_IMPORT
#define TT_CLASS		DLL_CLASS_IMPORT
#endif

#else // BUILD_AS_DLL

#define TT_INTERFACE	extern
#define TT_OVERLOAD	
#define TT_CLASS		
#endif // BUILD_AS_DLL

#ifndef _RETAIL
#define THREAD_MUTEX_TRACING_SUPPORTED
#if defined(_WIN32) && defined(_DEBUG)
#define THREAD_MUTEX_TRACING_ENABLED
#endif
#endif

#ifdef _WIN32
typedef void *HANDLE;
#endif

//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------

const unsigned TT_INFINITE = 0xffffffff;

#ifndef NO_THREAD_LOCAL

#ifndef THREAD_LOCAL
#ifdef _WIN32
#define THREAD_LOCAL __declspec(thread)
#elif _LINUX
#define THREAD_LOCAL __thread
#endif
#endif

#endif // NO_THREAD_LOCAL

typedef unsigned long ThreadId_t;

//-----------------------------------------------------------------------------
//
// Simple thread creation. Differs from VCR mode/CreateThread/_beginthreadex
// in that it accepts a standard C function rather than compiler specific one.
//
//-----------------------------------------------------------------------------
FORWARD_DECLARE_HANDLE( ThreadHandle_t );
typedef unsigned (*ThreadFunc_t)( void *pParam );

TT_OVERLOAD ThreadHandle_t CreateSimpleThread( ThreadFunc_t, void *pParam, ThreadId_t *pID, unsigned stackSize = 0 );
TT_INTERFACE ThreadHandle_t CreateSimpleThread( ThreadFunc_t, void *pParam, unsigned stackSize = 0 );
TT_INTERFACE bool ReleaseThreadHandle( ThreadHandle_t );


//-----------------------------------------------------------------------------

TT_INTERFACE void ThreadSleep(unsigned duration = 0);
TT_INTERFACE uint ThreadGetCurrentId();
TT_INTERFACE ThreadHandle_t ThreadGetCurrentHandle();
TT_INTERFACE int ThreadGetPriority( ThreadHandle_t hThread = NULL );
TT_INTERFACE bool ThreadSetPriority( ThreadHandle_t hThread, int priority );
inline		 bool ThreadSetPriority( int priority ) { return ThreadSetPriority( NULL, priority ); }
TT_INTERFACE bool ThreadInMainThread();
TT_INTERFACE void DeclareCurrentThreadIsMainThread();

// NOTE: ThreadedLoadLibraryFunc_t needs to return the sleep time in milliseconds or TT_INFINITE
typedef int (*ThreadedLoadLibraryFunc_t)(); 
TT_INTERFACE void SetThreadedLoadLibraryFunc( ThreadedLoadLibraryFunc_t func );
TT_INTERFACE ThreadedLoadLibraryFunc_t GetThreadedLoadLibraryFunc();

#if defined( _WIN32 ) && !defined( _WIN64 ) && !defined( _X360 )
extern "C" unsigned long __declspec(dllimport) __stdcall GetCurrentThreadId();
#define ThreadGetCurrentId GetCurrentThreadId
#endif

inline void ThreadPause()
{
#if defined( _WIN32 ) && !defined( _X360 )
	__asm pause;
#elif _LINUX
	__asm __volatile("pause");
#elif defined( _X360 )
#else
#error "implement me"
#endif
}

TT_INTERFACE bool ThreadJoin( ThreadHandle_t, unsigned timeout = TT_INFINITE );

TT_INTERFACE void ThreadSetDebugName( ThreadId_t id, const char *pszName );
inline		 void ThreadSetDebugName( const char *pszName ) { ThreadSetDebugName( (ThreadId_t)-1, pszName ); }

TT_INTERFACE void ThreadSetAffinity( ThreadHandle_t hThread, int nAffinityMask );

//-----------------------------------------------------------------------------

enum ThreadWaitResult_t
{
	TW_FAILED = 0xffffffff, // WAIT_FAILED
	TW_TIMEOUT = 0x00000102, // WAIT_TIMEOUT
};

#ifdef _WIN32
TT_INTERFACE int ThreadWaitForObjects( int nEvents, const HANDLE *pHandles, bool bWaitAll = true, unsigned timeout = TT_INFINITE );
inline int ThreadWaitForObject( HANDLE handle, bool bWaitAll = true, unsigned timeout = TT_INFINITE ) { return ThreadWaitForObjects( 1, &handle, bWaitAll, timeout ); }
#endif

//-----------------------------------------------------------------------------
//
// Interlock methods. These perform very fast atomic thread
// safe operations. These are especially relevant in a multi-core setting.
//
//-----------------------------------------------------------------------------

#ifdef _WIN32
#define NOINLINE
#elif _LINUX
#define NOINLINE __attribute__ ((noinline))
#endif

#if defined(_WIN32) && !defined(_X360)
#if ( _MSC_VER >= 1310 )
#define USE_INTRINSIC_INTERLOCKED
#endif
#endif

#ifdef USE_INTRINSIC_INTERLOCKED
extern "C"
{
	long __cdecl _InterlockedIncrement(volatile long*);
	long __cdecl _InterlockedDecrement(volatile long*);
	long __cdecl _InterlockedExchange(volatile long*, long);
	long __cdecl _InterlockedExchangeAdd(volatile long*, long);
	long __cdecl _InterlockedCompareExchange(volatile long*, long, long);
}

#pragma intrinsic( _InterlockedCompareExchange )
#pragma intrinsic( _InterlockedDecrement )
#pragma intrinsic( _InterlockedExchange )
#pragma intrinsic( _InterlockedExchangeAdd ) 
#pragma intrinsic( _InterlockedIncrement )

inline long ThreadInterlockedIncrement( long volatile *p )										{ Assert( (size_t)p % 4 == 0 ); return _InterlockedIncrement( p ); }
inline long ThreadInterlockedDecrement( long volatile *p )										{ Assert( (size_t)p % 4 == 0 ); return _InterlockedDecrement( p ); }
inline long ThreadInterlockedExchange( long volatile *p, long value )							{ Assert( (size_t)p % 4 == 0 ); return _InterlockedExchange( p, value ); }
inline long ThreadInterlockedExchangeAdd( long volatile *p, long value )						{ Assert( (size_t)p % 4 == 0 ); return _InterlockedExchangeAdd( p, value ); }
inline long ThreadInterlockedCompareExchange( long volatile *p, long value, long comperand )	{ Assert( (size_t)p % 4 == 0 ); return _InterlockedCompareExchange( p, value, comperand ); }
inline bool ThreadInterlockedAssignIf( long volatile *p, long value, long comperand )			{ Assert( (size_t)p % 4 == 0 ); return ( _InterlockedCompareExchange( p, value, comperand ) == comperand ); }
#else
TT_INTERFACE long ThreadInterlockedIncrement( long volatile * ) NOINLINE;
TT_INTERFACE long ThreadInterlockedDecrement( long volatile * ) NOINLINE;
TT_INTERFACE long ThreadInterlockedExchange( long volatile *, long value ) NOINLINE;
TT_INTERFACE long ThreadInterlockedExchangeAdd( long volatile *, long value ) NOINLINE;
TT_INTERFACE long ThreadInterlockedCompareExchange( long volatile *, long value, long comperand ) NOINLINE;
TT_INTERFACE bool ThreadInterlockedAssignIf( long volatile *, long value, long comperand ) NOINLINE;
#endif

inline unsigned ThreadInterlockedExchangeSubtract( long volatile *p, long value )	{ return ThreadInterlockedExchangeAdd( (long volatile *)p, -value ); }

#if defined( USE_INTRINSIC_INTERLOCKED ) && !defined( _WIN64 )
#define TIPTR()
inline void *ThreadInterlockedExchangePointer( void * volatile *p, void *value )							{ return (void *)_InterlockedExchange( reinterpret_cast<long volatile *>(p), reinterpret_cast<long>(value) ); }
inline void *ThreadInterlockedCompareExchangePointer( void * volatile *p, void *value, void *comperand )	{ return (void *)_InterlockedCompareExchange( reinterpret_cast<long volatile *>(p), reinterpret_cast<long>(value), reinterpret_cast<long>(comperand) ); }
inline bool ThreadInterlockedAssignPointerIf( void * volatile *p, void *value, void *comperand )			{ return ( _InterlockedCompareExchange( reinterpret_cast<long volatile *>(p), reinterpret_cast<long>(value), reinterpret_cast<long>(comperand) ) == reinterpret_cast<long>(comperand) ); }
#else
TT_INTERFACE void *ThreadInterlockedExchangePointer( void * volatile *, void *value ) NOINLINE;
TT_INTERFACE void *ThreadInterlockedCompareExchangePointer( void * volatile *, void *value, void *comperand ) NOINLINE;
TT_INTERFACE bool ThreadInterlockedAssignPointerIf( void * volatile *, void *value, void *comperand ) NOINLINE;
#endif

inline void const *ThreadInterlockedExchangePointerToConst( void const * volatile *p, void const *value )							{ return ThreadInterlockedExchangePointer( const_cast < void * volatile * > ( p ), const_cast < void * > ( value ) );  }
inline void const *ThreadInterlockedCompareExchangePointerToConst( void const * volatile *p, void const *value, void const *comperand )	{ return ThreadInterlockedCompareExchangePointer( const_cast < void * volatile * > ( p ), const_cast < void * > ( value ), const_cast < void * > ( comperand ) ); }
inline bool ThreadInterlockedAssignPointerToConstIf( void const * volatile *p, void const *value, void const *comperand )			{ return ThreadInterlockedAssignPointerIf( const_cast < void * volatile * > ( p ), const_cast < void * > ( value ), const_cast < void * > ( comperand ) ); }

TT_INTERFACE int64 ThreadInterlockedIncrement64( int64 volatile * ) NOINLINE;
TT_INTERFACE int64 ThreadInterlockedDecrement64( int64 volatile * ) NOINLINE;
TT_INTERFACE int64 ThreadInterlockedCompareExchange64( int64 volatile *, int64 value, int64 comperand ) NOINLINE;
TT_INTERFACE int64 ThreadInterlockedExchange64( int64 volatile *, int64 value ) NOINLINE;
TT_INTERFACE int64 ThreadInterlockedExchangeAdd64( int64 volatile *, int64 value ) NOINLINE;
TT_INTERFACE bool ThreadInterlockedAssignIf64(volatile int64 *pDest, int64 value, int64 comperand ) NOINLINE;

inline unsigned ThreadInterlockedExchangeSubtract( unsigned volatile *p, unsigned value )	{ return ThreadInterlockedExchangeAdd( (long volatile *)p, value ); }
inline unsigned ThreadInterlockedIncrement( unsigned volatile *p )	{ return ThreadInterlockedIncrement( (long volatile *)p ); }
inline unsigned ThreadInterlockedDecrement( unsigned volatile *p )	{ return ThreadInterlockedDecrement( (long volatile *)p ); }
inline unsigned ThreadInterlockedExchange( unsigned volatile *p, unsigned value )	{ return ThreadInterlockedExchange( (long volatile *)p, value ); }
inline unsigned ThreadInterlockedExchangeAdd( unsigned volatile *p, unsigned value )	{ return ThreadInterlockedExchangeAdd( (long volatile *)p, value ); }
inline unsigned ThreadInterlockedCompareExchange( unsigned volatile *p, unsigned value, unsigned comperand )	{ return ThreadInterlockedCompareExchange( (long volatile *)p, value, comperand ); }
inline bool ThreadInterlockedAssignIf( unsigned volatile *p, unsigned value, unsigned comperand )	{ return ThreadInterlockedAssignIf( (long volatile *)p, value, comperand ); }

inline int ThreadInterlockedExchangeSubtract( int volatile *p, int value )	{ return ThreadInterlockedExchangeAdd( (long volatile *)p, value ); }
inline int ThreadInterlockedIncrement( int volatile *p )	{ return ThreadInterlockedIncrement( (long volatile *)p ); }
inline int ThreadInterlockedDecrement( int volatile *p )	{ return ThreadInterlockedDecrement( (long volatile *)p ); }
inline int ThreadInterlockedExchange( int volatile *p, int value )	{ return ThreadInterlockedExchange( (long volatile *)p, value ); }
inline int ThreadInterlockedExchangeAdd( int volatile *p, int value )	{ return ThreadInterlockedExchangeAdd( (long volatile *)p, value ); }
inline int ThreadInterlockedCompareExchange( int volatile *p, int value, int comperand )	{ return ThreadInterlockedCompareExchange( (long volatile *)p, value, comperand ); }
inline bool ThreadInterlockedAssignIf( int volatile *p, int value, int comperand )	{ return ThreadInterlockedAssignIf( (long volatile *)p, value, comperand ); }

//-----------------------------------------------------------------------------
// Access to VTune thread profiling
//-----------------------------------------------------------------------------
#if defined(_WIN32) && defined(THREAD_PROFILER)
TT_INTERFACE void ThreadNotifySyncPrepare(void *p);
TT_INTERFACE void ThreadNotifySyncCancel(void *p);
TT_INTERFACE void ThreadNotifySyncAcquired(void *p);
TT_INTERFACE void ThreadNotifySyncReleasing(void *p);
#else
#define ThreadNotifySyncPrepare(p)		((void)0)
#define ThreadNotifySyncCancel(p)		((void)0)
#define ThreadNotifySyncAcquired(p)		((void)0)
#define ThreadNotifySyncReleasing(p)	((void)0)
#endif

//-----------------------------------------------------------------------------
// Encapsulation of a thread local datum (needed because THREAD_LOCAL doesn't
// work in a DLL loaded with LoadLibrary()
//-----------------------------------------------------------------------------

#ifndef __AFXTLS_H__ // not compatible with some Windows headers
#ifndef NO_THREAD_LOCAL

class TT_CLASS CThreadLocalBase
{
public:
	CThreadLocalBase();
	~CThreadLocalBase();

	void * Get() const;
	void   Set(void *);

private:
#ifdef _WIN32
	uint32 m_index;
#elif _LINUX
	pthread_key_t m_index;
#endif
};

//---------------------------------------------------------

#ifndef __AFXTLS_H__

template <class T>
class CThreadLocal : public CThreadLocalBase
{
public:
	CThreadLocal()
	{
		COMPILE_TIME_ASSERT( sizeof(T) == sizeof(void *) );
	}

	T Get() const
	{
		return reinterpret_cast<T>(CThreadLocalBase::Get());
	}

	void Set(T val)
	{
		CThreadLocalBase::Set(reinterpret_cast<void *>(val));
	}
};

#endif

//---------------------------------------------------------

template <class T = int>
class CThreadLocalInt : public CThreadLocal<T>
{
public:
	operator const T() const { return Get(); }
	int	operator=( T i ) { Set( i ); return i; }

	T operator++()					{ T i = Get(); Set( ++i ); return i; }
	T operator++(int)				{ T i = Get(); Set( i + 1 ); return i; }

	T operator--()					{ T i = Get(); Set( --i ); return i; }
	T operator--(int)				{ T i = Get(); Set( i - 1 ); return i; }
};

//---------------------------------------------------------

template <class T>
class CThreadLocalPtr : private CThreadLocalBase
{
public:
	CThreadLocalPtr() {}

	operator const void *() const          					{ return (T *)Get(); }
	operator void *()                      					{ return (T *)Get(); }

	operator const T *() const							    { return (T *)Get(); }
	operator const T *()          							{ return (T *)Get(); }
	operator T *()											{ return (T *)Get(); }

	int			operator=( int i )							{ AssertMsg( i == 0, "Only NULL allowed on integer assign" ); Set( NULL ); return 0; }
	T *			operator=( T *p )							{ Set( p ); return p; }

	bool        operator !() const							{ return (!Get()); }
	bool        operator!=( int i ) const					{ AssertMsg( i == 0, "Only NULL allowed on integer compare" ); return (Get() != NULL); }
	bool        operator==( int i ) const					{ AssertMsg( i == 0, "Only NULL allowed on integer compare" ); return (Get() == NULL); }
	bool		operator==( const void *p ) const			{ return (Get() == p); }
	bool		operator!=( const void *p ) const			{ return (Get() != p); }
	bool		operator==( const T *p ) const				{ return operator==((void*)p); }
	bool		operator!=( const T *p ) const				{ return operator!=((void*)p); }

	T *  		operator->()								{ return (T *)Get(); }
	T &  		operator *()								{ return *((T *)Get()); }

	const T *   operator->() const							{ return (T *)Get(); }
	const T &   operator *() const							{ return *((T *)Get()); }

	const T &	operator[]( int i ) const					{ return *((T *)Get() + i); }
	T &			operator[]( int i )							{ return *((T *)Get() + i); }

private:
	// Disallowed operations
	CThreadLocalPtr( T *pFrom );
	CThreadLocalPtr( const CThreadLocalPtr<T> &from );
	T **operator &();
	T * const *operator &() const;
	void operator=( const CThreadLocalPtr<T> &from );
	bool operator==( const CThreadLocalPtr<T> &p ) const;
	bool operator!=( const CThreadLocalPtr<T> &p ) const;
};

#endif // NO_THREAD_LOCAL
#endif // !__AFXTLS_H__

//-----------------------------------------------------------------------------
//
// A super-fast thread-safe integer A simple class encapsulating the notion of an 
// atomic integer used across threads that uses the built in and faster 
// "interlocked" functionality rather than a full-blown mutex. Useful for simple 
// things like reference counts, etc.
//
//-----------------------------------------------------------------------------

template <typename T>
class CInterlockedIntT
{
public:
	CInterlockedIntT() : m_value( 0 ) 				{ COMPILE_TIME_ASSERT( sizeof(T) == sizeof(long) ); }
	CInterlockedIntT( T value ) : m_value( value ) 	{}

	operator T() const				{ return m_value; }

	bool operator!() const			{ return ( m_value == 0 ); }
	bool operator==( T rhs ) const	{ return ( m_value == rhs ); }
	bool operator!=( T rhs ) const	{ return ( m_value != rhs ); }

	T operator++()					{ return (T)ThreadInterlockedIncrement( (long *)&m_value ); }
	T operator++(int)				{ return operator++() - 1; }

	T operator--()					{ return (T)ThreadInterlockedDecrement( (long *)&m_value ); }
	T operator--(int)				{ return operator--() + 1; }

	bool AssignIf( T conditionValue, T newValue )	{ return ThreadInterlockedAssignIf( (long *)&m_value, (long)newValue, (long)conditionValue ); }

	T operator=( T newValue )		{ ThreadInterlockedExchange((long *)&m_value, newValue); return m_value; }

	void operator+=( T add )		{ ThreadInterlockedExchangeAdd( (long *)&m_value, (long)add ); }
	void operator-=( T subtract )	{ operator+=( -subtract ); }
	void operator*=( T multiplier )	{ 
		T original, result; 
		do 
		{ 
			original = m_value; 
			result = original * multiplier; 
		} while ( !AssignIf( original, result ) );
	}
	void operator/=( T divisor )	{ 
		T original, result; 
		do 
		{ 
			original = m_value; 
			result = original / divisor;
		} while ( !AssignIf( original, result ) );
	}

	T operator+( T rhs ) const		{ return m_value + rhs; }
	T operator-( T rhs ) const		{ return m_value - rhs; }

private:
	volatile T m_value;
};

typedef CInterlockedIntT<int> CInterlockedInt;
typedef CInterlockedIntT<unsigned> CInterlockedUInt;

//-----------------------------------------------------------------------------

template <typename T>
class CInterlockedPtr
{
public:
	CInterlockedPtr() : m_value( 0 ) 				{ COMPILE_TIME_ASSERT( sizeof(T *) == sizeof(long) ); /* Will need to rework operator+= for 64 bit */ }
	CInterlockedPtr( T *value ) : m_value( value ) 	{}

	operator T *() const			{ return m_value; }

	bool operator!() const			{ return ( m_value == 0 ); }
	bool operator==( T *rhs ) const	{ return ( m_value == rhs ); }
	bool operator!=( T *rhs ) const	{ return ( m_value != rhs ); }

	T *operator++()					{ return ((T *)ThreadInterlockedExchangeAdd( (long *)&m_value, sizeof(T) )) + 1; }
	T *operator++(int)				{ return (T *)ThreadInterlockedExchangeAdd( (long *)&m_value, sizeof(T) ); }

	T *operator--()					{ return ((T *)ThreadInterlockedExchangeAdd( (long *)&m_value, -sizeof(T) )) - 1; }
	T *operator--(int)				{ return (T *)ThreadInterlockedExchangeAdd( (long *)&m_value, -sizeof(T) ); }

	bool AssignIf( T *conditionValue, T *newValue )	{ return ThreadInterlockedAssignPointerToConstIf( (void const **) &m_value, (void const *) newValue, (void const *) conditionValue ); }

	T *operator=( T *newValue )		{ ThreadInterlockedExchangePointerToConst( (void const **) &m_value, (void const *) newValue ); return newValue; }

	void operator+=( int add )		{ ThreadInterlockedExchangeAdd( (long *)&m_value, add * sizeof(T) ); }
	void operator-=( int subtract )	{ operator+=( -subtract ); }

	T *operator+( int rhs ) const		{ return m_value + rhs; }
	T *operator-( int rhs ) const		{ return m_value - rhs; }
	T *operator+( unsigned rhs ) const	{ return m_value + rhs; }
	T *operator-( unsigned rhs ) const	{ return m_value - rhs; }
	size_t operator-( T *p ) const		{ return m_value - p; }
	size_t operator-( const CInterlockedPtr<T> &p ) const	{ return m_value - p.m_value; }

private:
	T * volatile m_value;
};


//-----------------------------------------------------------------------------
//
// Platform independent for critical sections management
//
//-----------------------------------------------------------------------------

class TT_CLASS CThreadMutex
{
public:
	CThreadMutex();
	~CThreadMutex();

	//------------------------------------------------------
	// Mutex acquisition/release. Const intentionally defeated.
	//------------------------------------------------------
	void Lock();
	void Lock() const		{ (const_cast<CThreadMutex *>(this))->Lock(); }
	void Unlock();
	void Unlock() const		{ (const_cast<CThreadMutex *>(this))->Unlock(); }

	bool TryLock();
	bool TryLock() const	{ return (const_cast<CThreadMutex *>(this))->TryLock(); }

	//------------------------------------------------------
	// Use this to make deadlocks easier to track by asserting
	// when it is expected that the current thread owns the mutex
	//------------------------------------------------------
	bool AssertOwnedByCurrentThread();

	//------------------------------------------------------
	// Enable tracing to track deadlock problems
	//------------------------------------------------------
	void SetTrace( bool );

private:
	// Disallow copying
	CThreadMutex( const CThreadMutex & );
	CThreadMutex &operator=( const CThreadMutex & );

#if defined( _WIN32 )
	// Efficient solution to breaking the windows.h dependency, invariant is tested.
#ifdef _WIN64
	#define TT_SIZEOF_CRITICALSECTION 40	
#else
#ifndef _X360
	#define TT_SIZEOF_CRITICALSECTION 24
#else
	#define TT_SIZEOF_CRITICALSECTION 28
#endif // !_XBOX
#endif // _WIN64
	byte m_CriticalSection[TT_SIZEOF_CRITICALSECTION];
#elif _LINUX
	pthread_mutex_t m_Mutex;
	pthread_mutexattr_t m_Attr;
#else
#error
#endif

#ifdef THREAD_MUTEX_TRACING_SUPPORTED
	// Debugging (always here to allow mixed debug/release builds w/o changing size)
	uint	m_currentOwnerID;
	uint16	m_lockCount;
	bool	m_bTrace;
#endif
};

//-----------------------------------------------------------------------------
//
// An alternative mutex that is useful for cases when thread contention is 
// rare, but a mutex is required. Instances should be declared volatile.
// Sleep of 0 may not be sufficient to keep high priority threads from starving 
// lesser threads. This class is not a suitable replacement for a critical
// section if the resource contention is high.
//
//-----------------------------------------------------------------------------

#if defined(_WIN32) && !defined(THREAD_PROFILER)

class CThreadFastMutex
{
public:
	CThreadFastMutex()
	  :	m_ownerID( 0 ),
	  	m_depth( 0 )
	{
	}

private:
	FORCEINLINE bool TryLockInline( const uint32 threadId ) volatile
	{
		if ( threadId != m_ownerID && !ThreadInterlockedAssignIf( (volatile long *)&m_ownerID, (long)threadId, 0 ) )
			return false;

		++m_depth;
		return true;
	}

	bool TryLock( const uint32 threadId ) volatile
	{
		return TryLockInline( threadId );
	}

	TT_CLASS void Lock( const uint32 threadId, unsigned nSpinSleepTime ) volatile;

public:
	bool TryLock() volatile
	{
#ifdef _DEBUG
		if ( m_depth == INT_MAX )
			DebuggerBreak();

		if ( m_depth < 0 )
			DebuggerBreak();
#endif
		return TryLockInline( ThreadGetCurrentId() );
	}

#ifndef _DEBUG
	FORCEINLINE 
#endif
	void Lock( unsigned nSpinSleepTime = 0 ) volatile
	{
		const uint32 threadId = ThreadGetCurrentId();

		if ( !TryLockInline( threadId ) )
		{
			ThreadPause();
			Lock( threadId, nSpinSleepTime );
		}
#ifdef _DEBUG
		if ( m_ownerID != ThreadGetCurrentId() )
			DebuggerBreak();

		if ( m_depth == INT_MAX )
			DebuggerBreak();

		if ( m_depth < 0 )
			DebuggerBreak();
#endif
	}

#ifndef _DEBUG
	FORCEINLINE 
#endif
	void Unlock() volatile
	{
#ifdef _DEBUG
		if ( m_ownerID != ThreadGetCurrentId() )
			DebuggerBreak();

		if ( m_depth <= 0 )
			DebuggerBreak();
#endif

		--m_depth;
		if ( !m_depth )
			ThreadInterlockedExchange( &m_ownerID, 0 );
	}

	bool TryLock() const volatile							{ return (const_cast<CThreadFastMutex *>(this))->TryLock(); }
	void Lock(unsigned nSpinSleepTime = 1 ) const volatile	{ (const_cast<CThreadFastMutex *>(this))->Lock( nSpinSleepTime ); }
	void Unlock() const	volatile							{ (const_cast<CThreadFastMutex *>(this))->Unlock(); }

	// To match regular CThreadMutex:
	bool AssertOwnedByCurrentThread()	{ return true; }
	void SetTrace( bool )				{}

	uint32 GetOwnerId() const			{ return m_ownerID;	}
	int	GetDepth() const				{ return m_depth; }
private:
	volatile uint32	m_ownerID;
	int				m_depth;
};

class ALIGN128 CAlignedThreadFastMutex : public CThreadFastMutex
{
public:
	CAlignedThreadFastMutex()
	{
		Assert( (size_t)this % 128 == 0 && sizeof(*this) == 128 );
	}

private:
	uint8 pad[128-sizeof(CThreadFastMutex)];
};

#else
typedef CThreadMutex CThreadFastMutex;
#endif

//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------

class CThreadNullMutex
{
public:
	static void Lock()				{}
	static void Unlock()			{}

	static bool TryLock()			{ return true; }
	static bool AssertOwnedByCurrentThread() { return true; }
	static void SetTrace( bool b )	{}

	static uint32 GetOwnerId() 		{ return 0;	}
	static int	GetDepth() 			{ return 0; }
};

//-----------------------------------------------------------------------------
//
// A mutex decorator class used to control the use of a mutex, to make it
// less expensive when not multithreading
//
//-----------------------------------------------------------------------------

template <class BaseClass, bool *pCondition>
class CThreadConditionalMutex : public BaseClass
{
public:
	void Lock()				{ if ( *pCondition ) BaseClass::Lock(); }
	void Lock() const 		{ if ( *pCondition ) BaseClass::Lock(); }
	void Unlock()			{ if ( *pCondition ) BaseClass::Unlock(); }
	void Unlock() const		{ if ( *pCondition ) BaseClass::Unlock(); }

	bool TryLock()			{ if ( *pCondition ) return BaseClass::TryLock(); else return true; }
	bool TryLock() const 	{ if ( *pCondition ) return BaseClass::TryLock(); else return true; }
	bool AssertOwnedByCurrentThread() { if ( *pCondition ) return BaseClass::AssertOwnedByCurrentThread(); else return true; }
	void SetTrace( bool b ) { if ( *pCondition ) BaseClass::SetTrace( b ); }
};

//-----------------------------------------------------------------------------
// Mutex decorator that blows up if another thread enters
//-----------------------------------------------------------------------------

template <class BaseClass>
class CThreadTerminalMutex : public BaseClass
{
public:
	bool TryLock()			{ if ( !BaseClass::TryLock() ) { DebuggerBreak(); return false; } return true; }
	bool TryLock() const 	{ if ( !BaseClass::TryLock() ) { DebuggerBreak(); return false; } return true; }
	void Lock()				{ if ( !TryLock() ) BaseClass::Lock(); }
	void Lock() const 		{ if ( !TryLock() ) BaseClass::Lock(); }

};

//-----------------------------------------------------------------------------
//
// Class to Lock a critical section, and unlock it automatically
// when the lock goes out of scope
//
//-----------------------------------------------------------------------------

template <class MUTEX_TYPE = CThreadMutex>
class CAutoLockT
{
public:
	FORCEINLINE CAutoLockT( MUTEX_TYPE &lock)
		: m_lock(lock)
	{
		m_lock.Lock();
	}

	FORCEINLINE CAutoLockT(const MUTEX_TYPE &lock)
		: m_lock(const_cast<MUTEX_TYPE &>(lock))
	{
		m_lock.Lock();
	}

	FORCEINLINE ~CAutoLockT()
	{
		m_lock.Unlock();
	}


private:
	MUTEX_TYPE &m_lock;

	// Disallow copying
	CAutoLockT<MUTEX_TYPE>( const CAutoLockT<MUTEX_TYPE> & );
	CAutoLockT<MUTEX_TYPE> &operator=( const CAutoLockT<MUTEX_TYPE> & );
};

typedef CAutoLockT<CThreadMutex> CAutoLock;

//---------------------------------------------------------

template <int size>	struct CAutoLockTypeDeducer {};
template <> struct CAutoLockTypeDeducer<sizeof(CThreadMutex)> {	typedef CThreadMutex Type_t; };
template <> struct CAutoLockTypeDeducer<sizeof(CThreadNullMutex)> {	typedef CThreadNullMutex Type_t; };
#if defined(_WIN32) && !defined(THREAD_PROFILER)
template <> struct CAutoLockTypeDeducer<sizeof(CThreadFastMutex)> {	typedef CThreadFastMutex Type_t; };
template <> struct CAutoLockTypeDeducer<sizeof(CAlignedThreadFastMutex)> {	typedef CAlignedThreadFastMutex Type_t; };
#endif

#define AUTO_LOCK_( type, mutex ) \
	CAutoLockT< type > UNIQUE_ID( static_cast<const type &>( mutex ) )

#define AUTO_LOCK( mutex ) \
	AUTO_LOCK_( CAutoLockTypeDeducer<sizeof(mutex)>::Type_t, mutex )


#define AUTO_LOCK_FM( mutex ) \
	AUTO_LOCK_( CThreadFastMutex, mutex )

#define LOCAL_THREAD_LOCK_( tag ) \
	; \
	static CThreadFastMutex autoMutex_##tag; \
	AUTO_LOCK( autoMutex_##tag )

#define LOCAL_THREAD_LOCK() \
	LOCAL_THREAD_LOCK_(_)

//-----------------------------------------------------------------------------
//
// Base class for event, semaphore and mutex objects.
//
//-----------------------------------------------------------------------------

class TT_CLASS CThreadSyncObject
{
public:
	~CThreadSyncObject();

	//-----------------------------------------------------
	// Query if object is useful
	//-----------------------------------------------------
	bool operator!() const;

	//-----------------------------------------------------
	// Access handle
	//-----------------------------------------------------
#ifdef _WIN32
	operator HANDLE() { return m_hSyncObject; }
#endif
	//-----------------------------------------------------
	// Wait for a signal from the object
	//-----------------------------------------------------
	bool Wait( uint32 dwTimeout = TT_INFINITE );

protected:
	CThreadSyncObject();
	void AssertUseable();

#ifdef _WIN32
	HANDLE m_hSyncObject;
#elif _LINUX
	pthread_mutex_t	m_Mutex;
	pthread_cond_t	m_Condition;
	bool m_bInitalized;
	int m_cSet;
	bool m_bManualReset;
#else
#error "Implement me"
#endif

private:
	CThreadSyncObject( const CThreadSyncObject & );
	CThreadSyncObject &operator=( const CThreadSyncObject & );
};


//-----------------------------------------------------------------------------
//
// Wrapper for unnamed event objects
//
//-----------------------------------------------------------------------------

#if defined( _WIN32 )

//-----------------------------------------------------------------------------
//
// CThreadSemaphore
//
//-----------------------------------------------------------------------------

class TT_CLASS CThreadSemaphore : public CThreadSyncObject
{
public:
	CThreadSemaphore(long initialValue, long maxValue);

	//-----------------------------------------------------
	// Increases the count of the semaphore object by a specified
	// amount.  Wait() decreases the count by one on return.
	//-----------------------------------------------------
	bool Release(long releaseCount = 1, long * pPreviousCount = NULL );

private:
	CThreadSemaphore(const CThreadSemaphore &);
	CThreadSemaphore &operator=(const CThreadSemaphore &);
};


//-----------------------------------------------------------------------------
//
// A mutex suitable for out-of-process, multi-processor usage
//
//-----------------------------------------------------------------------------

class TT_CLASS CThreadFullMutex : public CThreadSyncObject
{
public:
	CThreadFullMutex( bool bEstablishInitialOwnership = false, const char * pszName = NULL );

	//-----------------------------------------------------
	// Release ownership of the mutex
	//-----------------------------------------------------
	bool Release();

	// To match regular CThreadMutex:
	void Lock()							{ Wait(); }
	void Lock( unsigned timeout )		{ Wait( timeout ); }
	void Unlock()						{ Release(); }
	bool AssertOwnedByCurrentThread()	{ return true; }
	void SetTrace( bool )				{}

private:
	CThreadFullMutex( const CThreadFullMutex & );
	CThreadFullMutex &operator=( const CThreadFullMutex & );
};
#endif


class TT_CLASS CThreadEvent : public CThreadSyncObject
{
public:
	CThreadEvent( bool fManualReset = false );

	//-----------------------------------------------------
	// Set the state to signaled
	//-----------------------------------------------------
	bool Set();

	//-----------------------------------------------------
	// Set the state to nonsignaled
	//-----------------------------------------------------
	bool Reset();

	//-----------------------------------------------------
	// Check if the event is signaled
	//-----------------------------------------------------
	bool Check();

	bool Wait( uint32 dwTimeout = TT_INFINITE );

private:
	CThreadEvent( const CThreadEvent & );
	CThreadEvent &operator=( const CThreadEvent & );
#ifdef _LINUX
	CInterlockedInt m_cSet;
#endif
};

// Hard-wired manual event for use in array declarations
class CThreadManualEvent : public CThreadEvent
{
public:
	CThreadManualEvent()
	 :	CThreadEvent( true )
	{
	}
};

inline int ThreadWaitForEvents( int nEvents, const CThreadEvent *pEvents, bool bWaitAll = true, unsigned timeout = TT_INFINITE )
{
#ifdef _LINUX
  Assert(0);
  return 0;
#else
  return ThreadWaitForObjects( nEvents, (const HANDLE *)pEvents, bWaitAll, timeout );
#endif
}

//-----------------------------------------------------------------------------
//
// CThreadRWLock
//
//-----------------------------------------------------------------------------

class TT_CLASS CThreadRWLock
{
public:
	CThreadRWLock();

	void LockForRead();
	void UnlockRead();
	void LockForWrite();
	void UnlockWrite();

	void LockForRead() const { const_cast<CThreadRWLock *>(this)->LockForRead(); }
	void UnlockRead() const { const_cast<CThreadRWLock *>(this)->UnlockRead(); }
	void LockForWrite() const { const_cast<CThreadRWLock *>(this)->LockForWrite(); }
	void UnlockWrite() const { const_cast<CThreadRWLock *>(this)->UnlockWrite(); }

private:
	void WaitForRead();

	CThreadFastMutex m_mutex;
	CThreadEvent m_CanWrite;
	CThreadEvent m_CanRead;

	int m_nWriters;
	int m_nActiveReaders;
	int m_nPendingReaders;
};

//-----------------------------------------------------------------------------
//
// CThreadSpinRWLock
//
//-----------------------------------------------------------------------------

#define TFRWL_ALIGN ALIGN8

TFRWL_ALIGN 
class TT_CLASS CThreadSpinRWLock
{
public:
	CThreadSpinRWLock()	{ COMPILE_TIME_ASSERT( sizeof( LockInfo_t ) == sizeof( int64 ) ); Assert( (int)this % 8 == 0 ); memset( this, 0, sizeof( *this ) ); }

	bool TryLockForWrite();
	bool TryLockForRead();

	void LockForRead();
	void UnlockRead();
	void LockForWrite();
	void UnlockWrite();

	bool TryLockForWrite() const { return const_cast<CThreadSpinRWLock *>(this)->TryLockForWrite(); }
	bool TryLockForRead() const { return const_cast<CThreadSpinRWLock *>(this)->TryLockForRead(); }
	void LockForRead() const { const_cast<CThreadSpinRWLock *>(this)->LockForRead(); }
	void UnlockRead() const { const_cast<CThreadSpinRWLock *>(this)->UnlockRead(); }
	void LockForWrite() const { const_cast<CThreadSpinRWLock *>(this)->LockForWrite(); }
	void UnlockWrite() const { const_cast<CThreadSpinRWLock *>(this)->UnlockWrite(); }

private:
	struct LockInfo_t
	{
		uint32	m_writerId;
		int		m_nReaders;
	};

	bool AssignIf( const LockInfo_t &newValue, const LockInfo_t &comperand );
	bool TryLockForWrite( const uint32 threadId );
	void SpinLockForWrite( const uint32 threadId );

	volatile LockInfo_t m_lockInfo;
	CInterlockedInt m_nWriters;
};

//-----------------------------------------------------------------------------
//
// A thread wrapper similar to a Java thread.
//
//-----------------------------------------------------------------------------

class TT_CLASS CThread
{
public:
	CThread();
	virtual ~CThread();

	//-----------------------------------------------------

	const char *GetName();
	void SetName( const char * );

	size_t CalcStackDepth( void *pStackVariable )		{ return ((byte *)m_pStackBase - (byte *)pStackVariable); }

	//-----------------------------------------------------
	// Functions for the other threads
	//-----------------------------------------------------

	// Start thread running  - error if already running
	virtual bool Start( unsigned nBytesStack = 0 );

	// Returns true if thread has been created and hasn't yet exited
	bool IsAlive();

	// This method causes the current thread to wait until this thread
	// is no longer alive.
	bool Join( unsigned timeout = TT_INFINITE );

#ifdef _WIN32
	// Access the thread handle directly
	HANDLE GetThreadHandle();
	uint GetThreadId();
#endif

	//-----------------------------------------------------

	int GetResult();

	//-----------------------------------------------------
	// Functions for both this, and maybe, and other threads
	//-----------------------------------------------------

	// Forcibly, abnormally, but relatively cleanly stop the thread
	void Stop( int exitCode = 0 );

	// Get the priority
	int GetPriority() const;

	// Set the priority
	bool SetPriority( int );

	// Suspend a thread
	unsigned Suspend();

	// Resume a suspended thread
	unsigned Resume();

	// Force hard-termination of thread.  Used for critical failures.
	bool Terminate( int exitCode = 0 );

	//-----------------------------------------------------
	// Global methods
	//-----------------------------------------------------

	// Get the Thread object that represents the current thread, if any.
	// Can return NULL if the current thread was not created using
	// CThread
	static CThread *GetCurrentCThread();

	// Offer a context switch. Under Win32, equivalent to Sleep(0)
#ifdef Yield
#undef Yield
#endif
	static void Yield();

	// This method causes the current thread to yield and not to be
	// scheduled for further execution until a certain amount of real
	// time has elapsed, more or less.
	static void Sleep( unsigned duration );

protected:

	// Optional pre-run call, with ability to fail-create. Note Init()
	// is forced synchronous with Start()
	virtual bool Init();

	// Thread will run this function on startup, must be supplied by
	// derived class, performs the intended action of the thread.
	virtual int Run() = 0;

	// Called when the thread exits
	virtual void OnExit();

#ifdef _WIN32
	// Allow for custom start waiting
	virtual bool WaitForCreateComplete( CThreadEvent *pEvent );
#endif

	// "Virtual static" facility
	typedef unsigned (__stdcall *ThreadProc_t)( void * );
	virtual ThreadProc_t GetThreadProc();

	CThreadMutex m_Lock;

private:
	enum Flags
	{
		SUPPORT_STOP_PROTOCOL = 1 << 0
	};

	// Thread initially runs this. param is actually 'this'. function
	// just gets this and calls ThreadProc
	struct ThreadInit_t
	{
		CThread *     pThread;
#ifdef _WIN32
		CThreadEvent *pInitCompleteEvent;
#endif
		bool *        pfInitSuccess;
	};

	static unsigned __stdcall ThreadProc( void * pv );

	// make copy constructor and assignment operator inaccessible
	CThread( const CThread & );
	CThread &operator=( const CThread & );

#ifdef _WIN32
	HANDLE 	m_hThread;
	ThreadId_t m_threadId;
#elif _LINUX
	pthread_t m_threadId;
#endif
	int		m_result;
	char	m_szName[32];
	void *	m_pStackBase;
	unsigned m_flags;
};

//-----------------------------------------------------------------------------
// Simple thread class encompasses the notion of a worker thread, handing
// synchronized communication.
//-----------------------------------------------------------------------------

#ifdef _WIN32

// These are internal reserved error results from a call attempt
enum WTCallResult_t
{
	WTCR_FAIL			= -1,
	WTCR_TIMEOUT		= -2,
	WTCR_THREAD_GONE	= -3,
};

class TT_CLASS CWorkerThread : public CThread
{
public:
	CWorkerThread();

	//-----------------------------------------------------
	//
	// Inter-thread communication
	//
	// Calls in either direction take place on the same "channel."
	// Seperate functions are specified to make identities obvious
	//
	//-----------------------------------------------------

	// Master: Signal the thread, and block for a response
	int CallWorker( unsigned, unsigned timeout = TT_INFINITE, bool fBoostWorkerPriorityToMaster = true );

	// Worker: Signal the thread, and block for a response
	int CallMaster( unsigned, unsigned timeout = TT_INFINITE );

	// Wait for the next request
	bool WaitForCall( unsigned dwTimeout, unsigned *pResult = NULL );
	bool WaitForCall( unsigned *pResult = NULL );

	// Is there a request?
	bool PeekCall( unsigned *pParam = NULL );

	// Reply to the request
	void Reply( unsigned );

	// Wait for a reply in the case when CallWorker() with timeout != TT_INFINITE
	int WaitForReply( unsigned timeout = TT_INFINITE );

	// If you want to do WaitForMultipleObjects you'll need to include
	// this handle in your wait list or you won't be responsive
	HANDLE GetCallHandle();

	// Find out what the request was
	unsigned GetCallParam() const;

	// Boost the worker thread to the master thread, if worker thread is lesser, return old priority
	int BoostPriority();

protected:
	typedef uint32 (__stdcall *WaitFunc_t)( uint32 nHandles, const HANDLE*pHandles, int bWaitAll, uint32 timeout );
	int Call( unsigned, unsigned timeout, bool fBoost, WaitFunc_t = NULL );
	int WaitForReply( unsigned timeout, WaitFunc_t );

private:
	CWorkerThread( const CWorkerThread & );
	CWorkerThread &operator=( const CWorkerThread & );

#ifdef _WIN32
	CThreadEvent	m_EventSend;
	CThreadEvent	m_EventComplete;
#endif

	unsigned        m_Param;
	int				m_ReturnVal;
};

#else

typedef CThread CWorkerThread;

#endif

// a unidirectional message queue. A queue of type T. Not especially high speed since each message
// is malloced/freed. Note that if your message class has destructors/constructors, they MUST be
// thread safe!
template<class T> class CMessageQueue
{
	CThreadEvent SignalEvent;								// signals presence of data
	CThreadMutex QueueAccessMutex;

	// the parts protected by the mutex
	struct MsgNode
	{
		MsgNode *Next;
		T Data;
	};

	MsgNode *Head;
	MsgNode *Tail;

public:
	CMessageQueue( void )
	{
		Head = Tail = NULL;
	}

	// check for a message. not 100% reliable - someone could grab the message first
	bool MessageWaiting( void ) 
	{
		return ( Head != NULL );
	}

	void WaitMessage( T *pMsg )
	{
		for(;;)
		{
			while( ! MessageWaiting() )
				SignalEvent.Wait();
			QueueAccessMutex.Lock();
			if (! Head )
			{
				// multiple readers could make this null
				QueueAccessMutex.Unlock();
				continue;
			}
			*( pMsg ) = Head->Data;
			MsgNode *remove_this = Head;
			Head = Head->Next;
			if (! Head)										// if empty, fix tail ptr
				Tail = NULL;
			QueueAccessMutex.Unlock();
			delete remove_this;
			break;
		}
	}

	void QueueMessage( T const &Msg)
	{
		MsgNode *new1=new MsgNode;
		new1->Data=Msg;
		new1->Next=NULL;
		QueueAccessMutex.Lock();
		if ( Tail )
		{
			Tail->Next=new1;
			Tail = new1;
		}
		else
		{
			Head = new1;
			Tail = new1;
		}
		SignalEvent.Set();
		QueueAccessMutex.Unlock();
	}
};


//-----------------------------------------------------------------------------
//
// CThreadMutex. Inlining to reduce overhead and to allow client code
// to decide debug status (tracing)
//
//-----------------------------------------------------------------------------

#ifdef _WIN32
typedef struct _RTL_CRITICAL_SECTION RTL_CRITICAL_SECTION;
typedef RTL_CRITICAL_SECTION CRITICAL_SECTION;

#ifndef _X360
extern "C"
{
	void __declspec(dllimport) __stdcall InitializeCriticalSection(CRITICAL_SECTION *);
	void __declspec(dllimport) __stdcall EnterCriticalSection(CRITICAL_SECTION *);
	void __declspec(dllimport) __stdcall LeaveCriticalSection(CRITICAL_SECTION *);
	void __declspec(dllimport) __stdcall DeleteCriticalSection(CRITICAL_SECTION *);
};
#endif

//---------------------------------------------------------

inline void CThreadMutex::Lock()
{
#ifdef THREAD_MUTEX_TRACING_ENABLED
	uint thisThreadID = ThreadGetCurrentId();
	if ( m_bTrace && m_currentOwnerID && ( m_currentOwnerID != thisThreadID ) )
		Msg( "Thread %u about to wait for lock %x owned by %u\n", ThreadGetCurrentId(), (CRITICAL_SECTION *)&m_CriticalSection, m_currentOwnerID );
#endif

	VCRHook_EnterCriticalSection((CRITICAL_SECTION *)&m_CriticalSection);

#ifdef THREAD_MUTEX_TRACING_ENABLED
	if (m_lockCount == 0)
	{
		// we now own it for the first time.  Set owner information
		m_currentOwnerID = thisThreadID;
		if ( m_bTrace )
			Msg( "Thread %u now owns lock 0x%x\n", m_currentOwnerID, (CRITICAL_SECTION *)&m_CriticalSection );
	}
	m_lockCount++;
#endif
}

//---------------------------------------------------------

inline void CThreadMutex::Unlock()
{
#ifdef THREAD_MUTEX_TRACING_ENABLED
	AssertMsg( m_lockCount >= 1, "Invalid unlock of thread lock" );
	m_lockCount--;
	if (m_lockCount == 0)
	{
		if ( m_bTrace )
			Msg( "Thread %u releasing lock 0x%x\n", m_currentOwnerID, (CRITICAL_SECTION *)&m_CriticalSection );
		m_currentOwnerID = 0;
	}
#endif
	LeaveCriticalSection((CRITICAL_SECTION *)&m_CriticalSection);
}

//---------------------------------------------------------

inline bool CThreadMutex::AssertOwnedByCurrentThread()
{
#ifdef THREAD_MUTEX_TRACING_ENABLED
	if (ThreadGetCurrentId() == m_currentOwnerID)
		return true;
	AssertMsg3( 0, "Expected thread %u as owner of lock 0x%x, but %u owns", ThreadGetCurrentId(), (CRITICAL_SECTION *)&m_CriticalSection, m_currentOwnerID );
	return false;
#else
	return true;
#endif
}

//---------------------------------------------------------

inline void CThreadMutex::SetTrace( bool bTrace )
{
#ifdef THREAD_MUTEX_TRACING_ENABLED
	m_bTrace = bTrace;
#endif
}

//---------------------------------------------------------

#elif _LINUX

inline CThreadMutex::CThreadMutex()
{
	// enable recursive locks as we need them
	pthread_mutexattr_init( &m_Attr );
	pthread_mutexattr_settype( &m_Attr, PTHREAD_MUTEX_RECURSIVE_NP );
	pthread_mutex_init( &m_Mutex, &m_Attr );
}

//---------------------------------------------------------

inline CThreadMutex::~CThreadMutex()
{
	pthread_mutex_destroy( &m_Mutex );
}

//---------------------------------------------------------

inline void CThreadMutex::Lock()
{
	pthread_mutex_lock( &m_Mutex );
}

//---------------------------------------------------------

inline void CThreadMutex::Unlock()
{
	pthread_mutex_unlock( &m_Mutex );
}

//---------------------------------------------------------

inline bool CThreadMutex::AssertOwnedByCurrentThread()
{
	return true;
}

//---------------------------------------------------------

inline void CThreadMutex::SetTrace(bool fTrace)
{
}

#endif // _LINUX

//-----------------------------------------------------------------------------
//
// CThreadRWLock inline functions
//
//-----------------------------------------------------------------------------

inline CThreadRWLock::CThreadRWLock()
:	m_CanRead( true ),
	m_nWriters( 0 ),
	m_nActiveReaders( 0 ),
	m_nPendingReaders( 0 )
{
}

inline void CThreadRWLock::LockForRead()
{
	m_mutex.Lock();
	if ( m_nWriters)
	{
		WaitForRead();
	}
	m_nActiveReaders++;
	m_mutex.Unlock();
}

inline void CThreadRWLock::UnlockRead()
{
	m_mutex.Lock();
	m_nActiveReaders--;
	if ( m_nActiveReaders == 0 && m_nWriters != 0 )
	{
		m_CanWrite.Set();
	}
	m_mutex.Unlock();
}


//-----------------------------------------------------------------------------
//
// CThreadSpinRWLock inline functions
//
//-----------------------------------------------------------------------------

inline bool CThreadSpinRWLock::AssignIf( const LockInfo_t &newValue, const LockInfo_t &comperand )
{
	return ThreadInterlockedAssignIf64( (int64 *)&m_lockInfo, *((int64 *)&newValue), *((int64 *)&comperand) );
}

inline bool CThreadSpinRWLock::TryLockForWrite( const uint32 threadId )
{
	// In order to grab a write lock, there can be no readers and no owners of the write lock
	if ( m_lockInfo.m_nReaders > 0 || ( m_lockInfo.m_writerId && m_lockInfo.m_writerId != threadId ) )
	{
		return false;
	}

	static const LockInfo_t oldValue = { 0, 0 };
	LockInfo_t newValue = { threadId, 0 };
	const bool bSuccess = AssignIf( newValue, oldValue );
#if defined(_X360)
	if ( bSuccess )
	{
		// X360TBD: Serious perf implications. Not Yet. __sync();
	}
#endif
	return bSuccess;
}

inline bool CThreadSpinRWLock::TryLockForWrite()
{
	m_nWriters++;
	if ( !TryLockForWrite( ThreadGetCurrentId() ) )
	{
		m_nWriters--;
		return false;
	}
	return true;
}

inline bool CThreadSpinRWLock::TryLockForRead()
{
	if ( m_nWriters != 0 )
	{
		return false;
	}
	// In order to grab a write lock, the number of readers must not change and no thread can own the write
	LockInfo_t oldValue;
	LockInfo_t newValue;

	oldValue.m_nReaders = m_lockInfo.m_nReaders;
	oldValue.m_writerId = 0;
	newValue.m_nReaders = oldValue.m_nReaders + 1;
	newValue.m_writerId = 0;

	const bool bSuccess = AssignIf( newValue, oldValue );
#if defined(_X360)
	if ( bSuccess )
	{
		// X360TBD: Serious perf implications. Not Yet. __sync();
	}
#endif
	return bSuccess;
}

inline void CThreadSpinRWLock::LockForWrite()
{
	const uint32 threadId = ThreadGetCurrentId();

	m_nWriters++;

	if ( !TryLockForWrite( threadId ) )
	{
		ThreadPause();
		SpinLockForWrite( threadId );
	}
}

//-----------------------------------------------------------------------------

#if defined( _WIN32 )
#pragma warning(pop)
#endif

#endif // THREADTOOLS_H
