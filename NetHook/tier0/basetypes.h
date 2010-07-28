//========= Copyright © 1996-2005, Valve Corporation, All rights reserved. ============//
//
// Purpose: 
//
// $NoKeywords: $
//=============================================================================//

#ifndef BASETYPES_H
#define BASETYPES_H

#include "commonmacros.h"
#include "wchartypes.h"

#include "tier0/valve_off.h"

#ifdef _WIN32
#pragma once
#endif


#include "protected_things.h"

// There's a different version of this file in the xbox codeline
// so the PC version built in the xbox branch includes things like 
// tickrate changes.
#include "xbox_codeline_defines.h"

#ifdef IN_XBOX_CODELINE
#define XBOX_CODELINE_ONLY()
#else
#define XBOX_CODELINE_ONLY() Error_Compiling_Code_Only_Valid_in_Xbox_Codeline
#endif

// stdio.h
#ifndef NULL
#define NULL 0
#endif


#ifdef _LINUX
typedef unsigned int uintptr_t;
#endif

#define ExecuteNTimes( nTimes, x )	\
	{								\
		static int __executeCount=0;\
		if ( __executeCount < nTimes )\
		{							\
			x;						\
			++__executeCount;		\
		}							\
	}


#define ExecuteOnce( x )			ExecuteNTimes( 1, x )


template <typename T>
inline T AlignValue( T val, unsigned alignment )
{
	return (T)( ( (uintptr_t)val + alignment - 1 ) & ~( alignment - 1 ) );
}


// Pad a number so it lies on an N byte boundary.
// So PAD_NUMBER(0,4) is 0 and PAD_NUMBER(1,4) is 4
#define PAD_NUMBER(number, boundary) \
	( ((number) + ((boundary)-1)) / (boundary) ) * (boundary)

// In case this ever changes
#define M_PI			3.14159265358979323846

#include "valve_minmax_on.h"

#if !defined(_X360)
#define fpmin min
#define fpmax max
#endif

#ifdef __cplusplus
	template< class T >
	inline T clamp( T const &val, T const &minVal, T const &maxVal )
	{
		if( val < minVal )
			return minVal;
		else if( val > maxVal )
			return maxVal;
		else
			return val;
	}
#endif

#ifndef FALSE
#define FALSE 0
#define TRUE (!FALSE)
#endif


typedef int BOOL;
typedef int qboolean;
typedef unsigned long ULONG;
typedef unsigned char BYTE;
typedef unsigned char byte;
typedef unsigned short word;

typedef unsigned int uintptr_t;


enum ThreeState_t
{
	TRS_FALSE,
	TRS_TRUE,
	TRS_NONE,
};

typedef float vec_t;


// FIXME: this should move 
#ifndef __cplusplus
#define true TRUE
#define false FALSE
#endif

//-----------------------------------------------------------------------------
// look for NANs, infinities, and underflows. 
// This assumes the ANSI/IEEE 754-1985 standard
//-----------------------------------------------------------------------------

#ifdef __cplusplus

inline unsigned long& FloatBits( vec_t& f )
{
	return *reinterpret_cast<unsigned long*>(&f);
}

inline unsigned long const& FloatBits( vec_t const& f )
{
	return *reinterpret_cast<unsigned long const*>(&f);
}

inline vec_t BitsToFloat( unsigned long i )
{
	return *reinterpret_cast<vec_t*>(&i);
}

inline bool IsFinite( vec_t f )
{
	return ((FloatBits(f) & 0x7F800000) != 0x7F800000);
}

inline unsigned long FloatAbsBits( vec_t f )
{
	return FloatBits(f) & 0x7FFFFFFF;
}

inline float FloatMakeNegative( vec_t f )
{
	return BitsToFloat( FloatBits(f) | 0x80000000 );
}

#if defined( WIN32 )

//#include <math.h>
// Just use prototype from math.h
#ifdef __cplusplus
extern "C" 
{
#endif
	double __cdecl fabs(double);
#ifdef __cplusplus
}
#endif

// In win32 try to use the intrinsic fabs so the optimizer can do it's thing inline in the code
#pragma intrinsic( fabs )
// Also, alias float make positive to use fabs, too
// NOTE:  Is there a perf issue with double<->float conversion?
inline float FloatMakePositive( vec_t f )
{
	return (float)fabs( f );
}
#else
inline float FloatMakePositive( vec_t f )
{
	return BitsToFloat( FloatBits(f) & 0x7FFFFFFF );
}
#endif

inline float FloatNegate( vec_t f )
{
	return BitsToFloat( FloatBits(f) ^ 0x80000000 );
}


#define FLOAT32_NAN_BITS     (unsigned long)0x7FC00000	// not a number!
#define FLOAT32_NAN          BitsToFloat( FLOAT32_NAN_BITS )

#define VEC_T_NAN FLOAT32_NAN

#endif

// FIXME: why are these here?  Hardly anyone actually needs them.
struct color24
{
	byte r, g, b;
};

typedef struct color32_s
{
	bool operator!=( const struct color32_s &other ) const;

	byte r, g, b, a;
} color32;

inline bool color32::operator!=( const color32 &other ) const
{
	return r != other.r || g != other.g || b != other.b || a != other.a;
}

struct colorVec
{
	unsigned r, g, b, a;
};


