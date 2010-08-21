
#ifndef CSTEAMID_H
#define CSTEAMID_H
#ifdef _WIN32
#pragma once
#endif

#include "steamtypes.h"

#pragma pack( push, 1 )

// Steam universes.  Each universe is a self-contained Steam instance.
enum EUniverse
{
	k_EUniverseInvalid = 0,
	k_EUniversePublic = 1,
	k_EUniverseBeta = 2,
	k_EUniverseInternal = 3,
	k_EUniverseDev = 4,
	k_EUniverseRC = 5,

	k_EUniverseMax
};


// Steam account types
enum EAccountType
{
	k_EAccountTypeInvalid = 0,			
	k_EAccountTypeIndividual = 1,		// single user account
	k_EAccountTypeMultiseat = 2,		// multiseat (e.g. cybercafe) account
	k_EAccountTypeGameServer = 3,		// game server account
	k_EAccountTypeAnonGameServer = 4,	// anonymous game server account
	k_EAccountTypePending = 5,			// pending
	k_EAccountTypeContentServer = 6,	// content server
	k_EAccountTypeClan = 7,
	k_EAccountTypeChat = 8,
	k_EAccountTypeP2PSuperSeeder = 9,	// a fake steamid used by superpeers to seed content to users of Steam P2P stuff
	k_EAccountTypeAnonUser = 10,

	// Max of 16 items in this field
	k_EAccountTypeMax
};

const int k_unSteamAccountIDMask = 0xFFFFFFFF;
const int k_unSteamAccountInstanceMask = 0x000FFFFF;

// Special flags for Chat accounts - they go in the top 8 bits
// of the steam ID's "instance", leaving 12 for the actual instances
enum EChatSteamIDInstanceFlags
{
	k_EChatAccountInstanceMask = 0x00000FFF, // top 8 bits are flags

	k_EChatInstanceFlagClan = ( k_unSteamAccountInstanceMask + 1 ) >> 1,	// top bit
	k_EChatInstanceFlagLobby = ( k_unSteamAccountInstanceMask + 1 ) >> 2,	// next one down, etc
	k_EChatInstanceFlagMMSLobby = ( k_unSteamAccountInstanceMask + 1 ) >> 3,	// next one down, etc

	// Max of 8 flags
};

// Steam ID structure (64 bits total)
class CSteamID
{
public:

	//-----------------------------------------------------------------------------
	// Purpose: Constructor
	//-----------------------------------------------------------------------------
	CSteamID()
	{
		m_steamid.m_comp.m_unAccountID = 0;
		m_steamid.m_comp.m_EAccountType = k_EAccountTypeInvalid;
		m_steamid.m_comp.m_EUniverse = k_EUniverseInvalid;
		m_steamid.m_comp.m_unAccountInstance = 0;
	}


	//-----------------------------------------------------------------------------
	// Purpose: Constructor
	// Input  : unAccountID -	32-bit account ID
	//			eUniverse -		Universe this account belongs to
	//			eAccountType -	Type of account
	//-----------------------------------------------------------------------------
	CSteamID( uint32 unAccountID, EUniverse eUniverse, EAccountType eAccountType )
	{
		Set( unAccountID, eUniverse, eAccountType );
	}


	//-----------------------------------------------------------------------------
	// Purpose: Constructor
	// Input  : unAccountID -	32-bit account ID
	//			unAccountInstance - instance
	//			eUniverse -		Universe this account belongs to
	//			eAccountType -	Type of account
	//-----------------------------------------------------------------------------
	CSteamID( uint32 unAccountID, unsigned int unAccountInstance, EUniverse eUniverse, EAccountType eAccountType )
	{
#if defined(_SERVER) && defined(Assert)
		Assert( ! ( ( k_EAccountTypeIndividual == eAccountType ) && ( 1 != unAccountInstance ) ) );	// enforce that for individual accounts, instance is always 1
#endif // _SERVER
		InstancedSet( unAccountID, unAccountInstance, eUniverse, eAccountType );
	}


