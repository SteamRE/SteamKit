
#include "cryptobuffer.h"

CCryptoOutBuffer::CCryptoOutBuffer()
{

	m_pubData = NULL;
	m_cubData = 0;

}

CCryptoOutBuffer::~CCryptoOutBuffer()
{

	if ( m_pubData )
		delete [] m_pubData;

	m_pubData = NULL;
	m_cubData = 0;

}
