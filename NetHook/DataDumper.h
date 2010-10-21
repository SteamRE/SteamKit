#ifndef DATADUMPER_H_
#define DATADUMPER_H_
#ifdef _WIN32
#pragma once
#endif

#include "time.h"

#include "crypto.h"
#include "steam/emsg.h"

enum ENetDirection
{
	k_eNetIncoming,
	k_eNetOutgoing,
};

class CDataDumper : public ICryptoCallback
{
public:
	CDataDumper();

	void DataEncrypted(const uint8* pubPlaintextData, uint32 cubPlaintextData);
	void DataDecrypted(const uint8* pubPlaintextData, uint32 cubPlaintextData);

	const char* GetFileName( const char* file );

private:
	bool HandleNetMsg( ENetDirection eDirection, EMsg eMsg, const uint8 *pData, uint32 cubData );
	bool MultiplexMsgMulti( ENetDirection eDirection, const uint8 *pData, uint32 cubData );

	const char* GetFileName( ENetDirection eDirection, EMsg eMsg );

	char m_szSessionDir[MAX_PATH];
	uint32 m_uiMsgNum;
};

extern CDataDumper* g_Dumper;

#endif // !DATADUMPER_H_