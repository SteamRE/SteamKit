
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


typedef uint64	SteamUnsigned64_t;


typedef void  (*SteamAPIWarningMessageHook_t)(int hpipe, const char *message);


//-----------------------------------------------------------------------------
// GID (GlobalID) stuff
// This is a globally unique identifier.  It's guaranteed to be unique across all
// racks and servers for as long as a given universe persists.
//-----------------------------------------------------------------------------
// NOTE: for GID parsing/rendering and other utils, see gid.h
typedef uint64 GID_t;

constexpr GID_t k_GIDNil = 0xffffffffffffffffull;

// For convenience, we define a number of types that are just new names for GIDs
typedef GID_t JobID_t;			// Each Job has a unique ID
typedef GID_t TxnID_t;			// Each financial transaction has a unique ID

constexpr GID_t k_TxnIDNil = k_GIDNil;
constexpr GID_t k_TxnIDUnknown = 0;

// this is baked into client messages and interfaces as an int, 
// make sure we never break this.  AppIds and DepotIDs also presently
// share the same namespace, but since we'd like to change that in the future
// I've defined it seperately here.
typedef uint32 AppId_t;
typedef uint32 PackageId_t;
typedef uint32 DepotId_t;

constexpr AppId_t k_uAppIdInvalid = 0x0;

constexpr PackageId_t k_uPackageIdFreeSub = 0x0;
constexpr PackageId_t k_uPackageIdInvalid = 0xFFFFFFFF;
constexpr PackageId_t k_uPackageIdWallet = -2;
constexpr PackageId_t k_uPackageIdMicroTxn = -3;

constexpr DepotId_t k_uDepotIdInvalid = 0x0;


typedef uint32 CellID_t;
constexpr CellID_t k_uCellIDInvalid = 0xFFFFFFFF;

// handle to a Steam API call
typedef uint64 SteamAPICall_t;
constexpr SteamAPICall_t k_uAPICallInvalid = 0x0;


// handle to a communication pipe to the Steam client
typedef int32 HSteamPipe;
// handle to single instance of a steam user
typedef int32 HSteamUser;
// reference to a steam call, to filter results by
typedef int32 HSteamCall;

// return type of GetAuthSessionTicket
typedef uint32 HAuthTicket;
constexpr HAuthTicket k_HAuthTicketInvalid = 0;

typedef int HVoiceCall;


constexpr int k_cchSystemIMTextMax = 4096;



// RTime32
// We use this 32 bit time representing real world time.
// It offers 1 second resolution beginning on January 1, 1970 (Unix time)
typedef uint32 RTime32;
constexpr RTime32 k_RTime32Nil = 0;
constexpr RTime32 k_RTime32MinValid = 10;
constexpr RTime32 k_RTime32Infinite = 0x7FFFFFFF;



constexpr uint32 k_nMagic = 0x31305356; // "VS01"
constexpr uint32 k_nMagic_Old1 = 0x4D545356; // "VSTM"

constexpr uint32 k_cchTruncatedPassword = 20;
constexpr uint32 k_cchAccountName = 64;

constexpr uint32 k_nChallengeMask = 0xA426DF2B;
constexpr uint32 k_nObfuscationMask = 0xBAADF00D;

typedef void* (*CreateInterfaceFn)(const char *pName, int *pReturnCode);


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


//-----------------------------------------------------------------------------
// Purpose: Base values for callback identifiers, each callback must
//			have a unique ID.
//-----------------------------------------------------------------------------
enum ECallbackType
{
	k_iSteamUserCallbacks = 100,
	k_iSteamGameServerCallbacks = 200,
	k_iSteamFriendsCallbacks = 300,
	k_iSteamBillingCallbacks = 400,
	k_iSteamMatchmakingCallbacks = 500,
	k_iSteamContentServerCallbacks = 600,
	k_iSteamUtilsCallbacks = 700,
	k_iClientFriendsCallbacks = 800,
	k_iClientUserCallbacks = 900,
	k_iSteamAppsCallbacks = 1000,
	k_iSteamUserStatsCallbacks = 1100,
	k_iSteamNetworkingCallbacks = 1200,
	k_iClientRemoteStorageCallbacks = 1300,
	k_iSteamUserItemsCallbacks = 1400,
	k_iSteamGameServerItemsCallbacks = 1500,
	k_iClientUtilsCallbacks = 1600,
	k_iSteamGameCoordinatorCallbacks = 1700,
	k_iSteamGameServerStatsCallbacks = 1800,
	k_iSteam2AsyncCallbacks = 1900,
	k_iSteamGameStatsCallbacks = 2000,
	k_iClientHTTPCallbacks = 2100
};

