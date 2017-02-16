#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <Psapi.h>
#include <stdlib.h>
#include <memory>

#include "nh2_string.h"
#include "sedebug.h"

typedef std::shared_ptr<void> SafeHandle;
inline SafeHandle MakeSafeHandle(HANDLE hHandle)
{
	return SafeHandle(hHandle, [](HANDLE hHandleToClose)
		{
			if (hHandleToClose != NULL)
			{
				CloseHandle(hHandleToClose);
			}
		});
}

typedef std::shared_ptr<void> SafeRemoteMem;
inline SafeRemoteMem MakeSafeRemoteMem(SafeHandle hRemoteProcess, void * pRemoteMemory)
{
	return SafeRemoteMem(pRemoteMemory,
		[hRemoteProcess](void * pRemoteMemoryToDelete)
		{
			if (pRemoteMemoryToDelete != NULL)
			{
				VirtualFreeEx(hRemoteProcess.get(), pRemoteMemoryToDelete, 0, MEM_RELEASE);
			}
		});
}

EXTERN_C IMAGE_DOS_HEADER __ImageBase;

int FindSteamProcessID();
BOOL FindProcessByName(const char * szProcessName, int * piFirstProcessID, int * piNumProcesses);
BOOL ProcessHasModuleLoaded(const int iProcessID, const char * szModuleName, bool bPartialMatchFromEnd);
BOOL TryParseInt(const char * szStringIn, int * pIntOut);
BOOL SelfInjectIntoSteam(const HWND hWindow, const int iSteamProcessID, const char * szNetHookDllPath);
BOOL InjectEjection(const HWND hWindow, const int iSteamProcessID, const char * szModuleName);

//
// RunDLL Interface
//
// rundll32.exe C:\Path\To\NetHook2.dll,Inject
// rundll32.exe C:\Path\To\NetHook2.dll,Inject <process ID>
// rundll32.exe C:\Path\To\NetHook2.dll,Inject <process name>
// rundll32.exe C:\Path\To\NetHook2.dll,Eject
// rundll32.exe C:\Path\To\NetHook2.dll,Eject <process ID>
// rundll32.exe C:\Path\To\NetHook2.dll,Eject <process name>
//

#pragma comment(linker, "/EXPORT:Inject=?Inject@@YGXPAUHWND__@@PAUHINSTANCE__@@PADH@Z")
__declspec(dllexport) void CALLBACK Inject(HWND hWindow, HINSTANCE hInstance, LPSTR lpszCommandLine, int nCmdShow);

#pragma comment(linker, "/EXPORT:Eject=?Eject@@YGXPAUHWND__@@PAUHINSTANCE__@@PADH@Z")
__declspec(dllexport) void CALLBACK Eject(HWND hWindow, HINSTANCE hInstance, LPSTR lpszCommandLine, int nCmdShow);

typedef enum {
	k_ESteamProcessSearchErrorNone = 0,
	k_ESteamProcessSearchErrorCouldNotFindSteam,
	k_ESteamProcessSearchErrorCouldNotFindProcessWithSuppliedName,
	k_ESteamProcessSearchErrorFoundMultipleProcessesWithSuppliedName,
	k_ESteamProcessSearchErrorTargetProcessDoesNotHaveSteamClientDllLoaded,

	k_ESteamProcessSearchErrorMax,
} ESteamProcessSearchError;

const char * NameFromESteamProcessSearchError( ESteamProcessSearchError eValue )
{
	switch ( eValue )
	{
		case k_ESteamProcessSearchErrorCouldNotFindSteam:
			return "Unable to find Steam. Make sure Steam is running, then try again.";

		case k_ESteamProcessSearchErrorCouldNotFindProcessWithSuppliedName:
			return "Unable to find any processes with the supplied name.";

		case k_ESteamProcessSearchErrorFoundMultipleProcessesWithSuppliedName:
			return "Multiple processes found with the supplied name. Please supply a process ID instead.";

		case k_ESteamProcessSearchErrorTargetProcessDoesNotHaveSteamClientDllLoaded:
			return "Invalid process: Target process does not have steamclient.dll loaded.";

		default:
			return "Unknown error.";
	}
}

