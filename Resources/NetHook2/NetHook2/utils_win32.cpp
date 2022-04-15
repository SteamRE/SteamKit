#include "utils.h"
#include <Windows.h>

namespace NetHook
{
namespace Utils
{

std::string GetNetHookLogDirOverride()
{
    const char* logDir = NULL;
    logDir = getenv("NETHOOK_LOG_DIR");
    if(logDir)
    {
        return std::string(logDir);
    }

    return "";
}

std::string GetCurrentWorkDir()
{
    char buf[MAX_PATH];
    if(GetCurrentDirectoryA(MAX_PATH, buf))
    {
        return buf;
    }
    return "";
}

bool MkDir(const char *path)
{
    return CreateDirectoryA(path, nullptr);
}

bool RenameFile(const char *src, const char *dest)
{
    return MoveFileA(src, dest);
}

bool GetClientModule(const char* name, ModuleInfo* info)
{
    info->m_moduleName = name;
    HMODULE mod = GetModuleHandle(name);
    if (mod != NULL)
    {
        info->m_base = (const char*)mod;

        const IMAGE_DOS_HEADER* dosHdr = (IMAGE_DOS_HEADER*)info->m_base;
        const IMAGE_NT_HEADERS* peHdr = (IMAGE_NT_HEADERS*)((unsigned long)dosHdr + (unsigned long)dosHdr->e_lfanew);

        if (peHdr->Signature != IMAGE_NT_SIGNATURE) {
            info->m_base = nullptr;
            return false;
        }

        info->m_size = (size_t)peHdr->OptionalHeader.SizeOfImage;

        char nameBuf[MAX_PATH];
        if (GetModuleFileName(mod, nameBuf, MAX_PATH) == ERROR_SUCCESS)
        {
            info->m_modulePath = nameBuf;
        }

        return true;
    }
    return false;
}

}
}
