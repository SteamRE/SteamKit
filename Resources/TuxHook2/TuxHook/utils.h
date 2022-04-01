#ifndef UTILS_H
#define UTILS_H

#include <string>
#include "clientmodule.h"

namespace NetHook
{
namespace Utils
{

std::string GetNetHookLogDirOverride();
std::string GetCurrentWorkDir();
bool MkDir(const char* path);
bool RenameFile(const char* src, const char* dest);
bool GetClientModule(const char* name, ModuleInfo* info);

}
}

#endif // UTILS_H
