#include "StringUtil.h"

unsigned int StringLength(const char *chString)
{
	unsigned int nCount = 0;

	while (chString[nCount] != '\0')
		nCount++;

	return nCount;
}