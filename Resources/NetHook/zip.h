
#ifndef ZIP_H_
#define ZIP_H_
#ifdef _WIN32
#pragma once
#endif


#include "steam/steamtypes.h"


class CZip
{

public:
	static bool Inflate( const uint8 *pCompressed, uint32 cubCompressed, uint8 *pDecompressed, uint32 cubDecompressed );

private:
	static bool InternalInflate( const uint8 *pCompressed, uint32 cubCompressed, uint8 *pDecompressed, uint32 cubDecompressed );

};


#endif // !ZIP_H_

