//========= Copyright © 1996-2005, Valve Corporation, All rights reserved. ============//
//
// Purpose: 
//
// $NoKeywords: $
//=============================================================================//

#ifndef FASTTIMER_H
#define FASTTIMER_H
#ifdef _WIN32
#pragma once
#endif

#include <assert.h>
#include "tier0/platform.h"

PLATFORM_INTERFACE int64 g_ClockSpeed;
PLATFORM_INTERFACE unsigned long g_dwClockSpeed;
#if defined( _X360 ) && defined( _CERT )
PLATFORM_INTERFACE unsigned long g_dwFakeFastCounter;
#endif

PLATFORM_INTERFACE double g_ClockSpeedMicrosecondsMultiplier;
PLATFORM_INTERFACE double g_ClockSpeedMillisecondsMultiplier;
PLATFORM_INTERFACE double g_ClockSpeedSecondsMultiplier;

class CCycleCount
{
friend class CFastTimer;

public:
					CCycleCount();
					CCycleCount( int64 cycles );

	void			Sample();	// Sample the clock. This takes about 34 clocks to execute (or 26,000 calls per millisecond on a P900).

	void			Init();		// Set to zero.
	void			Init( float initTimeMsec );
	void			Init( double initTimeMsec )		{ Init( (float)initTimeMsec ); }
	void			Init( int64 cycles );
	bool			IsLessThan( CCycleCount const &other ) const;					// Compare two counts.

	// Convert to other time representations. These functions are slow, so it's preferable to call them
	// during display rather than inside a timing block.
	unsigned long	GetCycles()  const;
	int64			GetLongCycles() const;

	unsigned long	GetMicroseconds() const;
	uint64			GetUlMicroseconds() const;
	double			GetMicrosecondsF() const; 	
	void			SetMicroseconds( unsigned long nMicroseconds );

	unsigned long	GetMilliseconds() const;
	double			GetMillisecondsF() const;

	double			GetSeconds() const;

	CCycleCount&	operator+=( CCycleCount const &other );

	// dest = rSrc1 + rSrc2
	static void		Add( CCycleCount const &rSrc1, CCycleCount const &rSrc2, CCycleCount &dest );	// Add two samples together.
	
	// dest = rSrc1 - rSrc2
	static void		Sub( CCycleCount const &rSrc1, CCycleCount const &rSrc2, CCycleCount &dest );	// Add two samples together.

	static int64	GetTimestamp();

	int64			m_Int64;
};

class CClockSpeedInit
{
public:
	CClockSpeedInit()
	{
		Init();
	}

	static void Init()
	{
#if defined( _X360 ) && !defined( _CERT )
		PMCStart();
		PMCInitIntervalTimer( 0 );
#endif
		const CPUInformation& pi = GetCPUInformation();

		g_ClockSpeed = pi.m_Speed;
		g_dwClockSpeed = (unsigned long)g_ClockSpeed;

		g_ClockSpeedMicrosecondsMultiplier = 1000000.0 / (double)g_ClockSpeed;
		g_ClockSpeedMillisecondsMultiplier = 1000.0 / (double)g_ClockSpeed;
		g_ClockSpeedSecondsMultiplier = 1.0f / (double)g_ClockSpeed;
	}
};

class CFastTimer
{
public:
	// These functions are fast to call and should be called from your sampling code.
	void				Start();
	void				End();

	const CCycleCount &	GetDuration() const;	// Get the elapsed time between Start and End calls.
	CCycleCount 		GetDurationInProgress() const; // Call without ending. Not that cheap.

	// Return number of cycles per second on this processor.
	static inline unsigned long	GetClockSpeed();

private:
	CCycleCount	m_Duration;
#ifdef DEBUG_FASTTIMER
	bool m_bRunning;		// Are we currently running?
#endif
};


// This is a helper class that times whatever block of code it's in
class CTimeScope
{
public:
				CTimeScope( CFastTimer *pTimer );
				~CTimeScope();

private:	
	CFastTimer	*m_pTimer;
};

inline CTimeScope::CTimeScope( CFastTimer *pTotal )
{
	m_pTimer = pTotal;
	m_pTimer->Start();
}

