#ifdef _WIN32
    #define WIN32_LEAN_AND_MEAN
    #include <Windows.h>
    #include "nh2_string.h"
#endif 
#include <iostream>
#include <string>

#include "clientmodule.h"
#include "crypto.h"
#include "net.h"
#include "steammessages_base.pb.h"
#include "version.h"
#include "utils.h"
#include "logger.h"

NetHook::CLogger *g_pLogger = nullptr;
NetHook::CCrypto *g_pCrypto = nullptr;
NetHook::CNet *g_pNet = nullptr;
NetHook::ClientModule *g_pClientModule = nullptr;

#ifdef __linux__
    extern "C" void th2_Init();
#endif

#ifdef _WIN32
    bool g_bOwnsConsole = false;

    BOOL IsRunDll32()
    {
        char szMainModulePath[MAX_PATH];
        DWORD dwMainModulePathLength = GetModuleFileNameA(NULL, szMainModulePath, sizeof(szMainModulePath));

        return stringCaseInsensitiveEndsWith(szMainModulePath, "\\rundll32.exe");
    }   
#endif

#ifdef __linux__
    __attribute__((destructor))
#endif
static void detach()
{
    if(g_pCrypto)
        delete g_pCrypto;

    if(g_pNet)
        delete g_pNet;

    if(g_pClientModule)
        delete g_pClientModule;

    if(g_pLogger)
        delete g_pLogger;

#ifdef _WIN32
    if (g_bOwnsConsole)
        FreeConsole();
#endif
}

void th2_Init()
{
    GOOGLE_PROTOBUF_VERIFY_VERSION;

#ifdef _WIN32
    g_bOwnsConsole = AllocConsole();
#endif
    g_pLogger = new NetHook::CLogger();

    NetHook::ModuleInfo steamClient;
#ifdef __linux__
    if(!NetHook::Utils::GetClientModule("steamclient.so", &steamClient))
#elif _WIN32
    if(!NetHook::Utils::GetClientModule("steamclient.dll", &steamClient))
#endif
    {
        g_pLogger->LogConsole("steamclient not found!\n");
        return;
    }
    g_pLogger->LogConsole("Found steamclient: %s @ 0x%08x\n", steamClient.m_modulePath.c_str(), (size_t)steamClient.m_base);

    g_pClientModule = new NetHook::ClientModule(steamClient);

    g_pLogger->InitSessionLogDir();

    g_pCrypto = new NetHook::CCrypto();
    g_pNet = new NetHook::CNet();
}


#ifdef _WIN32
BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
    if (fdwReason == DLL_PROCESS_ATTACH)
    {
        if (IsRunDll32())
        {
            return TRUE;
        }

        th2_Init();
    }
    else if (fdwReason == DLL_PROCESS_DETACH)
    {
        detach();
    }

    return TRUE;
}
#endif


