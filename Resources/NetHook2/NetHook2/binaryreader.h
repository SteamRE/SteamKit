

#ifndef NETHOOK_BINARYREADER_H_
#define NETHOOK_BINARYREADER_H_

#include "steam/steamtypes.h"


class CBinaryReader
{

public:
	CBinaryReader( uint8 *pData, uint32 cubData );

	uint32 GetPosition() noexcept { return m_Position; }
	void SetPosition( uint32 pos ) noexcept { m_Position = pos; }
	void SeekRelative( uint32 pos ) noexcept { m_Position += pos; }

	uint32 GetSizeLeft() noexcept { return m_cubData - m_Position; }

	template<typename T>
	T Read() noexcept
	{
		T readData = *(T *)( m_pData + m_Position );
		m_Position += sizeof( T );

		return readData;
	}

	uint8 *ReadBytes( uint32 len ) noexcept
	{
		uint8 *ret = ( m_pData + m_Position );
		m_Position += len;

		return ret;
	}

private:
	uint8 *m_pData;
	uint32 m_cubData;

	uint32 m_Position;
};


#endif // !NETHOOK_BINARYREADER_H_
