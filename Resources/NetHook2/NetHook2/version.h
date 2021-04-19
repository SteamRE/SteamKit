#ifndef VERSION_H
#define VERSION_H

#define WIN32_LEAN_AND_MEAN
#include <windows.h>

extern const char *g_szBuildDate;
extern const char *g_szBuiltFromCommitSha;
extern const char *g_szBuiltFromCommitDate;
extern const BOOL g_bBuiltFromDirty;

#endif
