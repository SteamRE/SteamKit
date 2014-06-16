#include <cstring>

bool stringCaseInsensitiveEndsWith(const char * szHaystack, const char * szNeedle)
{
	int iHaystackLen = strlen(szHaystack);
	int iNeedleLen = strlen(szNeedle);

	if (iHaystackLen < iNeedleLen)
	{
		return false;
	}

	const char * szHaystackFromNeedleStartPosition = szHaystack + iHaystackLen - iNeedleLen;

	return _stricmp(szHaystackFromNeedleStartPosition, szNeedle) == 0;
}