ESteamProcessSearchError GetSteamProcessID( HWND hWindow, LPSTR lpszCommandLine, int * piSteamProcessID )
{
	int iNumProcesses;
	if ( strlen( lpszCommandLine ) == 0 )
	{
		*piSteamProcessID = FindSteamProcessID();
		if ( *piSteamProcessID <= 0 )
		{
			return k_ESteamProcessSearchErrorCouldNotFindSteam;
		}
	}
	else if ( !TryParseInt( lpszCommandLine, piSteamProcessID ) )
	{
		if ( !FindProcessByName( lpszCommandLine, piSteamProcessID, &iNumProcesses ) )
		{
			return k_ESteamProcessSearchErrorCouldNotFindProcessWithSuppliedName;
		}
		else if ( iNumProcesses > 1 )
		{
			return k_ESteamProcessSearchErrorFoundMultipleProcessesWithSuppliedName;
		}
	}
	
	if ( !ProcessHasModuleLoaded( *piSteamProcessID, "steamclient.dll", /* bPartialMatchFromEnd */ true ) )
	{
		return k_ESteamProcessSearchErrorTargetProcessDoesNotHaveSteamClientDllLoaded;
	}

	return k_ESteamProcessSearchErrorNone;
}

void CALLBACK Inject( HWND hWindow, HINSTANCE hInstance, LPSTR lpszCommandLine, int nCmdShow )
{
	HANDLE hSeDebugToken = NULL;

	int iSteamProcessID = -1;
	ESteamProcessSearchError eError = GetSteamProcessID( hWindow, lpszCommandLine, &iSteamProcessID );
	if ( eError == k_ESteamProcessSearchErrorCouldNotFindSteam || eError == k_ESteamProcessSearchErrorCouldNotFindProcessWithSuppliedName || eError == k_ESteamProcessSearchErrorTargetProcessDoesNotHaveSteamClientDllLoaded )
	{
		hSeDebugToken = SeDebugAcquire();
		if ( hSeDebugToken == NULL )
		{
			MessageBoxA( hWindow, "Unable to acquire SeDebug privilege. Make sure you're running as Administrator (elevated).", "NetHook2", MB_OK | MB_ICONASTERISK );
			return;
		}
		eError = GetSteamProcessID( hWindow, lpszCommandLine, &iSteamProcessID );
	}

	if ( eError != k_ESteamProcessSearchErrorNone )
	{
		MessageBoxA( hWindow, NameFromESteamProcessSearchError( eError ), "NetHook2", MB_OK | MB_ICONASTERISK );
		CloseHandle( hSeDebugToken );
		return;
	}

	char szNethookDllPath[MAX_PATH];
	ZeroMemory( szNethookDllPath, sizeof( szNethookDllPath ) );
	int result = GetModuleFileNameA( (HINSTANCE)&__ImageBase, szNethookDllPath, sizeof( szNethookDllPath ) );

	if (ProcessHasModuleLoaded(iSteamProcessID, szNethookDllPath, /* bPartialMatchFromEnd */ false))
	{
		MessageBoxA(hWindow, "Error: NetHook2 is already injected into this process.", "NetHook2", MB_OK | MB_ICONASTERISK);
		CloseHandle(hSeDebugToken);
		return;
	}

	BOOL bInjected = SelfInjectIntoSteam( hWindow, iSteamProcessID, szNethookDllPath );
	if ( !bInjected )
	{
		// Do nothing, SelfInjectIntoSteam already shows a messagebox with details.
	}

	CloseHandle( hSeDebugToken );
}

