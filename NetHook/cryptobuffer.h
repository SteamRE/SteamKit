
#ifndef CRYPTOBUFFER_H_
#define CRYPTOBUFFER_H_
#ifdef _WIN32
#pragma once
#endif


#include "steam/steamtypes.h"


class CCryptoOutBuffer
{

public:
	CCryptoOutBuffer();
	~CCryptoOutBuffer();


	void Set( uint8 *pubData, uint32 cubData )
	{
		m_pubData = pubData;
		m_cubData = cubData;
	}

	uint8 *PubData() { return m_pubData; }
	uint32 CubData() { return m_cubData; }


private:
	uint8 *m_pubData;
	uint32 m_cubData;

};

#endif // !CRYPTOBUFFER_H_
