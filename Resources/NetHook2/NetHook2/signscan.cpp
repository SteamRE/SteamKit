#include <cstring>
#include "signscan.h"

namespace NetHook
{

bool FindSignature(const char* start, const char* end, const char *sig, const char *mask, void **func, const char* prev) noexcept
{
    int signLen = strlen(mask);

    const char* searchBase = start;
    const char* searchEnd = end;

    while(searchBase < searchEnd)
    {
        int i;
        for(i = 0; i < signLen; ++i)
        {
            if(mask[i] != '?' && searchBase[i] != sig[i])
            {
                break;
            }
        }

        if(i == signLen && searchBase != prev)
        {
            *func = (void*)searchBase;
            return true;
        }

        ++searchBase;
    }

    return false;
}

}