void CALLBACK Eject( HWND hWindow, HINSTANCE hInstance, LPSTR lpszCommandLine, int nCmdShow )
{
	HANDLE hSeDebugToken = NULL;

	int iSteamProcessID = -1;
	ESteamProcessSearchError eError = GetSteamProcessID( hWindow, lpszCommandLine, &iSteamProcessID );

	// Steam sets permissions such that PROCESS_VM_READ is denied unless we either modify Steam's permissions somehow, or use
	// SeDebugPrivilege. As such, GetSteamProcessID will fail at OpenProcess and signal that it could not find the process.
	if ( eError == k_ESteamProcessSearchErrorCouldNotFindSteam || eError == k_ESteamProcessSearchErrorCouldNotFindProcessWithSuppliedName || eError == k_ESteamProcessSearchErrorTargetProcessDoesNotHaveSteamClientDllLoaded )
	{
		hSeDebugToken = SeDebugAcquire();
		if ( hSeDebugToken == NULL )
		{
			MessageBoxA( hWindow, "Unable to acquire SeDebug privilege. Make sure you're running as Administrator (elevated).", "NetHook2", MB_OK | MB_ICONASTERISK );
			return;
		}
		eError = GetSteamProcessID( hWindow, lpszCommandLine, &iSteamProcessID );
	}

	if ( eError != k_ESteamProcessSearchErrorNone )
	{
		MessageBoxA( hWindow, NameFromESteamProcessSearchError( eError ), "NetHook2", MB_OK | MB_ICONASTERISK );
		CloseHandle( hSeDebugToken );
		return;
	}

	char szNethookDllPath[MAX_PATH];
	ZeroMemory( szNethookDllPath, sizeof( szNethookDllPath ) );
	int result = GetModuleFileNameA( (HINSTANCE)&__ImageBase, szNethookDllPath, sizeof( szNethookDllPath ) );

	if ( !ProcessHasModuleLoaded( iSteamProcessID, szNethookDllPath, /* bPartialMatchFromEnd */ false ) )
	{
		MessageBoxA( hWindow, "Unable to eject NetHook2: This instance of Steam does not have NetHook2 loaded.", "NetHook2", MB_OK | MB_ICONASTERISK );
		CloseHandle( hSeDebugToken );
		return;
	}
	
	BOOL bInjected = InjectEjection( hWindow, iSteamProcessID, szNethookDllPath );
	if ( !bInjected )
	{
		// Do nothing, InjectEjection already shows a messagebox with details.
	}

	CloseHandle( hSeDebugToken );
}

//
// Process Helpers
//
int FindSteamProcessID()
{
	int iSteamProcessID = 0;
	int iNumProcesses = 0;

	if (FindProcessByName("steam.exe", &iSteamProcessID, &iNumProcesses) && iNumProcesses == 1)
	{
		return iSteamProcessID;
	}

	return -1;
}

BOOL FindProcessByName(const char * szProcessName, int * piFirstProcessID, int * piNumProcesses)
{
	int iNumProcessesFound = 0;
	*piFirstProcessID = 0;

	DWORD cbNeeded = 0;
	const int MAX_NUM_PROCESSES = 2048; // Be generous
	DWORD piProcesses[MAX_NUM_PROCESSES];

	if (!EnumProcesses(piProcesses, sizeof(piProcesses), &cbNeeded))
	{
		return -1;
	}

	int iNumEnumeratedProcesses = cbNeeded / sizeof(DWORD);
	for (int i = 0; i < iNumEnumeratedProcesses; i++)
	{
		DWORD pid = piProcesses[i];

		SafeHandle hProcess = MakeSafeHandle(OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, pid));
		if (hProcess != NULL)
		{
			char szProcessPath[MAX_PATH];
			if(GetModuleFileNameEx(hProcess.get(), 0, szProcessPath, sizeof(szProcessPath)) == 0)
			{
				continue;
			}

			char szEndsWithKey[MAX_PATH];
			ZeroMemory(szEndsWithKey, sizeof(szEndsWithKey));

			szEndsWithKey[0] = '\\';
			strncpy_s(szEndsWithKey + 1, sizeof(szEndsWithKey) - 1, szProcessName, strlen(szProcessName));

			if (stringCaseInsensitiveEndsWith(szProcessPath, szEndsWithKey))
			{
				if (*piFirstProcessID <= 0)
				{
					*piFirstProcessID = pid;
				}

				iNumProcessesFound++;
			}
		}
	}

	*piNumProcesses = iNumProcessesFound;
	return iNumProcessesFound > 0;
}

