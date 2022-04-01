#ifndef CLIENTMODULE_H
#define CLIENTMODULE_H

#include <string>

namespace NetHook
{

struct ModuleInfo
{
    std::string m_moduleName;
    std::string m_modulePath;
    const char* m_base = nullptr;
    size_t m_size = 0;
};

class ClientModule
{
public:
    explicit ClientModule(const ModuleInfo& modInfo) noexcept;
    ~ClientModule();

    std::string GetFullPath();
    std::string GetDirectory();
    std::string GetName();

    bool FindSignature(const char *sig, const char *mask, void **func, const char *prev) noexcept;

private:
    ClientModule();
    ClientModule(const ClientModule&);
    ClientModule& operator=(const ClientModule&);

    ModuleInfo m_moduleInfo;
};

}

extern NetHook::ClientModule *g_pClientModule;

#endif // CLIENTMODULE_H
