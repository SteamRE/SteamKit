
#ifndef WSHOOKS_H_
#define WSHOOKS_H_
#ifdef _WIN32
#pragma once
#endif


#include <winsock2.h>
#include <windows.h>

#include "csimpledetour.h"


SETUP_DETOUR_EXTERN( int, PASCAL, WSAStartup, ( WORD wVersionRequired, LPWSADATA lpWSAData ) );

// udp
SETUP_DETOUR_EXTERN( int, PASCAL, recvfrom, ( SOCKET s, char *buf, int len, int flags, sockaddr *from, int *fromlen ) );
SETUP_DETOUR_EXTERN( int, WSAAPI, WSASendTo, ( SOCKET s, LPWSABUF lpBuffers, DWORD dwBufferCount, LPDWORD lpNumberOfBytesSent, DWORD dwFlags, const sockaddr *lpTo, int iTolen, LPWSAOVERLAPPED lpOverLapped, LPWSAOVERLAPPED_COMPLETION_ROUTINE lpCompletionRoutine ) );

// tcp
SETUP_DETOUR_EXTERN( int, WSAAPI, WSARecv, ( SOCKET s, LPWSABUF lpBuffers, DWORD dwBufferCount, LPDWORD lpNumberOfBytesRecvd, LPDWORD lpFlags, LPWSAOVERLAPPED lpOverlapped, LPWSAOVERLAPPED_COMPLETION_ROUTINE lpCompletetionRoutine ) );
SETUP_DETOUR_EXTERN( int, WSAAPI, WSASend, ( SOCKET s, LPWSABUF lpBuffers, DWORD dwBufferCount, LPDWORD lpNumberOfBytesSent, DWORD dwFlags, LPWSAOVERLAPPED lpOverLapped, LPWSAOVERLAPPED_COMPLETION_ROUTINE lpCompletionRoutine ) );


#endif // !WSHOOKS_H_
