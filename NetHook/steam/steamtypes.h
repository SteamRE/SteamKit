
#ifndef STEAMTYPES_H_
#define STEAMTYPES_H_
#ifdef _WIN32
#pragma once
#endif

#include <cstdio>

#ifdef _WIN32
	#if defined( STEAM_API_EXPORTS )
		#define S_API extern "C" __declspec( dllexport ) 
	#else
		#define S_API extern "C" __declspec( dllimport ) 
	#endif
#else
	#define S_API extern "C"

	#ifndef __cdecl
		#define __cdecl __attribute__((__cdecl__))
	#endif
#endif

#if defined( __x86_64__ ) || defined( _WIN64 )
	#define X64BITS
#endif


#define STEAM_CALL __cdecl


// Steam-specific types. Defined here so this header file can be included in other code bases.
#ifndef WCHARTYPES_H
	typedef unsigned char uint8;
#endif

#if defined( _WIN32 )

	typedef __int16 int16;
	typedef unsigned __int16 uint16;
	typedef __int32 int32;
	typedef unsigned __int32 uint32;
	typedef __int64 int64;
	typedef unsigned __int64 uint64;

	#ifdef X64BITS
		typedef __int64 intp;				// intp is an integer that can accomodate a pointer
		typedef unsigned __int64 uintp;		// (ie, sizeof(intp) >= sizeof(int) && sizeof(intp) >= sizeof(void *)
	#else
		typedef __int32 intp;
		typedef unsigned __int32 uintp;
	#endif

#else // !_WIN32

	typedef short int16;
	typedef unsigned short uint16;
	typedef int int32;
	typedef unsigned int uint32;
	typedef long long int64;
	typedef unsigned long long uint64;

	#ifdef X64BITS
		typedef long long intp;
		typedef unsigned long long uintp;
	#else
		typedef int intp;
		typedef unsigned int uintp;
	#endif

#endif // else _WIN32



//-----------------------------------------------------------------------------
// GID (GlobalID) stuff
// This is a globally unique identifier.  It's guaranteed to be unique across all
// racks and servers for as long as a given universe persists.
//-----------------------------------------------------------------------------
// NOTE: for GID parsing/rendering and other utils, see gid.h
typedef uint64 GID_t;

const GID_t k_GIDNil = 0xffffffffffffffffull;

// For convenience, we define a number of types that are just new names for GIDs
typedef GID_t JobID_t;			// Each Job has a unique ID
typedef GID_t TxnID_t;			// Each financial transaction has a unique ID

const GID_t k_TxnIDNil = k_GIDNil;
const GID_t k_TxnIDUnknown = 0;




// RTime32
// We use this 32 bit time representing real world time.
// It offers 1 second resolution beginning on January 1, 1970 (Unix time)
typedef uint32 RTime32;
const RTime32 k_RTime32Nil = 0;
const RTime32 k_RTime32MinValid = 10;
const RTime32 k_RTime32Infinite = 0x7FFFFFFF;

typedef uint32 CellID_t;
const CellID_t k_uCellIDInvalid = 0xFFFFFFFF;


const uint32 k_nMagic = 0x31305356; // "VS01"
const uint32 k_nMagic_Old1 = 0x4D545356; // "VSTM"

const uint32 k_cchTruncatedPassword = 20;
const uint32 k_cchAccountName = 64;

const uint32 k_nChallengeMask = 0xA426DF2B;
const uint32 k_nObfuscationMask = 0xBAADF00D;


// General result codes
enum EResult
{
	k_EResultOK	= 1,							// success
	k_EResultFail = 2,							// generic failure 
	k_EResultNoConnection = 3,					// no/failed network connection
	//	k_EResultNoConnectionRetry = 4,				// OBSOLETE - removed
	k_EResultInvalidPassword = 5,				// password/ticket is invalid
	k_EResultLoggedInElsewhere = 6,				// same user logged in elsewhere
	k_EResultInvalidProtocolVer = 7,			// protocol version is incorrect
	k_EResultInvalidParam = 8,					// a parameter is incorrect
	k_EResultFileNotFound = 9,					// file was not found
	k_EResultBusy = 10,							// called method busy - action not taken
	k_EResultInvalidState = 11,					// called object was in an invalid state
	k_EResultInvalidName = 12,					// name is invalid
	k_EResultInvalidEmail = 13,					// email is invalid
	k_EResultDuplicateName = 14,				// name is not unique
	k_EResultAccessDenied = 15,					// access is denied
	k_EResultTimeout = 16,						// operation timed out
	k_EResultBanned = 17,						// VAC2 banned
	k_EResultAccountNotFound = 18,				// account not found
	k_EResultInvalidSteamID = 19,				// steamID is invalid
	k_EResultServiceUnavailable = 20,			// The requested service is currently unavailable
	k_EResultNotLoggedOn = 21,					// The user is not logged on
	k_EResultPending = 22,						// Request is pending (may be in process, or waiting on third party)
	k_EResultEncryptionFailure = 23,			// Encryption or Decryption failed
	k_EResultInsufficientPrivilege = 24,		// Insufficient privilege
	k_EResultLimitExceeded = 25,				// Too much of a good thing
	k_EResultRevoked = 26,						// Access has been revoked (used for revoked guest passes)
	k_EResultExpired = 27,						// License/Guest pass the user is trying to access is expired
	k_EResultAlreadyRedeemed = 28,				// Guest pass has already been redeemed by account, cannot be acked again
	k_EResultDuplicateRequest = 29,				// The request is a duplicate and the action has already occurred in the past, ignored this time
	k_EResultAlreadyOwned = 30,					// All the games in this guest pass redemption request are already owned by the user
	k_EResultIPNotFound = 31,					// IP address not found
	k_EResultPersistFailed = 32,				// failed to write change to the data store
	k_EResultLockingFailed = 33,				// failed to acquire access lock for this operation
	k_EResultLogonSessionReplaced = 34,
	k_EResultConnectFailed = 35,
	k_EResultHandshakeFailed = 36,
	k_EResultIOFailure = 37,
	k_EResultRemoteDisconnect = 38,
	k_EResultShoppingCartNotFound = 39,			// failed to find the shopping cart requested
	k_EResultBlocked = 40,						// a user didn't allow it
	k_EResultIgnored = 41,						// target is ignoring sender
	k_EResultNoMatch = 42,						// nothing matching the request found
	k_EResultAccountDisabled = 43,
	k_EResultServiceReadOnly = 44,				// this service is not accepting content changes right now
	k_EResultAccountNotFeatured = 45,			// account doesn't have value, so this feature isn't available
	k_EResultAdministratorOK = 46,				// allowed to take this action, but only because requester is admin
	k_EResultContentVersion = 47,				// A Version mismatch in content transmitted within the Steam protocol.
	k_EResultTryAnotherCM = 48,					// The current CM can't service the user making a request, user should try another.
	k_EResultPasswordRequiredToKickSession = 49,		// You are already logged in elsewhere, this cached credential login has failed.
	k_EResultAlreadyLoggedInElsewhere = 50,		// You are already logged in elsewhere, you must wait
	k_EResultSuspended = 51,
	k_EResultCancelled = 52,
	k_EResultDataCorruption = 53,
	k_EResultDiskFull = 54,
	k_EResultRemoteCallFailed = 55,

};

#endif // !STEAMTYPES_H_
