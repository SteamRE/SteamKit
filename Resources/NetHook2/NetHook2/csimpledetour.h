
#ifndef CSIMPLEDETOUR_H_
#define CSIMPLEDETOUR_H_
#ifdef _WIN32
#pragma once
#endif


#include "detours.h"


class CSimpleDetour
{

public:
	CSimpleDetour( void **old, void *replacement ) noexcept;

	void Attach() noexcept;
	void Detach() noexcept;

private:
	void **m_fnOld;
	void *m_fnReplacement;

	bool m_bAttached;

};


#define SETUP_SIMPLE_DETOUR(name, old, replacement) \
	CSimpleDetour name(&(void * &)old, (void *)(&(void * &)replacement))


#define SETUP_DETOUR_FUNCTION( ret, conv, name, params ) \
	ret conv name##_H params; \
	ret ( conv *name##_T ) params = name; \
	CSimpleDetour *Detour_##name = new CSimpleDetour( &(void * &)name##_T, (void *)(&(void * &)name##_H) ); \
	ret conv name##_H params

#define SETUP_DETOUR_FUNCTION_LATE( ret, conv, name, params ) \
	ret conv name##_H params; \
	ret ( conv *name##_T ) params = NULL; \
	CSimpleDetour *Detour_##name = NULL; \
	ret conv name##_H params

#define SETUP_DETOUR_LATE( name ) \
	Detour_##name = new CSimpleDetour( &(void * &)name##_T, (void *)(&(void * &)name##_H) )

#define SETUP_DETOUR_EXTERN( ret, conv, name, params ) \
	extern ret ( conv *name##_T ) params; \
	extern CSimpleDetour *Detour_##name

#define SETUP_DETOUR_TRAMP( ret, conv, name, params ) \
	ret ( conv *name##_T ) params = NULL; \

#endif // !CSIMPLEDETOUR_H_