inline CTimeScope::~CTimeScope()
{
	m_pTimer->End();
}

// This is a helper class that times whatever block of code it's in and
// adds the total (int microseconds) to a global counter.
class CTimeAdder
{
public:
				CTimeAdder( CCycleCount *pTotal );
				~CTimeAdder();

	void		End();

private:	
	CCycleCount	*m_pTotal;
	CFastTimer	m_Timer;
};

inline CTimeAdder::CTimeAdder( CCycleCount *pTotal )
{
	m_pTotal = pTotal;
	m_Timer.Start();
}

inline CTimeAdder::~CTimeAdder()
{
	End();
}

inline void CTimeAdder::End()
{
	if( m_pTotal )
	{
		m_Timer.End();
		*m_pTotal += m_Timer.GetDuration();
		m_pTotal = 0;
	}
}



// -------------------------------------------------------------------------- // 
// Simple tool to support timing a block of code, and reporting the results on
// program exit or at each iteration
//
//	Macros used because dbg.h uses this header, thus Msg() is unavailable
// -------------------------------------------------------------------------- // 

#define PROFILE_SCOPE(name) \
	class C##name##ACC : public CAverageCycleCounter \
	{ \
	public: \
		~C##name##ACC() \
		{ \
			Msg("%-48s: %6.3f avg (%8.1f total, %7.3f peak, %5d iters)\n",  \
				#name, \
				GetAverageMilliseconds(), \
				GetTotalMilliseconds(), \
				GetPeakMilliseconds(), \
				GetIters() ); \
		} \
	}; \
	static C##name##ACC name##_ACC; \
	CAverageTimeMarker name##_ATM( &name##_ACC )