	//-----------------------------------------------------------------------------
	// Purpose: Constructor
	// Input  : ulSteamID -		64-bit representation of a Steam ID
	// Note:	Will not accept a uint32 or int32 as input, as that is a probable mistake.
	//			See the stubbed out overloads in the private: section for more info.
	//-----------------------------------------------------------------------------
	CSteamID( uint64 ulSteamID )
	{
		SetFromUint64( ulSteamID );
	}


	//-----------------------------------------------------------------------------
	// Purpose: Sets parameters for steam ID
	// Input  : unAccountID -	32-bit account ID
	//			eUniverse -		Universe this account belongs to
	//			eAccountType -	Type of account
	//-----------------------------------------------------------------------------
	void Set( uint32 unAccountID, EUniverse eUniverse, EAccountType eAccountType )
	{
		m_steamid.m_comp.m_unAccountID = unAccountID;
		m_steamid.m_comp.m_EUniverse = eUniverse;
		m_steamid.m_comp.m_EAccountType = eAccountType;

		if ( eAccountType == k_EAccountTypeClan )
		{
			m_steamid.m_comp.m_unAccountInstance = 0;
		}
		else
		{
			m_steamid.m_comp.m_unAccountInstance = 1;
		}
	}


	//-----------------------------------------------------------------------------
	// Purpose: Sets parameters for steam ID
	// Input  : unAccountID -	32-bit account ID
	//			eUniverse -		Universe this account belongs to
	//			eAccountType -	Type of account
	//-----------------------------------------------------------------------------
	void InstancedSet( uint32 unAccountID, uint32 unInstance, EUniverse eUniverse, EAccountType eAccountType )
	{
		m_steamid.m_comp.m_unAccountID = unAccountID;
		m_steamid.m_comp.m_EUniverse = eUniverse;
		m_steamid.m_comp.m_EAccountType = eAccountType;
		m_steamid.m_comp.m_unAccountInstance = unInstance;
	}


	//-----------------------------------------------------------------------------
	// Purpose: Initializes a steam ID from its 52 bit parts and universe/type
	// Input  : ulIdentifier - 52 bits of goodness
	//-----------------------------------------------------------------------------
	void FullSet( uint64 ulIdentifier, EUniverse eUniverse, EAccountType eAccountType )
	{
		m_steamid.m_comp.m_unAccountID = ( ulIdentifier & 0xFFFFFFFF );						// account ID is low 32 bits
		m_steamid.m_comp.m_unAccountInstance = ( ( ulIdentifier >> 32 ) & 0xFFFFF );			// account instance is next 20 bits
		m_steamid.m_comp.m_EUniverse = eUniverse;
		m_steamid.m_comp.m_EAccountType = eAccountType;
	}


	//-----------------------------------------------------------------------------
	// Purpose: Initializes a steam ID from its 64-bit representation
	// Input  : ulSteamID -		64-bit representation of a Steam ID
	//-----------------------------------------------------------------------------
	void SetFromUint64( uint64 ulSteamID )
	{
		m_steamid.m_unAll64Bits = ulSteamID;
	}

	//-----------------------------------------------------------------------------
	// Purpose: Initializes a steam ID from a Steam2 ID structure
	// Input:	pTSteamGlobalUserID -	Steam2 ID to convert
	//			eUniverse -				universe this ID belongs to
	//-----------------------------------------------------------------------------
	void SetFromSteam2( TSteamGlobalUserID *pTSteamGlobalUserID, EUniverse eUniverse )
	{
		m_steamid.m_comp.m_unAccountID = pTSteamGlobalUserID->m_SteamLocalUserID.Split.Low32bits * 2 +
			pTSteamGlobalUserID->m_SteamLocalUserID.Split.High32bits;
		m_steamid.m_comp.m_EUniverse = eUniverse;		// set the universe
		m_steamid.m_comp.m_EAccountType = k_EAccountTypeIndividual; // Steam 2 accounts always map to account type of individual
		m_steamid.m_comp.m_unAccountInstance = 1;	// individual accounts always have an account instance ID of 1
	}

