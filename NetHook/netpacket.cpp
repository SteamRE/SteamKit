
#include "netpacket.h"

#include <memory>

CNetPacket::CNetPacket( uint32 seqStart, uint32 numPkts, bool bEncrypted )
{

	m_seqStart = seqStart;
	m_numPkts = numPkts;

	m_seqEnd = m_seqStart + m_numPkts;

	m_bEncrypted = bEncrypted;
	m_bCompleted = false;

}

CNetPacket::~CNetPacket()
{
	FOR_EACH_VEC( m_dataVec, i )
	{
		MsgSegment_t msgSeg = m_dataVec[ i ];

		if ( msgSeg.m_pData )
			delete [] msgSeg.m_pData;
	}
}


void CNetPacket::AddData( const UDPPktHdr_t *pHdr, const uint8 *pData, uint32 cubData )
{
	if ( HasData( pHdr ) )
		return;

	MsgSegment_t msgSeg;

	msgSeg.m_dataSequence = pHdr->m_nSeqThis;
	msgSeg.m_dataSize = cubData;
	msgSeg.m_pData = new uint8[ cubData ];

	memcpy( msgSeg.m_pData, pData, cubData );

	m_dataVec.AddToTail( msgSeg );

	if ( m_dataVec.Count() == m_numPkts )
		m_bCompleted = true;
}

bool CNetPacket::HasData( const UDPPktHdr_t *pHdr )
{
	FOR_EACH_VEC( m_dataVec, i )
	{
		if ( m_dataVec[ i ].m_dataSequence == pHdr->m_nSeqThis )
			return true;
	}

	return false;
}


int msgCompare( const MsgSegment_t *a, const MsgSegment_t *b )
{
	if ( a->m_dataSequence < b->m_dataSequence )
		return -1;

	if ( a->m_dataSequence == b->m_dataSequence )
		return 0;

	return 1;
}

uint8 *CNetPacket::GetData()
{
	Assert( m_bCompleted );

	uint32 uiSize = this->GetSize();

	uint8 *pData = new uint8[ uiSize ];

	m_dataVec.Sort( msgCompare );

	uint32 uiOff = 0;

	FOR_EACH_VEC( m_dataVec, i )
	{
		uint8 *pDataDst = (uint8 *)( pData + uiOff );
		MsgSegment_t msgSeg = m_dataVec[ i ];

		memcpy( pDataDst, msgSeg.m_pData, msgSeg.m_dataSize );

		uiOff += msgSeg.m_dataSize;
	}

	return pData;
}

uint32 CNetPacket::GetSize()
{
	Assert( m_bCompleted );

	uint32 uiSize = 0;

	FOR_EACH_VEC( m_dataVec, i )
	{
		uiSize += m_dataVec[ i ].m_dataSize;
	}

	return uiSize;
}