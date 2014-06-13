#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <Psapi.h>

//
// rundll32.exe C:\Path\To\NetHook2.dll,Inject
//

EXTERN_C IMAGE_DOS_HEADER __ImageBase;

#pragma comment(linker, "/EXPORT:Inject=?Inject@@YGXPAUHWND__@@PAUHINSTANCE__@@PADH@Z")
__declspec(dllexport) void CALLBACK Inject(HWND hWindow, HINSTANCE hInstance, LPSTR lpszCommandLine, int nCmdShow);

int FindSteamProcessID();
BOOL SelfInjectIntoSteam(const HWND hWindow, const int iSteamProcessID, const char * szNetHookDllPath);


void CALLBACK Inject(HWND hWindow, HINSTANCE hInstance, LPSTR lpszCommandLine, int nCmdShow)
{
	int iSteamProcessID = FindSteamProcessID();
	if (iSteamProcessID <= 0)
	{
		MessageBoxA(hWindow, "Unable to find Steam. Make sure Steam is running, then try again.", "NetHook2", MB_OK | MB_ICONASTERISK);
		return;
	}

	char szNethookDllPath[MAX_PATH];
	ZeroMemory(szNethookDllPath, sizeof(szNethookDllPath));
	int result = GetModuleFileNameA((HINSTANCE)&__ImageBase, szNethookDllPath, sizeof(szNethookDllPath));

	BOOL bInjected = SelfInjectIntoSteam(hWindow, iSteamProcessID, szNethookDllPath);
	if (!bInjected)
	{
		// Do nothing, SelfInjectIntoSteam already shows a messagebox with details.
	}
}

int FindSteamProcessID()
{
	DWORD cbNeeded = 0;
	const int MAX_NUM_PROCESSES = 2048; // Be generous
	DWORD piProcesses[MAX_NUM_PROCESSES];

	if (!EnumProcesses(piProcesses, sizeof(piProcesses), &cbNeeded))
	{
		return -1;
	}

	int iNumProcesses = cbNeeded / sizeof(DWORD);
	for (int i = 0; i < iNumProcesses; i++)
	{
		DWORD pid = piProcesses[i];

		HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, pid);
		if (hProcess != NULL)
		{
			char szProcessPath[MAX_PATH];
			DWORD dwPathLength = GetModuleFileNameEx(hProcess, 0, szProcessPath, sizeof(szProcessPath));
			const char * szEndsWithKey = "\\steam.exe";
			unsigned int cubEndsWithKey = strlen(szEndsWithKey);

			if (dwPathLength != 0 && dwPathLength > cubEndsWithKey)
			{
				if (_stricmp(szProcessPath + dwPathLength - cubEndsWithKey, szEndsWithKey) == 0)
				{
					CloseHandle(hProcess);
					return pid;
				}
			}

			CloseHandle(hProcess);
		}
	}

	return -1;
}

BOOL SelfInjectIntoSteam(const HWND hWindow, const int iSteamProcessID, const char * szNetHookDllPath)
{
	HANDLE hSteamProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, iSteamProcessID);
	if (hSteamProcess == NULL)
	{
		MessageBoxA(hWindow, "Unable to open Steam process.", "NetHook2", MB_OK | MB_ICONASTERISK);
		return false;
	}

	HMODULE hKernel32Module = GetModuleHandleA("kernel32.dll");
	if (hKernel32Module == NULL)
	{
		MessageBoxA(hWindow, "Unable to open load kernel32.dll.", "NetHook2", MB_OK | MB_ICONASTERISK);
		CloseHandle(hSteamProcess);
		return false;
	}

	LPVOID pLoadLibraryA = (LPVOID)GetProcAddress(hKernel32Module, "LoadLibraryA");
	if (pLoadLibraryA == NULL)
	{
		MessageBoxA(hWindow, "Unable to find LoadLibraryA.", "NetHook2", MB_OK | MB_ICONASTERISK);
		CloseHandle(hSteamProcess);
		return false;
	}

	LPVOID pArgBuffer = VirtualAllocEx(hSteamProcess, NULL, strlen(szNetHookDllPath), MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
	if (pArgBuffer == NULL)
	{
		MessageBoxA(hWindow, "Unable to allocate memory inside Steam.", "NetHook2", MB_OK | MB_ICONASTERISK);
		CloseHandle(hSteamProcess);
		return false;
	}

	BOOL bWritten = WriteProcessMemory(hSteamProcess, pArgBuffer, szNetHookDllPath, strlen(szNetHookDllPath), NULL);
	if (!bWritten)
	{
		MessageBoxA(hWindow, "Unable to write to allocated memory inside Steam.", "NetHook2", MB_OK | MB_ICONASTERISK);
		CloseHandle(hSteamProcess);
		return false;
	}

	HANDLE hRemoteThread = CreateRemoteThread(hSteamProcess, NULL, 0, (LPTHREAD_START_ROUTINE)pLoadLibraryA, pArgBuffer, NULL, NULL);
	if (hRemoteThread == NULL)
	{
		MessageBoxA(hWindow, "Unable to create remote thread inside Steam.", "NetHook2", MB_OK | MB_ICONASTERISK);
		CloseHandle(hSteamProcess);
		return false;
	}

	CloseHandle(hSteamProcess);
	return true;
}
