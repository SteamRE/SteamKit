
#include "zip.h"
#include "logger.h"

#include "zlib/zlib.h"


#pragma pack( push, 1 )

struct LocalFileHeader
{
	uint32 m_Signature;

	uint16 m_VersionNeededToExtract;
	uint16 m_GeneralPurposeBitFlag;

	uint16 m_CompressionMethod;

	uint16 m_LastModFileTime;
	uint16 m_LastModFileDate;

	uint32 m_Crc32;

	uint32 m_CompressedSize;
	uint32 m_UncompressedSize;

	uint16 m_FileNameLength;
	uint16 m_ExtraFieldLength;
};

#pragma pack( pop )


bool CZip::Inflate( const uint8 *pCompressed, uint32 cubCompressed, uint8 *pDecompressed, uint32 cubDecompressed )
{
	LocalFileHeader *fileHeader = (LocalFileHeader *)pCompressed;

	uint32 offsetStart = sizeof( LocalFileHeader ) + fileHeader->m_FileNameLength +fileHeader->m_ExtraFieldLength;
	uint32 dataSize = fileHeader->m_CompressedSize;

	return CZip::InternalInflate( pCompressed + offsetStart, dataSize, pDecompressed, cubDecompressed );
}

bool CZip::InternalInflate( const uint8 *pCompressed, uint32 cubCompressed, uint8 *pDecompressed, uint32 cubDecompressed )
{
	z_stream zstrm;

    zstrm.zalloc = Z_NULL;
    zstrm.zfree = Z_NULL;
    zstrm.opaque = Z_NULL;
    zstrm.avail_in = 0;
    zstrm.next_in = Z_NULL;

	int ret = inflateInit2( &zstrm, -15 );

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