// Each Steam instance (licensed Steam Service Provider) has a unique SteamInstanceID_t.
//
// Each Steam instance as its own DB of users.
// Each user in the DB has a unique SteamLocalUserID_t (a serial number, with possible 
// rare gaps in the sequence).

typedef	unsigned short		SteamInstanceID_t;		// MUST be 16 bits

#if defined (WIN32)
	typedef	unsigned __int64	SteamLocalUserID_t;		// MUST be 64 bits
#else
	typedef	unsigned long long	SteamLocalUserID_t;		// MUST be 64 bits
#endif






// Applications need to be able to authenticate Steam users from ANY instance.
// So a SteamIDTicket contains SteamGlobalUserID, which is a unique combination of 
// instance and user id.

// SteamLocalUserID is an unsigned 64-bit integer.
// For platforms without 64-bit int support, we provide access via a union that splits it into 
// high and low unsigned 32-bit ints.  Such platforms will only need to compare LocalUserIDs 
// for equivalence anyway - not perform arithmetic with them.
struct TSteamSplitLocalUserID
{
	unsigned int	Low32bits;
	unsigned int	High32bits;
};

struct TSteamGlobalUserID
{
	SteamInstanceID_t m_SteamInstanceID;

	union m_SteamLocalUserID
	{
		SteamLocalUserID_t		As64bits;
		TSteamSplitLocalUserID	Split;
	} m_SteamLocalUserID;

};

// structure that contains client callback data
struct CallbackMsg_t
{
	HSteamUser m_hSteamUser;
	int m_iCallback;
	uint8 *m_pubParam;
	int m_cubParam;
};

enum EServerType
{
	k_EServerTypeInvalid = -1,
	k_EServerTypeShell = 0,
	k_EServerTypeGM = 1,
	k_EServerTypeBUM = 2,
	k_EServerTypeAM = 3,
	k_EServerTypeBS = 4,
	k_EServerTypeVS = 5,
	k_EServerTypeATS = 6,
	k_EServerTypeCM = 7,
	k_EServerTypeFBS = 8,
	k_EServerTypeFG = 9,
	k_EServerTypeSS = 10,
	k_EServerTypeDRMS = 11,
	k_EServerTypeHubOBSOLETE = 12,
	k_EServerTypeConsole = 13,
	k_EServerTypeASBOBSOLETE = 14,
	k_EServerTypeClient = 15,
	k_EServerTypeBootstrapOBSOLETE = 16,
	k_EServerTypeDP = 17,
	k_EServerTypeWG = 18,
	k_EServerTypeSM = 19,
	k_EServerTypeUFS = 21,
	k_EServerTypeUtil = 23,
	k_EServerTypeDSS = 24,
	k_EServerTypeP2PRelayOBSOLETE = 25,
	k_EServerTypeAppInformation = 26,
	k_EServerTypeSpare = 27,
	k_EServerTypeFTS = 28,
	k_EServerTypeEPM = 29,
	k_EServerTypePS = 30,
	k_EServerTypeIS = 31,
	k_EServerTypeCCS = 32,
	k_EServerTypeDFS = 33,
	k_EServerTypeLBS = 34,
	k_EServerTypeMDS = 35,
	k_EServerTypeCS = 36,
	k_EServerTypeGC = 37,
	k_EServerTypeNS = 38,
	k_EServerTypeOGS = 39,
	k_EServerTypeWebAPI = 40,
	k_EServerTypeUDS = 41,
	k_EServerTypeMMS = 42,
	k_EServerTypeGMS = 43,
	k_EServerTypeKGS = 44,
	k_EServerTypeUCM = 45,
	k_EServerTypeRM = 46,
	k_EServerTypeFS = 47,
	k_EServerTypeEcon = 48,
	k_EServerTypeBackpack = 49,
	k_EServerTypeMax = 50,
};

#endif // !STEAMTYPES_H_