	//-----------------------------------------------------------------------------
	// Purpose: Fills out a Steam2 ID structure
	// Input:	pTSteamGlobalUserID -	Steam2 ID to write to
	//-----------------------------------------------------------------------------
	void ConvertToSteam2( TSteamGlobalUserID *pTSteamGlobalUserID ) const
	{
		// only individual accounts have any meaning in Steam 2, only they can be mapped
		// Assert( m_steamid.m_comp.m_EAccountType == k_EAccountTypeIndividual );

		pTSteamGlobalUserID->m_SteamInstanceID = 0;
		pTSteamGlobalUserID->m_SteamLocalUserID.Split.High32bits = m_steamid.m_comp.m_unAccountID % 2;
		pTSteamGlobalUserID->m_SteamLocalUserID.Split.Low32bits = m_steamid.m_comp.m_unAccountID / 2;
	}

	//-----------------------------------------------------------------------------
	// Purpose: Converts steam ID to its 64-bit representation
	// Output : 64-bit representation of a Steam ID
	//-----------------------------------------------------------------------------
	uint64 ConvertToUint64() const
	{
		return m_steamid.m_unAll64Bits;
	}


	//-----------------------------------------------------------------------------
	// Purpose: Converts the static parts of a steam ID to a 64-bit representation.
	//			For multiseat accounts, all instances of that account will have the
	//			same static account key, so they can be grouped together by the static
	//			account key.
	// Output : 64-bit static account key
	//-----------------------------------------------------------------------------
	uint64 GetStaticAccountKey() const
	{
		// note we do NOT include the account instance (which is a dynamic property) in the static account key
		return (uint64) ( ( ( (uint64) m_steamid.m_comp.m_EUniverse ) << 56 ) + ((uint64) m_steamid.m_comp.m_EAccountType << 52 ) + m_steamid.m_comp.m_unAccountID );
	}


	//-----------------------------------------------------------------------------
	// Purpose: create an anonymous game server login to be filled in by the AM
	//-----------------------------------------------------------------------------
	void CreateBlankAnonLogon( EUniverse eUniverse )
	{
		m_steamid.m_comp.m_unAccountID = 0;
		m_steamid.m_comp.m_EAccountType = k_EAccountTypeAnonGameServer;
		m_steamid.m_comp.m_EUniverse = eUniverse;
		m_steamid.m_comp.m_unAccountInstance = 0;
	}


	//-----------------------------------------------------------------------------
	// Purpose: create an anonymous game server login to be filled in by the AM
	//-----------------------------------------------------------------------------
	void CreateBlankAnonUserLogon( EUniverse eUniverse )
	{
		m_steamid.m_comp.m_unAccountID = 0;
		m_steamid.m_comp.m_EAccountType = k_EAccountTypeAnonUser;
		m_steamid.m_comp.m_EUniverse = eUniverse;
		m_steamid.m_comp.m_unAccountInstance = 0;
	}

	//-----------------------------------------------------------------------------
	// Purpose: Is this an anonymous game server login that will be filled in?
	//-----------------------------------------------------------------------------
	bool BBlankAnonAccount() const
	{
		return m_steamid.m_comp.m_unAccountID == 0 && BAnonAccount() && m_steamid.m_comp.m_unAccountInstance == 0;
	}

	//-----------------------------------------------------------------------------
	// Purpose: Is this a game server account id?
	//-----------------------------------------------------------------------------
	bool BGameServerAccount() const
	{
		return m_steamid.m_comp.m_EAccountType == k_EAccountTypeGameServer || m_steamid.m_comp.m_EAccountType == k_EAccountTypeAnonGameServer;
	}

