
#include "zip.h"
#include "logger.h"

#include "zlib/zlib.h"


bool CZip::Inflate( const uint8 *pCompressed, uint32 cubCompressed, uint8 *pDecompressed, uint32 cubDecompressed )
{
	z_stream zstrm;

	zstrm.zalloc = Z_NULL;
	zstrm.zfree = Z_NULL;
	zstrm.opaque = Z_NULL;
	zstrm.avail_in = 0;
	zstrm.next_in = Z_NULL;

	int ret = inflateInit2( &zstrm, 16 );

	if ( ret != Z_OK )
		return false;

	zstrm.avail_in = cubCompressed;
	zstrm.next_in = (Bytef *)pCompressed;

	zstrm.avail_out = cubDecompressed;
	zstrm.next_out = pDecompressed;

	ret = inflate( &zstrm, Z_NO_FLUSH );

	inflateEnd( &zstrm );

	if ( ret != Z_STREAM_END )
		return false;

	return true;
}