BOOL ProcessHasModuleLoaded(const int iProcessID, const char * szModuleName, bool bPartialMatchFromEnd)
{
	SafeHandle hProcess = MakeSafeHandle(OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, iProcessID));
	if (hProcess != NULL)
	{
		HMODULE hModules[1024];
		DWORD cbNeeded;
		if (EnumProcessModules(hProcess.get(), hModules, sizeof(hModules), &cbNeeded))
		{
			int iNumModules = cbNeeded / sizeof(HMODULE);

			for (int i = 0; i < iNumModules; i++)
			{
				char szModulePath[MAX_PATH];
				ZeroMemory(szModulePath, sizeof(szModulePath));

				if (GetModuleFileNameExA(hProcess.get(), hModules[i], szModulePath, sizeof(szModulePath)))
				{
					bool bMatches;

					if (bPartialMatchFromEnd)
					{
						bMatches = stringCaseInsensitiveEndsWith(szModulePath, szModuleName);
					}
					else
					{
						bMatches = (_stricmp(szModulePath, szModuleName) == 0);
					}

					if (bMatches)
					{
						return true;
					}
				}
			}
		}
	}
	return false;
}


BOOL TryParseInt(const char * szStringIn, int * pIntOut)
{
	int iStrLen = strlen(szStringIn);

	for (int i = 0; i < iStrLen; i++)
	{
		if (!isdigit(szStringIn[i]))
		{
			return false;
		}
	}

	*pIntOut = atoi(szStringIn);
	return true;
}

//
// Code Cave
//
typedef HMODULE (WINAPI *GetModuleHandleAPtr)(LPCSTR);
typedef BOOL (WINAPI *FreeLibraryPtr)(HMODULE);

struct EjectParams
{
	GetModuleHandleAPtr GetModuleHandleA;
	FreeLibraryPtr FreeLibrary;
	char szModuleName[MAX_PATH];
};

//
// This function can not have any (absolute?) 'jmp' statements.
// Any referenced functions must come from function pointers in EjectParams.
#ifdef __MSVC_RUNTIME_CHECKS
#error /RTC is not allowed.
#endif
static DWORD WINAPI _Eject(LPVOID lpThreadParameter)
{
	struct EjectParams * pParams = (struct EjectParams *)lpThreadParameter;
	HMODULE hModule = pParams->GetModuleHandleA(pParams->szModuleName);
	pParams->FreeLibrary(hModule);

	return 0;
}

//
// Code Injection
//

// Inject NetHook2 via LoadLibrary
BOOL SelfInjectIntoSteam(const HWND hWindow, const int iSteamProcessID, const char * szNetHookDllPath)
{
	SafeHandle hSteamProcess = MakeSafeHandle(OpenProcess(PROCESS_ALL_ACCESS, FALSE, iSteamProcessID));
	if (hSteamProcess == NULL)
	{
		MessageBoxA(hWindow, "Unable to open Steam process.", "NetHook2", MB_OK | MB_ICONASTERISK);
		return false;
	}

	HMODULE hKernel32Module = GetModuleHandleA("kernel32.dll");
	if (hKernel32Module == NULL)
	{
		MessageBoxA(hWindow, "Unable to open load kernel32.dll.", "NetHook2", MB_OK | MB_ICONASTERISK);
		return false;
	}

	LPVOID pLoadLibraryA = (LPVOID)GetProcAddress(hKernel32Module, "LoadLibraryA");
	if (pLoadLibraryA == NULL)
	{
		MessageBoxA(hWindow, "Unable to find LoadLibraryA.", "NetHook2", MB_OK | MB_ICONASTERISK);
		return false;
	}

	SafeRemoteMem pArgBuffer = MakeSafeRemoteMem(hSteamProcess, VirtualAllocEx(hSteamProcess.get(), NULL, strlen(szNetHookDllPath), MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE));
	if (pArgBuffer == NULL)
	{
		MessageBoxA(hWindow, "Unable to allocate memory inside Steam.", "NetHook2", MB_OK | MB_ICONASTERISK);
		return false;
	}

	BOOL bWritten = WriteProcessMemory(hSteamProcess.get(), pArgBuffer.get(), szNetHookDllPath, strlen(szNetHookDllPath), NULL);
	if (!bWritten)
	{
		MessageBoxA(hWindow, "Unable to write to allocated memory inside Steam.", "NetHook2", MB_OK | MB_ICONASTERISK);
		return false;
	}

	HANDLE hRemoteThread = CreateRemoteThread(hSteamProcess.get(), NULL, 0, (LPTHREAD_START_ROUTINE)pLoadLibraryA, pArgBuffer.get(), NULL, NULL);
	if (hRemoteThread == NULL)
	{
		MessageBoxA(hWindow, "Unable to create remote thread inside Steam.", "NetHook2", MB_OK | MB_ICONASTERISK);
		return false;
	}

	if (WaitForSingleObject(hRemoteThread, 10000 /* milliseconds */) == WAIT_TIMEOUT)
	{
		MessageBoxA(hWindow, "Injection timed out.", "NetHook2", MB_OK | MB_ICONASTERISK);
		return false;
	}

	return true;
}

