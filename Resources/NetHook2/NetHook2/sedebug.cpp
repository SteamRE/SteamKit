#include "sedebug.h"

// See: http://support.microsoft.com/KB/131065

BOOL SetPrivilege( HANDLE hToken, LPCTSTR szPrivilege, BOOL bEnablePrivilege )
{
    TOKEN_PRIVILEGES tp;
    LUID luid;
    TOKEN_PRIVILEGES tpPrevious;
    DWORD cbPrevious = sizeof( tpPrevious );

    if ( !LookupPrivilegeValue( NULL, szPrivilege, &luid ) )
    {
        return FALSE;
    }

    //
    // first pass.  get current privilege setting
    //
    tp.PrivilegeCount = 1;
    tp.Privileges[0].Luid = luid;
    tp.Privileges[0].Attributes = 0;

    AdjustTokenPrivileges( hToken, FALSE, &tp, sizeof( tp ), &tpPrevious, &cbPrevious );

    if ( GetLastError() != ERROR_SUCCESS )
    {
        return FALSE;
    }

    //
    // second pass.  set privilege based on previous setting
    //
    tpPrevious.PrivilegeCount = 1;
    tpPrevious.Privileges[0].Luid = luid;

    if ( bEnablePrivilege )
    {
        tpPrevious.Privileges[0].Attributes |= SE_PRIVILEGE_ENABLED;
    }
    else
    {
        tpPrevious.Privileges[0].Attributes ^= (SE_PRIVILEGE_ENABLED & tpPrevious.Privileges[0].Attributes);
    }

    AdjustTokenPrivileges( hToken, FALSE, &tpPrevious, cbPrevious, NULL, NULL );

    if ( GetLastError() != ERROR_SUCCESS )
    {
        return FALSE;
    }

    return TRUE;
}

HANDLE SeDebugAcquire()
{
    HANDLE hToken;
    if ( !OpenThreadToken( GetCurrentThread(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, FALSE, &hToken ) )
    {
        if ( GetLastError() == ERROR_NO_TOKEN )
        {
            if ( !ImpersonateSelf( SecurityImpersonation ) )
            {
                return NULL;
            }

            if ( !OpenThreadToken( GetCurrentThread(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, FALSE, &hToken ) )
            {
                return NULL;
            }
        }
        else
        {
            return NULL;
        }
    }

    if ( !SetPrivilege( hToken, SE_DEBUG_NAME, true ) )
    {
        CloseHandle( hToken );
        return NULL;
    }

    return hToken;
}