
#include "wshooks.h"

#include "udpconnection.h"
#include "logger.h"
#include "crypto.h"


SETUP_DETOUR_FUNCTION( int, PASCAL, recvfrom, ( SOCKET s, char *buf, int len, int flags, sockaddr *from, int *fromlen ) )
{
	int ret = recvfrom_T( s, buf, len, flags, from, fromlen );

	if ( ret == SOCKET_ERROR )
		return ret;

	sockaddr_in *sockAddr = (sockaddr_in *)from;

	bool bRet = g_udpConnection->ReceivePacket( (uint8 *)buf, ret, sockAddr );

	if ( !bRet )
		return 0; // drop the packet

	return ret;
}

SETUP_DETOUR_FUNCTION( int, WSAAPI, WSASendTo, ( SOCKET s, LPWSABUF lpBuffers, DWORD dwBufferCount, LPDWORD lpNumberOfBytesSent, DWORD dwFlags, const sockaddr *lpTo, int iTolen, LPWSAOVERLAPPED lpOverLapped, LPWSAOVERLAPPED_COMPLETION_ROUTINE lpCompletionRoutine ) )
{
	sockaddr_in *sockAddr = (sockaddr_in *)lpTo;

	bool bRet = g_udpConnection->SendPacket( (uint8 *)lpBuffers->buf, lpBuffers->len, sockAddr );

	if ( !bRet )
		return 0; // drop the packet

	return WSASendTo_T( s, lpBuffers, dwBufferCount, lpNumberOfBytesSent, dwFlags, lpTo, iTolen, lpOverLapped, lpCompletionRoutine );
}


SETUP_DETOUR_FUNCTION( int, WSAAPI, WSARecv, ( SOCKET s, LPWSABUF lpBuffers, DWORD dwBufferCount, LPDWORD lpNumberOfBytesRecvd, LPDWORD lpFlags, LPWSAOVERLAPPED lpOverlapped, LPWSAOVERLAPPED_COMPLETION_ROUTINE lpCompletetionRoutine ) )
{
	int ret = WSARecv_T( s, lpBuffers, dwBufferCount, lpNumberOfBytesRecvd, lpFlags, lpOverlapped, lpCompletetionRoutine );

	if ( ret == SOCKET_ERROR )
		return ret;

	sockaddr_in sockAddr;
	int sockSize = sizeof( sockAddr );
	getpeername( s, (sockaddr *)&sockAddr, &sockSize );

	// todo:
	// process tcp incoming

	return ret;
}


SETUP_DETOUR_FUNCTION( int, WSAAPI, WSASend, ( SOCKET s, LPWSABUF lpBuffers, DWORD dwBufferCount, LPDWORD lpNumberOfBytesSent, DWORD dwFlags, LPWSAOVERLAPPED lpOverLapped, LPWSAOVERLAPPED_COMPLETION_ROUTINE lpCompletionRoutine ) )
{
	int ret = WSASend_T( s, lpBuffers, dwBufferCount, lpNumberOfBytesSent, dwFlags, lpOverLapped, lpCompletionRoutine );

	sockaddr_in sockAddr;
	int sockSize = sizeof( sockAddr );
	getpeername( s, (sockaddr *)&sockAddr, &sockSize );

	// todo:
	// process tcp outgoing

	return ret;
}




CCrypto *g_Crypto = NULL;


int nStartups = 0;
SETUP_DETOUR_FUNCTION( int, PASCAL, WSAStartup, ( WORD wVersionRequired, LPWSADATA lpWSAData ) )
{
	nStartups++;

	int ret = WSAStartup_T( wVersionRequired, lpWSAData );

	g_Logger->LogConsole( "WSAStartup( %d, 0x%p ) = %d\n", wVersionRequired, lpWSAData, ret );

	// for steam.exe: nStartups must be 3
	// otherwise loading just steamclient.dll will call this hook only once
	if ( nStartups == 3 ) // at the third call, steamclient has already been loaded
	{
		g_Logger->LogConsole( "\nApplying hooks...\n\n" );


		// udp
		Detour_recvfrom->Attach();
		Detour_WSASendTo->Attach();

		// tcp
		Detour_WSARecv->Attach();
		Detour_WSASend->Attach();


		g_Crypto = new CCrypto(); // init crypto hooks

	}

	return ret;
}