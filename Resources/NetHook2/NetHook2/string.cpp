#include <cstring>

bool stringCaseInsensitiveEndsWith(const char * szHaystack, const char * szNeedle)
{
	int uHaystackLen = strlen(szHaystack);
	int uNeedleLen = strlen(szNeedle);

	if (uHaystackLen < uNeedleLen)
	{
		return false;
	}

	const char * szHaystackFromNeedleStartPosition = szHaystack + uHaystackLen - uNeedleLen;

	return _stricmp(szHaystackFromNeedleStartPosition, szNeedle) == 0;
}