// Inject the 'Eject' code cave instructions
// This seems to be the only way to not crash Steam when ejecting.
BOOL InjectEjection(const HWND hWindow, const int iSteamProcessID, const char * szModuleName)
{
	SafeHandle hSteamProcess = MakeSafeHandle(OpenProcess(PROCESS_ALL_ACCESS, FALSE, iSteamProcessID));
	if (hSteamProcess == NULL)
	{
		MessageBoxA(hWindow, "Unable to open Steam process.", "NetHook2", MB_OK | MB_ICONASTERISK);
		return false;
	}

	HMODULE hKernel32Module = GetModuleHandleA("kernel32.dll");
	if (hKernel32Module == NULL)
	{
		MessageBoxA(hWindow, "Unable to open load kernel32.dll.", "NetHook2", MB_OK | MB_ICONASTERISK);
		return false;
	}

	struct EjectParams params;
	
	SafeRemoteMem pEjectParams = MakeSafeRemoteMem(hSteamProcess, VirtualAllocEx(hSteamProcess.get(), NULL, sizeof(struct EjectParams), MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE));
	if (pEjectParams == NULL)
	{
		MessageBoxA(hWindow, "Unable to allocate memory inside Steam.", "NetHook2", MB_OK | MB_ICONASTERISK);
		return false;
	}

	params.FreeLibrary = (FreeLibraryPtr)GetProcAddress(hKernel32Module, "FreeLibrary");
	params.GetModuleHandleA = (GetModuleHandleAPtr)GetProcAddress(hKernel32Module, "GetModuleHandleA");
	strncpy_s(params.szModuleName, szModuleName, sizeof(params.szModuleName));

	BOOL bWritten = WriteProcessMemory(hSteamProcess.get(), pEjectParams.get(), &params, sizeof(params), NULL);
	if (!bWritten)
	{
		MessageBoxA(hWindow, "Unable to write to allocated memory inside Steam.", "NetHook2", MB_OK | MB_ICONASTERISK);
		return false;
	}

	int cubRemoteFunc = 0x1000; // Be generous, we can't precisely measure this.
	SafeRemoteMem pRemoteFunc = MakeSafeRemoteMem(hSteamProcess, VirtualAllocEx(hSteamProcess.get(), NULL, cubRemoteFunc, MEM_COMMIT, PAGE_EXECUTE_READWRITE));

	if (pRemoteFunc == NULL)
	{
		MessageBoxA(hWindow, "Unable to allocate executable memory inside Steam.", "NetHook2", MB_OK | MB_ICONASTERISK);
		return false;
	}

	bWritten = WriteProcessMemory(hSteamProcess.get(), pRemoteFunc.get(), &_Eject, cubRemoteFunc, NULL);
	if (!bWritten)
	{
		MessageBoxA(hWindow, "Unable to write to executable allocated memory inside Steam.", "NetHook2", MB_OK | MB_ICONASTERISK);
		return false;
	}

	HANDLE hRemoteThread = CreateRemoteThread(hSteamProcess.get(), NULL, 0, (LPTHREAD_START_ROUTINE)pRemoteFunc.get(), pEjectParams.get(), NULL, NULL);
	if (hRemoteThread == NULL)
	{
		MessageBoxA(hWindow, "Unable to create remote thread inside Steam.", "NetHook2", MB_OK | MB_ICONASTERISK);
		return false;
	}

	if (WaitForSingleObject(hRemoteThread, 5000 /* milliseconds */) == WAIT_TIMEOUT)
	{
		MessageBoxA(hWindow, "Injection timed out.", "NetHook2", MB_OK | MB_ICONASTERISK);
		return false;
	}

	return true;
}