	//-----------------------------------------------------------------------------
	// Purpose: Is this a content server account id?
	//-----------------------------------------------------------------------------
	bool BContentServerAccount() const
	{
		return m_steamid.m_comp.m_EAccountType == k_EAccountTypeContentServer;
	}


	//-----------------------------------------------------------------------------
	// Purpose: Is this a clan account id?
	//-----------------------------------------------------------------------------
	bool BClanAccount() const
	{
		return m_steamid.m_comp.m_EAccountType == k_EAccountTypeClan;
	}


	//-----------------------------------------------------------------------------
	// Purpose: Is this a chat account id?
	//-----------------------------------------------------------------------------
	bool BChatAccount() const
	{
		return m_steamid.m_comp.m_EAccountType == k_EAccountTypeChat;
	}

	//-----------------------------------------------------------------------------
	// Purpose: Is this a chat account id?
	//-----------------------------------------------------------------------------
	bool IsLobby() const
	{
		return ( m_steamid.m_comp.m_EAccountType == k_EAccountTypeChat )
			&& ( m_steamid.m_comp.m_unAccountInstance & k_EChatInstanceFlagLobby );
	}


	//-----------------------------------------------------------------------------
	// Purpose: Is this an individual user account id?
	//-----------------------------------------------------------------------------
	bool BIndividualAccount() const
	{
		return m_steamid.m_comp.m_EAccountType == k_EAccountTypeIndividual;
	}


	//-----------------------------------------------------------------------------
	// Purpose: Is this an anonymous account?
	//-----------------------------------------------------------------------------
	bool BAnonAccount() const
	{
		return m_steamid.m_comp.m_EAccountType == k_EAccountTypeAnonUser || m_steamid.m_comp.m_EAccountType == k_EAccountTypeAnonGameServer;
	}

	//-----------------------------------------------------------------------------
	// Purpose: Is this an anonymous user account? ( used to create an account or reset a password )
	//-----------------------------------------------------------------------------
	bool BAnonUserAccount() const
	{
		return m_steamid.m_comp.m_EAccountType == k_EAccountTypeAnonUser;
	}


	// simple accessors
	void SetAccountID( uint32 unAccountID )		{ m_steamid.m_comp.m_unAccountID = unAccountID; }
	uint32 GetAccountID() const					{ return m_steamid.m_comp.m_unAccountID; }
	uint32 GetUnAccountInstance() const			{ return m_steamid.m_comp.m_unAccountInstance; }
	EAccountType GetEAccountType() const		{ return ( EAccountType ) m_steamid.m_comp.m_EAccountType; }
	EUniverse GetEUniverse() const				{ return m_steamid.m_comp.m_EUniverse; }
	void SetEUniverse( EUniverse eUniverse )	{ m_steamid.m_comp.m_EUniverse = eUniverse; }
	bool IsValid() const;

	// this set of functions is hidden, will be moved out of class
	explicit CSteamID( const char *pchSteamID, EUniverse eDefaultUniverse = k_EUniverseInvalid );
	const char * Render() const				// renders this steam ID to string
	{
		static char szSteamID[64];
		switch(m_steamid.m_comp.m_EAccountType)
		{
		case k_EAccountTypeInvalid:
		case k_EAccountTypeIndividual:
			sprintf_s(szSteamID, sizeof(szSteamID), "STEAM_0:%u:%u", (m_steamid.m_comp.m_unAccountID % 2) ? 1 : 0, (int32)m_steamid.m_comp.m_unAccountID/2);
			break;
		default:
			sprintf_s(szSteamID, sizeof(szSteamID), "%llu", ConvertToUint64());
		}
		return szSteamID;
	}
	static const char * Render( uint64 ulSteamID )	// static method to render a uint64 representation of a steam ID to a string
	{
		return CSteamID(ulSteamID).Render();
	}