#define TIME_SCOPE(name) \
	class CTimeScopeMsg_##name \
	{ \
	public: \
		CTimeScopeMsg_##name() { m_Timer.Start(); } \
		~CTimeScopeMsg_##name() \
		{ \
			m_Timer.End(); \
			Msg( #name "time: %.4fms\n", m_Timer.GetDuration().GetMillisecondsF() ); \
		} \
	private:	\
		CFastTimer	m_Timer; \
	} name##_TSM;


// -------------------------------------------------------------------------- // 

class CAverageCycleCounter
{
public:
	CAverageCycleCounter();
	
	void Init();
	void MarkIter( const CCycleCount &duration );
	
	unsigned GetIters() const;
	
	double GetAverageMilliseconds() const;
	double GetTotalMilliseconds() const;
	double GetPeakMilliseconds() const;

private:
	unsigned	m_nIters;
	CCycleCount m_Total;
	CCycleCount	m_Peak;
	bool		m_fReport;
	const tchar *m_pszName;
};

// -------------------------------------------------------------------------- // 

class CAverageTimeMarker
{
public:
	CAverageTimeMarker( CAverageCycleCounter *pCounter );
	~CAverageTimeMarker();
	
private:
	CAverageCycleCounter *m_pCounter;
	CFastTimer	m_Timer;
};


// -------------------------------------------------------------------------- // 
// CCycleCount inlines.
// -------------------------------------------------------------------------- // 

inline CCycleCount::CCycleCount()
{
	Init( (int64)0 );
}

inline CCycleCount::CCycleCount( int64 cycles )
{
	Init( cycles );
}

inline void CCycleCount::Init()
{
	Init( (int64)0 );
}

inline void CCycleCount::Init( float initTimeMsec )
{
	if ( g_ClockSpeedMillisecondsMultiplier > 0 )
		Init( (int64)(initTimeMsec / g_ClockSpeedMillisecondsMultiplier) );
	else
		Init( (int64)0 );
}

inline void CCycleCount::Init( int64 cycles )
{
	m_Int64 = cycles;
}

#pragma warning(push)
#pragma warning(disable : 4189) // warning C4189: local variable is initialized but not referenced

inline void CCycleCount::Sample()
{
#if defined( _X360 )
#if !defined( _CERT )
	// read the highest resolution timer directly (ticks at native 3.2GHz), bypassing any calls into PMC
	// can only resolve 32 bits, rollover is ~1.32 secs
	// based on PMCGetIntervalTimer() from the April 2007 XDK
	int64 temp;
	__asm 
	{
		lis		r11,08FFFh
		ld		r11,011E0h(r11)
		rldicl	r11,r11,32,32
		// unforunate can't get the inline assembler to write directly into desired target
		std		r11,temp
	}
	m_Int64 = temp;
#else
	m_Int64 = ++g_dwFakeFastCounter;
#endif
#elif defined( _WIN32 )
	unsigned long* pSample = (unsigned long *)&m_Int64;
	__asm
	{
		// force the cpu to synchronize the instruction queue
		// NJS: CPUID can really impact performance in tight loops.
		//cpuid
		//cpuid
		//cpuid
		mov		ecx, pSample
		rdtsc
		mov		[ecx], eax
		mov		[ecx+4], edx
	}
#elif defined( _LINUX )
	unsigned long* pSample = (unsigned long *)&m_Int64;
    __asm__ __volatile__ (  
		"rdtsc\n\t"
		"movl %%eax,  (%0)\n\t"
		"movl %%edx, 4(%0)\n\t"
		: /* no output regs */
		: "D" (pSample)
		: "%eax", "%edx" );
#endif
}

#pragma warning(pop)


inline CCycleCount& CCycleCount::operator+=( CCycleCount const &other )
{
	m_Int64 += other.m_Int64;
	return *this;
}


inline void CCycleCount::Add( CCycleCount const &rSrc1, CCycleCount const &rSrc2, CCycleCount &dest )
{
	dest.m_Int64 = rSrc1.m_Int64 + rSrc2.m_Int64;
}

inline void CCycleCount::Sub( CCycleCount const &rSrc1, CCycleCount const &rSrc2, CCycleCount &dest )
{
	dest.m_Int64 = rSrc1.m_Int64 - rSrc2.m_Int64;
}

inline int64 CCycleCount::GetTimestamp()
{
	CCycleCount c;
	c.Sample();
	return c.GetLongCycles();
}

inline bool CCycleCount::IsLessThan(CCycleCount const &other) const
{
	return m_Int64 < other.m_Int64;
}


inline unsigned long CCycleCount::GetCycles() const
{
	return (unsigned long)m_Int64;
}

inline int64 CCycleCount::GetLongCycles() const
{
	return m_Int64;
}

inline unsigned long CCycleCount::GetMicroseconds() const
{
	return (unsigned long)((m_Int64 * 1000000) / g_ClockSpeed);
}

inline uint64 CCycleCount::GetUlMicroseconds() const
{
	return ((m_Int64 * 1000000) / g_ClockSpeed);
}


inline double CCycleCount::GetMicrosecondsF() const
{
	return (double)( m_Int64 * g_ClockSpeedMicrosecondsMultiplier );
}


inline void	CCycleCount::SetMicroseconds( unsigned long nMicroseconds )
{
	m_Int64 = ((int64)nMicroseconds * g_ClockSpeed) / 1000000;
}


inline unsigned long CCycleCount::GetMilliseconds() const
{
	return (unsigned long)((m_Int64 * 1000) / g_ClockSpeed);
}


inline double CCycleCount::GetMillisecondsF() const
{
	return (double)( m_Int64 * g_ClockSpeedMillisecondsMultiplier );
}


inline double CCycleCount::GetSeconds() const
{
	return (double)( m_Int64 * g_ClockSpeedSecondsMultiplier );
}


// -------------------------------------------------------------------------- // 
// CFastTimer inlines.
// -------------------------------------------------------------------------- // 
inline void CFastTimer::Start()
{
	m_Duration.Sample();
#ifdef DEBUG_FASTTIMER
	m_bRunning = true;
#endif
}


inline void CFastTimer::End()
{
	CCycleCount cnt;
	cnt.Sample();
	if ( IsX360() )
	{
		// have to handle rollover, hires timer is only accurate to 32 bits
		// more than one overflow should not have occured, otherwise caller should use a slower timer
		if ( (uint64)cnt.m_Int64 <= (uint64)m_Duration.m_Int64 )
		{
			// rollover occured	
			cnt.m_Int64 += 0x100000000LL;	
		}
	}

	m_Duration.m_Int64 = cnt.m_Int64 - m_Duration.m_Int64;

#ifdef DEBUG_FASTTIMER
	m_bRunning = false;
#endif
}

inline CCycleCount CFastTimer::GetDurationInProgress() const
{
	CCycleCount cnt;
	cnt.Sample();
	if ( IsX360() )
	{
		// have to handle rollover, hires timer is only accurate to 32 bits
		// more than one overflow should not have occured, otherwise caller should use a slower timer
		if ( (uint64)cnt.m_Int64 <= (uint64)m_Duration.m_Int64 )
		{
			// rollover occured	
			cnt.m_Int64 += 0x100000000LL;	
		}
	}

	CCycleCount result;
	result.m_Int64 = cnt.m_Int64 - m_Duration.m_Int64;
	
	return result;
}


inline unsigned long CFastTimer::GetClockSpeed()
{
	return g_dwClockSpeed;
}


inline CCycleCount const& CFastTimer::GetDuration() const
{
#ifdef DEBUG_FASTTIMER
	assert( !m_bRunning );
#endif
	return m_Duration;
}


// -------------------------------------------------------------------------- // 
// CAverageCycleCounter inlines

inline CAverageCycleCounter::CAverageCycleCounter()
 :	m_nIters( 0 )
{
}

inline void CAverageCycleCounter::Init()
{
	m_Total.Init();
	m_Peak.Init();
	m_nIters = 0;
}

inline void CAverageCycleCounter::MarkIter( const CCycleCount &duration )
{
	++m_nIters;
	m_Total += duration;
	if ( m_Peak.IsLessThan( duration ) )
		m_Peak = duration;
}

inline unsigned CAverageCycleCounter::GetIters() const
{
	return m_nIters;
}

inline double CAverageCycleCounter::GetAverageMilliseconds() const
{
	if ( m_nIters )
		return (m_Total.GetMillisecondsF() / (double)m_nIters);
	else
		return 0;
}

inline double CAverageCycleCounter::GetTotalMilliseconds() const
{
	return m_Total.GetMillisecondsF();
}

inline double CAverageCycleCounter::GetPeakMilliseconds() const
{
	return m_Peak.GetMillisecondsF();
}

// -------------------------------------------------------------------------- // 

inline CAverageTimeMarker::CAverageTimeMarker( CAverageCycleCounter *pCounter )
{
	m_pCounter = pCounter;
	m_Timer.Start();
}

inline CAverageTimeMarker::~CAverageTimeMarker()
{
	m_Timer.End();
	m_pCounter->MarkIter( m_Timer.GetDuration() );
}


// CLimitTimer
// Use this to time whether a desired interval of time has passed.  It's extremely fast
// to check while running.
class CLimitTimer
{
public:
	void SetLimit( uint64 m_cMicroSecDuration );
	bool BLimitReached( void );

private:
	int64 m_lCycleLimit;
};


//-----------------------------------------------------------------------------
// Purpose: Initializes the limit timer with a period of time to measure.
// Input  : cMicroSecDuration -		How long a time period to measure
//-----------------------------------------------------------------------------
inline void CLimitTimer::SetLimit( uint64 m_cMicroSecDuration )
{
	int64 dlCycles = ( ( uint64 ) m_cMicroSecDuration * ( int64 ) g_dwClockSpeed ) / ( int64 ) 1000000L;
	CCycleCount cycleCount;
	cycleCount.Sample( );
	m_lCycleLimit = cycleCount.GetLongCycles( ) + dlCycles;
}


//-----------------------------------------------------------------------------
// Purpose: Determines whether our specified time period has passed
// Output:	true if at least the specified time period has passed
//-----------------------------------------------------------------------------
inline bool CLimitTimer::BLimitReached( )
{
	CCycleCount cycleCount;
	cycleCount.Sample( );
	return ( cycleCount.GetLongCycles( ) >= m_lCycleLimit );
}



#endif // FASTTIMER_H
