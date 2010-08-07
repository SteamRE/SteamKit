
#ifndef NETPACKET_H_
#define NETPACKET_H_
#ifdef _WIN32
#pragma once
#endif


#include "utlvector.h"

#include "steam/steamtypes.h"
#include "steam/udppkt.h"


struct MsgSegment_t
{
	uint32 m_dataSequence;

	uint32 m_dataSize;
	uint8 *m_pData;
};

class CNetPacket
{

public:
	CNetPacket( uint32 seqStart, uint32 numPkts, bool bEncrypted = false );
	~CNetPacket();

	void AddData( const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData );

	uint8 *GetData();
	uint32 GetSize();

	bool IsEncrypted() { return m_bEncrypted; }
	bool IsCompleted() { return m_bCompleted; }

private:
	bool HasData( const UDPPktHdr_t *pHdr );


private:
	uint32 m_seqStart;
	uint32 m_seqEnd;

	uint32 m_numPkts;

	CUtlVector< MsgSegment_t > m_dataVec;

	bool m_bEncrypted;
	bool m_bCompleted;

};

#endif // !NETPACKET_H_