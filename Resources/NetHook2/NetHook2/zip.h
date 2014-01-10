

#ifndef NETHOOK_ZIP_H_
#define NETHOOK_ZIP_H_

#include "steam/steamtypes.h"


class CZip
{

public:
	static bool Inflate( const uint8 *pCompressed, uint32 cubCompressed, uint8 *pDecompressed, uint32 cubDecompressed );

};


#endif // !NETHOOK_ZIP_H_

