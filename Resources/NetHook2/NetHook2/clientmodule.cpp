#include <iostream>
#include <fstream>
#include <cstring>
#include <fstream>
#include "clientmodule.h"
#include "signscan.h"

namespace NetHook
{

ClientModule::ClientModule(const ModuleInfo& modInfo) noexcept:
    m_moduleInfo(modInfo)
{
}

ClientModule::~ClientModule()
{
}

std::string ClientModule::GetDirectory()
{
#ifdef __linux__
    return m_moduleInfo.m_modulePath.substr(0, m_moduleInfo.m_modulePath.find_last_of("/"));
#elif _WIN32
    return m_moduleInfo.m_modulePath.substr(0, m_moduleInfo.m_modulePath.find_last_of("\\"));
#endif
}

std::string ClientModule::GetFullPath()
{
    return m_moduleInfo.m_modulePath;
}

std::string ClientModule::GetName()
{
    return m_moduleInfo.m_moduleName;
}

bool ClientModule::FindSignature(const char *sig, const char *mask, void **func, const char *prev) noexcept
{
    const char* base = m_moduleInfo.m_base;
    const char* end = base + m_moduleInfo.m_size;

    return NetHook::FindSignature(base, end, sig, mask, func, prev);
}

}