	const char *SteamRender() const // renders this steam ID to string using the new rendering style
	{
		const int k_cBufLen = 37;
		const int k_cBufs = 4;
		char* pchBuf;

		static char rgchBuf[k_cBufs][k_cBufLen];
		static int nBuf = 0;

		pchBuf = rgchBuf[nBuf++];
		nBuf %= k_cBufs;

		switch (m_steamid.m_comp.m_EAccountType)
		{
		case k_EAccountTypeAnonGameServer:
			sprintf_s(pchBuf, k_cBufLen, "[A:%u:%u:%u]", m_steamid.m_comp.m_EUniverse, m_steamid.m_comp.m_unAccountID, m_steamid.m_comp.m_unAccountInstance);
			break;
		case k_EAccountTypeGameServer:
			sprintf_s(pchBuf, k_cBufLen, "[G:%u:%u]", m_steamid.m_comp.m_EUniverse, m_steamid.m_comp.m_unAccountID);
			break;
		case k_EAccountTypeMultiseat:
			sprintf_s(pchBuf, k_cBufLen, "[M:%u:%u:%u]", m_steamid.m_comp.m_EUniverse, m_steamid.m_comp.m_unAccountID, m_steamid.m_comp.m_unAccountInstance);
			break;
		case k_EAccountTypePending:
			sprintf_s(pchBuf, k_cBufLen, "[P:%u:%u]", m_steamid.m_comp.m_EUniverse, m_steamid.m_comp.m_unAccountID);
			break;
		case k_EAccountTypeContentServer:
			sprintf_s(pchBuf, k_cBufLen, "[C:%u:%u]", m_steamid.m_comp.m_EUniverse, m_steamid.m_comp.m_unAccountID);
			break;
		case k_EAccountTypeClan:
			sprintf_s(pchBuf, k_cBufLen, "[g:%u:%u]", m_steamid.m_comp.m_EUniverse, m_steamid.m_comp.m_unAccountID);
			break;
		case k_EAccountTypeChat:
			switch (m_steamid.m_comp.m_unAccountInstance & ~k_EChatAccountInstanceMask)
			{
			case k_EChatInstanceFlagClan:
				sprintf_s(pchBuf, k_cBufLen, "[c:%u:%u]", m_steamid.m_comp.m_EUniverse, m_steamid.m_comp.m_unAccountID);
				break;
			case k_EChatInstanceFlagLobby:
				sprintf_s(pchBuf, k_cBufLen, "[L:%u:%u]", m_steamid.m_comp.m_EUniverse, m_steamid.m_comp.m_unAccountID);
				break;
			default:
				sprintf_s(pchBuf, k_cBufLen, "[T:%u:%u]", m_steamid.m_comp.m_EUniverse, m_steamid.m_comp.m_unAccountID);
				break;
			}
			break;
		case k_EAccountTypeInvalid:
			sprintf_s(pchBuf, k_cBufLen, "[I:%u:%u]", m_steamid.m_comp.m_EUniverse, m_steamid.m_comp.m_unAccountID);
			break;
		case k_EAccountTypeIndividual:
			sprintf_s(pchBuf, k_cBufLen, "[U:%u:%u]", m_steamid.m_comp.m_EUniverse, m_steamid.m_comp.m_unAccountID);
			break;
		default:
			sprintf_s(pchBuf, k_cBufLen, "[i:%u:%u]", m_steamid.m_comp.m_EUniverse, m_steamid.m_comp.m_unAccountID);
			break;
		}

		return pchBuf;
	}

	static const char *SteamRender( uint64 ulSteamID )	// static method to render a uint64 representation of a steam ID to a string
	{
		return CSteamID(ulSteamID).SteamRender();
	}

	void SetFromString( const char *pchSteamID, EUniverse eDefaultUniverse );
	bool SetFromSteam2String( const char *pchSteam2ID, EUniverse eUniverse );

	inline bool operator==( const CSteamID &val ) const { return m_steamid.m_unAll64Bits == val.m_steamid.m_unAll64Bits; }
	inline bool operator!=( const CSteamID &val ) const { return !operator==( val ); }
	inline bool operator<( const CSteamID &val ) const { return m_steamid.m_unAll64Bits < val.m_steamid.m_unAll64Bits; }
	inline bool operator>( const CSteamID &val ) const { return m_steamid.m_unAll64Bits > val.m_steamid.m_unAll64Bits; }