#ifndef NOTE_UNUSED
#define NOTE_UNUSED(x)	(x = x)	// for pesky compiler / lint warnings
#endif
#ifdef __cplusplus

struct vrect_t
{
	int				x,y,width,height;
	vrect_t			*pnext;
};

#endif


//-----------------------------------------------------------------------------
// MaterialRect_t struct - used for DrawDebugText
//-----------------------------------------------------------------------------
struct Rect_t
{
    int x, y;
	int width, height;
};


//-----------------------------------------------------------------------------
// Interval, used by soundemittersystem + the game
//-----------------------------------------------------------------------------
struct interval_t
{
	float start;
	float range;
};


//-----------------------------------------------------------------------------
// Declares a type-safe handle type; you can't assign one handle to the next
//-----------------------------------------------------------------------------

// 32-bit pointer handles.

// Typesafe 8-bit and 16-bit handles.
template< class HandleType >
class CBaseIntHandle
{
public:
	
	inline bool			operator==( const CBaseIntHandle &other )	{ return m_Handle == other.m_Handle; }
	inline bool			operator!=( const CBaseIntHandle &other )	{ return m_Handle != other.m_Handle; }

	// Only the code that doles out these handles should use these functions.
	// Everyone else should treat them as a transparent type.
	inline HandleType	GetHandleValue()					{ return m_Handle; }
	inline void			SetHandleValue( HandleType val )	{ m_Handle = val; }

	typedef HandleType	HANDLE_TYPE;

protected:

	HandleType	m_Handle;
};

template< class DummyType >
class CIntHandle16 : public CBaseIntHandle< unsigned short >
{
public:
	inline			CIntHandle16() {}

	static inline	CIntHandle16<DummyType> MakeHandle( HANDLE_TYPE val )
	{
		return CIntHandle16<DummyType>( val );
	}

protected:
	inline			CIntHandle16( HANDLE_TYPE val )
	{
		m_Handle = val;
	}
};


template< class DummyType >
class CIntHandle32 : public CBaseIntHandle< unsigned long >
{
public:
	inline			CIntHandle32() {}

	static inline	CIntHandle32<DummyType> MakeHandle( HANDLE_TYPE val )
	{
		return CIntHandle32<DummyType>( val );
	}

protected:
	inline			CIntHandle32( HANDLE_TYPE val )
	{
		m_Handle = val;
	}
};


// NOTE: This macro is the same as windows uses; so don't change the guts of it
#define DECLARE_HANDLE_16BIT(name)	typedef CIntHandle16< struct name##__handle * > name;
#define DECLARE_HANDLE_32BIT(name)	typedef CIntHandle32< struct name##__handle * > name;

#define DECLARE_POINTER_HANDLE(name) struct name##__ { int unused; }; typedef struct name##__ *name
#define FORWARD_DECLARE_HANDLE(name) typedef struct name##__ *name

// @TODO: Find a better home for this
#if !defined(_STATIC_LINKED) && !defined(PUBLISH_DLL_SUBSYSTEM)
// for platforms built with dynamic linking, the dll interface does not need spoofing
#define PUBLISH_DLL_SUBSYSTEM()
#endif

#define UID_PREFIX generated_id_
#define UID_CAT1(a,c) a ## c
#define UID_CAT2(a,c) UID_CAT1(a,c)
#define EXPAND_CONCAT(a,c) UID_CAT1(a,c)
#ifdef _MSC_VER
#define UNIQUE_ID UID_CAT2(UID_PREFIX,__COUNTER__)
#else
#define UNIQUE_ID UID_CAT2(UID_PREFIX,__LINE__)
#endif

// this allows enumerations to be used as flags, and still remain type-safe!
#define DEFINE_ENUM_BITWISE_OPERATORS( Type ) \
	inline Type  operator|  ( Type  a, Type b ) { return Type( int( a ) | int( b ) ); } \
	inline Type  operator&  ( Type  a, Type b ) { return Type( int( a ) & int( b ) ); } \
	inline Type  operator^  ( Type  a, Type b ) { return Type( int( a ) ^ int( b ) ); } \
	inline Type  operator<< ( Type  a, int  b ) { return Type( int( a ) << b ); } \
	inline Type  operator>> ( Type  a, int  b ) { return Type( int( a ) >> b ); } \
	inline Type &operator|= ( Type &a, Type b ) { return a = a |  b; } \
	inline Type &operator&= ( Type &a, Type b ) { return a = a &  b; } \
	inline Type &operator^= ( Type &a, Type b ) { return a = a ^  b; } \
	inline Type &operator<<=( Type &a, int  b ) { return a = a << b; } \
	inline Type &operator>>=( Type &a, int  b ) { return a = a >> b; } \
	inline Type  operator~( Type a ) { return Type( ~int( a ) ); }

// defines increment/decrement operators for enums for easy iteration
#define DEFINE_ENUM_INCREMENT_OPERATORS( Type ) \
	inline Type &operator++( Type &a      ) { return a = Type( int( a ) + 1 ); } \
	inline Type &operator--( Type &a      ) { return a = Type( int( a ) - 1 ); } \
	inline Type  operator++( Type &a, int ) { Type t = a; ++a; return t; } \
	inline Type  operator--( Type &a, int ) { Type t = a; --a; return t; }

#include "tier0/valve_on.h"

#endif // BASETYPES_H
