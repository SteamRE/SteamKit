#ifndef CSIMPLESCAN_H
#define CSIMPLESCAN_H

#include "sigscan.h"

#include "steam/steamtypes.h"

class CSimpleScan
{

public:
	CSimpleScan() noexcept;
	CSimpleScan( const char *filename ) noexcept;

	bool SetDLL( const char *filename ) noexcept;
	bool FindFunction( const char *sig, const char *mask, void **func ) noexcept;

private:
	bool m_bInterfaceSet;

	CreateInterfaceFn m_Interface;
	CSigScan m_Signature;

};

#endif //CSIMPLESCAN_H