	// DEBUG function
	bool BValidExternalSteamID() const;

private:
	// These are defined here to prevent accidental implicit conversion of a u32AccountID to a CSteamID.
	// If you get a compiler error about an ambiguous constructor/function then it may be because you're
	// passing a 32-bit int to a function that takes a CSteamID. You should explicitly create the SteamID
	// using the correct Universe and account Type/Instance values.
	CSteamID( uint32 );
	CSteamID( int32 );

	// 64 bits total
	union SteamID_t
	{
		struct SteamIDComponent_t
		{
			uint32				m_unAccountID : 32;			// unique account identifier
			unsigned int		m_unAccountInstance : 20;	// dynamic instance ID (used for multiseat type accounts only)
			unsigned int		m_EAccountType : 4;			// type of account - can't show as EAccountType, due to signed / unsigned difference
			EUniverse			m_EUniverse : 8;	// universe this account belongs to
		} m_comp;

		uint64 m_unAll64Bits;
	} m_steamid;
};


inline bool CSteamID::IsValid() const
{
	if ( m_steamid.m_comp.m_EAccountType <= k_EAccountTypeInvalid || m_steamid.m_comp.m_EAccountType >= k_EAccountTypeMax )
		return false;

	if ( m_steamid.m_comp.m_EUniverse <= k_EUniverseInvalid || m_steamid.m_comp.m_EUniverse >= k_EUniverseMax )
		return false;

	if ( m_steamid.m_comp.m_EAccountType == k_EAccountTypeIndividual )
	{
		if ( m_steamid.m_comp.m_unAccountID == 0 || m_steamid.m_comp.m_unAccountInstance != 1 )
			return false;
	}

	if ( m_steamid.m_comp.m_EAccountType == k_EAccountTypeClan )
	{
		if ( m_steamid.m_comp.m_unAccountID == 0 || m_steamid.m_comp.m_unAccountInstance != 0 )
			return false;
	}
	return true;
}

// generic invalid CSteamID
const CSteamID k_steamIDNil;

// This steamID comes from a user game connection to an out of date GS that hasnt implemented the protocol
// to provide its steamID
const CSteamID k_steamIDOutofDateGS( 0, 0, k_EUniverseInvalid, k_EAccountTypeInvalid );
// This steamID comes from a user game connection to an sv_lan GS
const CSteamID k_steamIDLanModeGS( 0, 0, k_EUniversePublic, k_EAccountTypeInvalid );
// This steamID can come from a user game connection to a GS that has just booted but hasnt yet even initialized
// its steam3 component and started logging on.
const CSteamID k_steamIDNotInitYetGS( 1, 0, k_EUniverseInvalid, k_EAccountTypeInvalid );
// This steamID can come from a user game connection to a GS that isn't using the steam authentication system but still
// wants to support the "Join Game" option in the friends list
const CSteamID k_steamIDNonSteamGS( 2, 0, k_EUniverseInvalid, k_EAccountTypeInvalid );


#ifdef STEAM
// Returns the matching chat steamID, with the default instance of 0
// If the steamID passed in is already of type k_EAccountTypeChat it will be returned with the same instance
CSteamID ChatIDFromSteamID( const CSteamID &steamID );
// Returns the matching clan steamID, with the default instance of 0
// If the steamID passed in is already of type k_EAccountTypeClan it will be returned with the same instance
CSteamID ClanIDFromSteamID( const CSteamID &steamID );
// Asserts steamID type before conversion
CSteamID ChatIDFromClanID( const CSteamID &steamIDClan );
// Asserts steamID type before conversion
CSteamID ClanIDFromChatID( const CSteamID &steamIDChat );

#endif // _STEAM

#pragma pack( pop )

#endif // CSTEAMID_H
