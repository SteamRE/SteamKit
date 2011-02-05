

#include "binaryreader.h"


CBinaryReader::CBinaryReader( uint8 *pData, uint32 cubData )
{
	m_pData = pData;
	m_cubData = cubData;

	m_Position = 0;
}
