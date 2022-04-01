#include <limits.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#include <pwd.h>
#include <link.h>
#include <dlfcn.h>
#include <elf.h>
#include "utils.h"

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
    char buf[PATH_MAX];
    if(getcwd(buf, PATH_MAX))
    {
        return buf;
    }

    return "";
}

bool MkDir(const char *path)
{
    if(!mkdir(path, 0755))
    {
        return true;
    }
    return false;
}

bool RenameFile(const char *src, const char *dest)
{
    if(!rename(src, dest))
    {
        return true;
    }
    return false;
}

namespace
{
static int dl_callback(struct dl_phdr_info *info, size_t size, void *data)
{
    ModuleInfo* modInfo = (ModuleInfo*)data;
    std::string strName(info->dlpi_name);
    if(strName.find(modInfo->m_moduleName) != std::string::npos)
    {
        modInfo->m_modulePath = info->dlpi_name;
        modInfo->m_base = (const char*)info->dlpi_addr;
        for(int i = 0; i < info->dlpi_phnum; ++i)
        {
            if(info->dlpi_phdr[i].p_type == PT_LOAD)
            {
                modInfo->m_size = info->dlpi_phdr[i].p_vaddr + info->dlpi_phdr[i].p_memsz;
            }
        }
    }

    return 0;
}
}

bool GetClientModule(const char* name, ModuleInfo* info)
{
    info->m_moduleName = name;
    dl_iterate_phdr(dl_callback, info);
    if(info->m_base != nullptr)
    {
        return true;
    }
    return false;
}

}
}
