#include "NumberUtil.h"

unsigned short EndianSwap16(unsigned short nValue)
{
	return (
		(nValue << 8) |
		(nValue >> 8)
	);
}

unsigned int EndianSwap32(unsigned int nValue)
{
	return (
		(nValue >> 24) |
		((nValue >> 8) & 0x0000FF00) |
		((nValue << 8) & 0x00FF0000) |
		(nValue << 24)
	);
}

unsigned long long EndianSwap64(unsigned long long nValue)
{
	return (
		(nValue >> 56) |
		((nValue >> 40) & 0x000000000000FF00) |
		((nValue >> 24) & 0x0000000000FF0000) |
		((nValue >> 8) & 0x00000000FF000000) |
		((nValue << 8) & 0x000000FF00000000) |
		((nValue << 24) & 0x0000FF0000000000) |
		((nValue << 40) & 0x00FF000000000000) |
		(nValue << 56)
	);
}